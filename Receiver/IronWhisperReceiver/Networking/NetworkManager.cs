using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Networking
{
    public class NetworkDevice
    {
        public string Address;
        public string HostName;
        public string MACAddress;
    }
    public class NetworkManager
    {
        public static NetworkManager Instance;
        public List<NetworkDevice> devices;

        public NetworkManager ()
        {
            Instance = this;
        }

        public async Task PingNetworkAsync()
        {
            if (Core.Verbosity >= 1)
            {
                Console.WriteLine("Locating network devices...");
            }

            devices = new List<NetworkDevice>();

            List<Task> tasks = new List<Task>();
            var interfaces = GetNetworkInterfaces();
            foreach (var kvp in interfaces)
            {
                for (int i = 1; i <= 255; i++)
                {
                    int currentI = i;
                    var rawSubnet = kvp.Key.ToString();
                    string subnet = rawSubnet.Substring(0, rawSubnet.LastIndexOf('.'));

                    tasks.Add(Task.Run(() => PingAsync($"{subnet}.{currentI}")));
                }
            }

            await Task.WhenAll(tasks);

            if (Core.Verbosity >= 1)
            {
                Console.WriteLine("Finished locating network devices.");
            }
        }

        public async Task PingAsync (string address)
        {
            using (Ping ping = new Ping())
            {
                try
                {
                    PingReply reply = ping.Send(address, 100);

                    if (reply.Status == IPStatus.Success)
                    {
                        string macAddress = GetMacAddress(address);
                        string hostname = GetHostName(address);
                        var device = new NetworkDevice()
                        {
                            Address = address,
                            HostName = hostname,
                            MACAddress = macAddress
                        };
                        devices.Add(device);

                        Core.Log($"(A) {address} - (M) {macAddress} - (H) {GetHostName(address)}", 1);
                    }
                }
                catch (PingException e)
                {
                    Core.Log($"Ping exception ({address}): {e.Message}", 2);
                }
                catch (Exception e)
                {
                    Core.Log("General network exception: " + e.Message, 2);
                }
            }
        }
        
        static Dictionary<IPAddress, IPAddress> GetNetworkInterfaces ()
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
                        Core.Log($"Interface: {unicastAddress.Address.ToString()} || {unicastAddress.IPv4Mask.ToString()}", 1);
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
                r = "Unable to retrieve hostname";
            }
            return r;
        }

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

        public static NetworkDevice GetDeviceDetails(string ipAddress)
        {
            string hostName = GetHostName(ipAddress);
            string mac = GetMacAddress(ipAddress);
            return new NetworkDevice()
            {
                Address = ipAddress,
                HostName = hostName,
                MACAddress = mac
            };
        }
    }
}
