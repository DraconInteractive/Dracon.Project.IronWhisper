using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class CompoundAction : CoreAction
    {
        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override void InternalInit()
        {
            Name = "Compound";
            Phrases = new string[]
            {
                "try a compound action",
                "enter the matrix"
            };
        }

        protected override async Task InternalRun(CoreSpeech command, CoreAction ctx = null)
        {
            // There is no way to get new speech here, so we have to change the foundation...
        }
    }
}
