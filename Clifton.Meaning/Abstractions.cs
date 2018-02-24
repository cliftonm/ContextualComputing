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
using System.Collections.Generic;
using System.Linq;

namespace Clifton.Meaning
{
    public class Abstractions
    {
        protected List<AbstractionDeclaration> abstractions = new List<AbstractionDeclaration>();

        public AbstractionDeclaration Add<SubType, SuperType>(string label = null)
            where SubType : IEntity
            where SuperType : IEntity
        {
            var decl = AbstractionDeclaration.Create<SubType, SuperType>(label);
            abstractions.Add(decl);

            return decl;
        }

        public bool Exists<SubType, SuperType>()
            where SubType : IEntity
            where SuperType : IEntity
        {
            return abstractions.Any(a => a.SubType == typeof(SubType) && a.SuperType == typeof(SuperType));
        }

        public IEnumerable<AbstractionDeclaration> GetAbstractions()
        {
            return abstractions;
        }

        public IEnumerable<AbstractionDeclaration> GetAbstractions(Type subType)
        {
            return abstractions.Where(a => a.SubType == subType);
        }

        /// <summary>
        /// Returns the abstractions of a particular type.
        /// </summary>
        public IEnumerable<AbstractionDeclaration> GetAbstractions<T>() where T : IEntity
        {
            return abstractions.Where(a => a.SubType == typeof(T));
        }

        public IEnumerable<AbstractionDeclaration> GetImplementations(Type superType)
        {
            return abstractions.Where(a => a.SuperType == superType);
        }

        /// <summary>
        /// Returns the implementors of a particular abstraction.
        /// </summary>
        public IEnumerable<AbstractionDeclaration> GetImplementations<T>() where T : IEntity
        {
            return abstractions.Where(a => a.SuperType == typeof(T));
        }
    }
}
