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

namespace Clifton.Meaning
{
    public class RelationshipDeclaration : IRelationship
    {
        public static RelationshipDeclaration NullRelationship
        {
            get
            {
                return new RelationshipDeclaration() { RelationshipType = typeof(NullRelationship) };
            }
        }

        public Type SourceType { get; protected set; }
        public Type TargetType { get; protected set; }
        public Type RelationshipType { get; protected set; }
        public int Minimum { get; protected set; }
        public int Maximum { get; protected set; }
        public bool RenderAsGrid { get; protected set; }        // rendering helper.
        public string Label { get; protected set; }
        public bool ShouldCoalesceRelationship { get; protected set; }

        public IReadOnlyList<Type> OrSourceTypes => orSourceTypes.AsReadOnly();
        public IReadOnlyList<Type> AndSourceTypes => andSourceTypes.AsReadOnly();

        public IReadOnlyList<Type> AllSourceTypes
        {
            get
            {
                // TODO: Modify this to include "required", "or", and "and" indicators.
                List<Type> types = new List<Type>();
                types.Add(SourceType);
                types.AddRange(orSourceTypes);
                types.AddRange(andSourceTypes);

                return types.AsReadOnly();
            }
        }

        protected List<Type> orSourceTypes = new List<Type>();
        protected List<Type> andSourceTypes = new List<Type>();

        public static RelationshipDeclaration Create<R, T, S>(string label = null)
            where R : IRelationship
            where T : IEntity
            where S : IEntity
        {
            return new RelationshipDeclaration(typeof(R), typeof(T), typeof(S), label);
        }

        protected RelationshipDeclaration()
        {
            Minimum = 0;
            Maximum = int.MaxValue;
        }

        protected RelationshipDeclaration(Type relationshipType, Type targetType, Type sourceType, string label = null)
        {
            Label = label;
            Minimum = 0;
            Maximum = int.MaxValue;
            RelationshipType = relationshipType;
            TargetType = targetType;
            SourceType = sourceType;
        }

        public RelationshipDeclaration Coalesce()
        {
            ShouldCoalesceRelationship = true;

            return this;
        }

        public RelationshipDeclaration OneOrMore()
        {
            Minimum = 1;

            return this;
        }

        public RelationshipDeclaration ZeroOrOne()
        {
            Minimum = 0;
            Maximum = 1;

            return this;
        }

        public RelationshipDeclaration ZeroOrMore()
        {
            Minimum = 0;
            Maximum = int.MaxValue;

            return this;
        }

        public RelationshipDeclaration OneAndOnlyOne()
        {
            Minimum = 1;
            Maximum = 1;

            return this;
        }

        public RelationshipDeclaration Exactly(int n)
        {
            Minimum = n;
            Maximum = n;

            return this;
        }

        public RelationshipDeclaration Min(int n)
        {
            Minimum = n;

            return this;
        }

        public RelationshipDeclaration Max(int n)
        {
            Maximum = n;

            return this;
        }

        public RelationshipDeclaration Or<E>() where E : IEntity
        {
            orSourceTypes.Add(typeof(E));

            return this;
        }

        public RelationshipDeclaration And<E>() where E : IEntity
        {
            andSourceTypes.Add(typeof(E));

            return this;
        }

        public RelationshipDeclaration AsGrid()
        {
            RenderAsGrid = true;

            return this;
        }
    }
}
