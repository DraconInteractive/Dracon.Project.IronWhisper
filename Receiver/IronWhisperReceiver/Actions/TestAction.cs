using IronWhisperReceiver.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    internal class TestAction : CoreAction
    {
        protected override void InternalInit()
        {
            Name = "Test";
            Phrases = new string[]
            {
                "do a test",
                "run a test",
                ", do a test",
                ", run a test"
            };
        }

        protected override async Task InternalRun(TSpeech command)
        {
            InternalMessage = "Running a test";
            ExternalMessage = "Test!";
        }
    }
}
