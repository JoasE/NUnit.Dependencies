using NUnit.Framework;

namespace NUnit.Dependencies.Tests.Injection
{
    [Dependant]
    public class InjectTestTests
    {
        public InjectTestTests(EmptyDependency dependency, EmptyDependency dependency2)
        {
            Dependency = dependency;
            Dependency2 = dependency2;
        }

        public EmptyDependency Dependency { get; }
        public EmptyDependency Dependency2 { get; }

        [Test]
        public void InjectsConstructor()
        {
            Assert.NotNull(Dependency);
            Assert.NotNull(Dependency2);
            Assert.AreEqual(Dependency, Dependency2);
        }

        [Dependant, Test]
        public void InjectsMethod(EmptyDependency dependency)
        {
            Assert.NotNull(dependency);
            Assert.AreNotEqual(dependency, Dependency);
        }
    }
}
