﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver
{
    public class EventsManager
    {
        public static EventsManager Instance;
        public Queue<CoreEvent> eventQueue;

        public EventsManager()
        {
            Instance = this;
            eventQueue = new Queue<CoreEvent>();
        }

        public bool EventsAvailable ()
        {
            return eventQueue.Count > 0;
        }

        public CoreEvent DequeueEvent ()
        {
            return eventQueue.Dequeue();
        }

        public void RegisterEvent (CoreEvent ev)
        {
           eventQueue.Enqueue(ev);
        }
    }

    public class CoreEvent
    {
        public virtual async Task Consume ()
        {

        }
    }

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
