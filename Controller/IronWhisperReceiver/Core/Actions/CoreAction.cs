using IronWhisper_CentralController.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class CoreAction
    {
        public string Name;
        public string[] Phrases;
        protected string InternalMessage;
        protected string ExternalMessage;
        public bool AlwaysRun = false;
        public int Priority = 0;
        public List<Type> ContextOptions;

        public CoreAction()
        {
            ContextOptions = new List<Type>();
            InternalInit();
            for (int i = 0; i < Phrases.Length; i++)
            {
                Phrases[i] = Phrases[i].ToLower().Trim();
            }
        }

        protected virtual void InternalInit ()
        {

        }

        public virtual bool Evaluate (CoreSpeech command)
        {
            return (Phrases.Contains(command.Command));
        }

        protected bool PhrasesContainsFull(CoreSpeech command)
        {
            return (Phrases.Contains(command.Command.ToLower()));
        }

        protected bool PhrasesContainsPartial(CoreSpeech command)
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

        public async Task Run(CoreSpeech command, CoreAction ctx = null)
        {
            await InternalRun(command, ctx);
        }

        protected virtual async Task InternalRun(CoreSpeech command, CoreAction ctx)
        {
            
        }
    }
}
