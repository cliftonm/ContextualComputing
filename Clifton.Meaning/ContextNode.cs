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

using Newtonsoft.Json;

using Clifton.Core.ExtensionMethods;

namespace Clifton.Meaning
{
    public class ContextNode
    {
        // Must be explicitly marked as JsonProperty because we are using a protected/private setter.
        [JsonProperty] public Guid InstanceId { get; protected set; }
        [JsonProperty] public Type Type { get; protected set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)] public ContextNode Parent { get; protected set; }
        
        public ContextValue ContextValue { get; set; }      // setter should probably only be used internally.
        public IReadOnlyList<ContextNode> Children { get { return children; } }
        public List<string> DebugChidrenTypeNames { get { return children.Select(c => c.Type.Name).ToList(); } }

        public static ContextNode Root { get { return new ContextNode(); } }

        protected List<ContextNode> children = new List<ContextNode>();

        protected ContextNode()
        {
        }

        public ContextNode(Guid instanceId, Type t)
        {
            InstanceId = instanceId;
            Type = t;
        }

        public void AddChild(ContextNode node)
        {
            children.Add(node);
            node.Parent = this;
        }

        public bool HasChild(Guid id)
        {
            return children.Any(child => child.InstanceId == id);
        }

        /// <summary>
        /// The parent instance chain from this child, including this child's ID.
        /// </summary>
        public List<Guid> GetParentInstancePath()
        {
            List<Guid> path = new List<Guid>();
            path.Add(InstanceId);
            ContextNode parentNode = Parent;

            while (parentNode != null)
            {
                path.Insert(0, parentNode.InstanceId);
                parentNode = parentNode.Parent;
            }

            return path.Skip(1).ToList();            // Exclude the Guid.Empty root.
        }

        /// <summary>
        /// Walk the child instance graph.
        /// </summary>
        public List<List<Guid>> ChildInstancePaths()
        {
            // There's probably a way to do this with iterators and yield return, but it's too complicated for me to figure out!
            List<List<Guid>> allPaths = new List<List<Guid>>();
            List<Guid> idpath = new List<Guid>();
            idpath.Add(InstanceId);
            Walk(children, idpath, allPaths);

            return allPaths;
        }

        protected void Walk(List<ContextNode> children, List<Guid> idpath, List<List<Guid>> allPaths)
        {
            if (children.Count == 0)
            {
                // Make a copy as we're manipulating the original!
                allPaths.Add(idpath.ToList());
            }
            else
            {
                foreach (var cn in children)
                {
                    idpath.Add(cn.InstanceId);
                    Walk(cn.children, idpath, allPaths);
                    idpath.RemoveLast();
                }
            }
        }
    }
}
