using MediatR;
using CleanERP.Application.Common.Interfaces;

namespace CleanERP.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior for caching query results
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly ICacheService _cacheService;

    public CachingBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only apply caching for queries (not commands)
        var requestName = typeof(TRequest).Name;
        var isQuery = requestName.EndsWith("Query");

        if (!isQuery)
        {
            return await next();
        }

        // Check if request implements ICacheableRequest
        if (request is ICacheableRequest cacheableRequest)
        {
            var cacheKey = cacheableRequest.CacheKey;

            // Try to get from cache first
            var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            // Not in cache, execute the request
            var response = await next();

            // Cache the response
            await _cacheService.SetAsync(cacheKey, response, cacheableRequest.CacheExpiration);

            return response;
        }

        return await next();
    }
}
