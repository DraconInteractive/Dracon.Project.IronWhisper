using IronWhisper_CentralController.Core.Audio.TTS;
using IronWhisper_CentralController.Core.Networking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class AskLLMAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "LLM";
            Phrases = new string[] {"send a language studio request"};
            UseGate = false;
            Priority = 0;
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            CoreSystem.Log($"[LLM] Please provide input:");
            TTSManager.PlayAudio(CachedTTS.Labs_LLM_WhatInput);

            ChangeState(State.WaitingForInput);

        }

        protected override async Task InternalRunAgain(CoreSpeech command)
        {
            OpenAIRequestPacket packet = new OpenAIRequestPacket()
                .SetDefault()
                .ModifyMessageContent(0, "\"### Instruction: You are an AI assistant, in the middle of a conversation with your user. The user has just send you the following message:" +
                command.Message + "\n" +
                "Respond to this message appropriately." +
                "###Response: \"");

            APIRequestData request = new APIRequestData(APIManager.lmURL)
                .SetData(packet)
                .ContentType("application/json")
                .SetTimeout(30);

            string result = await APIManager.Instance.PostAsync(request);
            if (string.IsNullOrEmpty(result))
            {
                CoreSystem.Log("No valid response");
            }
            else
            {
                TTSManager.PlayAudio(CachedTTS.Labs_LLM_Response);

                try
                {
                    var response = JsonConvert.DeserializeObject<OpenAIResponsePacket>(result);
                    CoreSystem.Log("AI: " + response?.choices[0].message.content);
                }
                catch
                {
                    CoreSystem.LogError("Response in unexpected format");
                    CoreSystem.Log("Response: " + result);
                }
            }
            
            ChangeState(State.Finished);
        }

        public override string HelpInformation()
        {
            return "[Compound] \"Try a compound action\"";
        }
    }
}
