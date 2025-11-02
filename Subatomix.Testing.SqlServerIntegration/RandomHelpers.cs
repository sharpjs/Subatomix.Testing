// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Security;

namespace Subatomix.Testing.SqlServerIntegration;

internal static class RandomHelpers
{
    private const string AlphanumericChars
        // 0         1         2         3         4         5         6
        // 012345678901234567890123456789012345678901234567890123456789012
        = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string GenerateDatabaseName(string? prefix)
    {
        prefix ??= "Temp";

        var random = CreateRandom();
        var c0     = random.Next(AlphanumericChars);
        var c1     = random.Next(AlphanumericChars);
        var c2     = random.Next(AlphanumericChars);
        var c3     = random.Next(AlphanumericChars);
        var c4     = random.Next(AlphanumericChars);
        var c5     = random.Next(AlphanumericChars);

#if NET6_0_OR_GREATER
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{prefix}_{DateTime.UtcNow:yyMMddHHmmss}Z_{c0}{c1}{c2}{c3}{c4}{c5}"
        );
#else
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}_{1:yyMMddHHmmss}Z_{2}{3}{4}{5}{6}{7}",
            prefix, DateTime.UtcNow, c0, c1, c2, c3, c4, c5
        );
#endif
    }

    public static SecureString GeneratePassword()
    {
        // This password is used for temporary databases storing test data.
        // Because the data is unimportant and ephemeral, it is acceptable to
        // use the simpler Random rather than a cryptographically secure RNG.

        const int Length = 24;

        var password = new SecureString();
        var random   = CreateRandom();

        for (var i = 0; i < Length; i++)
            password.AppendChar(random.Next(AlphanumericChars));

        password.MakeReadOnly();

        return password;
    }

#if NET6_0_OR_GREATER
    private static Random CreateRandom()
    {
        return new(Random.Shared.Next());
    }
#else
    private static int _seed;

    private static Random CreateRandom()
    {
        return new(unchecked(
            (int) DateTime.UtcNow.Ticks ^ Interlocked.Add(ref _seed, 0x5C9AB78D)
        ));
    }
#endif

    private static char Next(this Random random, string chars)
    {
        return chars[random.Next(chars.Length)];
    }
}
