using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;

namespace IW_ID_Service
{
    public partial class BroadcastService : ServiceBase
    {
        private Timer _timer;
        private UdpClient _udpClient;
        private int BroadcastInterval = 45000; // For example, 10 seconds
        private const string BroadcastAddress = "255.255.255.255"; // Broadcast address
        private const int Port = 9876; // Desired port number
        private string BroadcastPacket;

        public BroadcastService()
        {
            InitializeComponent();
            _timer = new Timer(BroadcastInterval);
            _timer.Elapsed += OnTimerElapsed;
            EventLog.WriteEntry("IronWhisper", "Starting IW Broadcast service, v1.0.1");
            BroadcastInterval = int.Parse(ConfigurationManager.AppSettings["BroadcastInterval"]);
            BroadcastPacket = ConfigurationManager.AppSettings["DeviceID"];
        }

        protected override void OnStart(string[] args)
        {
            _udpClient = new UdpClient();
            _timer.Start();
        }

        protected override void OnStop()
        {
            _timer.Stop();
            _udpClient.Close();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            SendBroadcast();
        }

        private void SendBroadcast()
        {
            var data = Encoding.UTF8.GetBytes(BroadcastPacket);
            try
            {
                _udpClient.Send(data, data.Length, BroadcastAddress, Port);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("IronWhisper", ex.ToString(), EventLogEntryType.Error);
            }
        }
    }
}
