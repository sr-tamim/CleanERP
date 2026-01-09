using System.Linq.Expressions;

namespace GoldenFiberERP.Domain.Specifications;

/// <summary>
/// Base interface for specifications
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Expression that defines the criteria for this specification
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// List of include expressions for related entities
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// List of include expressions for related entities (as strings)
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Order by expression
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Order by descending expression
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Secondary order by expressions
    /// </summary>
    List<Expression<Func<T, object>>> ThenBy { get; }

    /// <summary>
    /// Secondary order by descending expressions
    /// </summary>
    List<Expression<Func<T, object>>> ThenByDescending { get; }

    /// <summary>
    /// Group by expression
    /// </summary>
    Expression<Func<T, object>>? GroupBy { get; }

    /// <summary>
    /// Number of records to take
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Number of records to skip
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Whether paging is enabled
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Check if an entity satisfies this specification
    /// </summary>
    bool IsSatisfiedBy(T entity);
}
