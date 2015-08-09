namespace TaskRemoting
{
    using System;
    using System.Threading.Tasks;

    class SampleRemoteClass : MarshalByRefObject
    {
        public Task<int> Add(int a, int b)
        {
            var completionSource = new TaskCompletionSource<int>();
            completionSource.SetResult(a + b);
            return completionSource.Task;
        }

        public Task NoResult()
        {
            return Task.FromResult(42);
        }
    }
}
