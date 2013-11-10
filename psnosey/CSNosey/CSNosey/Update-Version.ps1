param(
	$updateLocation,
	$targetLocation
)

New-EventLog -LogName Application -Source NoseyUpdater -ErrorAction SilentlyContinue

$noseyService = Get-Service "nosey"

if($noseyService)
{
    $noseyService.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped, [TimeSpan]::FromMinutes(1))
}

Copy-Item $updateLocation $targetLocation -Force -Verbose -Recurse -Include *.*

Write-EventLog `
    -LogName Application `
    -Source NoseyUpdater `
    -EntryType Information `
    -Message ("Upgraded nosey service {0} with downloaded files from {1}" -f $targetLocation, $updateLocation) `
    -EventId 1 
