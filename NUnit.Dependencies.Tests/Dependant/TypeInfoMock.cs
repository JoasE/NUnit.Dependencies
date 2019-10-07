using System;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework.Interfaces;

namespace NUnit.Dependencies.Tests.Dependant
{
    public class TypeInfoMock : ITypeInfo
    {
        private readonly Type type;

        public TypeInfoMock(Type type)
        {
            this.type = type;
        }

        public Type Type => type;

        public ITypeInfo BaseType => type.BaseType != null ? new TypeInfoMock(type.BaseType) : null;

        public string Name => type.Name;

        public string FullName => type.FullName;

        public Assembly Assembly => type.Assembly;

        public string Namespace => type.Namespace;

        public bool IsAbstract => type.IsAbstract;

        public bool IsGenericType => type.IsGenericType;

        public bool ContainsGenericParameters => type.ContainsGenericParameters;

        public bool IsGenericTypeDefinition => IsGenericTypeDefinition;

        public bool IsSealed => type.IsSealed;

        public bool IsStaticClass => type.IsAbstract && type.IsSealed;

        public object Construct(object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        public ConstructorInfo GetConstructor(Type[] argTypes)
        {
            return type.GetConstructor(argTypes);
        }

        public T[] GetCustomAttributes<T>(bool inherit) where T : class
        {
            return type.GetCustomAttributes(typeof(T)).Cast<T>().ToArray();
        }

        public string GetDisplayName()
        {
            return "TestDisplayName";
        }

        public string GetDisplayName(object[] args)
        {
            return "TestDisplayName";
        }

        public Type GetGenericTypeDefinition()
        {
            return type.GetGenericTypeDefinition();
        }

        public IMethodInfo[] GetMethods(BindingFlags flags)
        {
            return type.GetMethods(flags).Select(x => new MethodInfoMock(x)).ToArray();
        }

        public bool HasConstructor(Type[] argTypes)
        {
            return type.GetConstructor(argTypes) != null;
        }

        public bool HasMethodWithAttribute(Type attrType)
        {
            return type.GetMethods().Any(x => Attribute.IsDefined(x, attrType));
        }

        public bool IsDefined<T>(bool inherit) where T : class
        {
            return type.GetCustomAttributes(inherit).Any(x => typeof(T).IsAssignableFrom(x.GetType()));
        }

        public bool IsType(Type type)
        {
            return this.type == type;
        }

        public ITypeInfo MakeGenericType(Type[] typeArgs)
        {
            return new TypeInfoMock(type.MakeGenericType(typeArgs));
        }
    }

    public class MethodInfoMock : IMethodInfo
    {
        private readonly MethodInfo method;

        public MethodInfoMock(MethodInfo method)
        {
            this.method = method;
        }

        public ITypeInfo TypeInfo => new TypeInfoMock(method.DeclaringType);

        public MethodInfo MethodInfo => method;

        public string Name => method.Name;

        public bool IsAbstract => method.IsAbstract;

        public bool IsPublic => method.IsPublic;

        public bool ContainsGenericParameters => method.ContainsGenericParameters;

        public bool IsGenericMethod => method.IsGenericMethod;

        public bool IsGenericMethodDefinition => method.IsGenericMethodDefinition;

        public ITypeInfo ReturnType => new TypeInfoMock(method.ReturnType);

        public T[] GetCustomAttributes<T>(bool inherit) where T : class
        {
            return method.GetCustomAttributes().Where(x => x is T).Cast<T>().ToArray();
        }

        public Type[] GetGenericArguments()
        {
            return method.GetGenericArguments();
        }

        public IParameterInfo[] GetParameters()
        {
            return new IParameterInfo[0];
        }

        public object Invoke(object fixture, params object[] args)
        {
            return method.Invoke(fixture, args);
        }

        public bool IsDefined<T>(bool inherit) where T : class
        {
            return method.GetCustomAttributes(inherit).Any(x => typeof(T).IsAssignableFrom(x.GetType()));
        }

        public IMethodInfo MakeGenericMethod(params Type[] typeArguments)
        {
            return new MethodInfoMock(method.MakeGenericMethod());
        }
    }
}
