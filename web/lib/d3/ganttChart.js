/**
 * @author Dimitry Kudrayvtsev
 * @version 2.0
 */

d3.gantt = function (element, elementId) {
    var FIT_TIME_DOMAIN_MODE = "fit";
    var FIXED_TIME_DOMAIN_MODE = "fixed";

    var margin = {
        top: 0,
        right: 0,
        bottom: 20,
        left: 100
    };
    var timeDomainStart = d3.time.day.offset(new Date(), -3);
    var timeDomainEnd = d3.time.hour.offset(new Date(), +3);
    var timeDomainMode = FIT_TIME_DOMAIN_MODE; // fixed or fit
    var taskTypes = [];
    var taskStatus = [];
    var width = (element[0].clientWidth - margin.right - margin.left - 5); //document.body.clientHeight - margin.top - margin.bottom-5;
    var height = (element[0].clientHeight - margin.top - margin.bottom - 5); //document.body.clientWidth - margin.right - margin.left-5;

    var tickFormat = "%H:%M";

    var keyFunction = function (d) {
        return d.fields.start + d.fields.name + d.fields.end;
    };

    var rectTransform = function (d) {
        if (d.fields.type === "package") {
            return "translate(" + x(d.fields.start) + "," + y(d.fields.name) + ")";
        } else {
            return "translate(" + x(d.fields.start) + "," + 0 + ")";
        }


    };

    var x = d3.time.scale().domain([timeDomainStart, timeDomainEnd]).range([0, width]).clamp(true);

    var y = d3.scale.ordinal().domain(taskTypes).rangeRoundBands([0, height - margin.top - margin.bottom], .1);

    var xAxis = d3.svg.axis().scale(x).orient("bottom").tickFormat(d3.time.format(tickFormat)).tickSubdivide(true)
        .tickSize(8).tickPadding(8);

    var yAxis = d3.svg.axis().scale(y).orient("left").tickSize(0);

    var initTimeDomain = function () {
        if (timeDomainMode === FIT_TIME_DOMAIN_MODE) {
            if (tasks === undefined || tasks.length < 1) {
                timeDomainStart = d3.time.day.offset(new Date(), -3);
                timeDomainEnd = d3.time.hour.offset(new Date(), +3);
                return;
            }
            tasks.sort(function (a, b) {
                return a.fields.end - b.fields.end;
            });
            timeDomainEnd = tasks[tasks.length - 1].fields.end;
            tasks.sort(function (a, b) {
                return a.fields.start - b.fields.start;
            });
            timeDomainStart = tasks[0].fields.start;
        }
    };

    var initAxis = function () {
        x = d3.time.scale().domain([timeDomainStart, timeDomainEnd]).range([0, width]).clamp(true);
        y = d3.scale.ordinal().domain(taskTypes).rangeRoundBands([0, height - margin.top - margin.bottom], .1);
        xAxis = d3.svg.axis().scale(x).orient("bottom").tickFormat(d3.time.format(tickFormat)).tickSubdivide(true)
            .tickSize(8).tickPadding(8);

        yAxis = d3.svg.axis().scale(y).orient("left").tickSize(0);
    };

    function gantt(tasks, element) {

        initTimeDomain();
        initAxis();



        var svg = d3.select("#" + elementId)
            .append("svg")
            .attr("class", "chart")
            .attr("width", width + margin.left + margin.right)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("class", "gantt-chart")
            .attr("width", width + margin.left + margin.right)
            .attr("height", height + margin.top + margin.bottom)
            .attr("transform", "translate(" + margin.left + ", " + margin.top + ")");

        svg.selectAll(".chart")
            .data(tasks, keyFunction).enter()
            .append("rect")
            .attr("rx", 1)
            .attr("ry", 1)
            .attr("class", function (d) {
            if (taskStatus[d.fields.status] == null) {
                return "bar";
            }
            return taskStatus[d.fields.status];
        })
            .attr("y", 0)
            .attr("transform", rectTransform)
            .attr("height", function (d) {
            return y.rangeBand();
        })
            .attr("width", function (d) {
            //         return d3.scale.linear()(d.fields.end) - d3.scale.linear()(d.fields.start);
            return (x(d.fields.end) - x(d.fields.start));
        })

        ;


        svg.append("g")
            .attr("class", "x axis")
            .attr("transform", "translate(0, " + (height - margin.top - margin.bottom) + ")")
            .transition()
            .call(xAxis);

        svg.append("g").attr("class", "y axis").transition().call(yAxis);

        return gantt;

    };

    gantt.redraw = function (tasks) {

        initTimeDomain();
        initAxis();

        var colors = []
        var colors = {};

        var color = d3.rgb(91, 115, 195);
        
        var tooltip = d3.select("body")
            .append("div")
            .style("position", "absolute")
            .style("z-index", "10")
            .style("visibility", "hidden");

        var svg = d3.select("svg");

        var ganttChartGroup = svg.select(".gantt-chart");
        var rect = ganttChartGroup.selectAll("rect").data(tasks, keyFunction);

        rect.enter()
            .insert("rect", ":first-child")
            .attr("rx", 1)
            .attr("ry", 1)
            .attr("class", function (d) {
            if (taskStatus[d.fields.status] == null) {
                return "bar";
            } else {
                return taskStatus[d.fields.status];
            }
        })
            .style("fill", function(d){
                if(d.fields.stage) {
                    color = color.darker(1);
                    return color;
                }
            })
            .transition()
            .attr("y", 0)
            .attr("transform", rectTransform)
            .attr("height", function (d) {
            return y.rangeBand();
        })
            .attr("width", function (d) {
            return (x(d.fields.end) - x(d.fields.start));
        });

        rect.transition()
            .attr("transform", rectTransform)
            .attr("height", function (d) {
            if (d.fields.stage) {
                return y.rangeExtent()[1]
            } else {
                return y.rangeBand();
            }
        })
            .attr("opacity", function (d) {
            if (d.fields.stage) {
                return 0.1;
            } else {
                return 1;
            }
        })
            .attr("width", function (d) {
            return (x(d.fields.end) - x(d.fields.start));
        });

        rect.on("mouseover", function (d) {
            tooltip.text(d.fields.typeName + " for " + d.fields.packageName);
            setTimeout(function(){
                tooltip.style("visibility", "hidden");
            }, 3000);
            return tooltip.style("visibility", "visible");
        })
        .on("mousemove", function () {
            return tooltip.style("top", (event.pageY - 10) + "px").style("left", (event.pageX + 10) + "px");
        })
        .on("mouseout", function (d) {
            return tooltip.style("visibility", "hidden");
        });

        rect.exit().remove();

        svg.select(".x").transition().call(xAxis);
        svg.select(".y").transition().call(yAxis);

        return gantt;
    };

    gantt.margin = function (value) {
        if (!arguments.length)
            return margin;
        margin = value;
        return gantt;
    };

    gantt.timeDomain = function (value) {
        if (!arguments.length)
            return [timeDomainStart, timeDomainEnd];
        timeDomainStart = +value[0], timeDomainEnd = +value[1];
        return gantt;
    };

    /**
     * @param {string}
     *                vale The value can be "fit" - the domain fits the data or
     *                "fixed" - fixed domain.
     */
    gantt.timeDomainMode = function (value) {
        if (!arguments.length)
            return timeDomainMode;
        timeDomainMode = value;
        return gantt;

    };

    gantt.taskTypes = function (value) {
        if (!arguments.length)
            return taskTypes;
        taskTypes = value;
        return gantt;
    };

    gantt.taskStatus = function (value) {
        if (!arguments.length)
            return taskStatus;
        taskStatus = value;
        return gantt;
    };

    gantt.width = function (value) {
        if (!arguments.length)
            return width;
        width = +value;
        return gantt;
    };

    gantt.height = function (value) {
        if (!arguments.length)
            return height;
        height = +value;
        return gantt;
    };

    gantt.tickFormat = function (value) {
        if (!arguments.length)
            return tickFormat;
        tickFormat = value;
        return gantt;
    };



    return gantt;
};