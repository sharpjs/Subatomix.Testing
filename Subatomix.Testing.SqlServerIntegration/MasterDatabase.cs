// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace Subatomix.Testing.SqlServerIntegration;

/// <summary>
///   Represents the <c>master</c> database on a SQL Server instance used for
///   integration tests.
/// </summary>
public sealed class MasterDatabase : TestSqlDatabase
{
    internal MasterDatabase(string connectionString)
        : base("master", connectionString) { }

    internal void CreateDatabase(string name)
        => Execute(GetCreateDatabaseCommand(name));

    internal Task CreateDatabaseAsync(string name, CancellationToken cancellation = default)
        => ExecuteAsync(GetCreateDatabaseCommand(name), cancellation);

    internal void RemoveDatabase(string name)
        => Execute(GetRemoveDatabaseCommand(name));

    internal Task RemoveDatabaseAsync(string name, CancellationToken cancellation = default)
        => ExecuteAsync(GetRemoveDatabaseCommand(name), cancellation);

    internal static string GetCreateDatabaseCommand(string name)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        name           = name.EscapeForSqlString();
        var nameQuoted = name.EscapeForSqlQuoted();

        return
        $"""
        IF DB_ID(N'{name}') IS NULL EXEC(N'
            CREATE DATABASE [{nameQuoted}] COLLATE Latin1_General_100_CI_AI_SC_UTF8;
        ');
        """;
    }

    internal static string GetRemoveDatabaseCommand(string name)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        name           = name.EscapeForSqlString();
        var nameQuoted = name.EscapeForSqlQuoted();

        return
        $"""
        IF DB_ID(N'{name}') IS NOT NULL EXEC(N'
            ALTER DATABASE [{nameQuoted}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP  DATABASE [{nameQuoted}];
        ');
        """;
    }
}
