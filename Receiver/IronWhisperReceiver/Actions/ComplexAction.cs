using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    internal class ComplexAction : CoreAction
    {
        public override CoreAction Init()
        {
            Name = "Complex";
            AlwaysRun = false;
            Phrases = new string[] {"run a more complex action", "fetch my remote configuration file"};
            return this;
        }

        protected override async Task InternalRun(TCommand command)
        {
            Console.WriteLine(">> Connecting to server...");
            await Task.Delay(100);
            Console.WriteLine(">> Downloading file...");
            await Task.Delay(100);
            Console.WriteLine(">> File downloaded. Assigning file to action output");
            Console.WriteLine();
            OutputMessage = ComplexFile.GetTest();
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
