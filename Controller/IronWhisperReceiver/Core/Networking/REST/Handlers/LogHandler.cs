using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Providers.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IronWhisper_CentralController.Core.Networking.REST.Handlers
{
    public class LogHandler : CoreAPIHandler
    {
        public override string EndpointPath()
        {
            return "/log";
        }

        public override ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = request.Respond()
                                 .Content(new JsonContent(CoreSystem.Logs, JsonSerializerOptions.Default))
                                 .Type(new FlexibleContentType(ContentType.ApplicationJson))
                                 .Build();

            return new ValueTask<IResponse?>(response);
        }
    }
}
