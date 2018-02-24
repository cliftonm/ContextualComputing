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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Meaning;

namespace ContextUnitTests
{
    public class ChildContext : Context
    {
        public ChildContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
        }
    }

    public class ParentContext : Context
    {
        public ParentContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
            Declare<ChildContext>().ZeroOrMore();
        }
    }

    [TestClass]
    public class ContextSearchTests
    {
        [TestMethod]
        public void CreateContextFromDictionarySearchTest()
        {
            ContextValueDictionary cvd = new ContextValueDictionary();

            // An example of two different contexts having a PersonNameContext.
            var context1 = new EmployeeName();
            var parser1 = new Parser();
            parser1.Parse(context1);

            // Set the same values in the firstname/lastname fields of the two different contexts.
            var cv0fn = cvd.CreateValue<EmployeeName, PersonNameContext, FirstName>(parser1, "Marc");
            var cv0ln = cvd.CreateValue<EmployeeName, PersonNameContext, LastName>(parser1, "Clifton");

            var search = new PersonNameContext();
            var searchParser = new Parser();
            searchParser.Parse(search);

            var cvFirstName = searchParser.CreateValue<PersonNameContext, FirstName>("Marc");
            var cvLastName = searchParser.CreateValue<PersonNameContext, LastName>("Clifton");

            List<ContextNode> matches = cvd.Search(cvFirstName, cvLastName);

            Assert.IsTrue(matches.Count == 1);
            Assert.IsTrue(parser1.FieldContextPaths[0].Path.Count == 3);
            Assert.IsTrue(parser1.FieldContextPaths[1].Path.Count == 3);

            // TODO: We should be asserting the values from the match record!

            /*                    
            var (newParser, newContext) = cvd.CreateContext(matches[0]);

            // Verify that the parser's field context path ID's match the dictionary ID's and
            // the actual ContextValue matches the 

            //Assert.IsTrue(cv0fn.InstancePath[0] == newParser.FieldContextPaths[0].Path[0].InstanceId);
            //Assert.IsTrue(cv0fn.InstancePath[1] == newParser.FieldContextPaths[0].Path[1].InstanceId);
            //Assert.IsTrue(cv0fn.InstancePath[2] == newParser.FieldContextPaths[0].Path[2].InstanceId);

            //Assert.IsTrue(cv0ln.InstancePath[0] == newParser.FieldContextPaths[1].Path[0].InstanceId);
            //Assert.IsTrue(cv0ln.InstancePath[1] == newParser.FieldContextPaths[1].Path[1].InstanceId);
            //Assert.IsTrue(cv0ln.InstancePath[2] == newParser.FieldContextPaths[1].Path[2].InstanceId);

            Assert.IsTrue(newParser.FieldContextPaths[0].Field.ContextValue.Value == "Marc");
            Assert.IsTrue(newParser.FieldContextPaths[1].Field.ContextValue.Value == "Clifton");
            */
        }

        /// <summary>
        /// Verifies that sub-contexts along different branches of the search context are populated.
        /// </summary>
        [TestMethod]
        public void CreateContextFromMultiBranchDictionarySearchTest()
        {
            ContextValueDictionary cvd = new ContextValueDictionary();
            var context = new EmployeeContext();
            var parser = new Parser();
            parser.Parse(context);
            cvd.CreateValue<EmployeeContext, EmployeeId>(parser, "0001");
            cvd.CreateValue<EmployeeContext, PersonContext, PersonNameContext, FirstName>(parser, "Marc");
            cvd.CreateValue<EmployeeContext, PersonContext, PersonNameContext, LastName>(parser, "Clifton");

            var search = new PersonNameContext();
            var searchParser = new Parser();
            searchParser.Parse(search);
            var cvFirstName = searchParser.CreateValue<PersonNameContext, FirstName>("Marc");

            List<ContextNode> matches = cvd.Search(cvFirstName);
            Assert.IsTrue(matches.Count == 1);

            // TODO: We should be asserting the values from the match record!

            /*
            var (newParser, newContext) = cvd.CreateContext(matches[0]);
            Assert.IsTrue(newParser.FieldContextPaths[0].Field.ContextValue.Value == "0001");
            Assert.IsTrue(newParser.FieldContextPaths[1].Field.ContextValue.Value == "Marc");
            Assert.IsTrue(newParser.FieldContextPaths[2].Field.ContextValue.Value == "Clifton");
            */
        }

        /// <summary>
        /// Testing searches for a context that supports multiple rows, such as "children of a parent."
        /// </summary>
        [TestMethod]
        public void MultiRowSearchTest()
        {
            ContextValueDictionary cvd = new ContextValueDictionary();
            var context = new ParentContext();
            var parser = new Parser();
            parser.Parse(context);
            cvd.CreateValue<ParentContext, PersonContext, PersonNameContext, FirstName>(parser, "John");
            cvd.CreateValue<ParentContext, PersonContext, PersonNameContext, LastName>(parser, "Doe");
            cvd.CreateValue<ParentContext, ChildContext, PersonContext, PersonNameContext, FirstName>(parser, "Jane", 0);
            cvd.CreateValue<ParentContext, ChildContext, PersonContext, PersonNameContext, LastName>(parser, "Doe", 0);
            cvd.CreateValue<ParentContext, ChildContext, PersonContext, PersonNameContext, FirstName>(parser, "Joey", 1);
            cvd.CreateValue<ParentContext, ChildContext, PersonContext, PersonNameContext, LastName>(parser, "Doe", 1);

            var search = new PersonNameContext();
            var searchParser = new Parser();
            searchParser.Parse(search);
            var cvFirstName = searchParser.CreateValue<PersonNameContext, LastName>("Doe");

            List<ContextNode> matches = cvd.Search(cvFirstName);
        }
    }
}
