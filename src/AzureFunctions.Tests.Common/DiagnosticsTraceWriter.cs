using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AzureFunctions.Tests.Common
{
    public class DiagnosticsTraceWriter : TraceWriter
    {
        public DiagnosticsTraceWriter() : this(TraceLevel.Verbose)
        {

        }

        public DiagnosticsTraceWriter(TraceLevel level) : base(level)
        {
        }

        public override void Trace(TraceEvent traceEvent)
        {
            System.Diagnostics.Debug.WriteLine(traceEvent.Message);
        }
    }
}
