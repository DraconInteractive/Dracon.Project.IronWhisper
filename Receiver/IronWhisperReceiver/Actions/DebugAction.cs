using IronWhisperReceiver.Networking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    internal class DebugAction : CoreAction
    {
        protected override void InternalInit()
        {
            Name = "Debug";
            Phrases = Array.Empty<string>();
            AlwaysRun = true;
        }

        protected override async Task InternalRun(TSpeech command)
        {
            string verboseOutput = "Command Data:\n\n"
                + $"{JsonConvert.SerializeObject(command, Formatting.Indented)}\n\n";

            switch (Core.Verbosity)
            {
                case 2:
                    InternalMessage = verboseOutput;
                    break;
            }
            ExternalMessage = ">> " + command.Message;
        }
    }
}
