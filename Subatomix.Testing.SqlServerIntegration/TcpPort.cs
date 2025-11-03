// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Sockets;

namespace Subatomix.Testing.SqlServerIntegration;

internal static class TcpPort
{
    [ExcludeFromCodeCoverage]
    // - Difficult to exercise all paths on a single platform
    // - Tested indirectly by TestSqlServerIntegrationTests
    public static bool IsListening(ushort port)
    {
        const int TimeoutMs = 1000;

        try
        {
            using var client = new TcpClient();

            return client.ConnectAsync(IPAddress.Loopback, port).Wait(TimeoutMs)
                && client.Connected;
        }
        catch (AggregateException e) when (e.GetBaseException() is SocketException)
        {
            return false;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
