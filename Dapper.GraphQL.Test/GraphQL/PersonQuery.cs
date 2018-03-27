using Dapper.GraphQL.Test.Models;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

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
                    var query = SqlBuilder.From($"Person {alias}");
                    query = personQueryBuilder.Build(query, context.FieldAst, alias);

                    // Create a mapper that understands how to uniquely identify the 'Person' class.
                    var personMapper = entityMapperFactory.Build<Person>(
                        person => person.Id,
                        context.FieldAst,
                        query.GetSplitOnTypes()
                    );

                    using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                    {
                        var results = query.Execute(connection, personMapper);
                        return results;
                    }
                }
            );

            FieldAsync<ListGraphType<PersonType>>(
                "peopleAsync",
                description: "A list of people fetched asynchronously.",
                resolve: async context => 
                {
                    var alias = "person";
                    var query = SqlBuilder.From($"Person {alias}");
                    query = personQueryBuilder.Build(query, context.FieldAst, alias);

                    // Create a mapper that understands how to uniquely identify the 'Person' class.
                    var personMapper = entityMapperFactory.Build<Person>(
                        person => person.Id,
                        context.FieldAst,
                        query.GetSplitOnTypes()
                    );

                    using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                    {
                        var results = await query.ExecuteAsync(connection, personMapper);
                        return results;
                    }
                }
            );

            Field<PersonType>(
                "person",
                description: "Gets a person by ID.",
                arguments: new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "id", Description = "The ID of the person." }
                ),
                resolve: context =>
                {
                    var id = context.Arguments["id"];
                    var alias = "person";
                    var query = SqlBuilder
                        .From($"Person {alias}")
                        .Where($"{alias}.Id = @id", new { id });

                    query = personQueryBuilder.Build(query, context.FieldAst, alias);

                    // Create a mapper that understands how to uniquely identify the 'Person' class.
                    var personMapper = entityMapperFactory.Build<Person>(
                        person => person.Id,
                        context.FieldAst,
                        query.GetSplitOnTypes()
                    );

                    using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                    {
                        var results = query.Execute(connection, personMapper);
                        return results.FirstOrDefault();
                    }
                }
            );
        }
    }
}