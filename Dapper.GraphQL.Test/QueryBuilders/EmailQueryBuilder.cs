using System;
using System.Linq;
using Dapper.GraphQL.Test.Models;
using GraphQL.Language.AST;
using GraphQLParser.AST;

namespace Dapper.GraphQL.Test.QueryBuilders
{
    public class EmailQueryBuilder :
        IQueryBuilder<Email>
    {
        public SqlQueryContext Build(SqlQueryContext query, IHasSelectionSetNode context, string alias)
        {
            // Always get the ID of the email
            query.Select($"{alias}.Id");

            // Tell Dapper where the Email class begins (at the Id we just selected)
            query.SplitOn<Email>("Id");

            var fields = context.GetSelectedFields();
            if (fields.Keys.Any(k => k.StringValue.Equals("address", StringComparison.OrdinalIgnoreCase)))
            {
                query.Select($"{alias}.Address");
            }

            return query;
        }
    }
}
