angular.module('myApp.controllers').
controller('EnvironmentController',
    function ($scope, ejsResource) {

        var ejs = ejsResource('http://asnav-monitor-01:9200');

        $scope.environments = {}
        
        ejs.Request()
            .indices("deploy")
            .types("detail")
            .sort("date", "asc")
            .fields(["deploymentId", "environment", "servers", "packages", "sourceBranches", "date"])
            .doSearch(function (result) {
                
                for(var i = 0; i < result.hits.hits.length; i++) {
                    var detail = result.hits.hits[i];
                    
                    if(!(detail.fields.environment in $scope.environments)){
                        $scope.environments[detail.fields.environment] = {
                            environment: detail.fields.environment,
                            deploymentId: detail.fields.deploymentId,
                            servers: detail.fields.servers.value,
                            sourceBranches: detail.fields.sourceBranches,
                            date: moment(detail.fields.date, "dd/MM/yyyy HH:mm:ss")
                        };
                    }else{
                        
                        if($scope.environments[detail.fields.environment].date < moment(detail.fields.date, "dd/MM/yyyy HH:mm:ss")){
                            $scope.environments[detail.fields.environment] = {
                                environment: detail.fields.environment,
                                deploymentId: detail.fields.deploymentId,
                                servers: detail.fields.servers.value,
                                sourceBranches: detail.fields.sourceBranches,
                                date: moment(detail.fields.date, "dd/MM/yyyy HH:mm:ss")
                            }
                        }
                    }
                }
                
            });
    });