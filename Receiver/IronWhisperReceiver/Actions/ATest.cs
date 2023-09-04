using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    internal class ATest : CoreAction
    {
        public override CoreAction Init()
        {
            Name = "Test";
            AlwaysRun = false;
            Phrases = new string[]
            {
                "do a test",
                "run a test",
                ", do a test",
                ", run a test"
            };
            return this;
        }

        protected override void InternalRun(string message, params object[] parameters)
        {
            Console.WriteLine("Running a test!");
        }
    }
}
