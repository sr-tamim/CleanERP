namespace CleanERP.Shared.Exceptions;

public class BusinessException : Exception
{
    public string ErrorCode { get; }
    
    public BusinessException(string message, string errorCode = "BUSINESS_ERROR") : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(string message, Exception innerException, string errorCode = "BUSINESS_ERROR") 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public class ConfigurationException : Exception
{
    public string ConfigurationKey { get; }
    
    public ConfigurationException(string configurationKey, string message) : base(message)
    {
        ConfigurationKey = configurationKey;
    }

    public ConfigurationException(string configurationKey, string message, Exception innerException) 
        : base(message, innerException)
    {
        ConfigurationKey = configurationKey;
    }
}

public class ExternalServiceException : Exception
{
    public string ServiceName { get; }
    public string? ServiceResponse { get; }
    
    public ExternalServiceException(string serviceName, string message) : base(message)
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, string serviceResponse) : base(message)
    {
        ServiceName = serviceName;
        ServiceResponse = serviceResponse;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException) 
        : base(message, innerException)
    {
        ServiceName = serviceName;
    }
}

public class ConcurrencyException : Exception
{
    public string EntityName { get; }
    public string EntityId { get; }
    
    public ConcurrencyException(string entityName, string entityId) 
        : base($"The {entityName} with ID {entityId} was modified by another user. Please refresh and try again.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

public class SecurityException : Exception
{
    public string? UserId { get; }
    public string? Action { get; }
    
    public SecurityException(string message) : base(message)
    {
    }

    public SecurityException(string message, string userId, string action) : base(message)
    {
        UserId = userId;
        Action = action;
    }
}
