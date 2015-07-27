namespace TaskRemoting
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public class RemoteInvoker: MarshalByRefObject
    {
        object Target { get; set; }
        MethodInfo Method { get; set; }
        object[] Arguments { get; set; }

        void Initialize(object target, MethodInfo method, object[] arguments)
        {
            this.Target = target;
            this.Method = method;
            this.Arguments = arguments;
        }

        public void Invoke<T>(ITaskCompletionSource<T> taskCompletionSource)
        {
            var task = (Task<T>)Method.Invoke(Target, Arguments);
            if (task == null) {
                var nullResultException = new InvalidOperationException(
                    nameof(RemoteInvoker) + " invoked " + Method.Name + ", but resulting task was null");
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

        public static RemoteInvoker Invoke<TArg1, TArg2, TResult>(
            [NotNull] AppDomain targetDomain,
            [NotNull] Func<TArg1, TArg2, Task<TResult>> methodCall,
            TArg1 arg1, TArg2 arg2)
        {
            if (methodCall == null)
                throw new ArgumentNullException(nameof(methodCall));
            if (targetDomain == null)
                throw new ArgumentNullException(nameof(targetDomain));

            var invoker = targetDomain.CreateInstanceAndUnwrap<RemoteInvoker>();
            invoker.Initialize(methodCall.Target, methodCall.Method, new object[] { arg1, arg2 });
            return invoker;
        }
    }
}
