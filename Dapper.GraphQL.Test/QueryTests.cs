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

        [Fact(DisplayName = "FROM without SELECT should throw")]
        public void FromWithoutSelectShouldThrow()
        {
            Assert.Throws<InvalidOperationException>(() => new SqlBuilder()
                .From("Person person")
                .ToString()
            );
        }

        [Fact(DisplayName = "SELECT without FROM should throw")]
        public void SelectWithoutFromShouldThrow()
        {
            Assert.Throws<InvalidOperationException>(() => new SqlBuilder()
                .From("Person person")
                .ToString()
            );
        }

        [Fact(DisplayName = "SELECT without matching alias should throw")]
        public void SelectWithoutMatchingAliasShouldThrow()
        {
            Assert.Throws<SqliteException>(() =>
            {
                var query = new SqlBuilder()
                    .From("Person person")
                    .Select("person.Id", "notAnAlias.Id")
                    .SplitOn<Person>("Id");

                using (var dbConnection = fixture.DbConnection)
                {
                    var customer = query
                        .Execute(dbConnection, c => c)
                        .FirstOrDefault();
                }
            });
        }
    }
}