using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.GraphQL
{
    public static class SqlInsertContextExtensions
    {
        public static TIdentityType ExecuteWithPostgreSqlIdentity<TEntityType, TIdentityType>(this SqlInsertContext<TEntityType> context, IDbConnection dbConnection, Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
            where TEntityType : class
        {
            if (identityNameSelector.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException("Cannot execute a PostgreSQL identity with an expression of type " + identityNameSelector.Body.NodeType);
            }

            var memberExpression = identityNameSelector.Body as MemberExpression;

            var sb = BuildPostgreSqlIdentityQuery(context, memberExpression.Member.Name.ToLower());

            return dbConnection
                .Query<TIdentityType>(sb.ToString(), context.Parameters)
                .Single();
        }

        public static async Task<TIdentityType> ExecuteWithPostgreSqlIdentityAsync<TEntityType, TIdentityType>(this SqlInsertContext<TEntityType> context, IDbConnection dbConnection, Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
            where TEntityType : class
        {
            if (identityNameSelector.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException("Cannot execute a PostgreSQL identity with an expression of type " + identityNameSelector.Body.NodeType);
            }

            var memberExpression = identityNameSelector.Body as MemberExpression;

            var sb = BuildPostgreSqlIdentityQuery(context, memberExpression.Member.Name.ToLower());

            var result = await dbConnection.QueryAsync<TIdentityType>(sb.ToString(), context.Parameters);
            return result.Single();
        }

        public static TIdentityType ExecuteWithSqlIdentity<TEntityType, TIdentityType>(this SqlInsertContext<TEntityType> context, IDbConnection dbConnection, Func<TEntityType, TIdentityType> identityTypeSelector)
            where TEntityType : class
        {
            return ExecuteWithSqlIdentity<TIdentityType>(context, dbConnection);
        }

        public static TIdentityType ExecuteWithSqlIdentity<TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = BuildSqlIdentityQuery<TIdentityType>(context);

            return dbConnection
                .Query<TIdentityType>(sb.ToString(), context.Parameters)
                .Single();
        }

        public static Task<TIdentityType> ExecuteWithSqlIdentityAsync<TEntityType, TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection, Func<TEntityType, TIdentityType> identityTypeSelector)
            where TEntityType : class
        {
            return ExecuteWithSqlIdentityAsync<TIdentityType>(context, dbConnection);
        }

        public static async Task<TIdentityType> ExecuteWithSqlIdentityAsync<TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = BuildSqlIdentityQuery<TIdentityType>(context);

            var task = dbConnection
                .QueryAsync<TIdentityType>(sb.ToString(), context.Parameters);
            return (await task).Single();
        }

        public static int ExecuteWithSqliteIdentity(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = BuildSqliteIdentityQuery(context);

            return dbConnection
                .Query<int>(sb.ToString(), context.Parameters)
                .Single();
        }

        public static async Task<int> ExecuteWithSqliteIdentityAsync(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = BuildSqliteIdentityQuery(context);

            var task = dbConnection
                .QueryAsync<int>(sb.ToString(), context.Parameters);
            return (await task).Single();
        }

        private static StringBuilder BuildPostgreSqlIdentityQuery(SqlInsertContext context, string idName)
        {
            var sb = new StringBuilder();
            sb.AppendLine(context.ToString());
            sb.AppendLine($"SELECT currval(pg_get_serial_sequence('{context.Table.ToLower()}', '{idName.ToLower()}'));");
            return sb;
        }

        private static StringBuilder BuildSqlIdentityQuery<TIdentityType>(SqlInsertContext context)
        {
            var sb = new StringBuilder();

            if (typeof(TIdentityType) == typeof(int))
            {
                sb.AppendLine(context.ToString());
                sb.AppendLine("SELECT CAST(SCOPE_IDENTITY() AS INT)");
            }
            else if (typeof(TIdentityType) == typeof(long))
            {
                sb.AppendLine(context.ToString());
                sb.AppendLine("SELECT CAST(SCOPE_IDENTITY() AS BIGINT)");
            }
            else
                throw new InvalidCastException($"Type {typeof(TIdentityType).Name} in not supported this SQL context.");

            return sb;
        }

        private static StringBuilder BuildSqliteIdentityQuery(SqlInsertContext context)
        {
            var sb = new StringBuilder();

            sb.AppendLine(context.ToString());
            sb.AppendLine("SELECT last_insert_rowid();");

            return sb;
        }
    }
}
