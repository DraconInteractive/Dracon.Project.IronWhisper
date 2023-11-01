using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Providers.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GenHTTP.Modules.IO;

namespace IronWhisper_CentralController.Core.Networking.REST
{
    public class RootHandler : CoreAPIHandler
    {
        public override string EndpointPath()
        {
            return "/";
        }

        public override ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            string responseStr = $"IronWhisper REST Terminal - {CoreSystem.Config.Version}";
            var response = request.Respond()
                                  .Content(responseStr)
                                  .Type(new FlexibleContentType(ContentType.TextPlain))
                                  .Build();

            return new ValueTask<IResponse?>(response);
        }
    }
}
