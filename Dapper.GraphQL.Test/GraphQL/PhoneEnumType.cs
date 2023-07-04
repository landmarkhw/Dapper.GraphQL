using GraphQL.Types;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PhoneEnumType
        : EnumerationGraphType<Models.PhoneType>
    {
        public PhoneEnumType()
        {
            Name = "PhoneType";
        }
    }
}
