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
        public bool ListenUDP = true; // D
        public bool RefreshRegistry = false;
        public bool InitTCP = true; // D

        public bool UseMimic3 = false; // dir -> source .venv/bin/activate -> mimic3-server --preload-voice en_UK/apope_low --num-threads 2
        public bool BlindAccessible = false;
        public int TTSVerbosity = 0;

        public bool useTerminalSocket = true;
        public bool useRestAPI = true;
        public bool useManualInput = true;

        public bool LaunchNGROK = false;
        public bool LaunchTerminal = true;
        public string Version = "v0.1.9a";
    }

    public class CoreSystem
    {
        public static CoreSystem Instance;
        public static CoreConfig Config;

        public async Task Run()
        {
            Config = new CoreConfig();

            Log($"IW-Core {Config.Version}\n-------------------------------------------\n");

            Instance = this;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);

            // These are all here to create their singleton instances. 
            // I could use a proper _instance/Instance implementation, but so far im just lazy
            var actionsController = new ActionManager();
            var apiManager = new APIManager();
            var eventsManager = new EventsManager();
            var ttsManager = new TTSManager();
            var restManager = new RESTManager();
            var inputHandler = new InputHandler();
            var socketManager = new SocketManager();

            if (Config.UseMimic3)
            {
                var ttsOnline = await apiManager.GetURLOnline(APIManager.ttsURL);
                if (ttsOnline)
                {
                    Log("[TTS] Setup: Success", "Success", ConsoleColor.Green);
                    await Speak(CachedTTS.Boot_TTS_Online);
                    await Task.Delay(500);
                }
                else
                {
                    Config.UseMimic3 = false;
                    Log("[TTS] Setup: Failure", "Failure", ConsoleColor.Red);
                }
                Log();
            }

            RegistryCore registry;

            if (Config.RefreshRegistry)
            {
                registry = RegistryCore.CreateDefault();
                registry.Save();
            }
            else
            {
                registry = new RegistryCore().Load();
            }

            if (Config.ListenUDP)
            {
                socketManager.StartListening_IDBroadcast();
            }

            inputHandler.onInputReceived += async x => await actionsController.ParseCommand(x);

            if (Config.useRestAPI)
            {
                // Using REST API means no socket handler. Disabling until a shared input handler can be made (should support manual input, and programmatic input)
                Log("Starting rest server");
                restManager.LaunchServer();
            }
            if (Config.useTerminalSocket)
            {
                Log("Opening WSL instance");
                // TODO: change this to a relative path, or environment variable. 
                string windowsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //string wslPath = $"/mnt/c{windowsFolderPath.Replace("C:", "").Replace("\\", "/")}/IronWhisper/Whisper-Terminal";
                string path = Environment.GetEnvironmentVariable("WSLTerminalPath", EnvironmentVariableTarget.User);
                Log("Path: " + path);
                string wslCommand = $"cd {path} && ./command -t 8";

                Utilities.CreateWSLWindowWithPrompt(wslCommand);

                // Wait a bit for WSL to launch
                await Task.Delay(500);

                // Setup socket, including getting WSL IP
                var terminalInputSocket = new TerminalInputSocket();

                terminalInputSocket.RunLoop();
            }

            Log("Core Loop: Success", "Success", ConsoleColor.Green);
            while (true)
            {
                if (Config.useManualInput)
                {
                    string input = Console.ReadLine();
                    inputHandler.RegisterInput(input);
                }
                else
                {
                    await Task.Delay(100);
                }
                while (eventsManager.EventsAvailable())
                {
                    var ev = eventsManager.DequeueEvent();
                    await ev.Consume();
                }
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
            }
        }

        // Print log with word highlight
        public static void Log(string message, string highlight, ConsoleColor color, int verbosity = 0, bool writeLine = true)
        {
            if (Config.Verbosity < verbosity)
            {
                return;
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

        public static string GetInput(string prompt, Predicate<string> validation, int retries = 0, string retryMessage = "")
        {
            int currentRetries = 0;
            string input = "";
            if (retryMessage == "")
            {
                retryMessage = "Invalid input. Please try again";
            }

            while (currentRetries <= retries)
            {
                Log(prompt);
                input = Console.ReadLine().Trim();
                if (validation(input.ToLower()))
                {
                    return input;
                }
                else
                {
                    Log(retryMessage);
                }
                if (retries > 0)
                {
                    currentRetries++;
                }
            }
            return input;
        }
    }
}
