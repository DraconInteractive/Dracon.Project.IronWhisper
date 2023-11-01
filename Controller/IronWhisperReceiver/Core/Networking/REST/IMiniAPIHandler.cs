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
    public class CoreAPIHandler
    {
        public virtual ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            return default;
        }

        public virtual string EndpointPath() => "/";
    }
}
