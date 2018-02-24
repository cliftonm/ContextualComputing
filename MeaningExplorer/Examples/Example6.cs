using Clifton.Meaning;

namespace MeaningExplorer.Examples.Example6
{
    class FirstName : IValueEntity { }
    class LastName : IValueEntity { }
    class EmployeeId : IValueEntity { }

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

    class PersonNameRelationship : IRelationship { }

    class EmployeeContext : Context
    {
        public EmployeeContext()
        {
            Declare<EmployeeId>().OneAndOnlyOne();
            Declare<PersonNameRelationship, EmployeeContext, PersonContext>("Person Name").Exactly(1).Coalesce();
        }
    }
}
