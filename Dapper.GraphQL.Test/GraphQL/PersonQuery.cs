using Dapper.GraphQL.Test.Models;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.Common;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PersonQuery :
        ObjectGraphType
    {
        public PersonQuery(
            IEntityMapperFactory entityMapperFactory,
            IQueryBuilder<Person> personQueryBuilder,
            IServiceProvider serviceProvider)
        {
            Field<ListGraphType<PersonType>>(
                "people",
                description: "A list of people.",
                resolve: context =>
                {
                    var alias = "person";
                    var query = new SqlQueryBuilder().From($"Person {alias}");
                    query = personQueryBuilder.Build(query, context.FieldAst, alias);

                    // Create a mapper that understands how to uniquely identify the 'Person' class.
                    var personMapper = entityMapperFactory.Build<Person>(person => person.Id);

                    using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                    {
                        var results = query.Execute(connection, personMapper);
                        return results;
                    }
                }
          );
        }
    }
}