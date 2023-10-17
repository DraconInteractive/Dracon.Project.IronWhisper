using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Providers.Json;
using GenHTTP.Modules.IO;
using IronWhisper_CentralController.Core.Registry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Networking.REST.Handlers
{
    public class DeviceHandler : IMiniAPIHandler
    {
        public override string EndpointPath()
        {
            return "/device";
        }

        public override ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var parameters = request.Query;
            var content = request.Content;
            var path = request.Target.Path;

            IResponse response;

            if (parameters.ContainsKey("id"))
            {
                var deviceAddress = request.Client.IPAddress;
                bool registerSuccess = RegistryCore.Instance.UpdateNetworkDevice(parameters["id"], deviceAddress);

                ResponsePacket packet = new ResponsePacket();
                packet.isValidID = registerSuccess;
                // get content, register device details
                if (parameters.ContainsKey("events"))
                {
                    switch(parameters["events"])
                    {
                        case "get":
                            packet.getEvents = true;
                            break;
                        case "consume":
                            packet.consumeEvents = true;
                            break;
                    }
                }

                response = request.Respond()
                                 .Content(JsonConvert.SerializeObject(packet, Formatting.Indented))
                                 .Type(new FlexibleContentType(ContentType.ApplicationJson))
                                 .Build();
            }
            else
            {
                response = request.Respond()
                                 .Content("Please specify ID parameter!")
                                 .Type(new FlexibleContentType(ContentType.TextPlain))
                                 .Status(ResponseStatus.BadRequest)
                                 .Build();

            }
            return new ValueTask<IResponse?>(response);

        }

        public class ResponsePacket
        {
            public bool isValidID;
            public string placeholder = "I still need to implement device events, and include them here!";
            public bool getEvents;
            public bool consumeEvents;
        }
    }
}
