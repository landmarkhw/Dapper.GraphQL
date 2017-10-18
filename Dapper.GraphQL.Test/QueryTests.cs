using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using Dapper.GraphQL.Test.QueryBuilders;
using DbUp;
using DbUp.SQLite.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class QueryTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture fixture;

        public QueryTests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "SELECT without matching alias should throw")]
        public void SelectWithoutMatchingAliasShouldThrow()
        {
            Assert.ThrowsAsync<SqliteException>(async () =>
            {
                var query = SqlBuilder
                    .From("Person person")
                    .Select("person.Id", "notAnAlias.Id")
                    .SplitOn<Person>("Id");

                // Get a mapper that compares primary keys
                var personMapper = fixture
                    .ServiceProvider
                    .GetRequiredService<IEntityMapperFactory>()
                    .Build<Person>(person => person.Id);

                using (var dbConnection = fixture.DbConnection)
                {
                    query.Execute<Person>(dbConnection, personMapper);
                }
            });
        }
    }
}