// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Subatomix.Testing.SqlServerIntegration;

internal class ExternalProgram
{
    protected ProcessStartInfo Info { get; }

    public ExternalProgram(string name)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));
        if (name.Length is 0)
            throw new ArgumentException("Argument must not be empty.", nameof(name));

        Info = new ProcessStartInfo
        {
            FileName               = name,
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
        };
    }

    public ExternalProgram WithArguments(params string[] args)
    {
#if NETCOREAPP2_1_OR_GREATER
        foreach (var arg in args)
            Info.ArgumentList.Add(arg);
#else
        var builder = new StringBuilder(Info.Arguments);

        foreach (var arg in args)
            AppendArgument(builder, arg);

        Info.Arguments = builder.ToString();
#endif
        return this;
    }

    public ExternalProgram WithArguments(IEnumerable<string> args)
    {
#if NETCOREAPP2_1_OR_GREATER
        foreach (var arg in args)
            Info.ArgumentList.Add(arg);
#else
        var builder = new StringBuilder(Info.Arguments);

        foreach (var arg in args)
            AppendArgument(builder, arg);

        Info.Arguments = builder.ToString();
#endif
        return this;
    }

    public (int ExitCode, string Output) Run()
    {
        using var process = new Process { StartInfo = Info };

        var output = new StringBuilder();

        void OnDataReceived(object? _, DataReceivedEventArgs e)
        {
            // e.Data is null when the stream is closed
            if (e.Data is null)
                return;

            output.AppendLine(e.Data);
        }

        process.OutputDataReceived += OnDataReceived;
        process.ErrorDataReceived  += OnDataReceived;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return (process.ExitCode, output.ToString());
    }

    public string Run(int expecting)
    {
        var (exitCode, output) = Run();

        if (exitCode != expecting)
            throw OnExitedWithCode(exitCode, output);

        return output;
    }

#if NEEDED
    public T Run<T>(Func<int, string, T> projection)
    {
        if (projection is null)
            throw new ArgumentNullException(nameof(projection));

        var (exitCode, output) = Run();

        return projection(exitCode, output);
    }
#endif

    private Exception OnExitedWithCode(int exitCode, string output)
    {
        var message = new StringBuilder()
            .AppendFormat("{0} exited with code {1}.", Info.FileName, exitCode);

        if (output.Length > 0)
            message
                .AppendLine()
                .AppendLine("----- BEGIN OUTPUT -----")
                .Append/**/(output) // output already ends with a newline
                .AppendLine("----- END OUTPUT -----");

        return new ExternalException(message.ToString())
        {
            Data =
            {
                ["ExitCode"] = exitCode,
                ["Output"]   = output
            }
        };
    }

#if !NETCOREAPP2_1_OR_GREATER // .NET Framework only
    internal void AppendArgument(StringBuilder args, string arg)
    {
        if (args.Length > 0)
            args.Append(' ');
        
        args.Append(Quote(arg));
    }

    internal static string Quote(string arg)
    {
        if (!arg.Contains(' '))
            return arg;

        const string Quote = "\"", Escape = "\\";

        return string.Concat(Quote, arg.Replace(Quote, Escape + Quote), Quote);
    }
#endif
}
