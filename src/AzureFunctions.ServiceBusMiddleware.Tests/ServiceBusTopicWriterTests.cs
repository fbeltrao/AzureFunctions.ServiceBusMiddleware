using AzureFunctions.ServiceBusMiddleware.Functions;
using AzureFunctions.Tests.Common;
using Microsoft.Azure.ServiceBus;
using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AzureFunctions.ServiceBusMiddleware.Tests
{
    public class ServiceBusTopicWriterTests : AzureFunctionTest
    {
        [Fact]
        public async Task RunManual_When_Empty_Properties_Sends_Messages_Without_Custom_Properties()
        {
            var target = new ServiceBusTopicWriter()
            {
                Properties = new TopicMessageCustomProperty[0]
            };

            var topicClient = new Mock<ITopicClient>();
            topicClient
                .Setup(c => c.SendAsync(It.Is<IList<Message>>(x => x.Count == 2)))
                .Returns(Task.CompletedTask);

            var actual = await target.RunManual(
                CreateJsonRequest(@"[{},{}]")
                , new Mock<ILogger>().Object
                , topicClient.Object);

            Assert.IsType<OkResult>(actual);
        }

        [Fact]
        public async Task RunManual_When_Body_Is_Empty_Return_BadRequest()
        {
            var target = new ServiceBusTopicWriter();           

            var topicClient = new Mock<ITopicClient>();

            var actual = await target.RunManual(
                CreateEmptyRequest(),
                new Mock<ILogger>().Object,
                new Mock<ITopicClient>().Object);

            Assert.IsType<BadRequestObjectResult>(actual);
        }


        [Fact]
        public async Task RunManual_When_Body_Has_Not_Matching_Properties_Send_Items_To_TopicClient_Without_Custom_Properties()
        {
            var target = new ServiceBusTopicWriter()
            {
                Properties = new TopicMessageCustomProperty[] { new TopicMessageCustomProperty("test") }
            };
            

            var topicClient = new Mock<ITopicClient>();
            topicClient
                .Setup(x => x.SendAsync(It.Is<IList<Message>>(m => m.Count == 2 && !m.Any(i => i.UserProperties.Count > 0))))
                .Returns(Task.CompletedTask);

            var actual = await target.RunManual(
                CreateJsonRequest("[{}, {}]")
                , new Mock<ILogger>().Object
                , topicClient.Object);

            Assert.IsType<OkResult>(actual);
        }


        [Fact]
        public async Task RunManual_When_Body_Has_Matching_Properties_Send_Items_To_TopicClient_With_Custom_Properties()
        {
            var target = new ServiceBusTopicWriter()
            {
                Properties = new TopicMessageCustomProperty[] { new TopicMessageCustomProperty("test") }
            };
            
            var topicClient = new Mock<ITopicClient>();
            topicClient
                .Setup(x => x.SendAsync(It.Is<IList<Message>>(m => m.Count == 2 && (string)m[0].UserProperties["test"] == "value1" && (string)m[1].UserProperties["test"] == "value2")))
                .Returns(Task.CompletedTask);

            var actual = await target.RunManual(
                CreateJsonRequest(new[] { new { test = "value1" }, new { test = "value2" } })
                , new Mock<ILogger>().Object
                , topicClient.Object);

            Assert.IsType<OkResult>(actual);

        }
    }
}
