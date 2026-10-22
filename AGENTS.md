# Repository Guidelines

## Project Structure & Module Organization

The solution is `src/BinaryKits.Zpl.sln`. Its main libraries are:

- `BinaryKits.Zpl.Label`: label elements, ZPL generation, and image conversion.
- `BinaryKits.Zpl.Protocol`: raw Zebra protocol commands.
- `BinaryKits.Zpl.Viewer`: local ZPL analysis and image rendering.
- `BinaryKits.Zpl.Labelary`: client for the external Labelary service.

Matching `*.UnitTest` projects sit alongside the libraries. `BinaryKits.Zpl.Viewer.WebApi` hosts the viewer API; `BinaryKits.Zpl.TestConsole` provides executable examples. Documentation images live in `doc/`, viewer test fixtures in `src/BinaryKits.Zpl.Viewer.UnitTest/Data/`, and API sample labels in its `Labels/` directory.

## Build, Test, and Development Commands

Use the .NET 8 SDK. Run these commands from `src/`:

```sh
dotnet restore
dotnet build --configuration Release --no-restore /p:NoWarn=1591
dotnet test --configuration Release --no-restore --no-build --verbosity normal
dotnet run --project BinaryKits.Zpl.Viewer.WebApi
```

These restore dependencies, build using CI settings, execute tests, and start the local API. For focused tests, use `dotnet test BinaryKits.Zpl.Viewer.UnitTest --filter FullyQualifiedName~DrawerTest`. Preserve compatibility with each library's declared target frameworks; several support .NET Framework 4.7.2 and .NET Standard alongside .NET 8.

## Coding Style & Naming Conventions

Follow `src/.editorconfig`: spaces, LF endings, final newlines, Allman braces, and explicit local types. Match surrounding code's four-space indentation. Use PascalCase for types and public members, camelCase for parameters and locals, and an `I` prefix for interfaces. Keep namespaces aligned with project folders. Avoid unrelated formatting changes; the editor configuration supplies the shared style rules.

## Testing Guidelines

Tests use MSTest with `[TestClass]` and `[TestMethod]`. Follow existing names such as `BarcodeTest` and descriptive methods such as `Barcode128`. Add regression tests to the corresponding module, asserting exact ZPL output or relevant analyzer/rendering behavior. Keep required fixtures under the existing test data directories. No numeric coverage threshold is configured. CI builds and tests on Windows and Linux; consider platform-dependent fonts and rendering when changing viewer behavior.

## Commit & Pull Request Guidelines

Recent commits use short, imperative descriptions, such as `Fix copy-paste-error for ellipse (#342)`; Conventional Commit prefixes are not required. Keep commits focused. In PRs, explain the problem, resulting behavior, linked issues, and tests run. Include sample ZPL and before/after images for rendering changes. Ensure applicable CI checks pass before merging.

## Data Handling

Use synthetic label data in examples and fixtures. Do not send customer information to Labelary; use the local viewer for sensitive labels.
