// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    /// <summary>
    /// Class to enclose all the details of an action. 
    /// Action Type is an enum which is modified when a new ActionTypeImplementation is added. 
    /// Parameters is a Dictionary used to store all the other related info required for an action type.
    /// </summary>
    public interface IActionItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        Type ActionType { get; set; }

        Dictionary<string, object> getParameters();
    }

    public class EmailActionItem : IActionItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Type ActionType { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<string> Email { get; set; }

        public EmailActionItem() { }

        public EmailActionItem(Type type, IDictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase);
            this.ActionType = type;
            if (parameters.ContainsKey("Subject")) this.Subject = (string)parameters["Subject"];
            if (parameters.ContainsKey("Body")) this.Body = (string)parameters["Body"];

            try
            {
                if (parameters.ContainsKey("Email"))
                {
                    this.Email = ((Newtonsoft.Json.Linq.JArray)parameters["Email"]).ToObject<List<String>>();
                }
                else
                {
                    throw new InvalidInputException("No Email address provided for actionType Email");
                }
            }
            catch (InvalidCastException e)
            {
                throw new InvalidInputException("Email field is a list of string");
            }

            if (!this.IsValid())
            {
                throw new InvalidInputException("Invalid Email Address.");
            }
        }

        public Dictionary<string, object> getParameters()
        {
            return new Dictionary<string, object>()
            {
                {"Subject", this.Subject},
                {"Body", this.Body },
                {"Email", this.Email }
            };
        }

        private bool IsValid()
        {
            try
            {
                if (!this.Email.Any()) throw new InvalidInputException("Empty email list provided for actionType Email");
                foreach (string emailToValidate in this.Email)
                {
                    MailAddress mail = new MailAddress(emailToValidate);
                }
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidCastException e)
            {
                return false;
            }
        }
    }

    public enum Type
    {
        Email
    }
}
