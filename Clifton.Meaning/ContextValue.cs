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
    public class ContextValue
    {
        public Guid InstanceId { get; protected set; }
        public string Value { get; protected set; }
        public int RecordNumber { get; set; }       // Public setter should be used internally.
        public Type Type { get { return typePath.Last(); } }

        public IReadOnlyList<Guid> InstancePath { get { return instancePath; } }
        public IReadOnlyList<Type> TypePath { get { return typePath; } }

        protected List<Guid> instancePath = new List<Guid>();
        protected List<Type> typePath = new List<Type>();

        public ContextValue(string value, List<Guid> instancePath, List<Type> typePath, int recordNumber = 0)
        {
            InstanceId = instancePath.Last();
            Value = value;
            RecordNumber = recordNumber;
            this.instancePath = instancePath;
            this.typePath = typePath;
        }
    }
}
