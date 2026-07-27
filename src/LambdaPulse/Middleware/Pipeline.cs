using LambdaPulse.Http.Abstractions;
using PulseInject;

namespace LambdaPulse.Middleware;

internal sealed class Pipeline
{
    private readonly DependencyResolver _dependencyResolver;
    private Func<WebContext, CancellationToken, Task> _func;
    private readonly List<Type> _middlewareTypes;

    public Pipeline(DependencyResolver dependencyResolver, Func<WebContext, CancellationToken, Task>? func = null)
    {
        _dependencyResolver = dependencyResolver;
        _middlewareTypes = [];

        //set the default delegate if no middleware is added to the pipeline
        _func = func ?? (async (webContext, cancellationToken) =>
        {
            Console.WriteLine($"Default Endpoint (no middleware).");
            await Task.CompletedTask;
        });
    }

    public Pipeline AddMiddleware<T>()
        where T : MiddlewareBase
    {
        _middlewareTypes.Add(typeof(T));
        return this; //allows method chaining
    }

    public Func<WebContext, CancellationToken, Task> Build()
    {
        //loop through the middleware in reverse order
        for (int i = _middlewareTypes.Count - 1; i >= 0; i--)
        {
            //construct the middleware instance with its parameters
            var middlewareType = _middlewareTypes[i];
            var constructor = middlewareType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First() ?? throw new InvalidOperationException($"No public constructors available for type {middlewareType.Name}.");
            var parameters = constructor.GetParameters();

            //array holding instantiated parameters
            var paramInstances = new object[parameters.Length];

            for (int j = 0; j < parameters.Length; j++)
            {
                var paramType = parameters[j].ParameterType;

                if (paramType == typeof(Func<WebContext, CancellationToken, Task>))
                {
                    //chain to next middleware
                    paramInstances[j] = _func;
                }
                else
                {
                    var instance = _dependencyResolver.GetService(paramType);
                    paramInstances[j] = instance ?? throw new InvalidOperationException($"Unable to resolve dependency: {paramType.Name}.");
                }
            }

            //create the middleware instance, passing in the previously built delegate
            if (Activator.CreateInstance(middlewareType, paramInstances) is MiddlewareBase middlewareInstance)
            {
                //points to the Invoke function of the middleware that's currently in context 
                _func = middlewareInstance.Invoke;
            }
            else
            {
                throw new InvalidOperationException($"Failed to create instance of type {_middlewareTypes[i].Name}.");
            }
        }
        return _func;
    }
}
