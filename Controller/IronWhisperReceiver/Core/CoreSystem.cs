using IronWhisper_CentralController.Core.InputPipe;
using IronWhisper_CentralController.Core.Networking;
using IronWhisper_CentralController.Core.Registry;
using IronWhisper_CentralController.Core.Events;
using IronWhisper_CentralController.Core.Audio.TTS;

using System.Windows.Forms;
using System.Windows;
using System;
using IronWhisper_CentralController.Core.Networking.REST;
using IronWhisper_CentralController.Core.Networking.Sockets;

namespace IronWhisper_CentralController.Core
{
    // TODO    
    // Implement core loop functionality
    public class CoreConfig
    {
        public int Verbosity = 1;
        public bool RefreshRegistry = false;

        public bool UseMimic3 = false; // dir -> source .venv/bin/activate -> mimic3-server --preload-voice en_UK/apope_low --num-threads 2
        public bool BlindAccessible = false;
        public int TTSVerbosity = 0;

        public bool useTerminalSocket = true;
        public bool useUDPIDSocket = true;
        public bool useTCPCommandSocket = true;
        public bool useRestAPI = true;
        public bool useManualInput = true;

        public bool LaunchNGROK = false;
        public string Version = "v0.2.0a";
    }

    public class CoreSystem
    {
        public static CoreSystem Instance;
        public static CoreConfig Config;

        public static List<string> Logs = new();

        public async Task Run()
        {
            Config = new CoreConfig();

            Log($"IW-Core {Config.Version}");
            Log("-------------------------------------------\n");

            Instance = this;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);

            var managerTypes = Utilities.GetArchetypes(typeof(CoreManager));
            foreach (Type type in managerTypes)
            {
                Activator.CreateInstance(type);
            }

            if (Config.UseMimic3)
            {
                TTSManager.InitializeCacheDictionary();

                var ttsOnline = await APIManager.Instance.GetURLOnline(APIManager.ttsURL);
                if (ttsOnline)
                {
                    LogSystemStatus("Mimic3", SystemStatus.Online);
                    await Speak(CachedTTS.Boot_TTS_Online);
                    await Task.Delay(500);
                }
                else
                {
                    Config.UseMimic3 = false;
                    LogSystemStatus("Mimic3", SystemStatus.Offline);
                }
            }
            else
            {
                LogSystemStatus("Mimic3", SystemStatus.Disabled);
            }

            RegistryManager registry;

            if (Config.RefreshRegistry)
            {
                registry = RegistryManager.CreateDefault();
                registry.Save();
            }
            else
            {
                registry = new RegistryManager().Load();
            }

            bool allConfigsValid = true;
            foreach (var config in registry.ConfigFiles)
            {
                bool valid = await config.Validate();
                if (!valid)
                {
                    allConfigsValid = false;
                }
            }

            if (allConfigsValid)
            {
                LogSystemStatus("Registry", SystemStatus.Online);
            }
            else
            {
                LogSystemStatus("Registry", SystemStatus.Error);
            }

            if (Config.useUDPIDSocket)
            {
                SocketManager.Instance.StartListening_IDBroadcast();
                LogSystemStatus("UDP Broadcast Receiver", SystemStatus.Online);
            }
            else
            {
                LogSystemStatus("UDP Broadcast Receiver", SystemStatus.Disabled);
            }

            InputHandler.Instance.onInputReceived += async x => await ActionManager.Instance.ParseCommand(x);

            if (Config.useRestAPI)
            {
                _ = RESTManager.Instance.LaunchServer();
            }
            else
            {
                LogSystemStatus("REST Server", SystemStatus.Disabled);
            }

            if (Config.useTerminalSocket)
            {
                string windowsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string path = Environment.GetEnvironmentVariable("WSLTerminalPath", EnvironmentVariableTarget.User) ?? "";
                string wslCommand = $"cd {path} && ./command -t 8";

                _ = Utilities.CreateWSLWindowWithPrompt(wslCommand);

                // Wait a bit for WSL to launch
                await Task.Delay(500);

                _ = SocketManager.Instance.StartWSLLoop();
                LogSystemStatus("Whisper", SystemStatus.Online);
            }
            else
            {
                LogSystemStatus("Whisper", SystemStatus.Disabled);
            }

