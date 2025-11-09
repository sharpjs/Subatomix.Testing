// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace Subatomix.Testing.SqlServerIntegration.Integration;

[TestFixture]
[NonParallelizable]
public class TestSqlServerIntegrationTests
{
    [SetUp]
    public void SetUp()
    {
        TestSqlServer.IsReady.ShouldBeFalse();

        Environment.GetEnvironmentVariable(TestSqlServer.PasswordName).ShouldBeNull();

        if (TestSqlServer.IsLocalSqlServerListening())
            Assert.Inconclusive("This test expects that no local SQL Server instance is running.");
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (TestSqlServer.IsReady)
                TestSqlServer.TearDown();

            Environment.SetEnvironmentVariable(TestSqlServer.PasswordName, null);
        }
        catch { } // best effort only
    }

    [Test]
    [NonParallelizable]
    public async Task SetUp_TearDown_Integrated()
    {
        using var container = new SqlServerContainer(TestSqlServer.ServerPort);

        // SetUp() succeeds even though the test SQL Server container does not
        // support integrated authentication.  That is because SetUp() first
        // checks whether something is listening on port 1433 on the loopback
        // IPv4 interface, and if so, just assumes that the listener is a SQL
        // Server supporting integrated authentication.

        TestSqlServer.SetUp();
        TestSqlServer.SetUp(); // Idempotence check
        TestSqlServer.IsReady                        .ShouldBeTrue();
        TestSqlServer.IsEphemeralContainer           .ShouldBeFalse();
        TestSqlServer.Credential                     .ShouldBeNull();
        TestSqlServer.MasterDatabase.ConnectionString.ShouldContain("Integrated Security=True");
        TestSqlServer.TemporaryDatabases             .ShouldBeEmpty();

        // Skip temporary database tests in this scenario, because the test
        // container does not actually support integrated authentication.

        TestSqlServer.TearDown();
        TestSqlServer.TearDown(); // Idempotence check
        await TestSqlServerShouldNotBeReady();
    }

    [Test]
    [NonParallelizable]
    public async Task SetUp_TearDown_Password()
    {
        using var container = new SqlServerContainer(TestSqlServer.ServerPort);

        Environment.SetEnvironmentVariable(TestSqlServer.PasswordName, container.Credential.Password);

        TestSqlServer.SetUp();
        TestSqlServer.IsReady                        .ShouldBeTrue();
        TestSqlServer.IsEphemeralContainer           .ShouldBeFalse();
        TestSqlServer.Credential                     .ShouldNotBeNull();
        TestSqlServer.Credential.UserName            .ShouldBe("sa");
        TestSqlServer.Credential.Password            .ShouldBe(container.Credential.Password);
        TestSqlServer.MasterDatabase.ConnectionString.ShouldNotContain("Integrated Security=True");
        TestSqlServer.TemporaryDatabases             .ShouldBeEmpty();

        await TestTemporaryDatabases();

        // Leak a temporary database to check if teardown removes it
        var masterDatabase   = TestSqlServer.MasterDatabase;
        var leakedDatabase   = TestSqlServer.CreateTemporaryDatabase();

        var nameInString     = leakedDatabase.Name.EscapeForSqlString();
        var connectionString = masterDatabase.ConnectionString;
        var credential       = TestSqlServer.SqlCredential;

        TestSqlServer.TearDown();
        await TestSqlServerShouldNotBeReady();

        // This server is persistent, so its master database still should be
        // usable to verify that the leaked temporary database was removed
        using var session = new SqlSession(connectionString, credential);

        await session.ExecuteAsync(
            $"""
            IF DB_ID(N'{nameInString}') IS NOT NULL
                THROW 50000, 'Temporary database was not removed.', 1;
            """
        );
    }

    [Test]
    [NonParallelizable]
    public async Task SetUp_TearDown_Container()
    {
        // Let TestSqlServer create its own ephemeral container

        TestSqlServer.SetUp();
        TestSqlServer.IsReady                        .ShouldBeTrue();
        TestSqlServer.IsEphemeralContainer           .ShouldBeTrue();
        TestSqlServer.Credential                     .ShouldNotBeNull();
        TestSqlServer.Credential.UserName            .ShouldBe("sa");
        TestSqlServer.Credential.Password            .ShouldNotBeNullOrEmpty();
        TestSqlServer.MasterDatabase.ConnectionString.ShouldNotContain("Integrated Security=True");
        TestSqlServer.TemporaryDatabases             .ShouldBeEmpty();

        await TestTemporaryDatabases();

        TestSqlServer.TearDown();
        await TestSqlServerShouldNotBeReady();
    }

    private async Task TestTemporaryDatabases()
    {
        TestCreateTemporaryDatabase(null,  @"^Temp_");
        TestCreateTemporaryDatabase("Foo", @"^Foo_" );
        TestCreateTemporaryDatabaseThrowing();
        await TestCreateTemporaryDatabaseAsync(null,  @"^Temp_");
        await TestCreateTemporaryDatabaseAsync("Foo", @"^Foo_" );
        await TestCreateTemporaryDatabaseAsyncThrowing();
        TestSqlSessionErrorNoProcedureName();
        TestSqlSessionErrorWithProcedureName();
        TestSqlSessionUnexpectedDispose();
    }

    private void TestCreateTemporaryDatabase(string? prefix, string prefixPattern)
    {
        string name;
        string nameInString;

        using (var db = TestSqlServer.CreateTemporaryDatabase(prefix))
        {
            db.Name            .ShouldMatch(prefixPattern + @"[0-9]{12}Z_[A-Za-z0-9]{6}$");
            db.ConnectionString.ShouldContain(db.Name);

            TestSqlServer.TemporaryDatabases[db.Name].ShouldBeSameAs(db);

            name         = db.Name;
            nameInString = name.EscapeForSqlString();

            db.Execute(
                $"""
                IF DB_NAME() != N'{nameInString}'
                    THROW 50000, 'Unexpected database name.', 1;
                """
            );
        }

        TestSqlServer.TemporaryDatabases.ContainsKey(name).ShouldBeFalse();

        TestSqlServer.MasterDatabase.Execute(
            $"""
            IF DB_ID(N'{nameInString}') IS NOT NULL
                THROW 50000, 'Temporary database was not removed.', 1;
            """
        );
    }

    private void TestCreateTemporaryDatabaseThrowing()
    {
        TestSqlServer.ThrowForTestingAtNextOpportunity();

        Should.Throw<Exception>(() =>
        {
            using (TestSqlServer.CreateTemporaryDatabase()) { }
        });
    }

    private async Task TestCreateTemporaryDatabaseAsync(string? prefix, string prefixPattern)
    {
        string name;
        string nameInString;

        await using (var db = await TestSqlServer.CreateTemporaryDatabaseAsync(prefix))
        {
            db.Name            .ShouldMatch(prefixPattern + @"[0-9]{12}Z_[A-Za-z0-9]{6}$");
            db.ConnectionString.ShouldContain(db.Name);

            TestSqlServer.TemporaryDatabases[db.Name].ShouldBeSameAs(db);

            name         = db.Name;
            nameInString = name.EscapeForSqlString();

            await db.ExecuteAsync(
                $"""
                IF DB_NAME() != N'{nameInString}'
                    THROW 50000, 'Unexpected database name.', 1;
                """
            );
        }

        TestSqlServer.TemporaryDatabases.ContainsKey(name).ShouldBeFalse();

        await TestSqlServer.MasterDatabase.ExecuteAsync(
            $"""
            IF DB_ID(N'{nameInString}') IS NOT NULL
                THROW 50000, 'Temporary database was not removed.', 1;
            """
        );
    }

    private async Task TestCreateTemporaryDatabaseAsyncThrowing()
    {
        TestSqlServer.ThrowForTestingAtNextOpportunity();

        await Should.ThrowAsync<Exception>(async () =>
        {
            await using (await TestSqlServer.CreateTemporaryDatabaseAsync()) { }
        });
    }

    private void TestSqlSessionErrorNoProcedureName()
    {
        using var session = new SqlSession(
            TestSqlServer.MasterDatabase.ConnectionString,
            TestSqlServer.SqlCredential
        );

        Should.Throw<DataException>(() =>
        {
            session.Execute("THROW 50000, 'Test error.', 1;");
        });
    }

    private void TestSqlSessionErrorWithProcedureName()
    {
        using var session = new SqlSession(
            TestSqlServer.MasterDatabase.ConnectionString,
            TestSqlServer.SqlCredential
        );

        Should.Throw<DataException>(() =>
        {
            session.Execute("CREATE PROCEDURE dbo.Foo AS ?");
        });
    }

    private void TestSqlSessionUnexpectedDispose()
    {
        using var session = new SqlSession(
            TestSqlServer.MasterDatabase.ConnectionString,
            TestSqlServer.SqlCredential
        );

        session.Execute("DECLARE @x int;");

        Should.Throw<DataException>(() =>
        {
            session.Connection.Dispose();
        });
    }

    private async Task TestSqlServerShouldNotBeReady()
    {
        TestSqlServer.IsReady.ShouldBeFalse();

        Should.Throw<InvalidOperationException>(() =>
        {
            var _ = TestSqlServer.MasterDatabase;
        });

        Should.Throw<InvalidOperationException>(() =>
        {
            var _ = TestSqlServer.TemporaryDatabases;
        });

        Should.Throw<InvalidOperationException>(() =>
        {
            TestSqlServer.CreateTemporaryDatabase();
        });

        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await TestSqlServer.CreateTemporaryDatabaseAsync();
        });
    }
}
