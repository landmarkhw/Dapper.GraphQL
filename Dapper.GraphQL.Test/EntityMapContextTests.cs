using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class EntityMapContextTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public EntityMapContextTests(TestFixture fixture)
        {
            this._fixture = fixture;
        }

        [Fact(DisplayName = "EntityMap properly deduplicates")]
        public void EntityMapSucceeds()
        {
            var person1 = new Person
            {
                FirstName = "Doug",
                Id = 2,
                LastName = "Day",
                MergedToPersonId = 2,
            };
            var person2 = new Person
            {
                FirstName = "Douglas",
                Id = 2,
                LastName = "Day",
                MergedToPersonId = 2,
            };

            var email1 = new Email
            {
                Address = "dday@landmarkhw.com",
                Id = 2,
            };

            var email2 = new Email
            {
                Address = "dougrday@gmail.com",
                Id = 3,
            };

            var phone = new Phone
            {
                Id = 1,
                Number = "8011234567",
                Type = PhoneType.Mobile,
            };

            var splitOn = new[]
            {
                typeof(Person),
                typeof(Email),
                typeof(Phone),
            };

            var personEntityMapper = new PersonEntityMapper();

            var graphql = @"
{
    query {
        firstName
        lastName
        id
        emails {
            id
            address
        }
        phones {
            id
            number
            type
        }
    }
}";

            var selectionSet = _fixture.BuildGraphQlSelection(graphql);
            var context1 = new EntityMapContext
            {
                Items = new object[]
                {
                    person1,
                    email1,
                    phone,
                },
                SelectionSet = selectionSet,
                SplitOn = splitOn,
            };

            person1 = personEntityMapper.Map(context1);
            Assert.Equal(3, context1.MappedCount);

            Assert.Equal(2, person1.Id);
            Assert.Equal("Doug", person1.FirstName);
            Assert.Single(person1.Emails);
            Assert.Single(person1.Phones);

            var context2 = new EntityMapContext
            {
                Items = new object[]
                {
                    person2,
                    email2,
                    null,
                },
                SelectionSet = selectionSet,
                SplitOn = splitOn,
            };

            person2 = personEntityMapper.Map(context2);
            Assert.Equal(3, context2.MappedCount);

            // The same reference should have been returned
            Assert.Same(person1, person2);

            // A 2nd email was added to person
            Assert.Equal(2, person1.Emails.Count);
        }
    }
}
