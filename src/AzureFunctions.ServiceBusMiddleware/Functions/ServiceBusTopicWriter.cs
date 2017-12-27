using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.ServiceBusMiddleware.Functions
{
    public class ServiceBusTopicWriter
    {
        public IList<TopicMessageCustomProperty> Properties { get; set; }


        public async Task<IActionResult> Run(HttpRequest req, TraceWriter log, IAsyncCollector<Message> outputMessages)
        {
            if (this.Properties == null)
            {
                return new BadRequestObjectResult(new { error = $"{nameof(Properties)} was not defined" });
            }

            // Stream analytics will send an json array of elements
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var payloadData = JsonConvert.DeserializeObject(requestBody);

            var data = JsonConvert.DeserializeObject(requestBody) as JArray;

            if (data == null || data.Count == 0)
                return new BadRequestObjectResult("Expecting an array of json elements in the request body");

            var messageCount = 0;
            foreach (var element in data)
            {
                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(element)));
                foreach (var prop in this.Properties)
                {
                    var propValue = element[prop.SourcePropertyName];
                    if (propValue != null)
                        message.UserProperties[prop.TargetPropertyName] = propValue.ToString();
                }

                await outputMessages.AddAsync(message);
                messageCount++;
            }

            log.Info($"Created {messageCount} messages to send to topic");

            return new OkResult();
        }


        public async Task<IActionResult> RunManual(HttpRequest req, TraceWriter log, ITopicClient topicClient)
        {
            if (topicClient == null)
            {
                return new BadRequestObjectResult(new { error = $"{nameof(topicClient)} was not defined" });
            }

            if (this.Properties == null)
            {
                return new BadRequestObjectResult(new { error = $"{nameof(Properties)} was not defined" });
            }
            
            // Stream analytics will send an json array of elements
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var payloadData = JsonConvert.DeserializeObject(requestBody);

            var data = JsonConvert.DeserializeObject(requestBody) as JArray;

            if (data == null || data.Count == 0)
                return new BadRequestObjectResult("Expecting an array of json elements in the request body");

            var outputMessages = new List<Message>();
            foreach (var element in data)
            {
                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(element)));
                foreach (var prop in this.Properties)
                {
                    var propValue = element[prop.SourcePropertyName];
                    if (propValue != null)
                        message.UserProperties[prop.TargetPropertyName] = propValue.ToString();
                }

                outputMessages.Add(message);
            }

            log.Info($"Created {outputMessages.Count} messages to send to topic {topicClient.TopicName}");

            try
            {
                await topicClient.SendAsync(outputMessages);
                log.Info($"Sent {outputMessages.Count} messages to topic {topicClient.TopicName}");

            }
            catch (Exception sendException)
            {
                log.Error("Error sending message to Service Bus Topic", sendException);
                return new BadRequestObjectResult("Error sending message to topic");
            }

            return new OkResult();
        }

    }
}
