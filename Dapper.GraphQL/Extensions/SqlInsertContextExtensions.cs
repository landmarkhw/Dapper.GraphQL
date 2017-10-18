using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Dapper.GraphQL
{
    public static class SqlInsertContextExtensions
    {
        public static TIdentityType ExecuteWithSqlIdentity<TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = new StringBuilder();

            if (typeof(TIdentityType) == typeof(int))
            {
                sb.AppendLine(context.ToString());
                sb.AppendLine("SELECT CAST(SCOPE_IDENTITY() AS INT)");
            }
            else if (typeof(TIdentityType) == typeof(Guid))
            {
                sb.AppendLine($"DECLARE @InsertedRows AS TABLE (Id UNIQUEIDENTIFIER);");
                sb.AppendLine(context.ToString());
                sb.AppendLine($"SELECT Id FROM @InsertedRows");
            }
            else throw new InvalidCastException($"Type {typeof(TIdentityType).Name} in not supported this SQL context.");

            return dbConnection
                .Query<TIdentityType>(sb.ToString(), context.Parameters)
                .Single();
        }

        public static int ExecuteWithSqliteIdentity(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = new StringBuilder();

            sb.AppendLine(context.ToString());
            sb.AppendLine("SELECT last_insert_rowid();");

            return dbConnection
                .Query<int>(sb.ToString(), context.Parameters)
                .Single();
        }
    }
}