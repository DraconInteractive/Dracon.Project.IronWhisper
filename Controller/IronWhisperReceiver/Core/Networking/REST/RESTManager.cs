using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine;
using GenHTTP.Modules.Conversion.Providers.Json;
using Newtonsoft.Json;

namespace IronWhisper_CentralController.Core.Networking.REST
{
    public class RESTManager
    {
        public async Task LaunchServer ()
        {
            Host.Create()
                .Handler(new APIHandlerBuilder())
                .Run();
        }
    }

    public class APIHandler : IHandler
    {
        private readonly IHandler _content;

        public APIHandler(IHandler content)
        {
            _content = content;
        }

        public IHandler Parent { get; }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
        {
            return AsyncEnumerable.Empty<ContentElement>();
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var parameters = request.Query;
            var content = request.Content;
            var path = request.Target.Path;

            var response = request.Respond()
                                  .Content(new JsonContent("Hello world", JsonSerializerOptions.Default))
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
