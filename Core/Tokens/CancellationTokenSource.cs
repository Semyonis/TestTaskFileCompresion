namespace Core.Tokens
{
    public sealed class CancellationTokenSource
    {
        public CancellationToken Token
        {
            get { return new CancellationToken(this); }
        }

        public bool IsCancellationRequested { get; private set; }

        public void Cancel() { IsCancellationRequested = true; }
    }
}