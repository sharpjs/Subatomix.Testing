// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Net;

namespace Subatomix.Testing.SqlServerIntegration;

[TestFixture]
public class RandomHelpersTests
{
    [Test]
    public void GenerateDatabaseName_DefaultPrefix()
    {
        var nameA = RandomHelpers.GenerateDatabaseName(null);
        var nameB = RandomHelpers.GenerateDatabaseName(null);

        nameA.ShouldMatch(@"^Temp_[0-9]{12}Z_[A-Za-z0-9]{6}$");
        nameB.ShouldMatch(@"^Temp_[0-9]{12}Z_[A-Za-z0-9]{6}$");
        nameA.ShouldNotBe(nameB);
    }

    [Test]
    public void GenerateDatabaseName_CustomPrefix()
    {
        var nameA = RandomHelpers.GenerateDatabaseName("Foo");
        var nameB = RandomHelpers.GenerateDatabaseName("Foo");

        nameA.ShouldMatch(@"^Foo_[0-9]{12}Z_[A-Za-z0-9]{6}$");
        nameB.ShouldMatch(@"^Foo_[0-9]{12}Z_[A-Za-z0-9]{6}$");
        nameA.ShouldNotBe(nameB);
    }

    [Test]
    public void GeneratePassword()
    {
        var password = RandomHelpers.GeneratePassword();

        password.IsReadOnly().ShouldBeTrue();

        new NetworkCredential("", password).Password
            .ShouldMatch(@"^[A-Za-z0-9]{24}$");
    }
}
