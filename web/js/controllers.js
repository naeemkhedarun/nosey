'use strict';

/* Controllers */

angular.module('myApp.controllers', []).
controller('MyCtrl1', 
    function ($scope, ejsResource) {
        var ejs = ejsResource('http://localhost:9200');
        
        ejs.Request()
            .indices("counter") 
            .types("machine")
            .sort("Date", "desc")
            .fields(["Date", "MachineName", "StatName", "Value"])
            .doSearch(function(result){
                $scope.results = result.hits.hits;    
            });
        
  })
    .controller('MyCtrl2', 
        function () {

  });