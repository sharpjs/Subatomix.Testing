# Subatomix.Testing

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
- Tests can run in parallel, since there is no longer any shared state within a test class.
- Test-support code can be isolated away from the tests themselves.

<!--
  Copyright 2022 Jeffrey Sharp

  Permission to use, copy, modify, and distribute this software for any
  purpose with or without fee is hereby granted, provided that the above
  copyright notice and this permission notice appear in all copies.

  THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
  WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
  MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
  ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
  WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
  ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
  OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
-->
