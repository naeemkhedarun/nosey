angular.module('myApp.controllers').
controller('CddsController',
    function ($scope, $http, $rootScope) {

        $rootScope.showDeploymentFilter = false;
        
        $scope.servers = [];
       
        $scope.sqlErrors = [];
       
        $scope.graphData = [];
        
        $scope.$on("filterServer", function(e, args){
            $scope.$apply(function(){
                $scope.serverFilter = args.server;    
            })
        });
        
        $http({method: 'GET', url: 'http://asnav-monitor-01:9200/replication/status/_search?size=200&q=isLatest:true'}).
          success(function(result, status, headers, config) {
            for(var i = 0; i < result.hits.hits.length; i++) {
                var doc = result.hits.hits[i]._source;
                doc.failedPublications = [];
                
                doc.publications.map(function(e){
                    if(e.Status === "Failed")
                    {
                        doc.failedPublications.push(e);
                    }
                })
                
                $scope.servers.push(doc);
            }
            
            $scope.servers.map(function(doc){
                var element = {
                    xAxis: doc.server,
                    yAxis: [{ name: "replication", value: doc.failedPublications.length, server: doc.server }]
                }
                $scope.graphData.push(element);
                return element;
            })
            
            var postData = {
                query : {
                    match_all : {}
                },
                facets : {
                    facets: {
                    terms: {
                        field: "server",
                        size : 500
                      }
                  }
              }
          }
          
          $http({method: 'POST', url: 'http://asnav-monitor-01:9200/sql/errors/_search?size=0', data: postData}).
          success(function(result, status, headers, config) {
            for(var i = 0; i < result.facets.facets.terms.length; i++) {
                var doc = result.facets.facets.terms[i];
                $scope.sqlErrors.push(doc);
            }
            
            $scope.sqlErrors.map(function(doc){
                
                var element = {
                    xAxis: doc.term,
                    yAxis: [{ name: "errors", value: doc.count / 10000, server: doc.term }]
                }
                $scope.graphData.push(element);   
            })
            
            
            $scope.$broadcast("replicationStatus", $scope.graphData);
          }).
          error(function(data, status, headers, config) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
          });
            
          }).
          error(function(data, status, headers, config) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
          });
          
          
        
    });