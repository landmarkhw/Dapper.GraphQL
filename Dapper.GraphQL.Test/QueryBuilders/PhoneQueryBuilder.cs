using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Language.AST;
using Dapper.GraphQL.Test.Models;

namespace Dapper.GraphQL.Test.QueryBuilders
{
    public class PhoneQueryBuilder :
        IQueryBuilder<Phone>
    {
        public SqlBuilder Build(SqlBuilder query, IHaveSelectionSet context, string alias)
        {
            query.Select($"{alias}.Id");
            query.SplitOn<Phone>("Id");

            var fields = context.GetSelectedFields();
            foreach (var kvp in fields)
            {
                switch (kvp.Key)
                {
                    case "number": query.Select($"{alias}.Number"); break;
                    case "type": query.Select($"{alias}.Type"); break;
                }
            }

            return query;
        }
    }
}