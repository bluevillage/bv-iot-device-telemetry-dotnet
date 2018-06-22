using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace WebService.Test
{
    class RuleActionTest
    {

        private RuleApiModel testRule;
        private ActionApiModel testAction;

        public RuleActionTest() { }

        public void Should_ReturnActionApiModelObject_When_ProperActionItemObject()
        {
            // Arrage
            var temp_parameters = new Dictionary<string, object>()
            {
                { "Subject", "Alert Notification" },
                {"Body", "Chiller pressure is at 250 which is high" },
                {"Email", new List<string>()
                {
                    "sampleEmail1@gmail.com", "sampleEmail2@gmail.com"
                }
                }
            };
            testRule = this.getRuleApiModelTestObject("Chiller pressure too high", "Pressure > 250", "Chillers", "Critical",true, "Instant", "00:00:00", "pressure" ,"GreaterThan", "250", "Email", temp_parameters);

            //Act
            var ruleServiceModelTest = testRule.ToServiceModel();
            //Assert
            Assert.NotNull(ruleServiceModelTest);
            Assert.NotNull(ruleServiceModelTest.Actions);
        }

        public void Should_ThrowInvalidInputException_When_InvalidActionType()
        {
            // Arrage
            var temp_parameters = new Dictionary<string, object>()
            {
                { "Subject", "Alert Notification" },
                {"Body", "Chiller pressure is at 250 which is high" },
                {"Email", new List<string>()
                {
                    "sampleEmail1@gmail.com", "sampleEmail2@gmail.com"
                }
                }
            };
            testRule = this.getRuleApiModelTestObject("Chiller pressure too high", "Pressure > 250", "Chillers", "Critical", true, "Instant", "00:00:00", "pressure", "GreaterThan", "250", "InvalidActionType", temp_parameters);
            //Act
            //Assert
            Assert.Throws<InvalidInputException>(() => testRule.ToServiceModel());
        }

        public void Should_ReturnTrue_When_ValidEmailAddressesInTheParameters()
        {
            // Arrage
            var temp_parameters = new Dictionary<string, object>()
            {
                { "Subject", "Alert Notification" },
                {"Body", "Chiller pressure is at 250 which is high" },
                {"Email", new List<string>()
                {
                    "sampleEmail1@gmail.com", "sampleEmail2@gmail.com"
                }
                }
            };
            testAction = this.getActionApiModelTest("Email", temp_parameters);
            Enum.TryParse<TypesOfActions>("Email", true, out TypesOfActions act);
            //Act
            // bool isValidTest = ActionApiModel.ValidateActionParameters(act, testAction);
            //Assert
            // Assert.True(isValidTest);
        }

        public void Should_ReturnFalse_When_InvalidEmailAddressInTheParameters()
        {
            // Arrage
            var temp_parameters = new Dictionary<string, object>()
            {
                { "Subject", "Alert Notification" },
                {"Body", "Chiller pressure is at 250 which is high" },
                {"Email", new List<string>()
                {
                    "sampleEmail1@gmail.com", "sampleEmail2gmail.com"
                }
                }
            };
            ActionApiModel testAction = this.getActionApiModelTest("Email", temp_parameters);
            Enum.TryParse<TypesOfActions>("Email", true, out TypesOfActions act);
            //Act
            // bool isValidTest = ActionApiModel.ValidateActionParameters(act, testAction);
            //Assert
            // Assert.False(isValidTest);
        }

        public void Should_ThrowInvalidInputException_When_EmptyEmailAddressList()
        {
            //Arrange
            var temp_parameters = new Dictionary<string, object>()
            {
                { "Subject", "Alert Notification" },
                {"Body", "Chiller pressure is at 250 which is high" },
                {"Email", new List<string>()
                
                }
            };
            ActionApiModel testAction = this.getActionApiModelTest("Email", temp_parameters);
            Enum.TryParse<TypesOfActions>("Email", true, out TypesOfActions act);
            //Act and Assert
            // Assert.Throws<InvalidInputException>(() => testAction.ValidateActionParameters(act, temp_parameters));
        }

        private RuleApiModel getRuleApiModelTestObject(
            string name,
            string description,
            string groupID,
            string severity,
            bool enabled,
            string calculation,
            string timePeriod,
            string conditions_field,
            string conditions_operator,
            string conditions_value,
            string action_type,
            IDictionary<string, object> parameters)
        {
            RuleApiModel rule = new RuleApiModel()
            {
                Name = name,
                Description = description,
                GroupId = groupID,
                Severity = severity,
                Enabled = enabled,
                Calculation = calculation,
                TimePeriod = timePeriod,
                Conditions = new List<ConditionApiModel>()
                {
                    new ConditionApiModel()
                    {
                        Field = conditions_field,
                        Operator = conditions_operator,
                        Value = conditions_value
                    },
                },
                Actions = new List<ActionApiModel>()
                {
                    new ActionApiModel()
                    {
                        ActionType = action_type,
                        Parameters = parameters
                    }
                }
            };
            return rule;
        }

        public ActionApiModel getActionApiModelTest(string actionType, IDictionary<string, object> parameters)
        {
            return new ActionApiModel()
            {
                ActionType = actionType,
                Parameters = parameters
            };
        }
    }
}

enum RuleApiModelTypes
{

}
