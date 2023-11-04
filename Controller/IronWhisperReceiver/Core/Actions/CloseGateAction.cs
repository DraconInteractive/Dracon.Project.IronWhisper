using IronWhisper_CentralController.Core.Audio.TTS;
using IronWhisper_CentralController.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class CloseGateAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            CoreSystem.Log("CGA: " + command.Message + " | " + command.Command + " | " + command.ContainsPrompt + " | " + ActionManager.Instance.Gated + " | " + UseGate);
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Close Gate";
            Phrases = new string[]
            {
                "stop listening",
                "thats all"
            };
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            CoreSystem.Log("[Gate] Okay, I'll wait for you to ask for me by name");
            CoreSystem.Log("Gate Closed", ConsoleColor.Red);
            await TTSManager.PlayAudio("Labs_Action_CloseGate.mp3");
            ActionManager.Instance.CloseGate();
            ChangeState(State.Finished);
        }

        public override string HelpInformation()
        {
            return "[Open Gate] \"Keep listening\", \"Listen to me\"";
        }
    }
}
