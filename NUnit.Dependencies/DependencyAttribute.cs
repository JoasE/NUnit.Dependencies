using System;
using Microsoft.Extensions.DependencyInjection;

namespace NUnit.Dependencies
{
    /// <summary>
    /// Specifies a class is intended for dependency injection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class DependencyAttribute : Attribute
    {
        public DependencyAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            Lifetime = lifetime;
        }

        public ServiceLifetime Lifetime { get; }
    }
}
