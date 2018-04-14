using System;
using System.Collections.Generic;

namespace Dapper.GraphQL
{
    /// <summary>
    /// Helper for table related operations.
    /// </summary>
    public static class TableHelper
    {
        private static Dictionary<Type, string> TableNameCache = new Dictionary<Type, string>();

        /// <summary>
        /// Gets a table name for a given type, as configured with a default.
        /// </summary>
        /// <typeparam name="TType">The type to get a table name for.</typeparam>
        /// <returns>The configured table name or a generated default.</returns>
        public static string GetTableName<TType>()
        {
            var type = typeof(TType);
            if (!TableNameCache.ContainsKey(type))
            {
                lock (TableNameCache)
                {
                    if (!TableNameCache.ContainsKey(type))
                    {
                        // Generate a default and cache it
                        TableNameCache[type] = type.Name;
                    }
                    else
                    {
                        return TableNameCache[type];
                    }
                }
            }

            return TableNameCache[type];
        }

        public static void AddCustomTableNameMapping<TType>(string name)
        {
            var type = typeof(TType);
            if (TableNameCache.ContainsKey(type))
            {
                throw new InvalidOperationException($"Dapper.GraphQL - Cannot add duplicate custom table name for type '${type.Name}'");
            }

            TableNameCache[typeof(TType)] = name;
        }
    }
}