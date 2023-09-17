using IronWhisperReceiver.Registry;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class UDPListener
{
    public static UDPListener Instance;

    private const int listenPort = 9876;
    private static CancellationTokenSource cts;
    public static void StartListening ()
    {
        cts = new CancellationTokenSource();
        StartListeningAsync(cts.Token);
    }

    public static void StopListening ()
    {
        cts.Cancel();
    }

    private static async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        UdpClient listener = new(listenPort);
        IPEndPoint groupEP = new(IPAddress.Any, listenPort);

        try
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("[UDP] Stopping UDP receiver...");
                    break;
                }
                Console.WriteLine("[UDP] Waiting for a broadcast on port {0}...", listenPort);
                UdpReceiveResult result = await listener.ReceiveAsync();
                byte[] bytes = result.Buffer;
                string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                Console.WriteLine("[UDP] Received broadcast from {0} : {1}\n", result.RemoteEndPoint.ToString(), message);
                Registry.Instance.UpdateNetworkDevice(message, result.RemoteEndPoint.Address.ToString());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            listener.Close();
        }
    }

}
