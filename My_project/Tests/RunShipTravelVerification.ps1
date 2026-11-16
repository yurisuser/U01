param([string]$UnityEditor)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$projectFile = Join-Path $projectRoot 'Assembly-CSharp.csproj'
if (!(Test-Path -LiteralPath $projectFile)) { throw 'Generate the Unity solution before running verification.' }
if (!$UnityEditor) {
    $versionLine = Get-Content (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') | Select-Object -First 1
    $version = $versionLine.Split(':')[1].Trim()
    $UnityEditor = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe"
}
$UnityEditor = (Resolve-Path -LiteralPath $UnityEditor).Path

# Проверяем сборку и запускаем реальные математические функции Unity в отдельном временном проекте.
dotnet build $projectFile --no-restore -v:q -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { throw 'Project build failed.' }
$testRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('u01-travel-check-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path "$testRoot/Assets/Editor", "$testRoot/ProjectSettings", "$testRoot/Packages" | Out-Null
Copy-Item -LiteralPath "$projectRoot/ProjectSettings/ProjectVersion.txt" -Destination "$testRoot/ProjectSettings/ProjectVersion.txt"
Copy-Item -LiteralPath "$PSScriptRoot/ShipTravelPlannerVerification.cs" -Destination "$testRoot/Assets/Editor/TravelForecastVerification.cs"
Copy-Item -LiteralPath "$PSScriptRoot/ShipTravelAiVerification.cs" -Destination "$testRoot/Assets/Editor/ShipTravelAiVerification.cs"
Copy-Item -LiteralPath "$PSScriptRoot/UnityHarnessManifest.json" -Destination "$testRoot/Packages/manifest.json"
$runtimeAssembly = (Resolve-Path -LiteralPath "$projectRoot/Temp/bin/Debug/Assembly-CSharp.dll").Path
$logPath = Join-Path $testRoot 'verification.log'
$arguments = '-batchmode -nographics -projectPath "{0}" -executeMethod TravelForecastVerification.Run -u01TravelAssembly "{1}" -logFile "{2}"' -f $testRoot, $runtimeAssembly, $logPath
$process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
$log = Get-Content -LiteralPath $logPath
$log | Select-String 'TRAVEL_CHECK_PASS|TRAVEL_AI_CHECK_PASS|TRAVEL_VERIFICATION_'
Write-Output "Verification log: $logPath"
if ($process.ExitCode -ne 0 -or !($log -match 'TRAVEL_VERIFICATION_PASS')) {
    $log | Select-Object -Last 40
    throw 'Travel verification failed.'
}
