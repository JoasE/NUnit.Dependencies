using NUnit.Dependencies.Injection.Internal;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NUnit.Dependencies
{
    /// <summary>
    /// Specifies the test fixture or method uses dependency injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DependantAttribute : NUnitAttribute, IFixtureBuilder2, ITestBuilder
    {
        private readonly string _containsInvalidTypes;

        /// <summary>
        /// A value indicating whether the types injected into the constructor should be executed immediately, whether or not they are actually used. 
        /// </summary>
        public bool ExecuteInSetup { get; set; } = false;

        /// <summary>
        /// Any types of <see cref="IExecuteable"/>s which should be executed immediately, whether or not they are injected or actually used.
        /// Configurations of this property are inherited by any deriving test class with the <see cref="DependantAttribute"/> applied unless <see cref="Inherit"/> is set to <see langword="false"/>.
        /// </summary>
        public Type[] ExecuteInSetupTypes { get; private set; } = new Type[0];

        /// <summary>
        /// A value indicating whether the types specified in <see cref="ExecuteInSetupTypes"/> are inherited by any deriving test class with the <see cref="DependantAttribute"/> applied.
        /// </summary>
        public bool Inherit { get; set; } = true;

        /// <summary>
        /// Specifies the test fixture or method uses dependency injection.
        /// </summary>
        /// <param name="executeInSetup">A value indicating whether the types injected into the constructor should be executed immediately, whether or not they are actually used. </param>
        public DependantAttribute(bool executeInSetup = false)
        {
            ExecuteInSetup = executeInSetup;
        }

        /// <summary>
        /// Specifies the test fixture or method uses dependency injection.
        /// </summary>
        /// <param name="executeInSetup">Any types of <see cref="IExecuteable"/>s which should be executed immediately, whether or not they are injected or actually used. </param>
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
            InheritAttributes(typeInfo);

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
                CheckApplyExecuteInSetup(fixture, constructor);

                yield return fixture;
            }
        }

        #endregion

        #region ITestBuilder

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
        {
            InheritAttributes(method);

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

        private void InheritAttributes(IReflectionInfo test)
        {
            var otherAttributes = test.GetCustomAttributes<DependantAttribute>(true);

            var newSetupTypes = new List<Type>(this.ExecuteInSetupTypes);
            foreach (var otherAttribute in otherAttributes)
            {
                if (otherAttribute.Inherit)
                {
                    foreach (var otherSetupType in otherAttribute.ExecuteInSetupTypes)
                    {
                        if (!newSetupTypes.Contains(otherSetupType))
                        {
                            newSetupTypes.Add(otherSetupType);
                        }
                    }
                }
            }
            ExecuteInSetupTypes = newSetupTypes.ToArray();
        }

        private void CheckApplyExecuteInSetup(Test test, ConstructorInfo constructor = null)
        {
            // Check if there where any errors in the declaration of this attribute
            if (_containsInvalidTypes != null)
            {
                test.MakeInvalid(_containsInvalidTypes);
                return;
            }

            // Execute the injected arguments if specified
            if (this.ExecuteInSetup)
            {
                foreach (var argument in test.Arguments)
                {
                    ExecuteFor(argument, test);
                }
            }

            // Execute other specified types
            foreach (var executeType in this.ExecuteInSetupTypes)
            {
                // don't execute any arguments that have already been executed
                if (this.ExecuteInSetup)
                {
                    if (test.Arguments.Any(x => x.GetType() == executeType))
                    {
                        continue;
                    }
                }

                // Get the executable from the dependency injection
                object executable;
                if (constructor == null)
                {
                    executable = Resolver.GetService(test.Method, executeType);
                }
                else
                {
                    executable = Resolver.GetService(constructor, executeType);
                }
                
                // Execute it
                ExecuteFor(executable, test);
            }
        }

        private void ExecuteFor(object argument, Test test)
        {
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
