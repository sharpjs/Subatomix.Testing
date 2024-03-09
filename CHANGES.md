# Changes in Subatomix.Testing
This file documents all notable changes.

Most lines should begin with one of these words:
*Add*, *Fix*, *Update*, *Change*, *Deprecate*, *Remove*.

<!--
## [Unreleased](https://github.com/sharpjs/Subatomix.Testing/compare/release/3.1.0..HEAD)
-->

## [3.1.0](https://github.com/sharpjs/Subatomix.Testing/compare/release/3.0.0..release/3.1.0)

## [3.0.0](https://github.com/sharpjs/Subatomix.Testing/compare/release/2.6.0..release/3.0.0)
- **BREAKING:** Change target frameworks to:
  - .NET Framework 4.8.1
  - .NET 6.0
  - .NET 8.0
- Update Microsoft.NET.Test.Sdk to [17.8.0](https://github.com/microsoft/vstest/releases/tag/v17.8.0).
- Update Moq to [4.20.70](https://github.com/moq/moq/releases/tag/v4.20.70)
- Update NUnit to [4.0.1](https://docs.nunit.org/articles/nunit/release-notes/framework.html#nunit-401---december-2-2023)
  See [NUnit 4 Migration Guidance](https://docs.nunit.org/articles/nunit/release-notes/Nunit4.0-MigrationGuide.html).

## [2.6.0](https://github.com/sharpjs/Subatomix.Testing/compare/release/2.5.1..release/2.6.0)
- Update FluentAssertions to [6.12.0](https://github.com/fluentassertions/fluentassertions/releases/tag/6.12.0)
- Update Moq to [4.20.69](https://github.com/moq/moq/releases/tag/v4.20.69)
- Update Microsoft.NET.Test.Sdk to [17.7.2](https://github.com/microsoft/vstest/releases/tag/v17.7.2).

## [2.5.1](https://github.com/sharpjs/Subatomix.Testing/compare/release/2.5.0..release/2.5.1)
- Update Microsoft.NET.Test.Sdk to [17.6.3](https://github.com/microsoft/vstest/releases/tag/v17.6.3).

## [2.5.0](https://github.com/sharpjs/Subatomix.Testing/compare/release/2.4.1..release/2.5.0)
- Update Coverlet to [6.0.0](https://github.com/coverlet-coverage/coverlet/releases/tag/v6.0.0)
- Update FluentAssertions to [6.11.0](https://github.com/fluentassertions/fluentassertions/releases/tag/6.11.0)
- Update Microsoft.NET.Test.Sdk to [17.6.0](https://github.com/microsoft/vstest/blob/main/docs/releases.md#1760).
- Update Moq to [4.18.4](https://github.com/moq/moq4/blob/v4.18.4/CHANGELOG.md)
- Update NUnit3TestAdapter to [4.5.0](https://docs.nunit.org/articles/vs-test-adapter/AdapterV4-Release-Notes.html#nunit3-test-adapter-for-visual-studio-and-dotnet---version-450---may-30-2023).

## [2.4.1](https://github.com/sharpjs/Subatomix.Testing/compare/release/2.4.0..release/2.4.1)
- Update Moq to [4.18.3](https://github.com/moq/moq4/blob/v4.18.3/CHANGELOG.md)
- Update NUnit3TestAdapter to [4.3.1](https://docs.nunit.org/articles/vs-test-adapter/AdapterV4-Release-Notes.html#nunit3-test-adapter-for-visual-studio---version-431---nov-19-2022),
  which fixes [nunit-console #1178](https://github.com/nunit/nunit-console/issues/1178).

## [2.4.0](https://github.com/sharpjs/Subatomix.Testing/compare/release/2.3.0..release/2.4.0)
- Add .NET 7.0 support.
- Update Microsoft.NET.Test.Sdk to [17.4.0](https://github.com/microsoft/vstest-docs/blob/main/docs/releases.md#1740).
- Update NUnit3TestAdapter to [4.3.0](https://docs.nunit.org/articles/vs-test-adapter/AdapterV4-Release-Notes.html#nunit3-test-adapter-for-visual-studio---version-430---oct-29-2022).
- Fix: exception from `TestHarnessBase.CleanUp()` was always ignored.

Known issue: [nunit-console #1178](https://github.com/nunit/nunit-console/issues/1178)

## [2.3.0](https://github.com/sharpjs/Subatomix.Testing/compare/release/2.2.0..release/2.3.0)
- Update Coverlet to [3.2.0](https://github.com/coverlet-coverage/coverlet/releases/tag/v5.8.0)
- Update FluentAssertions to [6.8.0](https://github.com/fluentassertions/fluentassertions/releases/tag/6.8.0)
- Update Microsoft.NET.Test.Sdk to [17.3.2](https://github.com/microsoft/vstest-docs/blob/main/docs/releases.md#1732)
- Update Moq to [4.18.2](https://github.com/moq/moq4/blob/v4.18.2/CHANGELOG.md)

## [2.2.0](https://github.com/sharpjs/Subatomix.Testing/compare/release/2.1.0..release/2.2.0)
- Add implicit `using` statements for testing-related namespaces:
  - `FluentAssertions`
  - `FluentAssertions.FluentActions` (static)
  - `Moq`
  - `NUnit.Framework`
  - `Subatomix.Testing`
- Fix packaging issues:
  - Enable deterministic build.
  - Embed untracked sources in symbols package.

## [2.1.0](https://github.com/sharpjs/Subatomix.Testing/compare/release/2.0.1..release/2.1.0)
- Update FluentAssertions to [6.7.0](https://github.com/fluentassertions/fluentassertions/releases/tag/6.7.0)
- Update Microsoft.NET.Test.Sdk to [17.2.0](https://github.com/microsoft/vstest-docs/blob/main/docs/releases.md#1720)
- Update Moq to [4.18.1](https://github.com/moq/moq4/blob/v4.18.1/CHANGELOG.md)

## [2.0.1](https://github.com/sharpjs/Subatomix.Testing/compare/release/2.0.0..release/2.0.1)
- Fix missing IntelliSense.

## [2.0.0](https://github.com/sharpjs/Subatomix.Testing/compare/release/1.1.5..release/2.0.0)
- Add `ExceptionTests`.
- Add support for .NET 6.0
- `BREAKING` Remove support for .NET Core 2.1
- Update Coverlet to [3.1.2](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/Changelog.md#release-date-2022-02-06)
- `BREAKING` Update FluentAssertions to [6.6.0](https://github.com/fluentassertions/fluentassertions/releases/tag/6.6.0)
  - See [migration details](https://fluentassertions.com/upgradingtov6).
- Update Microsoft.NET.Test.Sdk to [17.1.0](https://github.com/microsoft/vstest-docs/blob/main/docs/releases.md#1710)
- Update Moq to [4.17.2](https://github.com/moq/moq4/blob/v4.17.2/CHANGELOG.md)
- Update NUnit to [3.13.3](https://docs.nunit.org/articles/nunit/release-notes/framework.html#nunit-3133---march-20-2022)
- Update NUnit3TestAdapter to [4.2.1](https://docs.nunit.org/articles/vs-test-adapter/AdapterV4-Release-Notes.html#nunit3-test-adapter-for-visual-studio---version-421---jan-21-2022)

## [1.1.5](https://github.com/sharpjs/Subatomix.Testing/compare/release/1.1.4..release/1.1.5)
- Update Microsoft.NET.Test.Sdk to [16.11.0](https://github.com/microsoft/vstest-docs/blob/main/docs/releases.md#16110)

## [1.1.4](https://github.com/sharpjs/Subatomix.Testing/compare/release/1.1.3..release/1.1.4)
- Update Microsoft.NET.Test.Sdk to [16.10.0](https://github.com/microsoft/vstest-docs/blob/master/docs/releases.md#16100)
- Update NUnit to [3.13.2](https://docs.nunit.org/articles/nunit/release-notes/framework.html#nunit-3132---april-27-2021)

## [1.1.3](https://github.com/sharpjs/Subatomix.Testing/compare/release/1.1.2..release/1.1.3)
- Update Microsoft.NET.Test.Sdk to [16.9.4](https://github.com/microsoft/vstest-docs/blob/master/docs/releases.md#1694)

## [1.1.2](https://github.com/sharpjs/Subatomix.Testing/compare/release/1.1.1..release/1.1.2)
- Update Microsoft.NET.Test.Sdk to [16.9.1](https://github.com/microsoft/vstest-docs/blob/master/docs/releases.md#1691)
- Update Moq to [4.16.1](https://github.com/moq/moq4/blob/v4.16.1/CHANGELOG.md)

## [1.1.1](https://github.com/sharpjs/Subatomix.Testing/compare/release/1.1.0..release/1.1.1)
- Update NUnit to [3.13.1](https://docs.nunit.org/articles/nunit/release-notes/framework.html#nunit-3131---january-31-2021)

## [1.1.0](https://github.com/sharpjs/Subatomix.Testing/compare/release/1.0.0..release/1.1.0)
- Update Moq to [4.16.0](https://github.com/moq/moq4/blob/v4.16.0/CHANGELOG.md))
- Update NUnit to [3.13.0](https://docs.nunit.org/articles/nunit/release-notes/framework.html#nunit-313---january-7-2021)

## [1.0.0](https://github.com/sharpjs/Subatomix.Testing/tree/release/1.0.0)
Initial release.

<!--
  Copyright 2023 Subatomix Research Inc.
  SPDX-License-Identifier: ISC
-->
