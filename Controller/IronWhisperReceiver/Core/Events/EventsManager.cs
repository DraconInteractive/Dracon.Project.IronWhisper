using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Events
{
    public class EventsManager : CoreManager
    {
        public static EventsManager Instance;
        public Queue<CoreEvent> eventQueue;

        public EventsManager()
        {
            Instance = this;
            eventQueue = new Queue<CoreEvent>();
        }

        public bool EventsAvailable()
        {
            return eventQueue.Count > 0;
        }

        public CoreEvent DequeueEvent()
        {
            return eventQueue.Dequeue();
        }

        public void RegisterEvent(CoreEvent ev)
        {
            eventQueue.Enqueue(ev);
        }
    } 
}
