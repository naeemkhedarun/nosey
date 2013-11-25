angular.module('myApp.controllers', []).
controller('MyCtrl1',
    function ($scope, ejsResource) {

        var ejs = ejsResource('http://asnav-monitor-01:9200');

        $scope.results = []
        $scope.duration = 0;
        
        $scope.$parent.$watch("deploymentId", function (deploymentId) {
            if (deploymentId) {
                if ($scope.subscription == null) {
                    //                   $scope.results.splice($scope.results.length);
                }
                $scope.subscription = Rx.Observable.timer(200, 2000).select(function () {
                    return ejs.Request()
                        .indices("deploy")
                        .types("profiling")
                        .query(ejs.MatchQuery("deploymentId", deploymentId))
                        .sort("start", "asc")
                        .fields(["start", "end", "MachineName", "deploymentId", "type", "typeName", "packageName", "handler", "stage"])
                        .size(1000)
                        .doSearch(function (result) {
                            for (var i = $scope.results.length; i < result.hits.hits.length; i++) {
                                var item = result.hits.hits[i]

                                item.fields.start = moment(item.fields.start, "DD/MM/YYYY hh:mm:ss").toDate();
                                item.fields.end = moment(item.fields.end, "DD/MM/YYYY hh:mm:ss").toDate();
                                
                                if(item.fields.MachineName)
                                {
                                    item.fields.name = item.fields.MachineName;    
                                    item.fields.type = "package"
                                }
                                else
                                {
                                    item.fields.name = item.fields.stage;   
                                    item.fields.type = "scope"
                                }
                                
                                item.fields.status = item.fields.type;

                                $scope.results.push(item);
                                
                                $scope.$broadcast("profiling", {
                                    new: item
                                })
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