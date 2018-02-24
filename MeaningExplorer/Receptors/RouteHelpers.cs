using System;

using Newtonsoft.Json;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceInterfaces;
using Clifton.Meaning;
using Clifton.WebInterfaces;

namespace MeaningExplorer.Receptors
{
    public class OKResponse
    {
        public string Status { get; set; }

        public OKResponse()
        {
            Status = "OK";
        }
    }

    public class RouteHelpers
    {
        protected void JsonResponse(ISemanticProcessor proc, SemanticRoute packet)
        {
            proc.ProcessInstance<WebServerMembrane, JsonResponse>(r =>
            {
                r.Context = packet.Context;
                r.Json = "";
                r.StatusCode = 200;
            });
        }

        protected void JsonResponse(ISemanticProcessor proc, SemanticRoute packet, string jsonResp)
        {
            proc.ProcessInstance<WebServerMembrane, JsonResponse>(r =>
            {
                r.Context = packet.Context;
                // Here we return the key and value with single quotes.  If we use double quotes, jQuery post will fail!
                // But the resulting value cannot be parsed in Javascript because a JSON string must have double-quotes for the key and value.
                // WTF????
                r.Json = jsonResp.ExchangeQuoteSingleQuote();
                r.StatusCode = 200;
            });
        }

        protected void JsonResponse(ISemanticProcessor proc, SemanticRoute packet, string tag, object data)
        {
            string json = (tag.Quote() + ":" + JsonConvert.SerializeObject(data)).CurlyBraces();

            proc.ProcessInstance<WebServerMembrane, JsonResponse>(r =>
            {
                r.Context = packet.Context;
                r.Json = json;
                r.StatusCode = 200;
            });
        }

        protected void JsonResponse(ISemanticProcessor proc, SemanticRoute packet, object data)
        {
            string json = JsonConvert.SerializeObject(data);

            proc.ProcessInstance<WebServerMembrane, JsonResponse>(r =>
            {
                r.Context = packet.Context;
                r.Json = json;
                r.StatusCode = 200;
            });
        }

        protected ContextValueDictionary CreateOrGetContextValueDictionary(ISemanticProcessor proc, Clifton.WebInterfaces.IContext context)
        {
            ContextValueDictionary cvd;
            IWebSessionService session = proc.ServiceManager.Get<IWebSessionService>();

            if (!session.TryGetSessionObject(context, "CVD", out cvd))
            {
                cvd = new ContextValueDictionary();
                session.SetSessionObject(context, "CVD", cvd);
            }

            return cvd;
        }
    }
}
