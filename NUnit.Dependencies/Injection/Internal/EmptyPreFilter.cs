using System;
using System.Reflection;
using NUnit.Framework.Interfaces;

namespace NUnit.Dependencies.Injection.Internal
{
    internal class EmptyPreFilter : IPreFilter
    {
        public bool IsMatch(Type type)
        {
            return true;
        }

        public bool IsMatch(Type type, MethodInfo method)
        {
            return true;
        }
    }
}
