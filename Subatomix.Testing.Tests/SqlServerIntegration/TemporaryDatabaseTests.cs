// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace Subatomix.Testing.SqlServerIntegration;

[TestFixture]
public class TemporaryDatabaseTests
{
    [Test]
    public void Construct_NullName()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new TemporaryDatabase(null!, new("Server=Bar"));
        });
    }

    [Test]
    public void Construct_NullMasterDatabase()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new TemporaryDatabase("Foo", null!);
        });
    }

    [Test]
    public void Name_Get()
    {
        var db = new TemporaryDatabase("Foo", new("Server=Bar"));

        db.Name.ShouldBe("Foo");

        db.Dispose(remove: false);
    }

    [Test]
    public void ConnectionString_Get()
    {
        var db = new TemporaryDatabase("Foo", new("Server=Bar"));

        db.ConnectionString.ShouldBe("Data Source=Bar;Initial Catalog=Foo");

        db.Dispose(remove: false);
    }

    [Test]
    public void Dispose()
    {
        var db = new TemporaryDatabase("Foo", new("Server=Bar"));

        db.Dispose(remove: false);
        db.Dispose(remove: false); // Test multiple disposal
    }

    [Test]
    public void Execute_Disposed()
    {
        var db = new TemporaryDatabase("Foo", new("Server=Bar"));

        db.Dispose(remove: false);

        Should.Throw<ObjectDisposedException>(() =>
        {
            db.Execute("SELECT 1;");
        });
    }

    [Test]
    public async Task ExecuteAsync_Disposed()
    {
        var db = new TemporaryDatabase("Foo", new("Server=Bar"));

        db.Dispose(remove: false);

        await Should.ThrowAsync<ObjectDisposedException>(async () =>
        {
            await db.ExecuteAsync("SELECT 1;");
        });
    }
}
