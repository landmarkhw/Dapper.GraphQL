using System.Collections.Generic;
using System.Linq;

namespace GraphQL.Language.AST
{
    public static class IHaveSelectionSetExtensions
    {
        /// <summary>
        /// Returns a map of selected fields, keyed by the field name.
        /// </summary>
        /// <param name="haveSelectionSet">The GraphQL selection set container.</param>
        /// <returns>A dictionary whose key is the field name, and value is the field contents.</returns>
        public static IDictionary<string, Field> GetSelectedFields(this IHaveSelectionSet haveSelectionSet)
        {
            var fields = haveSelectionSet
                .SelectionSet
                .Selections
                .OfType<Field>()
                .ToDictionary(field => field.Name);

            return fields;
        }
    }
}