using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.GraphQL
{
    /// <summary>
    /// Options used to configure the dependency injection container for GraphQL and Dapper.
    /// </summary>
    public class GraphQLDapperOptions
    {
        private readonly IServiceCollection serviceCollection;

        public GraphQLDapperOptions(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        /// <summary>
        /// Adds a GraphQL entity mapper to the container.
        /// </summary>
        /// <typeparam name="TModelType">The model type to be mapped.</typeparam>
        /// <typeparam name="TEntityMapper">The mapper class.</typeparam>
        /// <returns>The GraphQLOptions object.</returns>
        public GraphQLDapperOptions AddEntityMapper<TModelType, TEntityMapper>()
            where TModelType : class
            where TEntityMapper : class, IEntityMapper<TModelType>
        {
            serviceCollection.AddTransient<IEntityMapper<TModelType>, TEntityMapper>();
            return this;
        }

        /// <summary>
        /// Adds a GraphQL query builder to the container.
        /// </summary>
        /// <typeparam name="TModelType">The model type to be queried.</typeparam>
        /// <typeparam name="TQueryBuilder">The query builder class.</typeparam>
        /// <returns>The GraphQLOptions object.</returns>
        public GraphQLDapperOptions AddQueryBuilder<TModelType, TQueryBuilder>()
            where TQueryBuilder : class, IQueryBuilder<TModelType>
        {
            serviceCollection.AddSingleton<IQueryBuilder<TModelType>, TQueryBuilder>();
            return this;
        }

        /// <summary>
        /// Adds a GraphQL type to the container.
        /// </summary>
        /// <typeparam name="TGraphType">The model type to be mapped.</typeparam>
        /// <returns>The GraphQLOptions object.</returns>
        public GraphQLDapperOptions AddType<TGraphType>() where TGraphType : class, IGraphType
        {
            serviceCollection.AddSingleton<TGraphType>();
            return this;
        }
    }
}