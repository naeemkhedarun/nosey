'use strict';

/* Directives */


angular.module('myApp.directives', []).
directive('appVersion', ['version',
    function (version) {
        return function (scope, elm, attrs) {
            elm.text(version);
        };
  }]).
directive("graph", function () {
        return {
            restrict: 'E',

            // set up the isolate scope so that we don't clobber parent scope
            scope: {
                onClick: '=',
                width: '=',
                height: '=',
                bind: '=',
                duration: '@'
            },
            link: function (scope, element, attrs) {

                var margin = {
                    top: 10,
                    right: 10,
                    bottom: 10,
                    left: 10
                };

                var width = scope.width || 300;
                var height = scope.height || 1020;

                // add margin
                width = width - margin.left - margin.right;
                height = height - margin.top - margin.bottom;

                var klass = attrs.class || '';
                var align = attrs.align || 'left';

                var viewAlign = align === 'right' ? 'xMaxYMin' : 'xMinYMin';

                var dateParseFormat = d3.time.format("%d/%m/%Y %X");

                scope.$watch('bind', function (data) {
                    if(data){
                        
                        var requests, runDimension, runGroup, all, duration, statusCode, second, seconds, time, deltaTimeGroup, maximumGroup, selectorChart;
                        var startDate = dateParseFormat.parse(data[0].fields.Date);

                        requests = crossfilter(data),
                        all = requests.groupAll(),
                        duration = requests.dimension(function (d) {
                            return d.fields.Duration;
                        }),
                        statusCode = requests.dimension(function (d) {
                            return d.fields.StatusCode;
                        }),
                        second = requests.dimension(function (d) {
                            return Math.round((dateParseFormat.parse(d.fields.Date) - startDate) / 1000 / 60);
                        }),
                        seconds = second.group(Math.floor),
                        time = requests.dimension(function (d) {
                            return (dateParseFormat.parse(d.fields.Date) - startDate);
                        }).group();

                        var statusCodeGroup = statusCode.group();

                        deltaTimeGroup = second.group().reduce(
                            function (p, v) {
                                ++p.Count;
                                p.TotalDuration += v.fields.Duration
                                p.AverageDuration = p.TotalDuration / p.Count;
                                p.MinimumDuration = Math.min(p.MinimumDuration, v.fields.Duration)
                                p.MaximumDuration = Math.max(p.MaximumDuration, v.fields.Duration)
                                return p;
                            }, function (p, v) {
                                --p.Count;
                                p.TotalDuration -= v.fields.Duration
                                p.AverageDuration = p.TotalDuration / p.Count;
                                p.MinimumDuration = Math.min(p.MinimumDuration, v.fields.Duration)
                                p.MaximumDuration = Math.max(p.MaximumDuration, v.fields.Duration)
                                return p;
                            }, function () {
                                return {
                                    TotalDuration: 0,
                                    Count: 0,
                                    AverageDuration: 0,
                                    MaximumDuration: 0,
                                    MinimumDuration: 999999
                                };
                            }
                        );

                        maximumGroup = second.group().reduce(
                            function (p, v) {
                                p.MaximumDuration = Math.max(p.MaximumDuration, v.fields.Duration)
                                return p;
                            }, function (p, v) {
                                p.MaximumDuration = Math.max(p.MaximumDuration, v.fields.Duration)
                                return p;
                            }, function () {
                                return {
                                    MaximumDuration: 0
                                };
                            }
                        );

                        selectorChart.width(1280)
                            .height(160)
                            .margins({
                                top: 0,
                                right: 50,
                                bottom: 20,
                                left: 40
                            })
                            .dimension(all)
                            .group(deltaTimeGroup)
                            .x(d3.scale.linear().rangeRound([0, 60]))
                            .centerBar(true)
                            .gap(1)
                            .elasticX(true)
                            .keyAccessor(function (d) {
                                return d.key;
                            })
                            .valueAccessor(function (d) {
                                return d.value.Count;
                            })
                            .yAxis().ticks(5).tickFormat(function (v) {
                                return (v / 1000) + "k";
                            });


                        lineChart
                            .width(1280)
                            .height(600)
                            .margins({
                                top: 0,
                                right: 50,
                                bottom: 20,
                                left: 100
                            })
                            .dimension(all)
                            .group(deltaTimeGroup)
                            .renderArea(true)
                            .transitionDuration(1000)
                            .rangeChart(selectorChart)
                            .elasticY(true)
                            .mouseZoomable(true)
                            .brushOn(false)
                            .x(d3.time.scale().domain([0, 60]))
                            .keyAccessor(function (d) {
                                return d.key;
                            })
                            .valueAccessor(function (e) {
                                return e.value.AverageDuration;
                            })
                            .yAxis().tickFormat(function (v) {
                                return (v) + "ms";
                            });

                        statusCodeChart.width(180)
                            .height(180)
                            .radius(80)
                            .innerRadius(30)
                            .dimension(statusCode)
                            .group(statusCode.group().reduceCount(function (d) {
                                return Math.round((dateParseFormat.parse(d.fields.Date) - startDate) / 1000 / 60);
                            }));
                    }
                }
                )
            }
        }

    } 
)