namespace QuickBite.API.Exceptions
{
    /// <summary>
    /// Exception for business logic violations
    /// </summary>
    public class BusinessLogicException : Exception
    {
        public string ErrorCode { get; }

        public BusinessLogicException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public BusinessLogicException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}