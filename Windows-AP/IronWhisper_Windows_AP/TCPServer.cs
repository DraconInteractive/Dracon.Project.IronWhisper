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
        private const int Port = 24765;

        private CancellationTokenSource cts;

        private const string Termination = "*&*";
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
                string message = "";
                bool messageReceived = false;
                try
                {
                    while (!messageReceived && !cancellationToken.IsCancellationRequested)
                    {
                        string line = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            CoreSystem.Log("[TCP] " + line);
                            message += line;
                            if (message.Contains(Termination))
                            {
                                message = message.Replace(Termination, "");
                                messageReceived = true;
                                CoreSystem.Log("Final Message: " + message);
                            }
                        }
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        // finally will still be executed, so client will be closed correctly
                        return;
                    }

                    string result = ProcessCommand(message);
                    CoreSystem.Log($"[TCP] Command result: {result}", 1);
                    CoreSystem.Log($"[TCP] Sending result to central processor...", 1);
                    await writer.WriteLineAsync(result + Termination);
                    /*
                    int i = 0; 
                    while (i < 1000)
                    {
                        i += 50;
                        Thread.Sleep(50);
                    }
                    CoreSystem.Log("Finished waiting");
                    */
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    client.Close();
                }
                CoreSystem.Log("Complete");
            }
        }

        static string ProcessCommand(string command)
        {
            // Process the command and return the result.
            return "COMMAND_PROCESSED";
        }
    }
}
