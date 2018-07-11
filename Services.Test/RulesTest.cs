﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.StorageAdapter;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class RulesTest
    {
        private readonly Mock<IStorageAdapterClient> storageAdapter;
        private readonly Mock<ILogger> logger;
        private readonly Mock<IServicesConfig> servicesConfig;
        private readonly Mock<IRules> rulesMock;
        private readonly Mock<IAlarms> alarms;
        private readonly IRules rules;

        private const int LIMIT = 1000;

        public RulesTest()
        {
            this.storageAdapter = new Mock<IStorageAdapterClient>();
            this.logger = new Mock<ILogger>();
            this.servicesConfig = new Mock<IServicesConfig>();
            this.rulesMock = new Mock<IRules>();
            this.alarms = new Mock<IAlarms>();
            this.rules = new Rules(this.storageAdapter.Object, this.logger.Object, this.alarms.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task InitialListIsEmptyAsync()
        {
            // Arrange
            this.ThereAreNoRulessInStorage();

            // Act
            var list = await this.rulesMock.Object.GetListAsync(null, 0, LIMIT, null, false);

            // Assert
            Assert.Equal(0, list.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetListWithValuesAsync()
        {
            // Arrange
            this.ThereAreSomeRulesInStorage();

            // Act
            var list = await this.rulesMock.Object.GetListAsync(null, 0, LIMIT, null, false);

            // Assert
            Assert.NotEmpty(list);
        }

        /**
         * Verify call to delete on non-deleted rule will get and update rule as expected.
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task VerifyBasicDeleteAsync()
        {
            // Arrange
            Rule test = new Rule
            {
                Enabled = true,
                Deleted = false
            };

            this.SetUpStorageAdapterGet(test);

            // Act
            await this.rules.DeleteAsync("id");

            this.storageAdapter.Verify(x => x.GetAsync(It.IsAny<string>(), "id"), Times.Once);
            this.storageAdapter.Verify(x => x.UpdateAsync(It.IsAny<string>(), "id", It.IsAny<string>(), "123"), Times.Once);
        }

        /**
         * If rule is already deleted and delete is called, verify it will not throw exception
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task VerifyDeleteDoesNotFailIfAlreadyDeletedAsync()
        {
            // Arrange
            Rule test = new Rule
            {
                Enabled = false,
                Deleted = true
            };

            this.SetUpStorageAdapterGet(test);

            // Act
            await this.rules.DeleteAsync("id");

            this.storageAdapter.Verify(x => x.GetAsync(It.IsAny<string>(), "id"), Times.Once);
            this.storageAdapter.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /**
         * If rule does not exist and delete is called, verify it will not throw exception
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task VerifyDeleteDoesNotFailIfRuleNotExistsAsync()
        {
            // Arrange
            Rule test = new Rule
            {
                Enabled = false,
                Deleted = true
            };

            this.storageAdapter
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ResourceNotFoundException());

            // Act
            await this.rules.DeleteAsync("id");

            this.storageAdapter.Verify(x => x.GetAsync(It.IsAny<string>(), "id"), Times.Once);
            this.storageAdapter.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }


        /** If get rule throws an exception that is not a resource not found exception,
         * delete should throw that exception.
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task VerifyDeleteFailsIfGetRuleThrowsException()
        {
            // Arrange
            Rule test = new Rule
            {
                Enabled = false,
                Deleted = true
            };

            this.storageAdapter
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await this.rules.DeleteAsync("id"));

            this.storageAdapter.Verify(x => x.GetAsync(It.IsAny<string>(), "id"), Times.Once);
            this.storageAdapter.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /**
         * If upsert is called on a deleted rule, verify a NotFoundException will be thrown.
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task VerifyCannotUpdateDeletedRuleAsync()
        {
            // Arrange
            Rule test = new Rule
            {
                Enabled = false,
                Deleted = true,
                Id = "id",
                ETag = "123"
            };
            this.SetUpStorageAdapterGet(test);

            // Act
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await this.rules.UpsertIfNotDeletedAsync(test));

            // Assert
            this.storageAdapter.Verify(x => x.GetAsync(It.IsAny<string>(), "id"), Times.Once);
            this.storageAdapter.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /**
         * If GetListAsync() is called with includeDeleted = false, verify no
         * deleted rules will be returned
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task VerifyGetBehaviorIfDontIncludeDeleted()
        {
            // Arrange
            Rule test = new Rule
            {
                Enabled = false,
                Deleted = true,
                Id = "id",
                ETag = "123"
            };
            string ruleString = JsonConvert.SerializeObject(test);
            ValueApiModel model = new ValueApiModel
            {
                Data = ruleString,
                ETag = "123",
                Key = "id"
            };
            ValueListApiModel result = new ValueListApiModel();
            result.Items = new List<ValueApiModel> { model };
            this.storageAdapter.Setup(x => x.GetAllAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(result));

            // Act
            List<Rule> rulesList = await this.rules.GetListAsync("asc", 0, LIMIT, null, false);

            // Assert
            Assert.Empty(rulesList);
            this.storageAdapter.Verify(x => x.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        /**
         * If GetListAsync() is called with includeDeleted = true, verify 
         * deleted rules will be returned
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task VerifyGetBehaviorIfDoIncludeDeleted()
        {
            // Arrange
            Rule test = new Rule
            {
                Enabled = false,
                Deleted = true,
                Id = "id",
                ETag = "123"
            };
            string ruleString = JsonConvert.SerializeObject(test);
            ValueApiModel model = new ValueApiModel
            {
                Data = ruleString,
                ETag = "123",
                Key = "id"
            };
            ValueListApiModel result = new ValueListApiModel();
            result.Items = new List<ValueApiModel> { model };
            this.storageAdapter.Setup(x => x.GetAllAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(result));

            // Act
            List<Rule> rulesList = await this.rules.GetListAsync("asc", 0, LIMIT, null, true);

            // Assert
            Assert.Single(rulesList);
            this.storageAdapter.Verify(x => x.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        /**
          * If upsert is called with a rule that is not created and a 
          * specified Id, it should be created with that Id.
        */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UpsertNewRuleWithId_CreatesNewRuleWithId()
        {
            // Arrange
            string newRuleId = "TESTRULEID" + DateTime.Now.ToString("yyyyMMddHHmmss");
            Rule test = new Rule
            {
                Enabled = true,
                Id = newRuleId
            };

            string ruleString = JsonConvert.SerializeObject(test);

            ValueApiModel result = new ValueApiModel
            {
                Data = ruleString,
                ETag = "1234",
                Key = newRuleId
            };

            this.storageAdapter
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ResourceNotFoundException());

            this.storageAdapter.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(result));

            // Act
            Rule rule = await this.rules.UpsertIfNotDeletedAsync(test);

            // Assert
            Assert.Equal(newRuleId, rule.Id);
        }

        private void ThereAreNoRulessInStorage()
        {
            this.rulesMock.Setup(x => x.GetListAsync(null, 0, LIMIT, null, false))
                .ReturnsAsync(new List<Rule>());
        }

        private void ThereAreSomeRulesInStorage()
        {
            var sampleConditions = new List<Condition>
            {
                new Condition()
                {
                    Field = "sample_conddition",
                    Operator = OperatorType.Equals,
                    Value = "1"
                }
            };

            var sampleRules = new List<Rule>
            {
                new Rule()
                {
                    Name = "Sample 1",
                    Enabled = true,
                    Description = "Sample description 1",
                    GroupId = "Prototyping devices",
                    Severity = SeverityType.Critical,
                    Conditions = sampleConditions
                },
                new Rule()
                {
                    Name = "Sample 2",
                    Enabled = true,
                    Description = "Sample description 2",
                    GroupId =  "Prototyping devices",
                    Severity =  SeverityType.Warning,
                    Conditions =  sampleConditions
                }
            };

            this.rulesMock.Setup(x => x.GetListAsync(null, 0, LIMIT, null, false))
                .ReturnsAsync(sampleRules);
        }

        /**
         * Set up storage adapater to return given rule as part of ValueApiModel on GetAsync
         */
        private void SetUpStorageAdapterGet(Rule toReturn)
        {
            string ruleString = JsonConvert.SerializeObject(toReturn);
            ValueApiModel result = new ValueApiModel
            {
                Data = ruleString,
                ETag = "123",
                Key = "id"
            };

            this.storageAdapter
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(result));
        }
    }
}
