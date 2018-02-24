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
