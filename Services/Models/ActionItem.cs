// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    /// <summary>
    /// Class to enclose all the details of an action. 
    /// Action Type is an enum which is modified when a new ActionTypeImplementation is added. 
    /// Parameters is a Dictionary used to store all the other related info required for an action type.
    /// </summary>
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
