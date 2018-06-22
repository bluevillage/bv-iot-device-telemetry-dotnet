// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1
{
    /// <summary>
    /// Interface to dynamically assign the ActionValidator depending on the ActionType.
    /// </summary>
    public interface IActionValidator
    {
        Dictionary<string, object> IsValid(IDictionary<String, Object> parameters);
    }

    /// <summary>
    /// Class to validate parameters for the actionTypes implemented.
    /// To add new ActionValidator, add a class that implements IActionValidator and include the custom validation in that class.
    /// Then, assign the custom validator to the validationMethod, and call the isValid method to do custom validation.
    /// </summary>
    public class ActionValidator
    {
        public IActionValidator ValidationMethod { get; set; }

        public Dictionary<string, object> IsValid(IDictionary<String, Object> parameters)
        {
            if (ValidationMethod is null)
            {
                throw new InvalidConfigurationException("Validation Method not specified.");
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
    public class EmailValidator : IActionValidator
    {
        public Dictionary<string, object> IsValid(IDictionary<String, Object> parameters)
        {
            Dictionary<string, object> tempParameters = new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase);
            if (!tempParameters.ContainsKey("email"))
            {
                throw new InvalidInputException("Email not specified for actionType Email");
            }
            try
            {
                IList<String> emailListToValidate = ((Newtonsoft.Json.Linq.JArray)parameters["Email"]).ToObject<List<String>>();
                if (!emailListToValidate.Any())
                {
                    throw new InvalidInputException("Empty email list for actionType email");
                }
                foreach (String emailToValidate in emailListToValidate)
                {
                    MailAddress email = new MailAddress(emailToValidate);
                }
                tempParameters["email"] = ((Newtonsoft.Json.Linq.JArray)parameters["Email"]).ToObject<IList<String>>();
                return tempParameters;
            }
            catch (FormatException f)
            {
                throw new InvalidInputException("Invalid Email Parameters.");
            }
            catch (InvalidCastException e)
            {
                throw new InvalidInputException("String specified for Email parameter for action type Email");
            }
        }
    }
}
