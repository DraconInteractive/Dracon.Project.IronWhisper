package com.draconinteractive.ironwhisperid;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import java.util.UUID;

public class DeviceEnterIdActivity extends AppCompatActivity {

    private EditText edtDeviceId;
    private Button btnSave;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_device_id);

        edtDeviceId = findViewById(R.id.edtDeviceId);
        btnSave = findViewById(R.id.btnSave);

        String currentDeviceId = DeviceIDUtil.getCustomDeviceId(this);
        if (currentDeviceId != null) {
            edtDeviceId.setText(currentDeviceId);
        }

        btnSave.setOnClickListener(v -> {
            String deviceId = edtDeviceId.getText().toString().trim();
            DeviceIDUtil.saveCustomDeviceId(this, deviceId);
            Toast.makeText(this, "Device ID Saved", Toast.LENGTH_SHORT).show();
        });

        Intent serviceIntent = new Intent(this, UDPSenderService.class);
        startService(serviceIntent);
    }
}

