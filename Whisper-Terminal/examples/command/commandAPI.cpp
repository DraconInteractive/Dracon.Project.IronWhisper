#include "CommandAPI.h"
#include <iostream>
#include <curl/curl.h>
#include <iomanip>
#include <cctype>
#include <sstream>
#include <string>

// Callback function for writing received data
size_t WriteCallback(void* contents, size_t size, size_t nmemb, void* userp) {
    ((std::string*)userp)->append((char*)contents, size * nmemb);
    return size * nmemb;
}

std::string URLEncode(const std::string& value) {
    std::ostringstream escaped;
    escaped.fill('0');
    escaped << std::hex;

    for (char c : value) {
        // Keep alphanumeric and other accepted characters intact
        if (std::isalnum(c) || c == '-' || c == '_' || c == '.' || c == '~') {
            escaped << c;
            continue;
        }

        // Any other characters are percent-encoded
        escaped << std::uppercase;
        escaped << '%' << std::setw(2) << int((unsigned char)c);
        escaped << std::nouppercase;
    }

    return escaped.str();
}

void CommandAPI::SendGetRequest(const std::string& url, const std::string& message) {
    std::string fullUrl = url + URLEncode(message);
    SendGetRequest(fullUrl);
}

// Implementation of SendGetRequest
void CommandAPI::SendGetRequest(const std::string& url) {
    CURL* curl;
    CURLcode res;
    std::string readBuffer;
    long response_code = 0; // Variable to hold the response code

    curl_global_init(CURL_GLOBAL_DEFAULT);
    curl = curl_easy_init();

    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);
        
        // Disable SSL certificate verification
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0L);
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 0L);

        res = curl_easy_perform(curl); // Perform the request

        if (res != CURLE_OK) {
            std::cerr << "curl_easy_perform() failed: " << curl_easy_strerror(res) << std::endl;
        }
        else {
            curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &response_code);
            if (response_code == 200)
            {
                std::cout << "[Rest] [200] " << readBuffer << std::endl;

            }
            else if (response_code == 502)
            {
                std::cout << "[Rest] [502]" << std::endl;
            } 
            else 
            {
                std::cerr << "[Rest] [" << response_code << "]" << std::endl;
            }
        }

        curl_easy_cleanup(curl); // Cleanup
    }

    curl_global_cleanup();
}
