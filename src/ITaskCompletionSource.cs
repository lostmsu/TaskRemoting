namespace TaskRemoting
{
    using System;
    using System.Threading.Tasks;

    public interface ITaskCompletionSource<T>
    {
        bool TrySetCancelled();
        bool TrySetException(Exception exception);
        bool TrySetResult(T result);
        Task<T> Task { get; }
    }
}
