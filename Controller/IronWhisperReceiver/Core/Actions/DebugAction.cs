using IronWhisper_CentralController.Core.Networking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    internal class DebugAction : CoreAction
    {
        protected override void InternalInit()
        {
            Name = "Debug";
            Phrases = Array.Empty<string>();
            AlwaysRun = true;
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            string verboseOutput = "Command Data:\n\n"
                + $"{JsonConvert.SerializeObject(command, Formatting.Indented)}\n\n";

            switch (CoreSystem.Verbosity)
            {
                case 2:
                    InternalMessage = verboseOutput;
                    break;
            }
            ExternalMessage = ">> " + command.Message;
        }
    }
}
