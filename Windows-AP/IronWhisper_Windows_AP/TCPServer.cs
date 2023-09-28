using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_Windows_AP
{
    public class TCPServer
    {
        public static TCPServer Instance;
        private const int Port = 65931;

        private CancellationTokenSource cts;

        public TCPServer()
        {
            Instance = this;
        }

        public async Task Start ()
        {
            CoreSystem.Log("Started TCP sequence", 1);
            cts = new CancellationTokenSource();
            await TCPSequence(cts.Token);

        }

        public void Stop ()
        {
            CoreSystem.Log("Stopping TCP sequence", 1);
            cts.Cancel();
        }

        private async Task TCPSequence (CancellationToken cancellationToken)
        {
            CoreSystem.Log("[TCP] Opening listener to all addresses on port " + Port, 1);
            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                NetworkStream stream = client.GetStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                string command = "";
                bool messageReceived = false;
                try
                {
                    while (!messageReceived && !cancellationToken.IsCancellationRequested)
                    {
                        string message = await reader.ReadLineAsync();

                        if (message == "**FIN**")
                        {
                            CoreSystem.Log($"[TCP] Received: {message}", 1);

                            messageReceived = true;
                        }
                        else
                        {
                            command += message;
                        }
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        // finally will still be executed, so client will be closed correctly
                        return;
                    }

                    string result = ProcessCommand(command);
                    CoreSystem.Log($"[TCP] Command result: {result}\nSending result to central processor", 1);
                    writer.WriteLine(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    client.Close();
                }
            }
        }

        static string ProcessCommand(string command)
        {
            // Process the command and return the result.
            return "COMMAND_PROCESSED";
        }
    }
}
