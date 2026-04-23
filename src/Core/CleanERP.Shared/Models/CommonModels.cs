namespace CleanERP.Shared.Models;

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;
}

public class PaginationResponse<T>
{
    public IEnumerable<T> Data { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public class SortRequest
{
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

public class FilterRequest
{
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Dictionary<string, object> AdditionalFilters { get; set; } = new();
}

public class AuditInfo
{
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Changes { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class NotificationModel
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Type { get; set; } // Success, Warning, Error, Info
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class FileUploadModel
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long Size { get; set; }
    public Stream? Content { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}
