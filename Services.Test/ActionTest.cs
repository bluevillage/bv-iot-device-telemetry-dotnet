using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class ActionTest
    {
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionModel_When_ValidActionType()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                {"Subject", "Alert Notification"},
                { "Body", "Chiller pressure is at 250 which is high"},
                { "Email", new Newtonsoft.Json.Linq.JArray() { "sampleEmail@gmail.com" } }
            };

            // Act 
            var targetAction = new EmailActionItem(Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email, parameters);

            // Assert 
            Assert.True(this.IsEmailActionItemReadProperly(targetAction));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeIsEmailAndInvalidEmail()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                {"Subject", "Alert Notification"},
                { "Body", "Chiller pressure is at 250 which is high"},
                { "Email", new Newtonsoft.Json.Linq.JArray() { "sampleEmailgmail.com" } }
            };

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => new EmailActionItem(Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email, parameters));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_Throw_InvalidInputException_WhenActionTypeIsEmailAndNoEmailField()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                {"Subject", "Alert Notification"},
                { "Body", "Chiller pressure is at 250 which is high"}
            };

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => new EmailActionItem(Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email, parameters));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeIsEmailAndEmailIsString()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                {"Subject", "Alert Notification"},
                { "Body", "Chiller pressure is at 250 which is high"},
                { "Email", "sampleEmail@gmail.com" }
            };

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => new EmailActionItem(Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email, parameters));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionModel_When_ValidActionTypeParametersIsCaseInsensitive()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                { "Subject", "Alert Notification"},
                { "bOdy", "Chiller pressure is at 250 which is high"},
                { "eMail", new Newtonsoft.Json.Linq.JArray() { "sampleEmail@gmail.com" } }
            };

            // Act 
            var targetAction = new EmailActionItem(Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email, parameters);

            // Assert 
            Assert.True(this.IsEmailActionItemReadProperly(targetAction));
        }

        private bool IsEmailActionItemReadProperly(EmailActionItem emailActionItem)
        {
            return emailActionItem.ActionType == Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email
                && string.Equals(emailActionItem.Body, "Chiller pressure is at 250 which is high")
                && this.IsListOfEmailEqual(emailActionItem.Email);
        }

        private bool IsListOfEmailEqual(IList<string> emailList)
        {
            var checkList = new Newtonsoft.Json.Linq.JArray() { "sampleEmail@gmail.com" };
            foreach (var email in checkList)
            {
                if (!emailList.Contains((string)email)) return false;
            }
            return true;
        }
    }
}
