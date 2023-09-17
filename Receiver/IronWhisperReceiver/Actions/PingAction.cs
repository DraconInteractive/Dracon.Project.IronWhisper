﻿using IronWhisperReceiver.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    public class PingAction : CoreAction
    {
        public override bool Evaluate(TSpeech command)
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

        protected override async Task InternalRun(TSpeech command)
        {
            NetworkManager.Instance.PingNetworkAsync();
        }
    }
}
