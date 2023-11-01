using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Registry
{
    public class RegConfig : RegCore
    {
        // Must include extension (aka reg.json, not just reg)
        public string fileName;

        private string FilePath => Path.Combine(RegistryManager.FolderPath, "Config/" + fileName);
        public RegConfig ()
        {
            fileName = "";
        }

        public async Task<bool> Validate ()
        {
            if (!Directory.Exists(Path.Combine(RegistryManager.FolderPath, "Config" )))
            {
                CoreSystem.Log("Config folder doesnt exist.");
                return false;
            }

            if (!File.Exists(FilePath))
            {
                bool confirmation = await InputHandler.GetConfirmationInput($"Config file \"{fileName}\" doesnt exist. Create?");
                if (confirmation)
                {
                    File.Create(FilePath);
                }    
                else
                {
                    CoreSystem.LogError($"No config file at {FilePath}");
                    return false;
                }
            }
            return true;
        }

        public string Load()
        {
            string filePath = FilePath;
            string contents = File.ReadAllText(filePath);
            return contents;
        }

        public T? Load<T>()
        {
            var contents = Load();
            if (contents == null)
            {
                return default;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(contents);
            }
        }

        public object? Load(Type type)
        {
            var contents = Load();
            if (contents == null)
            {
                return default;
            }
            else
            {
                return JsonConvert.DeserializeObject(contents, type);
            }
        }
    }
}
