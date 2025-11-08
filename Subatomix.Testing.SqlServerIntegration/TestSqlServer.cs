// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Net;
using System.Runtime.InteropServices;

namespace Subatomix.Testing.SqlServerIntegration;

/// <summary>
///   Represents a SQL Server instance for use in integration tests.
/// </summary>
public static class TestSqlServer
{
    internal const string
        PasswordName = "MSSQL_SA_PASSWORD",
        ServerPipe   = @"\\.\pipe\sql\query";

    internal const ushort
        ServerPort = 1433;

    private static SqlServerContainer? _container;
    private static MasterDatabase?     _masterDatabase;
    private static NetworkCredential?  _netCredential;
    private static SqlCredential?      _sqlCredential;

    private static readonly ConcurrentDictionary<string, TemporaryDatabase>
        TemporaryDatabasesInternal = new();

    /// <summary>
    ///   Gets whether the test SQL Server instance is ready for use.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   The test SQL Server instance is not ready for use.  Invoke
    ///   <see cref="SetUp"/> before using this method.
    /// </exception>
    [MemberNotNullWhen(true, nameof(_masterDatabase))]
    public static bool IsReady
        => _masterDatabase is not null;

    /// <summary>
    ///   Gets whether the test SQL Server instance is running in an ephemeral
    ///   container.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   The test SQL Server instance is not ready for use.  Invoke
    ///   <see cref="SetUp"/> before using this method.
    /// </exception>
    public static bool IsEphemeralContainer
    {
        get { RequireReady(); return _container is not null; }
    }

    /// <summary>
    ///   Gets an object representing the <c>master</c> database on the server.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   The test SQL Server instance is not ready for use.  Invoke
    ///   <see cref="SetUp"/> before using this method.
    /// </exception>
    public static MasterDatabase MasterDatabase
    {
        get { RequireReady(); return _masterDatabase; }
    }

    /// <summary>
    ///   Gets an administrator credential for the server, or
    ///   <see langword="null"/> if admin connections to the server do not
    ///   require a separate credential.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   The test SQL Server instance is not ready for use.  Invoke
    ///   <see cref="SetUp"/> before using this method.
    /// </exception>
    public static NetworkCredential? Credential
    {
        get { RequireReady(); return _netCredential; }
    }

    /// <summary>
    ///   Gets an administrator credential for the server, or
    ///   <see langword="null"/> if admin connections to the server do not
    ///   require a separate credential.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   The test SQL Server instance is not ready for use.  Invoke
    ///   <see cref="SetUp"/> before using this method.
    /// </exception>
    public static SqlCredential? SqlCredential
    {
        get { RequireReady(); return _sqlCredential; }
    }

    /// <summary>
    ///   Gets a read-only dictionary of objects representing temporary
    ///   databases on the server.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   The test SQL Server instance is not ready for use.  Invoke
    ///   <see cref="SetUp"/> before using this method.
    /// </exception>
    public static IReadOnlyDictionary<string, TemporaryDatabase> TemporaryDatabases
    {
        get { RequireReady(); return TemporaryDatabasesInternal; }
    }

