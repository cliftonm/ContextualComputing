using Clifton.Meaning;

namespace MeaningExplorer.Examples.Example8
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
    class EmergencyContactRelationship : IRelationship { }

    class EmergencyContactContext : Context
    {
        public EmergencyContactContext()
        {
            AddAbstraction<EmergencyContactContext, PersonContext>("Contact Name").Coalesce();
        }
    }

    class EmployeeContext : Context
    {
        public EmployeeContext()
        {
            Declare<EmployeeId>().OneAndOnlyOne();
            AddAbstraction<EmployeeContext, PersonContext>("Employee Name").Coalesce();
            Declare<EmergencyContactRelationship, EmployeeContext, EmergencyContactContext>("Emergency Contact").Min(1).Max(2);
        }
    }
}
