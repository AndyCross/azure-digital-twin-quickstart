using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Linq;
using Azure.DigitalTwins.Core;
using System;
using Azure.Identity;
using Azure.Core.Pipeline;
using Azure;
using System.Threading.Tasks;

namespace funcs
{
    public class PowerMeter {
        public double Consumption { get; set; }
    }
    public static class IoTHubTwinBridge
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("IoTHubTwinBridge")]
        public static async Task Run([IoTHubTrigger("messages/events", Connection = "IoTNorth")]EventData message, ILogger log)
        {
            string body = Encoding.UTF8.GetString(message.Body.Array);
            log.LogInformation($"C# IoT Hub trigger function processed a message: {body}");
            var powerMeterPayload = JsonSerializer.Deserialize<PowerMeter>(body);
            log.LogInformation($"The powerMeter has some value {powerMeterPayload.Consumption}");

            //what device sent this?
            log.LogInformation($"Some app level properties: {string.Join(',', message.Properties.Select(p=> $"{p.Key}:{p.Value}"))}");
            log.LogInformation($"Some system properties: {string.Join(',', message.SystemProperties.Select(p=> $"{p.Key}:{p.Value}"))}");

            log.LogInformation("********************************************************************************************************************************");
            log.LogInformation($"We can rely on the value of this property EventData.SystemProperties['iothub-connection-device-id']: {message.SystemProperties["iothub-connection-device-id"]}");
            log.LogInformation("********************************************************************************************************************************");
            var powerMeterId = (string)message.SystemProperties["iothub-connection-device-id"];

            // what do we do about Semantic Versions?

            // do we want to do something about updating average values? 

            // let's just get the Digital Twin to do all that for us
            var dtClient = GetTwinClient("https://iotnorth-twin.api.uks.digitaltwins.azure.net");
            var updateDigitalTwinData = new JsonPatchDocument();
            updateDigitalTwinData.AppendReplace("/importPower", powerMeterPayload.Consumption);
            updateDigitalTwinData.AppendReplace("/timestamp", DateTime.UtcNow.ToString("u"));
            await dtClient.UpdateDigitalTwinAsync(powerMeterId, updateDigitalTwinData);

            // this trivial approach to properties is incorrect, we should be using telemetry to enable downstream consumers
            await dtClient.PublishTelemetryAsync(powerMeterId, Guid.NewGuid().ToString(), body);
        }

        private static DigitalTwinsClient GetTwinClient(string adtInstanceUrl)
        {
            DefaultAzureCredential credentialsForDT = new DefaultAzureCredential();
            DigitalTwinsClient clientDT = new DigitalTwinsClient(
                                                new Uri(adtInstanceUrl),
                                                credentialsForDT,
                                                new DigitalTwinsClientOptions
                                                {
                                                    Transport = new HttpClientTransport(httpClient)
                                                }
                                            );
            return clientDT;
        }
    }
}