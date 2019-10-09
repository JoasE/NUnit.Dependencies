using System;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace NUnit.Dependencies.Tests.Dependant
{
    public class ConfigurationInheritanceTests
    {
        [Test]
        public void InheritedClassAttribute_ExecutesTypesInSetup()
        {
            // Arrange
            var attribute = new DependantAttribute();
            var mock = new TypeInfoMock(typeof(Child));

            // Act
            var results = attribute.BuildFrom(mock);
            Assert.IsNotEmpty(results);
            foreach (var result in results)
            {
                // Assert
                Assert.AreEqual(RunState.Ignored, result.RunState);
            }
        }

        [Test]
        public void InheritedMethodAttribute_ExecutesTypesInSetup()
        {

            // Arrange

            // Create dynamic type
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");
            var typeBuilder = moduleBuilder.DefineType("MyNamespace.Base", TypeAttributes.Public | TypeAttributes.Abstract);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            var abstractMethod = typeBuilder.DefineMethod("Method", MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual);

            var con = typeof(DependantAttribute).GetConstructor(new Type[] { typeof(Type[]) });
            var builder = new CustomAttributeBuilder(con, new object[] { new Type[] { typeof(FaultyPingMock) } });
            abstractMethod.SetCustomAttribute(builder);

            // Create the type itself
            var baseType = typeBuilder.CreateType();

            // Create another dynamic type
            typeBuilder = moduleBuilder.DefineType("MyNamespace.Child", TypeAttributes.Public, baseType);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            var newMethod = typeBuilder.DefineMethod("Method", MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig);

            newMethod.SetCustomAttribute(builder);

            var il = newMethod.GetILGenerator();
            il.Emit(OpCodes.Ret);

            var newType = typeBuilder.CreateType();

            var attribute = new DependantAttribute();
            var mock = new MethodInfoMock(newType.GetMethods()[0]);

            // Act
            var results = attribute.BuildFrom(mock, new TestSuite("Test"));
            Assert.IsNotEmpty(results);
            foreach (var result in results)
            {
                // Assert
                Assert.AreEqual(RunState.Ignored, result.RunState);
            }
        }
    }

    [Dependant(typeof(FaultyPingMock))]
    public abstract class Base
    {
    }

    [Dependant()]
    public class Child : Base
    {
    }
}
