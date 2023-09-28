using IronWhisperReceiver.Core.Networking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Core.Actions
{
    internal class ComplexAction : CoreAction
    {
        protected override void InternalInit()
        {
            Name = "Complex";
            Phrases = new string[] {"run a more complex action", "fetch my remote configuration file"};
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            Console.WriteLine(">> Connecting to server...");
            await Task.Delay(100);
            Console.WriteLine(">> Downloading file...");
            await Task.Delay(100);
            Console.WriteLine();
            InternalMessage = "File downloaded. Assigning file to external output";
            ExternalMessage = ComplexFile.GetTest();
        }
    }

    public class ComplexFile
    {
        public string ID;
        public string[] Attributes;
        public string keyURL;

        public static string GetTest()
        {
            var c = new ComplexFile()
            {
                ID = Guid.NewGuid().ToString(),
                Attributes = new string[] {
                    "ONLINE",
                    "STATUS_ACTIVE",
                    "RELAY_ACTIVE",
                    "TUNNEL_INACTIVE"
                },
                keyURL = "http://draconrepository.com"
            };
            return JsonConvert.SerializeObject(c, Formatting.Indented);
        }
    }
}
