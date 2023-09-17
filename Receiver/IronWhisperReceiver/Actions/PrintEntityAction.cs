﻿using IronWhisperReceiver.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    public class PrintEntityAction : CoreAction
    {
        public override bool Evaluate(TSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Print Entity";
            Phrases = new string[]
            {
                "give me information on",
                "give me information about",
                "tell me about",
                "print out info on"
            };
        }

        protected override async Task InternalRun(TSpeech command)
        {
            if (command.Entities == null || command.Entities.Length == 0)
            {
                InternalMessage = "";
                ExternalMessage = "Sorry, I dont have information on that";
            }
            else
            {
                string output = "Here is the information you requested: \n";
                foreach (var e in command.Entities)
                {
                   output += $"{e.DisplayName}  --  {e.ID}\nTags:\n";

                    foreach (var t in e.Tags)
                    {
                        output += $"  --  {t}\n";
                    }

                    output += "Capabilities:\n";

                    foreach (var c in e.Capabilities)
                    {
                        output += $"  --  {c}\n";
                    }
                    output += "\n";
                }

                ExternalMessage = output;
                InternalMessage = "";
            }
        }
    }
}
