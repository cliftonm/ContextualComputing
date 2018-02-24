using Clifton.Meaning;

namespace MeaningExplorer.Examples.Example13
{
    class FirstName : IValueEntity { } //  SemanticType<FirstName, string>, IEntity { }
    class LastName : IValueEntity { } //  SemanticType<LastName, string>, IEntity { }

    public class ParentChildRelationship : IRelationship { }

    public class PersonNameContext : Context
    {
        public PersonNameContext()
        {
            Declare<FirstName>().OneAndOnlyOne();
            Declare<LastName>().OneAndOnlyOne();
        }
    }

    public class PersonContext : Context
    {
        public PersonContext()
        {
            Declare<PersonNameContext>().OneAndOnlyOne();
        }
    }

    public class ChildContext : Context
    {
        public ChildContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
        }
    }

    public class ParentContext : Context
    {
        public ParentContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
            Declare<ParentChildRelationship, ParentContext, ChildContext>().Max(5);//.AsGrid();
        }
    }
}
