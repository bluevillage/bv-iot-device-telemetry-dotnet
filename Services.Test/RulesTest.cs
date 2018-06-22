// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.StorageAdapter;
using Moq;
using Services.Test.helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Services.Test
{
    public class RulesTest
    {
        private readonly Mock<IStorageAdapterClient> storageAdapter;
        private readonly Mock<ILogger> logger;
        private readonly Mock<IServicesConfig> servicesConfig;
        private readonly Mock<IRules> rules;

        public RulesTest()
        {
            this.storageAdapter = new Mock<IStorageAdapterClient>();
            this.logger = new Mock<ILogger>();
            this.servicesConfig = new Mock<IServicesConfig>();
            this.rules = new Mock<IRules>();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task InitialListIsEmptyAsync()
        {
            // Arrange
            this.ThereAreNoRulessInStorage();

            // Act
            var list = await this.rules.Object.GetListAsync(null, 0, 1000, null);

            // Assert
            Assert.Empty(list);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetListWithValuesAsync()
        {
            // Arrange
            this.ThereAreSomeRulesInStorage();

            // Act
            var list = await this.rules.Object.GetListAsync(null, 0, 1000, null);

            // Assert
            Assert.NotEmpty(list);

            // Assert action have been read.
            Assert.True(this.TestActionItemIsParsedProperly(list));
        }

        private void ThereAreNoRulessInStorage()
        {
            this.rules.Setup(x => x.GetListAsync(null, 0, 1000, null))
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

            var sampleActions = new List<ActionItem>
            {
                new ActionItem()
                {
                    ActionType = TypesOfActions.Email,
                    Parameters = new Dictionary<string, object>()
                    {
                        { "email", new List<string>(){ "sampleEmail@gmail.com", "sampleEmail2@gmail.com"}},
                        { "subject", "Test Email"}
                    }
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
                    Conditions = sampleConditions,
                    Actions = sampleActions
                },
                new Rule()
                {
                    Name = "Sample 2",
                    Enabled = true,
                    Description = "Sample description 2",
                    GroupId =  "Prototyping devices",
                    Severity =  SeverityType.Warning,
                    Actions = sampleActions
                }
            };

            this.rules.Setup(x => x.GetListAsync(null, 0, 1000, null))
                .ReturnsAsync(sampleRules);
        }

        private bool TestActionItemIsParsedProperly(List<Rule> rules)
        {
            foreach (Rule rule in rules)
            {
                if (rule.Actions == null) return false;
            }
            return true;
        }
    }
}
