using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class PhoneNumberApiModel
    {
        [JsonProperty(PropertyName = "CountryCode")]
        public string CountryCode { get; set; } = String.Empty;

        [JsonProperty(PropertyName = "PhoneNumber")]
        public string PhoneNumber { get; set; } = String.Empty;

        public PhoneNumberApiModel(string code, string num)
        {
            this.CountryCode = code;
            this.PhoneNumber = num;
        }

        public PhoneNumberApiModel(PhoneNumber number)
        {
            this.CountryCode = number.CountryCode;
            this.PhoneNumber = number.ContactPhoneNumber;
        }

        public PhoneNumberApiModel() { }

        public PhoneNumber ToServiceModel()
        {
            return new PhoneNumber()
            {
                CountryCode = this.CountryCode,
                ContactPhoneNumber = this.PhoneNumber
            };
        }
    }
}
