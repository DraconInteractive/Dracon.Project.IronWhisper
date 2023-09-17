using IronWhisperReceiver.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    public class CoreAction
    {
        public string Name;
        public string[] Phrases;
        protected string InternalMessage;
        protected string ExternalMessage;
        public bool AlwaysRun = false;
        public int Priority = 0;

        public CoreAction()
        {
            InternalInit();
            for (int i = 0; i < Phrases.Length; i++)
            {
                Phrases[i] = Phrases[i].ToLower().Trim();
            }
        }

        protected virtual void InternalInit ()
        {

        }

        public virtual bool Evaluate (TSpeech command)
        {
            return (Phrases.Contains(command.Command));
        }

        protected bool PhrasesContainsFull(TSpeech command)
        {
            return (Phrases.Contains(command.Command.ToLower()));
        }

        protected bool PhrasesContainsPartial(TSpeech command)
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

        public async Task Run(TSpeech command)
        {
            await InternalRun(command);
            if (Core.Verbosity >= 1)
            {
                InternalOutput();
            }
            ExternalOutput();
        }

        protected virtual async Task InternalRun(TSpeech command)
        {
            
        }

        protected void InternalOutput()
        {
            if (string.IsNullOrEmpty(InternalMessage)) { return; }

            Console.WriteLine($"[{Priority}] [{Name}] {InternalMessage}");
            Console.WriteLine();
        }

        protected void ExternalOutput()
        {
            if (string.IsNullOrEmpty(ExternalMessage)) { return; }

            Console.WriteLine($"[{Name}] {ExternalMessage}");
            Console.WriteLine();
        }
    }
}
