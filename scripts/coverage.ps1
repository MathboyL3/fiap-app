param(
    [switch]$NoBuild,
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $root "artifacts"
$testResults = Join-Path $artifacts "TestResults"
$tools = Join-Path $artifacts "tools"
$reportDir = Join-Path $root "coverage-report"
$testProject = Join-Path $root "tests/Oficina.Tests/Oficina.Tests.csproj"
$runSettings = Join-Path $root "coverage.runsettings"

New-Item -ItemType Directory -Force -Path $artifacts, $testResults, $tools, $reportDir | Out-Null

$env:DOTNET_CLI_HOME = $artifacts
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "false"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

$reportGenerator = Join-Path $tools "reportgenerator.exe"
if (-not (Test-Path $reportGenerator)) {
    dotnet tool install dotnet-reportgenerator-globaltool --tool-path $tools
}

$testArgs = @(
    "test",
    $testProject,
    "--configuration",
    $Configuration,
    "--collect:XPlat Code Coverage",
    "--settings",
    $runSettings,
    "--results-directory",
    $testResults
)

if ($NoBuild) {
    $testArgs += "--no-build"
}

dotnet @testArgs

$coverageFile = Get-ChildItem -Path $testResults -Recurse -Filter "coverage.cobertura.xml" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $coverageFile) {
    throw "Coverage file was not generated under '$testResults'."
}

& $reportGenerator `
    "-reports:$($coverageFile.FullName)" `
    "-targetdir:$reportDir" `
    "-reporttypes:Html;TextSummary;Cobertura"

@(
    "Oficina.Tests coverage report",
    "",
    "Open index.html in this folder to read the HTML report.",
    "",
    "Generated from:",
    $coverageFile.FullName,
    "",
    "Key files:",
    "- index.html: interactive HTML report",
    "- Summary.txt: text summary",
    "- Cobertura.xml: machine-readable coverage XML"
) | Set-Content -Path (Join-Path $reportDir "README.txt")

Write-Host ""
Write-Host "Coverage report generated:"
Write-Host "  $reportDir\index.html"
Write-Host "  $reportDir\Summary.txt"
