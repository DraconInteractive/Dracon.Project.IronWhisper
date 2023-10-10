using IronWhisper_CentralController.Core.InputPipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Audio.TTS
{
    public enum CachedTTS
    {
        Boot_WaitForTerminal,
        Boot_TTS_Online,
        Activate_Prepare,
        Activate_Complete,
        Terminal_OnConnected
    }

    public class TTSManager
    {
        public static TTSManager Instance;

        private static readonly Dictionary<CachedTTS, string> CachedAudio = new Dictionary<CachedTTS, string>();

        public TTSManager ()
        {
            Instance = this;
            InitializeCacheDictionary();
        }

        private static void InitializeCacheDictionary()
        {
            string audioFilesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/TTS Audio");
            bool foundAll = true;
            foreach (CachedTTS ttsEnum in Enum.GetValues(typeof(CachedTTS)))
            {
                string fileName = ttsEnum.ToString() + ".wav";
                string filePath = Path.Combine(audioFilesDirectory, fileName);

                if (File.Exists(filePath))
                {
                    CachedAudio[ttsEnum] = filePath;
                }
                else
                {
                    CoreSystem.LogError($"Audio file not found for {ttsEnum}: {filePath}");
                    foundAll = false;
                }
            }
            if (foundAll)
            {
                CoreSystem.Log($"[TTS] Cache population: Success", "Success", ConsoleColor.Green, 1);
            }
            else
            {
                CoreSystem.Log($"[TTS] Cache population: Failure", "Failure", ConsoleColor.Red, 1);
            }
        }

        public async Task ProcessTTS (string text)
        {
            if (!CoreSystem.Config.UseMimic3) return;
            var data = await APIManager.Instance.GetTTSAudioAsync(text);
            PlayAudio(data);
        }

        public void PlayAudio(byte[] audioData)
        {
            // Play the audio
            using (var stream = new MemoryStream(audioData))
            {
                var player = new SoundPlayer(stream);
                player.Play();  // Use Play() for asynchronous playback
            }
        }

        public void PlayAudio(string fileName)
        {
            string audioFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/TTS Audio", fileName);
            try
            {
                using (var player = new SoundPlayer(audioFilePath))
                {
                    player.PlaySync();
                }
            }
            catch (FileNotFoundException)
            {
                CoreSystem.LogError($"No audio file located at: \"{audioFilePath}\"");
            }
            
        }

        public void PlayAudio(CachedTTS audioRef)
        {
            string filePath = CachedAudio[audioRef];
            PlayAudio(filePath);
        }
    }
}
