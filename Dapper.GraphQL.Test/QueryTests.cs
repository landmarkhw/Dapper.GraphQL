using Dapper.GraphQL.Test.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class QueryTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture fixture;
        private readonly Func<IEnumerable<object>, Person> personMapper;

        public QueryTests(TestFixture fixture)
        {
            this.fixture = fixture;

            // Build a mapper that compares primary keys when building 'Person' objects
            this.personMapper = fixture
                .ServiceProvider
                .GetRequiredService<IEntityMapperFactory>()
                .Build<Person>(person => person.Id);
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

                using (var dbConnection = fixture.DbConnection)
                {
                    query.Execute<Person>(dbConnection, personMapper);
                }
            });
        }
    }
}