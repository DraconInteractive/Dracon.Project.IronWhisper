using IronWhisper_CentralController.Core.InputPipe;
using IronWhisper_CentralController.Core.Networking;
using IronWhisper_CentralController.Core.Registry;
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
    }
    public class CoreSystem
    {
        public static CoreSystem Instance;
        public static CoreConfig Config;

        // TODO
        // Identify this 'access point' - add device data json to same appdata folder as registry
        // Move persistence logic to its own class

        // TODO
        // Phase 1. Identification
        /*
        1. The server opens a UDP socket to listen for broadcasts
        2. When connecting to a network, or upon a similar key event, the mobile device broadcasts an identification packet
        3. It will continue to do this until it receives an 'id confirmed' packet from the server in response
        4. Both devices are now aware of each other. Stop broadcasting on the mobile device. 
        5. Open a TCP connection is opened to update the cached data of each device. Close once all data is transferred
        5. Start a timer on mobile device. After the interval, send an 'im alive' packet to the server. The server is already aware of the device, and this will just update the devices online status.
        5a. Alternatively, the server could periodically ping the mobile devices IP address in order to check if its still connected. 
        */

        // Phase 2. Communication
        /*
        1. Ascertain the command, encode as a string to create packet
        2. Open a TCP stream between server and android device
        3. Server sends packet
        4. Device receives packet, executes command
        5. Device collates result, sends to server
        6. Close TCP stream
        */

        // Phase 3. Focused persistence
        /*
        1. Get command to open persistent stream to device
        2. Run Phase 2, but the command is to keep the stream open, and phase 2.6 doesnt happen
        3. Server continues to send commands and receive results over the same stream
        4. When server shuts down, or command to cease communication is received, send command to device to disconnect and disconnect server. 
        */

        // TODO

        /*
        Authenticated Commands. 
        Most commands will require a passphrase such as a prompt etc, that prefixes it. 
        "Okay X, do ..." 
        However, in order to execute subsequent commands, or tiered input, we need an 'open gate' mode
        When specific command is registered, register all incoming voice as valid input until the close gate command is reached
        
        On the inverse, a lock gate command will make even prompt commands not work until a single unlock phrase is given, or the server recieves a manual command to resume. 
        This is to prevent the misuse of the device due to the lack of subject/speaker identification
        */

        public async Task Run()
        {
            Config = new CoreConfig();

            Log("IW-Core v0.1.7a\n-------------------------------------------\n");

            Instance = this;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);

            var terminalInputSocket = new TerminalInputSocket();
            var actionsController = new ActionManager();
            var apiManager = new APIManager();
            var networkManager = new NetworkManager();
            var eventsManager = new EventsManager();

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
            socket.Connect();
            socket.StartStream();

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
            Log("", verbosity);
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
