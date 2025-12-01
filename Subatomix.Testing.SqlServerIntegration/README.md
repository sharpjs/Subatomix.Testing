## About

[![Build](https://github.com/sharpjs/Subatomix.Testing/workflows/Build/badge.svg)](https://github.com/sharpjs/Subatomix.Testing/actions)
[![Build](https://img.shields.io/badge/coverage-100%25-brightgreen.svg)](https://github.com/sharpjs/Subatomix.Testing/actions)
[![NuGet](https://img.shields.io/nuget/v/Subatomix.Testing.SqlServerIntegration.svg)](https://www.nuget.org/packages/Subatomix.Testing.SqlServerIntegration)
[![NuGet](https://img.shields.io/nuget/dt/Subatomix.Testing.SqlServerIntegration.svg)](https://www.nuget.org/packages/Subatomix.Testing.SqlServerIntegration)

Provides easy temporary SQL Server databases for integration testing.

The top-level API surface of this package is the `TestSqlServer` static class,
which sets up and tears down access to a local SQL Server instance.  The class
provides methods to generate and populate temporary databases on the instance.
The class deletes the temporary databases as part of teardown.

`TestSqlServer` supports three scenarios, which its `SetUp` method attempts in
order:

- **Existing Server via SQL Authentication**

  If the environment variable `MSSQL_SA_PASSWORD` is defined, setup assumes
  that a local SQL Server default instance is running and that tests can
  authenticate as the system administrator (`sa`) using the given password.

- **Existing Server via Integrated Authentication**

  Else, setup attempts to detect a local SQL Server default instance, assuming
  that the instance supports integrated authentication and that the current
  user has sufficient privileges to run tests.

- **Ephemeral Server**

  Else, setup assumes that a `docker` command exists and can run Linux
  containers.  Setup uses the command to start an ephemeral Linux SQL Server
  container with a random system administrator password.

A typical pattern is to use `TestSqlServer` from an NUnit
[setup fixture](https://docs.nunit.org/articles/nunit/writing-tests/attributes/setupfixture.html)
in a namespace that contains integration tests.

```csharp
using Subatomix.Testing.SqlServerIntegration;

namespace MyProject.Tests.Integration;

[SetUpFixture]
public static class IntegrationTestsSetup
{
    private static TemporaryDatabase? _database;

    public static TemporaryDatabase Database
        => _database
        ?? throw new InvalidOperationException("SetUp has not executed.");

    [OneTimeSetUp]
    public static void SetUp()
    {
        // Discover a local SQL Server default instance, if any; else, spin up
        // a new instance in a Linux container
        TestSqlServer.SetUp();

        // Create a temporary database (as many as desired)
        _database = TestSqlServer.CreateTemporaryDatabase();

        // Set up the temporary database
        _database.Execute("-- your setup SQL here --");
    }

    [OneTimeTearDown]
    public static void TearDown()
    {
        // Delete temporary databases and stop the containerized instance if
        // created above
        TestSqlServer.TearDown();

        _database = null;
    }
}
```

The temporary databases are then available to all tests in the namespace.

```csharp
[Test]
public void Foo()
{
    // The generated name of the temporary database
    _ = IntegrationTestsSetup.Database.Name;

    // The connection string for the temporary database
    _ = IntegrationTestsSetup.Database.ConnectionString;

    // A NetworkCredential (if SQL Authentication is used)
    _ = TestSqlServer.Credential;

    // A SqlCredential (if SQL Authentication is used)
    _ = TestSqlServer.SqlCredential;
}
```

<!--
  Copyright Subatomix Research Inc.
  SPDX-License-Identifier: MIT
-->
