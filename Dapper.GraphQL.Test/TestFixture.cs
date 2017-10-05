using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.GraphQL;
using Dapper.GraphQL.Test.Models;
using Dapper.GraphQL.Test.QueryBuilders;
using DbUp;
using DbUp.SQLite.Helpers;
using GraphQL;
using GraphQL.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.GraphQL.Test
{
    public class TestFixture : IDisposable
    {
        private readonly DocumentExecuter documentExecuter;

        public SharedConnection DbConnection { get; set; }
        public PersonSchema Schema { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        public TestFixture()
        {
            this.documentExecuter = new DocumentExecuter();
            var serviceCollection = new ServiceCollection();

            SetupDapperGraphQL(serviceCollection);
            SetupInMemorySqliteDatabase(serviceCollection);

            this.ServiceProvider = serviceCollection.BuildServiceProvider();
            this.Schema = ServiceProvider.GetRequiredService<PersonSchema>();
        }

        public void Dispose()
        {
            if (DbConnection != null)
            {
                DbConnection.Close();
                DbConnection.Dispose();
                DbConnection = null;
            }
        }

        public bool JsonEquals(string expectedJson, string actualJson)
        {
            // To ensure formatting doesn't affect our results, we first convert to JSON tokens
            // and only compare the structure of the resulting objects.
            return JToken.DeepEquals(JObject.Parse(expectedJson), JObject.Parse(actualJson)));
        }

        public async Task<string> QueryGraphQLAsync(string query)
        {
            var result = await documentExecuter
                .ExecuteAsync(options =>
                {
                    options.Schema = Schema;
                    options.Query = query;
                })
                .ConfigureAwait(false);

            var json = new DocumentWriter(indent: true).Write(result);
            return json;
        }

        private void SetupDapperGraphQL(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDapperGraphQL(options =>
            {
                // Add GraphQL types
                options.AddType<EmailType>();
                options.AddType<PersonType>();
                options.AddType<GraphQL.PhoneType>();
                options.AddType<PersonQuery>();

                // Add the GraphQL schema
                options.AddSchema<PersonSchema>();

                // Add query builders for dapper
                options.AddQueryBuilder<Email, EmailQueryBuilder>();
                options.AddQueryBuilder<Person, PersonQueryBuilder>();
                options.AddQueryBuilder<Phone, PhoneQueryBuilder>();

                // Add entity mappers
                options.AddEntityMapper<Person, PersonEntityMapper>();
            });
        }

        private void SetupInMemorySqliteDatabase(IServiceCollection serviceCollection)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            DbConnection = new SharedConnection(new SqliteConnection(connectionStringBuilder.ToString()));

            var upgrader = DeployChanges.To
                .SQLiteDatabase(DbConnection)
                .WithScriptsEmbeddedInAssembly(typeof(Person).GetTypeInfo().Assembly)
                .LogToConsole()
                .Build();

            var upgradeResult = upgrader.PerformUpgrade();

            serviceCollection.AddSingleton<IDbConnection>(DbConnection);
        }
    }
}