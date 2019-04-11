param (
	[Parameter(Mandatory=$true)][string]$dir
)

$vswhere = $PSScriptRoot + "\..\bin\vswhere"

echo "Finding msbuild using $vswhere"

$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1

if (!$msbuild) {
	echo "Failed to find msbuild"
    exit 1
}

echo "  Found => $msbuild"

echo "Finding csprojs in $dir ..."
$projs = ls $dir -Recurse -include *.csproj -Name

foreach($p in $projs) {
	echo ""
	echo "Restoring packages for $p ..."
	& $msbuild /verbosity:minimal /nologo /t:restore /m $p
}
