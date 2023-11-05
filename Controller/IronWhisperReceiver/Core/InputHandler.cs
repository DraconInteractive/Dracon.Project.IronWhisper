using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core
{
    public class InputHandler : CoreManager
    {
        public enum Mode
        {
            ManualStandard,
            ManualCustom
        }

        public static InputHandler Instance;

        private const string queryPrompt = "okay david";

        public Action<CoreSpeech> onInputReceived;

        public Mode mode;

        public string customInput;

        public InputHandler()
        {
            Instance = this;
            mode = Mode.ManualStandard;

            if (CoreSystem.Config.useManualInput)
            {
                ManualInputLoop();
            }
        }

        public async Task ManualInputLoop ()
        {
            while (true)
            {
                string input = await ReadLineAsync();

                if (mode == Mode.ManualStandard)
                {
                    RegisterStandardInput(input, "Console");
                }
                else if (mode == Mode.ManualCustom)
                {
                    customInput = input;
                }
                await Task.Delay(100);
            }
        }

        public void RegisterStandardInput(string message, string source)
        {
            var speech = new CoreSpeech(queryPrompt, message, source);
            onInputReceived?.Invoke(speech);
        }

        static async Task<string> ReadLineAsync()
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    CoreSystem.Log(">> ", writeLine: false);
                    return Console.ReadLine() ?? "";
                }
                await Task.Delay(50);
            }
        }

        public static async Task<string> WaitForCustomInput ()
        {
            Instance.customInput = "";
            while (string.IsNullOrEmpty(Instance.customInput))
            {
                await Task.Delay(50);
            }
            return Instance.customInput;
        }

        public static async Task<string> GetInput(string prompt, Predicate<string> validation, int retries = 0, string retryMessage = "")
        {
            CoreSystem.Log("Starting GetInput");
            Instance.mode = Mode.ManualCustom;
            int currentRetries = 0;
            string input = "";
            if (retryMessage == "")
            {
                retryMessage = "Invalid input. Please try again";
            }
            CoreSystem.Log(prompt);
            while (currentRetries <= retries)
            {
                input = await WaitForCustomInput();
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
            Instance.mode = Mode.ManualStandard;
            return input;
        }

        public static async Task<bool> GetConfirmationInput(string prompt, int retries = 0, string retryMessage = "")
        {
            string[] options = new string[] { "y", "n", "yes", "no" };
            string confirmation = await GetInput(prompt + " [y/n]", x => options.Contains(x), retries, retryMessage);
            return (confirmation == "y" || confirmation == "yes");
        }
    }
}
