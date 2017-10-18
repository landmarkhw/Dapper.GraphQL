using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using Dapper.GraphQL.Test.QueryBuilders;
using DbUp;
using DbUp.SQLite.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
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

        [Fact(DisplayName = "INSERT person succeeds", Skip = "Not quite setup to run properly")]
        public void InsertPerson()
        {
            var person = new Person
            {
                FirstName = "Steven",
                LastName = "Rollman",
            };

            var personId = SqlBuilder
                .Insert(person)
                .ExecuteWithSqliteIdentity(fixture.DbConnection);

            var email = new Email
            {
                Address = "srollman@landmarkhw.com",
                PersonId = personId,
            };

            var phone = new Phone
            {
                Number = "8011115555",
                Type = PhoneType.Mobile,
                PersonId = personId,
            };

            var insertedCount = SqlBuilder
                .Insert(email)
                .Insert(phone)
                .Execute(fixture.DbConnection);

            Assert.Equal(2, insertedCount);

            var personMapper = fixture.ServiceProvider.GetRequiredService<IEntityMapperFactory>().Build<Person>(p => p.Id);
            var query = SqlBuilder
                .From<Person>()
                .Select("Id", "FirstName", "LastName")
                .Where("Id = @id", new { id = personId })
                .Execute(fixture.DbConnection, personMapper)
                .FirstOrDefault();

            Assert.NotNull(person);
            Assert.Equal(personId, person.Id);
            Assert.Equal("Steven", person.FirstName);
            Assert.Equal("Rollman", person.LastName);
        }
    }
}