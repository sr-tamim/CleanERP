using GoldenFiberERP.Application.Common.Interfaces;

namespace GoldenFiberERP.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
