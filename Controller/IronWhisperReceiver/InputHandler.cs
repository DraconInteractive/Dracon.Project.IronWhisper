using IronWhisper_CentralController.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController
{
    public class InputHandler
    {
        public static InputHandler Instance;

        private const string queryPrompt = "Okay critter";

        public Action<CoreSpeech> onInputReceived;
        public CoreSpeech lastInputReceived;

        public InputHandler()
        {
            Instance = this;
        }

        public void RegisterInput (string message)
        {
            var speech = new CoreSpeech(queryPrompt, message);
            lastInputReceived = speech;
            onInputReceived?.Invoke(speech);
        }
    }
}
