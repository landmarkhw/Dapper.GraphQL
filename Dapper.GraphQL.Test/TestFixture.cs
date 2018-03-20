using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.GraphQL;
using Dapper.GraphQL.Test.Models;
using Dapper.GraphQL.Test.QueryBuilders;
using DbUp;
using GraphQL;
using GraphQL.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.GraphQL.Test
{
    public class TestFixture
    {
        #region Statics

        private static string ConnectionString { get; set; }

        static TestFixture()
        {
            EnsureSqlExpressDatabase();
        }

        private static void EnsureSqlExpressDatabase()
        {
            ConnectionString = "Server=(localdb)\\dapper-graphql-test;Integrated Security=true;MultipleActiveResultSets=true";

            // Drop the database if it already exists
            DropDatabase.For.SqlDatabase(ConnectionString);

            // Ensure the database exists
            EnsureDatabase.For.SqlDatabase(ConnectionString);

            var upgrader = DeployChanges.To
                .SqlDatabase(ConnectionString)
                .WithScriptsEmbeddedInAssembly(typeof(Person).GetTypeInfo().Assembly)
                .LogToConsole()
                .Build();

            var upgradeResult = upgrader.PerformUpgrade();
        }

        #endregion Statics

        private readonly DocumentExecuter documentExecuter;
        public PersonSchema Schema { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        public TestFixture()
        {
            this.documentExecuter = new DocumentExecuter();
            var serviceCollection = new ServiceCollection();

            SetupDapperGraphQL(serviceCollection);
            SetupDatabaseConnection(serviceCollection);

            this.ServiceProvider = serviceCollection.BuildServiceProvider();
            this.Schema = ServiceProvider.GetRequiredService<PersonSchema>();
        }

        public Func<object[], TEntityType> BuildMapper<TEntityType>(Func<TEntityType, object> mapper)
            where TEntityType : class
        {
            return ServiceProvider.GetRequiredService<IEntityMapperFactory>().Build(mapper);
        }

        public IDbConnection GetDbConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public bool JsonEquals(string expectedJson, string actualJson)
        {
            // To ensure formatting doesn't affect our results, we first convert to JSON tokens
            // and only compare the structure of the resulting objects.
            return JToken.DeepEquals(JObject.Parse(expectedJson), JObject.Parse(actualJson));
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

        private void SetupDatabaseConnection(ServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDbConnection>(serviceProvider => GetDbConnection());
        }
    }
}