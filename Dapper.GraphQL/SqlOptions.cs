using System.Data;

namespace Dapper.GraphQL
{
    /// <summary>
    /// Options for database queries and commands.
    /// These options corresponds to the optional parameters used by
    /// Dapper's SqlMapper class.
    /// </summary>
    public class SqlMapperOptions
    {
        /// <summary>
        /// The default options used unless other are specified.
        /// </summary>
        public static readonly SqlMapperOptions DefaultOptions = new SqlMapperOptions();

        /// <summary>
        /// Number of seconds before command execution timeout.
        /// </summary>
        public int? CommandTimeout { get; set; }

        /// <summary>
        /// Is it a stored proc or a batch?
        /// </summary>
        public CommandType? CommandType { get; set; }

        /// <summary>
        /// Whether to buffer the results in memory.
        /// </summary>
        public bool Buffered { get; set; } = true;
    }
}
