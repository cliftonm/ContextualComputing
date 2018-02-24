using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using Clifton.Core.Assertions;
using Clifton.Core.ExtensionMethods;
using Clifton.Meaning;

namespace MeaningExplorer
{
    public static class Renderer
    {
        private static string CRLF = "\r\n";

        public enum Mode
        {
            NewRecord,
            Search,
            View,
        }

        public static string CreatePage(Parser parser, Mode mode = Mode.NewRecord, ContextValueDictionary cvd = null, Guid rootId = default(Guid))
        {
            StringBuilder sb = new StringBuilder();
            List<LookupItem> lookupDictionary = new List<LookupItem>();
            sb.StartHtml();

            sb.StartHead().
                Script("/js/wireUpValueChangeNotifier.js").
                Stylesheet("/css/styles.css").
                EndHead();

            sb.StartBody();

            foreach (var group in parser.Groups)
            {
                // Bizarre alternatives:
                //(group.ContextPath.Count() == 0 ? new Action(()=>sb.StartDiv()) : new Action(()=>sb.StartInlineDiv()))();
                //var throwAway = group.ContextPath.Count() == 0 ? sb.StartDiv() : sb.StartInlineDiv();

                if (group.ContextPath.Count() == 0)
                {
                    // sb.StartDiv();
                    sb.StartInlineDiv();
                }
                else
                {
                    sb.StartInlineDiv();
                    // sb.StartDiv();
                }

                // if (group.Relationship.GetType() != typeof(NullRelationship))
                if (group.Relationship.RelationshipType != typeof(NullRelationship))
                {
                    // TODO: If the mode is search, then we only want to display one instance, and not as a grid.
                    if (group.Relationship.Maximum < 6 && !group.Relationship.RenderAsGrid)
                    {
                        for (int recNum = 0; recNum < group.Relationship.Maximum; recNum++)
                        {
                            // multiple instances are stacked vertically.
                            sb.StartDiv();

                            // Horizontal stacking:
                            // sb.StartInlineDiv();

                            CreateFieldset(parser, sb, lookupDictionary, group, mode, cvd, rootId, recNum, group.Relationship.Minimum);
                            sb.EndDiv();
                        }
                    }
                    else
                    {
                        // TODO: Some arbitrary cutoff would change the fieldset to use a grid instead of discrete text input's.
                        CreateGrid(parser, sb, group, mode, cvd, rootId, group.Relationship.Minimum, group.Relationship.Maximum);
                        // CreateFieldset(parser, sb, group, mode, cvd, rootId);
                    }
                }
                else
                {
                    CreateFieldset(parser, sb, lookupDictionary, group, mode, cvd, rootId);
                }

                sb.EndDiv();

                // if there's no path, the next group does on the next line, otherwise the groups start stacking horizontally.
                if (group.ContextPath.Count() == 0)
                {
                    // https://css-tricks.com/the-how-and-why-of-clearing-floats/
                    sb.StartDiv().CustomStyle("clear", "both").EndDiv();
                }

                sb.Append("\r\n");
            }

            // https://css-tricks.com/the-how-and-why-of-clearing-floats/
            sb.StartDiv().CustomStyle("clear", "both").EndDiv();
            sb.StartDiv();

            switch (mode)
            {
                case Mode.Search:
                    sb.StartButton().ID("searchContext").Class("margintop10").Append("Search Context").EndButton();
                    sb.StartParagraph().Append("<b>Search Results:</b>").EndParagraph();
                    sb.StartParagraph().StartDiv().ID("searchResults").EndDiv().EndParagraph();
                    sb.StartParagraph().StartDiv().ID("viewSelectedItem").EndDiv().EndParagraph();
                    break;

                case Mode.NewRecord:
                    sb.StartButton().ID("newContext").Class("margintop10").Append("New Context").EndButton();
                    sb.StartParagraph().Append("<b>Dictionary:</b>").EndParagraph();
                    sb.StartParagraph().StartDiv().ID("dictionary").EndDiv().EndParagraph();

                    sb.StartParagraph().Append("<b>Type Nodes:</b>").EndParagraph();
                    sb.StartParagraph().StartDiv().ID("nodes").EndDiv().EndParagraph();
                    break;
            }

            sb.EndDiv();

            string jsonLookupDict = JsonConvert.SerializeObject(lookupDictionary, Formatting.Indented);

            sb.Append(CRLF);
            sb.StartScript().Append(CRLF).
                Javascript("(function() {wireUpValueChangeNotifier(); wireUpEvents();})();").
                Append(CRLF).
                Javascript("lookupDictionary = " + jsonLookupDict + ";").
                Append(CRLF).
                EndScript().
                Append(CRLF);
            sb.EndBody().EndHtml();

            return sb.ToString();
        }
        
