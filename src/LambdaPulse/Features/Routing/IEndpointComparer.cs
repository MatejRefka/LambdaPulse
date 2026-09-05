namespace LambdaPulse.Features.Routing;

/// <summary>
/// Compares endpoints to determine matching priority for an incoming request.
/// </summary>
public interface IEndpointComparer : IComparer<Endpoint>
{
    /// <summary>
    /// Compares two endpoints for route-matching priority. 
    /// Returns a negative number if x has higher priority than y, a positive number if y has higher priority than x, or zero if they have equal priority.
    /// </summary>
    new int Compare(Endpoint x, Endpoint y);
}
