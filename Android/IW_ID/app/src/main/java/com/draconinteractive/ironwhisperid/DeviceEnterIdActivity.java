package com.draconinteractive.ironwhisperid;

import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

public class DeviceEnterIdActivity extends AppCompatActivity {

    private EditText edtDeviceId;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_device_id);

        edtDeviceId = findViewById(R.id.edtDeviceId);
        Button btnSave = findViewById(R.id.btnSave);

        String currentDeviceId = DeviceUtilities.getCustomDeviceId(this);
        edtDeviceId.setText(currentDeviceId);

        btnSave.setOnClickListener(v -> {
            String deviceId = edtDeviceId.getText().toString().trim();
            DeviceUtilities.saveCustomDeviceId(this, deviceId);
            Toast.makeText(this, "Device ID Saved", Toast.LENGTH_SHORT).show();
        });

        //Intent serviceIntent = new Intent(this, UDPSenderService.class);
        //startForegroundService(serviceIntent);
    }

    @Override
    protected void onStop() {
        super.onStop();

        DeviceUtilities.startSenderService(this);
    }
}

