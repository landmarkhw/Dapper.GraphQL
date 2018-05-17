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
                var personMapper = fixture
                    .ServiceProvider
                    .GetRequiredService<IEntityMapperFactory>()
                    .Build<Person>(person => person.Id);

                using (var db = fixture.GetDbConnection())
                {
                    query.Execute<Person>(db, personMapper, null);
                }
            });
        }
        
        [Fact(DisplayName = "SELECT query uses custom table name")]
        public void SelectWithCustomTableName()
        {
            // Check generic Select uses custom table name for Contact as configured in TestFixture
            var contact = new Contact();

            var query = SqlBuilder.From<Contact>();
            Assert.Equal("SELECT\r\n\r\nFROM Contacts\r\n", query.ToString());

            var queryWithAlias = SqlBuilder.From<Contact>("contacts");
            Assert.Equal("SELECT\r\n\r\nFROM Contacts contacts\r\n", queryWithAlias.ToString());
        }
    }
}