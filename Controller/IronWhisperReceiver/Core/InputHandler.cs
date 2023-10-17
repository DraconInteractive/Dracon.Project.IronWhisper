using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core
{
    public class InputHandler : CoreManager
    {
        public static InputHandler Instance;

        private const string queryPrompt = "Okay critter";

        public Action<CoreSpeech> onInputReceived;
        public CoreSpeech lastInputReceived;

        private static bool overrideInput;
        private static bool overridenInputAvailable;
        public InputHandler()
        {
            Instance = this;
        }

        public void RegisterInput(string message, string source)
        {
            var speech = new CoreSpeech(queryPrompt, message, source);
            lastInputReceived = speech;
            if (overrideInput)
            {
                overridenInputAvailable = true;
            }
            onInputReceived?.Invoke(speech);
        }

        public static async Task<string> GetInput(string prompt, Predicate<string> validation, int retries = 0, string retryMessage = "")
        {
            overrideInput = true;

            int currentRetries = 0;
            string input = "";
            if (retryMessage == "")
            {
                retryMessage = "Invalid input. Please try again";
            }

            while (currentRetries <= retries)
            {
                CoreSystem.Log(prompt);
                while (!overridenInputAvailable)
                {
                    await Task.Delay(10);
                }
                overridenInputAvailable = false;
                input = Instance.lastInputReceived.Command;
                if (validation(input))
                {
                    return input;
                }
                else
                {
                    CoreSystem.Log(retryMessage);
                }
                if (retries > 0)
                {
                    currentRetries++;
                }
            }
            overrideInput = false;
            return input;
        }
    }
}
