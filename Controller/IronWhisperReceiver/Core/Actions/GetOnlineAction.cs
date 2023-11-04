using IronWhisper_CentralController.Core.Audio.TTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
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
            var onlineDevices = Registry.RegistryManager.Instance.OnlineDevices();
            string output = $"{onlineDevices.Count()} online devices:\n";
            await TTSManager.ProcessTTS($"There are {onlineDevices.Count()} online devices.");
            
            foreach (var device in onlineDevices)
            {
                output += $"{device.DisplayName} : {device.networkDevice.Address}\n";
            }

            CoreSystem.Log("[GetOnline] " + output);
            ChangeState(State.Finished);
        }

        public override string HelpInformation()
        {
            return "[GetOnline] \"How many devices are online?\", \"Get online devices\", \"Find all online devices\"";
        }
    }
}
