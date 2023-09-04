using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    internal class CoreAction
    {
        public string Name;
        public string[] Phrases;
        protected string OutputMessage;
        public bool AlwaysRun;
        public int Priority = 0;

        public virtual CoreAction Init ()
        {
            return this;
        }

        public void Run(string message, params object[] parameters)
        {
            InternalRun(message, parameters);
            InternalOutput();
        }

        protected virtual void InternalRun(string message, params object[] parameters)
        {
            
        }

        protected void InternalOutput()
        {
            Console.WriteLine($"[{Priority}] [{Name}] {OutputMessage}");
            Console.WriteLine();
        }
    }
}
