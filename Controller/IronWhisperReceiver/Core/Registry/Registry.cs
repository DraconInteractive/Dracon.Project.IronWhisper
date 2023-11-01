using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Registry
{
    public class RegistryManager : CoreManager
    {
        public static RegistryManager Instance { get; private set; }
        public List<RegAccessPoint> AccessPoints = new();
        public List<RegTerminal> Terminals = new();
        public List<RegProject> Projects = new();
        public List<RegConfig> ConfigFiles = new();

        public static string FolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IronWhisper");
        private static string FilePath => Path.Combine(FolderPath, "registry.json");

        public RegistryManager()
        {
            Instance = this;
        }

        public static RegistryManager CreateDefault()
        {
            CoreSystem.Log("[Registry] Creating default", 1);
            RegistryManager registry = new();
            RegTerminal mainTerminal = new()
            {
                DisplayName = "Terminal - Main",
                Tags = new List<string>()
                {
                    "Home",
                    "WSL"
                },
                SpeechTags = new List<string>() { "home terminal" },
                deviceID = "TERMINAL_HOME_MAIN"
            };

            RegAccessPoint mainHomeAP = new()
            {
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
                Owner = "Peter Carey",
                deviceID = "AP_HOME_MAIN"
            };

            RegAccessPoint homeServerAP = new()
            {
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
                Owner = "Peter Carey",
                deviceID = "AP_HOME_SECONDARY"
            };

            RegAccessPoint homeLaptopAP = new()
            {
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
                    "my laptop",
                    "the laptop"
                },
                Owner = "Peter Carey",
                deviceID = "AP_HOME_LAPTOP"
            };

            RegAccessPoint homeTabletAP = new()
            {
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
                Owner = "Peter Carey",
                deviceID = "AP_HOME_TABLET"
            };

            RegAccessPoint mobileAP = new()
            {
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
                Owner = "Peter Carey",
                deviceID = "AP_HOME_MOBILE"
            };

            RegConfig testConfig = new ()
            {
                DisplayName = "Test Configuration File",
                fileName = "testConfig.json",
                Tags = new()
                {
                    "Test"
                }
            };

            RegConfig testConfig2 = new()
            {
                DisplayName = "Second Test Configuration File",
                fileName = "testConfig2.json",
                Tags = new()
                {
                    "Test"
                }
            };

            RegProject projectFIFA = new()
            {
                DisplayName = "FIFA AI League",
                Description = "FutureVerse / FIFA collaboration on an ML soccer-manager game.",
                isGitRepository = true,
                Path = "C:/Users/pmc10/Documents/afif-prototype/",
                Tags = new List<string>()
                {
                    "FV",
                    "ML"
                },
                SpeechTags = new List<string>()
                {
                    "ai league",
                    "fifa"
                },
                Configs = new List<RegConfig>()
                {
                    testConfig
                }
            };

            RegProject projectIronWhisper = new RegProject()
            {
                ID = Guid.NewGuid().ToString(),
                DisplayName = "Iron Whisper",
                Description = "AI Assistant, full pipeline from command input to action execution and subsequent feedback to the user",
                isGitRepository = true,
                Path = "C:/Users/pmc10/Documents/IronWhisper/",
                Tags = new List<string>()
                {
                    "Personal",
                    "AI",
                    "C++",
                    "C#"
                },
                SpeechTags = new List<string>() { "iron whisper" },
                Configs = new List<RegConfig>() { testConfig, testConfig2 }
            };

            registry.Projects.Add(projectFIFA);
            registry.Projects.Add(projectIronWhisper);

            registry.AccessPoints.Add(mainHomeAP);
            registry.AccessPoints.Add(homeServerAP);
            registry.AccessPoints.Add(homeLaptopAP);
            registry.AccessPoints.Add(homeTabletAP);
            registry.AccessPoints.Add(mobileAP);

            registry.ConfigFiles.Add(testConfig);
            registry.ConfigFiles.Add(testConfig2);

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

        public RegistryManager Load()
        {
            if (ValidateFile())
            {
                string data = File.ReadAllText(FilePath);
                var reg = JsonConvert.DeserializeObject<RegistryManager>(data);
                AccessPoints = reg.AccessPoints;
                Terminals = reg.Terminals;
                Projects = reg.Projects;
                ConfigFiles = reg.ConfigFiles;
            }
            else
            {
                var def = CreateDefault();
                AccessPoints = def.AccessPoints;
                Terminals = def.Terminals;
                Projects = def.Projects;
                ConfigFiles = def.ConfigFiles;
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

        public void UpdateNetworkDevice(string deviceID, string remoteAddress)
        {
            foreach (var device in AllDevices())
            {
                if (device.deviceID == deviceID)
                {
                    if (!device.networkDevice.Online)
                    {
                        CoreSystem.Log($"[UDP_ID] {deviceID} has come online", 1);
                    }
                    CoreSystem.Log($"[UDP_ID] [Registry] {deviceID} identified. RemoteAddress: {remoteAddress}", 2);
                    device.networkDevice.UpdateDetails(new NetworkDevice() { Address = remoteAddress });
                    Save();
                }
            }
        }

        public void UpdateNetworkDevice(string deviceID, IPAddress remoteAddress)
        {
            UpdateNetworkDevice(deviceID, remoteAddress.ToString());
        }


        public bool IsRegisteredNetworkDevice(string ip = "", string hostname = "", string deviceID = "")
        {
            bool match = AllDevices().Any(x => x.networkDevice.Address == ip || x.deviceID == deviceID);
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

        public RegConfig? GetConfig (string fileName)
        {
            return ConfigFiles.FirstOrDefault(x => x.fileName == fileName);
        }

        public RegConfig[]? GetProjectConfigs (string projectName)
        {
            return Projects.FirstOrDefault(x => x.DisplayName == projectName)?.Configs.ToArray();
        }
    }
}
