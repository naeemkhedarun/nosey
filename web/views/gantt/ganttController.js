angular.module('myApp.controllers').
controller('GanttController',
    function ($scope, ejsResource) {

        var ejs = ejsResource('http://asnav-monitor-01:9200');

        $scope.lookup = {}
        $scope.results = []
        
        $scope.duration = 0;
        
        $scope.$parent.$watch("deploymentId", function (deploymentId) {
            if (deploymentId) {
                if ($scope.subscription == null) {
                    //                   $scope.results.splice($scope.results.length);
                }
                
                ejs.Request()
                    .indices("deploy")
                    .types("detail")
                    .query(ejs.MatchQuery("deploymentId", deploymentId))
                    .fields(["deploymentId", "environment", "servers", "packages", "sourceBranches"])
                    .doSearch(function (result) {
                        
                        if(result.hits.hits.length > 0){
                            var detail = result.hits.hits[0];
                            
                            $scope.detail = {
                                environment: detail.fields.environment,
                                deploymentId: detail.fields.deploymentId,
                                servers: detail.fields.servers.value,
                                sourceBranches: detail.fields.sourceBranches
                            }
                            
                            $scope.$broadcast("detail", $scope.detail)
                        }
                        
                    });
                
                $scope.subscription = Rx.Observable.timer(200, 2000).select(function () {
                    return ejs.Request()
                        .indices("deploy")
                        .types("profiling")
                        .query(ejs.MatchQuery("deploymentId", deploymentId))
                        .sort("start", "asc")
                        .fields(["start", "end", "MachineName", "deploymentId", "type", "typeName", "packageName", "handler", "stage"])
                        .size(1000)
                        .doSearch(function (result) {
                            
                            for (var i = 0; i < result.hits.hits.length; i++) {
                                
                                var item = result.hits.hits[i]
                                var id = item._id;
                                if(!(id in $scope.lookup))
                                {
                                    item.fields.start = moment(item.fields.start, "DD/MM/YYYY hh:mm:ss").toDate();
                                    item.fields.end = moment(item.fields.end, "DD/MM/YYYY hh:mm:ss").toDate();
                                    
                                    if(item.fields.MachineName)
                                    {
                                        item.fields.name = item.fields.MachineName;    
                                        item.fields.type = "package"
    
                                        item.fields.status = item.fields.type;
                                        $scope.lookup[id] = item;
                                        $scope.results.push(item);
                                        
                                        $scope.$broadcast("profiling", {
                                            new: item
                                        })
                                    }
                                    else
                                    {
                                        item.fields.name = item.fields.stage;   
                                        item.fields.type = "scope"
                                    }
                                }
                            }
                            if($scope.results.length > 0) {
                                $scope.duration = moment.duration($scope.results[$scope.results.length-1].fields.end - $scope.results[0].fields.start).humanize()    
                            }
                            
                        });
                }).subscribe();
            } else {
                if ($scope.subscription) {
                    $scope.subscription.dispose();
                    $scope.subscription = null;
                    $scope.results.splice(0, $scope.results.length);
                    $scope.$broadcast("profiling", {
                        clear: true
                    })
                    $scope.duration = 0;
                }
            }
        })
    });