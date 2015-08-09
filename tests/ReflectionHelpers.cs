using System.Reflection;
using System.Security.Permissions;
using System.Security.Policy;

namespace TaskRemoting
{
    static class ReflectionHelpers
    {
        public static StrongName GetStrongName(this Assembly assembly)
        {
            var assemblyInfo = assembly.GetName();
            var publicKey = new StrongNamePublicKeyBlob(assemblyInfo.GetPublicKey());
            return new StrongName(publicKey, assemblyInfo.Name, assemblyInfo.Version);
        }
    }
}
