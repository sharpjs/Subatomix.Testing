// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Net;
using System.Runtime.InteropServices;

namespace Subatomix.Testing.SqlServerIntegration;

using static FormattableString;
using static RandomHelpers;

internal class SqlServerContainer : IDisposable
{
    private const long
        ReadyWaitTime =      TimeSpan.TicksPerMinute,
        EndedWaitTime = 15 * TimeSpan.TicksPerSecond;

    private const string
        Collation     = "Latin1_General_100_CI_AI_SC_UTF8",
        MemoryLimitMb = "2048";

    public SqlServerContainer(params ushort[] ports)
    {
        _ports     = ports;

        Credential = new("sa", GeneratePassword());
        Id         = Start();

        try
        {
            WaitForReady();
        }
        catch
        {
            try
            {
                Stop();
            }
            catch { } // Best effort only
            throw;
        }
    }

    public virtual void Dispose()
    {
        Stop();
    }

    public string Id { get; }

    public NetworkCredential Credential { get; }

    private readonly ushort[] _ports;

    private string Start()
    {
        new ExternalProgram("docker")
            .WithArguments("pull", "mcr.microsoft.com/mssql/server:2022-latest")
            .Run(expecting: 0);

        var id = new ExternalProgram("docker")
            .WithArguments("run", "-d", "--rm", "--name", "test-mssql")
            .WithArguments(Publish(_ports))
            .WithArguments(
                "--env",                 "ACCEPT_EULA="           + "Y",
                "--env",                 "MSSQL_SA_PASSWORD="     + Credential.Password,
                "--env",                 "MSSQL_COLLATION="       + Collation,
                "--env",                 "MSSQL_MEMORY_LIMIT_MB=" + MemoryLimitMb,
                "--health-cmd",          "/opt/mssql-tools18/bin/sqlcmd -S . -U sa -P $MSSQL_SA_PASSWORD -No -Q 'PRINT HOST_NAME();'",
                "--health-start-period", "8s",
                "--health-interval",     "1s",
                "--health-timeout",      "1s",
                "--health-retries",      "2",
                "mcr.microsoft.com/mssql/server:2022-latest"
            )
            .Run(expecting: 0);

        id = id.TrimEnd();
        if (id.Length is 0)
            throw new ExternalException(
                "Failed to start SQL Server container. " +
                "The docker command did not output a container id."
            );

        return id;
    }

    private static string[] Publish(ushort[] ports)
    {
        var args  = new string[ports.Length * 2];
        var index = 0;

        foreach (var port in ports)
        {
            args[index++] = "--publish";
            args[index++] = Invariant($"{port}:1433");
        }

        return args;
    }

    private void WaitForReady()
    {
        var deadline = DateTime.UtcNow + new TimeSpan(ReadyWaitTime);

        for(;;)
        {
            var isHealthy = new ExternalProgram("docker")
                .WithArguments("inspect", Id)
                .Run(expecting: 0)
#if NETCOREAPP
                .Contains(@"""Status"": ""healthy""", StringComparison.OrdinalIgnoreCase);
#else
                .IndexOf(@"""Status"": ""healthy""", StringComparison.OrdinalIgnoreCase) >= 0;
#endif

            if (isHealthy && _ports.All(TcpPort.IsListening))
                return;

            if (DateTime.UtcNow >= deadline)
                throw new TimeoutException(
                    "The SQL Server container did not become ready within the expected time."
                );

            Thread.Sleep(millisecondsTimeout: 500);
        }
    }

    private void Stop()
    {
        new ExternalProgram("docker")
            .WithArguments("kill", Id)
            .Run(expecting: 0);

        var deadline = DateTime.UtcNow + new TimeSpan(EndedWaitTime);

        for (;;)
        {
            if (!_ports.Any(TcpPort.IsListening))
                return;

            if (DateTime.UtcNow >= deadline)
                throw new TimeoutException(
                    "The SQL Server container did not stop within the expected time."
                );

            Thread.Sleep(millisecondsTimeout: 500);
        }
    }
}
