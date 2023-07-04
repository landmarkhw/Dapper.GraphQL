using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapper.GraphQL
{
    public static class ParameterHelper
    {
        private static Dictionary<Type, PropertyInfo[]> _propertyCache = new Dictionary<Type, PropertyInfo[]>();
        private static Dictionary<Type, TypeInfo> _typeInfoCache = new Dictionary<Type, TypeInfo>();

        /// <summary>
        /// Gets a list of flat properties that have been set on the object.
        /// </summary>
        /// <typeparam name="TType">The type to get properties from.</typeparam>
        /// <param name="obj">The object to get properties from.</param>
        /// <returns>A list of key-value pairs of property names and values.</returns>
        public static IEnumerable<KeyValuePair<string, object>> GetSetFlatProperties<TType>(TType obj)
        {
            var type = obj.GetType();
            PropertyInfo[] properties;
            if (!_propertyCache.ContainsKey(type))
            {
                lock (_propertyCache)
                {
                    if (!_propertyCache.ContainsKey(type))
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
                        _propertyCache[type] = properties;
                    }
                    else
                    {
                        properties = _propertyCache[type];
                    }
                }
            }
            else
            {
                properties = _propertyCache[type];
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
            if (!_typeInfoCache.ContainsKey(type))
            {
                lock (_typeInfoCache)
                {
                    if (!_typeInfoCache.ContainsKey(type))
                    {
                        _typeInfoCache[type] = type.GetTypeInfo();
                    }
                }
            }
            return _typeInfoCache[type];
        }
    }
}