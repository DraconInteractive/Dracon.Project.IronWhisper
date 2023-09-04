using IronWhisperReceiver.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver
{
    internal class ActionsController
    {
        public static ActionsController Instance;
        public List<CoreAction> Actions;

        private string[] _punctuation = new string[] {",", ".", ":", "?", "!"};
        
        public ActionsController()
        {
            Actions = new List<CoreAction>()
            {
                new ADebug().Init(),
                new ATest().Init(),
                new AComplex().Init()
            };
            SocketController.Instance.commandReceived += OnCommandReceived;
            OnCommandReceived("ActionsController created");
            Instance = this;
        }

        public string ParseCommand (string command)
        {
            foreach (var p in _punctuation)
            {
                command = command.Replace(p, "");
            }
            command = command.Trim();
            return command;
        }

        public void OnCommandReceived(string transcript, string command = "")
        {
            if (string.IsNullOrEmpty(command))
            {
                command = transcript;
            }

            command = ParseCommand(command);
            List<CoreAction> actionsToRun = new List<CoreAction>();

            foreach (var action in Actions)
            {
                if (action.Phrases.Contains(command) || action.AlwaysRun)
                {
                    actionsToRun.Add(action);
                }
            }
            actionsToRun = actionsToRun.OrderBy(action => action.Priority).ToList();
            foreach (var action in actionsToRun)
            {
                action.Run(command);
            }
        }
    }
}
