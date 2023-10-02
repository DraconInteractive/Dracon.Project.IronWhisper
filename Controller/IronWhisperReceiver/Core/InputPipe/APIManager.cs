using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IronWhisper_CentralController.Core.InputPipe
{
    public class APIManager
    {
        public static APIManager Instance;
        private readonly HttpClient _httpClient;

        public APIManager() : base()
        {
            _httpClient = new HttpClient();
            Instance = this;
        }

        public APIManager(string baseAddress) : base()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
        }

        // Fetch raw string data from a REST API
        public async Task<string> GetRawAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        // Fetch JSON data from a REST API and deserialize it to an object
        public async Task<T> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        // Send data as JSON to a REST API
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, httpContent);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResponse>(content);
        }

        // Download a file from a URL and save it to a local path
        public async Task DownloadFileAsync(string url, string localPath)
        {
            using (var response = await _httpClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();

                using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
        }

        // Add more API methods as needed (PUT, DELETE, etc.)
    }
}

