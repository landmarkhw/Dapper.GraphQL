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
    }
}