using IronWhisper_CentralController.Core.Networking;
using IronWhisper_CentralController.Core.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class PrintAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Print Entity";
            Phrases = new string[]
            {
                "information on",
                "information about",
                "tell me about",
                "print out",
                "details on"
            };
        }

        public override string HelpInformation()
        {
            return "[Print] \"Find information...\", \"Tell me about...\", \"Get details on...\". You can ask for information about 'an entity' or 'the registry' if needed.";
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            if (command.Entities == null || command.Entities.Length == 0)
            {
                var reg = RegistryCore.Instance;
                if (command.Command.Contains("an entity"))
                {
                    string name = await InputHandler.GetInput("[Print] Enter entity ID: ", x => reg.GetEntity(x) != null, 1, "[Print] I can't find an entity by that ID. Try once again");
                    var ent = reg.GetEntity(name);
                    if (ent == null)
                    {
                        CoreSystem.Log("[Print] Sorry, I don't have information on that...");
                    }
                    else
                    {
                        CoreSystem.Log("[Print] Information located:\n");
                        PrintEntity(ent);
                    }
                }
                else if (command.Command.Contains("the registry"))
                {
                    string[] options = new string[] { "y", "n", "yes", "no" };
                    string confirmation = await InputHandler.GetInput("[Print] Show registry information? [y/n]", x => options.Contains(x));
                    if (confirmation == "y" || confirmation == "yes")
                    {
                        // print registry
                        PrintRegistry();
                    }
                    else
                    {
                        CoreSystem.Log("[Print] Okay, aborting print");
                    }
                }
                else
                {
                    CoreSystem.Log("[Print] Sorry, I don't have information on that...");
                }
            }
            else
            {
                CoreSystem.Log("[Print] Here is the information you requested: ");
                foreach (var e in command.Entities)
                {
                    PrintEntity(e);
                }
            }
            ChangeState(State.Finished);
        }

        private void PrintEntity (RegCore e)
        {
            // if regdevice, output ip
            CoreSystem.Log($"{e.DisplayName} --  {e.ID}", e.DisplayName, ConsoleColor.Yellow);
            if (e is RegDevice d)
            {
                CoreSystem.Log($" >> {d.networkDevice.Address}");
            }
            CoreSystem.Log("Tags:");

            foreach (var t in e.Tags)
            {
                CoreSystem.Log($"  --  {t}");
            }
            CoreSystem.Log("Capabilities:");

            foreach (var c in e.Capabilities)
            {
                CoreSystem.Log($"  --  {c}");
            }

            CoreSystem.Log();
        }

        private void PrintRegistry ()
        {
            var reg = RegistryCore.Instance;
            CoreSystem.Log("[Print] Heres the high level information. Search for a specific entity or device for more detail.\n");
            CoreSystem.Log($"Terminals:\t{reg.Terminals.Count}\nAccess Points:\t{reg.AccessPoints.Count}\nProjects:\t{reg.Projects.Count}\nOnline Devices:\t{reg.OnlineDevices().Count}");
            CoreSystem.Log("\nAccess Points:");
            foreach (var ap in reg.AccessPoints)
            {
                CoreSystem.Log($"\t{ap.DisplayName.PadRight(30)}{ap.SpeechTags[0].PadRight(20)}{ap.ID}");
            }
            CoreSystem.Log("\nTerminals:");
            foreach (var term in reg.Terminals)
            {
                CoreSystem.Log($"\t{term.DisplayName.PadRight(30)}{term.SpeechTags[0].PadRight(20)}{term.ID}");
            }
            CoreSystem.Log("\nProjects:");
            foreach (var proj in reg.Projects)
            {
                CoreSystem.Log($"\t{proj.DisplayName.PadRight(30)}{proj.SpeechTags[0].PadRight(20)}{proj.ID}");
            }
            CoreSystem.Log();
        }
    }
}
