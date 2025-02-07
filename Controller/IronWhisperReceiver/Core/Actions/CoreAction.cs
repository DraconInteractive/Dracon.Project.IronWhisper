﻿using IronWhisper_CentralController.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Actions
{
    public class CoreAction
    {
        public string Name;
        public string[] Phrases;
        public bool AlwaysRun = false;
        public bool UseGate;
        public bool Enabled;
        public int Priority = 0;
        public enum State
        {
            NotStarted, 
            Running,
            WaitingForInput,
            Finished
        }
        public State state;
        Action<State> onStateChange;


        public CoreAction()
        {
            Priority = 1;
            InternalInit();
            if (Phrases != null)
            {
                for (int i = 0; i < Phrases.Length; i++)
                {
                    Phrases[i] = Phrases[i].ToLower().Trim();
                }
            }
            else
            {
                CoreSystem.LogError($"Invalid phrase structure in '{Name}' action");
            }
            UseGate = true;
            Enabled = true;
            ChangeState(State.NotStarted);
        }

        protected virtual void InternalInit ()
        {

        }

        public virtual bool Evaluate (CoreSpeech command)
        {
            return (Phrases.Contains(command.Command));
        }

        protected bool PhrasesContainsFull(CoreSpeech command)
        {
            return (Phrases.Contains(command.Command.ToLower()));
        }

        protected bool PhrasesContainsPartial(CoreSpeech command)
        {
            if (command == null || command.Message == null) return false;
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

        public async Task Run(CoreSpeech speech)
        {

            if (state == State.NotStarted)
            {
                ChangeState(State.Running);
                await InternalRun(speech);
            }
            else
            {
                ChangeState(State.Running);
                await InternalRunAgain(speech);
            }
        }

        protected virtual async Task InternalRun(CoreSpeech speech)
        {
            
        }

        protected virtual async Task InternalRunAgain(CoreSpeech speech)
        {

        }

        public virtual string HelpInformation ()
        {
            return "This action has no help information: " + Name;
        }

        protected void ChangeState (State newState)
        {
            state = newState;
            onStateChange?.Invoke(state);
        }
    }
}
