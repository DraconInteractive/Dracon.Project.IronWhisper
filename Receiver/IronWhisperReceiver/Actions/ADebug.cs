using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    internal class ADebug : CoreAction
    {
        public override CoreAction Init()
        {
            Name = "Debug";
            AlwaysRun = true;
            Phrases = Array.Empty<string>();
            return this;
        }

        protected override void InternalRun(string message, params object[] parameters)
        {
            OutputMessage = message;
        }
    }
}
