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

        public bool UseTTSCache = true;
        public bool UseMimic3 = false; // dir -> source .venv/bin/activate -> mimic3-server --preload-voice en_UK/apope_low --num-threads 2
        public bool BlindAccessible = false;
        public int TTSVerbosity = 0;

        public bool useWSLLauncher = false;
        public bool useNGROK = true;

        public bool useTerminalSocket = false;
        public bool useUDPIDSocket = false;
        public bool useTCPCommandSocket = false;
        public bool useRestAPI = true;
        public bool useManualInput = true;

        public string Version = "v0.2.2a";
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

            if (Config.UseTTSCache)
            {
                bool foundAll = TTSManager.InitializeCacheDictionary();
                if (foundAll)
                {
                    LogSystemStatus("TTS Cache", SystemStatus.Online);
                    TTSManager.PlayAudio(CachedTTS.Labs_Boot_Initializing);
                }
                else
                {
                    Config.UseTTSCache = false;
                    LogSystemStatus("TTS Cache", SystemStatus.Offline);
                }
            }
            else
            {
                LogSystemStatus("TTS Cache", SystemStatus.Disabled);
            }

            if (Config.UseMimic3)
            {
                var ttsOnline = await APIManager.Instance.IsOnline(APIManager.ttsURL);
                if (ttsOnline)
                {
                    LogSystemStatus("Mimic3", SystemStatus.Online);
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

            LogSystemStatus("Registry", allConfigsValid ? SystemStatus.Online : SystemStatus.Error);

            if (Config.useUDPIDSocket)
            {
                SocketManager.Instance.StartListening_IDBroadcast();
            }

            LogSystemStatus("UDP Broadcast Receiver", Config.useUDPIDSocket ? SystemStatus.Online : SystemStatus.Disabled);

            // Create input bind here since its needed for NGROK input. Could probs be earlier. 
            InputHandler.Instance.onInputReceived += async x => await ActionManager.Instance.ParseCommand(x);

            LogSystemStatus("Manual Input", Config.useManualInput ? SystemStatus.Online : SystemStatus.Error);


            if (Config.useRestAPI)
            {
                await RESTManager.Instance.LaunchServer();
            }

            LogSystemStatus("REST Server", Config.useRestAPI ? SystemStatus.Online : SystemStatus.Disabled);

            if (Config.useTerminalSocket)
            {
                if (Config.useWSLLauncher)
                {
                    string windowsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string path = Environment.GetEnvironmentVariable("WSLTerminalPath", EnvironmentVariableTarget.User) ?? "";
                    string wslCommand = $"cd {path} && ./command -t 8";

                    _ = Utilities.CreateWSLWindowWithPrompt(wslCommand);

                    // Wait a bit for WSL to launch
                    await Task.Delay(500);

                }

                LogSystemStatus("WSL Launcher", Config.useWSLLauncher ? SystemStatus.Online : SystemStatus.Disabled);

                _ = SocketManager.Instance.StartWSLLoop();
            }

            LogSystemStatus("Whisper", Config.useTerminalSocket ? SystemStatus.Online : SystemStatus.Disabled);


            await TTSManager.PlayAudio(CachedTTS.Labs_Boot_MinReq);

            Log();
            Log("System viable. Beginning core loop.");
            Log("-------------------------------------------\n");

            await Task.Delay(500);

            await TTSManager.PlayGreeting();

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
                TTSManager.ProcessTTS(Utilities.RemoveLogDescriptor(message));
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

        // Set color for entire word
        public static void Log(string message, ConsoleColor color, int verbosity = 0, bool writeLine = true)
        {
            Log(message, message, color, verbosity, writeLine);
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
            if (Config.TTSVerbosity >= verbosity)
            {
                await TTSManager.ProcessTTS(message);
            }
        }
    }
}
