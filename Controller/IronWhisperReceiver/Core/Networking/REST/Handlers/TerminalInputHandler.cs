using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Providers.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Networking.REST
{
    public class TerminalInputHandler : CoreAPIHandler
    {
        public override string EndpointPath()
        {
            return "/terminal";
        }

        public override ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var parameters = request.Query;
            var content = request.Content;
            var path = request.Target.Path;

            if (parameters.ContainsKey("input"))
            {
                var source = "REST";
                if (parameters.ContainsKey("source"))
                {
                    source = parameters["source"];
                }
                InputHandler.Instance.RegisterStandardInput(parameters["input"], source);
            }

            var response = request.Respond()
                                  .Status(ResponseStatus.OK)
                                  .Build();

            return new ValueTask<IResponse?>(response);
        }
    }
}
