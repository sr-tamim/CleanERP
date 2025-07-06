using MediatR;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;

namespace GoldenFiberERP.Application.Features.Health.Queries;

public record GetAdvancedDatabaseHealthQuery : IRequest<Result<AdvancedHealthCheckResult>>;

public class GetAdvancedDatabaseHealthQueryHandler : IRequestHandler<GetAdvancedDatabaseHealthQuery, Result<AdvancedHealthCheckResult>>
{
    private readonly IHealthCheckService _healthCheckService;

    public GetAdvancedDatabaseHealthQueryHandler(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public async Task<Result<AdvancedHealthCheckResult>> Handle(GetAdvancedDatabaseHealthQuery request, CancellationToken cancellationToken)
    {
        return await _healthCheckService.GetAdvancedDatabaseHealthAsync(cancellationToken);
    }
}
