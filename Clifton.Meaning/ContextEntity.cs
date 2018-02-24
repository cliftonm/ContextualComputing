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

namespace Clifton.Meaning
{
    public class NullEntity : IEntity
    {
        protected static NullEntity instance;

        public static NullEntity Instance => instance;

        static NullEntity()
        {
            instance = new NullEntity();
        }
    }

    public class NullRelationship : IRelationship 
    {
        protected static NullRelationship instance;

        public static NullRelationship Instance => instance;

        static NullRelationship()
        {
            instance = new NullRelationship();
        }
    }

    public class ContextEntity
    {
        //public enum RelationshipSide
        //{ 
        //    ConcreteEntity,
        //    RelatedTo,
        //}

        public static IEntity NullRelatedEntity = new NullEntity();
        public IEntity ConcreteEntity { get; protected set; }
        public IEntity RelatedTo { get; protected set; }
        public Type RelationshipType { get; protected set; }

        /// <summary>
        /// Create a root entity container that has no relationship to another entity.
        /// This method is used exclusively for unit tests.
        /// </summary>
        public static ContextEntity Create(IEntity concreteEntity)
        {
            return new ContextEntity(concreteEntity, NullRelatedEntity, typeof(NullRelationship));
        }

        /// <summary>
        /// Create an entity container that is in the specified relationship with another entity.
        /// </summary>
        public static ContextEntity Create<T>(IEntity concreteEntity, IEntity relatedTo) where T : IRelationship
        {
            return new ContextEntity(concreteEntity, relatedTo, typeof(T));
        }

        /// <summary>
        /// Constructor not accessible to the rest of the world.
        /// </summary>
        protected ContextEntity(IEntity concreteEntity, IEntity relatedTo, Type relationshipType)
        {
            ConcreteEntity = concreteEntity;
            RelatedTo = relatedTo;
            RelationshipType = relationshipType;
        }
    }
}
