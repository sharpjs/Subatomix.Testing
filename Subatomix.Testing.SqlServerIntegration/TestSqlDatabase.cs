// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Data;

namespace Subatomix.Testing.SqlServerIntegration;

/// <summary>
///   Represents a SQL Server database for use in integration tests.
/// </summary>
public class TestSqlDatabase
{
    private protected TestSqlDatabase(string name, string connectionString)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));

        Name             = name;
        ConnectionString = connectionString;
    }

    /// <summary>
    ///   Gets the name of the database.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///   Gets the connection string for the database.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    ///   Executes the specified SQL batch against the database.
    /// </summary>
    /// <param name="sql">
    ///   The SQL batch to execute.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="sql"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="DataException">
    ///   An error occurred while executing the SQL batch.
    /// </exception>
    public virtual void Execute(string sql)
    {
        if (sql is null)
            throw new ArgumentNullException(nameof(sql));

        using var session = new SqlSession(ConnectionString, TestSqlServer.SqlCredential);

        session.Execute(sql);
    }

    /// <summary>
    ///   Executes the specified SQL batch against the database asynchronously.
    /// </summary>
    /// <param name="sql">
    ///   The SQL batch to execute.
    /// </param>
    /// <param name="cancellation">
    ///   A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///   A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="sql"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="DataException">
    ///   An error occurred while executing the SQL batch.
    /// </exception>
    public virtual async Task ExecuteAsync(string sql, CancellationToken cancellation = default)
    {
        if (sql is null)
            throw new ArgumentNullException(nameof(sql));

        await using var session = new SqlSession(ConnectionString, TestSqlServer.SqlCredential);

        await session.ExecuteAsync(sql, cancellation);
    }
}
