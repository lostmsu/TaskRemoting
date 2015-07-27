namespace TaskRemoting
{
    using System;
    static class RemoteDomainHelpers
    {
        public static T CreateInstanceAndUnwrap<T>(this AppDomain appDomain) where T : MarshalByRefObject
        {
            var assemblyName = typeof(T).Assembly.FullName;
            var typeName = typeof(T).FullName;
            return (T)appDomain.CreateInstanceAndUnwrap(assemblyName, typeName);
        }
    }
}
