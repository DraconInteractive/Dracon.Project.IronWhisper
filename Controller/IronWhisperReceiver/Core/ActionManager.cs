﻿using IronWhisper_CentralController.Core.Actions;
using System.Reflection;


namespace IronWhisper_CentralController.Core
{
    public class ActionManager : CoreManager
    {
        public static ActionManager Instance;
        
        public List<CoreAction> Actions;
        public List<CoreAction> CurrentActions;

        public bool Gated { get; private set; }

        public ActionManager()
        {
            Instance = this;
            Actions = new List<CoreAction>();

            foreach (Type type in GetActionArchetypes())
            {
                if (Activator.CreateInstance(type) is CoreAction instance)
                {
                    Actions.Add(instance);
                }
            }
            CurrentActions = new List<CoreAction>();

            Gated = true;
        }

        public async Task ParseCommand(CoreSpeech command)
        {
            List<CoreAction> actionsToRun = new List<CoreAction>();
            List<Task> tasks = new List<Task>();


            if (CurrentActions.Count == 0)
            {
                foreach (var action in Actions.Where(x => x.Enabled))
                {
                    bool gated = Gated && action.UseGate && !command.ContainsPrompt;
                    bool viable = action.Evaluate(command) && !gated;
                   
                    if (viable || action.AlwaysRun)
                    {
                        actionsToRun.Add(action);
                    }
                }

                actionsToRun = actionsToRun.OrderBy(action => action.Priority).ToList();
                foreach (var action in actionsToRun)
                {
                    var instance = Activator.CreateInstance(action.GetType()) as CoreAction;
                    foreach (FieldInfo field in action.GetType().GetFields())
                    {
                        field.SetValue(instance, field.GetValue(action));
                    }
                    CurrentActions.Add(instance);
                    tasks.Add(instance.Run(command));
                }
            }
            else
            {
                foreach (var action in CurrentActions)
                {
                    tasks.Add(action.Run(command));
                }
            }

            await Task.WhenAll(tasks);
            CoreSystem.Log("");
            CurrentActions.RemoveAll(x => x.state == CoreAction.State.Finished);
        }

        List<Type> GetActionArchetypes()
        {
            List<Type> archetypes = new();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(CoreAction)))
                    {
                        archetypes.Add(type);
                    }
                }
            }
            return archetypes;
        }

        public void CloseGate()
        {
            Gated = true;
        }

        public void OpenGate()
        {
            Gated = false;
        }
    }
}
