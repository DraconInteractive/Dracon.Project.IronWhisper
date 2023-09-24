using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Core.Actions
{
    public class GetOnlineAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Get Online";
            Phrases = new string[]
            {
                "how many devices are online",
                "get online devices",
                "find all online devices"
            };
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            var onlineDevices = Registry.RegistryCore.Instance.OnlineDevices();
            string output = $"{onlineDevices.Count()} online devices:\n";
            foreach (var device in onlineDevices)
            {
                output += $"{device.DisplayName} : {device.networkDevice.Address}\n";
            }

            ExternalMessage = output;
        }
    }
}
