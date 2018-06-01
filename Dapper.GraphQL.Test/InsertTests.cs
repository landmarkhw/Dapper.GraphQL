using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class InsertTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture fixture;

        public InsertTests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "INSERT person succeeds")]
        public void InsertPerson()
        {
            var person = new Person
            {
                FirstName = "Steven",
                LastName = "Rollman",
            };

            // Ensure inserting a person works and we get the person's Id back
            int personId;
            using (var db = fixture.GetDbConnection())
            {
                personId = SqlBuilder
                    .Insert(person)
                    .ExecuteWithPostgreSqlIdentity(db, p => p.Id);

                Assert.True(personId > 0);

                var email = new Email
                {
                    Address = "srollman@landmarkhw.com",
                };

                var phone = new Phone
                {
                    Number = "8011115555",
                    Type = PhoneType.Mobile,
                };

                // Add email and phone number to the person
                int insertedCount;
                insertedCount = SqlBuilder
                    .Insert(email)
                    .Insert(phone)
                    .Execute(db);

                // Ensure both were inserted properly
                Assert.Equal(2, insertedCount);

                // Build an entity mapper for person
                var personMapper = new PersonEntityMapper();

                // Query the person from the database
                var query = SqlBuilder
                    .From<Person>("person")
                    .LeftJoin("Email email on person.Id = email.PersonId")
                    .LeftJoin("Phone phone on person.Id = phone.PersonId")
                    .Select("person.*, email.*, phone.*")
                    .SplitOn<Person>("Id")
                    .SplitOn<Email>("Id")
                    .SplitOn<Phone>("Id")
                    .Where("person.Id = @id", new { id = personId });

                var graphql = @"
{
    person {
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
        }
    }
}";

                var selection = fixture.BuildGraphQLSelection(graphql);

                person = query
                    .Execute(db, selection, personMapper)
                    .FirstOrDefault();
            }

            // Ensure all inserted data is present
            Assert.NotNull(person);
            Assert.Equal(personId, person.Id);
            Assert.Equal("Steven", person.FirstName);
            Assert.Equal("Rollman", person.LastName);
            Assert.Equal(1, person.Emails.Count);
            Assert.Equal("srollman@landmarkhw.com", person.Emails[0].Address);
            Assert.Equal(1, person.Phones.Count);
            Assert.Equal("8011115555", person.Phones[0].Number);
        }

        [Fact(DisplayName = "INSERT person asynchronously succeeds")]
        public async Task InsertPersonAsync()
        {
            var person = new Person
            {
                FirstName = "Steven",
                LastName = "Rollman",
            };

            // Ensure inserting a person works and we get the person's Id back
            int personId;
            using (var db = fixture.GetDbConnection())
            {
                db.Open();

                personId = await SqlBuilder
                    .Insert(person)
                    .ExecuteWithPostgreSqlIdentityAsync(db, p => p.Id);

                Assert.True(personId > 0);

                var email = new Email
                {
                    Address = "srollman@landmarkhw.com",
                };

                var phone = new Phone
                {
                    Number = "8011115555",
                    Type = PhoneType.Mobile,
                };

                // Add email and phone number to the person
                int insertedCount;
                insertedCount = await SqlBuilder
                    .Insert(email)
                    .Insert(phone)
                    .ExecuteAsync(db);

                // Ensure both were inserted properly
                Assert.Equal(2, insertedCount);

                // Build an entity mapper for person
                var personMapper = new PersonEntityMapper();

                // Query the person from the database
                var query = SqlBuilder
                    .From<Person>("person")
                    .LeftJoin("Email email on person.Id = email.PersonId")
                    .LeftJoin("Phone phone on person.Id = phone.PersonId")
                    .Select("person.*, email.*, phone.*")
                    .SplitOn<Person>("Id")
                    .SplitOn<Email>("Id")
                    .SplitOn<Phone>("Id")
                    .Where("person.Id = @id", new { id = personId });

                var graphql = @"
{
    person {
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
        }
    }
}";
                var selection = fixture.BuildGraphQLSelection(graphql);

                var people = await query.ExecuteAsync(db, selection, personMapper);
                person = people
                    .FirstOrDefault();
            }

            // Ensure all inserted data is present
            Assert.NotNull(person);
            Assert.Equal(personId, person.Id);
            Assert.Equal("Steven", person.FirstName);
            Assert.Equal("Rollman", person.LastName);
            Assert.Equal(1, person.Emails.Count);
            Assert.Equal("srollman@landmarkhw.com", person.Emails[0].Address);
            Assert.Equal(1, person.Phones.Count);
            Assert.Equal("8011115555", person.Phones[0].Number);
        }
    }
}