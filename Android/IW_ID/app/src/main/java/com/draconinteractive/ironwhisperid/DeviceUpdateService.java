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

import java.util.concurrent.atomic.AtomicBoolean;

import android.os.Handler;
import android.os.Looper;

import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;


public class DeviceUpdateService extends Service {

    private final Handler handler = new Handler(Looper.getMainLooper());
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
                    String id = DeviceUtilities.getCustomDeviceId(DeviceUpdateService.this);
                    String url = "https://connect.draconai.com.au/device";
                    //DeviceIDUtil.sendUDPBroadcast(id, 9876);
                    DeviceUtilities.sendHttpGetRequest(id, url);
                }
                finally {
                    isOnlineUpdateRunning.set(false);
                }
            }
            // seconds to milliseconds
            int interval = 30 * 1000;
            handler.postDelayed(this, interval);
        }
    };
}
