using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Language.AST;
using Dapper.GraphQL.Test.Models;

namespace Dapper.GraphQL.Test.QueryBuilders
{
    public class EmailQueryBuilder :
        IQueryBuilder<Email>
    {
        public SqlQueryContext Build(SqlQueryContext query, IHaveSelectionSet context, string alias)
        {
            // Always get the ID of the email
            query.Select($"{alias}.Id");
            // Tell Dapper where the Email class begins (at the Id we just selected)
            query.SplitOn<Email>("Id");

            var fields = context.GetSelectedFields();
            if (fields.ContainsKey("address"))
            {
                query.Select($"{alias}.Address");
            }

            return query;
        }
    }
}