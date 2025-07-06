using MediatR;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;

namespace GoldenFiberERP.Application.Features.Health.Queries;

public record GetDatabaseInfoQuery : IRequest<Result<DatabaseInfoResult>>;

public class GetDatabaseInfoQueryHandler : IRequestHandler<GetDatabaseInfoQuery, Result<DatabaseInfoResult>>
{
    private readonly IHealthCheckService _healthCheckService;

    public GetDatabaseInfoQueryHandler(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public async Task<Result<DatabaseInfoResult>> Handle(GetDatabaseInfoQuery request, CancellationToken cancellationToken)
    {
        return await _healthCheckService.GetDatabaseInfoAsync(cancellationToken);
    }
}
