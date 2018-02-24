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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Meaning;

namespace ContextUnitTests
{
    class MultipleAbstractions : Context
    {
        public MultipleAbstractions()
        {
            AddAbstraction<MultipleAbstractions, ContextA>();
            AddAbstraction<MultipleAbstractions, ContextB>();
        }
    }

    public class PersonContext : Context
    {
        public PersonContext()
        {
            Declare<PersonNameContext>().OneAndOnlyOne();
        }
    }

    public class RecursiveRelationship : IRelationship { }

    public class RecursiveContext : Context
    {
        public RecursiveContext()
        {
            Declare<RecursiveRelationship, RecursiveContext, RecursiveContext>();
        }
    }

    class EmergencyContactRelationship : IRelationship { }
    class EmployeeId : IValueEntity { }

    class FieldA : IValueEntity { } // SemanticType<FieldA, int>, IEntity { }
    class FieldB : IValueEntity { } // SemanticType<FieldB, int>, IEntity { }
    class FieldC : IValueEntity { } // SemanticType<FieldC, int>, IEntity { }

    class ABRelationship : IRelationship { }

    class ContextA : Context
    { 
        public ContextA()
        {
            Declare<FieldA>();
        }
    }

    class ContextB : Context
    {
        public ContextB()
        {
            Declare<FieldB>();
        }
    }

    class AbstractContext : Context
    {
        public AbstractContext()
        {
            Declare<FieldB>();
        }
    }

    class SimpleContext : Context
    {
        public SimpleContext()
        {
            Declare<FieldA>();
        }
    }

    class ContextWithAbstraction : Context
    {
        public ContextWithAbstraction()
        {
            Declare<FieldA>();
            AddAbstraction<ContextWithAbstraction, AbstractContext>();
        }
    }

    class SelfContextWithRelationship : Context
    {
        public SelfContextWithRelationship()
        {
            Declare<FieldA>();
            Declare<ABRelationship, SelfContextWithRelationship, ContextB>();
        }
    }

    class ContextWithRelationship : Context
    {
        public ContextWithRelationship()
        {
            Declare<ContextA>();
            Declare<ABRelationship, ContextA, ContextB>();
        }
    }

    class FieldWithRelationship : Context
    {
        public FieldWithRelationship()
        {
            Declare<FieldA>();
            Declare<ABRelationship, FieldA, ContextB>();
        }
    }

    class ContextFieldWithRelationship : Context
    {
        public ContextFieldWithRelationship()
        {
            Declare<ContextA>();
            Declare<ABRelationship, FieldA, ContextB>();
        }
    }

    class DisassociatedRelationship : Context
    {
        public DisassociatedRelationship()
        {
            Declare<FieldB>();
            // or ContextA, ContextB, because FieldB is not on the left-side of the declared relationship.
            Declare<ABRelationship, FieldA, ContextB>();
        }
    }

    class DisassociatedAbstraction : Context
    {
        public DisassociatedAbstraction()
        {
            Declare<FieldB>();
            // or ContextA, ContextB, because FieldB is not a sub-type of the abstraction.
            AddAbstraction<FieldA, ContextB>();
        }
    }

    class AbstractionAndRelationshipContext : Context
    {
        public AbstractionAndRelationshipContext()
        {
            Declare<FieldA>();
            AddAbstraction<AbstractionAndRelationshipContext, PersonContext>("Name").Coalesce();
            Declare<EmergencyContactRelationship, AbstractionAndRelationshipContext, PersonContext>("Emergency Contact");
        }
    }

    class EmployeeContext : Context
    {
        public EmployeeContext()
        {
            Declare<EmployeeId>().OneAndOnlyOne();
            AddAbstraction<EmployeeContext, PersonContext>("Employee Name").Coalesce();
        }
    }

    class EmployeeContractContext : Context
    {
        public EmployeeContractContext()
        {
            Label = "Employee Contract";
            Declare<EmployeeContext>("Employee").OneAndOnlyOne();
            Declare<InsuredSpouse, EmployeeContext, Spouse>("Spouse").ZeroOrOne();
            //Declare<InsuredChild, EmployeeContext, Child>("Child").ZeroOrMore();
            //Declare<EmergencyContactRelationship, EmployeeContext, PersonContext>("Emergency Contact").Min(1).Max(2);
            //Declare<Beneficiary, EmployeeContext, PersonContext>("Beneficiary").Or<BusinessContext>();
            //Declare<SupervisorRelationship, EmployeeContext, PersonContext>("Supervisor").Min(1).Max(3);

            AddAbstraction<Spouse, PersonContext>("Spouse Name");
            //AddAbstraction<Child, PersonContext>("Child Name");
            //AddAbstraction<Supervisor, PersonContext>("Supervisor Name");
        }
    }

    class SubContext : Context
    {
        public SubContext()
        {
            Declare<FieldA>().OneAndOnlyOne();
            AddAbstraction<SubContext, PersonContext>("Name");
        }
    }

    public class SuperContext : Context
    {
        public SuperContext()
        {
            Declare<SubContext>("SubContext").OneAndOnlyOne();
        }
    }

