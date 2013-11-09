param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("major", "minor", "patch")]
        [string]
        $increment
        # [Parameter(Mandatory=$true)]
        # $releaseMessage
)

. "$PSScriptRoot\semver.ps1"

$root = "$PSScriptRoot\.."



msbuild "$root\psnosey\csnosey\csnosey.sln" /p:configuration=release /p:outdir="$root\releases\agent"

push-location "$root\releases\agent"
Invoke-Semver $increment
$version = Invoke-Semver -Format "v%M.%m.%p$s"
Write-Host "Version is now:  $version."
#git tag -a $version -m "First release of agent"
pop-location

