using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class HelpAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Help";
            Phrases = new string[]
            {
                "what do i say",
                "what can i say",
                "help"
            };
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            CoreSystem.Log("Actions Inputs:");
            foreach (var action in ActionManager.Instance.Actions)
            {
                CoreSystem.Log($"\t{action.HelpInformation()}");
            }
            ChangeState(State.Finished);
        }

        public override string HelpInformation()
        {
            return "[Help] \"What do I say?\", \"What can I say?\", \"Help!\"";
        }
    }
}
