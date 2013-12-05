'use strict'

angular.module('myApp.directives').directive("gantt", function () {
    return {
        restrict: 'A',

        scope: {
            onClick: '=',
            width: '=',
            height: '=',
            bind: '=',
            duration: '@'
        },

        link: function ($scope, element, attrs) {

            var format = "%H:%M";

            $scope.$on("detail", function (e, args) {
                $scope.gantt = d3.gantt(element, attrs.id).taskTypes(args.servers).tickFormat(format);
                $scope.gantt.timeDomainMode("fit");
            });

            $scope.tasks = [];

            $scope.addTask = function (item) {

                if ($scope.tasks.length === 0) {
                    $scope.tasks.push(item);
                    $scope.gantt($scope.tasks, element);
                    return;
                }

                $scope.tasks.push(item);

                var endDate = $scope.tasks[$scope.tasks.length - 1].fields.end;

                if (item.redraw) {
                    $scope.gantt.redraw($scope.tasks);
                }
            };

            $scope.$on("profiling", function (e, args) {
                if (args.new) {
                    $scope.addTask(args.new);
                } else if (args.clear) {
                    $scope.tasks.splice(0, $scope.tasks.length);
                    d3.select("svg").remove();
                }
            });
        }
    }
})