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
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Assertions;

namespace Clifton.Meaning
{
    public class ContextValueDictionary
    {
        // TODO: Workaround for right now as we're testing a single session.
        // TODO: Remove this eventually, saving the cvd when the session terminates or some other mechanism.
        public static ContextValueDictionary CVD;

        public ContextNode Root { get { return tree; } }
        public Dictionary<Type, List<ContextNode>> FlatView { get { return flatView; } }

        protected ContextNode tree = ContextNode.Root;

        // TODO: Probably should be a concurrent dictionary, but we may need to lock the walking of the tree as we are modifying it as we're walking.
        protected Dictionary<Type, List<ContextNode>> flatView = new Dictionary<Type, List<ContextNode>>();

        public ContextValueDictionary()
        {
            // TODO: What we load should be specific to the user and their session.
            Load();
            CVD = this;     // TODO: Remove this eventually, saving the cvd when the session terminates or some other mechanism.
        }

        public void Save()
        {
            // https://stackoverflow.com/questions/7397207/json-net-error-self-referencing-loop-detected-for-type
            var settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            settings.ObjectCreationHandling = ObjectCreationHandling.Auto;

            string jsonTree = JsonConvert.SerializeObject(tree, settings);
            string jsonFlatView = JsonConvert.SerializeObject(flatView, settings);

            File.WriteAllText("tree.cvd", jsonTree);
            File.WriteAllText("flatView.cvd", jsonFlatView);
        }

        public void Load()
        {
            // https://stackoverflow.com/questions/7397207/json-net-error-self-referencing-loop-detected-for-type
            var settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            settings.ObjectCreationHandling = ObjectCreationHandling.Auto;

            if (File.Exists("tree.cvd"))
            {
                string jsonTree = File.ReadAllText("tree.cvd");
                tree = JsonConvert.DeserializeObject<ContextNode>(jsonTree, settings);
            }

            if (File.Exists("flatView.cvd"))
            {
                string jsonFlatView = File.ReadAllText("flatView.cvd");
                flatView = JsonConvert.DeserializeObject<Dictionary<Type, List<ContextNode>>>(jsonFlatView, settings);
            }
        }

        public ContextValue CreateValue<T1>(Parser parser, string val, int recordNumber = 0)
        {
            return CreateContextValue(parser, val, recordNumber, typeof(T1));
        }

        public ContextValue CreateValue<T1, T2>(Parser parser, string val, int recordNumber = 0)
        {
            return CreateContextValue(parser, val, recordNumber, typeof(T1), typeof(T2));
        }

        public ContextValue CreateValue<T1, T2, T3>(Parser parser, string val, int recordNumber = 0)
        {
            return CreateContextValue(parser, val, recordNumber, typeof(T1), typeof(T2), typeof(T3));
        }

