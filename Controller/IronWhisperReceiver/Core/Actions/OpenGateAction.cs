using IronWhisper_CentralController.Core.Audio.TTS;
using IronWhisper_CentralController.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class OpenGateAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Open Gate";
            Phrases = new string[]
            {
                "keep listening",
                "listen to me",
                "start listening"
            };
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            CoreSystem.Log("[Gate] Okay, I'm here to help");
            CoreSystem.Log("Gate Opened", ConsoleColor.Green);
            await TTSManager.PlayAudio("Labs_Action_OpenGate.mp3");
            ActionManager.Instance.OpenGate();
            ChangeState(State.Finished);
        }

        public override string HelpInformation()
        {
            return "[Open Gate] \"Keep listening\", \"Listen to me\"";
        }
    }
}
