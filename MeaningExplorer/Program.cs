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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceInterfaces;
using Clifton.Meaning;
using Clifton.WebInterfaces;

using MeaningExplorer.Receptors;
using MeaningExplorer.Semantics;

// If unit tests don't load, close VS then delete .vs file and delete %TEMP%\VisualStudioTestExplorerExtensions
// re-open VS.  unit tests should load.

namespace MeaningExplorer
{
    static partial class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Bootstrap();
            InitializeRoutes();
            RegisterRouteReceptors();
            StartWebServer();
            // new Explorer().InitializeHomePage();
            Process.Start("chrome.exe", "localhost");
            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            // TODO: Remove this eventually, saving the cvd when the session terminates or some other mechanism.
            ContextValueDictionary.CVD.Save();
        }

        private static void ShowAnyExceptions(List<Exception> exceptions)
        {
            foreach (var ex in exceptions)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void StartWebServer()
        {
            IWebServerService server = serviceManager.Get<IWebServerService>();
            IAppConfigService configService = serviceManager.Get<IAppConfigService>();
            string ip = configService.GetValue("ip");
            string ports = configService.GetValue("ports");
            int[] portVals = ports.Split(',').Select(p => p.Trim().to_i()).ToArray();
            server.Start(ip, portVals);
        }

        private static void InitializeRoutes()
        {
            IAuthenticatingRouterService authRouter = serviceManager.Get<IAuthenticatingRouterService>();
            authRouter.RegisterSemanticRoute<HtmlPageRoute>("GET:", RouteType.PublicRoute);
            authRouter.RegisterSemanticRoute<UpdateField>("POST:updateField", RouteType.PublicRoute);
            // authRouter.RegisterSemanticRoute<UpdateSearchField>("POST:updateSearchField", RouteType.PublicRoute);
            authRouter.RegisterSemanticRoute<GetDictionary>("GET:dictionary", RouteType.PublicRoute);
            authRouter.RegisterSemanticRoute<GetDictionaryTreeHtml>("GET:dictionaryTreeHtml", RouteType.PublicRoute);
            authRouter.RegisterSemanticRoute<GetDictionaryNodesHtml>("GET:dictionaryNodesHtml", RouteType.PublicRoute);
            authRouter.RegisterSemanticRoute<RenderContext>("GET:renderContext", RouteType.PublicRoute);
            authRouter.RegisterSemanticRoute<SearchContext>("POST:searchContext", RouteType.PublicRoute);
            authRouter.RegisterSemanticRoute<ViewContext>("POST:viewContext", RouteType.PublicRoute);
            authRouter.RegisterSemanticRoute<ClearDictionary>("POST:clearDictionary", RouteType.PublicRoute);
        }

        private static void RegisterRouteReceptors()
        {
            ISemanticProcessor semProc = serviceManager.Get<ISemanticProcessor>();
            semProc.Register<WebServerMembrane, PostReceptor>();
            semProc.Register<WebServerMembrane, GetReceptor>();
        }
    }
}