        private static void CreateGrid(Parser parser, StringBuilder sb, Group group, Mode mode, ContextValueDictionary cvd, Guid rootId, int min, int max)
        {
            sb.StartFieldSet().Legend(group.Name);
            sb.StartTable();

            // Create headers
            sb.StartRow();

            foreach (var field in group.Fields)
            {
                sb.StartHeader().Append(field.Label).EndHeader();
            }

            sb.EndRow();

            // int recNum = 0;

            // For giggles, let's create 20 rows
            for (int recNum = 0; recNum < 20.Min(max); recNum++)
            {
                sb.StartRow();

                foreach (var field in group.Fields)
                {
                    string fieldValue = field.ContextValue?.Value ?? "";
                    List<Guid> instanceIdList = new List<Guid>();

                    // If viewing an existing record, don't create a new instanceID for the field.
                    if (mode != Mode.View)
                    {
                        // Here we want to bind the field's context path type list, up to the path that contains the context value,
                        // with ID's for the path types, creating ID's if the path type doesn't exist.  This ensures that all data instances
                        // exist within a single root context.  

                        instanceIdList = parser.CreateInstancePath(field.ContextPath.Select(cp => cp.Type));
                    }
                    else
                    {
                        Assert.That(cvd != null, "ContextValueDictionary must be provided when viewing existing context data.");
                        Assert.That(rootId != Guid.Empty, "A non-empty root ID is required when viewing existing context data.");

                        // In view mode, the context path to the field [context path - 1] should match a dictionary entry,
                        // then we need to acquire the instance ID for the record number and type of the actual ContextValue.
                        // If there is no record number entry, the value is assigned to "".
                        if (cvd.TryGetContextNode(field, rootId, recNum, out ContextNode cn, out List<Guid> instanceIdPath))
                        {
                            instanceIdList = instanceIdPath;
                            fieldValue = cn.ContextValue.Value;
                        }
                        else
                        {
                            fieldValue = "";
                        }
                    }

                    string cls = mode == Mode.Search ? "contextualValueSearch" : "contextualValue";
                    cls = cls + (recNum < min ? " requiredValue" : "");

                    sb.StartColumn().
                    AppendTextInput().
                    Class(cls).
                    ID(String.Join(".", instanceIdList)).
                    CustomAttribute("cvid", String.Join(".", instanceIdList)).
                    CustomAttribute("contextPath", String.Join("|", field.ContextPath.Select(p => p.Type.AssemblyQualifiedName))).
                    CustomAttribute("recordNumber", recNum.ToString()).
                    CustomAttribute("value", mode == Mode.View ? fieldValue : "").
                    // Not as pretty as a real tooltip, but works well.
                    // Also see: https://www.w3schools.com/howto/howto_css_tooltip.asp
                    CustomAttribute("title", String.Join("&#013;&#010;", field.ContextPath.Select(p => p.Type.Name))).
                    // CustomAttribute("title", String.Join("&#013;&#010;", field.ContextPath.Select(p => p.InstanceId))).
                    EndColumn();
                }

                sb.EndRow();
            }

            sb.EndTable();
        }
        

        private static void CreateFieldset(Parser parser, StringBuilder sbHtml, List<LookupItem> lookupDictionary, Group group, Mode mode, ContextValueDictionary cvd, Guid rootId, int recNum = 0, int min = 0)
        {
            sbHtml.StartFieldSet().Legend(group.Name);
            List<LookupItem> lookupItems = new List<LookupItem>();

            if (mode != Mode.View)
            {
                lookupItems.AddRange(RenderOptionalLookup(sbHtml, parser, group, cvd));
                lookupDictionary.AddRange(lookupItems);
            }

            sbHtml.StartTable();

            foreach (var field in group.Fields)
            {
                string fieldValue = field.ContextValue?.Value ?? "";
                List<Guid> instanceIdList = new List<Guid>();

                // If viewing an existing record, don't create a new instanceID for the field.
                if (mode != Mode.View)
                {
                    // Here we want to bind the field's context path type list, up to the path that contains the context value,
                    // with ID's for the path types, creating ID's if the path type doesn't exist.  This ensures that all data instances
                    // exist within a single root context.  
                    instanceIdList = parser.CreateInstancePath(field.ContextPath.Select(cp => cp.Type));
                    LookupItemLateBinding(field, lookupItems, instanceIdList);
                }
                else
                {
                    Assert.That(cvd != null, "ContextValueDictionary must be provided when viewing existing context data.");
                    Assert.That(rootId != Guid.Empty, "A non-empty root ID is required when viewing existing context data.");

                    // In view mode, the context path to the field [context path - 1] should match a dictionary entry,
                    // then we need to acquire the instance ID for the record number and type of the actual ContextValue.
                    // If there is no record number entry, the value is assigned to "".
                    if (cvd.TryGetContextNode(field, rootId, recNum, out ContextNode cn, out List<Guid> instanceIdPath))
                    {
                        instanceIdList = instanceIdPath;
                        fieldValue = cn.ContextValue.Value;
                    }
                    else
                    {
                        fieldValue = "";
                    }
                }

                sbHtml.StartRow();

                string cls = mode == Mode.Search ? "contextualValueSearch" : "contextualValue";
                cls = cls + (recNum < min ? " requiredValue" : "");

                sbHtml.StartColumn().
                    AppendLabel(field.Label + ":").
                    NextColumn().
                    AppendTextInput().
                    Class(cls).
                    ID(String.Join(".", instanceIdList)).
                    CustomAttribute("cvid", String.Join(".", instanceIdList)).
                    CustomAttribute("contextPath", String.Join("|", field.ContextPath.Select(p => p.Type.AssemblyQualifiedName))).
                    CustomAttribute("recordNumber", recNum.ToString()).
                    CustomAttribute("value", mode == Mode.View ? fieldValue : "").
                    // Not as pretty as a real tooltip, but works well.
                    // Also see: https://www.w3schools.com/howto/howto_css_tooltip.asp
                    CustomAttribute("title", String.Join("&#013;&#010;", field.ContextPath.Select(p => p.Type.Name))).
                    // CustomAttribute("title", String.Join("&#013;&#010;", field.ContextPath.Select(p => p.InstanceId))).
                    EndColumn();

                sbHtml.EndRow();
            }

            sbHtml.EndTable();
            sbHtml.EndFieldSet();
            sbHtml.Append("\r\n");
        }

