using IronWhisper_CentralController.Core.InputPipe;
using IronWhisper_CentralController.Core.Networking;
using IronWhisper_CentralController.Core.Registry;
using IronWhisper_CentralController.Core.Events;
using IronWhisper_CentralController.Core.Audio.TTS;

using System.Windows.Forms;
using System.Windows;
using System;

namespace IronWhisper_CentralController.Core
{
    public class CoreConfig
    {
        public int Verbosity = 1;
        public bool LaunchServer = false;
        public bool ListenUDP = true;
        public bool SweepNetwork = false;
        public bool RefreshRegistry = false;
        public bool InitTCP = true;
        public bool UseMimic3 = true;
        public bool DeafAccessible = false;
        public int TTSVerbosity = 1;
    }

    public class CoreSystem
    {
        public static CoreSystem Instance;
        public static CoreConfig Config;

        // TODO Cache static TTS wav's

        public async Task Run()
        {
            Config = new CoreConfig();

            Log("IW-Core v0.1.7a\n-------------------------------------------\n");

            Instance = this;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);

            // These are all here to create their singleton instances. 
            // I could use a proper _instance/Instance implementation, but so far im just lazy
            var terminalInputSocket = new TerminalInputSocket();
            var actionsController = new ActionManager();
            var apiManager = new APIManager();
            var networkManager = new NetworkManager();
            var eventsManager = new EventsManager();
            var ttsManager = new TTSManager();

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
                CoreSystem.Log();
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
                UDPReceiver.StartListening();
            }

            if (Config.LaunchServer)
            {
                //TODO Fix
                new ServerLauncher().Launch();
            }

            if (Config.SweepNetwork)
            {
                await networkManager.PingNetworkAsync();
                Log();
            }
            if (Config.InitTCP)
            {
                var tcpSender = new TCPSender();
            }

            HandleSocket(terminalInputSocket, eventsManager, actionsController);
            
            while (true)
            {
                await Task.Delay(100);
            }
        }

        private async Task HandleSocket(TerminalInputSocket socket, EventsManager evt, ActionManager actions)
        {
            Log("[Socket] Connecting to WSL2 terminal...");
            await Speak(CachedTTS.Boot_WaitForTerminal);

            socket.Connect();
            socket.StartStream();
            await Speak(CachedTTS.Terminal_OnConnected);

            while (true)
            {
                var speech = socket.SocketTick();
                if (speech != null)
                {
                    await actions.ParseCommand(speech);
                }
                while (evt.EventsAvailable())
                {
                    var ev = evt.DequeueEvent();
                    await ev.Consume();
                }
            }
        }

        protected static void ExitHandler(object sender, ConsoleCancelEventArgs args)
        {
            Log("Ctrl-C pressed. Exiting");
            args.Cancel = true;

            if (ServerLauncher.Instance != null && ServerLauncher.Instance.process != null)
            {
                ServerLauncher.Instance.Close();
            }

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

            if (Config.DeafAccessible && Config.UseMimic3)
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
            if (Config.TTSVerbosity >= verbosity)
            {
                //await TTSManager.Instance.ProcessTTS(message);
            }
        }

        public static async Task Speak (CachedTTS audio)
        {
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
