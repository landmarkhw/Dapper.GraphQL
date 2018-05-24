using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class EntityMapContextTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture fixture;

        public EntityMapContextTests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "EntityMap properly deduplicates")]
        public void EntityMapSucceeds()
        {
            var person1 = new Person
            {
                FirstName = "Doug",
                Id = 2,
                LastName = "Day",
            };
            var person2 = new Person
            {
                FirstName = "Douglas",
                Id = 2,
                LastName = "Day",
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

            var deduplicatingPersonMapper = new DeduplicatingEntityMapper<Person>
            {
                Mapper = new PersonEntityMapper(),
                PrimaryKey = p => p.Id,
            };

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

            var selectionSet = fixture.BuildGraphQLSelection(graphql);
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

            person1 = deduplicatingPersonMapper.Map(context1);
            Assert.Equal(3, context1.MappedCount);

            Assert.Equal(2, person1.Id);
            Assert.Equal("Doug", person1.FirstName);
            Assert.Equal(1, person1.Emails.Count);
            Assert.Equal(1, person1.Phones.Count);

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

            person2 = deduplicatingPersonMapper.Map(context2);
            Assert.Equal(3, context2.MappedCount);

            // Duplicate is detected
            Assert.Null(person2);

            // 2nd email added to person
            Assert.Equal(2, person1.Id);
            Assert.Equal("Doug", person1.FirstName);
            Assert.Equal(2, person1.Emails.Count);
        }
    }
}