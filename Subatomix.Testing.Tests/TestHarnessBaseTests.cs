// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using NUnit.Framework.Interfaces;

namespace Subatomix.Testing;

[TestFixture]
public static class TestHarnessBaseTests
{
    [Test]
    public static void Mocks_Get()
    {
        using var h = new TestHarness();

        h.Mocks         .ShouldNotBeNull();
        h.Mocks.CallBase.ShouldBeFalse();

        // Verify mock repository uses strict behavior
        Should.Throw<MockException>(h.Mocks.Create<IDisposable>().Object.Dispose)
            .Message.ShouldContain(" invocation failed with mock behavior Strict.");
    }

    [Test]
    public static void Random_Get()
    {
        using var h = new TestHarness();

        h.Random.ShouldNotBeNull();
        h.Random.ShouldBeSameAs(TestContext.CurrentContext.Random);
    }

    [Test]
    public static void Cancellation_Get()
    {
        using var h = new TestHarness();

        h.Cancellation                        .ShouldNotBeNull();
        h.Cancellation.IsCancellationRequested.ShouldBeFalse();
        h.Cancellation.Token.CanBeCanceled    .ShouldBeTrue();
    }

    [Test]
    public static void Dispose()
    {
        var mock = new Mock<TestHarness>(MockBehavior.Strict);

        mock.Setup(h => h.Verify())
            .CallBase().Verifiable();

        mock.Setup(h => h.CleanUp(true))
            .CallBase().Verifiable();

        mock.Object.Dispose();

        mock.VerifyAll();

        PreventMoqFinalizerException(mock);
    }

    [Test]
    public static void Dispose_VerifyThrows()
    {
        var mock = new Mock<TestHarness>(MockBehavior.Strict);

        mock.Setup(h => h.Verify())
            .Throws(() => new ApplicationException("Boom!"));

        mock.Setup(h => h.CleanUp(true))
            .CallBase().Verifiable();

        Should.Throw<ApplicationException>(mock.Object.Dispose)
            .Message.ShouldBe("Boom!");

        mock.VerifyAll();

        PreventMoqFinalizerException(mock);
    }

    [Test]
    public static void Dispose_CleanUpThrows()
    {
        var mock = new Mock<TestHarness>(MockBehavior.Strict);

        mock.Setup(h => h.Verify())
            .CallBase().Verifiable();

        mock.Setup(h => h.CleanUp(true))
            .Throws(() => new ApplicationException("Pow!"));

        Should.Throw<ApplicationException>(mock.Object.Dispose)
            .Message.ShouldBe("Pow!");

        mock.VerifyAll();

        PreventMoqFinalizerException(mock);
    }

    [Test]
    public static void Dispose_VerifyThrows_CleanUpThrows()
    {
        var mock = new Mock<TestHarness>(MockBehavior.Strict);

        mock.Setup(h => h.Verify())
            .Throws(() => new ApplicationException("Boom!"));

        mock.Setup(h => h.CleanUp(true))
            .Throws(() => new ApplicationException("Pow!"));

        Should.Throw<ApplicationException>(mock.Object.Dispose)
            .Message.ShouldBe("Boom!");

        mock.VerifyAll();

        PreventMoqFinalizerException(mock);

        // The second exception (in CleanUp) is converted to a test warning
        TestContext.CurrentContext.Result.Assertions.ShouldContain(a
            => a.Status == AssertionStatus.Warning
        );

        // The warning was expected, so convert the test from warned to passed
        Assert.Pass();
    }

    [Test]
    public static void SimulatedFinalize()
    {
        var mock = new Mock<TestHarness>(MockBehavior.Strict);

        mock.Setup(h => h.CleanUp(false))
            .CallBase().Verifiable();

        mock.Object.SimulateFinalizer();

        mock.VerifyAll();

        PreventMoqFinalizerException(mock);
    }

    [Test]
    public static void Verify()
    {
        using var h = new TestHarness();

        var obj = h.Mocks.Create<IDisposable>();

        obj.Setup(o => o.Dispose()).Verifiable();

        Should.Throw<MockException>(h.Verify)
            .Message.ShouldContain(" failed verification ");

        // To prevent same exception during harness dipsosal
        obj.Reset();
    }

    [Test]
    public static void CleanUp_Managed()
    {
        using var h = new TestHarness();

        h.CleanUp(managed: true);

        Should.Throw<ObjectDisposedException>(() => _ = h.Cancellation.Token);
    }

    [Test]
    public static void CleanUp_Unmanaged()
    {
        using var h = new TestHarness();

        h.CleanUp(managed: false);

        Should.NotThrow(() => _ = h.Cancellation.Token);
    }

    internal class TestHarness : TestHarnessBase { }

    private static void PreventMoqFinalizerException(Mock<TestHarness> mock)
    {
        // Ensure that the mock is left in a state where its finalizer can run
        // successfully.  This prevents an intermittent test process crash.

        mock.Reset();
        mock.Setup(h => h.CleanUp(false)).CallBase();
    }
}
