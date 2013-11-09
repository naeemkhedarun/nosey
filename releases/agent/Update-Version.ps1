$noseyService = Get-Service "csnosey"

if($noseyService)
{
    $noseyService.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped, [TimeSpan]::FromMinutes(1))
}

