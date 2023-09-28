package com.draconinteractive.ironwhisperid;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.net.ConnectivityManager;
import android.net.Network;
import android.net.NetworkCapabilities;
import android.net.NetworkRequest;
import android.os.IBinder;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.os.Build;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketTimeoutException;
import java.util.concurrent.atomic.AtomicBoolean;

import android.os.Handler;
import android.os.Looper;

import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;


public class UDPSenderService extends Service {

    private final Handler handler = new Handler(Looper.getMainLooper());
    private final int interval = 30 * 1000; // seconds to milliseconds
    private final AtomicBoolean isOnlineUpdateRunning = new AtomicBoolean(false);

    @Override
    public IBinder onBind(Intent intent)
    {
        return null;
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    @Override
    public void onCreate() {
        ConnectivityManager connectivityManager =
                (ConnectivityManager) getSystemService(Context.CONNECTIVITY_SERVICE);

        ConnectivityManager.NetworkCallback networkCallback = new ConnectivityManager.NetworkCallback() {
            @Override
            public void onAvailable(@NonNull Network network) {
                super.onAvailable(network);
                handler.removeCallbacks(onlineUpdate);
                handler.post(onlineUpdate);
            }
        };

        NetworkRequest networkRequest = new NetworkRequest.Builder()
                .addTransportType(NetworkCapabilities.TRANSPORT_CELLULAR)
                .addTransportType(NetworkCapabilities.TRANSPORT_WIFI)
                .build();

        connectivityManager.registerNetworkCallback(networkRequest, networkCallback);
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        createNotificationChannel();

        // Create the Notification
        Notification notification = new Notification.Builder(this, "IronWhisperIDNotiChannel")
                .setContentTitle("IronWhisper ID Service")
                .setContentText("ID broadcast is running")
                .setSmallIcon(R.drawable.ic_launcher_foreground)  // Replace with your own icon
                .build();

        // Start the service in foreground
        startForeground(1, notification);

        // Schedule the first execution
        handler.post(onlineUpdate);

        return START_STICKY;
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel serviceChannel = new NotificationChannel(
                    "IronWhisperIDNotiChannel",
                    "IronWhisper ID",
                    NotificationManager.IMPORTANCE_DEFAULT
            );

            NotificationManager manager = getSystemService(NotificationManager.class);
            manager.createNotificationChannel(serviceChannel);
        }
    }

    // This is the interval update to keep the server aware of the devices online status. This should have a long interval, such as ~2 minutes.
    // TODO: Implement a singular update to occur on significant changes, like network connect or boot.
    // This singular update should interval in short bursts until it receives confirmation of registration from server.
    private final Runnable onlineUpdate = new Runnable() {
        @Override
        public void run() {
            if (isOnlineUpdateRunning.compareAndSet(false, true))
            {
                try
                {
                    //DeviceIDUtil.sendUDPBroadcast(DeviceIDUtil.getCustomDeviceId(UDPSenderService.this), 9876);
                }
                finally {
                    isOnlineUpdateRunning.set(false);
                }
            }
            handler.postDelayed(this, interval);
        }
    };

    private volatile boolean isListening = false;
    private DatagramSocket listeningSocket;

    private void startListening(int port, long timeout) {
        isListening = true;
        new Thread(new Runnable() {
            @Override
            public void run() {
                try {
                    listeningSocket = new DatagramSocket(port);
                    listeningSocket.setSoTimeout((int) timeout); // Set the socket timeout
                    byte[] buffer = new byte[1024];
                    DatagramPacket packet = new DatagramPacket(buffer, buffer.length);

                    while (isListening) {
                        try {
                            listeningSocket.receive(packet);
                            String receivedData = new String(packet.getData(), 0, packet.getLength());
                            // Handle the received data here
                        } catch (SocketTimeoutException e) {
                            // Socket timed out, stop listening
                            break;
                        }
                    }

                } catch (Exception e) {
                    e.printStackTrace();
                } finally {
                    if (listeningSocket != null && !listeningSocket.isClosed()) {
                        listeningSocket.close();
                    }
                }
            }
        }).start();
    }

    private void stopListening() {
        isListening = false;
        if (listeningSocket != null && !listeningSocket.isClosed()) {
            listeningSocket.close();
        }
    }
}
