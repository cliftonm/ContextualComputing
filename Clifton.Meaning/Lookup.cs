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
