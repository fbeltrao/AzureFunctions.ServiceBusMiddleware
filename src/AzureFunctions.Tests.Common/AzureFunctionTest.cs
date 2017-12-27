using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureFunctions.Tests.Common
{
    public class AzureFunctionTest
    {
        public HttpRequest CreateJsonRequest(string json, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(encoding.GetBytes(json));
            return httpContext.Request;
        }
        public HttpRequest CreateJsonRequest(object json, Encoding encoding = null)
        {
            var jsonString = JsonConvert.SerializeObject(json);
            return CreateJsonRequest(jsonString, encoding);
        }

        public HttpRequest CreateEmptyRequest()
        {
            var httpContext = new DefaultHttpContext();
            return httpContext.Request;
        }

        public TraceWriter CreateTraceWriter()
        {

            return new DiagnosticsTraceWriter();
        }

    }
}
