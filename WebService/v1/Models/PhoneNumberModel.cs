using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class PhoneNumberModel
    {
        [JsonProperty(PropertyName = "CountryCode")]
        public int CountryCode { get; set; } = 0;

        [JsonProperty(PropertyName = "PhoneNumber")]
        public int PhoneNumber { get; set; } = 0;

        public PhoneNumberModel(int code, int num)
        {
            this.CountryCode = code;
            this.PhoneNumber = num;
        }
    }
}
