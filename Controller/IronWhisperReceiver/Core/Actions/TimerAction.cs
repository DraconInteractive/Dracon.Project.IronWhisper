using IronWhisper_CentralController.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    internal class TimerAction : CoreAction
    {
        protected override void InternalInit()
        {
            Name = "Timer";
            Phrases = new string[] { "set a timer", "start a timer", "run a timer" };
        }

        public override bool Evaluate(CoreSpeech command)
        {
            return PhrasesContainsPartial(command);
        }

        protected override async Task InternalRun(CoreSpeech command)
        {
            TimeSpan span = GetSpan(command.Command);
            if (span != default)
            {
                CoreSystem.Log("[Timer] Beginning delayed event call", 2);
                CoreSystem.Log($"[Timer] Timer set for {span.TotalMinutes} minutes");

                // Discarded as we want the action to finish after setting the timer, not to wait until the timer is complete. 
                _ = WaitForTimer(span);
                ChangeState(State.Finished);
            }
            else
            {
                CoreSystem.Log("[Timer] I can see you want to set a timer. How long should the timer be set for?");
                ChangeState(State.WaitingForInput);
            }
        }

        protected override async Task InternalRunAgain(CoreSpeech speech)
        {
            TimeSpan span = GetSpan(speech.Command);
            if (span != default)
            {
                CoreSystem.Log("[Timer] Beginning delayed event call", 2);
                CoreSystem.Log($"[Timer] Timer set for {span.TotalMinutes} minutes");

                // Discarded as we want the action to finish after setting the timer, not to wait until the timer is complete. 
                _ = WaitForTimer(span);
            }
            else
            {
                CoreSystem.Log("[Timer] I'm sorry, I can't seem to get a valid input. You can try and set a timer again if you want to keep trying!");
            }
            ChangeState(State.Finished);
        }

        private TimeSpan GetSpan (string input)
        {
            CoreSystem.Log("[Timer] Assessing...", 1);
            string message = Utilities.ExtractNumberFromText(input).Item1;

            Regex regex = new Regex(@"(\d+)\s*(second|minute|hour)(?:s)?", RegexOptions.IgnoreCase);
            Match match = regex.Match(message);

            if (match.Success)
            {
                int num = int.Parse(match.Groups[1].Value);
                string modifier = match.Groups[2].Value;
                int secondsToWait = 0;
                switch (modifier)
                {
                    case "second":
                        secondsToWait = num;
                        break;
                    case "minute":
                        secondsToWait = num * 60;
                        break;
                    case "hour":
                        secondsToWait = num * 60 * 60;
                        break;
                }
                TimeSpan _timerDuration = TimeSpan.FromSeconds(secondsToWait);
                return _timerDuration;
            }
            return default;
        }

        private async Task WaitForTimer (TimeSpan duration)
        {
            await Task.Delay(duration);
            EventsManager.Instance.RegisterEvent(new TimerEvent()
            {
                duration = duration
            });
        }
    }
}
