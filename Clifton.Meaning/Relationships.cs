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
    public class Relationships
    {
        public int Count => relationships.Count;
        public IReadOnlyList<RelationshipDeclaration> RelationshipDeclarations => relationships.AsReadOnly(); 

        protected List<RelationshipDeclaration> relationships = new List<RelationshipDeclaration>();

        public RelationshipDeclaration Add<R, T, S>(string label = null)
            where R : IRelationship
            where T : IEntity
            where S : IEntity
        {
            var rel = RelationshipDeclaration.Create<R, T, S>(label);
            relationships.Add(rel);

            return rel;
        }

        /*
        /// <summary>
        /// Returns true if the target entity type has a null relationship.
        /// It can also have other relationships, but it must have one null relationship to be a root type.
        /// </summary>
        public bool IsRoot(Type entityType)
        {
            return relationships.Any(r => r.TargetType == entityType && r.RelationshipType == typeof(NullRelationship));
        }
        */

        /// <summary>
        /// Returns true if the specified relationship between the target and source exists.
        /// </summary>
        public bool Exists<R, T, S>()
            where R : IRelationship
            where T : IEntity
            where S : IEntity
        {
            return relationships.Any(r => 
                r.RelationshipType == typeof(R) &&
                r.TargetType == typeof(T) &&
                r.SourceType == typeof(S)
            );
        }

        public bool Exists<R>(IEntity target, IEntity source) where R : IRelationship
        {
            // TODO: Need to handle "and" relationship to entity type.
            return relationships.Any(r =>
                r.RelationshipType == typeof(R) &&
                r.TargetType == target.GetType() &&
                (
                    r.SourceType == source.GetType() || 
                    r.OrSourceTypes.Any(ot => ot == source.GetType())) || 
                    r.AndSourceTypes.Any(ot=>ot==source.GetType())
                );
        }

        public bool Validate<R>(IEntity target, IEntity source, List<ContextEntity> entities, out int min, out int count)
        {
            min = 0;
            count = 0;
            bool ret = true;

            var relationshipsToCheck = relationships.
                Where(r =>
                    r.RelationshipType == typeof(R) &&
                    r.TargetType == target.GetType() &&
                    (r.SourceType == source.GetType() || r.OrSourceTypes.Any(ot => ot == source.GetType())));

            foreach (var rcheck in relationshipsToCheck)
            {
                var possibleRelatedToTypes = new List<Type>() { rcheck.SourceType };
                possibleRelatedToTypes.AddRange(rcheck.OrSourceTypes);

                var entitiesInThisRelationship = entities.Where(e =>
                    e.RelationshipType == typeof(R) &&
                    e.ConcreteEntity == target &&
                    possibleRelatedToTypes.Any(rt => rt == e.RelatedTo.GetType()));

                count = entitiesInThisRelationship.Count();

                if (rcheck.Minimum > count || count > rcheck.Maximum)
                {
                    min = rcheck.Minimum;
                    ret = false;
                    break;
                }
            }

            return ret;
        }

        public bool Validate(ContextEntity ce, List<ContextEntity> entities)
        {
            bool ret = ValidateOrRelationships(ce, entities);

            if (ret)
            {
                ret &= ValidateAndRelationships(ce, entities);
            }

            return ret;
        }

        protected bool ValidateOrRelationships(ContextEntity ce, List<ContextEntity> entities)
        {
            bool ret = true;

            var relationshipsToCheck = relationships.
                Where(r => 
                    r.RelationshipType == ce.RelationshipType &&
                    r.TargetType == ce.ConcreteEntity.GetType() && 
                    (r.SourceType == ce.RelatedTo.GetType() || r.OrSourceTypes.Any(ot=>ot == ce.RelatedTo.GetType())));

            foreach (var rcheck in relationshipsToCheck)
            {
                var possibleRelatedToTypes = new List<Type>() { rcheck.SourceType };
                possibleRelatedToTypes.AddRange(rcheck.OrSourceTypes);

                var entitiesInThisRelationship = entities.Where(e =>
                    e.RelationshipType == ce.RelationshipType &&
                    e.ConcreteEntity == ce.ConcreteEntity &&
                    possibleRelatedToTypes.Any(rt => rt == ce.RelatedTo.GetType()));

                int count = entitiesInThisRelationship.Count();

                if (rcheck.Minimum > count || count > rcheck.Maximum)
                {
                    ret = false;
                    break;
                }
            }

            return ret;
        }

        protected bool ValidateAndRelationships(ContextEntity ce, List<ContextEntity> entities)
        {
            bool ret = true;

            // All relationships of the relationshtype having the same target must match the "and" requirements.
            var relationshipsToCheck = relationships.
                Where(r =>
                    r.RelationshipType == ce.RelationshipType &&
                    r.TargetType == ce.ConcreteEntity.GetType());

            foreach (var rcheck in relationshipsToCheck)
            {
                if (rcheck.AndSourceTypes.Count > 0)
                {
                    var requiredRelatedToTypes = new List<Type>() { rcheck.SourceType };
                    requiredRelatedToTypes.AddRange(rcheck.AndSourceTypes);

                    var entitiesInThisRelationship = entities.Where(e =>
                        e.RelationshipType == ce.RelationshipType &&
                        e.ConcreteEntity == ce.ConcreteEntity &&
                        requiredRelatedToTypes.Any(rt => rt == ce.RelatedTo.GetType()));

                    ret = entitiesInThisRelationship.Count() == requiredRelatedToTypes.Count;

                    if (!ret)
                    {
                        ret = false;
                        break;
                    }
                }
            }

            return ret;
        }
    }
}
