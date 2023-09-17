package com.draconinteractive.ironwhisperid;

import android.content.Intent;
import android.os.Bundle;

import androidx.appcompat.app.AppCompatActivity;

public class DebugActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // Optionally set a layout if you want a UI for this debug activity
        // setContentView(R.layout.activity_debug);

        // Start the service
        Intent serviceIntent = new Intent(this, UDPSenderService.class);
        startService(serviceIntent);

        // Optionally, you can also immediately finish this activity if you don't want it to display anything
        // finish();
    }
}
