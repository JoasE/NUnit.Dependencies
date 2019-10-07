using NUnit.Dependencies.Injection.Internal;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnit.Dependencies
{
    /// <summary>
    /// Specifies the test fixture or method uses dependency injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DependantAttribute : NUnitAttribute, IFixtureBuilder2, ITestBuilder
    {
        private readonly string _containsInvalidTypes;

        public bool ExecuteInSetup { get; } = false;
        public Type[] ExecuteInSetupTypes { get; } = new Type[0];

        public DependantAttribute(bool executeInSetup = false)
        {
            ExecuteInSetup = executeInSetup;
        }

        public DependantAttribute(params Type[] executeInSetup)
        {
            ExecuteInSetupTypes = executeInSetup;

            var exacutableType = typeof(IExecuteable);
            foreach (var type in ExecuteInSetupTypes)
            {
                if (!exacutableType.IsAssignableFrom(type))
                {
                    _containsInvalidTypes = $"Invalid executeInSetup type specified: '{type.FullName}'. Type must inherit from: '{exacutableType.FullName}' in order to be executed.";
                    break;
                }
            }
        }

        protected bool ShouldExecute(Type dependecyFixture)
        {
            return ExecuteInSetup || ExecuteInSetupTypes.Contains(dependecyFixture);
        }

        #region IFixtureBuilder2

        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo, IPreFilter filter)
        {
            return GetTestSuite(typeInfo, filter);
        }

        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            return GetTestSuite(typeInfo, new EmptyPreFilter());
        }

        private IEnumerable<TestSuite> GetTestSuite(ITypeInfo typeInfo, IPreFilter filter)
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

                var fixture = new NUnitTestFixtureBuilder().BuildFrom(typeInfo, filter, new TestFixtureData(arguments.ToArray()));
                CheckApplyExecuteInSetup(fixture);

                yield return fixture;
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

            var testMethod = new NUnitTestCaseBuilder().BuildTestMethod(method, suite, new TestCaseParameters(arguments.ToArray()));

            CheckApplyExecuteInSetup(testMethod);

            yield return testMethod;
        }

        #endregion

        protected void CheckApplyExecuteInSetup(Test test)
        {
            if (_containsInvalidTypes != null)
            {
                test.MakeInvalid(_containsInvalidTypes);
                return;
            }

            foreach (var argument in test.Arguments)
            {
                if (ShouldExecute(argument.GetType())) {
                    try
                    {
                        ((IExecuteable)argument).Execute();
                    }
                    catch (IgnoreException e)
                    {
                        test.RunState = RunState.Ignored;
                        test.Properties.Add(PropertyNames.SkipReason, e.Message);
                    }
                }
            }
        }
    }
}
