/*
    Copyright 2021 Jeffrey Sharp

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
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Subatomix.Testing
{
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

        /// <summary>
        ///   Gets the NUnit random-value generator.
        /// </summary>
        public Randomizer Random => TestContext.CurrentContext.Random;

        /// <summary>
        ///   Gets the mock repository.  All mocks constructed using this
        ///   mock repository will be verified when the test harness is
        ///   disposed.
        /// </summary>
        public MockRepository Mocks { get; }

        /// <summary>
        ///   Gets the cancellation token source.
        /// </summary>
        public CancellationTokenSource Cancellation { get; }

        /// <summary>
        ///   Finalizes the test harness instance.  This causes an unmanaged
        ///   disposal, in which the test harness disposes only the unmanaged
        ///   resources it owns, like temporary files.  The test harness does
        ///   not dispose managed resources (.NET objects) and does not verify
        ///   mocks.
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
            if (!managed)
            {
                CleanUp(managed: false);
                return;
            }

            try
            {
                // An error in verification should cause the test to fail, but
                // should not prevent resource cleanup.
                Verify();
            }
            finally
            {
                // Resource cleanup problems should be reported, but should not
                // mask an actual test error.
                try
                {
                    CleanUp(managed: true);
                }
                catch
                {
                    var status = TestContext.CurrentContext.Result.Outcome.Status;
                    if (status == TestStatus.Passed || status == TestStatus.Skipped)
                        throw;
                }
            }
        }

        /// <summary>
        ///   Verifies mocks created by the test harness.
        /// </summary>
        protected virtual void Verify()
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
        protected virtual void CleanUp(bool managed)
        {
            if (!managed)
                return;

            Cancellation.Dispose();
        }
    }
}
