using GraphQL.Language.AST;
using Dapper.GraphQL.Test.Models;
using GraphQLParser.AST;

namespace Dapper.GraphQL.Test.QueryBuilders
{
    public class PhoneQueryBuilder :
        IQueryBuilder<Phone>
    {
        public SqlQueryContext Build(SqlQueryContext query, IHasSelectionSetNode context, string alias)
        {
            query.Select($"{alias}.Id");
            query.SplitOn<Phone>("Id");

            var fields = context.GetSelectedFields();
            foreach (var kvp in fields)
            {
                switch (kvp.Key.StringValue)
                {
                    case "number": query.Select($"{alias}.Number"); break;
                    case "type": query.Select($"{alias}.Type"); break;
                }
            }

            return query;
        }
    }
}
