﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IronWhisperReceiver.Actions
{
    internal class TimerAction : CoreAction
    {
        public override CoreAction Init()
        {
            Name = "Timer";
            AlwaysRun = false;
            Phrases = new string[] { "set a timer for", "start a timer for", "run a timer for" };
            return this;
        }

        public override bool Evaluate(TCommand command)
        {
            bool match = false;
            foreach (var phrase in Phrases)
            {
                if (command.Message.ToLower().Contains(phrase))
                {
                    match = true;
                    break;
                }
            }
            return match;
        }

        protected override async Task InternalRun(TCommand command)
        {
            string message = command.Message;
            Regex regex = new Regex(@"(\d+)\s*(second|minute|hour)(?:s)?", RegexOptions.IgnoreCase);
            Match match = regex.Match(message);

            Console.WriteLine("Timer action detected");

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
                OutputMessage = $"Timer set for {_timerDuration.TotalMinutes} minutes.";
                // Discarded as we want the action to finish after setting the timer, not to wait until the timer is complete. 
                _ = WaitForTimer(_timerDuration);
            }
            else
            {
                OutputMessage = "I'm sorry, I didn't understand that. If you want to set a timer, say \"Set a timer for 10 minutes\"";
            }

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
