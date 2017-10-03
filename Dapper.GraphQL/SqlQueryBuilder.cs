using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Dapper.GraphQL
{
    /// <summary>
    /// A builder for SQL queries.
    /// </summary>
    public class SqlQueryBuilder
    {
        public DynamicParameters Parameters { get; set; }
        private StringBuilder _from { get; set; }
        private StringBuilder _orderBy { get; set; }
        private StringBuilder _select { get; set; }
        private List<string> _splitOn { get; set; }
        private List<Type> _types { get; set; }
        private StringBuilder _where { get; set; }

        public SqlQueryBuilder()
        {
            _types = new List<Type>();
            _select = new StringBuilder();
            _from = new StringBuilder();
            _where = new StringBuilder();
            _orderBy = new StringBuilder();
            _splitOn = new List<string>();
            Parameters = new DynamicParameters();
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
            return results;
        }

        public SqlQueryBuilder From(params string[] from)
        {
            if (from.Length > 0)
            {
                if (_from.Length > 0)
                {
                    _from.Append($",\n    {string.Join(",\n    ", from)}");
                }
                else _from.Append(string.Join(",\n    ", from));
            }

            return this;
        }

        public SqlQueryBuilder InnerJoin(params string[] join)
        {
            if (join.Length > 0)
            {
                if (_from.Length > 0)
                {
                    _from.Append($" INNER JOIN\n    {string.Join(" INNER JOIN\n    ", join)}");
                }
                else throw new NotSupportedException("Cannot join before adding a source.");
            }

            return this;
        }

        public SqlQueryBuilder LeftOuterJoin(params string[] join)
        {
            if (join.Length > 0)
            {
                if (_from.Length > 0)
                {
                    _from.Append($" LEFT OUTER JOIN\n    {string.Join(" LEFT OUTER JOIN\n    ", join)}");
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
    }
}