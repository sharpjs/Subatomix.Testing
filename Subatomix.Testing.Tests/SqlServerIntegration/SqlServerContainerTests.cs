// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace Subatomix.Testing.SqlServerIntegration;

[TestFixture]
public class SqlServerContainerTests
{
    [Test]
    public void ValidateId_Invalid()
    {
        Should.Throw<ExternalException>(() =>
        {
            SqlServerContainer.ValidateId("");
        });
    }

    [Test]
    public void ValidateId_Valid()
    {
        Should.NotThrow(() =>
        {
            SqlServerContainer.ValidateId("0123456789abcdef");
        });
    }
}
