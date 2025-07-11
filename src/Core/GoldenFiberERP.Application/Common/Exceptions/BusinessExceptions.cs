namespace GoldenFiberERP.Application.Common.Exceptions;

/// <summary>
/// Exception for business logic violations
/// </summary>
public class BusinessLogicException : Exception
{
    public string ErrorCode { get; }
    
    public BusinessLogicException(string message, string errorCode = "BUSINESS_LOGIC_ERROR") 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessLogicException(string message, Exception innerException, string errorCode = "BUSINESS_LOGIC_ERROR") 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception for forbidden operations
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException() : base()
    {
    }

    public ForbiddenException(string message) : base(message)
    {
    }

    public ForbiddenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception for conflict scenarios (e.g., duplicate resources)
/// </summary>
public class ConflictException : Exception
{
    public ConflictException() : base()
    {
    }

    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ConflictException(string name, object key) 
        : base($"Entity \"{name}\" ({key}) already exists.")
    {
    }
}
