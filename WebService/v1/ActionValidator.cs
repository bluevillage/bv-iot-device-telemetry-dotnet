// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1
{
    /// <summary>
    /// Interface to dynamically assign the ActionValidator depending on the ActionType.
    /// </summary>
    public interface IActionValidator
    {
        bool IsValid(IDictionary<String, Object> parameters);
    }

    /// <summary>
    /// Class to validate parameters for the actionTypes implemented.
    /// To add new ActionValidator, add a class that implements IActionValidator and include the custom validation in that class.
    /// Then, assign the custom validator to the validationMethod, and call the isValid method to do custom validation.
    /// </summary>
    public class ActionValidator
    {
        public IActionValidator ValidationMethod { get; set; }

        public bool IsValid(IDictionary<String, Object> parameters)
        {
            if (ValidationMethod is null)
            {
                return false;
            }
            else
            {
                return this.ValidationMethod.IsValid(parameters);
            }
        }
    }

    /// <summary>
    /// Email Validator class that uses MailAddress validation.
    /// </summary>
    public class EmailValidator: IActionValidator
    {
        public bool IsValid(IDictionary<String, Object> parameters)
        {
            if (!(parameters.ContainsKey("Email") || parameters.ContainsKey("email"))) return false;
            IList<String> emailListToValidate = ((Newtonsoft.Json.Linq.JArray)parameters["Email"]).ToObject<List<String>>();
            try
            {
                foreach(String emailToValidate in emailListToValidate)
                {
                    MailAddress email = new MailAddress(emailToValidate);
                }
                return true;
            }
            catch (FormatException f)
            {
                return false;
            }
        }
    }
}
