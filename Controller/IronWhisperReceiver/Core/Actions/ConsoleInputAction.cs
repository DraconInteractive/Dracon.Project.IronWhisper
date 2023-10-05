using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class ConsoleInputAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Ping";
            Phrases = new string[]
            {
                "enter some details",
                "register input"
            };
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            string name = CoreSystem.GetInput("Enter your name: ", x => true);
            CoreSystem.Log("Hello, " + name);
            ChangeState(State.Finished);
        }
    }
}
