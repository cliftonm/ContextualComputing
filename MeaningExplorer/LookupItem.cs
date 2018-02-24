using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Clifton.Meaning;

namespace MeaningExplorer
{
    /// <summary>
    /// Simplified structure for creating a data dictionary used by Javascript
    /// to populate the value in the appropriate input control and set the
    /// instance ID path when a lookup is used.
    /// </summary>
    public class LookupItem
    {
        /// <summary>
        /// Will contain the value and instance path for the original full path context associated with the lookup.
        /// </summary>
        public List<Guid> OriginalContextInstancePath { get; protected set; }

        /// <summary>
        /// This is the new path for the input control which includes the parent hierarchy so we know 
        /// for which input control the particular sub-context lookup item is associated.
        /// In other words, the field's ID.
        /// </summary>
        public List<Guid> NewContextInstancePath { get; protected set; }

        /// <summary>
        /// The value associated with each component of the lookup record.
        /// </summary>
        public string Value { get; protected set; }

        public int LookupId { get; protected set; }

        [JsonIgnore]
        public Type ContextType { get; protected set; }

        [JsonIgnore]
        public ContextValue ContextValue { get; protected set; }

        public LookupItem(ContextValue contextValue, Type contextType, int lookupId)
        {
            ContextValue = contextValue;
            ContextType = contextType;
            Value = contextValue.Value;
            LookupId = lookupId;
            // OriginalContextInstancePath = contextValue.InstancePath.ToList();

            // Initialize for later completion.
            OriginalContextInstancePath = new List<Guid>();
            NewContextInstancePath = new List<Guid>();
        }
    }
}
