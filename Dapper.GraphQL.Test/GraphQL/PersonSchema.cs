using System;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PersonSchema :
        global::GraphQL.Types.Schema
    {
        public PersonSchema(
            IServiceProvider serviceProvider,
            PersonQuery personQuery,
            PersonMutation personMutation)
        {
            Mutation = personMutation;
            ResolveType = type => serviceProvider.GetRequiredService(type) as GraphType;
            Query = personQuery;
        }
    }
}