// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace Subatomix.Testing.SqlServerIntegration;

[TestFixture]
public class MasterDatabaseTests
{
    [Test]
    public void Construct_NullConnectionString()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new MasterDatabase(null!);
        });
    }

    [Test]
    public void Name_Get()
    {
        new MasterDatabase("Server=Bar")
            .Name.ShouldBe("master");
    }

    [Test]
    public void ConnectionString_Get()
    {
        new MasterDatabase("Server=Bar")
            .ConnectionString.ShouldBe("Server=Bar");
    }

    [Test]
    public void GetCreateDatabaseCommand_NullName()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            MasterDatabase.GetCreateDatabaseCommand(null!);
        });
    }

    [Test]
    public void GetCreateDatabaseCommand_Ok()
    {
        MasterDatabase.GetCreateDatabaseCommand("Foo'[3]").ShouldBe(
            """
            IF DB_ID(N'Foo''[3]') IS NULL EXEC(N'
                CREATE DATABASE [Foo''[3]]] COLLATE Latin1_General_100_CI_AI_SC_UTF8;
            ');
            """
        );
    }

    [Test]
    public void GetRemoveDatabaseCommand_NullName()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            MasterDatabase.GetRemoveDatabaseCommand(null!);
        });
    }

    [Test]
    public void GetRemoveDatabaseCommand_Ok()
    {
        MasterDatabase.GetRemoveDatabaseCommand("Foo'[3]").ShouldBe(
            """
            IF DB_ID(N'Foo''[3]') IS NOT NULL EXEC(N'
                ALTER DATABASE [Foo''[3]]] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP  DATABASE [Foo''[3]]];
            ');
            """
        );
    }
}
