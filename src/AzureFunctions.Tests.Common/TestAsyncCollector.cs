using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureFunctions.Tests.Common
{
    public class TestAsyncCollector<T> : IAsyncCollector<T>
    {
        List<T> items = new List<T>();
        public IList<T> Items { get { return this.items; } }
        public Task AddAsync(T item, CancellationToken cancellationToken = default(CancellationToken))
        {
            items.Add(item);
            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }


    }
}
