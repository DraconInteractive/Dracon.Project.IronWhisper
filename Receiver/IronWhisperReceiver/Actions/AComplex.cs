using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    internal class AComplex : CoreAction
    {
        public override CoreAction Init()
        {
            Name = "Complex";
            AlwaysRun = false;
            Phrases = new string[] {"run a more complex action", "fetch my remote configuration file"};
            return this;
        }

        protected override void InternalRun(string message, params object[] parameters)
        {
            Console.WriteLine(">> Connecting to server...");
            Console.WriteLine(">> Downloading file...");
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
                ID = System.Guid.NewGuid().ToString(),
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
