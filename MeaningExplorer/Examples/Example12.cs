using Clifton.Meaning;

namespace MeaningExplorer.Examples.Example12
{
    class FirstName : IValueEntity { }
    class LastName : IValueEntity { }
    class EmployeeId : IValueEntity { }
    class PhoneNumber : IValueEntity { }
    class EmailAddress : IValueEntity { }

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

    public class PhoneContext : Context
    {
        public PhoneContext()
        {
            Declare<PhoneNumber>("Phone");
        }
    }

    public class EmailContext : Context
    {
        public EmailContext()
        {
            Declare<EmailAddress>("Email");
        }
    }

    public class ContactContext : Context
    {
        public ContactContext()
        {
            Declare<PhoneContext>();
            Declare<EmailContext>();
        }
    }

    public class AddressBookContext : Context
    {
        public AddressBookContext()
        {
            Label = "Address Book";
            Declare<PersonContext>();
            Declare<ContactContext>();
        }
    }
}
