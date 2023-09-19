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
        // TODO fix
        public static bool LaunchServer = false;
        public static bool ContactNetwork = true;
        public static bool ListenUDP = true;
        // TODO
        // Identify this 'access point' - add device data json to same appdata folder as registry
        // Move persistence logic to its own class
        // Add terminal id to the start of its packet
        public async Task Run()
        {
            Log("IW-Core v0.1.5a\n-------------------------------------------\n");
            Instance = this;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);

            var terminalInputSocket = new TerminalInputSocket();
            var actionsController = new ActionManager();
            var apiManager = new APIManager();
            var networkManager = new NetworkManager();
            var eventsManager = new EventsManager();
            //var registry = new Registry().Load();

            var registry = RegistryCore.CreateDefault();
            registry.Save();

            if (ListenUDP)
            {
                DeviceID_UDPListener.StartListening();
            }

            if (LaunchServer)
            {
                new ServerLauncher().Launch();
            }

            if (ContactNetwork)
            {
                await networkManager.PingNetworkAsync();
                Log();
            }

            Log("[Socket] Beginning connection process...");
            terminalInputSocket.Connect();
            terminalInputSocket.StartStream();

            while (true)
            {
                var speech = terminalInputSocket.SocketTick();
                if (speech != null)
                {
                    await actionsController.ParseCommand(speech);
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
