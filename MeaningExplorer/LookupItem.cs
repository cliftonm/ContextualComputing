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
