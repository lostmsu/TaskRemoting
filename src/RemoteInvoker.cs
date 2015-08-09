namespace TaskRemoting
{
    using System;
    using System.Reflection;
    using System.Security;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    sealed class RemoteInvoker: MarshalByRefObject
    {
        object target;
        MethodInfo method;
        object[] arguments;

        internal void Initialize(object target, MethodInfo method, object[] arguments)
        {
            this.target = target;
            this.method = method;
            this.arguments = arguments;
        }

        [SecurityCritical]
        internal void Invoke<T>([NotNull] RemoteTaskCompletionSource<T> taskCompletionSource)
        {
            var task = (Task<T>)method.Invoke(target, arguments);
            if (task == null) {
                var nullResultException = new InvalidOperationException(
                    nameof(RemoteInvoker) + " invoked " + method.Name + ", but resulting task was null");
                taskCompletionSource.TrySetException(nullResultException);
                return;
            }

            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    taskCompletionSource.TrySetException(t.Exception);
                else if (t.IsCanceled)
                    taskCompletionSource.TrySetCancelled();
                else
                    taskCompletionSource.TrySetResult(t.Result);
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        [SecurityCritical]
        internal void InvokeNoResult([NotNull] RemoteTaskCompletionSource<bool> taskCompletionSource)
        {
            var task = (Task)method.Invoke(target, arguments);
            if (task == null)
            {
                var nullResultException = new InvalidOperationException(
                    nameof(RemoteInvoker) + " invoked " + method.Name + ", but resulting task was null");
                taskCompletionSource.TrySetException(nullResultException);
                return;
            }

            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    taskCompletionSource.TrySetException(t.Exception);
                else if (t.IsCanceled)
                    taskCompletionSource.TrySetCancelled();
                else
                    taskCompletionSource.TrySetResult(true);
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
