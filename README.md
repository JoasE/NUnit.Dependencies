# NUnit.Dependencies

## What is NUnit.Dependencies?

NUnit.Dependencies is an NUnit extension for writing dependant tests using dependency injection. This can be very useful when writing integration tests.
It helps prevent code duplication in your tests and simplifies finding a bug in your code which is causing multiple tests to fail.

## Usage

The NUnit.Dependencies library provides a recomended structure using `DependencyFixture`'s which can help you reduce your code duplication and provide easy ways of receiving the result of another test, to use it in yours.
If you whish to use the dependency injection separately, or inject dependencies into your test methods, you can use the `DependantAttribute` and the `DependencyAttribute`.

### Using the DependencyFixture

You can inherit from the `DependencyFixture` class to create tests for a feature which test atleast the happy path and which can be dependency injected into another test, receiving it's result.
If you require your test to be async, you can use the `AsyncDependencyFixture`.

```csharp
public class Ping : DependencyFixture<string>
{
    protected override string TestHappyPath()
    {
        // Arrange
        var ping = "ping";
        
        // Act
        Console.WriteLine(ping);
        
        // Assert
        Assert.AreEqual("ping", ping);

        // Return
        return Param;
    }
}
```

Now that you have created a `DependencyFixture`, you can use it in another test.

```csharp
public class Pong : DependencyFixture<string>
{
    private readonly Ping ping;

    public Pong(Ping ping)
    {
        this.ping = ping;
    }

    protected override string TestHappyPath()
    {
        // Arrange
        var pingResult = ping.Result;
        
        // Act
        var result = pingResult + "pong";
        Console.WriteLine(result);

        // Assert
        Assert.AreEqual("pingpong", result);

        // Return
        return result;
    }
}
```

### Using the `DependantAttribute` and the `DependencyAttribute`

The `DependantAttribute` and `DependencyAttribute` control which classses have access to, or can be, dependecy injected.

#### Using the `DependencyAttribute`

Classes with the `DependencyAttribute` are eligible for dependecy injection. You can specify a service lifetime by passing a `ServiceLifetime` to the attribute. The default lifetime is `ServiceLifetime.Scoped`

```chsarp
[Dependency] // ServiceLifetime.Scoped
public class AnotherDependecy
{
}

[Dependency(ServiceLifetime.Singleton)]
public class Singleton
{
}
```

#### Using the `DependantAttribute`

The `DependantAttribute` can be used to specify a test method or fixture is using dependency injection.

##### Dependant test fixtures

If you whish to allow a test fixture that does not inherit from DependencyFixture to inject dependecies into their consturctors, you must specify it as dependant.

```csharp
[Dependant]
public class CustomFixture
{
    public CustomFixture(AnotherDependecy f)
    {
    }
}
```

##### Dependant test methods

If you whish to allow test methods to inject dependecies into their parameters, you must specify the test method as dependant.

```csharp
public class Pong : DependencyFixture<string>
{
    private readonly Ping ping;

    public Pong(Ping ping)
    {
        this.ping = ping;
    }

    protected override string TestHappyPath()
    {
        // Arrange
        var pingResult = ping.Result;
        
        // Act
        var result = pingResult + "pong";
        Console.WriteLine(result);

        // Assert
        Assert.AreEqual("pingpong", result);

        // Return
        return result;
    }
    
    [Dependant, Test]
    public void OtherPath(AnotherDependecy f)
    {
    }
}
```

## Conflicting behaviour

The `DependantAttribute` has conflicting behaviour with the Parameterized Tests in NUnit. You can not use the NUnit.Dependencies dependency injection in the same test method or test fixture constructor.

## Installation

https://www.nuget.org/packages/NUnit.Dependencies/
