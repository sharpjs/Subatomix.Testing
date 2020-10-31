using FluentAssertions;
using NUnit.Framework;

namespace Subatomix.Testing
{
    [TestFixture]
    public static class TestHarnessBaseTests
    {
        [Test]
        public static void Mocks_Get()
        {
            using var h = new TestHarness();

            h.Mocks         .Should().NotBeNull();
            h.Mocks.CallBase.Should().BeFalse();
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
        }

        private class TestHarness : TestHarnessBase { }
    }
}
