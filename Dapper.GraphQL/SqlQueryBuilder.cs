using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dapper.GraphQL
{
    /// <summary>
    /// A builder for SQL queries.
    /// </summary>
    public class SqlQueryBuilder
    {
        private readonly Regex AliasPattern = new Regex(@"^\s*(\[?[\w#_$]+\]?\.)?\[?([\w#_$]+)\]?\s+(as\s+)?\[?(?<alias>[\w#_$]+)\]?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public DynamicParameters Parameters { get; set; }
        private StringBuilder _from { get; set; }
        private StringBuilder _orderBy { get; set; }
        private StringBuilder _select { get; set; }
        private List<string> _splitOn { get; set; }
        private List<Type> _types { get; set; }
        private StringBuilder _where { get; set; }
        private HashSet<string> Aliases { get; set; }

        public SqlQueryBuilder()
        {
            Aliases = new HashSet<string>();
            Parameters = new DynamicParameters();
            _splitOn = new List<string>();
            _types = new List<Type>();
            _select = new StringBuilder();
            _from = new StringBuilder();
            _where = new StringBuilder();
            _orderBy = new StringBuilder();
        }

        public SqlQueryBuilder AndWhere(params string[] where)
        {
            if (where.Length > 0)
            {
                if (_where.Length > 0)
                {
                    _where.Append($" AND\n    {string.Join(" AND\n    ", where)}");
                }
                else _where.Append($"\nWHERE\n    {string.Join(" AND\n    ", where)}");
            }

            return this;
        }

        public IEnumerable<TType> Execute<TType>(DbConnection connection, Func<object[], TType> map)
        {
            // FIXME: log instead
            Console.WriteLine(this.ToString());

            var results = connection.Query<TType>(
                sql: this.ToString(),
                types: this._types.ToArray(),
                param: this.Parameters,
                map: map,
                splitOn: string.Join(",", this._splitOn)
            );
            return results.Where(e => e != null);
        }

        public SqlQueryBuilder From(string from, bool ignoreDuplicates = false)
        {
            return From(new[] { from }, ignoreDuplicates);
        }

        public SqlQueryBuilder From(IEnumerable<string> from, bool ignoreDuplicates = false)
        {
            if (from != null && from.Any())
            {
                if (_from.Length == 0)
                {
                    var expr = from.FirstOrDefault();
                    if (CacheAlias(expr, ignoreDuplicates))
                    {
                        _from.Append(expr);
                    }
                    from = from.Skip(1);
                }

                foreach (var expr in from)
                {
                    if (CacheAlias(expr, ignoreDuplicates))
                    {
                        _from.Append($",\n    {expr}");
                    }
                }
            }

            return this;
        }

        public SqlQueryBuilder InnerJoin(string join, bool ignoreDuplicates = false)
        {
            return InnerJoin(new[] { join }, ignoreDuplicates);
        }

        public SqlQueryBuilder InnerJoin(IEnumerable<string> join, bool ignoreDuplicates = false)
        {
            if (join != null && join.Any())
            {
                if (_from.Length > 0)
                {
                    foreach (var expr in join)
                    {
                        if (CacheAlias(expr, ignoreDuplicates))
                        {
                            _from.Append($" INNER JOIN\n    {expr}");
                        }
                    }
                }
                else throw new NotSupportedException("Cannot join before adding a source.");
            }

            return this;
        }

        public SqlQueryBuilder LeftOuterJoin(string join, bool ignoreDuplicates = false)
        {
            return LeftOuterJoin(new[] { join }, ignoreDuplicates);
        }

        public SqlQueryBuilder LeftOuterJoin(IEnumerable<string> join, bool ignoreDuplicates = false)
        {
            if (join != null && join.Any())
            {
                if (_from.Length > 0)
                {
                    foreach (var expr in join)
                    {
                        if (CacheAlias(expr, ignoreDuplicates))
                        {
                            _from.Append($" LEFT OUTER JOIN\n    {expr}");
                        }
                    }
                }
                else throw new NotSupportedException("Cannot join before adding a source.");
            }

            return this;
        }

        public SqlQueryBuilder OrderBy(params string[] orderBy)
        {
            if (orderBy.Length > 0)
            {
                if (_orderBy.Length > 0)
                {
                    _orderBy.Append($",\n    {string.Join(",\n    ", orderBy)}");
                }
                else _orderBy.Append($"\nORDER BY\n    {string.Join(",\n    ", orderBy)}");
            }

            return this;
        }

        public SqlQueryBuilder OrWhere(params string[] where)
        {
            if (where.Length > 0)
            {
                if (_where.Length > 0)
                {
                    _where.Append($" OR\n    {string.Join(" OR\n    ", where)}");
                }
                else _where.Append($"\nWHERE\n    {string.Join(" OR\n    ", where)}");
            }

            return this;
        }

        public SqlQueryBuilder Select(params string[] select)
        {
            if (select.Length > 0)
            {
                if (_select.Length > 0)
                {
                    _select.Append($",\n    {string.Join(",\n    ", select)}");
                }
                else _select.Append(string.Join(",\n    ", select));
            }

            return this;
        }

        public SqlQueryBuilder SplitOn<TEntityType>(string columnName)
        {
            _splitOn.Add(columnName);
            _types.Add(typeof(TEntityType));

            return this;
        }

        public SqlQueryBuilder SplitOn(string columnName, Type entityType)
        {
            _splitOn.Add(columnName);
            _types.Add(entityType);

            return this;
        }

        public override string ToString()
        {
            return $@"SELECT
    {_select}
FROM
    {_from}{_where}{_orderBy}";
        }

        public SqlQueryBuilder Where(params string[] where)
        {
            return AndWhere(where);
        }

        private bool CacheAlias(string fromOrJoin, bool ignoreDuplicates)
        {
            var alias = ParseAlias(fromOrJoin);
            if (alias == null)
            {
                throw new InvalidOperationException("No alias found in expression '{expr}'.");
            }
            if (Aliases.Contains(alias))
            {
                if (ignoreDuplicates)
                {
                    return false;
                }
                else
                {
                    throw new InvalidOperationException($"Alias '{alias}' in expression '{fromOrJoin}' has already been included in the sql query.");
                }
            }
            Aliases.Add(alias);
            return true;
        }

        private string ParseAlias(string fromOrJoin)
        {
            var match = AliasPattern.Match(fromOrJoin);
            if (match.Success)
            {
                return match.Groups["alias"].Value;
            }
            return null;
        }
    }
}