            Log();
            Log("System viable. Beginning core loop.");
            Log("-------------------------------------------\n");

            while (true)
            {
                while (EventsManager.Instance.EventsAvailable())
                {
                    var ev = EventsManager.Instance.DequeueEvent();
                    await ev.Consume();
                }
                await Task.Delay(100);
            }
        }

        protected static void ExitHandler(object sender, ConsoleCancelEventArgs args)
        {
            Log("Ctrl-C pressed. Exiting");
            args.Cancel = true;

            Environment.Exit(0);
        }

        // Print newline
        public static void Log(int verbosity = 0)
        {
            if (Config.Verbosity >= verbosity)
            {
                Log("", verbosity);
            }
        }

        // Basic log print
        public static void Log(string message, int verbosity = 0, bool writeLine = true)
        {
            if (Config.Verbosity >= verbosity)
            {
                Console.Write(message + (writeLine ? "\n" : ""));
                Logs.Add(message);
                while (Logs.Count > 100)
                {
                    Logs.RemoveAt(0);
                }
            }
        }

        // Print log with word highlight
        public static void Log(string message, string highlight, ConsoleColor color, int verbosity = 0, bool writeLine = true)
        {
            if (Config.Verbosity < verbosity)
            {
                return;
            }

            Logs.Add(message);
            while (Logs.Count > 100)
            {
                Logs.RemoveAt(0);
            }

            int currentIndex = 0;

            if (Config.BlindAccessible && Config.UseMimic3)
            {
                TTSManager.Instance.ProcessTTS(Utilities.RemoveLogDescriptor(message));
            }

            while (currentIndex < message.Length)
            {
                int foundIndex = message.IndexOf(highlight, currentIndex, StringComparison.OrdinalIgnoreCase);

                // If the highlight text isn't found, print the rest of the message and break out of the loop.
                if (foundIndex == -1)
                {
                    Console.Write(message.Substring(currentIndex));
                    break;
                }

                // Print text leading up to the highlight in default color.
                Console.Write(message.Substring(currentIndex, foundIndex - currentIndex));

                // Set the desired highlight color and print the highlighted text.
                Console.ForegroundColor = color;
                Console.Write(message.Substring(foundIndex, highlight.Length));
                Console.ResetColor();

                currentIndex = foundIndex + highlight.Length;
            }

            if (writeLine)
            {
                Console.WriteLine(); // To print a newline after the message.
            }
        }

        public static void LogError(string message, int verbosity = 0, bool writeLine = true)
        {
            Log($"[Error] {message}", "Error", ConsoleColor.Red, verbosity, writeLine);
        }

        public enum SystemStatus
        {
            Online,
            Offline,
            Disabled,
            Error
        }
        public static void LogSystemStatus (string system, SystemStatus status)
        {
            string statusSymbol = "";
            ConsoleColor color = ConsoleColor.Gray;
            switch (status)
            {
                case SystemStatus.Online:
                    statusSymbol = "✓";
                    color = ConsoleColor.Green;
                    break;
                case SystemStatus.Offline:
                    statusSymbol = "x";
                    color = ConsoleColor.Red;
                    break;
                case SystemStatus.Disabled:
                    statusSymbol = "-";
                    color = ConsoleColor.DarkGray;
                    break;
                case SystemStatus.Error:
                    statusSymbol = "=";
                    color = ConsoleColor.Red;
                    break;
            }

            Log($"[{statusSymbol}] {system}", statusSymbol, color);
        }

        public static async Task Speak(string message, int verbosity = 0)
        {
            if (!Config.UseMimic3)
            {
                return;
            }
            if (Config.TTSVerbosity >= verbosity)
            {
                await TTSManager.Instance.ProcessTTS(message);
            }
        }

        public static async Task Speak (CachedTTS audio)
        {
            if (!Config.UseMimic3)
            {
                return;
            }
            TTSManager.Instance.PlayAudio(audio);
        }

        
    }
}
