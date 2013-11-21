param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("major", "minor", "patch")]
        [string]
        $increment,
        [switch]$release
        # [Parameter(Mandatory=$true)]
        # $releaseMessage
)

. "$PSScriptRoot\semver.ps1"

$root = "$PSScriptRoot\.."

try {
    msbuild "$root\psnosey\csnosey\csnosey.sln" /p:configuration=release /p:outdir="C:\git\release_nosey\releases\agent"

    push-location "C:\git\release_nosey\releases\agent"
    Invoke-Semver $increment
    $version = Invoke-Semver -Format "v%M.%m.%p$s"
    Write-Host "Version is now:  $version."

    if($release)
    {
        git add . --force
        git commit -m "Automated checkin for release $version."
        git tag -a $version -m "Automated tagging for release $version."    
    }

    pop-location

}
catch {
    throw;
}