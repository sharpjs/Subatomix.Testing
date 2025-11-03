// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace Subatomix.Testing.SqlServerIntegration;

[TestFixture]
public class TestSqlDatabaseTests
{
    [Test]
    public void Construct_NullName()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new TestSqlDatabase(null!, "ConnectionString");
        });
    }

    [Test]
    public void Construct_NullConnectionString()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new TestSqlDatabase("Name", null!);
        });
    }

    [Test]
    public void Name_Get()
    {
        new TestSqlDatabase("Foo", "Server=Bar")
            .Name.ShouldBe("Foo");
    }

    [Test]
    public void ConnectionString_Get()
    {
        new TestSqlDatabase("Foo", "Server=Bar")
            .ConnectionString.ShouldBe("Server=Bar");
    }

    [Test]
    public void Execute_NullSql()
    {
        var db = new TestSqlDatabase("Foo", "Server=Bar");

        Should.Throw<ArgumentNullException>(() =>
        {
            db.Execute(null!);
        });
    }

    [Test]
    public async Task ExecuteAsync_NullSql()
    {
        var db = new TestSqlDatabase("Foo", "Server=Bar");

        await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await db.ExecuteAsync(null!);
        });
    }
}
