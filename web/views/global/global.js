angular.module('myApp.controllers').controller('AppController', function ($scope, $location, $rootScope) {
    
    $rootScope.showDeploymenFilter = true;
    
    $scope.unsafeDeploymentId = $location.search().deploymentId;
    
    $scope.$watch("unsafeDeploymentId", function (changed) {
        if (changed) {

            var str = changed;
            var patt1 = /[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}/;
            var result = str.match(patt1);
            if (result) {
                $scope.deploymentId = result[0];
                $location.search({deploymentId: result[0]});
            } else {
                $scope.deploymentId = "";
            }
        } else {
            $scope.deploymentId = "";
            $location.search({deploymentId: ""});
        }

    });
})