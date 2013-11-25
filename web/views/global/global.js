angular.module('myApp.controllers').controller('AppController', function ($scope) {
    
    $scope.$watch("unsafeDeploymentId", function (changed) {
        if (changed) {

            var str = changed;
            var patt1 = /[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}/;
            var result = str.match(patt1);
            if (result) {
                $scope.deploymentId = result[0];
            } else {
                $scope.deploymentId = "";
            }
        } else {
            $scope.deploymentId = "";
        }

    });
})