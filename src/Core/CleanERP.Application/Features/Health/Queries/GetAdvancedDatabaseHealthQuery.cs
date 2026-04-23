using MediatR;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Application.Common.Models;

namespace CleanERP.Application.Features.Health.Queries;

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
