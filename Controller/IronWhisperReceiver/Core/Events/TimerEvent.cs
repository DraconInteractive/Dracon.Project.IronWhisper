using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Events
{
    public class TimerEvent : CoreEvent
    {
        public TimeSpan duration;
        public override async Task Consume()
        {
            string plural = duration.TotalMinutes > 0 ? "minutes" : "minute";
            Console.WriteLine($"Your {duration.TotalMinutes} {plural} timer is finished!");
        }
    }
}
