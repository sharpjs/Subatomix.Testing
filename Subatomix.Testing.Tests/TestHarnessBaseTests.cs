// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Testing;

[TestFixture]
public static class TestHarnessBaseTests
{
    [Test]
    public static void Mocks_Get()
    {
        using var h = new TestHarness();

        h.Mocks         .Should().NotBeNull();
        h.Mocks.CallBase.Should().BeFalse();

        // Verify mock repository uses strict behavior
        h.Mocks.Create<IDisposable>().Object
            .Invoking(x => x.Dispose())
            .Should().Throw<MockException>()
            .WithMessage("* invocation failed with mock behavior Strict.*");
    }

    [Test]
    public static void Random_Get()
    {
        using var h = new TestHarness();

        h.Random.Should().NotBeNull();
        h.Random.Should().BeSameAs(TestContext.CurrentContext.Random);
    }

    [Test]
    public static void Cancellation_Get()
    {
        using var h = new TestHarness();

        h.Cancellation                        .Should().NotBeNull();
        h.Cancellation.IsCancellationRequested.Should().BeFalse();
        h.Cancellation.Token.CanBeCanceled    .Should().BeTrue();
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
    }

    [Test]
    public static void Dispose_VerifyThrows()
    {
        var mock = new Mock<TestHarness>(MockBehavior.Strict);

        mock.Setup(h => h.Verify())
            .Throws(() => new ApplicationException("Boom!"));

        mock.Setup(h => h.CleanUp(true))
            .CallBase().Verifiable();

        mock.Object.Invoking(h => h.Dispose())
            .Should().Throw<ApplicationException>().WithMessage("Boom!");

        mock.VerifyAll();
    }

    [Test]
    public static void Dispose_CleanUpThrows()
    {
        var mock = new Mock<TestHarness>(MockBehavior.Strict);

        mock.Setup(h => h.Verify())
            .CallBase().Verifiable();

        mock.Setup(h => h.CleanUp(true))
            .Throws(() => new ApplicationException("Pow!"));

        mock.Object.Invoking(h => h.Dispose())
            .Should().Throw<ApplicationException>().WithMessage("Pow!");

        mock.VerifyAll();
    }

    [Test]
    public static void Dispose_VerifyThrows_CleanUpThrows()
    {
        var mock = new Mock<TestHarness>(MockBehavior.Strict);

        mock.Setup(h => h.Verify())
            .Throws(() => new ApplicationException("Boom!"));

        mock.Setup(h => h.CleanUp(true))
            .Throws(() => new ApplicationException("Pow!"));

        mock.Object.Invoking(h => h.Dispose())
            .Should().Throw<ApplicationException>().WithMessage("Boom!");

        mock.VerifyAll();

        // Prevent warning from marking test as inconclusive or skipped
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
    }

    [Test]
    public static void Verify()
    {
        using var h = new TestHarness();

        var obj = h.Mocks.Create<IDisposable>();

        obj.Setup(o => o.Dispose()).Verifiable();

        h.Invoking(h => h.Verify())
            .Should().ThrowExactly<MockException>()
            .WithMessage("* failed verification *");

        // To prevent same exception during harness dipsosal
        obj.Reset();
    }

    [Test]
    public static void CleanUp_Managed()
    {
        using var h = new TestHarness();

        h.CleanUp(managed: true);

        h.Cancellation.Invoking(c => c.Token)
            .Should().Throw<ObjectDisposedException>();
    }

    [Test]
    public static void CleanUp_Unmanaged()
    {
        using var h = new TestHarness();

        h.CleanUp(managed: false);

        h.Cancellation.Invoking(c => c.Token)
            .Should().NotThrow();
    }

    internal class TestHarness : TestHarnessBase { }
}
