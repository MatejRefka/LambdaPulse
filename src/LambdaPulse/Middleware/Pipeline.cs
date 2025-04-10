namespace LambdaPulse
{
    public class Pipeline
    {
        private List<Type> _middlewareTypes;

        public Pipeline()
        {
            _middlewareTypes = new List<Type>();
        }

        public Pipeline AddMiddleware<T>()
            where T : MiddlewareBase
        {
            _middlewareTypes.Add(typeof(T));
            return this; //allows method chaining
        }

        public Action<string> Build()
        {
            //default delegate returned if no middleware is added to the pipeline
            Action<string>? function = data => Console.WriteLine($"Default Endpoint (no middleware): {data}");

            //loop through the middleware in reverse order
            for (int i = _middlewareTypes.Count - 1; i >= 0; i--)
            {
                //create the middleware instance, passing in the previously built delegate
                if (Activator.CreateInstance(_middlewareTypes[i], function) is MiddlewareBase middlewareInstance)
                {
                    //points to the Invoke function of the middleware that's currently in context 
                    function = middlewareInstance.Invoke;
                }
                else
                {
                    throw new InvalidOperationException($"Failed to create instance of type {_middlewareTypes[i].Name}");
                }
            }
            return function;
        }
    }
}
