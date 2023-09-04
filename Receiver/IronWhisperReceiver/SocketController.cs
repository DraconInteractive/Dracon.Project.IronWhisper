using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver
{
    internal class SocketController
    {
        public static SocketController Instance;

        readonly string serverIP = "172.28.31.175"; // Change this to the server's IP address
        readonly int serverPort = 31050;            // Change this to the server's port number
        readonly string voicePrompt = "Okay,";
        string lastReceived = "";

        TcpClient client;
        NetworkStream stream;

        public Action<string, string> commandReceived;

        public SocketController()
        {
            client = new TcpClient();
            Instance = this;
        }

        public bool TryConnect ()
        {
            try
            {
                client.Connect(serverIP, serverPort);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public void Connect ()
        {
            while (!client.Connected)
            {
                var result = TryConnect();
                if (!result)
                {
                    Thread.Sleep(100);
                }
            }

            Console.WriteLine($"[Socket] Connected at {serverIP}:{serverPort}");
            Console.WriteLine();
        }

        public void StartStream ()
        {
            stream = client.GetStream();
        }

        public void SocketTick ()
        {
            try
            {
                if (stream.DataAvailable)
                {
                    // Receive and display the response from the server
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Transcript received: {response}");
                    string command = response.Replace(voicePrompt, "").ToLower();
                    commandReceived?.Invoke(response.Trim(), command.Trim());
                    lastReceived = response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Socket error occurred: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void CloseStream ()
        {
            stream.Close();
            stream.Dispose();
        }

        public void Cleanup ()
        {
            CloseStream();
            client.Close();
        }
    }
}
