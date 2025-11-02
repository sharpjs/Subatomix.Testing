// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;

namespace Subatomix.Testing.SqlServerIntegration;

/// <summary>
///   Represents a temporary SQL Server database for use in integration tests.
/// </summary>
public sealed class TemporaryDatabase : TestSqlDatabase, IDisposable, IAsyncDisposable
{
    // This class keeps its own reference to the master database to facilitate
    // disposal and finalization.  The reference enables a TemporaryDatabase
    // instance to delete its temporary database from a persistent server even
    // after TestSqlServer teardown.

    private readonly MasterDatabase _masterDatabase;
    private          bool           _isDisposed;

    internal TemporaryDatabase(string name, MasterDatabase masterDatabase)
        : base(name, GetConnectionString(name))
    {
        if (masterDatabase is null)
            throw new ArgumentNullException(nameof(masterDatabase));

        _masterDatabase = masterDatabase;
    }

    internal void Create()
        => _masterDatabase.CreateDatabase(Name);

    internal Task CreateAsync(CancellationToken cancellation = default)
        => _masterDatabase.CreateDatabaseAsync(Name, cancellation);

    internal void Remove()
        => _masterDatabase.RemoveDatabase(Name);

    internal Task RemoveAsync(CancellationToken cancellation = default)
        => _masterDatabase.RemoveDatabaseAsync(Name, cancellation);

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">
    ///   The object has been disposed.
    /// </exception>
    public override void Execute(string sql)
    {
        RequireNotDisposed();
        base.Execute(sql);
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">
    ///   The object has been disposed.
    /// </exception>
    public override Task ExecuteAsync(string sql, CancellationToken cancellation = default)
    {
        RequireNotDisposed();
        return base.ExecuteAsync(sql, cancellation);
    }

    private static string GetConnectionString(string name)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        return new SqlConnectionStringBuilder(TestSqlServer.MasterDatabase.ConnectionString)
        {
            InitialCatalog = name
        }
        .ToString();
    }

    /// <summary>
    ///   Invoked when the object is being finalized.
    /// </summary>
    [ExcludeFromCodeCoverage] // Nondeterministic
    ~TemporaryDatabase()
    {
        try { Dispose(remove: true); } catch { } // best effort only
    }

    /// <summary>
    ///   Disposes the temporary database.
    /// </summary>
    /// <remarks>
    ///   If the containing SQL Server instance is persistent, this method
    ///   deletes the database.
    /// </remarks>
    public void Dispose()
    {
        Dispose(remove: true);
    }

    internal void Dispose(bool remove)
    {
        if (remove)
            Remove();

        DisposeCore();
    }

    /// <summary>
    ///   Disposes the temporary database asynchronously.
    /// </summary>
    /// <returns>
    ///   A task that represents the asynchronous dispose operation.
    /// </returns>
    /// <remarks>
    ///   If the containing SQL Server instance is persistent, this method
    ///   deletes the database.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(remove: true);
    }

    internal async ValueTask DisposeAsync(bool remove)
    {
        if (remove)
            await RemoveAsync();

        DisposeCore();
    }

    private void DisposeCore()
    {
        _isDisposed = true;
        TestSqlServer.OnTemporaryDatabaseDisposed(Name);
        GC.SuppressFinalize(this);
    }

    private void RequireNotDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);
    }
}
