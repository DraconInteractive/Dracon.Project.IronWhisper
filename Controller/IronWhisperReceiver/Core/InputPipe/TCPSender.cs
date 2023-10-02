using IronWhisper_CentralController.Core;
using IronWhisper_CentralController.Core.Networking;
using IronWhisper_CentralController.Core.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.InputPipe
{
    public class TCPSender
    {
        public static TCPSender Instance;

        private const int targetPort = 24765;
        private static CancellationTokenSource cts;
        private const string Termination = "*&*";

        public TCPSender ()
        {
            Instance = this;
        }

        public async Task SendCommand(RegDevice rDevice, string command, Action<string> onCommandResult)
        {
            SendCommandAsync(rDevice, command, onCommandResult);
        }

        public async Task SendCommandAsync (RegDevice rDevice, string command, Action<string> onCommandResult)
        {
            CoreSystem.Log("Sending async command");
            cts = new CancellationTokenSource();
            await SendCommandInternal(rDevice, command, onCommandResult, cts.Token);
        }

        public void StopCommandProcess ()
        {
            cts.Cancel();
        }

        private async Task SendCommandInternal(RegDevice rDevice, string command, Action<string> onCommandResult, CancellationToken cancellationToken)
        {
            // Create tcp client
            TcpClient client = new TcpClient();
            CoreSystem.Log("Connecting to access point...");
            client.Connect(rDevice.networkDevice.Address, targetPort);

            // setup read/right
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            CoreSystem.Log("Sending command...");

            // send command and termination packet
            await writer.WriteLineAsync(command + Termination);

            CoreSystem.Log("Waiting for response...", writeLine: false);

            bool resultReceived = false;
            string message = "";
            try
            {
                // Wait for data from client
                while (client.Connected && !cancellationToken.IsCancellationRequested && !resultReceived)
                {
                    if (stream.DataAvailable)
                    {
                        // When data received, get it
                        // If its a message, add it to our total message
                        // if its a termination packet, collate the message and invoke the event
                        string line = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            CoreSystem.Log($"\n[TCP] [{rDevice.DisplayName}] {line}", 1);
                            message += line;
                            if (message.Contains(Termination))
                            {
                                message = message.Replace(Termination, "");
                                resultReceived = true;
                                onCommandResult?.Invoke(message);
                                break;
                            }
                        }
                    }
                    else
                    {
                        CoreSystem.Log(".", writeLine: false);

                        await Task.Delay(250);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[TCP] [Error] " + ex.ToString());
            }
            finally
            {
                client.Close();
            }
            CoreSystem.Log("TCP client closed");

        }
    }
}
