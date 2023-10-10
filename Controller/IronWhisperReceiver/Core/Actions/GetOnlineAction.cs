﻿using IronWhisper_CentralController.Core.Audio.TTS;
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
            var onlineDevices = Registry.RegistryCore.Instance.OnlineDevices();
            string output = $"{onlineDevices.Count()} online devices:\n";
            await TTSManager.Instance.ProcessTTS($"There are {onlineDevices.Count()} online devices.");
            
            foreach (var device in onlineDevices)
            {
                output += $"{device.DisplayName} : {device.networkDevice.Address}\n";
            }

            CoreSystem.Log("[GetOnline] " + output);
            ChangeState(State.Finished);
        }
    }
}
