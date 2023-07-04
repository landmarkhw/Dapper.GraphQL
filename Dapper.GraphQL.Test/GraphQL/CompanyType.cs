using Dapper.GraphQL.Test.Models;
using GraphQL.Types;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class CompanyType :
        ObjectGraphType<Company>
    {
        public CompanyType()
        {
            Name = "company";
            Description = "A company.";

            Field<IntGraphType>("id")
                .Description("A unique identifier for the company.")
                .Resolve(context => context.Source?.Id);

            Field<StringGraphType>("name")
                .Description("The name of the company.")
                .Resolve(context => context.Source?.Name);

            Field<ListGraphType<EmailType>>("emails")
                .Description("A list of email addresses for the company.")
                .Resolve(context => context.Source?.Emails);

            Field<ListGraphType<PhoneType>>("phones")
                .Description("A list of phone numbers for the company.")
                .Resolve(context => context.Source?.Phones);
        }
    }
}
