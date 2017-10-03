using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dapper.GraphQL
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Initializes GraphQL and Dapper with the dependency injection container.
        /// </summary>
        /// <param name="serviceCollection">The service collection container.</param>
        /// <param name="setup">An action used to initialize GraphQL with the DI container.</param>
        /// <returns>The service collection container.</returns>
        public static IServiceCollection AddGraphQLDapper(this IServiceCollection serviceCollection, Action<GraphQLDapperOptions> setup)
        {
            // Inject the entity mapper factory
            serviceCollection.AddSingleton<IEntityMapperFactory, EntityMapperFactory>();

            var options = new GraphQLDapperOptions(serviceCollection);
            setup(options);

            return serviceCollection;
        }
    }
}