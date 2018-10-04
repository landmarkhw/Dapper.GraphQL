using Dapper.GraphQL.Test.GraphQL;
using Dapper.GraphQL.Test.Models;
using Dapper.GraphQL.Test.QueryBuilders;
using DbUp;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Http;
using GraphQL.Language.AST;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.GraphQL.Test
{
    public class TestFixture : IDisposable
    {
        #region Statics

        private static string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        private static Random random = new Random((int)(DateTime.Now.Ticks << 32));

        #endregion Statics

        private readonly string DatabaseName;
        private readonly DocumentExecuter DocumentExecuter;
        public PersonSchema Schema { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        private string ConnectionString { get; set; } = null;
        private bool IsDisposing { get; set; } = false;

        public TestFixture()
        {
            DatabaseName = "test-" + new string(chars.OrderBy(c => random.Next()).ToArray());

            DocumentExecuter = new DocumentExecuter();
            var serviceCollection = new ServiceCollection();

            SetupDatabaseConnection();
            SetupDapperGraphQL(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
            Schema = ServiceProvider.GetRequiredService<PersonSchema>();
        }

        public IHaveSelectionSet BuildGraphQLSelection(string body)
        {
            var document = new GraphQLDocumentBuilder().Build(body);
            return document
                .Operations
                .OfType<IHaveSelectionSet>()
                .First()?
                .SelectionSet
                .Selections
                .OfType<Field>()
                .FirstOrDefault();
        }

        public void Dispose()
        {
            if (!IsDisposing)
            {
                IsDisposing = true;
                //TeardownDatabase();
            }
        }

        public IDbConnection GetDbConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            return connection;
        }

        public bool JsonEquals(string expectedJson, string actualJson)
        {
            // To ensure formatting doesn't affect our results, we first convert to JSON tokens
            // and only compare the structure of the resulting objects.
            return JToken.DeepEquals(JObject.Parse(expectedJson), JObject.Parse(actualJson));
        }

        public async Task<string> QueryGraphQLAsync(string query)
        {
            var result = await DocumentExecuter
                .ExecuteAsync(options =>
                {
                    options.Schema = Schema;
                    options.Query = query;
                })
                .ConfigureAwait(false);

            var json = new DocumentWriter(indent: true).Write(result);
            return json;
        }

        public async Task<string> QueryGraphQLAsync(GraphQlQuery query)
        {
            var result = await DocumentExecuter
                .ExecuteAsync(options =>
                {
                    options.Schema = Schema;
                    options.Query = query.Query;
                    options.Inputs = query.Variables != null ? new Inputs(StringExtensions.GetValue(query.Variables) as Dictionary<string, object>) : null;
                })
                .ConfigureAwait(false);

            var json = new DocumentWriter(indent: true).Write(result);
            return json;
        }

        public void SetupDatabaseConnection()
        {
            // Generate a random db name

            ConnectionString = $"Server=localhost;Port=5432;Database={DatabaseName};User Id=postgres;Password=dapper-graphql;";

            // Ensure the database exists
            EnsureDatabase.For.PostgresqlDatabase(ConnectionString);

            var upgrader = DeployChanges.To
                .PostgresqlDatabase(ConnectionString)
                .WithScriptsEmbeddedInAssembly(typeof(Person).GetTypeInfo().Assembly)
                .LogToConsole()
                .Build();

            var upgradeResult = upgrader.PerformUpgrade();
            if (!upgradeResult.Successful)
            {
                throw new InvalidOperationException("The database upgrade did not succeed for unit testing.", upgradeResult.Error);
            }
        }

        public void TeardownDatabase()
        {
            // Connect to a different database, so we can drop the one we were working with
            var dropConnectionString = ConnectionString.Replace(DatabaseName, "template1");
            using (var connection = new NpgsqlConnection(dropConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // NOTE: I'm not sure why there are active connections to the database at
                // this point, as we're the only ones using this database, and the connection
                // is closed at this point.  In any case, we need to take an extra step of
                // dropping all connections to the database before dropping it.
                //
                // See http://www.leeladharan.com/drop-a-postgresql-database-if-there-are-active-connections-to-it
                command.CommandText = $@"
SELECT pg_terminate_backend(pg_stat_activity.pid)
FROM pg_stat_activity
WHERE pg_stat_activity.datname = '{DatabaseName}' AND pid <> pg_backend_pid();

DROP DATABASE ""{DatabaseName}"";";
                command.CommandType = CommandType.Text;

                // Drop the database
                command.ExecuteNonQuery();
            }
        }

        private void SetupDapperGraphQL(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDapperGraphQL(options =>
            {
                // Add GraphQL types
                options.AddType<CompanyType>();
                options.AddType<EmailType>();
                options.AddType<PersonType>();
                options.AddType<GraphQL.PhoneType>();
                options.AddType<PersonQuery>();
                options.AddType<PersonMutation>();
                options.AddType<PersonInputType>();

                // Add the GraphQL schema
                options.AddSchema<PersonSchema>();

                // Add query builders for dapper
                options.AddQueryBuilder<Company, CompanyQueryBuilder>();
                options.AddQueryBuilder<Email, EmailQueryBuilder>();
                options.AddQueryBuilder<Person, PersonQueryBuilder>();
                options.AddQueryBuilder<Phone, PhoneQueryBuilder>();
            });

            serviceCollection.AddTransient<IDbConnection>(serviceProvider => GetDbConnection());
        }
    }
}