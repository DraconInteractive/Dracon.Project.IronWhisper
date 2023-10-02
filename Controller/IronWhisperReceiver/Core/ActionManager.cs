using IronWhisper_CentralController.Core.Actions;
using IronWhisper_CentralController.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core
{
    public class ActionManager
    {
        public static ActionManager Instance;
        public List<CoreAction> Actions;

        public Stack<CoreAction> LogicStack;

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
            LogicStack = new Stack<CoreAction>();
        }

        public async Task ParseCommand(CoreSpeech command)
        {
            List<CoreAction> actionsToRun = new List<CoreAction>();

            if (LogicStack.Count == 0)
            {

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
