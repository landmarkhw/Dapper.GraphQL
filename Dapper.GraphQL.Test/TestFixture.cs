using Dapper.GraphQL.Test.EntityMappers;
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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.GraphQL.Test
{
    public class DbConnectionWrapper : IDbConnection
    {
        private readonly IDbConnection dbConnection;
        private readonly Action onDispose;
        private bool isDisposed = false;

        public string ConnectionString { get => dbConnection.ConnectionString; set => dbConnection.ConnectionString = value; }

        public int ConnectionTimeout => dbConnection.ConnectionTimeout;

        public string Database => dbConnection.Database;

        public ConnectionState State => dbConnection.State;

        public DbConnectionWrapper(IDbConnection dbConnection, Action onDispose)
        {
            this.dbConnection = dbConnection;
            this.onDispose = onDispose;
        }

        public IDbTransaction BeginTransaction()
        {
            return dbConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return dbConnection.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        {
            dbConnection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            dbConnection.Close();
        }

        public IDbCommand CreateCommand()
        {
            return dbConnection.CreateCommand();
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                dbConnection.Dispose();

                isDisposed = true;
                onDispose();
            }
        }

        public void Open()
        {
            dbConnection.Open();
        }
    }

    public class TestFixture
    {
        #region Statics

        private static string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        private static Random random = new Random((int)(DateTime.Now.Ticks << 32));

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

        public IDbConnection GetDbConnection()
        {
            // Generate a random db name
            var dbName = "test-" + new string(chars.OrderBy(c => random.Next()).ToArray());

            var connectionString = $"Server=localhost;Port=5432;Database={dbName};User Id=postgres;Password=dapper-graphql;";

            // Ensure the database exists
            EnsureDatabase.For.PostgresqlDatabase(connectionString);

            var upgrader = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(typeof(Person).GetTypeInfo().Assembly)
                .LogToConsole()
                .Build();

            var upgradeResult = upgrader.PerformUpgrade();
            if (!upgradeResult.Successful)
            {
                throw new InvalidOperationException("The database upgrade did not succeed for unit testing.", upgradeResult.Error);
            }

            var sqlConnection = new NpgsqlConnection(connectionString);
            var autoClosingConnection = new DbConnectionWrapper(sqlConnection, () =>
            {
                // Connect to a different database, so we can drop the one we were working with
                var dropConnectionString = connectionString.Replace(dbName, "template1");
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
WHERE pg_stat_activity.datname = '{dbName}' AND pid <> pg_backend_pid();

DROP DATABASE ""{dbName}"";";
                    command.CommandType = CommandType.Text;

                    // Drop the database
                    command.ExecuteNonQuery();
                }
            });
            return autoClosingConnection;
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
                options.AddType<CompanyType>();
                options.AddType<EmailType>();
                options.AddType<PersonType>();
                options.AddType<GraphQL.PhoneType>();
                options.AddType<PersonQuery>();

                // Add the GraphQL schema
                options.AddSchema<PersonSchema>();

                // Add query builders for dapper
                options.AddQueryBuilder<Company, CompanyQueryBuilder>();
                options.AddQueryBuilder<Email, EmailQueryBuilder>();
                options.AddQueryBuilder<Person, PersonQueryBuilder>();
                options.AddQueryBuilder<Phone, PhoneQueryBuilder>();
            });
        }

        private void SetupDatabaseConnection(ServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDbConnection>(serviceProvider => GetDbConnection());
        }
    }
}