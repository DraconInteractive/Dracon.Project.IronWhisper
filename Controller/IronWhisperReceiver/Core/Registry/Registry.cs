using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Registry
{
    public class RegistryCore : CoreManager
    {
        public static RegistryCore Instance;
        public List<RegAccessPoint> AccessPoints = new();
        public List<RegTerminal> Terminals = new();
        public List<RegProject> Projects = new();

        private static string FolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IronWhisper");
        private static string FilePath => Path.Combine(FolderPath, "registry.json");

        public RegistryCore()
        {
            Instance = this;
        }

        public static RegistryCore CreateDefault()
        {
            CoreSystem.Log("[Registry] Creating default", 1);
            RegistryCore registry = new();
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
                deviceID = "TERMINAL_HOME_MAIN"
            };

            RegAccessPoint mainHomeAP = new()
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

            RegAccessPoint homeServerAP = new()
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

            RegAccessPoint homeLaptopAP = new()
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

            RegAccessPoint homeTabletAP = new()
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

            RegAccessPoint mobileAP = new()
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

            RegProject projectFIFA = new()
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

        public void Save()
        {
            ValidateFile();
            string data = JsonConvert.SerializeObject(this, Formatting.Indented);
            CoreSystem.Log("[Registry] Saving to: " + FilePath, 2);
            File.WriteAllText(FilePath, data);
        }

        public RegistryCore Load()
        {
            if (ValidateFile())
            {
                string data = File.ReadAllText(FilePath);
                var reg = JsonConvert.DeserializeObject<RegistryCore>(data);
                AccessPoints = reg.AccessPoints;
                Terminals = reg.Terminals;
                Projects = reg.Projects;
            }
            else
            {
                var def = CreateDefault();
                AccessPoints = def.AccessPoints;
                Terminals = def.Terminals;
                Projects = def.Projects;
            }
            return this;
        }

        public void Debug_PrintLoad ()
        {
            if (ValidateFile())
            {
                CoreSystem.Log($"[Registry] Loading (non-alloc) from: {FilePath}", 1);
                string data = File.ReadAllText(FilePath);
                Console.WriteLine(data);
            }
        }

        private static bool ValidateFile()
        {
            var path = FolderPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return false;
            }
            if (!File.Exists(FilePath))
            {
                File.WriteAllText(FilePath, "{}");
                return false;
            }
            return true;
        }

        public List<RegCore> ParseEntities(string text)
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
            CoreSystem.Log($"[Reg] Detected {detected.Count} entitites. ", 2);
            return detected;
        }

        public List<RegCore> AllEntities ()
        {
            List<RegCore> entities = new();
            entities.AddRange(Terminals);
            entities.AddRange(AccessPoints);
            entities.AddRange(Projects);
            return entities;

        }
        public List<RegDevice> AllDevices()
        {
            List<RegDevice> devices = new();
            foreach (var device in AccessPoints)
            {
                devices.Add(device);
            }
            foreach (var device in Terminals)
            {
                devices.Add(device);
            }
            return devices;
        }

        public List<RegDevice> OnlineDevices ()
        {
            return AllDevices().Where(x => x.networkDevice.Online).ToList();
        }

        public bool UpdateNetworkDevice(string deviceID, string remoteAddress)
        {
            foreach (var device in AllDevices())
            {
                if (device.deviceID == deviceID)
                {
                    if (!device.networkDevice.Online)
                    {
                        CoreSystem.Log($"[UDP_ID] {deviceID} has come online", 1);
                    }
                    CoreSystem.Log($"[UDP_ID] [Registry] {deviceID} identified. Retrieving update.", 2);
                    var hostname = Networking.NetworkUtilities.GetHostName(remoteAddress);
                    device.networkDevice.UpdateDetails(new NetworkDevice () { Address = remoteAddress, HostName = hostname});
                    CoreSystem.Log($"[UDP_ID] [Registry] {deviceID} update complete.", 2);
                    Save();
                    return true;
                }
            }

            return false;
        }

        public bool UpdateNetworkDevice(string deviceID, IPAddress remoteAddress)
        {
            return UpdateNetworkDevice(deviceID, remoteAddress.ToString());
        }

        public bool IsRegisteredNetworkDevice(string ip = "", string hostname = "", string deviceID = "")
        {
            bool match = AllDevices().Any(x => x.networkDevice.Address == ip || x.networkDevice.HostName == hostname || x.deviceID == deviceID);
            return match;
        }

        public RegDevice? GetDevice(string deviceID)
        {
            return AllDevices().FirstOrDefault(x => x.deviceID == deviceID);
        }

        public RegCore? GetEntity (string entityID)
        {
            return AllEntities().FirstOrDefault(x => x.ID == entityID);
        }
    }
}
