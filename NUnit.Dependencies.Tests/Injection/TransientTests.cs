using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace NUnit.Dependencies.Tests.Injection
{
    [Dependency(ServiceLifetime.Transient)]
    public class TransientDependency
    {
    }

    [Dependant]
    public class TransientTests
    {
        public TransientTests(TransientDependency dependency, TransientDependency dependency2)
        {
            Dependency = dependency;
            Dependency2 = dependency2;
        }

        public TransientDependency Dependency { get; }
        public TransientDependency Dependency2 { get; }

        [Test]
        public void InjectsActionConstructor()
        {
            Assert.NotNull(Dependency);
            Assert.NotNull(Dependency2);
            Assert.AreNotEqual(Dependency, Dependency2);
        }

        [Dependant, Test]
        public void InjectsActionMethod(TransientDependency dependency)
        {
            Assert.NotNull(dependency);
            Assert.AreNotEqual(dependency, Dependency);
        }

        [Dependant, Test]
        public void InjectsActionMethod(TransientDependency dependency1, TransientDependency dependency2)
        {
            Assert.NotNull(dependency1);
            Assert.NotNull(dependency2);
            Assert.AreNotEqual(dependency1, dependency2);
        }
    }
}
