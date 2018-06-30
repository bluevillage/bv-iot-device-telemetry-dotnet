// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Helpers;
using Xunit;

namespace Services.Test.helpers
{
    public class QueryBuilderTest
    {
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetDocumentsSqlTest()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Act
            var querySpec = QueryBuilder.GetDocumentsSql(
                "alarms",
                "chiller-01",
                "deviceId",
                from,
                "fromProperty",
                to,
                "toProperty",
                "asc",
                "deviceId",
                0,
                100,
                new string[] { "chiller-01" },
                "deviceId");

            // Assert
            Assert.Equal($"SELECT TOP @top * FROM c WHERE (c[\"doc.schema\"] = @schemaName AND c[@devicesProperty] IN @devices AND c[@byIdPropertyName] = @byId AND c[@fromProperty] >= {from.ToUnixTimeMilliseconds()} AND c[@toProperty] <= {to.ToUnixTimeMilliseconds()}) ORDER BY c[@orderProperty] ASC", querySpec.QueryText);
            Assert.Equal(100, querySpec.Parameters[0].Value);
            Assert.Equal("alarms", querySpec.Parameters[1].Value);
            Assert.Equal("deviceId", querySpec.Parameters[2].Value);
            Assert.Equal(new string[] { "chiller-01" }, querySpec.Parameters[3].Value);
            Assert.Equal("deviceId", querySpec.Parameters[4].Value);
            Assert.Equal("chiller-01", querySpec.Parameters[5].Value);
            Assert.Equal("fromProperty", querySpec.Parameters[6].Value);
            Assert.Equal("toProperty", querySpec.Parameters[7].Value);
            Assert.Equal("deviceId", querySpec.Parameters[8].Value);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetDocumentsSqlWithInvalidInputTest()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Assert
            Assert.Throws<InvalidInputException>(() => QueryBuilder.GetDocumentsSql(
                "alarm's",
                "chiller-01",
                "deviceId",
                from,
                "fromProperty",
                to,
                "toProperty",
                "asc",
                "deviceId",
                0,
                100,
                new string[] { "chiller-01" },
                "deviceId"));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetCountSqlTest()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Act
            var querySpec = QueryBuilder.GetCountSql(
                "alarms",
                "chiller-01",
                "deviceId",
                from,
                "fromProperty",
                to,
                "toProperty",
                new string[] { "chiller-01" },
                "deviceId",
                new string[] { "warning", "critical" },
                "Severty");

            // Assert
            Assert.Equal($"SELECT VALUE COUNT(1) FROM c WHERE (c[\"doc.schema\"] = @schemaName AND c[@devicesProperty] IN @devices AND c[@byIdProperty] = @byId AND c[@fromProperty] >= {from.ToUnixTimeMilliseconds()} AND c[@toProperty] <= {to.ToUnixTimeMilliseconds()} AND c[@filterProperty] IN @filterValues)", querySpec.QueryText);
            Assert.Equal("alarms", querySpec.Parameters[0].Value);
            Assert.Equal("deviceId", querySpec.Parameters[1].Value);
            Assert.Equal(new string[] { "chiller-01" }, querySpec.Parameters[2].Value);
            Assert.Equal("deviceId", querySpec.Parameters[3].Value);
            Assert.Equal("chiller-01", querySpec.Parameters[4].Value);
            Assert.Equal("fromProperty", querySpec.Parameters[5].Value);
            Assert.Equal("toProperty", querySpec.Parameters[6].Value);
            Assert.Equal("Severty", querySpec.Parameters[7].Value);
            Assert.Equal(new string[] { "warning", "critical" }, querySpec.Parameters[8].Value);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetCountSqlWithInvalidInputTest()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Assert
            Assert.Throws<InvalidInputException>(() => QueryBuilder.GetCountSql(
                "alarms",
                "'chiller-01' or 1=1",
                "deviceId",
                from,
                "fromProperty",
                to,
                "toProperty",
                new string[] { "chiller-01" },
                "deviceId",
                new string[] { "warning", "critical" },
                "Severty"));
        }
    }
}
