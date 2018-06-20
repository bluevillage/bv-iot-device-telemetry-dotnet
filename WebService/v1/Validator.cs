// Copyright (c) Microsoft. All rights reserved.

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
        bool isValid(IDictionary<String, String> parameters);
    }

    /// <summary>
    /// Class to validate parameters for the actionTypes implemented.
    /// To add new ActionValidator, add a class that implements IActionValidator and include the custom validation in that class.
    /// Then, assign the custom validator to the validationMethod, and call the isValid method to do custom validation.
    /// </summary>
    public class ActionValidator
    {
        public IActionValidator ValidationMethod { get; set; }

        public bool isValid(IDictionary<String, String> parameters)
        {
            if (ValidationMethod is null)
            {
                // Throw an exception
                return false;
            }
            else
            {
                return this.ValidationMethod.isValid(parameters);
            }
        }
    }

    /// <summary>
    /// Email Validator class that uses MailAddress validation.
    /// </summary>
    public class EmailValidator: IActionValidator
    {
        public bool isValid(IDictionary<String, String> parameters)
        {
            if (!parameters.ContainsKey("email")) return false;
            string emailToValidate = parameters["email"];
            try
            {
                MailAddress email = new MailAddress(emailToValidate);
                return true;
            }
            catch (FormatException f)
            {
                // Write exception to logger.
                return false;
            }
        }
    }
}
