using IronWhisperReceiver;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        new Core().Run();
    }
}

class Core
{
    public SocketController socketController;
    public ActionsController actionsController;

    public void Run ()
    {
        socketController = new SocketController();
        actionsController = new ActionsController();

        socketController.Connect();
        socketController.StartStream();
        while (true)
        {
            socketController.SocketTick();
        }


    }
}
