﻿/* The MIT License (MIT)
* 
* Copyright (c) 2018 Marc Clifton
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

/* Code Project Open License (CPOL) 1.02
* https://www.codeproject.com/info/cpol10.aspx
*/

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
