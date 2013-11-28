function Install-ESTypes
{
    $searchServer = "localhost"
    # Invoke-RestMethod -Method PUT -Uri "http://localhost:9200/log"
    # Invoke-RestMethod -Method PUT -Uri "http://localhost:9200/log/iis/_mapping" -Body "{ `"iis`" : { `"properties`" : { `"date`" : { `"type`" : `"date`", `"format`" : `"dd/MM/yyyy HH:mm:ss`" } } } }"
    # Invoke-RestMethod -Method PUT -Uri "http://localhost:9200/log/event/_mapping" -Body "{ `"event`" : { `"properties`" : { `"Date`" : { `"type`" : `"date`", `"format`" : `"dd/MM/yyyy HH:mm:ss`" } } } }"

    Invoke-RestMethod -Method PUT -Uri "http://$searchServer:9200/counter"
    Invoke-RestMethod -Method PUT -Uri "http://$searchServer:9200/counter/machine/_mapping" -Body "{ `"machine`" : { `"properties`" : { `"Date`" : { `"type`" : `"date`", `"format`" : `"dd/MM/yyyy HH:mm:ss`" } } } }"
    Invoke-RestMethod -Method PUT -Uri "http://$searchServer:9200/counter/heartbeat/_mapping" -Body "{ `"heartbeat`" : { `"properties`" : { `"Date`" : { `"type`" : `"date`", `"format`" : `"dd/MM/yyyy HH:mm:ss`" } } } }"

    Invoke-RestMethod -Method PUT -Uri "http://$searchServer:9200/deploy"

    $date = @{ type = "date"; format = "dd/MM/yyyy HH:mm:ss" }
    $mapping = @{ profiling = @{ properties = @{ "start" = $date; "end" = $date; "deploymentId" = @{ "type" = "string"; "index" = "not_analyzed"; } } } }
    Invoke-RestMethod -Method PUT -Uri "http://$searchServer:9200/deploy/profiling/_mapping" -Body (ConvertTo-Json $mapping -Depth 99 -Compress)

    $mapping = @{ detail = @{ properties = @{ "deploymentId" = @{ "type" = "string"; "index" = "not_analyzed"; } } } }
    Invoke-RestMethod -Method PUT -Uri "http://$searchServer:9200/deploy/detail/_mapping" -Body (ConvertTo-Json $mapping -Depth 99 -Compress)
}

function Import-Log
{
    param($location)

    $importer = "$PSScriptRoot\CSNosey\CSNosey\bin\Release\CSNosey.exe"
    & $importer $location
}
