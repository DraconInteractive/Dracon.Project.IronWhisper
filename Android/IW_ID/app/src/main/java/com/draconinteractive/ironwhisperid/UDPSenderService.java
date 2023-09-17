package com.draconinteractive.ironwhisperid;
import android.app.Service;
import android.content.Intent;
import android.os.IBinder;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.os.Build;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import android.os.Handler;
import android.os.Looper;


public class UDPSenderService extends Service {

    private final Handler handler = new Handler(Looper.getMainLooper());
    private final int interval = 15000; // milliseconds

    @Override
    public IBinder onBind(Intent intent)
    {
        return null;
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        createNotificationChannel();

        // Create the Notification
        Notification notification = new Notification.Builder(this, "yourChannelId")
                .setContentTitle("My Foreground Service")
                .setContentText("This is running in the background...")
                .setSmallIcon(R.drawable.ic_launcher_foreground)  // Replace with your own icon
                .build();

        // Start the service in foreground
        startForeground(1, notification);

        // Schedule the first execution
        handler.post(runnableCode);

        return START_STICKY;
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel serviceChannel = new NotificationChannel(
                    "yourChannelId",
                    "Your Channel",
                    NotificationManager.IMPORTANCE_DEFAULT
            );

            NotificationManager manager = getSystemService(NotificationManager.class);
            manager.createNotificationChannel(serviceChannel);
        }
    }

    private final Runnable runnableCode = new Runnable() {
        @Override
        public void run() {
            // Your existing UDP sending code
            new Thread(new Runnable() {
                @Override
                public void run() {
                    try {
                        DatagramSocket socket = new DatagramSocket();
                        byte[] buffer = "DeviceID_1".getBytes();
                        InetAddress address = InetAddress.getByName("255.255.255.255");
                        DatagramPacket packet = new DatagramPacket(buffer, buffer.length, address, 9876);
                        socket.send(packet);
                        socket.close();
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                }
            }).start();

            // Reschedule the runnable
            handler.postDelayed(runnableCode, interval);
        }
    };
}
