﻿namespace TaskRemoting
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RemoveInvokerTests
    {
        [TestMethod]
        public async Task Sample()
        {
            var testAssembly = Assembly.GetExecutingAssembly();
            var domainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(testAssembly.Location),
            };
            var remoteDomain = AppDomain.CreateDomain("TestRemoteDomain", null, domainSetup);
            var sampleRemote = (SampleRemoteClass)remoteDomain.CreateInstanceAndUnwrap(
                assemblyName: testAssembly.FullName,
                typeName: typeof(SampleRemoteClass).FullName);

            var invokerAssembly = typeof(RemoteInvoker).Assembly;
            var remoteInvoker = RemoteInvoker.Invoke(remoteDomain, sampleRemote.Add, 1, 2);

            var completionSource = new RemoteTaskCompletionSource<int>();
            remoteInvoker.Invoke(completionSource);
            int result = await completionSource.Task;
            Assert.AreEqual(3, result);
        }
    }
}
