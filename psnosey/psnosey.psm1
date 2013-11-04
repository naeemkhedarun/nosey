function Install-ESTypes
{
    Invoke-RestMethod -Method PUT -Uri "http://localhost:9200/log"
    Invoke-RestMethod -Method PUT -Uri "http://localhost:9200/log/iis/_mapping" -Body "{ `"iis`" : { `"properties`" : { `"date`" : { `"type`" : `"date`", `"format`" : `"dd/MM/yyyy HH:mm:ss`" } } } }"
}

function Import-Log
{
    param($location)

    $importer = "$PSScriptRoot\CSNosey\CSNosey\bin\Release\CSNosey.exe"
    & $importer $location
}
