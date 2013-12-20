angular.module('myApp.controllers').
controller('GanttController', function ($scope, ejsResource, $http, $rootScope) {

    var ejs = ejsResource('http://asnav-monitor-01:9200');

    $rootScope.showDeploymentFilter = true;

    $scope.lookup = {};
    $scope.results = [];
    $scope.duration = 0;
    $scope.status = "waiting";
    $scope.errors = [];
    
    $scope.$parent.$watch("deploymentId", function (deploymentId) {
        if (deploymentId) {
            
            ejs.Request()
                .indices("deploy")
                .types("detail")
                .query(ejs.MatchQuery("deploymentId", deploymentId))
                .fields(["deploymentId", "environment", "servers", "packages", "sourceBranches", "isLatest"])
                .doSearch(function (result) {

                if (result.hits.hits.length > 0) {
                    var detail = result.hits.hits[0];
                    detail.fields.servers.push("Scope")
                    $scope.detail = {
                        environment: detail.fields.environment,
                        deploymentId: detail.fields.deploymentId,
                        servers: detail.fields.servers,
                        sourceBranches: detail.fields.sourceBranches
                    }

                    $scope.$broadcast("detail", $scope.detail)
                }

            });

            $scope.subscription = Rx.Observable.timer(200, 2000).select(function () {
                ejs.Request()
                    .indices("deploy")
                    .types("profiling")
                    .query(ejs.MatchQuery("deploymentId", deploymentId))
                    .sort("start", "asc")
                    .fields(["start", "end", "MachineName", "deploymentId", "type", "typeName", "packageName", "handler", "stage"])
                    .size(2000)
                    .doSearch(function (result) {

                    for (var i = 0; i < result.hits.hits.length; i++) {

                        var item = result.hits.hits[i]
                        var id = item._id;
                        if (!(id in $scope.lookup)) {
                            item.fields.start = moment(item.fields.start, "DD/MM/YYYY hh:mm:ss").toDate();
                            item.fields.end = moment(item.fields.end, "DD/MM/YYYY hh:mm:ss").add('s', 0).toDate();

                            if (item.fields.MachineName) {
                                item.fields.name = item.fields.MachineName;

                                item.fields.status = item.fields.type;
                                $scope.lookup[id] = item;
                                $scope.results.push(item);

                                item.redraw = i === (result.hits.hits.length - 1);
                                $scope.status = "running";
                                $scope.$broadcast("profiling", {
                                    new: item
                                });
                            }
                        }
                    }
                    if ($scope.results.length > 0) {
                        $scope.duration = moment.duration($scope.results[$scope.results.length - 1].fields.end - $scope.results[0].fields.start).humanize()
                    }

                });

                $http({
                    method: 'GET',
                    url: 'http://asnav-monitor-01:9200/deploy/profiling/_search?size=100&q=stage:* AND deploymentId:' + deploymentId
                }).
                success(function (result, status, headers, config) {
                    for (var i = 0; i < result.hits.hits.length; i++) {
                        var detail = result.hits.hits[i];
                        var id = detail._id;
                        if (!(id in $scope.lookup)) {
                            detail.fields = {
                                name: "Scope",
                                type: "package",
                                start: moment(detail._source.start, "DD/MM/YYYY hh:mm:ss").toDate(),
                                end: moment(detail._source.end, "DD/MM/YYYY hh:mm:ss").toDate(),
                                typeName: detail._source.stage,
                                status: detail._source.stage,
                                packageName: ""
                            }
                            $scope.lookup[id] = detail;

                            detail.redraw = i === (result.hits.hits.length - 1)

                            $scope.$broadcast("profiling", {
                                new: detail
                            })

                            switch (detail.fields.typeName) {
                                case "Invoke-BeforeDeploymentSteps":
                                    $scope.startTime = moment(detail.fields.start).format("dddd, Do MMMM YYYY, h:mma");
                                    break;
                                case "Invoke-AfterDeploymentSteps":
                                    $scope.endTime = detail.fields.end;
                                    $scope.status = "passed";
                                    $scope.subscription.dispose();
                                    break;   
                            }
                        }

                    }
                }).
                error(function (data, status, headers, config) {
                    // called asynchronously if an error occurs
                    // or server returns response with an error status.
                });

                $http({
                    method: 'GET',
                    url: 'http://asnav-monitor-01:9200/deploy/log/_search?q=level:error AND deploymentId:' + deploymentId
                }).
                success(function (result, status, headers, config) {
                    if(result.hits.hits.length > 0)
                    {
                        $scope.errors = result.hits.hits.map(function(e){return e._source});
                        $scope.status = "failed";    
                    }
                    
                }).
                error(function (data, status, headers, config) {
                    // called asynchronously if an error occurs
                    // or server returns response with an error status.
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