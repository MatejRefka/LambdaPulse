using LambdaPulse.Server.Services.Http.Routing;

namespace LambdaPulse.Server.Utility;

/// <summary>
/// Provides a comparer for two Endpoints. Prioritizes static path segments over parameterized segments.
/// E.g. "/api/users/me" is prioritized over "/api/users/{userId}"
/// </summary>
public class EndpointComparer : IComparer<Endpoint>
{
    public int Compare(Endpoint? x, Endpoint? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        var xSegments = x.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var ySegments = y.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        int segmentCount = Math.Min(xSegments.Length, ySegments.Length);

        for (int i = 0; i < segmentCount; i++)
        {
            bool xIsParameter = xSegments[i].StartsWith("{", StringComparison.OrdinalIgnoreCase) && xSegments[i].EndsWith("}", StringComparison.OrdinalIgnoreCase);
            bool yIsParameter = ySegments[i].StartsWith("{", StringComparison.OrdinalIgnoreCase) && ySegments[i].EndsWith("}", StringComparison.OrdinalIgnoreCase);

            //x is parameter, y is static
            if (xIsParameter && !yIsParameter)
            {
                return 1;
            }
            //x is static, y is parameter
            if (!xIsParameter && yIsParameter)
            {
                return -1;
            }

            //both are identical so continue to next segment
        }

        //both parts are identical but one may be deeper. Deeper is more specific so prioritize it
        return xSegments.Length.CompareTo(ySegments.Length);
    }
}
