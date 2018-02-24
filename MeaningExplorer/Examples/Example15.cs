using Clifton.Meaning;

namespace MeaningExplorer.Examples.Example15
{
    class FirstName : IValueEntity { } //  SemanticType<FirstName, string>, IEntity { }
    class LastName : IValueEntity { } //  SemanticType<LastName, string>, IEntity { }

    public class ChildParentRelationship : IRelationship { }

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
            Declare<ChildParentRelationship, ChildContext, FatherContext>().OneAndOnlyOne();
            Declare<ChildParentRelationship, ChildContext, MotherContext>().OneAndOnlyOne();
        }
    }

    public class FatherContext : Context
    {
        public FatherContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
        }
    }

    public class MotherContext : Context
    {
        public MotherContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
        }
    }
}
