// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace Subatomix.Testing.SqlServerIntegration;

[ExcludeFromCodeCoverage] // Platform-specific
internal class GlobalLock : IDisposable
{
    private readonly FileStream _lockFile;

    public GlobalLock(string name)
    {
        var path   = Path.Combine(Path.GetTempPath(), name + ".lock");
        var random = new Random(Guid.NewGuid().GetHashCode());
        //                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        // Because the Random() default constructor in .NET Framework uses
        // a simple time-derived seed, making collisions likely.

        for (;;)
        {
            try
            {
                _lockFile = new FileStream(
                    path,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 1,
                    FileOptions.WriteThrough | FileOptions.DeleteOnClose
                );
                break;
            }
            catch (IOException)
            {
                // Wait and retry
                Thread.Sleep(300 + random.Next(maxValue: 200));
            }
        }
    }

    public void Dispose()
    {
        _lockFile.Dispose();
    }
}
