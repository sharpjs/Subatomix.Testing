// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Net;

namespace Subatomix.Testing.SqlServerIntegration;

[TestFixture]
public class CredentialExtensionsTests
{
    [Test]
    public void ToSqlCredential_NullCredential()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            (null as NetworkCredential)!.ToSqlCredential();
        });
    }

    [Test]
    public void ToSqlCredential_Ok()
    {
        var netCredential = new NetworkCredential("username", "password");
        netCredential.SecurePassword.IsReadOnly().ShouldBeFalse();

        var sqlCredential = netCredential.ToSqlCredential();

        sqlCredential.UserId               .ShouldBe(netCredential.UserName);
        sqlCredential.Password.IsReadOnly().ShouldBeTrue();
        GetCleartextPassword(sqlCredential).ShouldBe(netCredential.Password);
    }

    private static string GetCleartextPassword(SqlCredential sqlCredential)
    {
        return new NetworkCredential("", sqlCredential.Password).Password;
    }
}
