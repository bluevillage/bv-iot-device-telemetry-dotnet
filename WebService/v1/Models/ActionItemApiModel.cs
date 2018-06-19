using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class ActionItemApiModel
    {
        [JsonProperty(PropertyName = "Type")]
        public string ActionType { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<String, String> Parameters { get; set; } = new Dictionary<String, String>();

        public ActionItemApiModel(string act, Dictionary<String, String> parameters)
        {
            this.ActionType = act;
            this.Parameters = parameters;
        }

        public ActionItemApiModel(Services.Models.ActionItem act)
        {
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

            return new Services.Models.ActionItem()
            {
                ActionType = act,
                Parameters = this.Parameters
            };
        }
    }
}
