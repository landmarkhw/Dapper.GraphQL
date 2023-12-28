using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser;
using GraphQLParser.AST;

namespace Dapper.GraphQL
{
    public static class EntityMapContextExtensions
    {
        /// <summary>
        /// Maps the next object from Dapper.
        /// </summary>
        /// <typeparam name="TItemType">The item type to be mapped.</typeparam>
        /// <param name="context">The context used to map object from Dapper.</param>
        /// <param name="fieldName">The name of the GraphQL field associated with the item.</param>
        /// <param name="entityMapper">An optional entity mapper.  This is used to map complex objects from Dapper mapping results.</param>
        /// <returns>The mapped item.</returns>
        public static TItemType Next<TItemType>(
            this EntityMapContext context,
            string fieldName,
            IEntityMapper<TItemType> entityMapper = null)
            where TItemType : class
        {
            var charArray = new ReadOnlyMemory<char>(fieldName.ToCharArray());
            var rom = new ROM(charArray);
            var graphQlName = new GraphQLName(rom);
            return context.Next<TItemType>(
                new[] { fieldName },
                (currentSelectionSet, selectionSet) => currentSelectionSet[graphQlName],
                entityMapper);
        }

        /// <summary>
        /// Maps the next object from Dapper, from a list of fields.
        /// </summary>
        /// <typeparam name="TItemType">The item type to be mapped.</typeparam>
        /// <param name="context">The context used to map object from Dapper.</param>
        /// <param name="fieldNames">The GraphQL fields associated with the item.</param>
        /// <param name="entityMapper">An optional entity mapper.  This is used to map complex objects from Dapper mapping results.</param>
        /// <returns>The mapped item.</returns>
        public static TItemType Next<TItemType>(
            this EntityMapContext context,
            IEnumerable<string> fieldNames,
            IEntityMapper<TItemType> entityMapper = null)
            where TItemType : class
        {
            return context.Next<TItemType>(
                fieldNames,
                (currentSelectionSet, selectionSet) => selectionSet,
                entityMapper);
        }

        /// <summary>
        /// Maps the next object from an inline fragment.
        /// </summary>
        /// <typeparam name="TItemType">The item type to be mapped.</typeparam>
        /// <param name="context">The context used to map object from Dapper.</param>
        /// <param name="fieldName">The GraphQL field that contains the inline fragment(s).</param>
        /// <param name="entityMapper">An optional entity mapper.  This is used to map complex objects from Dapper mapping results.</param>
        /// <returns>The mapped item.</returns>
        public static TItemType NextFragment<TItemType>(
            this EntityMapContext context,
            string fieldName,
            IEntityMapper<TItemType> entityMapper = null)
            where TItemType : class
        {
            var charArray = new ReadOnlyMemory<char>(fieldName.ToCharArray());
            var rom = new ROM(charArray);
            var graphQlName = new GraphQLName(rom);

            return context.Next<TItemType>(
                new[] { fieldName },
                (currentSelectionSet, selectionSet) => currentSelectionSet[graphQlName]
                    .SelectionSet
                    .Selections
                    .OfType<GraphQLInlineFragment>()
                    .FirstOrDefault(f => f.TypeCondition?.Type?.Name?.StringValue == typeof(TItemType).Name),
                entityMapper);
        }
    }
}
