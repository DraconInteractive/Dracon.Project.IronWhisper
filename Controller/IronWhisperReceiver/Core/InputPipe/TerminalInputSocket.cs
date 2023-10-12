using IronWhisper_CentralController.Core.Audio.TTS;
using IronWhisper_CentralController.Core.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace IronWhisper_CentralController.Core.InputPipe
{
    public class TerminalInputSocket
    {
        public static TerminalInputSocket Instance;
        string terminalIP = "172.28.31.175"; // Change this to the server's IP address
        int terminalPort = 31050;            // Change this to the server's port number
        string voicePrompt = "Okay,";

        readonly TcpClient client;
        NetworkStream stream;

        public TerminalInputSocket()
        {
            Instance = this;
            client = new TcpClient();

            string wslIP = FindWSLInternalIP();
            if (!string.IsNullOrEmpty(wslIP))
            {
                terminalIP = wslIP;
            }
        }

        public async Task RunLoop ()
        {
            CoreSystem.Log("[Socket] Connecting to WSL2 terminal...");
            await CoreSystem.Speak(CachedTTS.Boot_WaitForTerminal);

            Connect();
            StartStream();
            await CoreSystem.Speak(CachedTTS.Terminal_OnConnected);

            while (true)
            {
                CheckSocket();
                await Task.Delay(50);
            }
        }

        public void SetTerminal (string address)
        {
            if (client.Connected)
            {
                Disconnect();
            }
            Thread.Sleep(50);
            terminalIP = address;
        }

        public bool TryConnect()
        {
            try
            {
                client.Connect(terminalIP, terminalPort);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public void Connect()
        {
            while (!client.Connected)
            {
                var result = TryConnect();
                if (!result)
                {
                    Thread.Sleep(100);
                }
            }
            string con = $"{terminalIP}:{terminalPort}";
            CoreSystem.Log($"[Socket] Connected at {con}", $"{con}", ConsoleColor.Yellow, 1);
        }

        public void StartStream()
        {
            stream = client.GetStream();
        }

        public void CheckSocket()
        {
            // TODO - add | onto end of each command to allow for delayed commands to be separated
            try
            {
                if (stream.DataAvailable)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    DeserializeData(buffer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Socket] error occurred: {ex.Message}\n{ex.StackTrace}", $"{ex.Message}\n{ex.StackTrace}", ConsoleColor.Red);
            }
        }

        public void CloseStream()
        {
            stream.Close();
            stream.Dispose();
        }

        public void Disconnect()
        {
            CloseStream();
            client.Close();
        }

        public void DeserializeData(byte[] serializedData)
        {
            string serializedString = Encoding.UTF8.GetString(serializedData);
            string[] identifierSplit = serializedString.Split("**");
            string[] messageSplit = identifierSplit[1].Split(">>");

            InputHandler.Instance.RegisterInput(messageSplit[0]);
        }

        public static string FindWSLInternalIP ()
        {
            string distroName = "Ubuntu";  // Replace this with your WSL distro name

            Process process = new Process();
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
                CoreSystem.Log($"[TIS]: {e.Message}", e.Message, ConsoleColor.Red);
            }

            CoreSystem.Log($"[TIS] WSL2 IP Address: {ipAddress}\n", ipAddress, ConsoleColor.Yellow);
            return ipAddress;
        }
    }


}
