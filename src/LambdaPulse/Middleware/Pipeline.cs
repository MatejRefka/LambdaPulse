using LambdaPulse.Services.Http.Models;

namespace LambdaPulse.Middleware;

public class Pipeline
{
    private Func<WebContext, CancellationToken, Task> _func;
    private readonly List<Type> _middlewareTypes;

    public Pipeline(Func<WebContext, CancellationToken, Task>? func = null)
    {
        _middlewareTypes = [];

        //set the default delegate if no middleware is added to the pipeline
        _func = func ?? (async (webContext, cancellationToken) =>
        {
            Console.WriteLine($"Default Endpoint (no middleware)");
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
            //create the middleware instance, passing in the previously built delegate
            if (Activator.CreateInstance(_middlewareTypes[i], _func) is MiddlewareBase middlewareInstance)
            {
                //points to the Invoke function of the middleware that's currently in context 
                _func = middlewareInstance.Invoke;
            }
            else
            {
                throw new InvalidOperationException($"Failed to create instance of type {_middlewareTypes[i].Name}");
            }
        }
        return _func;
    }
}
