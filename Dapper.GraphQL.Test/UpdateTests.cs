using Dapper.GraphQL.Test.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class UpdateTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture fixture;

        public UpdateTests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "UPDATE person succeeds")]
        public void UpdatePerson()
        {
            Person person = new Person
            {
                FirstName = "Douglas"
            };
            Person previousPerson = null;

            // Get an entity mapper factory
            var entityMapperFactory = fixture
                .ServiceProvider
                .GetRequiredService<IEntityMapperFactory>();

            try
            {
                // Update the person with Id = 2 with a new FirstName
                using (var db = fixture.GetDbConnection())
                {
                    previousPerson = SqlBuilder
                        .From<Person>()
                        .Select("Id", "FirstName")
                        .Where("FirstName = @firstName", new { firstName = "Doug" })
                        .Execute(db, entityMapperFactory.Build<Person>(p => p.Id), null)
                        .FirstOrDefault();

                    SqlBuilder
                        .Update(person)
                        .Where("Id = @id", new { id = previousPerson.Id })
                        .Execute(db);

                    // Get the same person back
                    person = SqlBuilder
                        .From<Person>()
                        .Select("Id", "FirstName")
                        .Where("Id = @id", new { id = previousPerson.Id })
                        .Execute(db, entityMapperFactory.Build<Person>(p => p.Id), null)
                        .FirstOrDefault();
                }

                // Ensure we got a person and their name was indeed changed
                Assert.NotNull(person);
                Assert.Equal("Douglas", person.FirstName);
            }
            finally
            {
                if (previousPerson != null)
                {
                    using (var db = fixture.GetDbConnection())
                    {
                        person = new Person
                        {
                            FirstName = previousPerson.FirstName
                        };

                        // Put the entity back to the way it was
                        SqlBuilder
                            .Update<Person>(person)
                            .Where("Id = @id", new { id = 2 })
                            .Execute(db);
                    }
                }
            }
        }

        [Fact(DisplayName = "UPDATE person asynchronously succeeds")]
        public async Task UpdatePersonAsync()
        {
            Person person = new Person
            {
                FirstName = "Douglas"
            };
            Person previousPerson = null;

            // Get an entity mapper factory
            var entityMapperFactory = fixture
                .ServiceProvider
                .GetRequiredService<IEntityMapperFactory>();

            try
            {
                // Update the person with Id = 2 with a new FirstName
                using (var db = fixture.GetDbConnection())
                {
                    db.Open();

                    var previousPeople = await SqlBuilder
                        .From<Person>()
                        .Select("Id", "FirstName")
                        .Where("FirstName = @firstName", new { firstName = "Doug" })
                        .ExecuteAsync(db, entityMapperFactory.Build<Person>(p => p.Id), null);

                    previousPerson = previousPeople.FirstOrDefault();

                    await SqlBuilder
                        .Update(person)
                        .Where("Id = @id", new { id = previousPerson.Id })
                        .ExecuteAsync(db);

                    // Get the same person back
                    var people = await SqlBuilder
                        .From<Person>()
                        .Select("Id", "FirstName")
                        .Where("Id = @id", new { id = previousPerson.Id })
                        .ExecuteAsync(db, entityMapperFactory.Build<Person>(p => p.Id), null);
                    person = people
                        .FirstOrDefault();
                }

                // Ensure we got a person and their name was indeed changed
                Assert.NotNull(person);
                Assert.Equal("Douglas", person.FirstName);
            }
            finally
            {
                if (previousPerson != null)
                {
                    using (var db = fixture.GetDbConnection())
                    {
                        db.Open();

                        person = new Person
                        {
                            FirstName = previousPerson.FirstName
                        };

                        // Put the entity back to the way it was
                        await SqlBuilder
                            .Update<Person>(person)
                            .Where("Id = @id", new { id = 2 })
                            .ExecuteAsync(db);
                    }
                }
            }
        }

        [Fact(DisplayName = "UPDATE query uses custom table name")]
        public void UpdateWithCustomTableName()
        {
            // Check generic Update uses custom table name for Contact as configured in TestFixture
            var contact = new Contact();

            var query = SqlBuilder.Update<Contact>(contact);
            Assert.Equal("UPDATE Contacts SET \r\n", query.ToString());
        }
    }
}
