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

        public static string ttsURL { get; private set; }

        public APIManager() : base()
        {
            _httpClient = new HttpClient();
            Instance = this;
            ttsURL = "http://localhost:59125/api/tts";
            _httpClient.Timeout = TimeSpan.FromSeconds(10);

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

        public async Task<byte[]> GetTTSAudioAsync(string text, string voice = "en_UK/apope_low", float noiseScale = 0.667f, float noise = 0.8f, float lengthScale = 1f, bool ssml = false)
        {
            //http://localhost:59125/api/tts?text=This%20is%20a%20speech%20synthesis%20test%21&voice=en_UK%2Fapope_low&noiseScale=0.667&noiseW=0.8&lengthScale=1&ssml=false
            text = text.Replace(" ", "%20");
            voice = voice.Replace("/", "%2F");
            string finalURL = $"{ttsURL}?text={text}&voice={voice}&noiseScale={noiseScale}&noise={noise}&lengthScale={lengthScale}&ssml={ssml}";
            // Send GET request and get the audio data
            var response = await _httpClient.GetAsync(finalURL);
            response.EnsureSuccessStatusCode();
            byte[] audioData = await response.Content.ReadAsByteArrayAsync();

            return audioData;
        }

        public async Task<bool> GetURLOnline(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                return true;
            }
            catch (TaskCanceledException)  // This exception is thrown on a timeout
            {
                Console.WriteLine("TTS FAIL");
                return false;  
            }
        }

        // Add more API methods as needed (PUT, DELETE, etc.)
    }
}

