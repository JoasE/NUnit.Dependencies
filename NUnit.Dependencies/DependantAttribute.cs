using NUnit.Dependencies.Injection.Internal;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;

namespace NUnit.Dependencies
{
    /// <summary>
    /// Specifies the test fixture or method uses dependency injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DependantAttribute : NUnitAttribute, IFixtureBuilder2, ITestBuilder
    {
        #region IFixtureBuilder2

        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo, IPreFilter filter)
        {
            return GetTestSuite(typeInfo, filter);
        }

        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            return GetTestSuite(typeInfo);
        }

        private IEnumerable<TestSuite> GetTestSuite(ITypeInfo typeInfo, IPreFilter filter = null)
        {
            foreach (var constructor in typeInfo.Type.GetConstructors())
            {
                var arguments = new List<object>();
                foreach (var parameter in constructor.GetParameters())
                {
                    var argument = Resolver.GetService(constructor, parameter.ParameterType);
                    if (argument != null)
                    {
                        arguments.Add(argument);
                    }
                }
                
                yield return new NUnitTestFixtureBuilder().BuildFrom(typeInfo, filter, new TestFixtureData(arguments.ToArray()));
            }
        }

        #endregion

        #region ITestBuilder

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
        {
            var arguments = new List<object>();
            foreach (var parameter in method.MethodInfo.GetParameters())
            {
                var argument = Resolver.GetService(method, parameter.ParameterType);
                if (argument != null)
                {
                    arguments.Add(argument);
                }
            }

            yield return new NUnitTestCaseBuilder().BuildTestMethod(method, suite, new TestCaseParameters(arguments.ToArray()));
        }

        #endregion
    }
}
