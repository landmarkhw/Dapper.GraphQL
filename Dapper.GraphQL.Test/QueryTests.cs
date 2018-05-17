using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;
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

        [Fact(DisplayName = "ORDER BY should work")]
        public void OrderByShouldWork()
        {
            var query = SqlBuilder
                .From("Person person")
                .Select("person.Id")
                .SplitOn<Person>("Id")
                .OrderBy("LastName");

            Assert.Contains("ORDER BY", query.ToString());
        }

        [Fact(DisplayName = "SELECT without matching alias should throw")]
        public void SelectWithoutMatchingAliasShouldThrow()
        {
            Assert.Throws<SqlException>(() =>
            {
                var query = SqlBuilder
                    .From("Person person")
                    .Select("person.Id", "notAnAlias.Id")
                    .SplitOn<Person>("Id");

                // Build a mapper that compares primary keys when building 'Person' objects
                var personMapper = new PersonEntityMapper();
                using (var db = fixture.GetDbConnection())
                {
                    query.Execute<Person>(db, personMapper, null);
                }
            });
        }
    }
}