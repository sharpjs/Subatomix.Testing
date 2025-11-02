// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace Subatomix.Testing.SqlServerIntegration;

/// <summary>
///   Extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///   Escapes the string for inclusion in a T-SQL string literal.
    /// </summary>
    /// <param name="value">
    ///   The string to escape.
    /// </param>
    /// <returns>
    ///   The escaped string.
    /// </returns>
    /// <remarks>
    ///   This method replaces each single quote character with two single
    ///   quote characters.
    /// </remarks>
    public static string EscapeForSqlString(this string value)
        => value.Replace("'", "''");

    /// <summary>
    ///   Escapes the string for inclusion in a T-SQL delimited identifier.
    /// </summary>
    /// <param name="value">
    ///   The string to escape.
    /// </param>
    /// <returns>
    ///   The escaped string.
    /// </returns>
    /// <remarks>
    ///   This method replaces each right square bracket with two right square
    ///   brackets.
    /// </remarks>
    public static string EscapeForSqlQuoted(this string value)
        => value.Replace("]", "]]");
}
