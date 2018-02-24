using Clifton.Meaning;

namespace MeaningExplorer.Examples.Example16
{
    class FirstName : IValueEntity { } //  SemanticType<FirstName, string>, IEntity { }
    class LastName : IValueEntity { } //  SemanticType<LastName, string>, IEntity { }

    public class ChildParentRelationship : IRelationship { }
    public class PersonFatherRelationship : IRelationship { }
    public class PersonMotherRelationship : IRelationship { }
    public class FatherPersonRelationship : IRelationship { }
    public class MotherPersonRelationship : IRelationship { }

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
            Declare<PersonFatherRelationship, PersonContext, FatherContext>().OneAndOnlyOne();
            Declare<PersonMotherRelationship, PersonContext, MotherContext>().OneAndOnlyOne();
        }
    }

    public class FatherContext : Context
    {
        public FatherContext()
        {
            Declare<FatherPersonRelationship, FatherContext, PersonContext>().OneAndOnlyOne();
        }
    }

    public class MotherContext : Context
    {
        public MotherContext()
        {
            Declare<MotherPersonRelationship, MotherContext, PersonContext>().OneAndOnlyOne();
        }
    }
}
