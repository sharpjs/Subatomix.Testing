// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace Subatomix.Testing.SqlServerIntegration;

[TestFixture]
public class ExternalProgramTests
{
    [Test]
    public void Construct_NullName()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            _ = new ExternalProgram(null!);
        });
    }

    [Test]
    public void Construct_EmptyName()
    {
        Should.Throw<ArgumentException>(() =>
        {
            _ = new ExternalProgram("");
        });
    }

    [Test]
    public void Run_ExpectingExitCode_ExitsWithOtherCode_NoOuput()
    {
        var e = Should.Throw<ExternalException>(() =>
        {
            new ExternalProgram("pwsh")
                .WithArguments("-nop", "-c", "exit 42")
                .Run(expecting: 0);
        });

        e.Message.ShouldBe("pwsh exited with code 42.");
        e.Data["ExitCode"].ShouldBe(42);
        e.Data["Output"  ].ShouldBe("");
    }

    [Test]
    public void Run_ExpectingExitCode_ExitsWithOtherCode_WithOutput()
    {
        var e = Should.Throw<ExternalException>(() =>
        {
            new ExternalProgram("pwsh")
                .WithArguments("-nop", "-c", "Write-Host Foo; exit 42")
                .Run(expecting: 0);
        });

        e.Message.ShouldBe(string.Concat(
            "pwsh exited with code 42.", Environment.NewLine,
            "----- BEGIN OUTPUT -----",  Environment.NewLine,
            "Foo",                       Environment.NewLine,
            "----- END OUTPUT -----",    Environment.NewLine
        ));
        e.Data["ExitCode"].ShouldBe(42);
        e.Data["Output"  ].ShouldBe("Foo" + Environment.NewLine);
    }

    [Test]
    public void WithArguments_Enumerable()
    {
        new ExternalProgram("pwsh")
            .WithArguments((IEnumerable<string>) ["-nop", "-c", "exit 0"])
            .Run(expecting: 0);
    }
}
