// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class ActionApiModel
    {
        [JsonProperty(PropertyName = "Type")]
        public string ActionType { get; set; } = String.Empty;

        // Parameters dictionary is case-insensitive.
        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<String, Object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        // Maps Types of actions to their custom validator class.
        public static IDictionary<TypesOfActions, Func<IActionValidator>> validationMapping = new Dictionary<TypesOfActions, Func<IActionValidator>>()
            {
                { TypesOfActions.Email, () => new EmailValidator()}
            };

        public ActionApiModel(string action, Dictionary<String, Object> parameters)
        {
            this.ActionType = action;
            this.Parameters = parameters;
        }

        public ActionApiModel(Services.Models.ActionItem action)
        {
            this.ActionType = action.ActionType.ToString();
            this.Parameters = action.Parameters;
        }

        public ActionApiModel() { }

        public ActionItem ToServiceModel()
        {
            if (!Enum.TryParse<TypesOfActions>(this.ActionType, true, out TypesOfActions action))
            {
                throw new InvalidInputException($"The action type {this.ActionType} is not valid");
            }
            ActionValidator validator = new ActionValidator()
            {
                ValidationMethod = ActionApiModel.validationMapping[action]()
            };
            // Returns parameters to allow custom casting in implementation.
            this.Parameters = validator.IsValid(this.Parameters);
            return new ActionItem()
            {
                ActionType = action,
                Parameters = this.Parameters
            };
        }
    }
}
