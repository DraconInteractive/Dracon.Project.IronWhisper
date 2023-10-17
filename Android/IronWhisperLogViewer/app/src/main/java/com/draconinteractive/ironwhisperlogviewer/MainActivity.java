// Import necessary classes
package com.draconinteractive.ironwhisperlogviewer;

import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;

import androidx.appcompat.app.AppCompatActivity;
import android.content.pm.ActivityInfo;
import android.os.Bundle;
import android.os.Handler;
import android.widget.TextView;
import java.io.IOException;
import java.net.HttpURLConnection;
import java.net.URL;
import java.security.KeyManagementException;
import java.security.NoSuchAlgorithmException;
import java.util.Scanner;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import org.json.JSONArray;
import org.json.JSONException;
import android.util.Log;

// Define the main activity class
public class MainActivity extends AppCompatActivity {

    private TextView textViewResult;  // TextView to display result
    private Handler periodicHandler = new Handler();  // Handler to manage periodic updates
    private Runnable periodicUpdate;  // Runnable for periodic updates
    private ExecutorService executorService = Executors.newSingleThreadExecutor();  // ExecutorService for background tasks

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // Set orientation to landscape
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE);

        // Initialize TextView
        textViewResult = findViewById(R.id.textView_result);

        // Fetch data initially
        fetchData();

        // Define periodic update Runnable
        periodicUpdate = new Runnable() {
            @Override
            public void run() {
                fetchData();  // Fetch data
                periodicHandler.postDelayed(this, 8000);  // Schedule next update in 1 minute
            }
        };

        // Post the periodic update Runnable to the handler
        periodicHandler.post(periodicUpdate);
    }

    @Override
    protected void onPause() {
        super.onPause();
        // Remove any pending updates when activity is paused
        periodicHandler.removeCallbacks(periodicUpdate);
    }

    @Override
    protected void onResume() {
        super.onResume();
        // Re-post the periodic update Runnable when activity is resumed
        periodicHandler.post(periodicUpdate);
    }

    // Method to trigger data fetching
    private void fetchData() {
        // Execute the FetchDataTask using the ExecutorService
        executorService.execute(new FetchDataTask("https://connect.draconai.com.au/log"));
    }

    // Define FetchDataTask as a Runnable for executing network operations
    private class FetchDataTask implements Runnable {
        private String urlString;  // URL string

        // Constructor to initialize URL string
        FetchDataTask(String urlString) {
            this.urlString = urlString;
        }

        @Override
        public void run() {
            // Fetch data from URL
            final String result = fetchFromUrl(urlString);

            // Schedule a UI update on the main thread
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    // Process and display the result
                    processResult(result);
                }
            });
        }

        // Method to execute network operation
        private String fetchFromUrl(String urlString) {
            try {

                // Log the start of the HTTP call
                Log.d("FetchDataTask", "Starting HTTP call to: " + urlString);

                URL url = new URL(urlString);
                HttpsURLConnection connection = (HttpsURLConnection) url.openConnection();

                // Create a TrustManager that trusts all certificates
                TrustManager[] trustAllCerts = new TrustManager[]{
                        new X509TrustManager() {
                            public java.security.cert.X509Certificate[] getAcceptedIssuers() {
                                return null;
                            }
                            public void checkClientTrusted(
                                    java.security.cert.X509Certificate[] certs, String authType) {
                            }
                            public void checkServerTrusted(
                                    java.security.cert.X509Certificate[] certs, String authType) {
                            }
                        }
                };

                // Install the all-trusting TrustManager to the HttpsURLConnection
                SSLContext sc = SSLContext.getInstance("SSL");
                sc.init(null, trustAllCerts, new java.security.SecureRandom());
                connection.setSSLSocketFactory(sc.getSocketFactory());

                // Create all-trusting HostnameVerifier
                connection.setHostnameVerifier(new javax.net.ssl.HostnameVerifier() {
                    public boolean verify(String hostname, javax.net.ssl.SSLSession sslSession) {
                        return true;
                    }
                });

                connection.setRequestMethod("GET");
                connection.connect();

                // Read response using Scanner
                Scanner scanner = new Scanner(connection.getInputStream());
                StringBuilder result = new StringBuilder();
                while (scanner.hasNext()) {
                    result.append(scanner.nextLine());
                }
                Log.d("FetchDataTask", "Received result from: " + urlString);

                // Return the result as a string
                return result.toString();

            } catch (IOException e) {
                e.printStackTrace();
                Log.e("FetchDataTask", "1 Failed to fetch data from: " + urlString, e);  // Log error with exception
                return null;  // Return null on exception
            } catch (NoSuchAlgorithmException e) {
                e.printStackTrace();
                Log.e("FetchDataTask", "2 Failed to fetch data from: " + urlString, e);  // Log error with exception
                return null;  // Return null on exception
            } catch (KeyManagementException e) {
                e.printStackTrace();
                Log.e("FetchDataTask", "3 Failed to fetch data from: " + urlString, e);  // Log error with exception
                return null;  // Return null on exception
            }
        }
    }

    // Method to process network result and update UI
    private void processResult(String result) {
        if (result != null) {  // Check if result is not null
            try {
                // Parse result as JSON array
                JSONArray jsonArray = new JSONArray(result);
                StringBuilder displayText = new StringBuilder();

                // Build display text from JSON array
                for (int i = 0; i < jsonArray.length(); i++) {
                    displayText.append(jsonArray.getString(i)).append("\n\n");
                }

                // Set the display text to the TextView
                textViewResult.setText(displayText.toString());

            } catch (JSONException e) {
                e.printStackTrace();  // Print stack trace on exception
            }
        }
    }
}
