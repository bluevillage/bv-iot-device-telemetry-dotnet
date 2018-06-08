using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class ActionItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TypesOfActions ActionType { get; set; } = new TypesOfActions();
        public string Value { get; set; } = String.Empty;
        public ActionItemTemplate ActionTemplate { get; set; } = new ActionItemTemplate();
        public ActionItem() { }
    }

    public enum TypesOfActions{
        Email,
        Phone
    }
}
