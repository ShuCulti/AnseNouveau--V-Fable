using AnseNouveau.DataAccess.Constants;
using AnseNouveau.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace AnseNouveau.DataAccess
{
    internal static class SqlExceptionTranslator
    {
        public static Exception Translate(SqlException exception)
        {
            return exception.Number switch
            {
                SqlErrorCodes.UniqueConstraintViolation or SqlErrorCodes.UniqueIndexViolation =>
                    new DuplicateRecordException("A record with the same unique value already exists."),
                SqlErrorCodes.ForeignKeyViolation =>
                    new ForeignKeyViolationException("The operation references a record that does not exist or is still in use."),
                _ => exception
            };
        }
    }
}
