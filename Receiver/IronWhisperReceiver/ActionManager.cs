using IronWhisperReceiver.Actions;
using IronWhisperReceiver.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver
{
    public class ActionManager
    {
        public static ActionManager Instance;
        public List<CoreAction> Actions;

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
        }

        public async Task ParseCommand (TSpeech command)
        {
            List<CoreAction> actionsToRun = new List<CoreAction>();

            foreach (var action in Actions)
            {
                if (action.Evaluate(command) || action.AlwaysRun)
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
                await instance.Run(command);
            }
        }

        List<Type> GetActionArchetypes()
        {
            List<Type> archetypes = new List<Type>();
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
    }
}
