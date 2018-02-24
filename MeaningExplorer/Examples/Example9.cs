using Clifton.Meaning;

namespace MeaningExplorer.Examples.Example9
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

    class EmergencyContact : IEntity { }

    class EmployeeContext : Context
    {
        public EmployeeContext()
        {
            Declare<EmployeeId>().OneAndOnlyOne();
            Declare<EmergencyContactRelationship, EmployeeContext, EmergencyContact>("Emergency Contact").Min(1).Max(2);
            AddAbstraction<EmployeeContext, PersonContext>("Employee Name").Coalesce();
            AddAbstraction<EmergencyContact, PersonContext>("Contact Name").Coalesce();
        }
    }
}
