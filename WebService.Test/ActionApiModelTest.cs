// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;
using System.Collections.Generic;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test
{
    public class ActionApiModelTest
    {
        ActionApiModel targetActionApiModel;
        ActionItem testActionItem;
        public ActionApiModelTest() { }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionItemServiceModel_When_ValidActionType()
        {
            // Arrange
            targetActionApiModel = this.getActionApiModel(ActionApiModelType.ValidActionType);

            // Act 
            testActionItem = targetActionApiModel.ToServiceModel();

            // Assert
            Assert.NotNull(testActionItem);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_InvalidActionType()
        {
            // Arrange
            targetActionApiModel = this.getActionApiModel(ActionApiModelType.InvalidActionType);

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => targetActionApiModel.ToServiceModel());
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionItemServiceModel_When_ValidActionTypeValidParameters()
        {
            // Arrange
            targetActionApiModel = this.getActionApiModel(ActionApiModelType.ValidParameters);

            // Act 
            testActionItem = targetActionApiModel.ToServiceModel();

            // Assert
            Assert.NotNull(testActionItem);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_WhenValidActionTypeInvalidParameters()
        {
            // Arrange
            targetActionApiModel = this.getActionApiModel(ActionApiModelType.InvalidParameters);

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => targetActionApiModel.ToServiceModel());
        }


        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeEmailAndInvalidEmailAddress()
        {
            // Arrange
            targetActionApiModel = this.getActionApiModel(ActionApiModelType.EmailActionTypeInvalidEmailAddress);

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => targetActionApiModel.ToServiceModel());
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeEmailAndEmailIsNotAList()
        {
            // Arrange
            targetActionApiModel = this.getActionApiModel(ActionApiModelType.EmailActionTypeEmailNotAList);

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => targetActionApiModel.ToServiceModel());
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeEmailAndEmptyEmailList()
        {
            // Arrange
            targetActionApiModel = this.getActionApiModel(ActionApiModelType.EmailActionTypeEmptyList);

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => targetActionApiModel.ToServiceModel());
        }


        private ActionApiModel getActionApiModel(ActionApiModelType type)
        {
            switch (type)
            {
                case ActionApiModelType.InvalidActionType:
                    return new ActionApiModel()
                    {
                        ActionType = "Invalid ActionType",
                        Parameters = new Dictionary<string, object>()
                        {
                            { "Subject", "Alert Notification" },
                            {"Body", "Chiller pressure is at 250 which is high" },
                            {"Email", new Newtonsoft.Json.Linq.JArray(){"sampleEmail@gmail.com"} }
                        }
                    };
                case ActionApiModelType.ValidActionType:
                    return new ActionApiModel()
                    {
                        ActionType = "Email",
                        Parameters = new Dictionary<string, object>()
                        {
                            { "Subject", "Alert Notification" },
                            {"Body", "Chiller pressure is at 250 which is high" },
                            {"Email", new Newtonsoft.Json.Linq.JArray(){"sampleEmail@gmail.com"}}
                        }
                    };
                case ActionApiModelType.InvalidParameters:
                    return new ActionApiModel()
                    {
                        ActionType = "Email",
                        Parameters = new Dictionary<string, object>()
                        {
                            { "Subject", "Alert Notification" },
                            {"Body", "Chiller pressure is at 250 which is high" },
                            {"Email", new Newtonsoft.Json.Linq.JArray(){"sampleEmailgmail.com"}}
                        }
                    };
                case ActionApiModelType.ValidParameters:
                    return new ActionApiModel()
                    {
                        ActionType = "Email",
                        Parameters = new Dictionary<string, object>()
                        {
                            { "Subject", "Alert Notification" },
                            {"Body", "Chiller pressure is at 250 which is high" },
                            {"Email", new Newtonsoft.Json.Linq.JArray(){"sampleEmail@gmail.com"}}
                        }
                    };
                case ActionApiModelType.EmailActionTypeEmailNotAList:
                    return new ActionApiModel()
                    {
                        ActionType = "Email",
                        Parameters = new Dictionary<string, object>()
                        {
                            { "Subject", "Alert Notification" },
                            {"Body", "Chiller pressure is at 250 which is high" },
                            {"Email", "sampleEmail@gmail.com"}
                        }
                    };
                case ActionApiModelType.EmailActionTypeEmptyList:
                    return new ActionApiModel()
                    {
                        ActionType = "Email",
                        Parameters = new Dictionary<string, object>()
                        {
                            { "Subject", "Alert Notification" },
                            {"Body", "Chiller pressure is at 250 which is high" },
                            {"Email", new Newtonsoft.Json.Linq.JArray() }
                        }
                    };
                case ActionApiModelType.EmailActionTypeInvalidEmailAddress:
                    return new ActionApiModel()
                    {
                        ActionType = "Email",
                        Parameters = new Dictionary<string, object>()
                        {
                            { "Subject", "Alert Notification" },
                            {"Body", "Chiller pressure is at 250 which is high" },
                            {"Email", new Newtonsoft.Json.Linq.JArray(){"sampleEmailgmail.com"}}
                        }
                    };
            }
            return null;
        }
        public enum ActionApiModelType
        {
            ValidActionType,
            InvalidActionType,
            ValidParameters,
            InvalidParameters,
            EmailActionTypeEmptyList,
            EmailActionTypeInvalidEmailAddress,
            EmailActionTypeEmailNotAList
        }
    }
}
