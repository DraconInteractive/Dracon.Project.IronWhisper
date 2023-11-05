using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IronWhisper_CentralController.Core.Networking
{
    public class APIManager : CoreManager
    {
        public static APIManager Instance;
        private static HttpClient _httpClient;

        public static string ttsURL { get; private set; }
        public static string lmURL { get; private set; }


        public APIManager()
        {
            _httpClient = new HttpClient();
            Instance = this;
            ttsURL = "http://localhost:59125/api/tts";
            lmURL = "http://localhost:8756/v1/chat/completions";
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<string> GetAsync(APIRequestData request)
        {
            string url = request.URL;

            if (request.Parameters != null && request.Parameters.Count > 0)
            {
                url += "?";
                foreach (var header in request.Headers)
                {
                    url += $"{header.Key}={header.Value}";
                }
            }
            
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                // Add headers to the request if any
                if (request.Headers != null && request.Headers.Count > 0)
                {
                    foreach (var header in request.Headers)
                    {
                        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                
                // Send the request
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                // Read and return the response content as a string
                return await response.Content.ReadAsStringAsync();
            }
        }

        // Fetch raw string data from a REST API
        public async Task<string> GetRawAsync(string url, Dictionary<string, string> headers = null)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                // Add headers to the request if any
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Send the request
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                // Read and return the response content as a string
                return await response.Content.ReadAsStringAsync();
            }
        }

        // Fetch JSON data from a REST API and deserialize it to an object
        public async Task<T> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<string> PostAsync(APIRequestData request)
        {
            TimeSpan originalTimeout = _httpClient.Timeout;
            if (request.Timeout != -1)
            {
                _httpClient = new HttpClient();
                _httpClient.Timeout = TimeSpan.FromSeconds(request.Timeout);
            }
            string url = request.URL;
            try
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    // Add headers to the request if any
                    if (request.Headers != null && request.Headers.Count > 0)
                    {
                        foreach (var header in request.Headers)
                        {
                            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }

                    // Add data to the request if any
                    if (!string.IsNullOrEmpty(request.Data))
                    {
                        CoreSystem.Log("Data is valid");
                        requestMessage.Content = new StringContent(request.Data, Encoding.UTF8, "application/json");
                    }

                    // If there are Parameters, they are typically sent as FormUrlEncodedContent for POST
                    else if (request.Parameters != null && request.Parameters.Count > 0)
                    {
                        requestMessage.Content = new FormUrlEncodedContent(request.Parameters);
                    }

                    // Send the request
                    var response = await _httpClient.SendAsync(requestMessage);
                    response.EnsureSuccessStatusCode();

                    // Read and return the response content as a string
                    return await response.Content.ReadAsStringAsync();
                }
            } catch (Exception ex)
            {
                CoreSystem.LogError("Invalid Post Async! \n" + ex.Message);
                return "";
            }
            finally
            {
                if (request.Timeout != -1)
                {
                    _httpClient = new HttpClient();
                    _httpClient.Timeout = originalTimeout;
                }
            }
            
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
            byte[] audioData = Array.Empty<byte>();
            try
            {
                var response = await _httpClient.GetAsync(finalURL);
                response.EnsureSuccessStatusCode();
                audioData = await response.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                CoreSystem.LogError("TTS Failed. Disabling system.");
                CoreSystem.Config.UseMimic3 = false;
            }
            

            return audioData;
        }

        public async Task<bool> IsOnline(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                return true;
            }
            catch (TaskCanceledException)  // This exception is thrown on a timeout
            {
                Console.WriteLine("GET FAIL - Timeout");
                return false;  
            }
            catch (SocketException)
            {
                Console.WriteLine("GET FAIL - Socket");
                return false;
            }
            catch
            {
                Console.WriteLine("GET FAIL - Unknown");
                return false;
            }
        }
    }

    public class APIRequestData
    {
        public string URL { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public string Data;
        public int Timeout;
        public APIRequestData (string url)
        {
            URL = url;
            Headers = new Dictionary<string, string>();
            Parameters = new Dictionary<string, string>();
            Timeout = -1;
        }

        public APIRequestData ContentType (string type)
        {
            if (Headers.ContainsKey("Content-Type"))
            {
                Headers["Content-Type"] = type;
            }
            else
            {
                Headers.Add("Content-Type", type);
            }
            return this;
        }

        public APIRequestData TokenAuth (string token)
        {
            if (Headers.ContainsKey("Authorization"))
            {
                Headers["Authorization"] = "Bearer " + token;
            }
            else
            {
                Headers.Add("Authorization", "Bearer " + token);
            }
            return this;
        }

        public APIRequestData SetParameter(string key, string value)
        {
            if (Headers.ContainsKey(key))
            {
                Headers[key] = System.Net.WebUtility.UrlEncode(value);
            }
            else
            {
                Headers.Add(key, System.Net.WebUtility.UrlEncode(value));
            }
            return this;
        }

        public APIRequestData SetData (string data)
        {
            Data = data;
            return this;
        }

        public APIRequestData SetData (object data)
        {
            Data = JsonConvert.SerializeObject(data);
            return this;
        }

        public APIRequestData SetTimeout (int timeout)
        {
            Timeout = timeout;
            return this;
        }
    }

    public class OpenAIMessage
    {
        public string role;
        public string content;
    }

    public class OpenAIRequestPacket
    {
        public List<OpenAIMessage> messages;
        public List<string> stop;
        public float temperature;
        public int max_tokens;
        public bool stream;

        public OpenAIRequestPacket()
        {
            messages = new List<OpenAIMessage>();
            stop = new List<string>();
        }

        public OpenAIRequestPacket AddMessage (string role, string content)
        {
            messages.Add(new OpenAIMessage { role = role, content = content });
            return this;
        }

        public OpenAIRequestPacket ModifyMessageContent (int index, string content)
        {
            messages[index].content = content;
            return this;
        }

        public OpenAIRequestPacket SetDefault ()
        {
            stop = new List<string>() { "### Instruction:" };
            temperature = 0.7f;
            max_tokens = -1;
            stream = false;
            messages = new List<OpenAIMessage>()
            {
                new OpenAIMessage()
                {
                    role = "user",
                    content = ""
                }
            };
            return this;
        }
    }

    public class OpenAIResponsePacket
    {
        public string id;
        public int created;
        public string model;
        public class Choice
        {
            public int index;
            public OpenAIMessage message;
            public string finish_reason;
        }
        public List<Choice> choices;
        public class Usage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }
        public Usage usage;
    }
}



