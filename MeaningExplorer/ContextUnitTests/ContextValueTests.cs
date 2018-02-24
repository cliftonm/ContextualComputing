/* The MIT License (MIT)
* 
* Copyright (c) 2018 Marc Clifton
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

/* Code Project Open License (CPOL) 1.02
* https://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Meaning;

namespace ContextUnitTests
{
    class FirstName : IValueEntity { } // SemanticType<FirstName, string>, IEntity { }
    class LastName : IValueEntity { } // SemanticType<LastName, string>, IEntity { }

    public class PersonNameContext : Context
    {
        public PersonNameContext()
        {
            Declare<FirstName>().OneAndOnlyOne();
            Declare<LastName>().OneAndOnlyOne();
        }
    }

    public class EmployeeName : Context
    {
        public EmployeeName()
        {
            Declare<PersonNameContext>().OneAndOnlyOne();
        }
    }

    public class AddressBookName : Context
    {
        public AddressBookName()
        {
            Declare<PersonNameContext>().OneAndOnlyOne();
        }
    }

    [TestClass]
    public class ContextValueTests
    {
        [TestMethod]
        public void SubtypeFieldCountTest()
        {
            var search = new PersonNameContext();
            var searchParser = new Parser();
            searchParser.Parse(search);

            Assert.IsTrue(searchParser.FieldContextPaths.Count == 2);
        }

        [TestMethod]
        public void TwoContextsShareValuesTest()
        {
            ContextValueDictionary cvd = new ContextValueDictionary();

            // An example of two different contexts having a PersonNameContext.
            var context1 = new EmployeeName();
            var context2 = new AddressBookName();

            // We add a third context that should not match the search criteria.
            var context3 = new EmployeeName();

            var parser1 = new Parser();
            var parser2 = new Parser();
            var parser3 = new Parser();

            parser1.Parse(context1);
            parser2.Parse(context2);
            parser3.Parse(context3);

            Assert.IsTrue(parser1.Groups.Count == 1);
            Assert.IsTrue(parser2.Groups.Count == 1);
            Assert.IsTrue(parser3.Groups.Count == 1);

            Assert.IsTrue(parser1.Groups[0].Fields.Count == 2);
            Assert.IsTrue(parser2.Groups[0].Fields.Count == 2);
            Assert.IsTrue(parser3.Groups[0].Fields.Count == 2);

            Assert.IsTrue(parser1.Groups[0].Fields[0].ContextPath.Count == 3);
            Assert.IsTrue(parser2.Groups[0].Fields[0].ContextPath.Count == 3);
            Assert.IsTrue(parser3.Groups[0].Fields[0].ContextPath.Count == 3);

            // Set the same values in the firstname/lastname fields of the two different contexts.
            var cv0fn = cvd.CreateValue<EmployeeName, PersonNameContext, FirstName>(parser1, "Marc");
            var cv0ln = cvd.CreateValue<EmployeeName, PersonNameContext, LastName>(parser1, "Clifton");

            var cv1fn = cvd.CreateValue<AddressBookName, PersonNameContext, FirstName>(parser2, "Marc");
            var cv1ln = cvd.CreateValue<AddressBookName, PersonNameContext, LastName>(parser2, "Clifton");

            cvd.CreateValue<EmployeeName, PersonNameContext, FirstName>(parser3, "Ian");
            cvd.CreateValue<EmployeeName, PersonNameContext, LastName>(parser3, "Clifton");

            // TODO: Tie in a context instance with a parser for that instance so we're working with a unified object.

            /*
            // Alternate way of doing this directly from the parser groups.

            var c1f1 = parser1.Groups[0].Fields[0];
            var c1f2 = parser1.Groups[0].Fields[1];
            var c2f1 = parser2.Groups[0].Fields[0];
            var c2f2 = parser2.Groups[0].Fields[1];
            var c3f1 = parser3.Groups[0].Fields[0];
            var c3f2 = parser3.Groups[0].Fields[1];

            cvd.Add(c1f1.CreateValue("Marc"));
            cvd.Add(c1f2.CreateValue("Clifton"));

            cvd.Add(c2f1.CreateValue("Marc"));
            cvd.Add(c2f2.CreateValue("Clifton"));

            // Create a first/last name context that should not match.
            cvd.Add(c3f1.CreateValue("Ian"));
            cvd.Add(c3f2.CreateValue("Clifton"));
            */

            // When we ask what contexts these values exist, we should get back two context paths
            // as the search criteria (below) exists in two different context.

            var search = new PersonNameContext();
            var searchParser = new Parser();
            searchParser.Parse(search);

            //var searchFirstNameField = searchParser.Groups[0].Fields[0];
            //var searchLastNameField = searchParser.Groups[0].Fields[1];

            //var cvFirstName = searchFirstNameField.CreateValue("Marc");
            //var cvLastName = searchLastNameField.CreateValue("Clifton");

            // Use the parser to create the search context -- if we use the context value dictionary, the 
            // context values get stored in the dictionary, which we don't want!

            var cvFirstName = searchParser.CreateValue<PersonNameContext, FirstName>("Marc");
            var cvLastName = searchParser.CreateValue<PersonNameContext, LastName>("Clifton");

            List<ContextNode> matches = cvd.Search(cvFirstName, cvLastName);

            Assert.IsTrue(matches.Count == 2);
            Assert.IsTrue(matches.Any(m => m.Parent.Type == typeof(EmployeeName)));
            Assert.IsTrue(matches.Any(m => m.Parent.Type == typeof(AddressBookName)));
            Assert.IsTrue(matches[0].Children[0].InstanceId == cv0fn.InstanceId);
            Assert.IsTrue(matches[0].Children[1].InstanceId == cv0ln.InstanceId);
            Assert.IsTrue(matches[1].Children[0].InstanceId == cv1fn.InstanceId);
            Assert.IsTrue(matches[1].Children[1].InstanceId == cv1ln.InstanceId);
            Assert.IsTrue(matches[0].Children.Any(c => c.ContextValue.Value == "Marc"));
            Assert.IsTrue(matches[0].Children.Any(c => c.ContextValue.Value == "Clifton"));
            Assert.IsTrue(matches[1].Children.Any(c => c.ContextValue.Value == "Marc"));
            Assert.IsTrue(matches[1].Children.Any(c => c.ContextValue.Value == "Clifton"));
        }

        [TestMethod]
        [ExpectedException(typeof(ContextValueDictionaryException))]
        public void CreatingDuplicateContextValuePathShouldAssertTest()
        {
            ContextValueDictionary cvd = new ContextValueDictionary();
            var context = new ParentContext();
            var parser = new Parser();
            parser.Parse(context);
            cvd.CreateValue<ParentContext, PersonContext, PersonNameContext, FirstName>(parser, "John");
            cvd.CreateValue<ParentContext, PersonContext, PersonNameContext, FirstName>(parser, "Jane");
        }
    }
}
