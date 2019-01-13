using System.Collections.Generic;
using GraphQL.Language.AST;
using System.Linq;
using GraphQL;


namespace Dapper.GraphQL.Test.QueryBuilders
{
    public static class QueryBuilderHelper
    {
        public static Dictionary<string, Field> CollectFields(SelectionSet selectionSet)
        {
            return CollectFields(selectionSet, Fields.Empty());
        }

        private static Fields CollectFields(SelectionSet selectionSet, Fields fields)
        {
            List<string> skipList = new List<string> { "edges", "node", "cursor" };
            selectionSet?.Selections.Apply(selection =>
            {
                if (selection is Field field)
                {
                    if (!skipList.Exists(name => name.ToLower().Equals(field.Name)))
                    {
                        fields.Add(field);
                    }

                    CollectFields(field.SelectionSet, fields);
                }
            });

            return fields;
        }

        public static bool IsConnection(SelectionSet selectionSet)
        {
            return IsConnection(selectionSet, new Dictionary<string, Field>());
        }

        public static bool IsConnection(SelectionSet selectionSet, Dictionary<string, Field> fields)
        {
            selectionSet?.Selections.Apply(selection =>
            {
                if (selection is Field field)
                {
                    if (field.Name == "edges")
                    {
                        fields.Add(field.Name, field);
                    }

                    IsConnection(field.SelectionSet, fields);
                }
            });

            return fields.Any();
        }
    }
}
