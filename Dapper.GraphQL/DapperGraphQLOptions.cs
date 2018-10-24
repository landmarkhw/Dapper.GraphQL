using System;
using System.Reflection;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.GraphQL
{
    /// <summary>
    /// Options used to configure the dependency injection container for GraphQL and Dapper.
    /// </summary>
    public class DapperGraphQLOptions
    {
        private readonly IServiceCollection serviceCollection;

        public DapperGraphQLOptions(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        /// <summary>
        /// Adds a GraphQL query builder to the container.
        /// </summary>
        /// <typeparam name="TModelType">The model type to be queried.</typeparam>
        /// <typeparam name="TQueryBuilder">The query builder class.</typeparam>
        /// <returns>The GraphQLOptions object.</returns>
        public DapperGraphQLOptions AddQueryBuilder<TModelType, TQueryBuilder>()
            where TQueryBuilder : class, IQueryBuilder<TModelType>
        {
            serviceCollection.AddSingleton<IQueryBuilder<TModelType>, TQueryBuilder>();
            return this;
        }

        public DapperGraphQLOptions AddQueryBuilder(Type modelType, Type queryBuilderType)
        {
            var queryBuilderInterface = typeof(IQueryBuilder<>).MakeGenericType(modelType);
            if (!queryBuilderType.IsConcrete() || !queryBuilderInterface.IsAssignableFrom(queryBuilderType))
            {
                throw new ArgumentException($"QueryBuilder type must be concrete and implement IQueryBuilder<{modelType.Name}>.");
            }

            serviceCollection.Add(new ServiceDescriptor(queryBuilderInterface, queryBuilderType, ServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// Adds a GraphQL schema to the container.
        /// </summary>
        /// <typeparam name="TGraphSchema">The schema type to be mapped.</typeparam>
        /// <returns>The GraphQLOptions object.</returns>
        public DapperGraphQLOptions AddSchema<TGraphSchema>() where TGraphSchema : class, ISchema
        {
            serviceCollection.AddSingleton<TGraphSchema>();
            return this;
        }

        public DapperGraphQLOptions AddSchema(Type graphSchemaType)
        {
            if (!graphSchemaType.IsConcrete() || !typeof(ISchema).IsAssignableFrom(graphSchemaType))
            {
                throw new ArgumentException("Type must be concrete and implement ISchema.");
            }

            serviceCollection.Add(new ServiceDescriptor(graphSchemaType, graphSchemaType, ServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// Adds a GraphQL type to the container.
        /// </summary>
        /// <typeparam name="TGraphType">The model type to be mapped.</typeparam>
        /// <returns>The GraphQLOptions object.</returns>
        public DapperGraphQLOptions AddType<TGraphType>() where TGraphType : class, IGraphType
        {
            serviceCollection.AddSingleton<TGraphType>();
            return this;
        }

        public DapperGraphQLOptions AddType(Type type)
        {
            if (!type.IsConcrete() || !typeof(IGraphType).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type must be concrete and implement IGraphType.");
            }

            serviceCollection.Add(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));
            return this;
        }
    }
}