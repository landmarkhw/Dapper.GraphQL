using Dapper.GraphQL.Test.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PersonType :
        ObjectGraphType<Person>
    {
        public PersonType()
        {
            Name = "person";
            Description = "A person.";

            Field<IntGraphType>(
                "id",
                description: "A unique identifier for the person.",
                resolve: context => context.Source?.Id
            );

            Field<StringGraphType>(
                "firstName",
                description: "The first name of the person.",
                resolve: context => context.Source?.FirstName
            );

            Field<StringGraphType>(
                "lastName",
                description: "The last name of the person.",
                resolve: context => context.Source?.LastName
            );

            Field<ListGraphType<CompanyType>>(
                "companies",
                description: "A list of companies for this person.",
                resolve: context => context.Source?.Companies
            );

            Field<ListGraphType<EmailType>>(
                "emails",
                description: "A list of email addresses for the person.",
                resolve: context => context.Source?.Emails
            );

            Field<ListGraphType<PhoneType>>(
                "phones",
                description: "A list of phone numbers for the person.",
                resolve: context => context.Source?.Phones
            );

            Field<PersonType>(
                "supervisor",
                description: "This person's supervisor.",
                resolve: context => context.Source?.Supervisor
            );

            Field<PersonType>(
                "careerCounselor",
                description: "This person's career counselor.",
                resolve: context => context.Source?.CareerCounselor
            );
        }
    }
}