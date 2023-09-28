using System;
using System;
using System.Net;
using System.Net.NetworkInformation;


namespace IronWhisperReceiver.Core.Networking
{
    public class NetworkUtilities
    {
        public static async Task PingAsync(string address, Action<List<NetworkDevice>> onComplete)
        {
            List<NetworkDevice> devices = new();
            using (Ping ping = new())
            {
                try
                {
                    PingReply reply = ping.Send(address, 100);

                    if (reply.Status == IPStatus.Success)
                    {
                        string hostname = GetHostName(address);
                        var device = new NetworkDevice()
                        {
                            Address = address,
                            HostName = hostname
                        };
                        devices.Add(device);

                        CoreSystem.Log($"[Ping] (A) {address} - (H) {GetHostName(address)}", 2);
                    }
                }
                catch (PingException e)
                {
                    CoreSystem.Log($"[Ping] Exception ({address}): {e.Message}", 2);
                }
                catch (Exception e)
                {
                    CoreSystem.Log("[Ping] Exception: " + e.Message, 2);
                }
            }
            onComplete?.Invoke(devices);
        }

        public static Dictionary<IPAddress, IPAddress> GetNetworkInterfaces()
        {
            var results = new Dictionary<IPAddress, IPAddress>();

            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // Skip loopback and virtual network interfaces
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                UnicastIPAddressInformationCollection unicastAddresses = ipProperties.UnicastAddresses;

                foreach (UnicastIPAddressInformation unicastAddress in unicastAddresses)
                {
                    if (unicastAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        results.Add(unicastAddress.Address, unicastAddress.IPv4Mask);
                    }
                }
            }
            return results;
        }

        public static string GetHostName(string iAddress)
        {
            string r = "";
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(iAddress);
                r = hostEntry.HostName;
            }
            catch
            {
                r = "";
            }
            return r;
        }

        // Keeping this here just for utilities sake.
        public static string GetMacAddress(string ipAddress)
        {
            System.Diagnostics.Process pProcess = new();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a " + ipAddress;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();

            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                string macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                 + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                 + "-" + substrings[7] + "-"
                 + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "";
            }
        }
    }
}
