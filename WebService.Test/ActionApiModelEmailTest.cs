using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using WebService.Test.helpers;

namespace WebService.Test
{
    public class ActionApiModelEmailTest
    {
        ActionApiModel testActionApiModel;

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeEmailAndInvalidEmailAddress()
        {
            //Arrange
            testActionApiModel = this.getActionApiModel(EmailActionApiModelType.InvalidEmailAddress);
            //Assert
            Assert.Throws<InvalidInputException>(() => testActionApiModel.ToServiceModel());
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeEmailAndEmailIsNotAList()
        {
            //Arrange
            testActionApiModel = this.getActionApiModel(EmailActionApiModelType.EmailNotAList);
            //Assert
            Assert.Throws<InvalidInputException>(() => testActionApiModel.ToServiceModel());
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeEmailAndEmptyEmailList()
        {
            //Arrange
            testActionApiModel = this.getActionApiModel(EmailActionApiModelType.EmptyList);
            //Assert
            Assert.Throws<InvalidInputException>(() => testActionApiModel.ToServiceModel());
        }

        public ActionApiModel getActionApiModel(EmailActionApiModelType type)
        {
            switch (type)
            {
                case EmailActionApiModelType.EmailNotAList:
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
                case EmailActionApiModelType.EmptyList:
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
                case EmailActionApiModelType.InvalidEmailAddress:
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
    }
}

public enum EmailActionApiModelType
{
    EmptyList,
    InvalidEmailAddress,
    EmailNotAList
}