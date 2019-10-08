using System;
using System.Linq;
using NUnit.Dependencies.Tests.PingPong.Async;
using NUnit.Dependencies.Tests.PingPong.Sync;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace NUnit.Dependencies.Tests.Dependant
{
    using DependantAttribute = DependantAttribute;
    public class ExecuteInSetupTests
    {
        [TestCase(typeof(object))]
        [TestCase(typeof(ExecuteInSetupTests))]
        public void InvalidTypeSpecified_CreatesInvalidTestFixture(params Type[] types)
        {
            // Arrange
            var attribute = new DependantAttribute(types);
            var mock = new TypeInfoMock(typeof(ExecuteInSetupTests));

            // Act
            var results = attribute.BuildFrom(mock);
            Assert.IsNotEmpty(results);
            foreach (var result in results)
            {
                // Assert
                Assert.AreEqual(RunState.NotRunnable, result.RunState);
            }
        }

        [TestCase(typeof(object))]
        [TestCase(typeof(ExecuteInSetupTests))]
        public void InvalidTypeSpecified_CreatesInvalidTestMethod(params Type[] types)
        {
            // Arrange
            var testType = new TypeInfoMock(typeof(ExecuteInSetupTests));
            var attribute = new DependantAttribute(types);
            var testMock = attribute.BuildFrom(testType).First();
            var mock = new MethodInfoMock(typeof(ExecuteInSetupTests).GetMethod(nameof(InvalidTypeSpecified_CreatesInvalidTestMethod)));

            // Act
            var results = attribute.BuildFrom(mock, testMock);
            Assert.IsNotEmpty(results);
            foreach (var result in results)
            {
                // Assert
                Assert.AreEqual(RunState.NotRunnable, result.RunState);
            }
        }

        [TestCase(typeof(FaultyPingMock), typeof(PongMock), RunState.Ignored)]
        [TestCase(typeof(Ping), typeof(Pong), RunState.Runnable)]
        [TestCase(typeof(AsyncPing), typeof(AsyncPong), RunState.Runnable)]
        [TestCase(typeof(FaultyPingMock), typeof(AsyncPong), RunState.Ignored)]
        [TestCase(typeof(Ping), typeof(AsyncPong), RunState.Runnable)]
        public void ValidType_GetsExecuted(Type executable, Type test, RunState runstate)
        {
            // Arrange
            var attribute = new DependantAttribute(executable);
            var mock = new TypeInfoMock(test);

            // Act
            var results = attribute.BuildFrom(mock);
            Assert.IsNotEmpty(results);
            foreach (var result in results)
            {
                // Assert
                Assert.AreEqual(runstate, result.RunState);
            }
        }

        [TestCase(typeof(PongMock), RunState.Ignored)]
        [TestCase(typeof(Pong), RunState.Runnable)]
        public void ExecuteInSetupTrue_ExecutesParameters(Type test, RunState runstate)
        {
            // Arrange
            var attribute = new DependantAttribute(true);
            var mock = new TypeInfoMock(test);

            // Act
            var results = attribute.BuildFrom(mock);
            Assert.IsNotEmpty(results);
            foreach (var result in results)
            {
                // Assert
                Assert.AreEqual(runstate, result.RunState);
            }
        }
    }

    [Dependency]
    public class FaultyPingMock : IDependencyFixture<object>
    {
        public object Result => null;

        public void Execute()
        {
            throw new IgnoreException("test");
        }
    }

    public class PongMock
    {
        public PongMock(FaultyPingMock faultyPingMock)
        {
            FaultyPingMock = faultyPingMock;
        }

        public FaultyPingMock FaultyPingMock { get; }
    }
}
