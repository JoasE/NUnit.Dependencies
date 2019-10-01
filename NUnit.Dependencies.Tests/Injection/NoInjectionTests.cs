using NUnit.Framework;

namespace NUnit.Dependencies.Tests.Injection
{
    [Dependant]
    public class NoInjectionTests
    {
        [Dependant, Test]
        public void Works()
        {
        }
    }
}
