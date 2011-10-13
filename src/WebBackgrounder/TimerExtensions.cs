using System;
using System.Threading;

namespace WebBackgrounder
{
    public static class TimerExtensions
    {
        public static void Stop(this Timer timer)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public static void Next(this Timer timer, TimeSpan dueTime)
        {
            timer.Change(dueTime, TimeSpan.FromMilliseconds(Timeout.Infinite));
        }
    }
}
