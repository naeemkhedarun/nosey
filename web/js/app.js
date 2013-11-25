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
  $routeProvider.when('/view1', {templateUrl: 'views/gantt/gantt.html', controller: 'MyCtrl1'});
  $routeProvider.when('/view2', {templateUrl: 'partials/partial2.html', controller: 'MyCtrl2'});
  $routeProvider.otherwise({redirectTo: '/view1'});
}]);

angular.module('myApp.controllers', [])
    .controller('MyCtrl2',
        function () {
            
        })
    ;

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
