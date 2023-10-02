using IronWhisper_CentralController.Core.InputPipe;
using IronWhisper_CentralController.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class TestTCPCommandAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "TCPCommand";
            Phrases = new string[]
            {
                "send a test command"
            };
        }

        protected override async Task InternalRun(CoreSpeech command, CoreAction ctx = null)
        {
            CoreSystem.Log("Sending test command");
            var device = Registry.RegistryCore.Instance.GetDevice("AP_HOME_MAIN");
            await TCPSender.Instance.SendCommandAsync(device, "Test", x => CoreSystem.Log($"[TCP] Command result: {x}"));
            CoreSystem.Log("Test command complete");
        }
    }
}
