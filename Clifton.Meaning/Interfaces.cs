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
    public interface IEntity { }
    public interface IRelationship { }
    public interface IValueEntity : IEntity { }

    // General purpose relationships:
    public class Root : IRelationship { }
    public class HasA : IRelationship { }
    public class KindOf : IRelationship { }

    public interface IContext
    {
        string Label { get; set; }
        Lookup Lookup { get; }
        bool HasLookup { get; }
        IReadOnlyList<EntityDeclaration> RootEntities { get; }
        IEnumerable<AbstractionDeclaration> GetAbstractions();
        IEnumerable<AbstractionDeclaration> GetAbstractions(EntityDeclaration decl);
        IEnumerable<AbstractionDeclaration> GetAbstractions<T>() where T : IEntity;
        IEnumerable<AbstractionDeclaration> GetAbstractions(Type t);
        IEnumerable<AbstractionDeclaration> GetImplementations(EntityDeclaration decl);
        IEnumerable<AbstractionDeclaration> GetImplementations(Type t);
        IEnumerable<AbstractionDeclaration> GetImplementations<T>() where T : IEntity;
        IReadOnlyList<RelationshipDeclaration> Relationships { get; }
    }
}
