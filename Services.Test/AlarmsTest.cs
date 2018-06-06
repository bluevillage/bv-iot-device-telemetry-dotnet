// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class AlarmsTest
    {
        private readonly Mock<IStorageClient> storageClient;
        private readonly Mock<ILogger> logger;
        private readonly IAlarms alarms;

        public AlarmsTest()
        {
            var servicesConfig = new ServicesConfig
            {
                AlarmsConfig = new AlarmsConfig("database", "collection", 3, 50, 1000)
            };
            this.storageClient = new Mock<IStorageClient>();
            this.logger = new Mock<ILogger>();
            this.alarms = new Alarms(servicesConfig, this.storageClient.Object, this.logger.Object);
        }

        /**
         * Verify basic behavior of delete alarms by rule.
         * Verify document will be deleted when delete by rule is called, with no errors.
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void BasicDeleteByRule()
        {
            // Arrange
            Document d1 = new Document
            {
                Id = "test"
            };

            this.SetUpStorageClientQueryResults(new List<Document> { d1 });

            this.storageClient
                .Setup(x => x.DeleteDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(d1));
            Guid guid = Guid.NewGuid();

            // Act
            this.alarms.StartDeleteByRuleAsync("test", null, null, null, 0, 1000, new string[0], guid).Wait();

            // Assert
            // Document was deleted once
            this.storageClient.Verify(x => x.DeleteDocumentAsync("database", "collection", "test"), Times.Once);
            // Status was updated at least 3 times--on start, after query, after delete finished
            this.storageClient.Verify(x => x.UpsertDocumentAsync("database", "collection", It.IsAny<DeleteStatus>()), Times.AtLeast(3));

            this.VerifyNoErrorsOrWarnings();
        }

        /**
         * Verify transient failure behavior of delete alarms by rule.
         * Verify if delete fails once it will retry.
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void DeleteByRuleTransientFailure()
        {
            // Arrange
            Document d1 = new Document
            {
                Id = "test"
            };

            this.SetUpStorageClientQueryResults(new List<Document> { d1 });

            this.storageClient
                .SetupSequence(x => x.DeleteDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Throws(new Exception())
                .Returns(Task.FromResult(d1));
            Guid guid = Guid.NewGuid();

            // Act
            this.alarms.StartDeleteByRuleAsync("test", null, null, null, 0, 1000, new string[0], guid).Wait();

            // Assert
            this.storageClient.Verify(x => x.DeleteDocumentAsync("database", "collection", "test"), Times.Exactly(2));
            this.storageClient.Verify(x => x.UpsertDocumentAsync("database", "collection", It.IsAny<DeleteStatus>()), Times.AtLeast(3));
            
            this.VerifyNoErrorsOrWarnings();
        }

        /**
         * Verify failure behavior of delete alarms by rule.
         * Verify if delete fails 3 times an error will be logged and method will exit.
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void DeleteByRuleFailsAfter3Exceptions()
        {
            // Arrange
            Document d1 = new Document
            {
                Id = "test"
            };

            this.SetUpStorageClientQueryResults(new List<Document> { d1 });

            this.storageClient
                .SetupSequence(x => x.DeleteDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Throws(new Exception())
                .Throws(new Exception())
                .Throws(new Exception());

            Guid guid = Guid.NewGuid();

            // Act
            this.alarms.StartDeleteByRuleAsync("test", null, null, null, 0, 1000, new string[0], guid).Wait();

            // Assert
            this.storageClient.Verify(x => x.DeleteDocumentAsync("database", "collection", "test"), Times.Exactly(3));
            this.storageClient.Verify(x => x.UpsertDocumentAsync("database", "collection", It.IsAny<DeleteStatus>()), Times.AtLeast(3));

            this.logger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Exactly(2));
            this.logger.Verify(l => l.Warn(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Exactly(2));
        }

        /**
         * Verify basic behavior of get delete status.
         * If delete status is success, will return success along with
         * other metadata (timestamp, number of rules deleted).
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetDeleteStatusIfCompleted()
        {
            // Arrange
            DateTime timestamp = new DateTime(2018, 6, 4, 16, 40, 0, DateTimeKind.Utc);

            Document document = this.CreateFakeDocument("id", timestamp, "Success", 20);
            this.SetUpStorageClientQueryResults(new List<Document> { document });

            // Act
            var result = this.alarms.GetDeleteByRuleStatus("id");
            DeleteStatus status = JsonConvert.DeserializeObject<DeleteStatus>(result);

            // Assert
            Assert.Equal("Success", status.Status);
            Assert.Equal(20, status.RecordsDeleted);
            Assert.Equal(timestamp, status.Timestamp);
            Assert.Equal("id", status.Id);

            this.VerifyNoErrorsOrWarnings();
        }

        /**
         * Verify get delete status behavior if the in progress record
         * is too old. If in progress record is more than a minute old,
         * should return unknown status.
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetDeleteStatusIfInProgressOutOfDate()
        {
            // Arrange
            DateTime timestamp = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
            Document document = this.CreateFakeDocument("id", timestamp, "In Progress", 20);
            List<Document> documentResults = new List<Document>
            {
                document
            };

            this.SetUpStorageClientQueryResults(documentResults);

            // Act
            var result = this.alarms.GetDeleteByRuleStatus("id");
            DeleteStatus status = JsonConvert.DeserializeObject<DeleteStatus>(result);

            // Assert
            Assert.Equal("Unknown", status.Status);
            Assert.Null(status.Timestamp);
            Assert.Equal("id", status.Id);
            Assert.Null(status.RecordsDeleted);

            this.VerifyNoErrorsOrWarnings();
        }


        /**
         * Verify if do not find status record for get delete
         * status, will return unknown status.
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetDeleteStatusIfNoRecord()
        {
            // Arrange
            List<Document> documentResults = new List<Document>();
            this.SetUpStorageClientQueryResults(documentResults);

            // Act
            var result = this.alarms.GetDeleteByRuleStatus("id");
            DeleteStatus status = JsonConvert.DeserializeObject<DeleteStatus>(result);

            // Assert
            Assert.Equal("Unknown", status.Status);
            Assert.Null(status.Timestamp);
            Assert.Equal("id", status.Id);
            Assert.Null(status.RecordsDeleted);

            this.VerifyNoErrorsOrWarnings();
        }

        /**
         * Returns Cosmos DB Document object in format of a DeleteStatus object, with the
         * given id, timestamp, status, and recordsDeleted
         */
        private Document CreateFakeDocument(string id, DateTime timestamp, string status, int recordsDeleted)
        {
            Document document = new Document();
            document.SetPropertyValue("Status", status);
            document.Id = id;
            document.SetPropertyValue("Timestamp", timestamp);
            document.SetPropertyValue("RecordsDeleted", recordsDeleted);
            return document;
        }

        /**
         * Set up storage client to return given list of documents on QueryDocuments
         */
        private void SetUpStorageClientQueryResults(List<Document> documentResults)
        {
            this.storageClient.Setup(x => x.QueryDocuments(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FeedOptions>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(documentResults);
        }

        /**
         * Verify logger was never called for an error or a warning
         */
        private void VerifyNoErrorsOrWarnings()
        {
            this.logger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Never);
            this.logger.Verify(l => l.Warn(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Never);
        }
    }
}
