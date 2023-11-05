using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class CompoundAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Compound";
            Phrases = new string[]
            {
                "try a compound action"
            };
            Enabled = false;
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            // There is no way to get new speech here, so we have to change the foundation...
            CoreSystem.Log("[Compound] Starting compound action...");
            CoreSystem.Log("[Compound] Please say something , that I will intercept!");
            ChangeState(State.WaitingForInput);
        }

        bool firstRetry = true;
        protected override Task InternalRunAgain(CoreSpeech command)
        {
            if (firstRetry)
            {
                CoreSystem.Log($"[Compound] \'{command.Message}\' huh? Interesting...");
                firstRetry = false;
                ChangeState(State.WaitingForInput);
                return Task.CompletedTask;
            }

            if (command.Command == "goodbye")
            {
                CoreSystem.Log($"It was nice talking to you!");
                ChangeState(State.Finished);
            }
            else
            {
                CoreSystem.Log($"[Compound] I see you said \'{command.Command}\', cool!\nYou can say 'goodbye' if you want to end this interaction :)", command.Message, ConsoleColor.Gray);
                ChangeState(State.WaitingForInput);
            }
            return Task.CompletedTask;
        }

        public override string HelpInformation()
        {
            return "[Compound] \"Try a compound action\"";
        }
    }
}
