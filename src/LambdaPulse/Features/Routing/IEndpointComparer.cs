namespace LambdaPulse.Server.Features.Routing;

public interface IEndpointComparer : IComparer<Endpoint>
{
    public new int Compare(Endpoint x, Endpoint y);
}
