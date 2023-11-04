using IronWhisper_CentralController.Core.InputPipe;
using IronWhisper_CentralController.Core.Networking;
using IronWhisper_CentralController.Core.Networking.Sockets;
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
            Enabled = false;
        }
        public override string HelpInformation()
        {
            return "[TCP] \"Send a test command\"";
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            CoreSystem.Log("[TCP] Sending command");
            var device = Registry.RegistryManager.Instance.GetDevice("AP_HOME_MAIN");
            await SocketManager.Instance.SendTCP_Command(device, "test", x => CoreSystem.Log($"[TCP] Command result: {x}"));
            CoreSystem.Log("[TCP] Command complete");
            ChangeState(State.Finished);
        }
    }
}
