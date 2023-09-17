using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Registry
{
    public class Registry
    {
        public static Registry Instance;
        public List<RegAccessPoint> AccessPoints = new List<RegAccessPoint>();
        public List<RegTerminal> Terminals = new List<RegTerminal>();
        public List<RegProject> Projects = new List<RegProject>();

        private static string folderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IronWhisper");
        private static string filePath => Path.Combine(folderPath, "registry.json");

        public Registry ()
        {
            Instance = this;
        }

        public static Registry CreateDefault()
        {
            Console.WriteLine("Creating default registry");
            Registry registry = new();
            RegTerminal mainTerminal = new()
            {
                ID = Guid.NewGuid().ToString(),
                DisplayName = "Terminal - Main",
                Tags = new List<string>()
                {
                    "Home",
                    "WSL"
                },
                Capabilities = new List<string>()
                {
                    "Speech",
                    "Networked",
                    "Spacy"
                },
                SpeechTags = new List<string>() { "home terminal" },
                //deviceID = "TERMINAL_HOME_MAIN"
                deviceID = "DeviceID_1"
            };

            RegAccessPoint mainHomeAP = new ()
            {
                ID = Guid.NewGuid().ToString(),
                DisplayName = "Home PC - Main",
                Tags = new List<string>()
                {
                    "Home",
                    "WSL",
                    "PC"
                },
                SpeechTags = new List<string>() 
                { 
                    "home computer", 
                    "home pc", 
                    "my computer", 
                    "my pc", 
                    "main computer", 
                    "main pc" 
                },
                Capabilities = new List<string>()
                {
                    "Windows",
                    "Unity",
                    "Networked",
                    "Camera",
                    "Speaker"
                },
                Owner = "Peter Carey",
                deviceID = "AP_HOME_MAIN"
            };

            RegAccessPoint homeServerAP = new ()
            {
                ID = Guid.NewGuid().ToString(),
                DisplayName = "Home PC - Server",
                Tags = new List<string>()
                {
                    "Home",
                    "Workstation",
                    "ExternalControl",
                    "PC"
                },
                SpeechTags = new List<string>() 
                { 
                    "home server", 
                    "secondary pc", 
                    "work computer", 
                    "work pc" 
                },
                Capabilities = new List<string>()
                {
                    "Windows",
                    "Unity",
                    "Networked",
                    "REST",
                    "Server",
                    "Speaker"
                },
                Owner = "Peter Carey",
                deviceID = "AP_HOME_SECONDARY"
            };

            RegAccessPoint homeLaptopAP = new ()
            {
                ID = Guid.NewGuid().ToString(),
                DisplayName = "Home PC - Laptop",
                Tags = new List<string>()
                {
                    "Home",
                    "Workstation",
                    "Laptop"
                },
                SpeechTags = new List<string>() 
                { 
                    "home laptop", 
                    "my laptop" 
                },
                Capabilities = new List<string>()
                {
                    "Portable",
                    "Windows",
                    "Unity",
                    "Networked",
                    "Speaker"
                },
                Owner = "Peter Carey",
                deviceID = "AP_HOME_LAPTOP"
            };

            RegAccessPoint homeTabletAP = new ()
            {
                ID = Guid.NewGuid().ToString(),
                DisplayName = "Home Tablet - S8 Ultra",
                Tags = new List<string>()
                {
                    "Home",
                    "Mobile",
                    "Tablet"
                },
                SpeechTags = new List<string>() 
                { 
                    "home tablet", 
                    "my tablet", 
                    "the tablet" 
                },
                Capabilities = new List<string>()
                {
                    "Portable",
                    "Android",
                    "Touchscreen",
                    "Networked",
                    "Speaker"
                },
                Owner = "Peter Carey",
                deviceID = "AP_HOME_TABLET"
            };

            RegAccessPoint mobileAP = new ()
            {
                ID = Guid.NewGuid().ToString(),
                DisplayName = "Mobile Phone - S22 Ultra",
                Tags = new List<string>()
                {
                    "Home",
                    "Mobile",
                    "Phone"
                },
                SpeechTags = new List<string>() 
                { 
                    "my mobile", 
                    "my phone", 
                    "the phone" 
                },
                Capabilities = new List<string>()
                {
                    "Portable",
                    "Android",
                    "Touchscreen",
                    "Networked",
                    "Speech",
                    "Speaker"
                },
                Owner = "Peter Carey",
                deviceID = "AP_HOME_MOBILE"
            };

            RegProject projectFIFA = new ()
            {
                ID = Guid.NewGuid().ToString(),
                DisplayName = "FIFA AI League",
                Description = "FutureVerse / FIFA collaboration on an ML soccer-manager game.",
                isGitRepository = true,
                Path = "C:/Users/pmc10/Documents/afif-prototype/",
                Tags = new List<string>()
                {
                    "FV",
                    "ML"
                },
                Capabilities = new List<string>()
                {
                    "Windows",
                    "Unity",
                    "Android",
                    "Linux",
                    "iOS"
                },
                SpeechTags = new List<string>() 
                { 
                    "ai league", 
                    "fifa" 
                }
            };

            RegProject projectIronWhisper = new RegProject()
            {
                ID = Guid.NewGuid().ToString(),
                DisplayName = "Iron Whisper",
                Description = "AI Assistant, full pipeline from command input to action execution and subsequent feedback to the user",
                isGitRepository = true,
                Notes = new List<string>(),
                Path = "C:/Users/pmc10/Documents/IronWhisper/",
                Tags = new List<string>()
                {
                    "Personal",
                    "AI",
                    "C++",
                    "C#"
                },
                Capabilities = new List<string>()
                {
                    "Windows",
                    "Linux",
                    "Speech"
                },
                Files = new List<RegFile>(),
                Modules = new List<RegModuleCore>(),
                SpeechTags = new List<string>() { "iron whisper" }
            };

            RegModuleCore unityCoreModule = new RegModuleCore()
            {

            };

            projectFIFA.Modules.Add(unityCoreModule);

            registry.Projects.Add(projectFIFA);
            registry.Projects.Add(projectIronWhisper);

            registry.AccessPoints.Add(mainHomeAP);
            registry.AccessPoints.Add(homeServerAP);
            registry.AccessPoints.Add(homeLaptopAP);
            registry.AccessPoints.Add(homeTabletAP);
            registry.AccessPoints.Add(mobileAP);

            mainTerminal.AccessPoints.AddRange(registry.AccessPoints.Select(x => x.ID));

            mainHomeAP.Projects.Add(projectFIFA.ID);
            mainHomeAP.Projects.Add(projectIronWhisper.ID);

            homeServerAP.Projects.Add(projectFIFA.ID);

            registry.Terminals.Add(mainTerminal);
            return registry;
        }

        public void Save ()
        {
            ValidateFile();
            string data = JsonConvert.SerializeObject(this, Formatting.Indented);
            Core.Log("Saving registry to: " + filePath, 1);
            File.WriteAllText(filePath, data);
        }

        public Registry Load ()
        {
            if (ValidateFile())
            {
                string data = File.ReadAllText(filePath);
                var reg = JsonConvert.DeserializeObject<Registry>(data);
                AccessPoints = reg.AccessPoints;
                Terminals = reg.Terminals;
                Projects = reg.Projects;
            }
            else
            {
                Core.Log("No existing registry data to load, creating default", 1);
                var def = CreateDefault();
                AccessPoints = def.AccessPoints;
                Terminals = def.Terminals;
                Projects = def.Projects;
            }

            Core.Log($"Registry loaded.\nTerminals:\t{Terminals.Count}\nAccess Points:\t{AccessPoints.Count}\nProjects:\t{Projects.Count}\n");
            Core.Log(JsonConvert.SerializeObject(this, Formatting.Indented), 2);
            Core.Log(2);

            return this;
        }

        private static bool ValidateFile ()
        {
            var path = folderPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return false;
            }
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "{}");
                return false;
            }
            return true;
        }

        public List<RegCore> ParseEntities (string text)
        {
            var detected = new List<RegCore>();

            foreach (var ent in Projects)
            {
                if (ent.SpeechMatch(text))
                {
                    detected.Add(ent);
                }
            }

            foreach (var ent in AccessPoints)
            {
                if (ent.SpeechMatch(text))
                {
                    detected.Add(ent);
                }
            }

            foreach (var ent in Terminals)
            {
                if (ent.SpeechMatch(text))
                {
                    detected.Add(ent);
                }
            }
            Core.Log($"Detected {detected.Count} entitites. ", 1);
            return detected;
        }

        public void UpdateNetworkDevice(string deviceID, string remoteAddress)
        {
            // TODO Add 'last packet received' to RegDevice, as well as 'Online => (DateTime.Now - LastPacketReceived).TotalMinutes <= 5'
            foreach (var device in AccessPoints)
            {
                if (device.deviceID == deviceID)
                {
                    var details = Networking.NetworkManager.GetDeviceDetails(remoteAddress);
                    Console.WriteLine("Retrieved information on registered device: ");
                    Console.WriteLine(JsonConvert.SerializeObject(details, Formatting.Indented));
                }
            }

            foreach (var device in Terminals)
            {
                if (device.deviceID == deviceID)
                {
                    var details = Networking.NetworkManager.GetDeviceDetails(remoteAddress);
                    Console.WriteLine("Retrieved information on registered device: ");
                    Console.WriteLine(JsonConvert.SerializeObject(details, Formatting.Indented));
                }
            }
        }
    }
}
