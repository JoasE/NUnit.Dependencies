using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace NUnit.Dependencies.Tests.Injection
{
    [Dependency(ServiceLifetime.Singleton)]
    public class SingletonDependency
    {
    }

    [Dependant]
    public class SingletonTests
    {
        public SingletonTests(SingletonDependency dependency1, SingletonDependency dependency2)
        {
            Dependency1 = dependency1;
            Dependency2 = dependency2;
        }

        public SingletonDependency Dependency1 { get; }
        public SingletonDependency Dependency2 { get; }

        [Test]
        public void InjectsConstructor()
        {
            Assert.NotNull(Dependency1);
            Assert.NotNull(Dependency2);
            Assert.AreEqual(Dependency1, Dependency2);
        }

        [Dependant, Test]
        public void InjectsMethod(SingletonDependency dependency1, SingletonDependency dependency2)
        {
            Assert.NotNull(dependency1);
            Assert.NotNull(dependency2);
            Assert.AreEqual(dependency1, dependency2);

            Assert.AreEqual(dependency1, Dependency1);
        }
    }
}
