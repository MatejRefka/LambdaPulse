namespace LambdaPulse
{
    public abstract class MiddlewareBase
    {
        //delegate pointing to the next function in the pipeline chain
        protected Action<string> _nextFunction;

        public MiddlewareBase(Action<string> nextFunction)
        {
            _nextFunction = nextFunction;
        }

        //custom logic of the implementing middleware
        public abstract void Invoke(string data);
    }
}
