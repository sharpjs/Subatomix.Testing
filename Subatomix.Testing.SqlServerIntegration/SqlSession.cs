// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace Subatomix.Testing.SqlServerIntegration;

internal sealed class SqlSession : IDisposable, IAsyncDisposable
{
    private readonly SqlConnection _connection;
    private readonly SqlCommand    _command;

    private bool _hasErrors;

    public SqlSession(string connectionString, SqlCredential? credential)
    {
        _connection = new SqlConnection(connectionString, credential);
        _command    = _connection.CreateCommand();

        _connection.InfoMessage                     += HandleMessage;
        _connection.Disposed                        += HandleUnexpectedDisposal;
        _connection.FireInfoMessageEventOnUserErrors = true;
        _connection.RetryLogicProvider               = RetryLogicProvider;

        _command.CommandType        = CommandType.Text;
        _command.RetryLogicProvider = RetryLogicProvider;
    }

    private static SqlRetryLogicBaseProvider RetryLogicProvider { get; }
        = SqlConfigurableRetryFactory.CreateExponentialRetryProvider(new()
        {
            NumberOfTries   = 5,
            DeltaTime       = TimeSpan.FromSeconds(2),
            MaxTimeInterval = TimeSpan.FromMinutes(2),
        });

    public void Execute(string sql)
    {
        ClearErrors();

        _command.CommandText = sql;

        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        _command.ExecuteNonQuery();

        ThrowIfHasErrors();
    }

    public async Task ExecuteAsync(string sql, CancellationToken cancellation = default)
    {
        ClearErrors();

        _command.CommandText = sql;

        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync(cancellation);

        await _command.ExecuteNonQueryAsync(cancellation);

        ThrowIfHasErrors();
    }

    private void ClearErrors()
    {
        _hasErrors = false;
    }

    private void ThrowIfHasErrors()
    {
        if (_hasErrors)
            throw new DataException("An error occurred while executing the SQL batch.");
    }

    private void HandleMessage(object sender, SqlInfoMessageEventArgs e)
    {
        const int    MaxInformationalSeverity = 10;
        const string NonProcedureLocationName = "(batch)";

        foreach (SqlError error in e.Errors)
        {
            AssumeNotNull(error); // SqlClient code assumes error is never null

            if (error.Class <= MaxInformationalSeverity)
            {
                Console.WriteLine(error.Message);
            }
            else
            {
                _hasErrors = true;

                Console.WriteLine(
                    "{0}:{1}: E{2}:{3}: {4}",
                    error.Procedure is { Length: > 0 } p ? p : NonProcedureLocationName,
                    error.LineNumber,
                    error.Number,
                    error.Class,
                    error.Message
                );
            }
        }
    }

    private static void HandleUnexpectedDisposal(object? sender, EventArgs e)
    {
        throw new DataException(
            "The connection to the database server was closed unexpectedly."
        );
    }

    public void Dispose()
    {
        _connection.Disposed -= HandleUnexpectedDisposal;

        _command   .Dispose();
        _connection.Dispose();
    }

#if NETCOREAPP3_0_OR_GREATER
    public async ValueTask DisposeAsync()
    {
        _connection.Disposed -= HandleUnexpectedDisposal;

        await _command   .DisposeAsync();
        await _connection.DisposeAsync();
    }
#else
  #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async ValueTask DisposeAsync()
    {
        Dispose();
    }
  #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#endif

    [Conditional("DEBUG")]
    private static void AssumeNotNull([NotNull] object? obj)
    {
        if (obj is null)
            throw new InvalidOperationException("A not-null assumption was violated.");
    }
}
