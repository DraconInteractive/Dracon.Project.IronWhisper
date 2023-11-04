using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine;
using GenHTTP.Modules.Conversion.Providers.Json;
using IronWhisper_CentralController.Core.Audio.TTS;
using IronWhisper_CentralController.Core.InputPipe;
using Newtonsoft.Json;
using static IronWhisper_CentralController.Core.CoreSystem;

namespace IronWhisper_CentralController.Core.Networking.REST
{
    public class RESTManager : CoreManager
    {
        public static RESTManager Instance;

        public RESTManager ()
        {
            Instance = this;
        }

        public async Task LaunchServer ()
        {
            Host.Create()
                .Handler(new APIHandlerBuilder())
                .Start();

            await Task.Delay(500);
            if (Config.useNGROK)
            {
                KillProcessByName("cmd");
                KillProcessByName("ngrok");
                await CreateTunnel();
                LogSystemStatus("NGROK", SystemStatus.Online);
            }
            else
            {
                LogSystemStatus("NGROK", SystemStatus.Disabled);
            }
        }

        private static async Task CreateTunnel ()
        {
            /*
            Log("Enter NGROK domain: ");
            Log(">>", ">>", ConsoleColor.Yellow);
            string line = Console.ReadLine() ?? "";
            */
            string line = "connect.draconai.com.au";
            await Utilities.CreateCommandWindowWithPrompt($"ngrok http --domain {line} 8080");
        }

        private static void KillProcessByName(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to kill process {processName}. Error: {ex.Message}");
                }
            }
        }
    }

    public class APIHandler : IHandler
    {
        public List<CoreAPIHandler> MiniHandlers;

        private readonly IHandler _content;

        public APIHandler(IHandler content)
        {
            _content = content;
            InitMiniHandlers();
        }

        private void InitMiniHandlers ()
        {
            MiniHandlers = new List<CoreAPIHandler>();
            var handlerTypes = Utilities.GetArchetypes(typeof(CoreAPIHandler));
            foreach (Type type in handlerTypes)
            {
                if (Activator.CreateInstance(type) is CoreAPIHandler h)
                {
                    MiniHandlers.Add(h);
                }
            }
        }

        public IHandler Parent { get; }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
        {
            return AsyncEnumerable.Empty<ContentElement>();
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            //CoreSystem.Log(request.Target.Path.ToString());
            var handler = MiniHandlers.FirstOrDefault(x => x.EndpointPath() == request.Target.Path.ToString());

            if (handler != null)
            {
                var r = handler.HandleAsync(request);
                CoreSystem.Log("[REST][Handler] Path: " + request.Target.Path.ToString(), 2);
                return r;
            }
            else
            {
                return HandleDefault(request);
            }
        }

        public ValueTask<IResponse?> HandleDefault(IRequest request)
        {
            var parameters = request.Query;
            var content = request.Content;
            var path = request.Target.Path;

            var response = request.Respond()
                                  .Content(new JsonContent("Invalid endpoint target!", JsonSerializerOptions.Default))
                                  .Type(new FlexibleContentType(ContentType.TextPlain))
                                  .Build();

            return new ValueTask<IResponse?>(response);
        }


        public ValueTask PrepareAsync()
        {
            return new ValueTask();
        }
    }

    public class APIHandlerBuilder : IHandlerBuilder<APIHandlerBuilder>
    {
        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        public APIHandlerBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return Concerns.Chain(parent, _Concerns, (p) => new APIHandler(p));
        }
    }
}
