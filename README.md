## About

[![Build](https://github.com/sharpjs/Subatomix.Testing/workflows/Build/badge.svg)](https://github.com/sharpjs/Subatomix.Testing/actions)
[![Build](https://img.shields.io/badge/coverage-100%25-brightgreen.svg)](https://github.com/sharpjs/Mixer/actions)
[![NuGet](https://img.shields.io/nuget/v/Subatomix.Testing.svg)](https://www.nuget.org/packages/Subatomix.Testing)
[![NuGet](https://img.shields.io/nuget/dt/Subatomix.Testing.svg)](https://www.nuget.org/packages/Subatomix.Testing)

My preferred frameworks for automated testing in .NET.

See [here](https://github.com/sharpjs/Subatomix.Testing/blob/main/Subatomix.Testing/Subatomix.Testing.csproj)
for a list of included packages and their versions.

This package also includes the
[`TestHarnessBase`](https://github.com/sharpjs/Subatomix.Testing/blob/main/Subatomix.Testing/TestHarnessBase.cs)
class, which aids my preferred technique for setup/teardown code.  Generally, I
eschew traditional `SetUp` and `TearDown` methods.  Instead, in each test, I
create an instance of a disposable context class.  Construction is setup, and
disposal is teardown.  Because `TestContext` means something else already in
NUnit, I call this pattern *Test Harness* instead.

```csharp
[Test]
public void TestSomething()
{
    using var h = new TestHarness();

    // rest of test
}

private class TestHarness : TestHarnessBase
{
    // properties for mocks and things

    public TestHarness()
    {
        // setup code
    }

    protected override CleanUp()
    {
        // teardown code
    }
}
```

This pattern enables some cool things:

- I can enable the C# 8
  [nullability checker](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references)
  and not have to sprinkle `?` and `!` all over the test code.

- Tests can run in parallel, regardless of test fixture lifetime, since there
  is no longer any shared state within a test class.

- Test-support code can be isolated away from the tests themselves.

If the
[test fixture lifetime](https://docs.nunit.org/articles/nunit/writing-tests/attributes/fixturelifecycle.html)
is instance-per-test-case, the test fixture itself can be a subclass of
`TestHarnessBase`.  This results in a test fixture more closely resembling a
traditional one and removes the need for a `using` statement in each test,
while retaining the improved nullability ergonomics.  However, directly
subclassing `TestHarnessBase` forfeits the isolation afforded by having a
separate test harness class.

```csharp
[TestFixture]
public class SomeTests : TestHarnessBase
{
    // properties for mocks and things

    public TestHarness()
    {
        // setup code
    }

    protected override CleanUp()
    {
        // teardown code
    }

    [Test]
    public void TestSomething()
    {
        // test
    }
}
```

<!--
  Copyright 2023 Subatomix Research Inc.
  SPDX-License-Identifier: ISC
-->
