using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisper_CentralController.Core.Networking.REST.Handlers
{
    public class ConfigHandler : CoreAPIHandler
    {
        public override string EndpointPath()
        {
            return "/config";
        }

        public override ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var parameters = request.Query;
            var content = request.Content;
            var path = request.Target.Path;

            IResponse response;
            string responseContent = "";
            ResponseStatus status = ResponseStatus.OK;

            bool viable = true;
            if (parameters.ContainsKey("fileName"))
            {
                var config = Registry.RegistryManager.Instance.GetConfig(parameters["fileName"]);
                if (config == null)
                {
                    viable = false;
                }
                else
                {
                    responseContent = config.Load() ?? "Invalid configuration file reference. Refer to IronWhisper Registry System.";
                }
            }
            else if (parameters.ContainsKey("projectName"))
            {
                // get all associated configs from registry, return packet with contents
                var configs = Registry.RegistryManager.Instance.GetProjectConfigs(parameters["projectName"]);
                if (configs == null)
                {
                    viable = false;
                }
                else if (configs.Length == 0)
                {
                    responseContent = "";
                    status = ResponseStatus.NoContent;
                }
                else
                {
                    List<string> contents = new List<string>();
                    foreach (var c in configs)
                    {
                        contents.Add(c.Load() ?? "Invalid configuration file reference. Refer to IronWhisper Registry System.");
                    }
                    responseContent = JsonConvert.SerializeObject(contents, Formatting.Indented);
                }
            }

            if (!viable)
            {
                status = ResponseStatus.BadRequest;
                responseContent = "";
            }

            response = request.Respond()
                                 .Content(responseContent)
                                 .Type(new FlexibleContentType(ContentType.TextPlain))
                                 .Status(status)
                                 .Build();
            return new ValueTask<IResponse?>(response);
        }
    }
}
