using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapper.GraphQL
{
    public static class ParameterHelper
    {
        private static Dictionary<Type, PropertyInfo[]> PropertyCache = new Dictionary<Type, PropertyInfo[]>();
        private static Dictionary<Type, TypeInfo> TypeInfoCache = new Dictionary<Type, TypeInfo>();

        public static IEnumerable<KeyValuePair<string, object>> GetSetFlatProperties<TType>(TType obj)
        {
            var type = obj.GetType();
            PropertyInfo[] properties;
            if (!PropertyCache.ContainsKey(type))
            {
                // Get a list of properties that are "flat" on this object, i.e. singular values
                properties = type
                    .GetProperties()
                    .Where(p =>
                    {
                        var typeInfo = GetTypeInfo(p.PropertyType);

                        // Explicitly permit primitive, value, and serializable types
                        if (typeInfo.IsSerializable || typeInfo.IsPrimitive || typeInfo.IsValueType)
                        {
                            return true;
                        }

                        // Filter out list-types
                        if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                        {
                            return false;
                        }
                        if (p.PropertyType.IsConstructedGenericType)
                        {
                            var typeDef = p.PropertyType.GetGenericTypeDefinition();
                            if (typeof(IEnumerable<>).IsAssignableFrom(typeDef) ||
                                typeof(ICollection<>).IsAssignableFrom(typeDef) ||
                                typeof(IList<>).IsAssignableFrom(typeDef))
                            {
                                return false;
                            }
                        }
                        return true;
                    })
                    .ToArray();

                // Cache those properties
                PropertyCache[type] = properties;
            }
            else
            {
                properties = PropertyCache[type];
            }

            // Convert the properties to a dictionary where:
            // Key   = property name
            // Value = property value, or null if the property is set to its default value
            return properties
                .ToDictionary(
                    prop => prop.Name,
                    prop =>
                    {
                        // Ensure scalar values are properly skipped if they are set to their initial, default(type) value.
                        var value = prop.GetValue(obj);
                        if (value != null)
                        {
                            var valueType = value.GetType();
                            var valueTypeInfo = GetTypeInfo(valueType);
                            if (valueTypeInfo.IsValueType &&
                                object.Equals(value, Activator.CreateInstance(valueType)))
                            {
                                return null;
                            }
                        }
                        return value;
                    }
                )
                // Then, filter out "unset" properties, or properties that are set to their default value
                .Where(kvp => kvp.Value != null);
        }

        private static TypeInfo GetTypeInfo(Type type)
        {
            if (!TypeInfoCache.ContainsKey(type))
            {
                TypeInfoCache[type] = type.GetTypeInfo();
            }
            return TypeInfoCache[type];
        }
    }
}