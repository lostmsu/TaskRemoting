namespace TaskRemoting
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public static class RemoteTask
    {
        public static Task<TResult> Invoke<TResult>(
            [NotNull] this AppDomain targetDomain,
            [NotNull] Delegate methodCall,
            params object[] arguments)
        {
            if (methodCall == null)
                throw new ArgumentNullException(nameof(methodCall));
            if (targetDomain == null)
                throw new ArgumentNullException(nameof(targetDomain));

            var invoker = targetDomain.CreateInstanceAndUnwrap<RemoteInvoker>();
            invoker.Initialize(methodCall.Target, methodCall.Method, arguments);
            var completionSource = new RemoteTaskCompletionSource<TResult>();
            invoker.Invoke(completionSource);
            // TODO: track domain's unloaded event
            return completionSource.Task;
        }

        public static Task<TResult> Invoke<TResult>(
            [NotNull] this AppDomain targetDomain,
            [NotNull] Func<Task<TResult>> methodCall)
        {
            return targetDomain.Invoke<TResult>((Delegate)methodCall);
        }

        public static Task<TResult> Invoke<TArg, TResult>(
            [NotNull] this AppDomain targetDomain,
            [NotNull] Func<TArg, Task<TResult>> methodCall,
            TArg arg)
        {
            return targetDomain.Invoke<TResult>(methodCall, arg);
        }

        public static Task<TResult> Invoke<TArg1, TArg2, TResult>(
            [NotNull] this AppDomain targetDomain,
            [NotNull] Func<TArg1, TArg2, Task<TResult>> methodCall,
            TArg1 arg1, TArg2 arg2)
        {
            return targetDomain.Invoke<TResult>(methodCall, arg1, arg2);
        }
    }
}
