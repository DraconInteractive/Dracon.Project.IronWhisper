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

        public virtual bool Evaluate (TCommand command)
        {
            return (Phrases.Contains(command.Command));
        }

        public async Task Run(TCommand command)
        {
            await InternalRun(command);
            InternalOutput();
        }

        protected virtual async Task InternalRun(TCommand command)
        {
            
        }

        protected void InternalOutput()
        {
            Console.WriteLine($"[{Priority}] [{Name}] {OutputMessage}");
            Console.WriteLine();
        }
    }
}
