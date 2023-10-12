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
    public class ManualInputHandler : IMiniAPIHandler
    {
        public string EndpointPath()
        {
            return "/input";
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var parameters = request.Query;
            var content = request.Content;
            var path = request.Target.Path;

            if (parameters.ContainsKey("input"))
            {
                InputHandler.Instance.RegisterInput(parameters["input"]);
            }

            var response = request.Respond()
                                  .Status(ResponseStatus.OK)
                                  .Build();

            return new ValueTask<IResponse?>(response);
        }
    }
}
