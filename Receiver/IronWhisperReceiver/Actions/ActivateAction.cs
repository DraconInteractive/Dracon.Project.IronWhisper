﻿using IronWhisperReceiver.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    public class ActivateAction : CoreAction
    {
        public override bool Evaluate(TSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Activate";
            Phrases = new string[]
            {
                "come online",
                "wake up",
                "lets do this"
            };
        }

        protected override async Task InternalRun(TSpeech command)
        {
            InternalMessage = "";
            ExternalMessage = "Coming online...\nOnline and ready.";
        }
    }
}
