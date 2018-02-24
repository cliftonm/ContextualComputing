/* The MIT License (MIT)
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
using System.Linq;
using System.Text;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Meaning;

using MeaningExplorer.Semantics;

namespace MeaningExplorer.Receptors
{
    public class PostReceptor : RouteHelpers, IReceptor
    {
        public void Process(ISemanticProcessor proc, IMembrane membrane, UpdateField msg)
        {
            ContextValueDictionary cvd = CreateOrGetContextValueDictionary(proc, msg.Context);
            var instancePath = msg.ID.Split(".").Select(Guid.Parse).ToList();
            var typePath = msg.TypePath.Split("|").ToList();
            var cv = new ContextValue(msg.Value, instancePath, typePath.Select(t=>Type.GetType(t)).ToList(), msg.RecordNumber);
            cvd.AddOrUpdate(cv);
            JsonResponse(proc, msg, new OKResponse());
        }

        public void Process(ISemanticProcessor proc, IMembrane membrane, SearchContext msg)
        {
            List<ContextValue> cvSearch = new List<ContextValue>();

            foreach (var search in msg.SearchFields)
            {
                var instancePath = search.ID.Split(".").Select(Guid.Parse).ToList();
                var typePath = search.TypePath.Split("|").ToList();
                var cv = new ContextValue(search.Value, instancePath, typePath.Select(t => Type.GetType(t)).ToList());
                cvSearch.Add(cv);
            }

            ContextValueDictionary cvd = CreateOrGetContextValueDictionary(proc, msg.Context);

            // We want unique context instances, as the search will return all context paths based on the search context values.
            List<ContextNode> matches = cvd.Search(cvSearch).DistinctBy(cn => cn.InstanceId).ToList();

            var results = matches.Select(m => cvd.GetPath(m));
            var html = Render(results);

            JsonResponse(proc, msg, new { Status = "OK", html = html.ToString().ToBase64String() });
        }

        public void Process(ISemanticProcessor proc, IMembrane membrane, ViewContext msg)
        {
            var instancePath = msg.InstancePath.Split(".").Select(s=>Guid.Parse(s)).ToList();
            ContextValueDictionary cvd = CreateOrGetContextValueDictionary(proc, msg.Context);
            // var (parser, context) = cvd.CreateContext(instancePath);

            // TODO: WHY ARE WE EVEN DOING THIS?  IT SEEMS ALL WE NEED TO DO IS CREATE THE PARSER!
            // var (parser, fullInstancePath) = cvd.CreateContext(instancePath.First());

            Guid rootId = instancePath.First();
            Type rootType = cvd.GetRootType(rootId);
            Parser parser = new Parser();
            var context = (Context)Activator.CreateInstance(rootType);
            parser.Parse(context);

            string html = Renderer.CreatePage(parser, Renderer.Mode.View, cvd, rootId);
            JsonResponse(proc, msg, new { Status = "OK", html = html.ToString().ToBase64String() });
        }

        protected StringBuilder Render(IEnumerable<ContextNodePath> results)
        {
            StringBuilder sb = new StringBuilder();
            sb.StartTable();

            foreach (var result in results)
            {
                sb.StartRow().
                    StartColumn().
                        Append(result.Path.First().Type.Name).
                    EndColumn().
                    StartColumn().Append("\r\n").
                        StartButton().
                            CustomAttribute("onclick", "post(\"/viewContext\", {instancePath : \"" + String.Join(".", result.Path.Select(p => p.InstanceId)) + "\"}, onShowSelectedSearchItem)").
                            Append("View").
                        EndButton().Append("\r\n").
                    EndColumn().
                EndRow();
            }

            sb.EndTable();

            return sb;
        }
    }
}
