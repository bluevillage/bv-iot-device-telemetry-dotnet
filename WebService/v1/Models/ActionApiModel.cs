using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class ActionItemApiModel
    {
        [JsonProperty(PropertyName = "Type")]
        public string ActionType { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<String, String> Parameters { get; set; } = new Dictionary<String, String>();

        private IDictionary<TypesOfActions, Func<IActionValidator>> validationMapping = new Dictionary<TypesOfActions, Func<IActionValidator>>()
            {
                { TypesOfActions.Email, () => new EmailValidator()}
            };

        public ActionItemApiModel(string act, Dictionary<String, String> parameters)
        {
            this.ActionType = act;
            this.Parameters = parameters;
        }

        public ActionItemApiModel(Services.Models.ActionItem act)
        {
            // Backend to Frontend validation ?
                this.ActionType = act.ActionType.ToString();
                this.Parameters = act.Parameters;
        }


        public ActionItemApiModel() { }

        public ActionItem ToServiceModel()
        {
            if (!Enum.TryParse<TypesOfActions>(this.ActionType, true, out TypesOfActions act))
            {
                throw new InvalidInputException($"The action type {this.ActionType} is not valid");
            }
            if (this.ValidateActionParameters(act, this.Parameters))
            {
                return new ActionItem()
                {
                    ActionType = act,
                    Parameters = this.Parameters
                };
            }
            else
            {
                return null;
            }
        }

        private bool ValidateActionParameters(TypesOfActions type, IDictionary<String, String> parameters)
        {
            try
            {
                ActionValidator validator = new ActionValidator()
                {
                    ValidationMethod = this.validationMapping[type]()
                };
                if (validator.isValid(parameters))
                {
                    return true;
                }
                else
                {
                    throw new InvalidInputException($"Invalid parameters specified for actionType {this.ActionType}");

                }
            }
            catch (KeyNotFoundException k)
            {
                throw new InvalidInputException($"Invalid Action Type: {this.ActionType}");
            }
        }
    }
}
