// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace Subatomix.Testing.SqlServerIntegration;

[TestFixture]
public class TestSqlServerTests
{
    [Test]
    public void IsReady_WhenNotReady()
    {
        TestSqlServer.IsReady.ShouldBeFalse();
    }

    [Test]
    public void Credential_WhenNotReady()
    {
        Should.Throw<InvalidOperationException>(() =>
        {
            _ = TestSqlServer.Credential;
        });
    }

    [Test]
    public void MasterDatabase_WhenNotReady()
    {
        Should.Throw<InvalidOperationException>(() =>
        {
            _ = TestSqlServer.MasterDatabase;
        });
    }

    [Test]
    public void TemporaryDatabases_WhenNotReady()
    {
        Should.Throw<InvalidOperationException>(() =>
        {
            _ = TestSqlServer.TemporaryDatabases;
        });
    }

    [Test]
    public void CreateTemporaryDatabase_WhenNotReady()
    {
        Should.Throw<InvalidOperationException>(() =>
        {
            TestSqlServer.CreateTemporaryDatabase();
        });
    }

    [Test]
    public async Task CreateTemporaryDatabaseAsync_WhenNotReady()
    {
        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await TestSqlServer.CreateTemporaryDatabaseAsync();
        });
    }
}
