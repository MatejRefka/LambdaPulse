namespace LambdaPulse.Features.Routing;

public interface IEndpointComparer : IComparer<Endpoint>
{
    new int Compare(Endpoint x, Endpoint y);
}
