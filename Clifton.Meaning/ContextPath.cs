/* The MIT License (MIT)
* 
* Copyright (c) 2015 Marc Clifton
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

using System;

namespace Clifton.Meaning
{
    public class ContextPath
    {
        public enum ContextPathType
        {
            Root,
            Abstraction,
            Relationship,
            HasA,
            Field,
            Child,
        }

        public ContextPathType PathType { get; protected set; }
        public Type Type {get; protected set;}

        // We place the instance GUID in the context path rather than the context types (group, relationship, abstraction, field, etc.)
        // As it is the context path that logically tracks the instance graph.
        // public Guid InstanceId { get; protected set; }

        public ContextPath(ContextPathType pathType, Type t)
        {
            PathType = pathType;
            Type = t;
            // InstanceId = Guid.NewGuid();
        }

        /// <summary>
        /// Internal use only!
        /// </summary>
        //public void UpdateInstanceId(Guid id)
        //{
        //    InstanceId = id;
        //}
    }
}
