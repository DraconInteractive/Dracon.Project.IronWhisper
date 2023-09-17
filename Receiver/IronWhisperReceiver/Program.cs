using IronWhisperReceiver;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using IronWhisperReceiver.Registry;
using Newtonsoft.Json;
using IronWhisperReceiver.Networking;

class Program
{
    static async Task Main(string[] args)
    {
        var core = new Core();
        await core.Run();
    }
}

public class Core
{
    public static Core Instance; 

    public static int Verbosity = 1;
    // TODO fix
    public static bool LaunchServer = false;
    public static bool ContactNetwork = false;
    public static bool ListenUDP = true;
    // TODO
    // Identify this 'access point' - add device data json to same appdata folder as registry
    // Move persistence logic to its own class
    // Add terminal id to the start of its packet
    public async Task Run ()
    {
        Log("IW-Core v0.1.5a\n-------------------------------------------\n");
        Instance = this;

        Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);

        var socketController = new SocketController();
        var actionsController = new ActionManager();
        //new APIManager();
        var networkManager = new NetworkManager();
        var eventsManager = new EventsManager();
        //var registry = new Registry().Load();

        var registry = Registry.CreateDefault();
        registry.Save();

        if (ListenUDP)
        {
            UDPListener.StartListening();
        }

        if (LaunchServer)
        {
            new ServerLauncher().Launch();
        }

        if (ContactNetwork)
        {
            await networkManager.PingNetworkAsync();
            Log(JsonConvert.SerializeObject(networkManager.devices), 1);
            Log();
        }

        Log("[Socket] Beginning connection process...");
        socketController.Connect();
        socketController.StartStream();

        while (true)
        {
            var speech = socketController.SocketTick();
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

    protected static void ExitHandler (object sender, ConsoleCancelEventArgs args)
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

    public static void Log (string message, int verbosity = 0, bool writeLine = true)
    {
        if (Verbosity >= verbosity)
        {
            Console.Write(message + (writeLine ? "\n" : ""));
        }
    }
}
