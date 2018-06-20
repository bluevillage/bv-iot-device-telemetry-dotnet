using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;
using Newtonsoft.Json;
using Xunit;
using WebService.Test.helpers;
using Rule = Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Rule;

namespace WebService.Test.Controllers
{
    public class RuleControllerTest
    {
        private const string TELEMETRY_ENDPOINT_URI = "http://localhost:9004/v1/";

        IDictionary<string, object> ruleWithAllValidParameters = new Dictionary<string, object>();

        public RuleControllerTest()
        {
            this.ruleWithAllValidParameters = this.setAllRules();
            this.WhenCreatingNewRuleWithAllValidParameters();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        private void WhenCreatingNewRuleWithAllValidParameters()
        {
            bool isValid = false;
            var clientHandler = new HttpClientHandler();
            using (var client = new HttpClient(clientHandler))
            {
                var httpRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(TELEMETRY_ENDPOINT_URI + "rules")
                };
                string content = JsonConvert.SerializeObject(this.ruleWithAllValidParameters);
                httpRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");
                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                try
                {
                    var task = client.SendAsync(httpRequest);
                    task.Wait();
                    HttpResponseMessage response = task.Result;
                    // Parse response to RuleApiModel object.
                    Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
                    RuleApiModel ruleApiModel = JsonConvert.DeserializeObject<RuleApiModel>(response.ToString());
                    isValid = true;
                }
                catch (Exception e)
                {
                    isValid = false;
                }
            }
            Assert.True(isValid);
        } 

        private Rule CreateNewRule(
            string name, 
            string dateCreated, 
            string dateModified, 
            bool enabled,
            string description,
            string groupId,
            SeverityType severity, 
            CalculationType calculation,
            long timePeriod,
            IList<Condition> conditions, 
            IList<ActionItem> actions)
        {
            return new Rule()
            {
                Name = name,
                DateCreated = dateCreated,
                DateModified = dateModified,
                Enabled = enabled,
                Description = description,
                GroupId = groupId,
                Severity = severity,
                Calculation = calculation,
                TimePeriod = timePeriod,
                Conditions = conditions,
                Actions = actions
            };
        }

        private ActionItem CreateNewAction(
            TypesOfActions actionType, 
            Dictionary<string, Object> parameters
            )
        {
            return new ActionItem()
            {
                ActionType = actionType,
                Parameters = parameters
            };
        }

        private Condition CreateNewCondition(
            string field,
            OperatorType operatorType, 
            string value)
        {
            return new Condition()
            {
                Field = field,
                Operator = operatorType,
                Value = value
            };
        }

        private Dictionary<string, object> setAllRules()
        {
            var ruleWithAllValidParameters = new Dictionary<string, object>()
            {
                {"Name", "Pressure is greater than 300" },
                {"Description", "Pressure > 300" },
                {"GroupId", "chiller" },
                {"Seveiry", "warning" },
                {"Enabled", false },
                {"Calculation", "average" },
                {"TimePeriod", "30000" },
                {"Conditions", new List<Dictionary<string, Object>>()
                {
                    {new Dictionary<string, object>()
                    {
                        {"Field", "pressure" },
                        {"Operator", "greaterthan" },
                        {"Value", 300 }
                    } }
                }
                },
                {"Actions", new List<Dictionary<string, object>>()
                {
                    {new Dictionary<string, object>()
                    {
                        { "Parameters", new Dictionary<string, object>()
                        {
                            {"Subject", "Alert for Chiller" },
                            {"Body", "Chiller property above the set rule" },
                            {"Email", new List<String>(){"<sampleEmail1>@gmail.com", "<sampleEmail2>@yahoo.com"} }

                        }
                        }
                    }
                }
                }
                }
            };
            return ruleWithAllValidParameters;
        }
        
    }
}
