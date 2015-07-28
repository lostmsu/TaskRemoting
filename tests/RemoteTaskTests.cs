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
                ApplicationBase = Path.GetDirectoryName(testAssembly.Location),
            };
            var remoteDomain = AppDomain.CreateDomain("TestRemoteDomain", null, domainSetup);
            await RunSample(remoteDomain);
            AppDomain.Unload(remoteDomain);
        }

        [TestMethod]
        public async Task SandboxedSample()
        {
            var domainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(testAssembly.Location),
            };
            var evidence = new Evidence();
            evidence.AddHostEvidence(new Zone(SecurityZone.Internet));
            var permissions = SecurityManager.GetStandardSandbox(evidence);
            var remoteDomain = AppDomain.CreateDomain("TestSandboxDomain", null, domainSetup, permissions);
            await RunSample(remoteDomain);
            AppDomain.Unload(remoteDomain);
        }

        private static async Task RunSample(AppDomain remoteDomain)
        {
            var sampleRemote = (SampleRemoteClass)remoteDomain.CreateInstanceAndUnwrap(
                assemblyName: testAssembly.FullName,
                typeName: typeof(SampleRemoteClass).FullName);

            int result = await remoteDomain.Invoke(sampleRemote.Add, 1, 2);
            Assert.AreEqual(3, result);
        }
    }
}
