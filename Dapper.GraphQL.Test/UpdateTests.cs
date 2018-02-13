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
            var person = new Person
            {
                FirstName = "Douglas"
            };

            // Update the person with Id = 1 with a new FirstName
            SqlBuilder
                .Update(person)
                .Where("Id = @id", new { id = 1 })
                .Execute(fixture.DbConnection);

            // Build a person mapper for dapper
            var personMapper = fixture
                .ServiceProvider
                .GetRequiredService<IEntityMapperFactory>()
                .Build<Person>(p => p.Id);

            // Get the same person back
            person = SqlBuilder
                .From<Person>()
                .Select("Id", "FirstName")
                .Where("Id = @id", new { id = 1 })
                .Execute(fixture.DbConnection, personMapper)
                .FirstOrDefault();

            // Ensure we got a person and their name was indeed changed
            Assert.NotNull(person);
            Assert.Equal("Douglas", person.FirstName);
        }
    }
}