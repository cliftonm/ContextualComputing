using Clifton.Meaning;

namespace MeaningExplorer.Examples.Example3
{
    class FirstName : IValueEntity { }
    class LastName : IValueEntity { }

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

    class EmployeeContext : Context
    {
        public EmployeeContext()
        {
            AddAbstraction<EmployeeContext, PersonContext>("Employee Name");
        }
    }
}
