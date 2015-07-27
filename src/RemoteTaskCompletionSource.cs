namespace TaskRemoting
{
    using System;
    using System.Threading.Tasks;

    public class RemoteTaskCompletionSource<T> : MarshalByRefObject, ITaskCompletionSource<T>
    {
        readonly TaskCompletionSource<T> localCompletionSource = new TaskCompletionSource<T>();

        public bool TrySetCancelled() => this.localCompletionSource.TrySetCanceled();

        public bool TrySetException(Exception exception) => this.localCompletionSource.TrySetException(exception);

        public bool TrySetResult(T result) => this.localCompletionSource.TrySetResult(result);

        public Task<T> Task => this.localCompletionSource.Task;
    }
}
