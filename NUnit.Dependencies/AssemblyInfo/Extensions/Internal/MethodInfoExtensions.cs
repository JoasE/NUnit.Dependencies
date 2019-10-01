using NUnit.Framework.Interfaces;
using System.Linq;
using System.Reflection;

namespace NUnit.Dependencies.AssemblyInfo.Extensions.Internal
{
    public static class MethodInfoExtensions
    {
        public static string GetNunitName(this IMethodInfo info)
        {
            return info.MethodInfo.GetNunitName();
        }

        public static string GetNunitName(this MethodInfo info)
        {
            // This will not work with test cases
            return info.Name + "(" + string.Join(", ", info.GetParameters().Select(x => x.ParameterType.FullName)) + ")";
        }
    }
}