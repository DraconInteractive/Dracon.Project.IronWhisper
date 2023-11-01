using IronWhisper_CentralController.Core.Audio.TTS;
using IronWhisper_CentralController.Core.Registry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Networking.Sockets
{
    public class SocketManager : CoreManager
    {
        public static SocketManager Instance;

        private const int idBroadcastPort = 9876;
        private const int wslTerminalPort = 31050;
        private const int commandPort = 24765;
        private const string Termination = "*&*";
        private const string voicePrompt = "Okay,";

        private CancellationTokenSource idBroadcastCTS;
        private CancellationTokenSource commandCTS;
        private CancellationTokenSource wslCTS;

        private string terminalIP = "172.28.31.175"; // Change this to the server's IP address

        public SocketManager()
        {
            Instance = this;
            idBroadcastCTS = new CancellationTokenSource();
            commandCTS = new CancellationTokenSource();
            wslCTS = new CancellationTokenSource();
        }

        public void StartListening_IDBroadcast ()
        {
            ReceiveIDBroadcasts(idBroadcastCTS.Token);
        }

        public void StopListening_IDBroadcast ()
        {
            idBroadcastCTS.Cancel();
        }

        public async Task SendTCP_Command(RegDevice rDevice, string command, Action<string> onCommandResult)
        {
            await SendCommand(rDevice, command, onCommandResult, commandCTS.Token);
        }

        public void CancelTCP_Command ()
        {
            commandCTS.Cancel();
        }

        private async Task ReceiveIDBroadcasts (CancellationToken token)
        {
            UdpClient listener = new(idBroadcastPort);
            IPEndPoint groupEP = new(IPAddress.Any, idBroadcastPort);

            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        CoreSystem.Log("[UDP] Stopping broadcast receiver...", 1);
                        break;
                    }
                    UdpReceiveResult result = await listener.ReceiveAsync(token);
                    byte[] bytes = result.Buffer;
                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                    RegistryManager.Instance.UpdateNetworkDevice(message, result.RemoteEndPoint.Address);
                }
            }
            catch (Exception e)
            {
                CoreSystem.LogError($"[UDP] {e.Message}");
            }
            finally
            {
                listener.Close();
            }
            CoreSystem.Log("[UDP] Stopped", 1);
        }

        private async Task SendCommand(RegDevice rDevice, string command, Action<string> onCommandResult, CancellationToken token)
        {
            // Create tcp client
            TcpClient client = new();
            CoreSystem.Log($"[TCP][Command] Connecting to {rDevice.networkDevice.Address}:{commandPort}", $"{rDevice.networkDevice.Address}:{commandPort}", ConsoleColor.Yellow);
            client.Connect(rDevice.networkDevice.Address, commandPort);

            // setup read/right
            NetworkStream stream = client.GetStream();
            StreamReader reader = new(stream, Encoding.UTF8);
            StreamWriter writer = new(stream, Encoding.UTF8) { AutoFlush = true };

            CoreSystem.Log("[TCP][Command] Sending...");

            // send command and termination packet
            await writer.WriteLineAsync(command + Termination);

            CoreSystem.Log("[TCP][Command] Waiting for response...", writeLine: false);

            bool resultReceived = false;
            string message = "";
            try
            {
                // Wait for data from client
                while (client.Connected && !token.IsCancellationRequested && !resultReceived)
                {
                    if (stream.DataAvailable)
                    {
                        // When data received, get it
                        // If its a message, add it to our total message
                        // if its a termination packet, collate the message and invoke the event
                        string line = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            CoreSystem.Log($"\n[TCP][Command] {rDevice.DisplayName}: {line}", 1);
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

                        await Task.Delay(250, token);
                    }
                }
            }
            catch (Exception ex)
            {
                CoreSystem.LogError("[TCP][Command] " + ex.ToString());
            }
            finally
            {
                client.Close();
            }
            CoreSystem.Log("[TCP][Command] Closed");

        }


        public async Task StartWSLLoop ()
        {
            var token = wslCTS.Token;

            await CoreSystem.Speak(CachedTTS.Boot_WaitForTerminal);

            TcpClient client = new TcpClient();
            NetworkStream stream;

            string wslIP = FindWSLInternalIP();
            if (!string.IsNullOrEmpty(wslIP))
            {
                terminalIP = wslIP;
            }

            while (!client.Connected)
            {
                if (token.IsCancellationRequested)
                {
                    client.Close();
                    return;
                }

                var result = TryWSLConnect(client);
                if (!result)
                {
                    Thread.Sleep(100);
                }
            }
            string con = $"{terminalIP}:{wslTerminalPort}";
            //CoreSystem.Log($"[Whisper] Connected at {con}", $"{con}", ConsoleColor.Yellow, 1);
            stream = client.GetStream();

            await CoreSystem.Speak(CachedTTS.Terminal_OnConnected);

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    stream.Close();
                    stream.Dispose();
                    client.Close();
                    return;
                }
                CheckSocket(stream);
                await Task.Delay(50);
            }
        }

        public void StopWSLLoop ()
        {
            wslCTS.Cancel();
        }

        public bool TryWSLConnect(TcpClient client)
        {
            try
            {
                client.Connect(terminalIP, wslTerminalPort);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public void CheckSocket(NetworkStream stream)
        {
            try
            {
                if (stream.DataAvailable)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    DeserializeWSLData(buffer);
                }
            }
            catch (Exception ex)
            {
                CoreSystem.LogError($"[Socket] {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static string FindWSLInternalIP()
        {
            string distroName = "Ubuntu";  // Replace this with your WSL distro name

            Process process = new();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "powershell.exe",
                Arguments = $"wsl -d {distroName} -- ip addr show eth0 | findstr inet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            process.StartInfo = startInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            CoreSystem.Log("Standard Output:", 2);
            CoreSystem.Log(output, 2);

            CoreSystem.Log("Standard Error:", 2);
            CoreSystem.Log(error, 2);

            // Assuming the standard output looks something like "inet 172.28.31.175/20 brd ..."
            string ipAddress = "";
            try
            {
                ipAddress = output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].Split('/')[0];
            }
            catch (Exception e)
            {
                CoreSystem.Log($"[WSL]: {e.Message}", e.Message, ConsoleColor.Red);
            }

            //CoreSystem.Log($"[Whisper] Terminal IP: {ipAddress}\n", ipAddress, ConsoleColor.Yellow);
            return ipAddress;
        }

        public void DeserializeWSLData(byte[] serializedData)
        {
            string serializedString = Encoding.UTF8.GetString(serializedData);
            string[] identifierSplit = serializedString.Split("**");
            string[] messageSplit = identifierSplit[1].Split(">>");

            InputHandler.Instance.RegisterStandardInput(messageSplit[0], "Whisper");
        }
    }
}
