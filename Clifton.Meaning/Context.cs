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

namespace Clifton.Meaning
{
    // Contexts are themselves entities so that we can use then in relationship to other entities or contexts.
    public class Context : IEntity, IContext
    {
        public string Label { get; set; }

        public IReadOnlyList<ContextEntity> Entities => entities.AsReadOnly();
        public IReadOnlyList<RelationshipDeclaration> Relationships => relationships.RelationshipDeclarations;

        /// <summary>
        /// The root entities are entities where one of the declarations has no relationships.
        /// </summary>
        public IReadOnlyList<EntityDeclaration> RootEntities => contextDeclarations.AsReadOnly();

        public Lookup Lookup { get { return lookup; } }

        /// <summary>
        /// Returns true if the context is valid.  A valid context means that all
        /// instantiated entities in the context meet the entity and relationship declaration requirements.
        /// </summary>
        public bool IsValid 
        {
            get
            {
                bool ret = true;

                foreach (var cd in contextDeclarations)
                {
                    ret &= cd.Validate(entities);

                    if (!ret)
                    {
                        break;
                    }
                }

                if (ret)
                {
                    var entitiesToCheck = entities.Where(e => e.RelationshipType != typeof(NullRelationship));

                    foreach (var ce in entitiesToCheck)
                    {
                        ret &= relationships.Validate(ce, entities);

                        if (!ret)
                        {
                            break;
                        }
                    }
                }

                return ret;
            }
        }

        public bool HasLookup { get { return lookup != null; } }

        protected List<ContextEntity> entities = new List<ContextEntity>();
        protected Abstractions abstractions = new Abstractions();
        protected List<EntityDeclaration> contextDeclarations = new List<EntityDeclaration>();
        protected Relationships relationships = new Relationships();
        protected bool HasContextDeclarations { get { return contextDeclarations.Count > 0; } }
        protected bool HasRelationshipDeclarations { get { return relationships.Count > 0; } }
        protected Lookup lookup;

        public EntityDeclaration Declare<E>(string label = null) where E : IEntity
        {
            ValidateUniqueContextDeclaration<E>();
            var decl = EntityDeclaration.Create<E>(label);
            contextDeclarations.Add(decl);

            return decl;
        }

        public RelationshipDeclaration Declare<R, T, S>(string label = null)
            where R : IRelationship
            where T : IEntity
            where S : IEntity
        {
            ValidateUniqueRelationshipDeclaration<R, T, S>();
            var rel = relationships.Add<R, T, S>(label);

            return rel;
        }

        public void RelationshipRequired<E>() where E : IEntity
        {
            Declare<NullRelationship, E, NullEntity>().Max(0);
        }

        /// <summary>
        /// Adds a root entity to which other entities may be in relationship.  
        /// </summary>
        public void Add(IEntity entity)
        {
            var ce = ContextEntity.Create<NullRelationship>(entity, NullEntity.Instance);
            entities.Add(ce);
            ValidateContextDeclaration(entity, () => entities.Remove(ce));
            ValidateRelationshipDeclaration<NullRelationship>(entity, NullEntity.Instance, () => entities.Remove(ce));
        }

        /// <summary>
        /// Lookup starts with literal text.
        /// </summary>
        public Lookup LookupRenderer(string text)
        {
            lookup = new Lookup();
            lookup.Add(text);

            return lookup;
        }

        /// <summary>
        /// Lookup starts with a value type.
        /// </summary>
        public Lookup LookupRenderer<T>() where T : IValueEntity
        {
            lookup = new Lookup();
            lookup.Add<T>();

            return lookup;
        }

        /// <summary>
        /// Adds a target entity that has the specified relationship to the source entitiy.
        /// </summary>
        public void Add<R>(IEntity target, IEntity source) where R : IRelationship
        {
            ValidateRelationship<R>(target, source);
            var ce = ContextEntity.Create<R>(target, source);
            entities.Add(ce);
            ValidateContextDeclaration(target, () => entities.Remove(ce));
            ValidateRelationshipDeclaration<R>(target, source, () => entities.Remove(ce));
        }

        /// <summary>
        /// Returns all context entities in this context whose type is the specified generic parameter.
        /// </summary>
        public IEnumerable<ContextEntity> Get<T>() where T : IEntity
        {
            return entities.Where(e => e.ConcreteEntity.GetType() == typeof(T));
        }

        /// <summary>
        /// Returns all context entities in this context that are the entity passed in.
        /// </summary>
        public IEnumerable<ContextEntity> Get(IEntity entity)
        {
            return entities.Where(e => e.ConcreteEntity == entity);
        }

        public AbstractionDeclaration AddAbstraction<SubType, SuperType>(string label = null)
            where SubType : IEntity
            where SuperType : IEntity
        {
            ValidateAbstraction<SubType, SuperType>();
            var decl = abstractions.Add<SubType, SuperType>(label);

            return decl;
        }

        // Queries:

        /// <summary>
        /// Returns all abstractions for this context.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AbstractionDeclaration> GetAbstractions()
        {
            return abstractions.GetAbstractions();
        }

