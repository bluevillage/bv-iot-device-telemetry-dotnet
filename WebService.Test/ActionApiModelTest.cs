using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using WebService.Test.helpers;
using Newtonsoft.Json;

namespace WebService.Test
{
    public class ActionApiModelTest
    {
        ActionApiModel testActionModel;
        ActionItem testActionItem;
        public ActionApiModelTest() { }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionItemServiceModel_When_ValidActionType()
        {
            //Arrange
            testActionModel = this.getActionApiModel(ActionApiModelType.ValidActionType);
            //Act 
            testActionItem = testActionModel.ToServiceModel();
            //Assert
            Assert.NotNull(testActionItem);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_InvalidActionType()
        {
            //Arrange
            testActionModel = this.getActionApiModel(ActionApiModelType.InvalidActionType);

            //Assert
            Assert.Throws<InvalidInputException>(() => testActionModel.ToServiceModel());
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionItemServiceModel_When_ValidActionTypeValidParameters()
        {
            //Arrange
            testActionModel = this.getActionApiModel(ActionApiModelType.ValidParameters);
            //Act 
            testActionItem = testActionModel.ToServiceModel();
            //Assert
            Assert.NotNull(testActionItem);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_WhenValidActionTypeInvalidParameters()
        {
            //Arrange
            testActionModel = this.getActionApiModel(ActionApiModelType.InvalidParameters);

            //Assert
            Assert.Throws<InvalidInputException>(() => testActionModel.ToServiceModel());
        }


        public ActionApiModel getActionApiModel(ActionApiModelType type)
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
            }
            return null;
        }
    }

    public enum ActionApiModelType
    {
        ValidActionType,
        InvalidActionType,
        ValidParameters,
        InvalidParameters
    }
}
