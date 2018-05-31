using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class PhoneNumber
    {
        public string CountryCode { get; set; } = String.Empty;
        public string ContactPhoneNumber { get; set; } = String.Empty;
        public PhoneNumber() { }
    }
}
