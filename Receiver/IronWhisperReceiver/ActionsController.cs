using IronWhisperReceiver.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver
{
    internal class ActionsController
    {
        public static ActionsController Instance;
        public List<CoreAction> Actions;

        
        public ActionsController()
        {
            Actions = new List<CoreAction>()
            {
                new DebugAction().Init(),
                new TestAction().Init(),
                new ComplexAction().Init(),
                new TimerAction().Init()
            };
            Instance = this;
        }

        public async Task ParseCommand (TCommand command)
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
    }
}
