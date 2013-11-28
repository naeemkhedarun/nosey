'use strict'

angular.module('myApp.directives').directive("gantt", function () {
    return {
        restrict: 'A',

        // set up the isolate scope so that we don't clobber parent scope
        scope: {
            onClick: '=',
            width: '=',
            height: '=',
            bind: '=',
            duration: '@'
        },
        
        link: function ($scope, element, attrs) {

//            var margin = {
//                top: 10,
//                right: 10,
//                bottom: 10,
//                left: 10
//            };
//
//            var width = $scope.width || 1020;
//            var height = $scope.height || 300;
//
//            // add margin
//            width = width - margin.left - margin.right;
//            height = height - margin.top - margin.bottom;
//
//            var klass = attrs.class || '';
//            var align = attrs.align || 'left';
//
//            var viewAlign = align === 'right' ? 'xMaxYMin' : 'xMinYMin';
//
//            var dateParseFormat = d3.time.format("%d/%m/%Y %X");
//
//            var tasks = data;
//
            var taskStatus = {
                "feature" : "bar",
                "extension" : "bar-running"
            };
//            
//            tasks.sort(function(a, b) {
//                return a.end - b.end;
//            });
//            var maxDate = tasks[tasks.length - 1].end;
//            tasks.sort(function(a, b) {
//                return a.start - b.start;
//            });
//            var minDate = tasks[0].start;
//            
            var format = "%H:%M";
//            var timeDomainString = "1day";
            
              
            $scope.$on("detail", function(e, args){
                $scope.gantt = d3.gantt(element, attrs.id).taskTypes(args.servers).taskStatus(taskStatus).tickFormat(format);
                $scope.gantt.timeDomainMode("fixed");
            });
            
            $scope.tasks = [];
            
            $scope.addTask = function(item) {
                
                if($scope.tasks.length === 0){
                    $scope.tasks.push(item);
                    $scope.gantt($scope.tasks, element);
                    return;
                }
                
                $scope.tasks.push(item);
                
                var endDate = $scope.tasks[$scope.tasks.length - 1].fields.end;
  
                $scope.gantt.timeDomain([ 
                    $scope.tasks[0].fields.start, 
                    $scope.tasks[$scope.tasks.length - 1].fields.end
                ]);
                
//                    var taskStatusKeys = Object.keys(taskStatus);
//                    var taskStatusName = taskStatusKeys[Math.floor(Math.random() * taskStatusKeys.length)];
//                    var taskName = taskNames[Math.floor(Math.random() * taskNames.length)];
                
//                    tasks.push({
//                        "start" : ,
//                        "end" : d3.time.hour.offset(lastEndDate, (Math.ceil(Math.random() * 3)) + 1),
//                        "taskName" : taskName,
//                        "status" : taskStatusName
//                    });
                
//                    changeTimeDomain(timeDomainString);
                    $scope.gantt.redraw($scope.tasks);
                };
            
            
            $scope.$on("profiling", function(e, args){
                if(args.new) {
                    $scope.addTask(args.new);    
                } else if (args.clear){
                    $scope.tasks.splice(0, $scope.tasks.length);
                    d3.select("svg").remove();
                }
            })
          
            
            
//            scope.$parent.$watch('results', function (data) {
//                console.log(data);
//
//                if (data.length > 0) {
//
//                var tasks = data;
//
//                var taskStatus = {
//                    "feature" : "bar",
//                    "extension" : "bar-running"
//                };
//                
//                var taskNames = [ "asnav-sql-08-01", 
//                                 "asnav-sql-08-02", 
//                                 "asnav-web-08a", 
//                                 "asnav-web-08b", 
//                                 "asnav-app-08a", 
//                                 "asnav-app-08b", 
//                                 "asnav-app-08c",
//                                "asnav-boweb-08"];
//                
//                tasks.sort(function(a, b) {
//                    return a.end - b.end;
//                });
//                var maxDate = tasks[tasks.length - 1].end;
//                tasks.sort(function(a, b) {
//                    return a.start - b.start;
//                });
//                var minDate = tasks[0].start;
//                
//                var format = "%H:%M";
//                var timeDomainString = "1day";
//                
//                var gantt = d3.gantt().taskTypes(taskNames).taskStatus(taskStatus).tickFormat(format).height(450).width(800);
//                
//                
//                gantt.timeDomainMode("fixed");
//                    
//                    
//                format = "%H:%M";
//                var endDate = tasks[tasks.length - 1].fields.end;
//                
//                gantt.timeDomain([ 
//                    tasks[0].fields.start, 
//                    tasks[tasks.length - 1].fields.end
//                ]);
//                
//                    gantt.tickFormat(format);
//                
//                    
//                gantt(tasks);
////                    gantt.redraw(tasks);
////                    gantt.redraw(tasks);
////                tasks.forEach(function(t){
////                    t.fields.startDate = d3.time.hour.offset(endDate, Math.ceil(1 * Math.random()))
////                    t.fields.endEnd = d3.time.hour.offset(endDate, (Math.ceil(Math.random() * 3)) + 1)
////                })
//                
////                function addTask() {
////                
////                    var lastEndDate = getEndDate();
////                    var taskStatusKeys = Object.keys(taskStatus);
////                    var taskStatusName = taskStatusKeys[Math.floor(Math.random() * taskStatusKeys.length)];
////                    var taskName = taskNames[Math.floor(Math.random() * taskNames.length)];
////                
////                    tasks.push({
////                    "start" : d3.time.hour.offset(lastEndDate, Math.ceil(1 * Math.random())),
////                    "end" : d3.time.hour.offset(lastEndDate, (Math.ceil(Math.random() * 3)) + 1),
////                    "taskName" : taskName,
////                    "status" : taskStatusName
////                    });
////                
////                    changeTimeDomain(timeDomainString);
////                    gantt.redraw(tasks);
////                };
////                
////                function removeTask() {
////                    tasks.pop();
////                    changeTimeDomain(timeDomainString);
////                    gantt.redraw(tasks);
////                };
//
//                } else if(gantt){
//                    gantt.redraw(tasks);
//                }
//            }, true)
        }
    }

})