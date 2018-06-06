﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Helpers;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services
{
    public interface IAlarms
    {
        Alarm Get(string id);

        List<Alarm> List(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);

        List<Alarm> ListByRule(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);

        int GetCountByRule(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string[] devices);

        Task<Alarm> UpdateAsync(string id, string status);

        Task StartDeleteByRuleAsync(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int? limit,
            string[] devices,
            Guid operationId);

        string GetDeleteByRuleStatus(string id);
    }

    public class Alarms : IAlarms
    {
        private const string INVALID_CHARACTER = @"[^A-Za-z0-9:;.,_\-]";

        private readonly ILogger log;
        private readonly IStorageClient storageClient;

        private readonly string databaseName;
        private readonly string collectionId;
        private readonly int maxRetryCount;
        private readonly int deleteBatchSize;
        private readonly int deleteIntervalMsec;

        // constants for storage keys
        private const string MESSAGE_RECEIVED_KEY = "device.msg.received";
        private const string RULE_ID_KEY = "rule.id";
        private const string DEVICE_ID_KEY = "device.id";
        private const string STATUS_KEY = "status";
        private const string ALARM_SCHEMA_KEY = "alarm";

        private const string ALARM_STATUS_OPEN = "open";
        private const string ALARM_STATUS_ACKNOWLEDGED = "acknowledged";
        private const string UNKNOWN_STATUS = "Unknown";
        private const string SUCCESS_STATUS = "Success";
        private const string FAILED_STATUS = "Failed";
        private const string NOTHING_TO_DELETE_STATUS = "Nothing to Delete";
        private const string STARTED_STATUS = "Started";
        private const string IN_PROGRESS_STATUS = "In Progress";

        private const int DOC_QUERY_LIMIT = 1000;
        private const int DELETE_STATUS_UPDATE_INTERVAL_MS = 30000;

        public Alarms(
            IServicesConfig config,
            IStorageClient storageClient,
            ILogger logger)
        {
            this.storageClient = storageClient;
            this.databaseName = config.AlarmsConfig.StorageConfig.DocumentDbDatabase;
            this.collectionId = config.AlarmsConfig.StorageConfig.DocumentDbCollection;
            this.log = logger;
            this.maxRetryCount = config.AlarmsConfig.MaxRetries;
            this.deleteBatchSize = config.AlarmsConfig.DeleteBatchSize;
            this.deleteIntervalMsec = config.AlarmsConfig.DeleteIntervalMsec;
        }

        public Alarm Get(string id)
        {
            this.VerifyId(id);

            Document doc = this.GetDocumentById(id);
            return new Alarm(doc);
        }

        public List<Alarm> List(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            string sql = QueryBuilder.GetDocumentsSql(
                ALARM_SCHEMA_KEY,
                null, null,
                from, MESSAGE_RECEIVED_KEY,
                to, MESSAGE_RECEIVED_KEY,
                order, MESSAGE_RECEIVED_KEY,
                skip,
                limit,
                devices, DEVICE_ID_KEY);

            this.log.Debug("Created Alarm Query", () => new { sql });

            FeedOptions queryOptions = new FeedOptions();
            queryOptions.EnableCrossPartitionQuery = true;
            queryOptions.EnableScanInQuery = true;

            List<Document> docs = this.storageClient.QueryDocuments(
                this.databaseName,
                this.collectionId,
                queryOptions,
                sql,
                skip,
                limit);

            List<Alarm> alarms = new List<Alarm>();

            foreach (Document doc in docs)
            {
                alarms.Add(new Alarm(doc));
            }

            return alarms;
        }

        public List<Alarm> ListByRule(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            this.VerifyId(id);
            List<Document> docs = this.GetListOfDocuments(
                id,
                from,
                to,
                order,
                skip,
                limit,
                devices);

            List<Alarm> alarms = new List<Alarm>();
            foreach (Document doc in docs)
            {
                alarms.Add(new Alarm(doc));
            }

            return alarms;
        }

        public async Task StartDeleteByRuleAsync(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int? limit,
            string[] devices,
            Guid operationId)
        {
            this.VerifyId(id);

            DeleteStatus status = new DeleteStatus
            {
                Id = operationId.ToString(),
                Status = STARTED_STATUS,
                Timestamp = DateTime.UtcNow,
                RecordsDeleted = 0
            };

            try
            {
                await this.storageClient.UpsertDocumentAsync(this.databaseName, this.collectionId, status);
            }
            catch (Exception e)
            {
                this.log.Error("Could not write initial delete status", () => new { e.Message });
            }

            await Task.Run(() => this.DeleteByRuleAsync(id, from, to, order, skip, limit, devices, operationId));

        }


        public int GetCountByRule(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string[] devices)
        {
            // build sql query to get open/acknowledged alarm count for rule
            string[] statusList = { ALARM_STATUS_OPEN, ALARM_STATUS_ACKNOWLEDGED };
            string sql = QueryBuilder.GetCountSql(
                ALARM_SCHEMA_KEY,
                id, RULE_ID_KEY,
                from, MESSAGE_RECEIVED_KEY,
                to, MESSAGE_RECEIVED_KEY,
                devices, DEVICE_ID_KEY,
                statusList, STATUS_KEY);

            FeedOptions queryOptions = new FeedOptions();
            queryOptions.EnableCrossPartitionQuery = true;
            queryOptions.EnableScanInQuery = true;

            // request count of alarms for a rule id with given parameters
            var result = this.storageClient.QueryCount(
                this.databaseName,
                this.collectionId,
                queryOptions,
                sql);

            return result;
        }

        public async Task<Alarm> UpdateAsync(string id, string status)
        {
            this.VerifyId(id);

            Document document = this.GetDocumentById(id);
            document.SetPropertyValue(STATUS_KEY, status);

            document = await this.storageClient.UpsertDocumentAsync(
                this.databaseName,
                this.collectionId,
                document);

            return new Alarm(document);
        }

        /**
         * Returns status of delete by rule operation with the given id.
         * If the operation id is not found or if the status is "In Progress"
         * or "Started" and has not updated for DELETE_STATUS_UPDATE_INTERVAL_MS * 2,
         * return an unkown status
         */
        public string GetDeleteByRuleStatus(string id)
        {
            Document document = this.GetDocumentById(id);
            if (document == null)
            {
                return this.CreateUnknownStatus(id).ToString();
            }

            DeleteStatus status = new DeleteStatus(document);

            // If delete is in progress and have not seen update for 2 * update interval,
            // report unknown status (possible service was restarted)
            if ((status.Status.Equals(IN_PROGRESS_STATUS) || status.Status.Equals(STARTED_STATUS))
                && status.Timestamp.HasValue
                && DateTime.UtcNow.Subtract(status.Timestamp.Value).TotalMilliseconds > DELETE_STATUS_UPDATE_INTERVAL_MS * 2)
            {
                return this.CreateUnknownStatus(id).ToString();
            }

            return status.ToString();
        }

        /**
         * Query for list of alarms based on given rule id and other
         * query parameters and delete all alarms. This operation may take
         * a long time depending on how many alarms there are. Operation status
         * will be written to storage every DELETE_STATUS_UPDATE_INTERVAL_MS milliseconds.
         */
        private async Task DeleteByRuleAsync(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int? limit,
            string[] devices,
            Guid operationId)
        {

            Exception exception = null;
            int deletedCount = 0;
            try
            {
                List<Document> docs = this.GetListOfDocuments(
                    id,
                    from,
                    to,
                    order,
                    skip,
                    limit,
                    devices);

                if (docs.Count == 0)
                {
                    throw new ResourceNotFoundException("No alarms found");
                }

                await this.WriteIntermediateDeleteStatusAsync(operationId.ToString(), 0, docs.Count);

                this.DeleteAlarms(docs, out deletedCount, operationId.ToString());
            }
            catch (Exception e)
            {
                this.log.Error("Exception deleting alarms", () => new { e.Message });
                exception = e;
            }
            await this.WriteDeleteStatusAsync(operationId.ToString(), deletedCount, exception);
        }


        private Document GetDocumentById(string id)
        {
            this.VerifyId(id);

            // Retrieve the document using the DocumentClient.
            List<Document> documentList = this.storageClient.QueryDocuments(
                this.databaseName,
                this.collectionId,
                null,
                "SELECT * FROM c WHERE c.id='" + id + "'",
                0,
                DOC_QUERY_LIMIT);

            if (documentList.Count > 0)
            {
                return documentList[0];
            }

            return null;
        }

        /**
         * Delete given list of documents from storage.
         * Documents will be deleted in batches sized based on ServicesConfig.
         * If there is a delete failure, batch will retry up to max retry count
         * (defined in Services Config).
         * Intermediate delete status will be written to storage every
         * DELETE_STATUS_UPDATE_INTERVAL_MS milliseconds.
         */
        private void DeleteAlarms(List<Document> alarms, out int deletedCount, string operationId)
        {
            Stopwatch batchStopwatch = new Stopwatch();
            Stopwatch progressStopwatch = new Stopwatch();
            progressStopwatch.Start();
            deletedCount = 0;
            for (int i = 0; i < alarms.Count / this.deleteBatchSize + 1; i++)
            {
                int retryCount = 0;
                bool success = false;
                int start = i * this.deleteBatchSize;
                int end = start + this.deleteBatchSize > alarms.Count
                    ? alarms.Count
                    : start + this.deleteBatchSize;
                int toDelete = end - start;
                Task[] taskList = new Task[toDelete];

                while (!success && retryCount < this.maxRetryCount)
                {
                    try
                    {
                        batchStopwatch.Restart();
                        for (int j = start; j < end; j++)
                        {
                            taskList[j % this.deleteBatchSize] = 
                                this.storageClient.DeleteDocumentAsync(
                                    this.databaseName, 
                                    this.collectionId, 
                                    alarms[j].Id);
                        }

                        Task.WaitAll(taskList);
                        success = true;
                        deletedCount += toDelete;
                        if (progressStopwatch.ElapsedMilliseconds > DELETE_STATUS_UPDATE_INTERVAL_MS)
                        {
                            this.WriteIntermediateDeleteStatusAsync(operationId, deletedCount, alarms.Count - deletedCount).Wait();
                            progressStopwatch.Restart();
                        }
                    }

                    catch (Exception e)
                    {
                        retryCount++;
                        bool allNotFound = this.AreAllExceptionsNotFound(e, out TimeSpan retryTimeSpan);
                        
                        // If all exceptions are not found exceptions, treat as success. 
                        // Already were deleted.
                        if (allNotFound)
                        {
                            success = true;
                            deletedCount += toDelete;
                        }
                        else if (retryCount == this.maxRetryCount)
                        {
                            this.log.Error("Failed to delete batch of alarms", () => new { e.InnerException });
                            throw e;
                        }
                        else
                        {
                            this.log.Warn("Exception on delete", () => new { e.InnerException });
                            Thread.Sleep(retryTimeSpan);
                        }
                    }
                }

                batchStopwatch.Stop();
                if (batchStopwatch.ElapsedMilliseconds < this.deleteIntervalMsec)
                {
                    Thread.Sleep((int)(this.deleteIntervalMsec - batchStopwatch.ElapsedMilliseconds));
                }
            }
        }

        /**
         * Build document query and query storage client for list of documents
         */
        private List<Document> GetListOfDocuments(string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int? limit,
            string[] devices)
        {

            string sql = QueryBuilder.GetDocumentsSql(
                ALARM_SCHEMA_KEY,
                id, RULE_ID_KEY,
                from, MESSAGE_RECEIVED_KEY,
                to, MESSAGE_RECEIVED_KEY,
                order, MESSAGE_RECEIVED_KEY,
                skip,
                limit,
                devices, DEVICE_ID_KEY);

            this.log.Debug("Created Alarm By Rule Query", () => new { sql });

            FeedOptions queryOptions = new FeedOptions();
            queryOptions.EnableCrossPartitionQuery = true;
            queryOptions.EnableScanInQuery = true;

            return this.storageClient.QueryDocuments(
                this.databaseName,
                this.collectionId,
                queryOptions,
                sql,
                skip,
                limit);
        }

        /**
         * Verify id does not contain any invalid characters. Throw exception if it does
         */
        private void VerifyId(string id)
        {
            if (Regex.IsMatch(id, INVALID_CHARACTER))
            {
                throw new InvalidInputException("id contains illegal characters.");
            }
        }

        /**
         * Return DeleteStatus with given id and status "Unknown"
         */
        private DeleteStatus CreateUnknownStatus(string id)
        {
            return new DeleteStatus
            {
                Id = id,
                Status = UNKNOWN_STATUS
            };
        }

        /**
         * Create delete status with given id and deletedCount and current timestamp.
         * If there was nothing to delete status is "Nothing to Delete".
         * If request failed status is "Failed"
         * Otherwise status is "Success".
         * Write this delete status to storage.
         */
        private async Task WriteDeleteStatusAsync(string id, int deletedCount, Exception e)
        {
            try
            {
                DeleteStatus status = new DeleteStatus
                {
                    Id = id,
                    RecordsDeleted = deletedCount,
                    Timestamp = DateTime.UtcNow
                };
                if (e != null)
                {
                    status.Status = e.GetType() == typeof(ResourceNotFoundException)
                        ? NOTHING_TO_DELETE_STATUS
                        : FAILED_STATUS;
                }
                else
                {
                    status.Status = SUCCESS_STATUS;
                }

                await this.storageClient.UpsertDocumentAsync(this.databaseName, this.collectionId, status);
            }
            catch (Exception exception)
            {
                this.log.Error("Error writing delete status to Cosmos DB", () => new { exception.Message });
            }
        }

        /**
         * Write in progress status to storage with given id, deleted count and number of records
         * left to delete.
         */
        private async Task WriteIntermediateDeleteStatusAsync(string id, int deletedCount, int toDelete)
        {
            try
            {
                DeleteStatus status = new DeleteStatus
                {
                    Id = id,
                    Status = IN_PROGRESS_STATUS,
                    Timestamp = DateTime.UtcNow,
                    RecordsDeleted = deletedCount,
                    RecordsLeftToDelete = toDelete
                };

                await this.storageClient.UpsertDocumentAsync(this.databaseName, this.collectionId, status);
            }
            catch (Exception exception)
            {
                this.log.Error("Error writing intermediate delete status to Cosmos DB", () => new { exception.Message });
            }
        }

        /**
         * If given exception is an aggregate exception, check if all inner exceptions have
         * status code Not Found. If they do, return true, otherwise return false.
         * If an inner exception is a DocumentClientException that is not a Not Found,
         * store its recommended retry time in retryTimeSpan. Otherwise set retry time to 0.
         */
        private bool AreAllExceptionsNotFound(Exception exception, out TimeSpan retryTimeSpan)
        {
            retryTimeSpan = TimeSpan.Zero;
            if (exception.GetType() == typeof(AggregateException))
            {
                AggregateException aggregateException = (AggregateException) exception;
                foreach (Exception innerException in aggregateException.InnerExceptions)
                {
                    if (innerException.GetType() == typeof(DocumentClientException))
                    {
                        DocumentClientException documentClientException = (DocumentClientException) innerException;

                        // Ignore not found exception. Possible on retry already deleted some of the batch
                        if (documentClientException.StatusCode != HttpStatusCode.NotFound)
                        {
                            return false;
                        }

                        retryTimeSpan = documentClientException.RetryAfter;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
