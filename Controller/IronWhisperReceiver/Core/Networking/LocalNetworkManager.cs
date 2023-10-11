using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Util = IronWhisper_CentralController.Core.Networking.NetworkUtilities;

namespace IronWhisper_CentralController.Core.Networking
{
    public class LocalNetworkManager
    {
        public static LocalNetworkManager Instance;
        public List<NetworkDevice> devices;

        public LocalNetworkManager ()
        {
            Instance = this;
        }

        public async Task PingNetworkAsync()
        {
            Console.WriteLine("[Network] Locating devices", 1);

            devices = new List<NetworkDevice>();

            List<Task> tasks = new();
            var interfaces = Util.GetNetworkInterfaces();
            List<NetworkDevice> nDevices = new();
            foreach (var kvp in interfaces)
            {
                for (int i = 1; i <= 255; i++)
                {
                    int currentI = i;
                    var rawSubnet = kvp.Key.ToString();
                    string subnet = rawSubnet.Substring(0, rawSubnet.LastIndexOf('.'));

                    tasks.Add(Task.Run(() => Util.PingAsync($"{subnet}.{currentI}", x => nDevices.AddRange(x))));
                }
            }

            await Task.WhenAll(tasks);

            CoreSystem.Log($"[Network] Finished. Interfaces, {interfaces.Count}, Devices: {nDevices.Count}", 1);
            devices = new List<NetworkDevice>();
            devices.AddRange(nDevices);

            int x = 0; 
            foreach (var device in devices)
            {
                if (Registry.RegistryCore.Instance.IsRegisteredNetworkDevice(hostname: device.HostName))
                {
                    x++;
                }
            }
            CoreSystem.Log($"[Network] Registered devices: {x}", 1);

            // Log the output
            CoreSystem.Log($"[Network] Device Data: \n{JsonConvert.SerializeObject(devices, Formatting.Indented)}", 2);
        }

        
    }
}
