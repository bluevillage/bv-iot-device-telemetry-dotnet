using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class ActionItemApiModel
    {
        [JsonProperty(PropertyName = "ActionType")]
        public string ActionType { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "Value")]
        public string Value { get; set; } = String.Empty;

        public ActionItemApiModel(string act, string val)
        {
            this.ActionType = act;
            this.Value = val;
        }

        public ActionItemApiModel(Services.Models.ActionItem act)
        {
            this.ActionType = act.ActionType.ToString();
            this.Value = act.Value;
        }

        public ActionItemApiModel() { }

        public ActionItem ToServiceModel()
        {
            if(!Enum.TryParse<TypesOfActions>(this.ActionType, true, out TypesOfActions act)){
                    throw new InvalidInputException($"The action type {this.ActionType} is not valid");
            }
            
            return new Services.Models.ActionItem()
            {
                ActionType = act,
                Value = this.Value
            };
        }
    }
}
