using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    internal class DebugAction : CoreAction
    {
        public override CoreAction Init()
        {
            Name = "Debug";
            AlwaysRun = true;
            Phrases = Array.Empty<string>();
            return this;
        }

        protected override async Task InternalRun(TCommand command)
        {
            OutputMessage = command.Message;
        }
    }
}
