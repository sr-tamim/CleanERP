namespace CleanERP.Application.Common.Interfaces;

public interface IDateTime
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
