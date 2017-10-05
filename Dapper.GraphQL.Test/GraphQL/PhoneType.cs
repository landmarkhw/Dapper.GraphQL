using Dapper.GraphQL.Test.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PhoneType :
        ObjectGraphType<Phone>
    {
        public PhoneType()
        {
            Name = "phone";
            Description = "A phone number.";

            Field<IntGraphType>(
                "id",
                description: "A unique identifier for the phone number.",
                resolve: context => context.Source.Id
            );

            Field<StringGraphType>(
                "number",
                description: "The phone number.",
                resolve: context => context.Source.Number
            );

            Field<StringGraphType>(
                "type",
                description: "The type of phone number.  One of 'home', 'work', 'mobile', or 'other'.",
                resolve: context => context.Source.Type
            );
        }
    }
}