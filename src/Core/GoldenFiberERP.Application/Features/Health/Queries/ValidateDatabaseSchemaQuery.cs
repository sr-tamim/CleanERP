using MediatR;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;

namespace GoldenFiberERP.Application.Features.Health.Queries;

public record ValidateDatabaseSchemaQuery : IRequest<Result<SchemaValidationResult>>;

public class ValidateDatabaseSchemaQueryHandler : IRequestHandler<ValidateDatabaseSchemaQuery, Result<SchemaValidationResult>>
{
    private readonly IHealthCheckService _healthCheckService;

    public ValidateDatabaseSchemaQueryHandler(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public async Task<Result<SchemaValidationResult>> Handle(ValidateDatabaseSchemaQuery request, CancellationToken cancellationToken)
    {
        return await _healthCheckService.ValidateDatabaseSchemaAsync(cancellationToken);
    }
}
