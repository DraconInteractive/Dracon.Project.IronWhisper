using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Registry
{
    public class RegCore
    {
        public string ID;
        public string DisplayName;
        public List<string> SpeechTags;
        public List<string> Tags = new List<string>();
        public List<string> Capabilities = new List<string>();
        public List<RegModuleCore> Modules = new List<RegModuleCore>();

        public RegCore()
        {
            ID = "No ID";
            DisplayName = "____";
            SpeechTags = new List<string>();
            Tags = new List<string>();
            Capabilities = new List<string>();
            Modules = new List<RegModuleCore>();
        }

        public bool SpeechMatch (string speech)
        {
            if (SpeechTags == null || SpeechTags.Count == 0)
            {
                Console.WriteLine($"Entity {DisplayName} has null or empty speech tags");
                return false;
            }

            foreach (var tag in SpeechTags)
            {
                if (speech.Contains(tag))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
