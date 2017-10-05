using Dapper.GraphQL.Test.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class EmailType :
        ObjectGraphType<Email>
    {
        public EmailType()
        {
            Name = "email";
            Description = "An email address.";

            Field<IntGraphType>(
                "id",
                description: "A unique identifier for the email address.",
                resolve: context => context.Source.Id
            );

            Field<StringGraphType>(
                "address",
                description: "The email address.",
                resolve: context => context.Source.Address
            );
        }
    }
}