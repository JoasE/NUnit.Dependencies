using NUnit.Framework;

namespace NUnit.Dependencies.Tests.Injection
{
    [TestFixture]
    public class MethodScopeTests
    {
        public EmptyDependency Dependency { get; set; }

        [Dependant, Test]
        [Order(1)]
        public void Test1(EmptyDependency dependency1, EmptyDependency dependency2)
        {
            Dependency = dependency1;
            Assert.AreEqual(dependency1, dependency2);
        }

        [Dependant, Test(Description = "Tests whether a method with the same method name has a different scope")]
        [Order(2)]
        public void Test1(EmptyDependency dependency1) // Test same method name
        {
            Assert.NotNull(Dependency, "Test1 must run first.");
            Assert.AreNotEqual(Dependency, dependency1);
        }

        [Dependant, Test]
        [Order(3)]
        public void Test2(EmptyDependency dependency)
        {
            Assert.NotNull(Dependency, "Test1 must run first.");
            Assert.AreNotEqual(Dependency, dependency);
        }
    }
}
