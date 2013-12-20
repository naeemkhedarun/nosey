'use strict'

angular.module('myApp.directives').directive("groupedBar", function () {
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
                   
            $scope.$on("replicationStatus", function(e, args){
                
                var data = args;
                
                var margin = {
                    top: 0,
                    right: 0,
                    bottom: 20,
                    left: 30
                };
//                 width = 960 - margin.left - margin.right,
//                 height = 500 - margin.top - margin.bottom;
            
             var width = (element[0].clientWidth - margin.right - margin.left - 5); //document.body.clientHeight - margin.top - margin.bottom-5;
             var height = 150;//(element[0].clientHeight - margin.top - margin.bottom - 5); //document.body.clientWidth - margin.right - margin.left-5;
    

                var x0 = d3.scale.ordinal()
                    .rangeRoundBands([0, width], .1);
                
                var x1 = d3.scale.ordinal();
                
                var y = d3.scale.linear()
                    .range([height, 0]);
                
                var color = d3.scale.ordinal()
                    .range(["#98abc5", "#8a89a6", "#7b6888", "#6b486b", "#a05d56", "#d0743c", "#ff8c00"]);
                
                var xAxis = d3.svg.axis()
                    .scale(x0)
                    .orient("bottom");
                
                var yAxis = d3.svg.axis()
                    .scale(y)
                    .orient("left")
                    .tickFormat(d3.format(".2s"));
                
                var svg = d3.select("#"+element[0].id).append("svg")
                    .attr("width", width + margin.left + margin.right)
                    .attr("height", height + margin.top + margin.bottom)
                  .append("g")
                    .attr("transform", "translate(" + margin.left + "," + margin.top + ")");
                
                  var legendItems = data.map(function(e){
                      return e.yAxis.map(function(f){
                          return f.name
                      })
                  });
                
                  legendItems = d3.set(legendItems).values();
                
                  x0.domain(data.map(function(d) { return d.xAxis; }));
                  x1.domain(legendItems).rangeRoundBands([0, x0.rangeBand()]);
                  y.domain([0, d3.max(data, function(d) { return d3.max(d.yAxis, function(d) { return d.value; }); })]);
                
                  svg.append("g")
                      .attr("class", "x axis")
                      .attr("transform", "translate(0," + height + ")")
                      .call(xAxis)
                      .selectAll("text")  
                        .style("display", "none");
                
                  svg.append("g")
                      .attr("class", "y axis")
                      .call(yAxis)
                    .append("text")
                      .attr("transform", "rotate(-90)")
                      .attr("y", 6)
                      .attr("dy", ".71em")
                      .style("text-anchor", "end")
                      .text("Errors");
                
                  var state = svg.selectAll(".state")
                      .data(data)
                    .enter().append("g")
                      .attr("class", "g")
                      .attr("transform", function(d) { 
                        return "translate(" + x0(d.xAxis) + ",0)"; 
                      });
                
                  state.selectAll("rect")
                      .data(function(d) { return d.yAxis; })
                    .enter().append("rect")
                      .attr("width", x1.rangeBand())
                      .attr("x", function(d) { return x1(d.name); })
                      .attr("y", function(d) { return y(d.value); })
                      .attr("height", function(d) { 
                        return height - y(d.value); })
                      .style("fill", function(d) { return color(d.name); })
                      .on("click", function(d){
                        $scope.$parent.$broadcast("filterServer", d);
                      });
                
                  var legend = svg.selectAll(".legend")
                      .data(legendItems.slice())
                    .enter().append("g")
                      .attr("class", "legend")
                      .attr("transform", function(d, i) { return "translate(0," + i * 20 + ")"; });
                
                  legend.append("rect")
                      .attr("x", width - 18)
                      .attr("width", 18)
                      .attr("height", 18)
                      .style("fill", color);
                
                  legend.append("text")
                      .attr("x", width - 24)
                      .attr("y", 9)
                      .attr("dy", ".35em")
                      .style("text-anchor", "end")
                      .text(function(d) { return d; });
                
              
            });
            
        } 
    }
});