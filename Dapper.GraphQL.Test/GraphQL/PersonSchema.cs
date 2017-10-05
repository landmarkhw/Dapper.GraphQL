using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using Dapper.GraphQL.Test.QueryBuilders;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PersonSchema :
        global::GraphQL.Types.Schema
    {
        public PersonSchema(
            IServiceProvider serviceProvider,
            PersonQuery personQuery)
        {
            //Mutation = mutation;
            ResolveType = type => serviceProvider.GetRequiredService(type) as GraphType;
            Query = personQuery;
        }
    }
}