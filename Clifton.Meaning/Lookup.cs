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
using System.Text;

using Clifton.Core.ExtensionMethods;

namespace Clifton.Meaning
{
    public abstract class LookupComponent
    {
        public abstract string Render(ContextNode contextNode, ContextValueDictionary cvd, int recNum, IReadOnlyList<ContextValue> contextValues);
    }

    public class LookupText : LookupComponent
    {
        public string Text { get; protected set; }

        public LookupText(string text)
        {
            Text = text;
        }

        public override string Render(ContextNode contextNode, ContextValueDictionary cvd, int recNum, IReadOnlyList<ContextValue> contextValues)
        {
            return Text;
        }
    }

    public class LookupEntity : LookupComponent
    {
        public Type ValueEntity { get; protected set; }

        public LookupEntity(Type t)
        {
            ValueEntity = t;
        }

        public override string Render(ContextNode contextNode, ContextValueDictionary cvd, int recNum, IReadOnlyList<ContextValue> contextValues)
        {
            var contextValue = contextValues.SingleOrDefault(cv => cv.Type == ValueEntity && cv.RecordNumber == recNum);
            string ret = String.Empty;

            if (contextValue != null)
            {
                ret = contextValue.Value;
            }

            return ret;
        }
    }

    public class Lookup
    {
        protected List<LookupComponent> components = new List<LookupComponent>();

        public Lookup Add(string text)
        {
            components.Add(new LookupText(text));

            return this;
        }

        public Lookup Add<T>() where T : IValueEntity
        {
            components.Add(new LookupEntity(typeof(T)));

            return this;
        }

        // For semanticly better readability?

        public Lookup And(string text)
        {
            components.Add(new LookupText(text));

            return this;
        }

        public Lookup And<T>() where T : IValueEntity
        {
            components.Add(new LookupEntity(typeof(T)));

            return this;
        }

        public string Render(ContextNode contextNode, ContextValueDictionary cvd, int recNum, IReadOnlyList<ContextValue> contextValues)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var component in components)
            {
                sb.Append(component.Render(contextNode, cvd, recNum, contextValues));
            }

            return sb.ToString();
        }

        public IEnumerable<LookupEntity> GetLookupEntities()
        {
            return components.Where(c => c is LookupEntity).Cast<LookupEntity>();
        }
    }
}
