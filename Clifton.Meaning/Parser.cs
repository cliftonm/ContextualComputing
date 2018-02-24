/* The MIT License (MIT)
* 
* Copyright (c) 2015 Marc Clifton
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

using System;
using System.Collections.Generic;
using System.Linq;

using Clifton.Core.Assertions;

namespace Clifton.Meaning
{
    public class FieldContextPath
    {
        public Field Field { get; protected set; }
        public IReadOnlyList<ContextPath> Path { get { return contextPath; } }
        
        protected List<ContextPath> contextPath;

        public FieldContextPath(Field field, Stack<ContextPath> cp)
        {
            Field = field;
            contextPath = cp.Reverse().ToList();
        }
    }

    public class Parser
    {
        public const string CRLF = "\r\n";
        public Action<string> Log { get; set; }
        public IReadOnlyList<Group> Groups { get { return groups.AsReadOnly(); } }
        public IReadOnlyList<FieldContextPath> FieldContextPaths { get { return fieldContextPaths; } }
        public IReadOnlyList<Type> MissingDeclarations { get { return missingDeclarations.AsReadOnly(); } }

        protected List<Type> missingDeclarations = new List<Type>();
        protected List<Group> groups = new List<Group>();
        protected Stack<ContextPath> contextPath = new Stack<ContextPath>();
        protected bool parsed = false;
        protected List<Type> allEntities = new List<Type>();
        protected List<FieldContextPath> fieldContextPaths = new List<FieldContextPath>();

        // relationships and abstractions.
        // We don't really need the list of right-side types, but we may at some point want this.
        protected Dictionary<Type, List<Type>> allRelationships = new Dictionary<Type, List<Type>>();

        // Track visited abstractions for a particular context.
        protected Dictionary<Type, List<AbstractionDeclaration>> visitedContextAbstractions = new Dictionary<Type, List<AbstractionDeclaration>>();

        // Track type ID's within the parser's context so we use consistent context ID's when creating context values.
        protected Dictionary<Type, Guid> typeDictionary = new Dictionary<Type, Guid>();

        /// <summary>
        /// Track contexts that have lookups that define rendering.
        /// </summary>
        protected Dictionary<Type, Lookup> contextLookupMap = new Dictionary<Type, Lookup>();

        /// <summary>
        /// Returns true if the declarations are valid, meaning that there should not be any abstractions without a declared sub-entity
        /// and relationships without a left-side declared entity.  This requires that the context already be parsed, otherwise an exception
        /// is thrown.
        /// </summary>
        public bool AreDeclarationsValid
        {
            get
            {
                missingDeclarations.Clear();
                bool ret = true;

                if (!parsed)
                {
                    throw new ContextException("Context has not yet been parsed.");
                }

                // Keys must exist in allEntities.
                foreach (var key in allRelationships.Keys)
                {
                    if (!allEntities.Contains(key))
                    {
                        ret = false;
                        missingDeclarations.Add(key);
                    }
                }

                return ret;
            }
        }

        public void Parse(IContext context)
        {
            allEntities.Clear();
            allRelationships.Clear();
            visitedContextAbstractions.Clear();
            // contextPath.Push(new ContextPath(ContextPath.ContextPathType.Root, context.GetType()));

            Group group = CreateGroup(context, RelationshipDeclaration.NullRelationship, contextPath);
            groups.Add(group);

            GenerateMasterGroups(contextPath, groups, group, context, RelationshipDeclaration.NullRelationship);
            parsed = true;
        }

        public bool HasLookup(Type contextType)
        {
            return contextLookupMap.ContainsKey(contextType);
        }

        public Lookup GetLookup(Type contextType)
        {
            return contextLookupMap[contextType];
        }

        public ContextValue CreateValue<T1>(string val, int recordNumber = 0)
        {
            return CreateContextValue(val, recordNumber, typeof(T1));
        }

        public ContextValue CreateValue<T1, T2>(string val, int recordNumber = 0)
        {
            return CreateContextValue(val, recordNumber, typeof(T1), typeof(T2));
        }

        public ContextValue CreateValue<T1, T2, T3>(string val, int recordNumber = 0)
        {
            return CreateContextValue(val, recordNumber, typeof(T1), typeof(T2), typeof(T3));
        }

        public ContextValue CreateValue<T1, T2, T3, T4>(string val, int recordNumber = 0)
        {
            return CreateContextValue(val, recordNumber, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public ContextValue CreateValue<T1, T2, T3, T4, T5>(string val, int recordNumber = 0)
        {
            return CreateContextValue(val, recordNumber, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public ContextValue CreateValue<T1, T2, T3, T4, T5, T6>(string val, int recordNumber = 0)
        {
            return CreateContextValue(val, recordNumber, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public ContextValue CreateContextValue(string val, int recordNumber, params Type[] types)
        {
            Assert.That(types.Last().HasInterface<IValueEntity>(), "The last type in the context path must implement the IValueEntity interface.");

            // Clone the working list.
            List<FieldContextPath> matchingFieldPaths = new List<FieldContextPath>(fieldContextPaths);
            int n = 0;

            foreach (Type type in types)
            {
                // Reduce the working list down to only the matches.
                matchingFieldPaths = new List<FieldContextPath>(matchingFieldPaths.Where(fcp => fcp.Path[n].Type == type));
                ++n;
            }

            Assert.That(matchingFieldPaths.Count == 1, "Expected a single matching field to remain.");

            List<Guid> instancePath = CreateInstancePath(types);
            ContextValue cv = matchingFieldPaths[0].Field.CreateValue(val, instancePath);
            cv.RecordNumber = recordNumber;

            return cv;
        }

        public List<Guid> CreateInstancePath(IEnumerable<Type> types)
        {
            return CreateInstancePath(types.ToArray());
        }

        public List<Guid> CreateInstancePath(Type[] types)
        {
            List<Guid> instanceIdList = new List<Guid>(); 

            for (int pathIdx = 0; pathIdx < types.Length - 1; pathIdx++)
            {
                Guid pathId;
                Type t = types[pathIdx];

                if (!typeDictionary.TryGetValue(t, out pathId))
                {
                    pathId = Guid.NewGuid();
                    typeDictionary[t] = pathId;
                }

                instanceIdList.Add(pathId);
            }

            // Finally, always add a unique ID for the last path entry which is the context value!
            instanceIdList.Add(Guid.NewGuid());

            return instanceIdList;
        }

        protected void LogEntityType(Type t)
        {
            if (!allEntities.Contains(t))
            {
                allEntities.Add(t);
            }

            if (!allRelationships.ContainsKey(t))
            {
                allRelationships[t] = new List<Type>();
            }
        }

        protected void LogRelationship(Type left, Type right)
        {
            if (!allRelationships.ContainsKey(left))
            {
                allRelationships[left] = new List<Type>();
            }

            allRelationships[left].Add(right);
        }

        protected void LogVisitedAbstraction(IContext context, AbstractionDeclaration abstraction)
        {
            Type tcontext = context.GetType();

            if (!visitedContextAbstractions.ContainsKey(tcontext))
            {
                visitedContextAbstractions[tcontext] = new List<AbstractionDeclaration>();
            }

            visitedContextAbstractions[tcontext].Add(abstraction);
        }

        protected Group GenerateMasterGroups(Stack<ContextPath> contextPath, List<Group> groups, Group group, IContext context, RelationshipDeclaration relationship)
        {
            if (contextPath.Any(c => c.Type == context.GetType()))
            {
                throw new ContextException("Context " + context.GetType().Name + " is recursive.");
            }

            Log?.Invoke("Top level root entity: " + context.GetType().Name);
            LogEntityType(context.GetType());
            //Group group = CreateGroup(context, relationship, contextPath);
            //groups.Add(group);

            if (relationship.RelationshipType == typeof(NullRelationship))
            {
                contextPath.Push(new ContextPath(ContextPath.ContextPathType.Root, context.GetType()));
            }
            else
            {
                contextPath.Push(new ContextPath(ContextPath.ContextPathType.Relationship, context.GetType()));
            }

            var rootEntities = context.RootEntities;

            CreateFields(contextPath, context, group);

            foreach (var root in rootEntities)
            {
                DrillIntoAbstraction(contextPath, context, group, root);
            }

            GenerateRelationalGroups(contextPath, groups, group, context);

            // Get all abstractions defined by self-context.
            // This handles abstractions declared on this context by this context.
            // We skip abstractions we've already drilled into.
            // TODO: This seems kludgy but if we omit this, the unit tests on self-declared abstractions fails.
            // However, we can't qualify abstractions by this context type because then disassociated context don't
            // get parsed, which causes the DisassociatedAbtractionTest to fail.
            var abstractions = context.GetAbstractions();
            Type tcontext = context.GetType();
            
            foreach (var abstraction in abstractions)
            {
                // We only drill into abstractions that we haven't visited for this context.
                // See unit tests:
                // ContextWithAbstractionAndRelationTest
                // SubcontextWithSelfAbstractionTest
                // ContextWithAbstractionAndRelationshipTest
                if (!visitedContextAbstractions.ContainsKey(tcontext) || !visitedContextAbstractions[tcontext].Contains(abstraction))
                {
                    DrillIntoAbstraction(contextPath, context, group, abstraction);
                }
            }

            contextPath.Pop();

            return group;
        }

        protected void GenerateRelationalGroups(Stack<ContextPath> contextPath, List<Group> groups, Group group, IContext context)
        {
            var relationships = context.Relationships;

            foreach (var relationship in relationships)
            {
                foreach (var sourceType in relationship.AllSourceTypes)
                {
                    LogRelationship(relationship.TargetType, sourceType);
                    LogEntityType(sourceType);

                    if (sourceType.HasBaseClass<Context>())
                    {
                        Log?.Invoke("Context Relationship: " + relationship.RelationshipType.Name + " => Drilling into " + sourceType.Name);
                        // Create the context so we can explore its declarations of entities, relationships, abstractions, and ValueEntity types.
                        IContext relatedContext = (IContext)Activator.CreateInstance(sourceType);
                        MapPotentialLookup(relatedContext, sourceType);

                        // Push the relationship.
                        // contextPath.Push(new ContextPath(ContextPath.ContextPathType.Relationship, relationship.RelationshipType));

                        // Push the type related to.
                        // contextPath.Push(new ContextPath(ContextPath.ContextPathType.Implements, sourceType));

                        Group newGroup = group; 

                        if (!relationship.ShouldCoalesceRelationship)
                        {
                            newGroup = CreateGroup(relatedContext, relationship, contextPath);
                            groups.Add(newGroup);
                        }

                        GenerateMasterGroups(contextPath, groups, newGroup, relatedContext, relationship);
                        // contextPath.Pop();
                        // contextPath.Pop();
                    }

                    else if (sourceType.HasInterface<IEntity>())
                    {
                        // An IEntity doesn't have a context, it can't implement relationships or fields, so
                        // the only thing we can do is explore the abstractions.
                        // Note that Context implements IEntity, but IEntity is not a context!
                        Log?.Invoke(CRLF + "Entity Relationship: " + relationship.RelationshipType.Name + " => Drilling into " + sourceType.Name);

                        // Get abstractions for the source type:
                        var abstractions = context.GetAbstractions(sourceType);
                        Group egroup = CreateGroup(sourceType, relationship, contextPath, relationship.Label);
                        // contextPath.Push(new ContextPath(ContextPath.ContextPathType.Relationship, relationship.RelationshipType));
                        DrillIntoAbstractions(contextPath, context, egroup, abstractions);

                        // Only add the group if there are fields associated with the abstractions.
                        // Otherwise, we get groups for abstractions that have no fields.
                        if (egroup.Fields.Count > 0)
                        {
                            groups.Add(egroup);
                        }

                        // contextPath.Pop();
                    }
                }
            }
        }

        protected Group CreateGroup(EntityDeclaration decl, RelationshipDeclaration relationship, Stack<ContextPath> contextPath)
        {
            Log?.Invoke(CRLF + "Creating group: " + decl.EntityType.Name + " relationship: " + relationship.RelationshipType.Name);
            Group group = new Group(decl.Label ?? relationship.Label, decl.EntityType, contextPath, relationship);

            return group;
        }

        protected Group CreateGroup(IContext context, RelationshipDeclaration relationship, Stack<ContextPath> contextPath)
        {
            Type t = context.GetType();
            Log?.Invoke(CRLF + "Creating group: " + t.Name + " relationship: " + relationship.RelationshipType.Name);
            Group group = new Group(relationship.Label ?? context.Label ?? t.Name, t, contextPath, relationship);

            return group;
        }

        protected Group CreateGroup(Type t, RelationshipDeclaration relationship, Stack<ContextPath> contextPath, string label)
        {
            Log?.Invoke(CRLF + "Creating group: " + t.Name + " relationship: " + relationship.RelationshipType.Name);
            Group group = new Group(label ?? relationship.Label ?? t.Name, t, contextPath, relationship);

            return group;
        }

        protected void PopulateGroupFields(Stack<ContextPath> contextPath, IContext context, Group group, EntityDeclaration decl)
        {
            // CreateFields(contextPath, context, group);
            DrillIntoAbstraction(contextPath, context, group, decl);
        }

        protected void MapPotentialLookup(IContext context, Type contextType)
        {
            if (context.HasLookup)
            {
                contextLookupMap[contextType] = context.Lookup;
            }
        }

        protected void CreateFields(Stack<ContextPath> contextPath, IContext context, Group group)
        {
            foreach (var root in context.RootEntities)
            {
                LogEntityType(root.EntityType);

                // Drill into root entity declarations that are contexts.
                if (root.EntityType.HasBaseClass<Context>())
                {
                    Log?.Invoke("CreateFields: Drilling into " + root.EntityType.Name);
                    IContext rootContext = (IContext)Activator.CreateInstance(root.EntityType);
                    MapPotentialLookup(rootContext, root.EntityType);
                    var rootEntities = rootContext.RootEntities;

                    contextPath.Push(new ContextPath(ContextPath.ContextPathType.HasA, root.EntityType));

                    foreach (var rce in rootEntities)
                    {
                        LogEntityType(rce.EntityType);
                        // contextPath.Push(new ContextPath(ContextPath.ContextPathType.Root, root.EntityType));
                        // contextPath.Push(new ContextPath(ContextPath.ContextPathType.Root, rce.EntityType));

                        if (rce.EntityType.HasInterface<IValueEntity>())
                        {
                            Log?.Invoke("Adding field " + rce.EntityType.Name);
                            contextPath.Push(new ContextPath(ContextPath.ContextPathType.Field, rce.EntityType));
                            var field = group.AddField(root.Label ?? rce.EntityType.Name, contextPath);
                            fieldContextPaths.Add(new FieldContextPath(field, contextPath));
                            contextPath.Pop();
                        }
                        else if (rce.EntityType.HasBaseClass<Context>())
                        {
                            // Child entities (not abstractions or relationships) stay in the same group.
                            // See TwoSubcontextDeclarationsTest for a test that executes this code path.
                            Log?.Invoke("CreateFields: Drilling into child context " + rce.EntityType.Name);
                            contextPath.Push(new ContextPath(ContextPath.ContextPathType.Child, rce.EntityType));

                            // Create the context so we can explore its declarations of entities, relationships, abstractions, and IValueEntity types
                            IContext childContext = (IContext)Activator.CreateInstance(rce.EntityType);
                            MapPotentialLookup(childContext, rce.EntityType);

                            CreateFields(contextPath, childContext, group);
                            contextPath.Pop();
                        }
                        else
                        {
                            throw new ContextException(rce.EntityType.Name + " is not a Context or an IValueEntity.");
                        }
                    }

                    DrillIntoAbstraction(contextPath, rootContext, group, root);

                    contextPath.Pop();
                }
                else if (root.EntityType.HasInterface<IValueEntity>())
                {
                    Log?.Invoke("Adding field " + root.EntityType.Name);
                    contextPath.Push(new ContextPath(ContextPath.ContextPathType.Field, root.EntityType));
                    var field = group.AddField(root.Label ?? root.EntityType.Name, contextPath);
                    fieldContextPaths.Add(new FieldContextPath(field, contextPath));
                    contextPath.Pop();
                }
                else
                {
                    throw new ContextException(root.EntityType.Name + " is not a Context or an IValueEntity.");
                }
            }
        }

        protected void DrillIntoAbstraction(Stack<ContextPath> contextPath, IContext context, Group group, EntityDeclaration decl)
        {
            var abstractions = context.GetAbstractions(decl);
            DrillIntoAbstractions(contextPath, context, group, abstractions);
        }

        protected void DrillIntoAbstractions(Stack<ContextPath> contextPath, IContext context, Group group, IEnumerable<AbstractionDeclaration> abstractions)
        {
            foreach (var abstraction in abstractions)
            {
                DrillIntoAbstraction(contextPath, context, group, abstraction);
            }
        }

        protected void DrillIntoAbstraction(Stack<ContextPath> contextPath, IContext context, Group group, AbstractionDeclaration abstraction)
        {
            LogEntityType(abstraction.SuperType);
            LogRelationship(abstraction.SubType, abstraction.SuperType);
            LogVisitedAbstraction(context, abstraction);

            if (abstraction.SuperType.HasBaseClass<Context>())
            {
                Log?.Invoke("Abstraction: Drilling into " + abstraction.SuperType.Name);
                // Create the context so we can explore its declarations of entities, relationships, abstractions, and IValueEntity types.
                IContext superContext = (IContext)Activator.CreateInstance(abstraction.SuperType);
                MapPotentialLookup(superContext, abstraction.SuperType);
                var rootEntities = superContext.RootEntities;

                if (rootEntities.Count() > 0)
                {
                    Group group2 = group;

                    if (!abstraction.ShouldCoalesceAbstraction)
                    {
                        group2 = CreateGroup(abstraction.SuperType, RelationshipDeclaration.NullRelationship, contextPath, abstraction.Label);
                        groups.Add(group2);
                    }

                    foreach (var root in rootEntities)
                    {
                        contextPath.Push(new ContextPath(ContextPath.ContextPathType.Abstraction, abstraction.SuperType));
                        CreateFields(contextPath, superContext, group2);
                        PopulateGroupFields(contextPath, superContext, group2, root);
                        contextPath.Pop();
                    }
                }
            }

            // TODO: What if the abstraction is a plain-old entity?
            // Entities never have content?  Anything that has content would be in a context!
        }
    }
}
