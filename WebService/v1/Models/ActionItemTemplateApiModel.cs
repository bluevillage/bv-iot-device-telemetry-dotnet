using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class ActionItemTemplateApiModel
    {

        [JsonProperty(PropertyName = "TemplateString")]
        public string TemplateString { get; set; } = String.Empty;


        public ActionItemTemplateApiModel(Services.Models.ActionItemTemplate act)
        {
            this.TemplateString = act.templateValue;
        }

        public ActionItemTemplateApiModel() { }

        public ActionItemTemplate ToServiceModel()
        {

            return new Services.Models.ActionItemTemplate()
            {
                templateValue = TemplateString
            };
        }
    }
}
