// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime
{
    public class AlarmsConfig
    {
        public StorageConfig StorageConfig { get; set; }
        public int MaxRetries { get; set; }
        public int DeleteBatchSize { get; set; }
        public int DeleteIntervalMsec { get; set; }

        public AlarmsConfig(
            string documentDbDatabase,
            string documentDbCollection,
            int maxRetries,
            int deleteBatchSize,
            int deleteIntervalMsec)
        {
            this.StorageConfig = new StorageConfig(documentDbDatabase, documentDbCollection);
            this.MaxRetries = maxRetries;
            this.DeleteBatchSize = deleteBatchSize;
            this.DeleteIntervalMsec = deleteIntervalMsec;
        }
    }
}
