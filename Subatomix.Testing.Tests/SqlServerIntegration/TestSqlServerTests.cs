// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace Subatomix.Testing.SqlServerIntegration;

[TestFixture]
public class TestSqlServerTests
{
    [Test]
    public void IsReady_NotReady()
    {
        TestSqlServer.IsReady.ShouldBeFalse();
    }

    [Test]
    public void Credential_NotReady()
    {
        Should.Throw<InvalidOperationException>(() =>
        {
            _ = TestSqlServer.Credential;
        });
    }

    [Test]
    public void MasterDatabase_NotReady()
    {
        Should.Throw<InvalidOperationException>(() =>
        {
            _ = TestSqlServer.MasterDatabase;
        });
    }

    [Test]
    public void TemporaryDatabases_NotReady()
    {
        Should.Throw<InvalidOperationException>(() =>
        {
            _ = TestSqlServer.TemporaryDatabases;
        });
    }

    [Test]
    public void CreateTemporaryDatabase_NotReady()
    {
        Should.Throw<InvalidOperationException>(() =>
        {
            TestSqlServer.CreateTemporaryDatabase();
        });
    }

    [Test]
    public async Task CreateTemporaryDatabaseAsync_NotReady()
    {
        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await TestSqlServer.CreateTemporaryDatabaseAsync();
        });
    }

    [Test, NonParallelizable]
    public void DisposeContainerBestEffort_Throws()
    {
        TestSqlServer.ThrowForTestingAtNextOpportunity();
        TestSqlServer.DisposeContainerBestEffort();
    }

    [Test, NonParallelizable]
    public void DisposeDatabaseBestEffort_Throws()
    {
        TestSqlServer.ThrowForTestingAtNextOpportunity();
        TestSqlServer.DisposeDatabaseBestEffort(new("Foo", new("Server=Bar")), remove: false);
    }
}
