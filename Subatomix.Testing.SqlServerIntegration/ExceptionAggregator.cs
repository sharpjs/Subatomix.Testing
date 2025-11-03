// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Runtime.ExceptionServices;

namespace Subatomix.Testing.SqlServerIntegration;

internal static class ExceptionAggregator
{
    public static void Try(Action action, ref List<Exception>? exceptions)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            (exceptions ??= new()).Add(e);
        }
    }

    public static void ThrowIfAny(this List<Exception>? exceptions)
    {
        if (exceptions is null)
            return;

        if (exceptions.Count is 1)
            ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

        throw new AggregateException(exceptions);
    }
}
