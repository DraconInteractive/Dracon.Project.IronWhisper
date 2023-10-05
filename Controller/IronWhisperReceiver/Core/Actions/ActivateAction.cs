using IronWhisper_CentralController.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
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
            CoreSystem.Log("[Boot] Coming online...");
            await Task.Delay(1500);
            CoreSystem.Log("[Boot] Online and ready.");
            ChangeState(State.Finished);
        }
    }
}
