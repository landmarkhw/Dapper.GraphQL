using GraphQL.Types;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PersonInputType : InputObjectGraphType
    {
        public PersonInputType()
        {
            Name = "PersonInput";
            Field<StringGraphType>("firstName");
            Field<StringGraphType>("lastName");
        }
    }
}
