using Dapper.GraphQL.Test.EntityMappers;
using Dapper.GraphQL.Test.Models;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Linq;

namespace Dapper.GraphQL.Test.GraphQL
{
    public class PersonQuery :
        ObjectGraphType
    {
        public PersonQuery(
            IQueryBuilder<Person> personQueryBuilder,
            IServiceProvider serviceProvider)
        {
            Field<ListGraphType<PersonType>>("people")
                .Description("A list of people.")
                .Resolve(context =>
                    {
                        var alias = "person";
                        var query = SqlBuilder
                            .From<Person>(alias)
                            .OrderBy($"{alias}.Id");
                        query = personQueryBuilder.Build(query, context.FieldAst, alias);

                        // Create a mapper that understands how to uniquely identify the 'Person' class,
                        // and will deduplicate people as they pass through it
                        var personMapper = new PersonEntityMapper();

                        using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                        {
                            var results = query
                                .Execute(connection, context.FieldAst, personMapper)
                                .Distinct();
                            return results;
                        }
                    });

            Field<ListGraphType<PersonType>>("peopleAsync")
                .Description("A list of people fetched asynchronously.")
                .ResolveAsync(async context =>
                {
                    var alias = "person";
                    var query = SqlBuilder
                        .From($"Person {alias}")
                        .OrderBy($"{alias}.Id");
                    query = personQueryBuilder.Build(query, context.FieldAst, alias);

                    // Create a mapper that understands how to uniquely identify the 'Person' class,
                    // and will deduplicate people as they pass through it
                    var personMapper = new PersonEntityMapper();

                    using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                    {
                        connection.Open();

                        var results = await query.ExecuteAsync(connection, context.FieldAst, personMapper);
                        return results.Distinct();
                    }
                });

            Field<PersonType>("person")
                .Description("Gets a person by ID.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "id", Description = "The ID of the person." }
                ))
                .Resolve(context =>
                {
                    var id = context.Arguments["id"].Value;
                    var alias = "person";
                    var query = SqlBuilder
                        .From($"Person {alias}")
                        .Where($"{alias}.Id = @id", new { id })
                        // Even though we're only getting a single person, the process of deduplication
                        // may return several entities, so we sort by ID here for consistency
                        // with test results.
                        .OrderBy($"{alias}.Id");

                    query = personQueryBuilder.Build(query, context.FieldAst, alias);

                    // Create a mapper that understands how to uniquely identify the 'Person' class,
                    // and will deduplicate people as they pass through it
                    var personMapper = new PersonEntityMapper();

                    using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                    {
                        var results = query
                            .Execute(connection, context.FieldAst, personMapper)
                            .Distinct();
                        return results.FirstOrDefault();
                    }
                });
        }
    }
}
