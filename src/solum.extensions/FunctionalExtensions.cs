using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.extensions
{
    public static class Timed
    {
        public static TimeSpan RunTimed(this Action action)
        {
            var timer = Stopwatch.StartNew();

            try
            {
                action();
            }
            finally
            {
                timer.Stop();
            }

            return timer.Elapsed;
        }

        public static async Task<TimeSpan> RunTimedAsync(this Func<Task> task)
        {
            var timer = Stopwatch.StartNew();

            try
            {
                await task();
            }
            finally
            {
                timer.Stop();
            }

            return timer.Elapsed;
        }
    }
}
