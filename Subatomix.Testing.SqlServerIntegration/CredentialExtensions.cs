// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Net;

namespace Subatomix.Testing.SqlServerIntegration;

internal static class CredentialExtensions
{
    public static SqlCredential ToSqlCredential(this NetworkCredential credential)
    {
        if (credential is null)
            throw new ArgumentNullException(nameof(credential));

        // NetworkCredential always returns a writable clone of its internal
        // password, but SqlCredential requires a read-only SecureString.
        var password = credential.SecurePassword;
        password.MakeReadOnly();

        return new(credential.UserName, password);
    }
}
