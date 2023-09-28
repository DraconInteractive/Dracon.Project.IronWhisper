using IronWhisperReceiver.Core;
using IronWhisperReceiver.Core.Networking;
using IronWhisperReceiver.Core.Registry;
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

        private const int targetPort = 65931;
        private static CancellationTokenSource cts;


        public async Task SendCommand(RegDevice rDevice, string command, Action<string> onCommandResult)
        {
            SendCommandAsync(rDevice, command, onCommandResult);
        }

        public async Task SendCommandAsync (RegDevice rDevice, string command, Action<string> onCommandResult)
        {
            cts = new CancellationTokenSource();
            await SendCommandInternal(rDevice, command, onCommandResult, cts.Token);
        }

        public void StopCommandProcess ()
        {
            cts.Cancel();
        }

        private async Task SendCommandInternal(RegDevice rDevice, string command, Action<string> onCommandResult, CancellationToken cancellationToken)
        {
            TcpClient client = new TcpClient();
            client.Connect(rDevice.networkDevice.Address, targetPort);

            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            writer.WriteLine(command);
            writer.WriteLine("*FIN*");

            bool resultReceived = false;
            string message = "";
            try
            {
                while (client.Connected && !cancellationToken.IsCancellationRequested)
                {
                    if (stream.DataAvailable && !resultReceived)
                    {
                        string line = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            CoreSystem.Log($"[TCP] [{rDevice.DisplayName}] {line}", 1);
                            if (line.ToLower() == "*FIN*")
                            {
                                resultReceived = true;
                                onCommandResult?.Invoke(message);
                            }
                            else
                            {
                                message += line;
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
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

        }
    }
}
