using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronWhisper_CentralController.Core.Registry;
using IronWhisper_CentralController.Core.InputPipe;

namespace IronWhisper_CentralController.Core
{
    public class CoreSpeech
    {
        private string[] _punctuation = new string[] { ",", ".", ":", "?", "!" };

        public string Message;
        public string Command;
        public RegCore[] Entities;

        public string Source;
        public bool ContainsPrompt;

        public CoreSpeech (string prompt, string message, string source = "")
        {
            if (message == null) return;

            Message = message.TrimEnd('\0');
            Command = message.Replace(prompt, "").ToLower(); 

            if (message.ToLower().Contains(prompt))
            {
                ContainsPrompt = true;
            }

            foreach (var p in _punctuation)
            {
                Command = Command.Replace(p, "");
            }

            Message = Message.Trim();
            Command = Command.Trim();
            Source = source;

            Entities = RegistryManager.Instance.ParseEntities(Command).ToArray();
        }
    }

    public class TokenSpeech : CoreSpeech
    {
        public Token[] tokens;


        public TokenSpeech(string prompt, string message, Token[]? _tokens = null) : base(prompt, message)
        {
            if (_tokens == null)
            {
                tokens = Array.Empty<Token>();
            }
            else
            {
                tokens = _tokens;
            }
        }
    }

    public class Token
    {
        public string Text;
        public string Lemma;
        public string Pos;
        public string Dep;
    }
}
