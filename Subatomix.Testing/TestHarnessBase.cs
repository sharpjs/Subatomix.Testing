// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics.CodeAnalysis;
using NUnit.Framework.Internal;

namespace Subatomix.Testing;

/// <summary>
///   A base class for disposable test harnesses using NUnit and Moq.
/// </summary>
public abstract class TestHarnessBase : IDisposable
{
    /// <summary>
    ///   Initializes a new <see cref="TestHarnessBase"/> instance.
    /// </summary>
    protected TestHarnessBase()
    {
        Mocks = new MockRepository(MockBehavior.Strict);

        Cancellation = new CancellationTokenSource();
    }

#if NET5_0_OR_GREATER
    // Squelch warning that Random could be made static: avoiding breaking change
    // TODO: Deprecate NUnit Randomizer in favor of Bogus library
    #pragma warning disable CA1822
#endif

    /// <summary>
    ///   Gets the NUnit random-value generator.
    /// </summary>
    public Randomizer Random => TestContext.CurrentContext.Random;

#if NET5_0_OR_GREATER
    #pragma warning restore CA1822
#endif

    /// <summary>
    ///   Gets the mock repository.  All mocks constructed using this mock
    ///   repository will be verified when the test harness is disposed.
    /// </summary>
    public MockRepository Mocks { get; }

    /// <summary>
    ///   Gets the cancellation token source.
    /// </summary>
    public CancellationTokenSource Cancellation { get; }

    /// <summary>
    ///   Finalizes the test harness instance.  This causes an unmanaged
    ///   disposal, in which the test harness disposes only the unmanaged
    ///   resources it owns, like temporary files.  The test harness does not
    ///   dispose managed resources (.NET objects) and does not verify mocks.
    /// </summary>
    [ExcludeFromCodeCoverage]
    ~TestHarnessBase()
    {
        Dispose(managed: false);
    }
    internal void SimulateFinalizer()
    {
        Dispose(managed: false);
    }

    /// <summary>
    ///   Performs a managed disposal of the test harness instance.  The
    ///   test harness verifies mocks and disposes all resources it owns.
    /// </summary>
    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Disposes the test harness instance.  In a managed disposal, the
    ///   test harness verifies mocks and disposes any managed resources
    ///   (.NET objects) it owns.  In both managed and unmanaged disposals,
    ///   the test harness disposes any unmanaged resources it owns, like
    ///   temporary files.
    /// </summary>
    /// <param name="managed">
    ///   <c>true</c>  to indicate a    managed disposal, or
    ///   <c>false</c> to indicate an unmanaged disposal.
    /// </param>
    protected void Dispose(bool managed)
    {
        bool throwing = false;

        try
        {
            if (managed) Verify();
        }
        catch
        {
            throwing = true;
            throw;
        }
        finally
        {
            try
            {
                // Verification exception should not prevent resource cleanup
                CleanUp(managed);
            }
            catch (Exception e) when (throwing)
            {
                // Cleanup exception should not mask verification failure
                Assert.Warn("Exception thrown during test harness cleanup: {0}", e);
            }
        }
    }

    /// <summary>
    ///   Verifies mocks created by the test harness.
    /// </summary>
    protected internal virtual void Verify()
    {
        Mocks.Verify();
    }

    /// <summary>
    ///   Cleans up resources after each test.
    /// </summary>
    /// <param name="managed">
    ///   <c>true</c> to clean up both managed and unmanaged resources;
    ///   <c>false</c> to clean up only unmanaged resources.
    /// </param>
    protected internal virtual void CleanUp(bool managed)
    {
        if (!managed)
            return;

        Cancellation.Dispose();
    }
}
