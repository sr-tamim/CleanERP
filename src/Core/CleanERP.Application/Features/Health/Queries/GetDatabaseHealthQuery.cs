using MediatR;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Application.Common.Models;

namespace CleanERP.Application.Features.Health.Queries;

public record GetDatabaseHealthQuery : IRequest<Result<HealthCheckResult>>;

public class GetDatabaseHealthQueryHandler : IRequestHandler<GetDatabaseHealthQuery, Result<HealthCheckResult>>
{
    private readonly IHealthCheckService _healthCheckService;

    public GetDatabaseHealthQueryHandler(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public async Task<Result<HealthCheckResult>> Handle(GetDatabaseHealthQuery request, CancellationToken cancellationToken)
    {
        return await _healthCheckService.GetDatabaseHealthAsync(cancellationToken);
    }
}
