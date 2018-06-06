// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

/**
 * Stores data for status of delete operation
 */
namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class DeleteStatus
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? RecordsDeleted { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Timestamp { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? RecordsLeftToDelete { get; set; }

        public DeleteStatus() { }

        public DeleteStatus(Document doc)
        {
            if (doc != null)
            {
                this.Id = doc.Id;
                this.Status = doc.GetPropertyValue<string>("Status");
                this.RecordsDeleted = doc.GetPropertyValue<int?>("RecordsDeleted");
                this.RecordsLeftToDelete = doc.GetPropertyValue<int?>("RecordsLeftToDelete");
                this.Timestamp = doc.GetPropertyValue<DateTime?>("Timestamp");
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
