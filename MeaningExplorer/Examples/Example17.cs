using Clifton.Meaning;

namespace MeaningExplorer.Examples.Example17
{
    class FirstName : IValueEntity { }
    class LastName : IValueEntity { }

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
            LookupRenderer<FirstName>().And(" ").And<LastName>();
        }
    }

    public class FatherContext : Context
    {
        public FatherContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
            Declare<ParentChildRelationship, FatherContext, ChildContext>().OneAndOnlyOne();
        }
    }

    public class MotherContext : Context
    {
        public MotherContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
            Declare<ParentChildRelationship, MotherContext, ChildContext>().OneAndOnlyOne();
        }
    }
}
