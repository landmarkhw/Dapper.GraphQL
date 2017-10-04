using System;
using System.Reflection;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class SqlQueryBuilderTests
    {
        private readonly MethodInfo parseAliasMethod;
        private readonly SqlQueryBuilder sqlQueryBuilder;

        public SqlQueryBuilderTests()
        {
            sqlQueryBuilder = new SqlQueryBuilder();
            parseAliasMethod = typeof(SqlQueryBuilder).GetMethod("ParseAlias", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [Theory]
        [InlineData("test.Table table")]
        [InlineData("[test].Table table")]
        [InlineData("  [test].Table AS  table")]
        [InlineData("[test].[Table] [table]")]
        [InlineData("[test].[Table] AS [table]")]
        [InlineData("[test].[Table] table")]
        [InlineData("test.[Table] table")]
        [InlineData("[Table] table")]
        [InlineData("Table table")]
        [InlineData("test.Table table INNER JOIN other.OtherTable otherTable ON table.Id = otherTable.Id")]
        public void AliasParsingShouldSucceed(string value)
        {
            var alias = parseAliasMethod.Invoke(sqlQueryBuilder, new[] { value });
            Assert.Equal("table", alias);
        }
    }
}