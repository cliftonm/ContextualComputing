﻿/* The MIT License (MIT)
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

using Clifton.Meaning;

namespace MeaningExplorer
{
    // A concrete field implements a semantic type and is also an entity.
    class BusinessName : IValueEntity { } // SemanticType<BusinessName, string>, IEntity { }
    class FirstName : IValueEntity { } //  SemanticType<FirstName, string>, IEntity { }
    class LastName : IValueEntity { } //  SemanticType<LastName, string>, IEntity { }
    class EmployeeId : IValueEntity { } //  SemanticType<EmployeeId, string>, IEntity { }

    class PhoneNumber : IValueEntity { }
    class EmailAddress : IValueEntity { }

    // Entities never have content.  They don't derive from Context, so there's nothing to declare.
    // But entities can have abstractions that are contexts.
    class Spouse : IEntity { }
    class Child : IEntity { }
    class Supervisor : IEntity { }
    class NonProfitBusiness : IEntity { }

    class PersonNameRelationship : IRelationship { }

    class EmergencyContactRelationship : IRelationship { }
    class InsuredSpouse : IRelationship { }
    class InsuredChild : IRelationship { }
    class Beneficiary : IRelationship { }
    class SupervisorRelationship : IRelationship { }

    public class PersonNameContext : Context
    {
        public PersonNameContext()
        {
            Declare<FirstName>("First Name").OneAndOnlyOne();
            Declare<LastName>("Last Name").OneAndOnlyOne();
        }
    }

    public class BusinessContext : Context
    {
        public BusinessContext()
        {
            Declare<BusinessName>().OneAndOnlyOne();
        }
    }

    // class EmergencyContact : IEntity { }

    class EmployeeContext : Context
    {
        public EmployeeContext()
        {
            Declare<EmployeeId>().OneAndOnlyOne();
            AddAbstraction<EmployeeContext, PersonContext>("Employee Name").Coalesce();
            //Declare<EmergencyContactRelationship, EmployeeContext, EmergencyContact>("Emergency Contact").Min(1).Max(2);
            //AddAbstraction<EmergencyContact, PersonContext>("Contact Name").Coalesce();
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

    public class EmployeeContractContext : Context
    {
        public EmployeeContractContext()
        {
            Label = "Employee Contract";
            Declare<EmployeeContext>("Employee").OneAndOnlyOne();
            Declare<InsuredSpouse, EmployeeContext, Spouse>("Spouse").ZeroOrOne();
            Declare<InsuredChild, EmployeeContext, Child>("Child").ZeroOrMore();
            Declare<EmergencyContactRelationship, EmployeeContext, PersonContext>("Emergency Contact").Min(1).Max(2);
            Declare<Beneficiary, EmployeeContext, PersonContext>("Beneficiary").Or<BusinessContext>();
            Declare<SupervisorRelationship, EmployeeContext, PersonContext>("Supervisor").Min(1).Max(3);

            AddAbstraction<Spouse, PersonContext>("Spouse Name");
            AddAbstraction<Child, PersonContext>("Child Name");
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
    /*
    public class ParentChildRelationship : IRelationship { }
    public class ChildParentRelationship : IRelationship { }

    public class ChildContext : Context
    {
        public ChildContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
            Declare<ChildParentRelationship, ChildContext, FatherContext>().OneAndOnlyOne();
            Declare<ChildParentRelationship, ChildContext, MotherContext>().OneAndOnlyOne();
        }
    }

    public class PersonFatherRelationship : IRelationship { }
    public class PersonMotherRelationship : IRelationship { }
    public class FatherPersonRelationship : IRelationship { }
    public class MotherPersonRelationship : IRelationship { }

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

    public class ParentContext : Context
    {
        public ParentContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
            Declare<ParentChildRelationship, ParentContext, ChildContext>().Max(5).AsGrid();
            //Declare<InsuredSpouse, ParentContext, Spouse>("Spouse").ZeroOrOne();
            //AddAbstraction<Spouse, PersonContext>("Spouse Name");
        }
    }
    */
    /*
    public class ParentChildContext : Context
    {
        public ParentChildContext()
        {
            Declare<ParentContext>().OneAndOnlyOne();
            AddAbstraction<ParentContext, PersonContext>("Parent").Coalesce();
            Declare<ParentChildRelationship, ParentContext, ChildContext>("Child").ZeroOrMore().AsGrid();
        }
    }
    */

    //public class ChildFatherRelationship : IRelationship { }
    //public class ChildMotherRelationship : IRelationship { }
    public class ChildParentRelationship : IRelationship { }

    public class PersonContext : Context
    {
        public PersonContext()
        {
            Declare<PersonNameContext>().OneAndOnlyOne();
        }
    }

    public class ChildContext : Context
    {
        public ChildContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
            Declare<ChildParentRelationship, ChildContext, FatherContext>().OneAndOnlyOne();
            Declare<ChildParentRelationship, ChildContext, MotherContext>().OneAndOnlyOne();
        }
    }

    public class FatherContext : Context
    {
        public FatherContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
        }
    }

    public class MotherContext : Context
    {
        public MotherContext()
        {
            Declare<PersonContext>().OneAndOnlyOne();
        }
    }
}