    /// <summary>
    ///   Sets up access to the test SQL Server instance.
    /// </summary>
    /// <param name="requireTcp">
    ///   <para>
    ///     Whether to require the use of TCP transport when detecting an
    ///     existing local SQL Server default instance.
    ///   </para>
    ///   <para>
    ///     This parameter has no effect if the environment variable
    ///     <c>MSSQL_SA_PASSWORD</c> is defined.
    ///   </para>
    /// </param>
    /// <remarks>
    ///   <para>
    ///     Invoke this method from a test suite's one-time setup method,
    ///     before using other members of this type.
    ///   </para>
    ///   <para>
    ///     ⚠ This method is not thread-safe.  Do not invoke this method in
    ///     parallel with other members of this type.
    ///   </para>
    ///   <para>
    ///     This method selects one of the following three scenarios:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>
    ///       <term>Existing Server via Password</term>
    ///       <description>
    ///         If the environment variable <c>MSSQL_SA_PASSWORD</c> is
    ///         defined, this method assumes that a local SQL Server default
    ///         instance is running and that the current process can
    ///         authenticate as the system administrator (<c>sa</c>) using the
    ///         given password.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>Existing Server via Integrated Authentication</term>
    ///       <description>
    ///         Else, this method attempts to detect a local SQL Server default
    ///         instance, assuming that the instance supports integrated
    ///         authentication and that the current user has sufficient
    ///         privileges to run tests.  If the <paramref name="requireTcp"/>
    ///         argument is <see langword="true"/>, the detection checks only
    ///         whether a process is listening on TCP port 1433.  Otherwise,
    ///         the detection uses all supported transports.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>Ephemeral Server</term>
    ///       <description>
    ///         Else, this method assumes that a <c>docker</c> command exists
    ///         and runs Linux containers.  This method uses the command to
    ///         start an ephemeral Linux SQL Server container on TCP port 1433
    ///         with a random SA password.
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <exception cref="ExternalException">
    ///   An error occurred starting an ephemeral SQL Server container.
    /// </exception>
    public static void SetUp(bool requireTcp = false)
    {
        if (IsReady)
            return;

        var connectionString = new SqlConnectionStringBuilder { DataSource = "." };

        if (TryGetPasswordFromEnvironment(out var password))
        {
            // Scenario A: Environment variable MSSQL_SA_PASSWORD defined.
            // => Assume that a local SQL Server default instance is running.
            //    Use the given password to authenticate as SA.
            _netCredential = new("sa", password);
            _sqlCredential = _netCredential.ToSqlCredential();
        }
        else if (IsLocalSqlServerListening(requireTcp))
        {
            // Scenario B: Process listening on port 1433 or other transport.
            // => Assume that a local SQL Server default instance is running
            //    and supports integrated authentication.  Assume that the
            //    current user has sufficient privileges to run tests.
            connectionString.IntegratedSecurity = true;
        }
        else
        {
            // Scenario C: Nothing listening on port 1433 or other transport.
            // => Start an ephemeral SQL Server container on port 1433 using a
            //    generated SA password.
            _container     = new(ServerPort);
            _netCredential = _container.Credential;
            _sqlCredential = _netCredential.ToSqlCredential();
        }

        connectionString.Encrypt         = SqlConnectionEncryptOption.Optional;
        connectionString.ApplicationName = "Integration Tests";

        _masterDatabase = new(connectionString.ToString());
    }

    /// <summary>
    ///   Tears down access to the test SQL Server instance.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Invoke this method from a test suite's one-time teardown method,
    ///     when done using other members of this type.
    ///   </para>
    ///   <para>
    ///     ⚠ This method is not thread-safe.  Do not invoke this method in
    ///     parallel with other members of this type.
    ///   </para>
    ///   <para>
    ///     If the test SQL Server instance is an existing instance (not an
    ///     ephemeral instance started by <see cref="SetUp"/>), this method
    ///     attempts to delete all temporary databases created via
    ///     <see cref="CreateTemporaryDatabase"/> or
    ///     <see cref="CreateTemporaryDatabaseAsync"/> that have not yet been
    ///     deleted.
    ///   </para>
    /// </remarks>
    public static void TearDown()
    {
        if (!IsReady)
            return;

        var isPersistent = _container is null;

        foreach (var database in TemporaryDatabasesInternal.Values.ToArray())
            DisposeDatabaseBestEffort(database, remove: isPersistent);

        DisposeContainerBestEffort();

        _container      = null;
        _masterDatabase = null;
        _netCredential  = null;
        _sqlCredential  = null;

        TemporaryDatabasesInternal.Clear();
    }

    /// <summary>
    ///   Creates a temporary database.
    /// </summary>
    /// <param name="prefix">
    ///   The prefix to use to generate the name of the temporary database, or
    ///   <see langword="null"/> to use the default prefix <c>Temp</c>.
    /// </param>
    /// <returns>
    ///   An object representing the created temporary database.  The database
    ///   will be deleted when the object is disposed or when
    ///   <see cref="TearDown"/> is invoked.
    /// </returns>
    /// <remarks>
    ///   <para>
    ///     This method creates a database whose name conforms to the pattern:
    ///   </para>
    ///   <para>
    ///     <c>{prefix}_{yymmddhhmmssZ}_{random}</c>
    ///   </para>
    ///   <para>
    ///     Example:
    ///   </para>
    ///   <para>
    ///     <c>Temp_251231123456Z_fD9p3M</c>
    ///   </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   The test SQL Server instance is not ready for use.  Invoke
    ///   <see cref="SetUp"/> before using this method.
    /// </exception>
    /// <exception cref="DataException">
    ///   An error occurred creating the temporary database.
    /// </exception>
    public static TemporaryDatabase CreateTemporaryDatabase(string? prefix = null)
    {
        var database = CreateTemporaryDatabaseCore(prefix);

        try
        {
            database.Create();
            TemporaryDatabasesInternal.TryAdd(database.Name, database);
            ThrowForTestingIfRequested();
            return database;
        }
        catch
        {
            DisposeDatabaseBestEffort(database, remove: true);
            throw;
        }
    }