    public class Context1 : Context
    {
        public Context1()
        {
            Declare<FieldA>();
        }
    }

    class PhoneNumber : IValueEntity { }
    class EmailAddress : IValueEntity { }

    public class PhoneContext : Context
    {
        public PhoneContext()
        {
            Declare<PhoneNumber>("Phone:");
        }
    }

    public class EmailContext : Context
    {
        public EmailContext()
        {
            Declare<EmailAddress>("Email:");
        }
    }

    public class ContactContext : Context
    {
        public ContactContext()
        {
            Declare<PhoneContext>("Phone");
            Declare<EmailContext>("Email");
        }
    }
    public class SubContextsTest : Context
    {
        public SubContextsTest()
        {
            Declare<PersonContext>();
            Declare<ContactContext>();
        }
    }

    [TestClass]
    public class ContextPathTests
    {
        protected Parser parser;

        [TestInitialize]
        public void Init()
        {
            parser = new Parser();
        }

        [TestMethod]
        public void OneFieldInContextTest()
        {
            parser.Parse(new SimpleContext());
            Assert.IsTrue(parser.AreDeclarationsValid);

            Assert.IsTrue(parser.Groups.Count == 1, "Expected 1 group.");
            Assert.IsTrue(parser.Groups[0].Name == nameof(SimpleContext));
            Assert.IsTrue(parser.Groups[0].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[0].Fields[0].Label == nameof(FieldA), "Expected FieldA");
            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath.Count == 2);

            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath[0].Type == typeof(SimpleContext), "Root path should be of type SimpleContext");
        }

        [TestMethod]
        public void AbstractContextTest()
        {
            parser.Parse(new ContextWithAbstraction());
            Assert.IsTrue(parser.AreDeclarationsValid);

            Assert.IsTrue(parser.Groups.Count == 2, "Expected 2 groups.");
            Assert.IsTrue(parser.Groups[0].Name == nameof(ContextWithAbstraction));
            Assert.IsTrue(parser.Groups[1].Name == nameof(AbstractContext));
            Assert.IsTrue(parser.Groups[0].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[1].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[0].Fields[0].Label == nameof(FieldA), "Expected FieldA");
            Assert.IsTrue(parser.Groups[1].Fields[0].Label == nameof(FieldB), "Expected FieldB");
            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath.Count == 2);
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath.Count == 3);

            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath[0].Type == typeof(ContextWithAbstraction), "Root path should be of type ContextWithAbstraction");

            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[0].Type == typeof(ContextWithAbstraction), "Root path should be of type ContextWithAbstraction");
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[1].Type == typeof(AbstractContext), "Second context path entry should be of type AbstractContext");
        }

        [TestMethod]
        public void ContextWithAbstractionAndRelationshipTest()
        {
            parser.Parse(new AbstractionAndRelationshipContext());
            Assert.IsTrue(parser.AreDeclarationsValid);
            Assert.IsTrue(parser.Groups.Count == 2, "Expected 2 groups.");
            Assert.IsTrue(parser.Groups[0].Fields.Count == 3, "Expected 3 fields.");
            Assert.IsTrue(parser.Groups[1].Fields.Count == 2, "Expected 2 fields.");
        }

        [TestMethod]
        public void SubcontextWithSelfAbstractionTest()
        {
            parser.Parse(new SuperContext());
            Assert.IsTrue(parser.AreDeclarationsValid);
            Assert.IsTrue(parser.Groups.Count == 2, "Expected 2 groups.");
            Assert.IsTrue(parser.Groups[0].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[1].Fields.Count == 2, "Expected 2 fields.");
        }

        [TestMethod]
        public void ContextWithAbstractionAndRelationTest()
        {
            parser.Parse(new EmployeeContractContext());
            Assert.IsTrue(parser.AreDeclarationsValid);
            Assert.IsTrue(parser.Groups.Count == 2, "Expected 2 groups.");
            Assert.IsTrue(parser.Groups[0].Fields.Count == 3, "Expected 3 fields.");
            Assert.IsTrue(parser.Groups[1].Fields.Count == 2, "Expected 2 fields.");
        }

        [TestMethod]
        public void MultipleAbstractsTest()
        {
            parser.Parse(new MultipleAbstractions());
            Assert.IsTrue(parser.AreDeclarationsValid);

            Assert.IsTrue(parser.Groups.Count == 3, "Expected 3 groups.");
            Assert.IsTrue(parser.Groups[0].Fields.Count == 0, "First group should not have any fields.");
            Assert.IsTrue(parser.Groups[1].Fields.Count == 1, "Second group should have one field.");
            Assert.IsTrue(parser.Groups[2].Fields.Count == 1, "Third group should have one field.");

            Assert.IsTrue(parser.Groups[1].Fields[0].Label == nameof(FieldA));
            Assert.IsTrue(parser.Groups[2].Fields[0].Label == nameof(FieldB));

            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[0].Type == typeof(MultipleAbstractions));
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[1].Type == typeof(ContextA));

            Assert.IsTrue(parser.Groups[2].Fields[0].ContextPath[0].Type == typeof(MultipleAbstractions));
            Assert.IsTrue(parser.Groups[2].Fields[0].ContextPath[1].Type == typeof(ContextB));
        }

        [TestMethod]
        public void SelfRelationshipTest()
        {
            parser.Parse(new SelfContextWithRelationship());
            Assert.IsTrue(parser.AreDeclarationsValid);

            Assert.IsTrue(parser.Groups.Count == 2, "Expected 2 groups.");
            Assert.IsTrue(parser.Groups[0].Name == nameof(SelfContextWithRelationship));
            Assert.IsTrue(parser.Groups[1].Name == nameof(ContextB));
            Assert.IsTrue(parser.Groups[0].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[1].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[0].Fields[0].Label == nameof(FieldA), "Expected FieldA");
            Assert.IsTrue(parser.Groups[1].Fields[0].Label == nameof(FieldB), "Expected FieldB");
            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath.Count == 2);
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath.Count == 3);

            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath[0].Type == typeof(SelfContextWithRelationship));

            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[0].Type == typeof(SelfContextWithRelationship));
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[1].Type == typeof(ContextB));
        }

        [TestMethod]
        public void ContextRelationshipTest()
        {
            parser.Parse(new ContextWithRelationship());
            Assert.IsTrue(parser.AreDeclarationsValid);

            Assert.IsTrue(parser.Groups.Count == 2, "Expected 2 groups.");
            Assert.IsTrue(parser.Groups[0].Name == nameof(ContextWithRelationship));
            Assert.IsTrue(parser.Groups[1].Name == nameof(ContextB));
            Assert.IsTrue(parser.Groups[0].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[1].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[0].Fields[0].Label == nameof(FieldA), "Expected FieldA");
            Assert.IsTrue(parser.Groups[1].Fields[0].Label == nameof(FieldB), "Expected FieldB");
            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath.Count == 3);
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath.Count == 3);

            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath[0].Type == typeof(ContextWithRelationship));

            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[0].Type == typeof(ContextWithRelationship));
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[1].Type == typeof(ContextB));
        }

        [TestMethod]
        public void FieldRelationshipTest()
        {
            parser.Parse(new FieldWithRelationship());
            Assert.IsTrue(parser.AreDeclarationsValid);

            Assert.IsTrue(parser.Groups.Count == 2, "Expected 2 groups.");
            Assert.IsTrue(parser.Groups[0].Name == nameof(FieldWithRelationship));
            Assert.IsTrue(parser.Groups[1].Name == nameof(ContextB));
            Assert.IsTrue(parser.Groups[0].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[1].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[0].Fields[0].Label == nameof(FieldA), "Expected FieldA");
            Assert.IsTrue(parser.Groups[1].Fields[0].Label == nameof(FieldB), "Expected FieldB");
            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath.Count == 2);
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath.Count == 3);

            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath[0].Type == typeof(FieldWithRelationship));

            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[0].Type == typeof(FieldWithRelationship));
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[1].Type == typeof(ContextB));
        }

        [TestMethod]
        public void ContextFieldWithRelationship()
        {
            parser.Parse(new ContextFieldWithRelationship());
            Assert.IsTrue(parser.AreDeclarationsValid);

            Assert.IsTrue(parser.Groups.Count == 2, "Expected 2 groups.");
            Assert.IsTrue(parser.Groups[0].Name == nameof(ContextFieldWithRelationship));
            Assert.IsTrue(parser.Groups[1].Name == nameof(ContextB));
            Assert.IsTrue(parser.Groups[0].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[1].Fields.Count == 1, "Expected 1 field.");
            Assert.IsTrue(parser.Groups[0].Fields[0].Label == nameof(FieldA), "Expected FieldA");
            Assert.IsTrue(parser.Groups[1].Fields[0].Label == nameof(FieldB), "Expected FieldB");
            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath.Count == 3);
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath.Count == 3);

            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath[0].Type == typeof(ContextFieldWithRelationship));
            Assert.IsTrue(parser.Groups[0].Fields[0].ContextPath[1].Type == typeof(ContextA));

            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[0].Type == typeof(ContextFieldWithRelationship));
            Assert.IsTrue(parser.Groups[1].Fields[0].ContextPath[1].Type == typeof(ContextB));
        }

        [TestMethod]
        public void DisassociatedRelationshipTest()
        {
            parser.Parse(new DisassociatedRelationship());
            Assert.IsFalse(parser.AreDeclarationsValid, "Relationships should not be valid.");
        }

        [TestMethod]
        public void DisassociatedAbstractionTest()
        {
            parser.Parse(new DisassociatedAbstraction());
            Assert.IsFalse(parser.AreDeclarationsValid, "Abstractions should not be valid.");
        }

        [TestMethod]
        public void TwoSubcontextDeclarationsTest()
        {
            parser.Parse(new SubContextsTest());
            Assert.IsTrue(parser.Groups.Count == 1);
            Assert.IsTrue(parser.Groups[0].Fields.Count == 4);
        }

        [TestMethod, ExpectedException(typeof(ContextException))]
        public void RecursiveContextTest()
        {
            parser.Parse(new RecursiveContext());
        }
    }
}
