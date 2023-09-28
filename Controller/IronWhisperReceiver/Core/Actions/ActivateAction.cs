using IronWhisperReceiver.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Core.Actions
{
    public class ActivateAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Activate";
            Phrases = new string[]
            {
                "come online",
                "wake up",
                "lets do this"
            };
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            InternalMessage = "";
            ExternalMessage = "Coming online...\nOnline and ready.";
        }
    }
}
