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
    public class ActionApiModel
    {
        [JsonProperty(PropertyName = "Type")]
        public string ActionType { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<String, Object> Parameters { get; set; } = new Dictionary<String, Object>();

        private IDictionary<TypesOfActions, Func<IActionValidator>> validationMapping = new Dictionary<TypesOfActions, Func<IActionValidator>>()
            {
                { TypesOfActions.Email, () => new EmailValidator()}
            };

        public ActionApiModel(string act, Dictionary<String, Object> parameters)
        {
            this.ActionType = act;
            this.Parameters = parameters;
        }

        public ActionApiModel(Services.Models.ActionItem act)
        {
            // Backend to Frontend validation ?
                this.ActionType = act.ActionType.ToString();
                this.Parameters = act.Parameters;
        }


        public ActionApiModel() { }

        public ActionItem ToServiceModel()
        {
            if (!Enum.TryParse<TypesOfActions>(this.ActionType, true, out TypesOfActions act))
            {
                throw new InvalidInputException($"The action type {this.ActionType} is not valid");
            }
            if (this.ValidateActionParameters(act, this.Parameters))
            {
                // Cast email to a list of string.
                this.Parameters["email"] = ((Newtonsoft.Json.Linq.JArray)this.Parameters["email"]).ToObject<List<String>>();
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

        private bool ValidateActionParameters(TypesOfActions type, IDictionary<String, Object> parameters)
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
