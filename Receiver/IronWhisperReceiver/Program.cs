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

    public void Run ()
    {
        socketController = new SocketController();
        socketController.Connect();
        socketController.StartStream();
        while (true)
        {
            socketController.SocketTick();
        }
    }
}
