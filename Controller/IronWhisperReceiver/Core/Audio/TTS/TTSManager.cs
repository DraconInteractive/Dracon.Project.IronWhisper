using IronWhisper_CentralController.Core.InputPipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace IronWhisper_CentralController.Core.Audio.TTS
{
    public enum CachedTTS
    {
        Labs_Boot_Initializing,
        Labs_Boot_LabsTTSIs,
        Labs_Boot_Mimic3Is,
        Labs_Boot_MinReq,
        Labs_Boot_RegistryIs,
        Labs_Boot_RestAPIIs,
        Labs_Boot_UDPIs,
        Labs_Boot_WhisperAIIs,
        Labs_Boot_TTSActivated,
        Labs_Boot_LaunchingTunnel,
        Labs_Status_Disabled,
        Labs_Status_Online,
        Labs_Status_Offline,
        Labs_Status_OnlineAndReady,
        Labs_System_Registry,
        Labs_System_RestAPI,
        Labs_System_WhisperSpeechDetection,
        Labs_Greeting_1,
        Labs_Greeting_2,
        Labs_Greeting_3,
        Labs_Greeting_4,
        Labs_Greeting_Morning,
        Labs_Greeting_Afternoon,
        Labs_Greeting_Evening,
        Labs_Greeting_Name1,
        Labs_Greeting_Name2,
    }

    public class TTSManager : CoreManager
    {
        // TODO add a cancellation token to currently running audio to track and cancel when a new audio is started
        public static TTSManager Instance;

        private static readonly Dictionary<CachedTTS, string> CachedAudio = new Dictionary<CachedTTS, string>();
        private static List<CachedTTS> Greetings;
        private static List<CachedTTS> TimeGreetings;

        private const string extension = ".mp3";

        private CancellationTokenSource cts;
        private bool audioPlaying;

        public TTSManager ()
        {
            Instance = this;
            Greetings = new List<CachedTTS>()
            {
                CachedTTS.Labs_Greeting_1,
                CachedTTS.Labs_Greeting_2,
                CachedTTS.Labs_Greeting_3,
                CachedTTS.Labs_Greeting_4,
                CachedTTS.Labs_Greeting_Name1,
                CachedTTS.Labs_Greeting_Name2
            };
            TimeGreetings = new List<CachedTTS>()
            {
                CachedTTS.Labs_Greeting_Morning,
                CachedTTS.Labs_Greeting_Afternoon,
                CachedTTS.Labs_Greeting_Evening
            };
        }

        public static bool InitializeCacheDictionary()
        {
            string audioFilesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/TTS Audio");
            bool foundAll = true;
            foreach (CachedTTS ttsEnum in Enum.GetValues(typeof(CachedTTS)))
            {
                string fileName = ttsEnum.ToString() + extension;
                string filePath = Path.Combine(audioFilesDirectory, fileName);

                if (File.Exists(filePath))
                {
                    CachedAudio[ttsEnum] = filePath;
                }
                else
                {
                    CoreSystem.LogError($"  Audio file not found for {ttsEnum}: {filePath}");
                    foundAll = false;
                }
            }
            if (!foundAll)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static async Task ProcessTTS (string text)
        {
            if (!CoreSystem.Config.UseMimic3) return;
            var data = await APIManager.Instance.GetTTSAudioAsync(text);
            using (var stream = new MemoryStream(data))
            {
                var player = new SoundPlayer(stream);
                player.Play();  // Use Play() for asynchronous playback
            }
        }

        private async Task CancelRunningAudio ()
        {
            if (audioPlaying)
            {
                cts.Cancel();
            }

            while (audioPlaying)
            {
                await Task.Delay(50);
            }
        }

        public static async Task PlayAudio(string filePath, bool treatPathAsFileName = true)
        {
            if (!CoreSystem.Config.UseTTSCache) return;

            await Instance.CancelRunningAudio();

            Instance.cts = new CancellationTokenSource();
            var token = Instance.cts.Token;

            Instance.audioPlaying = true;

            string audioFilePath = filePath;
            if (treatPathAsFileName)
            {
                audioFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/TTS Audio", filePath);
            }

            // Check if the file exists to handle the file not found scenario
            if (!File.Exists(audioFilePath))
            {
                CoreSystem.LogError($"No audio file located at: \"{audioFilePath}\"");
                return;
            }


            try
            {
                using (var audioFileReader = new Mp3FileReader(audioFilePath))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFileReader);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(100);
                        if (token.IsCancellationRequested)
                        {
                            outputDevice.Stop();
                        }
                    }
                }
            }
            catch (Exception ex) // Catching Exception to handle all types of exceptions
            {
                CoreSystem.LogError($"An error occurred while trying to play the audio file: {ex.Message}");
            }
            finally
            {
                Instance.audioPlaying = false;
            }
        }

        public static async Task PlayAudio(CachedTTS audioRef)
        {
            if (!CoreSystem.Config.UseTTSCache) return;

            string filePath = CachedAudio[audioRef];
            await PlayAudio(filePath);
        }

        public static async Task PlayGreeting ()
        {
            var greeting = Greetings.GetRandom();
            await PlayAudio(greeting);
        }

        public static async Task PlayTimeGreeting ()
        {
            DateTime now = DateTime.Now;
            int hour = now.Hour;

            if (hour < 12)
            {
                await PlayAudio(CachedTTS.Labs_Greeting_Morning);
            } 
            else if (hour < 18)
            {
                await PlayAudio(CachedTTS.Labs_Greeting_Afternoon);
            }
            else
            {
                await PlayAudio(CachedTTS.Labs_Greeting_Evening);
            }
        }
    }
}
