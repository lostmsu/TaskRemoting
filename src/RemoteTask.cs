﻿namespace TaskRemoting
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    /// <summary>
    /// Wraps cross-domain asynchronous execution, based on <see cref="Task"/>s
    /// </summary>
    public static class RemoteTask
    {
        /// <summary>
        /// Performs specified invocation in target domain
        /// </summary>
        /// <typeparam name="TResult">Type of the resulting value</typeparam>
        /// <param name="targetDomain">Domain, to invoke in</param>
        /// <param name="method">Method to call</param>
        /// <param name="arguments">Arguments to pass to the method</param>
        public static Task<TResult> Invoke<TResult>(
            [NotNull] this AppDomain targetDomain,
            [NotNull] Delegate method,
            params object[] arguments)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (targetDomain == null)
                throw new ArgumentNullException(nameof(targetDomain));

            var invoker = targetDomain.CreateInstanceAndUnwrap<RemoteInvoker>();
            invoker.Initialize(method.Target, method.Method, arguments);
            var completionSource = new RemoteTaskCompletionSource<TResult>();
            invoker.Invoke(completionSource);
            // TODO: track domain's unloaded event
            return completionSource.Task;
        }

        /// <summary>
        /// Performs specified invocation in target domain
        /// </summary>
        /// <param name="targetDomain">Domain, to invoke in</param>
        /// <param name="method">Method to call</param>
        /// <param name="arguments">Arguments to pass to the method</param>
        public static Task Invoke([NotNull] this AppDomain targetDomain,
            [NotNull] Delegate method, params object[] arguments)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (targetDomain == null)
                throw new ArgumentNullException(nameof(targetDomain));

            var invoker = targetDomain.CreateInstanceAndUnwrap<RemoteInvoker>();
            invoker.Initialize(method.Target, method.Method, arguments);
            var completionSource = new RemoteTaskCompletionSource<bool>();
            invoker.InvokeNoResult(completionSource);
            // TODO: track domain's unloaded event
            return completionSource.Task;
        }

        #region 0-2 parameter wrappers for Task<T>
        /// <summary>
        /// Performs specified invocation in target domain
        /// </summary>
        /// <typeparam name="TResult">Type of the resulting value</typeparam>
        /// <param name="targetDomain">Domain, to invoke in</param>
        /// <param name="method">Method to call</param>
        public static Task<TResult> Invoke<TResult>(
            [NotNull] this AppDomain targetDomain,
            [NotNull] Func<Task<TResult>> method)
        {
            return targetDomain.Invoke<TResult>((Delegate)method);
        }

        /// <summary>
        /// Performs specified invocation in target domain
        /// </summary>
        /// <typeparam name="TArg">Type of the method parameter</typeparam>
        /// <typeparam name="TResult">Type of the resulting value</typeparam>
        /// <param name="targetDomain">Domain, to invoke in</param>
        /// <param name="method">Method to call</param>
        /// <param name="arg">Value of the parameter to pass</param>
        public static Task<TResult> Invoke<TArg, TResult>(
            [NotNull] this AppDomain targetDomain,
            [NotNull] Func<TArg, Task<TResult>> method,
            TArg arg)
        {
            return targetDomain.Invoke<TResult>(method, arg);
        }

        /// <summary>
        /// Performs specified invocation in target domain
        /// </summary>
        /// <typeparam name="TArg1">Type of the first parameter to the method</typeparam>
        /// <typeparam name="TArg2">Type of the second paramter to the method</typeparam>
        /// <typeparam name="TResult">Type of the resulting value</typeparam>
        /// <param name="targetDomain">Domain, to invoke in</param>
        /// <param name="method">Method to call</param>
        /// <param name="arg1">Value of the first parameter to pass</param>
        /// <param name="arg2">Value of the second parameter to pass</param>
        public static Task<TResult> Invoke<TArg1, TArg2, TResult>(
            [NotNull] this AppDomain targetDomain,
            [NotNull] Func<TArg1, TArg2, Task<TResult>> method,
            TArg1 arg1, TArg2 arg2)
        {
            return targetDomain.Invoke<TResult>(method, arg1, arg2);
        }
        #endregion

        #region 0-2 parameter wrappers for Task
        /// <summary>
        /// Performs specified invocation in target domain
        /// </summary>
        /// <param name="targetDomain">Domain, to invoke in</param>
        /// <param name="method">Method to call</param>
        public static Task Invoke([NotNull] this AppDomain targetDomain, [NotNull] Func<Task> method)
        {
            return targetDomain.Invoke((Delegate)method);
        }

        /// <summary>
        /// Performs specified invocation in target domain
        /// </summary>
        /// <typeparam name="TArg">Type of the method parameter</typeparam>
        /// <param name="targetDomain">Domain, to invoke in</param>
        /// <param name="method">Method to call</param>
        /// <param name="arg">Value of the parameter to pass</param>
        public static Task Invoke<TArg>(
            [NotNull] this AppDomain targetDomain,
            [NotNull] Func<TArg, Task> method,
            TArg arg)
        {
            return targetDomain.Invoke(method, arg);
        }

        /// <summary>
        /// Performs specified invocation in target domain
        /// </summary>
        /// <typeparam name="TArg1">Type of the first parameter to the method</typeparam>
        /// <typeparam name="TArg2">Type of the second paramter to the method</typeparam>
        /// <param name="targetDomain">Domain, to invoke in</param>
        /// <param name="method">Method to call</param>
        /// <param name="arg1">Value of the first parameter to pass</param>
        /// <param name="arg2">Value of the second parameter to pass</param>
        public static Task Invoke<TArg1, TArg2>(
            [NotNull] this AppDomain targetDomain,
            [NotNull] Func<TArg1, TArg2, Task> method,
            TArg1 arg1, TArg2 arg2)
        {
            return targetDomain.Invoke(method, arg1, arg2);
        }
        #endregion
    }
}