        public ContextValue CreateValue<T1, T2, T3, T4>(Parser parser, string val, int recordNumber = 0)
        {
            return CreateContextValue(parser, val, recordNumber, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public ContextValue CreateValue<T1, T2, T3, T4, T5>(Parser parser, string val, int recordNumber = 0)
        {
            return CreateContextValue(parser, val, recordNumber, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public ContextValue CreateValue<T1, T2, T3, T4, T5, T6>(Parser parser, string val, int recordNumber = 0)
        {
            return CreateContextValue(parser, val, recordNumber, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public IReadOnlyList<ContextNode> GetContextNodes(Type contextType)
        {
            List<ContextNode> nodes;

            if (!FlatView.TryGetValue(contextType, out nodes))
            {
                nodes = new List<ContextNode>();
            }

            return nodes.AsReadOnly();
        }

        public IReadOnlyList<ContextValue> GetContextValues(ContextNode node)
        {
            List<ContextValue> contextValues = new List<ContextValue>();
            GetContextValues(contextValues, node);

            return contextValues.AsReadOnly();
        }

        protected void GetContextValues(List<ContextValue> contextValues, ContextNode node)
        {
            foreach (var child in node.Children)
            {
                GetContextValues(contextValues, child);

                if (child.ContextValue != null)
                {
                    contextValues.Add(child.ContextValue);
                }
            }
        }

        /// <summary>
        /// Give a child node, returns the full path, starting from the topmost node, of the node hierarchy.
        /// </summary>
        public ContextNodePath GetPath(ContextNode node)
        {
            // A LINQ way of traversing the tree to its root.  Seems more complicated than the approach here.
            // https://stackoverflow.com/questions/34741456/linq-traverse-upwards-and-retrieve-parent-child-relationship
            // Also an interesting read: 
            // https://www.codeproject.com/Articles/62397/LINQ-to-Tree-A-Generic-Technique-for-Querying-Tree
            Type rootType = node.Type;
            Guid rootGuid = node.InstanceId;
            List<NodeTypeInstance> path = new List<NodeTypeInstance>() { new NodeTypeInstance() { Type = node.Type, InstanceId = node.InstanceId } };

            var parent = node.Parent;

            while (parent != null && parent.InstanceId != Guid.Empty)
            {
                rootType = parent.Type;
                rootGuid = parent.InstanceId;
                path.Insert(0, new NodeTypeInstance() { Type = parent.Type, InstanceId = parent.InstanceId });
                parent = parent.Parent;
            }

            var match = new ContextNodePath() { Type = rootType, Name = rootType.Name, Path = path };

            return match;
        }

        public bool TryGetContextNode(Field field, Guid rootId, int recordNumber, out ContextNode node, out List<Guid> instanceIdPath)
        {
            instanceIdPath = new List<Guid>();
            instanceIdPath.Add(rootId);
            ContextNode dictNode = tree.Children.Single(c => c.InstanceId == rootId);

            // Traverse the dictionary up to the node containing the ContextValue.
            // We traverse by type, building the instance ID list as we go.
            for (int i = 1; i < (field.ContextPath.Count - 1) && dictNode != null; i++)
            {
                dictNode = dictNode.Children.SingleOrDefault(c => c.Type == field.ContextPath[i].Type);

                if (dictNode != null)
                {
                    instanceIdPath.Add(dictNode.InstanceId);
                }
            }

            if (dictNode != null)
            {
                // Acquire the ContextValue based on field type and record number.
                dictNode = dictNode.Children.SingleOrDefault(c => c.Type == field.ContextPath[field.ContextPath.Count - 1].Type && c.ContextValue.RecordNumber == recordNumber);

                if (dictNode != null)
                {
                    instanceIdPath.Add(dictNode.InstanceId);
                }
            }

            node = dictNode;

            return node != null;
        }

        public Type GetRootType(Guid rootId)
        {
            ContextNode dictNode = tree.Children.Single(c => c.InstanceId == rootId);
            Type rootType = dictNode.Type;

            return rootType;
        }

        public void AddOrUpdate(ContextValue cv)
        {
            // We have to process this synchronously!
            // If async, we might get simultaneous requests (particularly from the browser's async PUT calls) to add a value.  
            // While we're constructing the dictionary entry for one context path, another request might come in before we've
            // created all the nodes for the first call.
            lock (this)
            {
                // Walk the instance/path, creating new nodes in the context tree as required.

                Assert.That(cv.TypePath.Count == cv.InstancePath.Count, "type path and instance path should have the same number of entries.");
                ContextNode node = tree;

                for (int i = 0; i < cv.TypePath.Count; i++)
                {
                    // Walk the tree.
                    var (id, type) = (cv.InstancePath[i], cv.TypePath[i]);

                    if (node.Children.TryGetSingle(c => c.InstanceId == id, out ContextNode childNode))
                    {
                        node = childNode;
                    }
                    else
                    {
                        // Are we referencing an existing sub-context?
                        if (flatView.TryGetValue(type, out List<ContextNode> nodes))
                        {
                            // The instance path of the node must match all the remaining instance paths in the context
                            // we're adding/updating!
                            bool foundExistingSubContext = false;

                            foreach (var fvnode in nodes)
                            {
                                foreach (var fvnodepath in fvnode.ChildInstancePaths())
                                {
                                    if (cv.InstancePath.Skip(i).SequenceEqual(fvnodepath))
                                    {
                                        // This node get's a child referencing the existing sub-context node.
                                        node.AddChild(fvnode);
                                        node = fvnode;
                                        foundExistingSubContext = true;
                                        break;
                                    }
                                }

                                if (foundExistingSubContext)
                                {
                                    break;
                                }
                            }

                            if (!foundExistingSubContext)
                            {
                                node = CreateNode(i, id, cv, node, type);
                            }
                        }
                        else
                        {
                            node = CreateNode(i, id, cv, node, type);
                        }
                    }
                }

                // The last entry in the tree gets the actual context value.  We've either added this node to the tree
                // or updating an existing node.
                node.ContextValue = cv;
            }
        }

        protected ContextNode CreateNode(int i, Guid id, ContextValue cv, ContextNode node, Type type)
        {
            if (i == cv.TypePath.Count - 1)
            {
                // At this point, node.Children[].Type && node.Children[].ContextValue.RecordNumber must be unique!
                Assert.That<ContextValueDictionaryException>(!node.Children.Any(c => c.Type == type && c.ContextValue.RecordNumber == cv.RecordNumber), "ContextValue type and record number must be unique to parent context.");
            }

            ContextNode childNode = new ContextNode(id, type);
            node.AddChild(childNode);

            // Since we're creating a node, add it to the flat tree view.
            if (!flatView.TryGetValue(type, out List<ContextNode> nodes))
            {
                flatView[type] = new List<ContextNode>();
            }

            flatView[type].Add(childNode);

            return childNode;
        }

        public List<ContextNode> Search(params ContextValue[] contextValuesToSearch)
        {
            return Search(contextValuesToSearch.ToList());
        }

        /// <summary>
        /// Returns the common context whose field values match the search values.
        /// The children of the context nodes that are returned are either IValueEntity's that match
        /// or (when implemented) will be sub-contexts that will match a separate context path IValueEntity.
        /// </summary>
        public List<ContextNode> Search(List<ContextValue> contextValuesToSearch)
        {
            // For now the parent type of each value must be the same, as the values (for now) cannot span group containers.
            // What we could do is collect all the unique parent group containers and find matches for those containers.  The parent group
            // must then be common for the super-parent to qualify the shared container.  This gets complicated when the matches are found
            // at different levels of the tree.

            Assert.That(contextValuesToSearch.Count > 0, "At least one ContextValue instance must be passed in to Search.");
            int pathItems = contextValuesToSearch[0].InstancePath.Count;
            Assert.That(contextValuesToSearch.All(cv => cv.InstancePath.Count == pathItems), "Context values must have the same path length for now.");

            // Make sure the parent types of all context values (the second to last entry in the TypePath) are the same.
            var parentTypes = contextValuesToSearch.Select(cv => cv.TypePath.Reverse().Skip(1).Take(1).First()).DistinctBy(cv => cv.AssemblyQualifiedName);

            Assert.That(parentTypes.Count() == 1, "Expected all context values to have the same field-parent.");

            // Get the parent type shared by the fields.
            Type parentType = parentTypes.Single();

            // Find this parent type in the dictionary of context values. 
            // We can find this parent type anywhere in the tree of any context value type path.

            List<ContextNode> matches = new List<ContextNode>();

            if (flatView.TryGetValue(parentType, out List<ContextNode> nodesOfParentType))
            {
                // Now compare the values in children of the context who's parent types match.
                foreach (var parentNode in nodesOfParentType)
                {
                    bool match = true;

                    // Handle multiple records at the last node which is the ContextValue holder.
                    var recordNumbers = parentNode.Children.Select(c => c.ContextValue.RecordNumber).Distinct();

                    foreach (int recNum in recordNumbers)
                    {
                        foreach (var cv in contextValuesToSearch)
                        {
                            Assert.That<ContextValueDictionaryException>(parentNode.Children.All(c => c.ContextValue != null), "Expected a ContextValue assigned to all children of the last context.");
                            var childMatch = parentNode.Children.SingleOrDefault(c => c.Type == cv.TypePath.Last() && c.ContextValue.RecordNumber == recNum);
                            Assert.That(childMatch != null, "A child matching the type and record number was expected.");
                            match = childMatch.ContextValue.Value == cv.Value;

                            if (!match)
                            {
                                break;
                            }
                        }

                        if (match)
                        {
                            matches.Add(parentNode);
                        }
                    }
                }
            }

            MatchWithContextsReferencingSubContext(matches);

            return matches;
        }

        protected void MatchWithContextsReferencingSubContext(List<ContextNode> matches)
        {
            // For each match, see if there are other super-contexts that reference this same matching context.
            // If so, these should be added to the list of matches.   
            // TODO: Would this be easier if we change ContextNode.Parent from a singleton to a List<ContextNode> ?
            // Clone, otherwise we may end up modifying the list of known matches.
            foreach (ContextNode match in matches.ToList())
            {
                List<Guid> matchParentPath = match.GetParentInstancePath();

                // We only want root contexts.
                foreach (var nodes in flatView.Values.Where(n=>n.Last().Parent.InstanceId == Guid.Empty))
                {
                    ContextNode lastNode = nodes.Last();

                    foreach (var childNodeIdPath in lastNode.ChildInstancePaths())
                    {
                        // We attempt to find a node where any sub-context ID equals the match last ID, as this gives us at least
                        // one context node in which we know that there is another reference.
                        // This, incidentally, is the common context type that we're searching on.
                        if (childNodeIdPath.Any(id => id == match.InstanceId))
                        {
                            // The root instance ID for this match should be different than any existing match.
                            if (!matches.Any(m => m.GetParentInstancePath().First() == nodes.First().InstanceId))
                            {
                                // Add this other context, that is referencing the context we matched on.
                                matches.Add(lastNode);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find the matching context path that results in a single field.
        /// </summary>
        protected ContextValue CreateContextValue(Parser parser, string val, int recordNumber, params Type[] types)
        {
            ContextValue cv = parser.CreateContextValue(val, recordNumber, types);
            AddOrUpdate(cv);

            return cv;
        }
    }
}
