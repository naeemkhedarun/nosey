angular.module('myApp.controllers').
controller('EnvironmentController',
    function ($scope, $http, $rootScope) {

        $rootScope.showDeploymentFilter = false;
        
        $scope.environments = {}
        
        $http({method: 'GET', url: 'http://asnav-monitor-01:9200/deploy/detail/_search?q=isLatest:true&size=200'}).
          success(function(result, status, headers, config) {
            for(var i = 0; i < result.hits.hits.length; i++) {
                var detail = result.hits.hits[i]._source;
                
                if(!(detail.environment in $scope.environments)){
                    $scope.environments[detail.environment] = {
                        environment: detail.environment,
                        deploymentId: detail.deploymentId,
                        servers: detail.servers.value,
                        sourceBranches: detail.sourceBranches,
                        date: moment(detail.date, "dd/MM/yyyy HH:mm:ss")
                    };
                }else{
                    
                    if($scope.environments[detail.environment].date < moment(detail.date, "dd/MM/yyyy HH:mm:ss")){
                        $scope.environments[detail.environment] = {
                            environment: detail.environment,
                            deploymentId: detail.deploymentId,
                            servers: detail.servers.value,
                            sourceBranches: detail.sourceBranches,
                            date: moment(detail.date, "dd/MM/yyyy HH:mm:ss")
                        }
                    }
                }
            }
          }).
          error(function(data, status, headers, config) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
          });
        
    });