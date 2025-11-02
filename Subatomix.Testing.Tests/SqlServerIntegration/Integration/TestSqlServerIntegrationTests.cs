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

        TestSqlServer.SetUp();
        TestSqlServer.IsReady                        .ShouldBeTrue();
        TestSqlServer.IsEphemeralContainer           .ShouldBeFalse();
        TestSqlServer.Credential                     .ShouldBeNull();
        TestSqlServer.MasterDatabase.ConnectionString.ShouldContain("Integrated Security=True");
        TestSqlServer.TemporaryDatabases             .ShouldBeEmpty();

        TestSqlServer.TearDown();
        await TestSqlServerShouldNotBeReady();
    }

    [Test]
    [NonParallelizable]
    public async Task SetUpTearDown_Password()
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
    public async Task SetUpTearDown_Container()
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
        await TestCreateTemporaryDatabaseAsync(null,  @"^Temp_");
        await TestCreateTemporaryDatabaseAsync("Foo", @"^Foo_" );
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
