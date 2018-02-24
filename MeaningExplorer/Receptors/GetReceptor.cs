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
using System.Text;
using System.Linq;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Meaning;
using Clifton.WebInterfaces;

using MeaningExplorer.Semantics;

namespace MeaningExplorer.Receptors
{
    public class GetReceptor : RouteHelpers, IReceptor
    {
        /// <summary>
        /// Returns HTML rendering the current state of the dictionary tree.
        /// </summary>
        public void Process(ISemanticProcessor proc, IMembrane membrane, GetDictionaryTreeHtml msg)
        {
            ContextValueDictionary cvd = CreateOrGetContextValueDictionary(proc, msg.Context);
            StringBuilder sb = new StringBuilder();
            List<Guid> path = new List<Guid>();
            NavigateChildren(sb, cvd.Root.Children, 0, path, msg.ShowInstanceIDs);

            JsonResponse(proc, msg, new { Status = "OK", html = sb.ToBase64String() });
        }

        /// <summary>
        /// Returns HTML rendering the current state of the dictionary nodes.
        /// </summary>
        public void Process(ISemanticProcessor proc, IMembrane membrane, GetDictionaryNodesHtml msg)
        {
            ContextValueDictionary cvd = CreateOrGetContextValueDictionary(proc, msg.Context);
            StringBuilder sb = new StringBuilder();

            foreach (var kvp in cvd.FlatView)
            {
                sb.Append(kvp.Key.Name + " : <br>");

                foreach (var node in kvp.Value)
                {
                    if (node.ContextValue != null)
                    {
                        sb.Append("&nbsp;&nbsp");
                        sb.Append(" = " + node.ContextValue.Value);
                        sb.Append("<br>");
                    }
                }
            }

            JsonResponse(proc, msg, new { Status = "OK", html = sb.ToBase64String() });
        }

        // Ex: http://localhost/renderContext?ContextName=MeaningExplorer.PersonContext
        // Ex: http://localhost/renderContext?ContextName=MeaningExplorer.PersonContext&isSearch=true
        public void Process(ISemanticProcessor proc, IMembrane membrane, RenderContext msg)
        {
            try
            {
                Type t = Type.GetType(msg.ContextName);
                Clifton.Meaning.IContext context = (Clifton.Meaning.IContext)Activator.CreateInstance(t);
                string html;

                try
                {
                    Parser parser = new Parser();
                    parser.Log = logMsg => Console.WriteLine(logMsg);
                    parser.Parse(context);

                    if (parser.AreDeclarationsValid)
                    {
                        ShowGroups(parser.Groups);
                        // The CVD is needed for rendering lookup values.
                        ContextValueDictionary cvd = CreateOrGetContextValueDictionary(proc, msg.Context);
                        html = Renderer.CreatePage(parser, msg.IsSearch ? Renderer.Mode.Search : Renderer.Mode.NewRecord, cvd);
                    }
                    else
                    {
                        html = "<p>Context declarations are not valid.  Missing entities:</p>" +
                            String.Join("<br>", parser.MissingDeclarations.Select(pt => pt.Name));
                    }
                }
                catch (Exception ex)
                {
                    html = "<p>Context declarations are not valid.</p><p>" + ex.Message + "</p>";
                    html = html + "<p>" + ex.StackTrace.Replace("\r\n", "<br>");
                }

                proc.ProcessInstance<WebServerMembrane, HtmlResponse>(r =>
                {
                    r.Context = msg.Context;
                    r.Html = html;
                });
            }
            catch (Exception ex)
            {
                proc.ProcessInstance<WebServerMembrane, HtmlResponse>(r =>
                {
                    r.Context = msg.Context;
                    r.Html = ex.Message + "<br>" + ex.StackTrace.Replace("\r\n", "<br>");
                });
            }
        }

        // Ex: http://localhost/dictionary
        // Ex: http://localhost/dictionary?showInstanceIds=true
        public void Process(ISemanticProcessor proc, IMembrane membrane, GetDictionary msg)
        {
            StringBuilder sb = new StringBuilder();
            sb.StartHtml();

            sb.StartHead().
                Script("/js/wireUpValueChangeNotifier.js").
                Stylesheet("/css/styles.css").
                EndHead();
            sb.StartBody();

            sb.StartParagraph().Append("<b>Dictionary:</b>").EndParagraph();
            sb.StartParagraph().StartDiv().ID("dictionary").EndDiv().EndParagraph();

            sb.StartParagraph().Append("<b>Type Nodes:</b>").EndParagraph();
            sb.StartParagraph().StartDiv().ID("nodes").EndDiv().EndParagraph();

            sb.StartScript().Javascript("(function() {getDictionaryHtml(" + msg.ShowInstanceIDs.ToString().ToLower() + ");})();").EndScript();
            sb.EndBody().EndHtml();

            proc.ProcessInstance<WebServerMembrane, HtmlResponse>(r =>
            {
                r.Context = msg.Context;
                r.Html = sb.ToString();
            });
        }

        protected void NavigateChildren(StringBuilder sb, IReadOnlyList<ContextNode> nodes, int level, List<Guid> path, bool showInstanceIds)
        {
            // Context value node.  Render different record numbers
            var recnums = nodes.Where(n=>n.ContextValue != null).Select(n => n.ContextValue.RecordNumber).Distinct();

            foreach (var recnum in recnums)
            {
                sb.Append(String.Concat(Enumerable.Repeat("&nbsp;", level * 2)));
                sb.Append("Record #:" + recnum);
                sb.Append("<br>");

                foreach (var node in nodes.Where(n=>n.ContextValue != null && n.ContextValue.RecordNumber == recnum))
                {
                    path.Add(node.InstanceId);
                    sb.Append(String.Concat(Enumerable.Repeat("&nbsp;", (level + 1) * 2)));
                    RenderNodeType(sb, node);
                    RenderNodeValue(sb, node);
                    RenderNodePath(sb, path, showInstanceIds);
                    sb.Append("<br>");
                    NavigateChildren(sb, node.Children, level + 1, path, showInstanceIds);
                    path.RemoveLast();
                }
            }

            // Non-context value node.
            foreach (var node in nodes.Where(n=>n.ContextValue == null))
            {
                path.Add(node.InstanceId);
                sb.Append(String.Concat(Enumerable.Repeat("&nbsp;", level * 2)));
                RenderNodeType(sb, node);
                RenderNodePath(sb, path, showInstanceIds);
                sb.Append("<br>");
                NavigateChildren(sb, node.Children, level + 1, path, showInstanceIds);
                path.RemoveLast();
            }
        }

        protected void RenderNodeType(StringBuilder sb, ContextNode node)
        {
            sb.Append(node.Type.Name);
        }

        protected void RenderNodeValue(StringBuilder sb, ContextNode node)
        {
            sb.Append(" = " + node.ContextValue.Value);
        }

        protected void RenderNodePath(StringBuilder sb, List<Guid> path, bool showInstanceIds)
        {
            if (showInstanceIds)
            {
                sb.Append("&nbsp;&nbsp;&nbsp;(");
                sb.Append(String.Join(".", path.Select(id => id.ToString().LeftOf("-"))));
                sb.Append(")");
            }
        }

        protected void ShowGroups(IReadOnlyList<Group> groups)
        {
            foreach (var group in groups)
            {
                Console.WriteLine("Group:");
                Console.WriteLine(group.Name + " => " + group.ContextPath.AsStringList());

                foreach (var field in group.Fields)
                {
                    Console.WriteLine("  " + field.Label + " => " + field.ContextPath.AsStringList());
                }

                Console.WriteLine();
            }
        }
    }
}
