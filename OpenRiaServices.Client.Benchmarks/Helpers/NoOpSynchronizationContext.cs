using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientBenchmarks.Helpers
{
    // Invoke everything synchronously
    class NoOpSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object state)
        {
            this.Send(d, state);
        }
    }
}
