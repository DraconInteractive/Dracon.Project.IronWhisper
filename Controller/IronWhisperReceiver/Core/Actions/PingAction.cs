﻿using IronWhisper_CentralController.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class PingAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Ping";
            Phrases = new string[]
            {
                "ping the network",
                "get network devices",
                "find devices in the network"
            };
        }

        protected override async Task InternalRun(CoreSpeech command, CoreAction ctx = null)
        {
            NetworkManager.Instance.PingNetworkAsync();
        }
    }
}
