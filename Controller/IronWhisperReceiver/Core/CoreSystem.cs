using IronWhisperReceiver.Core.InputPipe;
using IronWhisperReceiver.Core.Networking;
using IronWhisperReceiver.Core.Registry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Core
{
    public class CoreSystem
    {
        public static CoreSystem Instance;

        public static int Verbosity = 1;
        public static bool LaunchServer = false;
        public static bool SweepNetwork = false;
        public static bool ListenUDP = true;
        public static bool RefreshRegistry = false;
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
            Log("IW-Core v0.1.7a\n-------------------------------------------\n");
            Instance = this;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);

            var terminalInputSocket = new TerminalInputSocket();
            var actionsController = new ActionManager();
            var apiManager = new APIManager();
            var networkManager = new NetworkManager();
            var eventsManager = new EventsManager();

            RegistryCore registry;

            if (RefreshRegistry)
            {
                registry = RegistryCore.CreateDefault();
                registry.Save();
            }
            else
            {
                registry = new RegistryCore().Load();
            }

            if (ListenUDP)
            {
                UDPReceiver.StartListening();
            }

            if (LaunchServer)
            {
                //TODO Fix
                new ServerLauncher().Launch();
            }

            if (SweepNetwork)
            {
                await networkManager.PingNetworkAsync();
                Log();
            }

            HandleSocket(terminalInputSocket, eventsManager, actionsController);
            
            while (true)
            {
                await Task.Delay(100);
            }
        }

        private async Task HandleSocket(TerminalInputSocket socket, EventsManager evt, ActionManager actions)
        {
            Log("[Socket] Beginning connection process...");
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

        public static void Log(int verbosity = 0)
        {
            Log("", verbosity);
        }

        public static void Log(string message, int verbosity = 0, bool writeLine = true)
        {
            if (Verbosity >= verbosity)
            {
                Console.Write(message + (writeLine ? "\n" : ""));
            }
        }
    }
}
