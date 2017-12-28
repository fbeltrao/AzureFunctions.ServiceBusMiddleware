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
    /// <summary>
    /// Service bus topic writer
    /// </summary>
    public class ServiceBusTopicWriter
    {
        /// <summary>
        /// List of properties that will be saves as user properties
        /// </summary>
        public IList<TopicMessageUserProperty> UserProperties { get; set; }

        /// <summary>
        /// Runs returning the topic messages in parameter <paramref name="outputMessages"/>
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <param name="outputMessages"></param>
        /// <returns></returns>
        public async Task<IActionResult> Run(HttpRequest req, ILogger log, IAsyncCollector<Message> outputMessages)
        {
            if (this.UserProperties == null)
            {
                return new BadRequestObjectResult(new { error = $"{nameof(UserProperties)} was not defined" });
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
                foreach (var prop in this.UserProperties)
                {
                    var propValue = element[prop.SourcePropertyName];
                    if (propValue != null)
                        message.UserProperties[prop.TargetPropertyName] = propValue.ToString();
                }

                await outputMessages.AddAsync(message);
                messageCount++;
            }

            log.LogInformation($"Created {messageCount} messages to send to topic");

            return new OkResult();
        }


        /// <summary>
        /// Run an sends the messages through a <see cref="ITopicClient"/>
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <param name="topicClient"></param>
        /// <returns></returns>
        public async Task<IActionResult> RunManual(HttpRequest req, ILogger log, ITopicClient topicClient)
        {
            if (topicClient == null)
            {
                return new BadRequestObjectResult(new { error = $"{nameof(topicClient)} was not defined" });
            }

            if (this.UserProperties == null)
            {
                return new BadRequestObjectResult(new { error = $"{nameof(UserProperties)} was not defined" });
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
                foreach (var prop in this.UserProperties)
                {
                    var propValue = element[prop.SourcePropertyName];
                    if (propValue != null)
                        message.UserProperties[prop.TargetPropertyName] = propValue.ToString();
                }

                outputMessages.Add(message);
            }

            log.LogInformation($"Created {outputMessages.Count} messages to send to topic {topicClient.TopicName}");

            try
            {
                await topicClient.SendAsync(outputMessages);
                log.LogInformation($"Sent {outputMessages.Count} messages to topic {topicClient.TopicName}");

            }
            catch (Exception sendException)
            {
                log.LogError(sendException, "Error sending message to Service Bus Topic");
                return new BadRequestObjectResult("Error sending message to topic");
            }

            return new OkResult();
        }

    }
}
