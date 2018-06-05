using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class ActionItemTemplate
    {
        public string templateValue { get; set; } = String.Empty;
        public ActionItemTemplate() { }
    }
}