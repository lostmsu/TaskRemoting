namespace TaskRemoting
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Policy;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RemoteTaskTests
    {
        static readonly Assembly testAssembly = Assembly.GetExecutingAssembly();

        [TestMethod]
        public async Task Sample()
        {
            var domainSetup = new AppDomainSetup
            {
                ApplicationBase = Environment.CurrentDirectory,
            };
            var remoteDomain = AppDomain.CreateDomain("TestRemoteDomain", null, domainSetup);
            await TestRemoteExecution(remoteDomain);
        }

        [TestMethod]
        public async Task PartialTrustSample()
        {
            var permissions = this.GetPartialTrustPermissions();
            var domainSetup = new AppDomainSetup
            {
                // TODO: SECURITY: must be provided from outside
                ApplicationBase = Environment.CurrentDirectory,
            };

            var remoteDomain = AppDomain.CreateDomain("PartialTrust", null, domainSetup, permissions
                    , fullTrustAssemblies: typeof(RemoteTask).Assembly.GetStrongName()
                );
            await TestRemoteExecution(remoteDomain);
        }

        PermissionSet GetPartialTrustPermissions()
        {
            var evidence = new Evidence();
            evidence.AddHostEvidence(new Zone(SecurityZone.Internet));
            return SecurityManager.GetStandardSandbox(evidence);
        }

        static async Task TestRemoteExecution(AppDomain remoteDomain)
        {
            var testAssembly = Assembly.GetExecutingAssembly();
            var sampleRemote = (SampleRemoteClass)remoteDomain.CreateInstanceAndUnwrap(
                assemblyName: testAssembly.FullName,
                typeName: typeof(SampleRemoteClass).FullName);

            int result = await remoteDomain.Invoke(sampleRemote.Add, 1, 2);
            Assert.AreEqual(3, result);
        }
    }
}
