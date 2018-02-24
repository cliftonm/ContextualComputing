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
    public class EntityDeclaration
    {
        public Type EntityType { get; protected set; }
        public int Minimum { get; protected set; }
        public int Maximum { get; protected set; }
        public string Label { get; protected set; }

        public static EntityDeclaration Create<E>(string label = null) where E : IEntity
        {
            return new EntityDeclaration(typeof(E), label);
        }

        public EntityDeclaration OneOrMore()
        {
            Minimum = 1;

            return this;
        }

        public EntityDeclaration ZeroOrOne()
        {
            Minimum = 0;
            Maximum = 1;

            return this;
        }

        public EntityDeclaration ZeroOrMore()
        {
            Minimum = 0;
            Maximum = int.MaxValue;

            return this;
        }

        public EntityDeclaration OneAndOnlyOne()
        {
            Minimum = 1;
            Maximum = 1;

            return this;
        }

        public EntityDeclaration Exactly(int n)
        {
            Minimum = n;
            Maximum = n;

            return this;
        }

        public EntityDeclaration Min(int n)
        {
            Minimum = n;

            return this;
        }

        public EntityDeclaration Max(int n)
        {
            Maximum = n;

            return this;
        }

        public bool Validate(List<ContextEntity> entities)
        {
            var matches = entities.Where(e => e.ConcreteEntity.GetType() == EntityType);
            int c = matches.Count();

            return c >= Minimum && c <= Maximum;
        }

        protected EntityDeclaration(Type entityType, string label)
        {
            Label = label;
            Minimum = 0;
            Maximum = int.MaxValue;
            EntityType = entityType;
        }
    }
}
