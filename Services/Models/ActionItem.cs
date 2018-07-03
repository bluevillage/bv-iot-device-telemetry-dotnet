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
    /// Action Type is an enum which is modified when a new ActionType Implementation is added. 
    /// Parameters is a Dictionary used to store all the other related info required for an action type.
    /// </summary>
    public interface IActionItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        Type Type { get; set; }

        Dictionary<string, object> getParameters();
    }

    public class EmailActionItem : IActionItem
    {
        private const string SUBJECT = "Subject";
        private const string TEMPLATE = "Template";
        private const string EMAIL = "Email";

        [JsonConverter(typeof(StringEnumConverter))]
        public Type Type { get; set; }
        [JsonIgnore]
        public string Subject { get; set; } = string.Empty;
        [JsonIgnore]
        public string Body { get; set; } = string.Empty;
        [JsonIgnore]
        public List<string> Emails { get; set; }

        // Dictionary to serialize and store in the dictionary.
        public Dictionary<string, object> Parameters
        {
            get
            {
                return new Dictionary<string, object>()
                {
                    {"Subject", this.Subject },
                    {"Template", this.Body },
                    {"Email", this.Emails }
                };
            }
        }
        public EmailActionItem() { }

        public EmailActionItem(Type type, IDictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase);
            this.Type = type;
            if (parameters.ContainsKey(SUBJECT)) this.Subject = (string)parameters[SUBJECT];
            if (parameters.ContainsKey(TEMPLATE)) this.Body = (string)parameters[TEMPLATE];

            try
            {
                if (parameters.ContainsKey(EMAIL))
                {
                    this.Emails = ((Newtonsoft.Json.Linq.JArray)parameters[EMAIL]).ToObject<List<String>>();
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
                {SUBJECT, this.Subject},
                {TEMPLATE, this.Body },
                {EMAIL, this.Emails }
            };
        }

        private bool IsValid()
        {
            try
            {
                if (!this.Emails.Any()) throw new InvalidInputException("Empty email list provided for actionType Email");
                foreach (string emailToValidate in this.Emails)
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
