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
                "alarm",
                "bef978d4-54f6-429f-bda5-db2494b833ef",
                "rule.id",
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                "asc",
                "device.msg.received",
                0,
                100,
                new string[] { "chiller-01.0", "chiller-02.0" },
                "device.id");

            // Assert
            Assert.Equal($"SELECT TOP @top * FROM c WHERE (c[\"doc.schema\"] = @schemaName AND c['device.id'] IN ('chiller-01.0', 'chiller-02.0') AND c[@byIdProperty] = @byId AND c[@fromProperty] >= {from.ToUnixTimeMilliseconds()} AND c[@toProperty] <= {to.ToUnixTimeMilliseconds()}) ORDER BY c[@orderProperty] ASC", querySpec.QueryText);
            Assert.Equal(100, querySpec.Parameters[0].Value);
            Assert.Equal("alarm", querySpec.Parameters[1].Value);
            Assert.Equal("device.id", querySpec.Parameters[2].Value);
            Assert.Equal(new String[] { "chiller-01.0", "chiller-02.0" }, querySpec.Parameters[3].Value);
            Assert.Equal("rule.id", querySpec.Parameters[4].Value);
            Assert.Equal("bef978d4-54f6-429f-bda5-db2494b833ef", querySpec.Parameters[5].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[6].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[7].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[8].Value);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetDocumentsSqlWithNullIdPropertyTest()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Act
            var querySpec = QueryBuilder.GetDocumentsSql(
                "alarm",
                null,
                null,
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                "asc",
                "device.msg.received",
                0,
                100,
                new string[] { "chiller-01.0", "chiller-02.0" },
                "device.id");

            // Assert
            Assert.Equal($"SELECT TOP @top * FROM c WHERE (c[\"doc.schema\"] = @schemaName AND c['device.id'] IN ('chiller-01.0', 'chiller-02.0') AND c[@fromProperty] >= {from.ToUnixTimeMilliseconds()} AND c[@toProperty] <= {to.ToUnixTimeMilliseconds()}) ORDER BY c[@orderProperty] ASC", querySpec.QueryText);
            Assert.Equal(100, querySpec.Parameters[0].Value);
            Assert.Equal("alarm", querySpec.Parameters[1].Value);
            Assert.Equal("device.id", querySpec.Parameters[2].Value);
            Assert.Equal(new String[] { "chiller-01.0", "chiller-02.0" }, querySpec.Parameters[3].Value);
            Assert.Null(querySpec.Parameters[4].Value);
            Assert.Null(querySpec.Parameters[5].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[6].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[7].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[8].Value);
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
                "bef978d4-54f6-429f-bda5-db2494b833ef",
                "rule.id",
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                "asc",
                "device.msg.received",
                0,
                100,
                new string[] { "chiller-01.0", "chiller-02.0" },
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
                "alarm",
                "bef978d4-54f6-429f-bda5-db2494b833ef",
                "rule.id",
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                new string[] { "chiller-01.0", "chiller-02.0" },
                "device.id",
                new string[] { "open", "acknowledged" },
                "status");

            // Assert
            Assert.Equal($"SELECT VALUE COUNT(1) FROM c WHERE (c[\"doc.schema\"] = @schemaName AND c['device.id'] IN ('chiller-01.0', 'chiller-02.0') AND c[@byIdProperty] = @byId AND c[@fromProperty] >= {from.ToUnixTimeMilliseconds()} AND c[@toProperty] <= {to.ToUnixTimeMilliseconds()} AND c['status'] IN ('open', 'acknowledged'))", querySpec.QueryText);
            Assert.Equal("alarm", querySpec.Parameters[0].Value);
            Assert.Equal("device.id", querySpec.Parameters[1].Value);
            Assert.Equal(new String[] { "chiller-01.0", "chiller-02.0" }, querySpec.Parameters[2].Value);
            Assert.Equal("rule.id", querySpec.Parameters[3].Value);
            Assert.Equal("bef978d4-54f6-429f-bda5-db2494b833ef", querySpec.Parameters[4].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[5].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[6].Value);
            Assert.Equal("status", querySpec.Parameters[7].Value);
            Assert.Equal(new string[] { "open", "acknowledged" }, querySpec.Parameters[8].Value);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetCountSqlWithNullIdPropertyTest()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Act
            var querySpec = QueryBuilder.GetCountSql(
                "alarm",
                null,
                null,
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                new string[] { "chiller-01.0", "chiller-02.0" },
                "device.id",
                new string[] { "open", "acknowledged" },
                "status");

            // Assert
            Assert.Equal($"SELECT VALUE COUNT(1) FROM c WHERE (c[\"doc.schema\"] = @schemaName AND c['device.id'] IN ('chiller-01.0', 'chiller-02.0') AND c[@fromProperty] >= {from.ToUnixTimeMilliseconds()} AND c[@toProperty] <= {to.ToUnixTimeMilliseconds()} AND c['status'] IN ('open', 'acknowledged'))", querySpec.QueryText);
            Assert.Equal("alarm", querySpec.Parameters[0].Value);
            Assert.Equal("device.id", querySpec.Parameters[1].Value);
            Assert.Equal(new String[] { "chiller-01.0", "chiller-02.0" }, querySpec.Parameters[2].Value);
            Assert.Null(querySpec.Parameters[3].Value);
            Assert.Null(querySpec.Parameters[4].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[5].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[6].Value);
            Assert.Equal("status", querySpec.Parameters[7].Value);
            Assert.Equal(new string[] { "open", "acknowledged" }, querySpec.Parameters[8].Value);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetCountSqlWithInvalidInputTest()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Assert
            Assert.Throws<InvalidInputException>(() => QueryBuilder.GetCountSql(
                "alarm",
                "'chiller-01' or 1=1",
                "rule.id",
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                new string[] { "chiller-01.0", "chiller-02.0" },
                "device.id",
                new string[] { "open", "acknowledged" },
                "status"));
        }
    }
}
