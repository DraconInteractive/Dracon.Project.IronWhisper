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
    public class RootHandler : IMiniAPIHandler
    {
        public string EndpointPath()
        {
            return "/";
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = request.Respond()
                                  .Content(new JsonContent($"IronWhisper REST Terminal - {CoreSystem.Config.Version}", JsonSerializerOptions.Default))
                                  .Type(new FlexibleContentType(ContentType.TextPlain))
                                  .Build();

            return new ValueTask<IResponse?>(response);
        }
    }
}
