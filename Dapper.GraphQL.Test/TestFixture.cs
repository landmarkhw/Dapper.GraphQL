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
using System;
using System.Data;
using System.Data.SqlClient;
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
                isDisposed = true;
                onDispose();
            }

            dbConnection.Dispose();
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
        private static Random random = new Random();

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

            var connectionString = $"Server=(localdb)\\mssqllocaldb;Integrated Security=true;MultipleActiveResultSets=true;Database={dbName}";

            // Ensure the database exists
            EnsureDatabase.For.SqlDatabase(connectionString);

            var upgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(typeof(Person).GetTypeInfo().Assembly)
                .LogToConsole()
                .Build();

            var upgradeResult = upgrader.PerformUpgrade();

            var sqlConnection = new SqlConnection(connectionString);
            var autoDroppingConnection = new DbConnectionWrapper(sqlConnection, () =>
            {
                // Drop the database when we're done with it
                DropDatabase.For.SqlDatabase(connectionString);
            });
            return autoDroppingConnection;
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
            });
        }

        private void SetupDatabaseConnection(ServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDbConnection>(serviceProvider => GetDbConnection());
        }
    }
}