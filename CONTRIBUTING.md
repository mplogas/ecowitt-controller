# Contributing

Thanks for your interest in contributing to Ecowitt Controller.

## Development Setup

Requires .NET 10 SDK.

```bash
cd src
dotnet build Ecowitt.Controller.sln
dotnet test EcoWitt.Controller.Tests/EcoWitt.Controller.Tests.csproj
```

To run the application locally you need an MQTT broker and a valid `appsettings.json` (see README).

```bash
dotnet run --project Ecowitt.Controller/Ecowitt.Controller.csproj
```

## Submitting Changes

1. Fork the repository and create a branch from `main`
2. Make your changes and ensure the build passes with zero warnings
3. Run the test suite and confirm all tests pass
4. Open a pull request against `main`

Keep PRs focused on a single concern. If you're fixing a bug and spotted an unrelated issue, submit them separately.

## Code Style

- Follow the existing conventions in the codebase
- Use structured logging templates with Serilog (no string interpolation)
- Prefer partial classes for service separation (consumers, publishers, events)
- No warnings in the main project -- treat warnings as errors

## Bug Reports and Feature Requests

Open an [issue](https://github.com/mplogas/ecowitt-controller/issues). For bugs, include the log output (set `Serilog.MinimumLevel` to `Debug`), your configuration (redact credentials), and the firmware version of your gateway.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
