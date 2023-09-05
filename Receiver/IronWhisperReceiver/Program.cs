using IronWhisperReceiver;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        var core = new Core();
        await core.Run();
    }
}

class Core
{
    public SocketController socketController;
    public ActionsController actionsController;
    public APIManager apiManager;
    public EventsManager eventsManager;

    public async Task Run ()
    {
        socketController = new SocketController();
        actionsController = new ActionsController();
        apiManager = new APIManager();
        eventsManager = new EventsManager();

        socketController.Connect();
        socketController.StartStream();
        while (true)
        {
            var command = socketController.SocketTick();
            if (command != null)
            {
                await actionsController.ParseCommand(command);
            }
            while (eventsManager.EventsAvailable())
            {
                var ev = eventsManager.DequeueEvent();
                await ev.Consume();
            }
        }
    }
}
