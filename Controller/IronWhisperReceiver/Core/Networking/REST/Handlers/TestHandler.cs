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
    public class TestHandler : IMiniAPIHandler
    {
        public override string EndpointPath()
        {
            return "/test";
        }

        public override ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = request.Respond()
                                  .Content("Test!")
                                  .Type(new FlexibleContentType(ContentType.TextPlain))
                                  .Build();

            return new ValueTask<IResponse?>(response);
        }
    }
}
