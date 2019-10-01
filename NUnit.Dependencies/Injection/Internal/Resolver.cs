using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace NUnit.Dependencies.Injection.Internal
{
    using NUnit.Dependencies;
    using NUnit.Dependencies.AssemblyInfo.Extensions.Internal;
    using NUnit.Engine;
    using NUnit.Engine.Extensibility;
    using NUnit.Framework.Interfaces;
    using System.Collections.Concurrent;
    using System.Xml;

    [Extension]
    internal class Resolver : ITestEventListener
    {
        private static IServiceProvider ServiceProvider { get; set; }

        private static readonly ConcurrentDictionary<string, FixtureScope> FixtureScopes = new ConcurrentDictionary<string, FixtureScope>();

        private static Assembly Assembly { get; set; }

        private static readonly object _lock = new object();

        /// <summary>
        /// Gets a service for the specified fixture's scope
        /// </summary>
        /// <param name="fixtureType">The type of the fixture which is requesting a service</param>
        /// <param name="serviceType">The type of the service the fixture is requesting</param>
        /// <returns>The requested service instance or <see langword="null"/> if it was not found</returns>
        public static object GetService(ITypeInfo fixtureType, Type serviceType)
        {
            var serviceProvider = GetServiceProvider(fixtureType.Assembly);

            var scope = FixtureScopes.GetOrAdd(fixtureType.Type.AssemblyQualifiedName, (_) => new FixtureScope(serviceProvider.CreateScope()));

            return scope.GetService(serviceType);
        }

        /// <summary>
        /// Gets a service for the specified test's scope
        /// </summary>
        /// <param name="testMethod">The method info of the test method which is requesting a service</param>
        /// <param name="serviceType">The type of the service the method is requesting</param>
        /// <returns>The requested service instance or <see langword="null"/> if it was not found</returns>
        public static object GetService(IMethodInfo testMethod, Type serviceType)
        {
            var serviceProvider = GetServiceProvider(testMethod.TypeInfo.Assembly);

            var scope = FixtureScopes.GetOrAdd(testMethod.TypeInfo.Type.AssemblyQualifiedName, (_) => new FixtureScope(serviceProvider.CreateScope()));

            return scope.GetServiceFor(testMethod.GetNunitName(), serviceType);
        }

        private static IServiceProvider GetServiceProvider(Assembly assembly)
        {
            if (ServiceProvider == null)
            {
                lock (_lock)
                {
                    if (ServiceProvider == null)
                    {
                        Assembly = assembly;
                        var dependencyAttributeType = typeof(DependencyAttribute);

                        // Find any dependency implementation types
                        var dependencies = assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && Attribute.IsDefined(x, dependencyAttributeType)).Select(x => new { Attribute = x.GetCustomAttribute<DependencyAttribute>(), Type = x });

                        // Setup the service provider
                        var serviceCollection = new ServiceCollection();
                        foreach (var dependency in dependencies)
                        {
                            switch (dependency.Attribute.Lifetime)
                            {
                                case ServiceLifetime.Scoped:
                                    serviceCollection.AddScoped(dependency.Type);
                                    break;
                                case ServiceLifetime.Singleton:
                                    serviceCollection.AddSingleton(dependency.Type);
                                    break;
                                case ServiceLifetime.Transient:
                                    serviceCollection.AddTransient(dependency.Type);
                                    break;
                            }
                            
                        }

                        ServiceProvider = serviceCollection.BuildServiceProvider();
                    }
                }
            }

            return ServiceProvider;
        }

        /// <summary>
        /// NUnit event handler
        /// </summary>
        /// <param name="report">The XML event that occured</param>
        public void OnTestEvent(string report)
        {
            // Parse the event.
            var xml = new XmlDocument();
            xml.LoadXml(report);

            var node = (XmlElement)xml.FirstChild;

            // Check if the event is the end of a test fixture.
            if (node.Name == "test-suite" && node.GetAttribute("type") == "TestFixture")
            {
                // Dispose scope for test fixture
                var classType = Assembly.GetType(node.GetAttribute("fullname"));
                FixtureScopes.TryGetValue(classType.AssemblyQualifiedName, out var value);
                if (value != null)
                {
                    value.Dispose();
                }
            }
            // Check if the event is the end of a test case / method
            else if (node.Name == "test-case")
            {
                // Dispose scope for test case
                var classType = Assembly.GetType(node.GetAttribute("classname"));
                FixtureScopes.TryGetValue(classType.AssemblyQualifiedName, out var value);
                var method = classType.GetMethod(node.GetAttribute("name"));
                if (value != null)
                {
                    value.Dispose(method.GetNunitName());
                }
            }
        }

        private class FixtureScope
        {
            /// <summary>
            /// The child scopes of this scope
            /// </summary>
            private readonly ConcurrentDictionary<string, IServiceScope> TestScopes = new ConcurrentDictionary<string, IServiceScope>();

            /// <summary>
            /// The actual service scope this class is representing
            /// </summary>
            private readonly IServiceScope scope;

            public FixtureScope(IServiceScope scope)
            {
                this.scope = scope;
            }

            public IServiceProvider ServiceProvider => scope.ServiceProvider;

            /// <summary>
            /// Gets a service from this scope
            /// </summary>
            /// <param name="type">The type of the service which is requested</param>
            /// <returns>The requested service instance or <see langword="null"/> if it was not found</returns>
            public object GetService(Type type)
            {
                return scope.ServiceProvider.GetService(type);
            }

            /// <summary>
            /// Gets a service for one of the child scopes
            /// </summary>
            /// <param name="key">The child scope key</param>
            /// <param name="serviceType">The type of the service which is requested</param>
            /// <returns>The requested service instance or <see langword="null"/> if it was not found</returns>
            public object GetServiceFor(string key, Type serviceType)
            {
                var newScope = TestScopes.GetOrAdd(key, (_) => scope.ServiceProvider.CreateScope());

                return newScope.ServiceProvider.GetService(serviceType);
            }

            public void Dispose()
            {
                scope.Dispose();
                foreach (var testScope in TestScopes.Values)
                {
                    testScope.Dispose();
                }
            }

            public void Dispose(string key)
            {
                TestScopes.TryGetValue(key, out var value);
                if (value != null)
                {
                    value.Dispose();
                }
            }
        }
    }
}
