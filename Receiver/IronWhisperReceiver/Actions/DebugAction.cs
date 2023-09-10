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
            AlwaysRun = true;
            Phrases = Array.Empty<string>();
        }

        protected override async Task InternalRun(TCommand command)
        {
            OutputMessage = command.Message;
        }
    }
}
