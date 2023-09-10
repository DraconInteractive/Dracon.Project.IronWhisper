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

        public CoreAction Init ()
        {
            InternalInit();
            for (int i = 0; i < Phrases.Length; i++)
            {
                Phrases[i] = Phrases[i].ToLower().Trim();
            }
            return this;
        }

        protected virtual void InternalInit ()
        {

        }

        public virtual bool Evaluate (TCommand command)
        {
            return (Phrases.Contains(command.Command));
        }

        protected bool PhrasesContainsFull(TCommand command)
        {
            return (Phrases.Contains(command.Command.ToLower()));
        }

        protected bool PhrasesContainsPartial(TCommand command)
        {
            bool match = false;
            foreach (var phrase in Phrases)
            {
                if (command.Message.ToLower().Contains(phrase))
                {
                    match = true;
                    break;
                }
            }
            return match;
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
