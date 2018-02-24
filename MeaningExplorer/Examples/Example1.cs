using Clifton.Meaning;

namespace MeaningExplorer.Examples.Example1
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
}
