using System.Data;

namespace Dapper.GraphQL
{
    public class SqlOptions {
        public static readonly SqlOptions DefaultOptions = new SqlOptions();

        public int? CommandTimeout { get; set; }

        public CommandType? CommandType { get; set; }

        public bool Buffered { get; set; } = true;
    }
}
