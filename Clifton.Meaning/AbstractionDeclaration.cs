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
    public class AbstractionDeclaration
    {
        public string Label { get; protected set; }
        public Type SubType { get; protected set; }
        public Type SuperType { get; protected set; }
        public bool ShouldCoalesceAbstraction { get; protected set; }

        public static AbstractionDeclaration Create<T1, T2>(string label = null) 
            where T1 : IEntity 
            where T2 : IEntity
        {
            return new AbstractionDeclaration(typeof(T1), typeof(T2), label);
        }

        public AbstractionDeclaration Coalesce()
        {
            ShouldCoalesceAbstraction = true;

            return this;
        }

        protected AbstractionDeclaration(Type subtype, Type supertype, string label)
        {
            Label = label;
            SubType = subtype;
            SuperType = supertype;
        }
    }
}
