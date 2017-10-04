using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using DbUp;
using DbUp.SQLite.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class QueryTests
    {
        private SharedConnection dbConnection;

        public QueryTests()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            dbConnection = new SharedConnection(new SqliteConnection(connectionStringBuilder.ToString()));

            var upgrader = DeployChanges.To
                .SQLiteDatabase(dbConnection)
                .WithScriptsEmbeddedInAssembly(typeof(Person).GetTypeInfo().Assembly)
                .LogToConsole()
                .Build();

            var upgradeResult = upgrader.PerformUpgrade();
        }

        [Fact(DisplayName = "FROM without SELECT should throw")]
        public void FromWithoutSelectShouldThrow()
        {
            Assert.Throws<InvalidOperationException>(() => new SqlQueryBuilder()
                .From("Customer customer")
                .ToString()
            );
        }

        [Fact(DisplayName = "Single FROM should succeed")]
        public void SingleFromQuery()
        {
            var customers = new SqlQueryBuilder()
                .From("Person person")
                .Select("Id", "FirstName", "LastName")
                .SplitOn<Person>("Id")
                .Execute(dbConnection, objs => objs.OfType<Person>())
                .ToList();

            Assert.Equal(2, customers.Count);
        }

        [Fact(DisplayName = "Single JOIN should succeed")]
        public void SingleJoinQuery()
        {
            var services = new ServiceCollection();
            services.AddDapperGraphQL(options =>
            {
                options.AddEntityMapper<Person, PersonEntityMapper>();
            });

            var serviceProvider = services.BuildServiceProvider();

            var customers = new SqlQueryBuilder()
                .From("Person person")
                .Select("person.Id", "person.FirstName", "person.LastName")
                .SplitOn<Person>("Id")
                .LeftOuterJoin("Email email ON person.Id = email.PersonId")
                .Select("email.Id", "email.Address")
                .SplitOn<Email>("Id")
                .WithContainer(serviceProvider)
                .Execute(dbConnection, (Person person) => person.Id)
                .ToList();

            Assert.Equal(2, customers.Count);
        }
    }
}