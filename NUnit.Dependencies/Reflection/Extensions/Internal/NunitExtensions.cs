using NUnit.Framework.Interfaces;
using System.Linq;
using System.Reflection;

namespace NUnit.Dependencies.Reflection.Extensions.Internal
{
    public static class NunitExtensions
    {
        public static string GetNunitName(this IMethodInfo info)
        {
            return info.MethodInfo.GetNunitName();
        }

        public static string GetNunitName(this MethodInfo info)
        {
            // This will not work with test cases
            var name = info.Name;
            var parameters = info.GetParameters();

            if (parameters.Length > 0)
            {
                name += "(" + string.Join(",", info.GetParameters().Select(x => x.ParameterType.FullName)) + ")";
            }

            return name;
        }

        public static string GetNunitName(this ConstructorInfo info)
        {
            // This will not work with test cases
            var name = info.DeclaringType.FullName;
            var parameters = info.GetParameters();

            if (parameters.Length > 0)
            {
                name += "(" + string.Join(",", info.GetParameters().Select(x => x.ParameterType.FullName)) + ")";
            }

            return name;
        }
    }
}