    /// <summary>
    ///   Creates a temporary database asynchronously.
    /// </summary>
    /// <param name="prefix">
    ///   The prefix to use to generate the name of the temporary database, or
    ///   <see langword="null"/> to use the default prefix <c>Temp</c>.
    /// </param>
    /// <param name="cancellation">
    ///   The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///   A task representing the asynchronous creation operation.  The task's
    ///   result is an object representing the created temporary database.  The
    ///   database will be deleted when the object is disposed or when
    ///   <see cref="TearDown"/> is invoked.
    /// </returns>
    /// <remarks>
    ///   <para>
    ///     This method creates a database whose name conforms to the pattern:
    ///   </para>
    ///   <para>
    ///     <c>{prefix}_{yymmddhhmmssZ}_{random}</c>
    ///   </para>
    ///   <para>
    ///     Example:
    ///   </para>
    ///   <para>
    ///     <c>Temp_251231123456Z_fD9p3M</c>
    ///   </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   The test SQL Server instance is not ready for use.  Invoke
    ///   <see cref="SetUp"/> before using this method.
    /// </exception>
    /// <exception cref="DataException">
    ///   An error occurred creating the temporary database.
    /// </exception>
    public static async Task<TemporaryDatabase> CreateTemporaryDatabaseAsync(
        string?           prefix       = null,
        CancellationToken cancellation = default)
    {
        var database = CreateTemporaryDatabaseCore(prefix);

        try
        {
            await database.CreateAsync(cancellation);
            TemporaryDatabasesInternal.TryAdd(database.Name, database);
            ThrowForTestingIfRequested();
            return database;
        }
        catch
        {
            DisposeDatabaseBestEffort(database, remove: true);
            throw;
        }
    }

    private static TemporaryDatabase CreateTemporaryDatabaseCore(string? prefix)
    {
        RequireReady();

        var name = RandomHelpers.GenerateDatabaseName(prefix);

        return new(name, _masterDatabase);
    }

    private static bool TryGetPasswordFromEnvironment([MaybeNullWhen(false)] out string password)
    {
        password = Environment.GetEnvironmentVariable(PasswordName);
        return !string.IsNullOrEmpty(password);
    }

    internal static void DisposeContainerBestEffort()
    {
        try
        {
            _container?.Dispose();
            ThrowForTestingIfRequested();
        }
        catch { } // best effort only
    }

    internal static void DisposeDatabaseBestEffort(TemporaryDatabase database, bool remove)
    {
        try
        {
            database.Dispose(remove);
            ThrowForTestingIfRequested();
        }
        catch { } // best effort only
    }

    internal static void OnTemporaryDatabaseDisposed(string name)
    {
        TemporaryDatabasesInternal.TryRemove(name, out _);
    }

    [ExcludeFromCodeCoverage] // Environment-dependent
    internal static bool IsLocalSqlServerListening(bool requireTcp = false)
    {
        return TcpPort.IsListening(ServerPort)
            || !requireTcp && CanConnectWithIntegratedAuthentication();
    }

    [ExcludeFromCodeCoverage] // Environment-dependent
    internal static bool CanConnectWithIntegratedAuthentication()
    {
        const string ConnectionString
            = "Data Source=.;Integrated Security=True;Connect Timeout=1;"
            + "Encrypt=Optional;Application Name=Integration Tests Server Detection";

        try
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open(SqlConnectionOverrides.OpenWithoutRetry);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [MemberNotNull(nameof(_masterDatabase))]
    private static void RequireReady()
    {
        if (IsReady)
            return;

        throw new InvalidOperationException(
            "TestSqlServer is not ready.  " +
            "Did you forget to invoke TestSqlServer.SetUp()?"
        );
    }

    private static bool _shouldThrowForTesting;

    internal static void ThrowForTestingAtNextOpportunity()
    {
        _shouldThrowForTesting = true;
    }

    private static void ThrowForTestingIfRequested()
    {
        try
        {
            if (_shouldThrowForTesting)
                throw new Exception("An exception was thrown for testing purposes.");
        }
        finally
        {
            _shouldThrowForTesting = false;
        }
    }
}
