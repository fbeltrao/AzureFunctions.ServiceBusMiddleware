
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AzureFunctions.ServiceBusMiddleware
{
    /// <summary>
    /// Entry point for Topic writer with properties function
    /// </summary>
    public static class TopicWriterWithPropertiesByHttpTrigger
    {
        static Lazy<IList<Functions.TopicMessageCustomProperty>> TopicMessageCustomProperties = new Lazy<IList<Functions.TopicMessageCustomProperty>>(() =>
        {
            return Functions.TopicMessageCustomProperty.Parse(Environment.GetEnvironmentVariable($"{nameof(TopicWriterWithPropertiesByHttpTrigger)}_Properties"));
        });


        static Lazy<ITopicClient> TopicClient = new Lazy<ITopicClient>(() =>
        {
            var serviceBusConnectionString = Environment.GetEnvironmentVariable($"{nameof(TopicWriterWithPropertiesByHttpTrigger)}_Connection");
            var topicName = Environment.GetEnvironmentVariable($"{nameof(TopicWriterWithPropertiesByHttpTrigger)}_Topic");
            return new TopicClient(serviceBusConnectionString, topicName);
        });

        /*
         *  Use this version once we can use Functions 1.0.7+
        [FunctionName("HttpTrigger_TopicWriterWithProperties")]        
        //[return: ServiceBus("%HttpTrigger_TopicWriterWithProperties_Topic%", Connection = "%HttpTrigger_TopicWriterWithProperties_Connection%", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Topic)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, 
            ILogger log,
            IAsyncCollector<Message> outputMessages)
        {
            var func = new Functions.ServiceBusTopicWriter()
            {
                Properties = TopicMessageCustomProperties.Value
            };

            return await func.Run(req, log, outputMessages);
        }
        */


        [FunctionName(nameof(TopicWriterWithPropertiesByHttpTrigger))]        
        public static async Task<IActionResult> Run(            
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
           TraceWriter log)
        {
            var func = new Functions.ServiceBusTopicWriter()
            {
                Properties = TopicMessageCustomProperties.Value
            };

            return await func.RunManual(req, log, TopicClient.Value);
        }
    }
}
