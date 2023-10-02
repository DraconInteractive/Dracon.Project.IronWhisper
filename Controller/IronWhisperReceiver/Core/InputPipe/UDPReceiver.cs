using IronWhisper_CentralController.Core.Registry;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.InputPipe
{
    public class UDPReceiver
    {
        public static UDPReceiver Instance;

        private const int listenPort = 9876;
        private static CancellationTokenSource cts;
        public static void StartListening()
        {
            cts = new CancellationTokenSource();
            StartListeningAsync(cts.Token);
        }

        public static void StopListening()
        {
            cts.Cancel();
        }

        private static async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            UdpClient listener = new(listenPort);
            IPEndPoint groupEP = new(IPAddress.Any, listenPort);

            CoreSystem.Log($"[UDP_ID] Begun listening for device updates on port {listenPort}...", 1);

            try
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        CoreSystem.Log("[UDP_ID] Stopping UDP receiver...", 1);
                        break;
                    }
                    UdpReceiveResult result = await listener.ReceiveAsync();
                    byte[] bytes = result.Buffer;
                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                    //CoreSystem.Log($"[UDP_ID] {result.RemoteEndPoint} : {message}", 2);
                    RegistryCore.Instance.UpdateNetworkDevice(message, result.RemoteEndPoint.Address.ToString(), () =>
                    {
                        // Send response confirming ID received. 
                    });
                }
            }
            catch (Exception e)
            {
                CoreSystem.Log($"[UDP_ID] Exception: {e.Message}");
            }
            finally
            {
                listener.Close();
            }
            CoreSystem.Log("[UDP_ID] Stopped listening", 1);
        }

    }
}

