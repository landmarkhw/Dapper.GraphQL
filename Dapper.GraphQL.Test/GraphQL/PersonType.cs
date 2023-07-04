using Dapper.GraphQL.Test.Models;
using GraphQL.Types;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PersonType :
        ObjectGraphType<Person>
    {
        public PersonType()
        {
            Name = "person";
            Description = "A person.";

            Field<IntGraphType>("id")
                .Description("A unique identifier for the person.")
                .Resolve(context => context.Source?.Id);

            Field<StringGraphType>("firstName")
                .Description("The first name of the person.")
                .Resolve(context => context.Source?.FirstName);

            Field<StringGraphType>("lastName")
                .Description("The last name of the person.")
                .Resolve(context => context.Source?.LastName);

            Field<ListGraphType<CompanyType>>("companies")
                .Description("A list of companies for this person.")
                .Resolve(context => context.Source?.Companies);

            Field<ListGraphType<EmailType>>("emails")
                .Description("A list of email addresses for the person.")
                .Resolve(context => context.Source?.Emails);

            Field<ListGraphType<PhoneType>>("phones")
                .Description("A list of phone numbers for the person.")
                .Resolve(context => context.Source?.Phones);

            Field<PersonType>("supervisor")
                .Description("This person's supervisor.")
                .Resolve(context => context.Source?.Supervisor);

            Field<PersonType>("careerCounselor")
                .Description("This person's career counselor.")
                .Resolve(context => context.Source?.CareerCounselor);
        }
    }
}
