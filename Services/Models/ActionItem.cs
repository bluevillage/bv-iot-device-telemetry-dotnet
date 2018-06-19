using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class ActionItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TypesOfActions ActionType { get; set; } = new TypesOfActions();
        public IDictionary<String, String> Parameters { get; set; } = new Dictionary<String, String>();
        public ActionItem() { }
    }

    public enum TypesOfActions
    {
        Email,
        Phone
    }
}