        /// <summary>
        /// Returns the abstractions of a particular type.
        /// </summary>
        public IEnumerable<AbstractionDeclaration> GetAbstractions<T>() where T : IEntity
        {
            return abstractions.GetAbstractions<T>();
        }

        /// <summary>
        /// Returns the abstractions of a particular type.
        /// </summary>
        public IEnumerable<AbstractionDeclaration> GetAbstractions(Type t)
        {
            return abstractions.GetAbstractions(t);
        }

        public IEnumerable<AbstractionDeclaration> GetAbstractions(EntityDeclaration decl)
        {
            return abstractions.GetAbstractions(decl.EntityType);
        }

        /// <summary>
        /// Returns the implementors of a particular abstraction.
        /// </summary>
        public IEnumerable<AbstractionDeclaration> GetImplementations<T>() where T : IEntity
        {
            return abstractions.GetImplementations<T>();
        }

        public IEnumerable<AbstractionDeclaration> GetImplementations(Type t)
        {
            return abstractions.GetImplementations(t);
        }

        public IEnumerable<AbstractionDeclaration> GetImplementations(EntityDeclaration decl)
        {
            return abstractions.GetImplementations(decl.EntityType);
        }

        /*
        protected void ValidateRelationshipExists(IEntity entity, ContextEntity.RelationshipSide side)
        {
            switch (side)
            {
                case ContextEntity.RelationshipSide.ConcreteEntity:
                    if (!entities.Any(e => e.ConcreteEntity == entity))
                    {
                        throw new ContextException("The concrete entity does not exist in the context's entity collection.");
                    }
                    break;

                case ContextEntity.RelationshipSide.RelatedTo:
                    if (!entities.Any(e => e.RelatedTo == entity))
                    {
                        throw new ContextException("The related to entity does not exist in the context's entity collection.");
                    }
                    break;
            }
        }
        */

        /// <summary>
        /// If the declarations collection is populated, it validates that the entity meets to requirements of the declarations.
        /// </summary>
        protected void ValidateContextDeclaration(IEntity entity, Action undo)
        {
            if (HasContextDeclarations)
            {
                Type entityType = entity.GetType();
                var declItems = contextDeclarations.Where(d => d.EntityType == entity.GetType());

                // We know declarations must be unique, so the count is either 0 or 1.
                if (declItems.Count() == 0)
                {
                    undo();
                    throw new DeclarationViolationException("A declaration for " + entityType.Name + " does not exist.");
                }

                var decl = declItems.First();

                if (!decl.Validate(entities))
                {
                    // We allow additional declarations to be added if minimum is not satisfied.
                    // TODO: Review this.  Maybe we need a "suspend validations" flag?
                    if (decl.Minimum <= entities.Count(e => e.ConcreteEntity.GetType() == entityType))
                    {
                        undo();
                        throw new DeclarationViolationException("Declaration violation for " + entityType.Name + ".");
                    }
                }
            }
        }

        protected void ValidateRelationshipDeclaration<R>(IEntity target, IEntity source, Action undo) where R : IRelationship
        {
            if (HasRelationshipDeclarations)
            {
                if (!relationships.Validate<R>(target, source, entities, out int min, out int count))
                {
                    // We allow additional declarations to be added if minimum is not satisfied.
                    // TODO: Review this.  Maybe we need a "suspend validations" flag?
                    if (min <= count)
                    {
                        undo();
                        throw new RelationshipDeclarationException("Relationship violation for " + typeof(R).Name + ".");
                    }
                }
            }
        }

        protected void ValidateRelationship<T>(IEntity target, IEntity source) where T : IRelationship
        {
            if (HasRelationshipDeclarations)
            {
                if (!relationships.Exists<T>(target, source))
                {
                    throw new RelationshipDeclarationException("Relationship " + typeof(T).Name + " not defined for entity " + target.GetType().Name + " related to " + source.GetType().Name);
                }
            }
        }

        protected void ValidateUniqueContextDeclaration<E>() where E : IEntity
        {
            if (contextDeclarations.Any(d => d.EntityType == typeof(E)))
            {
                throw new DeclarationViolationException("Declarations must be unique. " + typeof(E).Name + " is not unique.");
            }
        }

        public void ValidateUniqueRelationshipDeclaration<R, T, S>()
            where R : IRelationship
            where T : IEntity
            where S : IEntity
        {
            if (relationships.Exists<R, T, S>())
            {
                throw new RelationshipDeclarationException("Relationship " + typeof(T).Name + " already defined for entity " + typeof(T).Name + " related to " + typeof(S).Name);
            }
        }

        protected void ValidateAbstraction<SubType, SuperType>()
            where SubType : IEntity
            where SuperType : IEntity
        {
            if (abstractions.Exists<SubType, SuperType>())
            {
                throw new AbstractionException("Abstract exists for " + typeof(SubType).Name + ":" + typeof(SuperType).Name);
            }
        }
    }
}