        /// <summary>
        /// Adds a "select" section with lookup items that are rendered by the
        /// lookup rendering declaration for a particular context, returning the
        /// lookup items that were created.
        /// </summary>
        private static List<LookupItem> RenderOptionalLookup(StringBuilder sb, Parser parser, Group group, ContextValueDictionary cvd)
        {
            List<LookupItem> lookupItems = new List<LookupItem>();
            var groupContextType = group.ContextType;
            int lookupId = 0;

            // If there's an associated lookup, query the dictionary for instances of this context (no filtering for now!)
            // and render the values as declared by the lookup.  If no dictionary is provided, lookups are not possible.
            if (cvd != null && parser.HasLookup(groupContextType))
            {
                Lookup lookup = parser.GetLookup(groupContextType);
                IReadOnlyList<ContextNode> contextNodes = cvd.GetContextNodes(groupContextType);

                if (contextNodes.Count > 0)
                {
                    // 1. Iterate each context node.
                    // 2. Navigate the children until we get concrete (not null) ContextValue's.
                    // 3. Get the distinct record numbers of these.
                    // 4. Itereate the record numbers
                    // 5. Render the lookup for that context node and record number.
                    // 6. For each actual IValueEntity (contained in a LookupEntity) acquire the ContextValue and add
                    // an entry in the lookupItems collection, which will serve as dictionary for how a selected item
                    // populates its associated input control.
                    List<(string, int)> lookups = new List<(string, int)>();

                    foreach (var cn in contextNodes) // 1
                    {
                        IReadOnlyList<ContextValue> contextValues = cvd.GetContextValues(cn); // 2
                        var recnums = contextValues.Select(cv => cv.RecordNumber).Distinct(); // 3

                        foreach (var recnum in recnums) // 4
                        {
                            string record = lookup.Render(cn, cvd, recnum, contextValues); // 5
                            lookups.Add((record, lookupId));

                            // 6
                            var lookupEntities = lookup.GetLookupEntities();

                            foreach (var lookupEntity in lookupEntities)
                            {
                                var contextValue = contextValues.SingleOrDefault(cv => cv.Type == lookupEntity.ValueEntity && cv.RecordNumber == recnum);

                                if (contextValue != null)
                                {
                                    // The full instance ID path is path, up to the lookup sub-context, in
                                    // which this sub-context is contained.  However, we don't have this value yet!
                                    // It is either generated (new context) or assigned (existing context)
                                    // so we need to defer this until we know the instance ID path we're using.
                                    lookupItems.Add(new LookupItem(contextValue, groupContextType, lookupId));
                                }
                            }

                            ++lookupId;
                        }
                    }

                    sb.Append("Lookup: ");
                    sb.StartSelect().OnChange("populateFromLookup(this)");
                    // Placeholder so user has to actively select a lookup because because otherwise the first item
                    // appears selected, and clicking on the option doesn't trigger the event.
                    sb.Option("Choose item:");
                    // lk is a tuple (lookup text, lookup ID)
                    lookups.ForEach(lk => sb.Option(lk.Item1, lk.Item2.ToString()));
                    sb.EndSelect();
                }
            }

            return lookupItems;
        }

        private static void LookupItemLateBinding(Field field, List<LookupItem> lookupItems, List<Guid> instanceIdList)
        {
            // Late binding of lookupItem's FullContextInstancePath:
            foreach (var lookupItem in lookupItems.Where(li => li.ContextValue.Type == field.ContextPath.Last().Type))
            {
                lookupItem.OriginalContextInstancePath.AddRange(instanceIdList);
                
                // Find where in the entire path for this field the lookup value sub-context starts.
                int idx = field.ContextPath.IndexOf(cp => cp.Type == lookupItem.ContextType);

                // The full context instance ID path starts with these base ID's...
                lookupItem.NewContextInstancePath.AddRange(lookupItem.OriginalContextInstancePath.Take(idx));

                // ...and finishes with the context ID's for the lookup.
                lookupItem.NewContextInstancePath.AddRange(lookupItem.ContextValue.InstancePath.Skip(idx));
            }
        }
    }
}
