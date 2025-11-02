// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace Subatomix.Testing.SqlServerIntegration;

[TestFixture]
public class StringExtensionsTests
{
    [Test]
    [TestCase("ab",  "ab"  )]
    [TestCase("a'b", "a''b")]
    public void EscapeForSqlString(string input, string output)
        => input.EscapeForSqlString().ShouldBe(output);

    [Test]
    [TestCase("ab",  "ab"  )]
    [TestCase("a]b", "a]]b")]
    public void EscapeForSqlQuoted(string input, string output)
        => input.EscapeForSqlQuoted().ShouldBe(output);
}
