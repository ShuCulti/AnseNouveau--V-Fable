namespace AnseNouveau.Domain.Exceptions
{
    public class ForeignKeyViolationException : Exception
    {
        public ForeignKeyViolationException(string message) : base(message)
        {
        }
    }
}
