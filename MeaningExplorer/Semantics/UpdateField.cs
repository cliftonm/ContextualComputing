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

using System.Collections.Generic;
using Clifton.WebInterfaces;

namespace MeaningExplorer.Semantics
{
    public class UpdateField : SemanticRoute
    {
        public string Value { get; set; }
        public string ID { get; set; }
        public string TypePath { get; set; }
        public int RecordNumber { get; set; }
    }

    public class GetDictionary : SemanticRoute
    {
        public bool ShowInstanceIDs { get; set; }
    }

    public class GetDictionaryTreeHtml : SemanticRoute
    {
        public bool ShowInstanceIDs { get; set; }
    }

    public class GetDictionaryNodesHtml : SemanticRoute { }

    public class RenderContext : SemanticRoute
    {
        public string ContextName { get; set; }
        public bool IsSearch { get; set; }
    }

    public class RenderSearchContext : SemanticRoute
    {
        public string ContextName { get; set; }
    }

    public class SearchField
    {
        public string Value { get; set; }
        public string ID { get; set; }
        public string TypePath { get; set; }
    }

    public class SearchContext : SemanticRoute
    {
        public List<SearchField> SearchFields { get; set; }
    }

    public class ViewContext : SemanticRoute
    {
        public string InstancePath { get; set; }
    }
}
