namespace TaskRemoting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    sealed class RemoteInvoker: MarshalByRefObject
    {
        Delegate method;
        object[] arguments;

        [SecurityCritical]
        internal void Initialize(object target, MethodInfo method, object[] arguments)
        {
            this.arguments = arguments;

            var delegateType = GetDelegateType(method);
            var myPermissionSet = new PermissionSet(PermissionState.None);
            myPermissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
            myPermissionSet.Assert();
            try
            {
                this.method = method.CreateDelegate(delegateType, target);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        static Type GetDelegateType(MethodInfo method)
        {
            var signatureTypes = method.GetParameters().Select(param => param.ParameterType).ToList();
            signatureTypes.Add(method.ReturnType);

            return System.Linq.Expressions.Expression.GetDelegateType(signatureTypes.ToArray());
        }

        [SecurityCritical]
        internal void Invoke<T>([NotNull] RemoteTaskCompletionSource<T> taskCompletionSource)
        {
            var task = (Task<T>)method.DynamicInvoke(arguments);
            if (task == null) {
                var nullResultException = new InvalidOperationException(
                    nameof(RemoteInvoker) + " invoked , but resulting task was null");
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
            var task = (Task)method.DynamicInvoke(arguments);
            if (task == null)
            {
                var nullResultException = new InvalidOperationException(
                    nameof(RemoteInvoker) + " invoked , but resulting task was null");
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
