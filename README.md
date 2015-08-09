# TaskRemoting

Package can be installed from NuGet: https://www.nuget.org/packages/TaskRemoting/ (TaskRemoting)

# Usage sample

```csharp
using TaskRemoting;

await remoteDomain.Invoke(myObj.MyMethod, myArg);
```

# Partial trust

RemoteTask.Invoke by itself demands unrestricted ReflectionPermission

If you want to call a method in a partial trust domain (sandbox), TaskRemoting must be fully trusted in that domain.

```csharp
public static StrongName GetStrongName(Assembly assembly)
{
    var assemblyInfo = assembly.GetName();
    var publicKey = new StrongNamePublicKeyBlob(assemblyInfo.GetPublicKey());
    return new StrongName(publicKey, assemblyInfo.Name, assemblyInfo.Version);
}

var taskRemotingStrongName = GetStrongName(typeof(RemoteTask).Assembly);
AppDomain.CreateDomain("Sandbox", null, domainSetup, permissions, fullTrustAssemblies: taskRemotingStrongName);
```