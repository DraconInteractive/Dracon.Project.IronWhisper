﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Util = IronWhisperReceiver.Core.Networking.NetworkUtilities;

namespace IronWhisperReceiver.Core.Networking
{
    public class NetworkDevice
    {
        public string Address;
        public string HostName;
        public string MACAddress;
        public string Port;
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
            if (CoreSystem.Verbosity >= 1)
            {
                Console.WriteLine("[Network] Locating devices", 1);
            }

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
                if (Registry.RegistryCore.Instance.IsRegisteredNetworkDevice(device.HostName))
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
