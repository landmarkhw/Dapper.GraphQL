using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using Dapper.GraphQL.Test.QueryBuilders;
using DbUp;
using DbUp.SQLite.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class QueryTests
    {
        public QueryTests()
        {
        }

        [Fact(DisplayName = "FROM without SELECT should throw")]
        public void FromWithoutSelectShouldThrow()
        {
            Assert.Throws<InvalidOperationException>(() => new SqlQueryBuilder()
                .From("Customer customer")
                .ToString()
            );
        }
    }
}