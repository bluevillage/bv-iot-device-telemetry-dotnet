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

        [JsonProperty(PropertyName = "ActionTemplate")]
        public ActionItemTemplateApiModel ActionTemplate { get; set; } = new ActionItemTemplateApiModel();


        public ActionItemApiModel() { }
        public ActionItemApiModel(Services.Models.ActionItem act)
        {
            this.ActionType = act.ActionType.ToString();
            this.Value = act.Value;
            this.ActionTemplate = new ActionItemTemplateApiModel(act.ActionTemplate);
        }

        public ActionItem ToServiceModel()
        {
            if(!Enum.TryParse<TypesOfActions>(this.ActionType, true, out TypesOfActions act)){
                    throw new InvalidInputException($"The action type {this.ActionType} is not valid");
            }

            return new Services.Models.ActionItem()
            {
                ActionType = act,
                Value = this.Value,
                ActionTemplate = this.ActionTemplate.ToServiceModel()
            };
        }
    }
}
