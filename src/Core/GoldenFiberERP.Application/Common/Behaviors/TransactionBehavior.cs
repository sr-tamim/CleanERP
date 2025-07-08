using MediatR;
using GoldenFiberERP.Domain.Interfaces;

namespace GoldenFiberERP.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for transaction management
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only apply transaction for commands (not queries)
        var requestName = typeof(TRequest).Name;
        var isCommand = requestName.EndsWith("Command");

        if (!isCommand)
        {
            return await next();
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var response = await next();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return response;
        }, cancellationToken);
    }
}
