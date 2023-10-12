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
    public interface IMiniAPIHandler
    {
        public ValueTask<IResponse?> HandleAsync(IRequest request);
        public string EndpointPath();
    }
}
