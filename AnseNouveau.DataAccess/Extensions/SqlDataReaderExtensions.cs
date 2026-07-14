using Microsoft.Data.SqlClient;

namespace AnseNouveau.DataAccess.Extensions
{
    public static class SqlDataReaderExtensions
    {
        public static string? GetStringOrNull(this SqlDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        public static int? GetInt32OrNull(this SqlDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
        }

        public static decimal? GetDecimalOrNull(this SqlDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
        }

        public static DateTime? GetDateTimeOrNull(this SqlDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
        }
    }
}
