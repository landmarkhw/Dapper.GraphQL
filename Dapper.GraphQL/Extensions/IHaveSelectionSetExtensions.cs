using System.Collections.Generic;
using System.Linq;

namespace GraphQL.Language.AST
{
    public static class IHaveSelectionSetExtensions
    {
        /// <summary>
        /// Returns a map of selected fields, keyed by the field name.
        /// </summary>
        /// <param name="selectionSet">The GraphQL selection set container.</param>
        /// <returns>A dictionary whose key is the field name, and value is the field contents.</returns>
        public static IDictionary<string, Field> GetSelectedFields(this IHaveSelectionSet selectionSet)
        {
            if (selectionSet != null)
            {
                var fields = selectionSet
                    .SelectionSet
                    .Selections
                    .OfType<Field>()
                    .ToDictionary(field => field.Name);

                return fields;
            }
            return null;
        }

        /// <summary>
        /// Returns the inline fragment for the specified entity within the GraphQL selection.
        /// </summary>
        /// <typeparam name="TEntityType">The type of entity to retrieve.</typeparam>
        /// <param name="selectionSet">The GraphQL selection set.</param>
        /// <returns>The inline framgent associated with the entity.</returns>
        public static InlineFragment GetInlineFragment<TEntityType>(this IHaveSelectionSet selectionSet)
        {
            return selectionSet
                .SelectionSet?
                .Selections?
                .OfType<InlineFragment>()
                .Where(f => f.Type?.Name == typeof(TEntityType).Name)
                .FirstOrDefault();
        }
    }
}