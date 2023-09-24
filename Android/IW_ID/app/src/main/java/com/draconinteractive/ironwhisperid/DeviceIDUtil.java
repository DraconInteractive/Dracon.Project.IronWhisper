package com.draconinteractive.ironwhisperid;

import android.content.Context;
import android.content.SharedPreferences;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.util.Log;

import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.Inet4Address;
import java.net.InetAddress;
import java.net.NetworkInterface;
import java.net.SocketException;
import java.util.Enumeration;
import java.util.UUID;

public class DeviceIDUtil {

    private static final String TAG = "DeviceIDUtil";

    public static String getCustomDeviceId(Context context) {
        SharedPreferences sharedPreferences = context.getSharedPreferences("DevicePrefs", Context.MODE_PRIVATE);
        String deviceId = sharedPreferences.getString("custom_device_id", null);
        if (deviceId == null) {
            deviceId = UUID.randomUUID().toString();
            saveCustomDeviceId(context, deviceId);
        }
        return deviceId;
    }

    public static void saveCustomDeviceId(Context context, String deviceId) {
        SharedPreferences sharedPreferences = context.getSharedPreferences("DevicePrefs", Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        editor.putString("custom_device_id", deviceId);
        editor.apply();
    }

    public static void sendUDPBroadcast(String deviceId, Integer port) {
        new Thread(new Runnable() {
            @Override
            public void run() {
                try {
                    DatagramSocket socket = new DatagramSocket();
                    byte[] buffer = deviceId.getBytes();
                    InetAddress address = InetAddress.getByName("255.255.255.255");
                    DatagramPacket packet = new DatagramPacket(buffer, buffer.length, address, port);
                    socket.send(packet);
                    socket.close();
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        }).start();
    }
}


