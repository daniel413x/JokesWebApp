# Jokes Web App

Based on https://www.youtube.com/watch?v=BfEjDD8mWYg

## Objective

Demonstrate NUnit testing of an ASP.NET Core application.

## Running tests

Run the command:

```bash
dotnet test
```

## Generating reports

Run the commands:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html;Badges
```