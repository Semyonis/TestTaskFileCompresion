namespace Core.Tokens
{
    public sealed class CancellationToken
    {
        private readonly CancellationTokenSource source;

        private CancellationToken() { }

        public CancellationToken(CancellationTokenSource cancellationTokenSource) { source = cancellationTokenSource; }

        public bool IsCancellationRequested
        {
            get { return source != null && source.IsCancellationRequested; }
        }
    }
}