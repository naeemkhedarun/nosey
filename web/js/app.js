'use strict';


// Declare app level module which depends on filters, and services
angular.module('myApp', [
  'ngRoute',
  'myApp.filters',
  'myApp.services',
  'myApp.directives',
  'myApp.controllers',
  'elasticjs.service'
]).
config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/deployment', {templateUrl: 'views/gantt/gantt.html', controller: 'GanttController'});
  $routeProvider.when('/environments', {templateUrl: 'views/environment/environment.html', controller: 'EnvironmentController'});
  $routeProvider.when('/cdds', {templateUrl: 'views/cdds/cdds.html', controller: 'CddsController'});
  $routeProvider.otherwise({redirectTo: '/deployment'});
}]);

angular.module('myApp.controllers', []);

angular.module('myApp.directives', []);

angular.module('myApp.filters', []).
  filter('interpolate', ['version', function(version) {
    return function(text) {
      return String(text).replace(/\%VERSION\%/mg, version);
    }
  }]).filter('reverse', function() {
  return function(items) {
    return items.slice().reverse();
  };
});;


angular.module('myApp.services', []).
value('version', '0.1');
