# Dapper.GraphQL
A library designed to integrate the Dapper and graphql-dotnet projects with ease-of-use in mind and performance as the primary concern.

# Design
Dapper.GraphQL combines the ideas that come out-of-the-box with graphql-dotnet, and adds the following concepts:

1. Query builders
2. Entity mapper

## Query Builders

Query builders are used to build dynamic queries based on what the client asked for in their GraphQL query.  For example, given this
GraphQL query:

```graphql
query {
    people {
        id
        firstName
        lastName
    }
}
```

A proper query builder will generate a SQL query that looks something like this:

```sql
SELECT id, firstName, lastName
FROM Person
```

Using the same query builder, and given the following GraphQL:

```graphql
query {
    people {
        id
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
            type
        }
    }
}
```

A more complex query should be generated, something like:

```sql
SELECT
  person.Id, person.firstName, person.lastName,
  email.id, email.address,
  phone.id, phone.number, phone.type  
FROM 
  Person person LEFT OUTER JOIN
  Email email ON person.Id = email.PersonId LEFT OUTER JOIN
  Phone phone ON person.Id = phone.PersonId
```

# Usage

## Setup

Dapper.GraphQL uses Microsoft's standard DI container, IServiceCollection, to manage all of the Dapper and GraphQL interactions.
If you're developing in ASP.NET Core, you can add this to the ConfigureServices() method:

```csharp
serviceCollection.AddDapperGraphQL(options =>
{
    // Add GraphQL types
    options.AddType<EmailType>();
    options.AddType<PersonType>();
    options.AddType<GraphQL.PhoneType>();
    options.AddType<PersonQuery>();

    // Add the GraphQL schema
    options.AddSchema<PersonSchema>();

    // Add query builders for dapper
    options.AddQueryBuilder<Email, EmailQueryBuilder>();
    options.AddQueryBuilder<Person, PersonQueryBuilder>();
    options.AddQueryBuilder<Phone, PhoneQueryBuilder>();

    // Add entity mappers
    options.AddEntityMapper<Person, PersonEntityMapper>();
});
```

## Queries

When creating a SQL query based on a GraphQL query, you need 2 things to build the query properly:  A *query builder* and *entity mapper*.

### Query builder

Each entity in a system should have its own query builder, so any GraphQL queryies that interact with those entities can be automatically
handled, even when nested within other entities.

In the above setup, the `Email` query builder looks like this:

```csharp
public class EmailQueryBuilder :
    IQueryBuilder<Email>
{
    public SqlQueryContext Build(SqlQueryContext query, IHaveSelectionSet context, string alias)
    {
        query.Select($"{alias}.Id");
        query.SplitOn<Email>("Id");

        var fields = context.GetSelectedFields();
        foreach (var kvp in fields)
        {
            switch (kvp.Key)
            {
                case "address": query.Select($"{alias}.Address"); break;
            }
        }

        return query;
    }
}
```

#### Arguments

* The `query` represents the SQL query that's been generated so far.
* The `context` is the GraphQL context - it contains the GraphQL query and what data has been requested.
* The `alias` is what SQL alias the current table has.  Since entities can be used more than once (multiple entities can have an `Email`, for example), it's important that our aliases are unique.

#### `Build()` method

Let's break down what's happening in the `Build()` method:

1. `query.Select($"{alias}.Id");` - select the Id of the entity.  This is good practice, so that even if the GraphQL query didn't include the `id`, we always include it.
2. `query.SplitOn<Email>("Id");` - tell Dapper that the `Id` marks the beginning of the `Email` class.
3. `var fields = context.GetSelectedFields();` - Gets a list of fields that have been selected from GraphQL.
4. `case "address": query.Select($"{alias}.Address");` - When `address` is found in the GraphQL query, add the `Address` to the SQL SELECT clause.

#### Query builder chaining

Query builders are intended to chain, as our entities tend to have a hierarchical relationship.  See the `PersonQueryBuilder.cs` file in the test project for a good example of chaining.

## GraphQL integration

Here's an example of a query definition in `graphql-dotnet`:

```csharp
Field<ListGraphType<PersonType>>(
    "people",
    description: "A list of people.",
    resolve: context =>
    {
        // Create an alias for the 'Person' table.
        var alias = "person";
        // Add the 'Person' table to the FROM clause in SQL
        var query = SqlBuilder.From($"Person {alias}");
        // Build the query, using the GraphQL query and SQL table alias.
        query = personQueryBuilder.Build(query, context.FieldAst, alias);

        // Create a mapper that understands how to uniquely identify the 'Person' class.
        var personMapper = entityMapperFactory.Build<Person>(person => person.Id);

        // Open a connection to the database
        using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
        {
            // Execute the query with the person mapper
            var results = query.Execute(connection, personMapper);
            
            // `results` contains a list of people.
            return results;
        }
    }
);
```

# Examples

See the Dapper.GraphQL.Test project for a full set of examples, including how *query builders* and *entity mappers* are designed.

# Roadmap

* Fluent-style pagination
