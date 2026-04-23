using MediatR;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Application.Common.Models;

namespace CleanERP.Application.Features.Health.Queries;

public record GetDatabaseConnectionQuery : IRequest<Result<DatabaseConnectionResult>>;

public class GetDatabaseConnectionQueryHandler : IRequestHandler<GetDatabaseConnectionQuery, Result<DatabaseConnectionResult>>
{
    private readonly IHealthCheckService _healthCheckService;

    public GetDatabaseConnectionQueryHandler(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public async Task<Result<DatabaseConnectionResult>> Handle(GetDatabaseConnectionQuery request, CancellationToken cancellationToken)
    {
        return await _healthCheckService.GetDatabaseConnectionAsync(cancellationToken);
    }
}
