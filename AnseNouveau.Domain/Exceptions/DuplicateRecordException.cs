namespace AnseNouveau.Domain.Exceptions
{
    public class DuplicateRecordException : Exception
    {
        public DuplicateRecordException(string message) : base(message)
        {
        }
    }
}
