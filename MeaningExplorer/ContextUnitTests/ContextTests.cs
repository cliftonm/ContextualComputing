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
    class TestRelationship : IRelationship { }
    class Entity1 : IEntity { };
    class Entity2 : IEntity { };
    class Entity3 : IEntity { };

    class Spouse : IEntity { }
    class Child : IEntity { }
    class Person : IEntity { }
    class Supervisor : IEntity { }
    class NonProfitBusiness : IEntity { }

    class Employee : IEntity { }
    class EmergencyContact : IRelationship { }
    class InsuredSpouse : IRelationship { }
    class InsuredChild : IRelationship { }
    class Beneficiary : IRelationship { }
    class SupervisorRelationship : IRelationship { }

    [TestClass]
    public class ContextTests
    {
        [TestMethod]
        public void GetContextEntityByTypeTest()
        {
            IEntity entity;
            Context context = new Context();
            context.Add<TestRelationship>(entity = new Entity1(), new Entity2());
            context.Add<TestRelationship>(new Entity2(), new Entity3());
            var entities = context.Get<Entity1>();
            Assert.IsTrue(entities.Count() == 1, "Expected 1 matching entity.");
            Assert.IsTrue(entities.Single().ConcreteEntity == entity, "Unexpected concrete entity.");
        }

        [TestMethod]
        public void GetContextEntityByReferenceTest()
        {
            IEntity entity;
            Context context = new Context();
            context.Add<TestRelationship>(entity = new Entity1(), new Entity2());
            context.Add<TestRelationship>(new Entity2(), new Entity3());
            var entities = context.Get(entity);
            Assert.IsTrue(entities.Count() == 1, "Expected 1 matching entity.");
            Assert.IsTrue(entities.Single().ConcreteEntity == entity, "Unexpected concrete entity.");
        }

        [TestMethod]
        public void RelationshipTypeTest()
        {
            IEntity entity1;
            IEntity entity2;
            Context context = new Context();
            context.Add<TestRelationship>(new Entity2(), new Entity3());
            context.Add<TestRelationship>(entity1 = new Entity1(), entity2 = new Entity2());
            var entities = context.Get<Entity1>();
            Assert.IsTrue(entities.Count() == 1, "Expected 1 matching entity.");
            var contextEntity = entities.Single();
            Assert.IsTrue(contextEntity.ConcreteEntity == entity1, "Entity instance does not match.");
            Assert.IsTrue(contextEntity.RelatedTo == entity2, "Entity instance does not match.");
            Assert.IsTrue(contextEntity.RelationshipType == typeof(TestRelationship), "Unexpected relationship type.");
        }

        [TestMethod]
        public void DeclaredRelationshipTypeTest()
        {
            Context context = new Context();
            context.Declare<TestRelationship, Entity1, Entity2>();
            context.Add<TestRelationship>(new Entity1(), new Entity2());
        }

        [TestMethod, ExpectedException(typeof(RelationshipDeclarationException))]
        public void DeclaredRelationshipTypeFaileTest()
        {
            Context context = new Context();
            context.Declare<TestRelationship, Entity1, Entity2>();
            context.Add<TestRelationship>(new Entity1(), new Entity3());
        }

        [TestMethod]
        public void AbstractionTest()
        {
            Context context = new Context();
            context.AddAbstraction<Spouse, Person>();
            context.AddAbstraction<Child, Person>();
            var abstractions = context.GetAbstractions<Spouse>();
            Assert.IsTrue(abstractions.Count() == 1, "Expected 1 abstraction.");
            Assert.IsTrue(abstractions.Single().SubType == typeof(Spouse), "Mismatch in subtype.");
            Assert.IsTrue(abstractions.Single().SuperType == typeof(Person), "Mismatch in supertype.");
        }

        [TestMethod]
        public void ImplementationTest()
        {
            Context context = new Context();
            context.AddAbstraction<Spouse, Person>();
            context.AddAbstraction<Child, Person>();
            var implementors = context.GetImplementations<Person>();
            Assert.IsTrue(implementors.Count() == 2, "Expected 2 implementors.");
            Assert.IsTrue(implementors.First().SubType == typeof(Spouse), "Mismatch in subtype.");
            Assert.IsTrue(implementors.First().SuperType == typeof(Person), "Mismatch in supertype.");
        }

        [TestMethod]
        public void DeclarationOneAndOnlyOneTest()
        {
            Context context = new Context();
            var decl = context.Declare<Employee>().OneAndOnlyOne();
            List<ContextEntity> entities = new List<ContextEntity>();
            Assert.IsFalse(decl.Validate(entities), "Expected no-valid result.");
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsFalse(decl.Validate(entities), "Expected no-valid result.");
        }

        [TestMethod]
        public void DeclarationZeroOrOneTest()
        {
            Context context = new Context();
            var decl = context.Declare<Employee>().ZeroOrOne();
            List<ContextEntity> entities = new List<ContextEntity>();
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsFalse(decl.Validate(entities), "Expected no-valid result.");
        }

        [TestMethod]
        public void DeclarationZeroOrMoreTest()
        {
            Context context = new Context();
            var decl = context.Declare<Employee>().ZeroOrOne();
            List<ContextEntity> entities = new List<ContextEntity>();
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsFalse(decl.Validate(entities), "Expected valid result.");
        }


        [TestMethod]
        public void DeclarationOneOrMoreTest()
        {
            Context context = new Context();
            var decl = context.Declare<Employee>().OneOrMore();
            List<ContextEntity> entities = new List<ContextEntity>();
            Assert.IsFalse(decl.Validate(entities), "Expected non-valid result.");
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");
        }

        [TestMethod]
        public void DeclarationExactlyTest()
        {
            Context context = new Context();
            var decl = context.Declare<Employee>().Exactly(2);
            List<ContextEntity> entities = new List<ContextEntity>();

            // 0
            Assert.IsFalse(decl.Validate(entities), "Expected non-valid result.");

            // 1
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsFalse(decl.Validate(entities), "Expected non-valid result.");

            // 2
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");

            // 3 
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsFalse(decl.Validate(entities), "Expected non-valid result.");
        }

        [TestMethod]
        public void DeclarationRangeTest()
        {
            Context context = new Context();
            var decl = context.Declare<Employee>().Min(2).Max(4);
            List<ContextEntity> entities = new List<ContextEntity>();

            // 0
            Assert.IsFalse(decl.Validate(entities), "Expected non-valid result.");

            // 1
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsFalse(decl.Validate(entities), "Expected non-valid result.");

            // 2
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");

            // 3 
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");

            // 4
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsTrue(decl.Validate(entities), "Expected valid result.");

            // 5 
            entities.Add(ContextEntity.Create(new Employee()));
            Assert.IsFalse(decl.Validate(entities), "Expected no-valid result.");
        }

        [TestMethod, ExpectedException(typeof(DeclarationViolationException))]
        public void UniqueEntityDeclarationFailTest()
        {
            Context context = new Context();
            context.Declare<Employee>();
            context.Declare<Employee>();
        }

        [TestMethod]
        public void UniqueEntityDeclarationSuccessTest()
        {
            Context context = new Context();
            context.Declare<Employee>();
            context.Declare<Spouse>();
        }

        [TestMethod]
        public void UniqueEntityRelationshipDeclarationSuccessTest()
        {
            Context context = new Context();
            context.Declare<EmergencyContact, Employee, Person>();
            context.Declare<InsuredSpouse, Employee, Person>();
        }

        [TestMethod, ExpectedException(typeof(RelationshipDeclarationException))]
        public void UniqueEntityRelationshipDeclarationFailTest()
        {
            Context context = new Context();
            context.Declare<EmergencyContact, Employee, Person>();
            context.Declare<EmergencyContact, Employee, Person>();
        }

        [TestMethod]
        public void UndoEntityAddTest()
        {
            Context context = new Context();
            var decl = context.Declare<Employee>().Exactly(2);

            // Should add Employee even though min requirement not met.
            context.Add(new Employee());
            Assert.IsFalse(context.IsValid, "Context should not yet be valid.");
            Assert.IsTrue(context.Entities.Count == 1, "Expected entity to be added.");

            context.Add(new Employee());
            Assert.IsTrue(context.IsValid, "Context should be valid.");
            Assert.IsTrue(context.Entities.Count == 2, "Expected entity to be added.");
            bool exceptionThrown = false;

            try
            {
                context.Add(new Employee());
            }
            catch (DeclarationViolationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Expected exception adding additional entity.");
            Assert.IsTrue(context.IsValid, "Context should be valid.");
            Assert.IsTrue(context.Entities.Count == 2, "Expected rollback.");
        }

        [TestMethod, ExpectedException(typeof(RelationshipDeclarationException))]
        public void MustBeInRelationshipTest()
        {
            Context context = new Context();

            // Must be in a relationship with someone!
            context.RelationshipRequired<Employee>();

            var emp = new Employee();
            context.Add(emp);
        }

        [TestMethod]
        public void OptionalRelationshipTest()
        {
            Context context = new Context();

            // Relationship is optional...
            context.Declare<SupervisorRelationship, Employee, Supervisor>().Min(0).Max(2);
            // ... but a single Employee is required.
            context.Declare<Employee>().OneAndOnlyOne();

            Assert.IsFalse(context.IsValid, "Context should not be valid.");

            context.Add(new Employee());

            Assert.IsTrue(context.IsValid, "Context should be valid.");

            bool exceptionThrown = false;

            try
            {
                context.Add(new Employee());
            }
            catch(DeclarationViolationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Expected exception adding additional entity.");
        }

        [TestMethod]
        public void OrRelationshipTest()
        {
            {
                Context context = new Context();
                context.Declare<Beneficiary, Employee, Person>().Or<NonProfitBusiness>();
                context.Add<Beneficiary>(new Employee(), new Person());
                Assert.IsTrue(context.IsValid, "Context expected to be valid.");
            }

            {
                Context context = new Context();
                context.Declare<Beneficiary, Employee, Person>().Or<NonProfitBusiness>();
                context.Add<Beneficiary>(new Employee(), new NonProfitBusiness());
                Assert.IsTrue(context.IsValid, "Context expected to be valid.");
            }
        }

        [TestMethod, ExpectedException(typeof(RelationshipDeclarationException))]
        public void OnlyOneOrRelationshipTest()
        {
            Context context = new Context();
            context.Declare<Beneficiary, Employee, Person>().Or<NonProfitBusiness>().OneAndOnlyOne();
            var emp = new Employee();
            context.Add<Beneficiary>(emp, new Person());
            context.Add<Beneficiary>(emp, new NonProfitBusiness());
        }

        /// <summary>
        /// "And" relationship types are only tested in the context's IsValid call.
        /// </summary>
        [TestMethod]
        public void AndRelationshipTest()
        {
            Context context = new Context();
            context.Declare<Beneficiary, Employee, Person>().And<NonProfitBusiness>();
            var emp = new Employee();
            context.Add<Beneficiary>(emp, new Person());
            context.Add<Beneficiary>(emp, new NonProfitBusiness());
            Assert.IsTrue(context.IsValid, "Context expected to be valid.");
        }

        [TestMethod]
        public void MissingAndRelationshipTest()
        {
            {
                // Test first requirement:
                Context context = new Context();
                context.Declare<Beneficiary, Employee, Person>().And<NonProfitBusiness>();
                context.Add<Beneficiary>(new Employee(), new Person());
                Assert.IsFalse(context.IsValid, "Context expected to not be valid.");
            }

            {
                // Test second requirement:
                Context context = new Context();
                context.Declare<Beneficiary, Employee, Person>().And<NonProfitBusiness>();
                context.Add<Beneficiary>(new Employee(), new NonProfitBusiness());
                Assert.IsFalse(context.IsValid, "Context expected to not be valid.");
            }
        }

        [TestMethod, ExpectedException(typeof(RelationshipDeclarationException))]
        public void WrongRelationshipTargetTest()
        {
            Context context = new Context();
            context.Declare<SupervisorRelationship, Employee, Supervisor>().Min(1).Max(2);
            context.Add<SupervisorRelationship>(new Child(), new Supervisor());
        }

        [TestMethod, ExpectedException(typeof(RelationshipDeclarationException))]
        public void WrongRelationshipSourceTest()
        {
            Context context = new Context();
            context.Declare<SupervisorRelationship, Employee, Supervisor>().Min(1).Max(2);
            context.Add<SupervisorRelationship>(new Employee(), new Child());
        }

        [TestMethod, ExpectedException(typeof(RelationshipDeclarationException))]
        public void WrongRelationshipTest()
        {
            Context context = new Context();
            context.Declare<SupervisorRelationship, Employee, Supervisor>().Min(1).Max(2);
            context.Add<Beneficiary>(new Employee(), new Supervisor());
        }

        [TestMethod]
        public void UndoRelationshipConstraintTest()
        {
            bool exceptionThrown = false;

            Context context = new Context();
            context.Declare<SupervisorRelationship, Employee, Supervisor>().Min(1).Max(2);

            var emp = new Employee();

            context.Add<SupervisorRelationship>(emp, new Supervisor());
            Assert.IsTrue(context.IsValid, "Context should be valid.");

            context.Add<SupervisorRelationship>(emp, new Supervisor());
            Assert.IsTrue(context.IsValid, "Context should be valid.");

            try
            {
                context.Add<SupervisorRelationship>(emp, new Supervisor());
            }
            catch (RelationshipDeclarationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Exception expected adding additional supervisor");
            Assert.IsTrue(context.IsValid, "Expected rollback.");
        }
    }
}
