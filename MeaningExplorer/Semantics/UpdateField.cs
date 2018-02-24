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
