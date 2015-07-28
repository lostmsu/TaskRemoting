namespace TaskRemoting
{
    using System;
    using System.Threading.Tasks;

    public class SampleRemoteClass: MarshalByRefObject
    {
        public Task<int> Add(int a, int b)
        {
            var completionSource = new TaskCompletionSource<int>();
            completionSource.SetResult(a + b);
            return completionSource.Task;
        }
    }
}
