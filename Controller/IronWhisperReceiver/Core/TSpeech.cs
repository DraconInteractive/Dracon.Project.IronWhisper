using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronWhisperReceiver.Core.Registry;
using IronWhisperReceiver.Core.InputPipe;

namespace IronWhisperReceiver.Core
{
    public class CoreSpeech
    {
        private string[] _punctuation = new string[] { ",", ".", ":", "?", "!" };

        public string Message;
        public string Command;
        public RegCore[] Entities;

        public CoreSpeech (string prompt, string message)
        {
            Message = message;
            Command = message.Replace(prompt, "").ToLower();

            foreach (var p in _punctuation)
            {
                Command = Command.Replace(p, "");
            }

            Message = Message.Trim();
            Command = Command.Trim();

            Entities = RegistryCore.Instance.ParseEntities(Command).ToArray();
        }
    }

    public class TokenSpeech : CoreSpeech
    {
        public Token[] tokens;


        public TokenSpeech(string prompt, string message, Token[] _tokens = null) : base (prompt, message)
        {
            tokens = _tokens;
        }
    }
}
