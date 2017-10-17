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
        public SqlBuilder Build(SqlBuilder query, IHaveSelectionSet context, string alias)
        {
            query.Select($"{alias}.Id");
            query.SplitOn<Email>("Id");

            var fields = context.GetSelectedFields();
            foreach (var kvp in fields)
            {
                switch (kvp.Key)
                {
                    case "address": query.Select($"{alias}.Address"); break;
                }
            }

            return query;
        }
    }
}