if ((Test-Path NuGet) -eq $false) { mkdir NuGet }
if ((Test-Path NuGet\lib) -eq $false) { mkdir NuGet\lib }
if ((Test-Path NuGet\lib\net45) -eq $false) { mkdir NuGet\lib\net45 }
if ((Test-Path NuGet\tools) -eq $false) { mkdir NuGet\tools }

$dll = Get-ChildItem ..\source\FeatherSharp.ComponentModel\bin\Release\FeatherSharp.ComponentModel.dll
$exe = Get-ChildItem ..\source\FeatherSharp\bin\Release\FeatherSharp.exe

$dllVersion = $dll.VersionInfo.ProductVersion
$exeVersion = $dll.VersionInfo.ProductVersion

if ($dllVersion -ne $exeVersion) {
    echo "Assembly versions do not match!"
    exit
}

echo "Copying $($dll.Name) $($dllVersion)"
copy $dll NuGet\lib\net45
copy ..\source\FeatherSharp.ComponentModel\bin\Release\FeatherSharp.ComponentModel.XML NuGet\lib\net45

echo "Copying $($exe.Name) $($exeVersion)"
copy $exe NuGet\tools
copy ..\source\FeatherSharp\bin\Release\*.dll NuGet\tools
copy ..\source\FeatherSharp\bin\Release\*.config NuGet\tools

$nuspecFileName = "NuGet\FeatherSharp.nuspec"
$pattern = "<version>.*</version>"
$version = "<version>$dllVersion</version>"
(Get-Content $nuspecFileName) | %{ $_ -replace $pattern, $version } | Set-Content $nuspecFileName

.\NuGet-Signed.exe pack NuGet\FeatherSharp.nuspec
