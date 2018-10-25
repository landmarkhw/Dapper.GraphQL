using GraphQL;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PersonSchema :
        global::GraphQL.Types.Schema
    {
        public PersonSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<PersonQuery>();
            Mutation = resolver.Resolve<PersonMutation>();
        }
    }
}