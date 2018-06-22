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

        // Parameters dictionary is Ordinal IgnoreCase to follow CaseInsensitive pattern used by the JsonProperty(PropertyName = ..) match. 
        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<String, Object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public static IDictionary<TypesOfActions, Func<IActionValidator>> validationMapping = new Dictionary<TypesOfActions, Func<IActionValidator>>()
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
                this.ActionType = act.ActionType.ToString();
                this.Parameters = act.Parameters;
        }


        public ActionApiModel() { }

        public ActionItem ToServiceModel()
        {
            // How to cast in this case? 
            if (!Enum.TryParse<TypesOfActions>(this.ActionType, true, out TypesOfActions act))
            {
                throw new InvalidInputException($"The action type {this.ActionType} is not valid");
            }
            this.Parameters = ValidateActionParametersAndCastParameters(act, this.Parameters);
            return new ActionItem()
            {
                ActionType = act,
                Parameters = this.Parameters
            };
        }

        public static Dictionary<string, object> ValidateActionParametersAndCastParameters(TypesOfActions type, IDictionary<String, Object> parameters)
        {
            ActionValidator validator = new ActionValidator()
            {
                ValidationMethod = ActionApiModel.validationMapping[type]()
            };
            return validator.IsValid(parameters);
        }
    }
}
