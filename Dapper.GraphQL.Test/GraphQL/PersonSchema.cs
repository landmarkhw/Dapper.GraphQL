using System;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PersonSchema :
        global::GraphQL.Types.Schema
    {
        public PersonSchema(IServiceProvider services)
            : base(services)
        {
            Query = services.GetService<PersonQuery>();
            Mutation = services.GetService<PersonMutation>();
        }
    }
}
