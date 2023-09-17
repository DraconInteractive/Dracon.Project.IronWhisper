using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronWhisperReceiver.Registry;
using IronWhisperReceiver.Networking;

namespace IronWhisperReceiver
{
    public class TSpeech
    {
        private string[] _punctuation = new string[] { ",", ".", ":", "?", "!" };

        public string Message;
        public string Command;

        public Token[] tokens;

        public RegCore[] Entities;

        public TSpeech(string prompt, string message, Token[] _tokens = null)
        {
            Message = message;
            Command = message.Replace(prompt, "").ToLower();

            foreach (var p in _punctuation)
            {
                Command = Command.Replace(p, "");
            }

            Message = Message.Trim();
            Command = Command.Trim();

            if (_tokens != null)
            {
                tokens = _tokens;
            }
            else
            {
                tokens = Array.Empty<Token>();
            }
            Entities = Registry.Registry.Instance.ParseEntities(Command).ToArray();
        }
    }
}
