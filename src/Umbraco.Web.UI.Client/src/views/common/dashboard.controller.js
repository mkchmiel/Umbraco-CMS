/**
 * @ngdoc controller
 * @name Umbraco.DashboardController
 * @function
 * 
 * @description
 * Controls the dashboards of the application
 * 
 */
 
function DashboardController($scope, $routeParams, $location, dashboardResource, localizationService) {

    const DASHBOARD_QUERY_PARAM = 'dashboard';

    $scope.page = {};
    $scope.page.nameLocked = true;
    $scope.page.loading = true;

    $scope.dashboard = {};
    localizationService.localize("sections_" + $routeParams.section).then(function(name){
    	$scope.dashboard.name = name;
    });
    
    dashboardResource.getDashboard($routeParams.section).then(function(tabs){
        $scope.dashboard.tabs = tabs;

        if ($scope.dashboard.tabs && $scope.dashboard.tabs.length > 0) {
            initActiveTab();
        }

        $scope.page.loading = false;
    });

    $scope.changeTab = function(tab) {
        if ($scope.dashboard.tabs && $scope.dashboard.tabs.length > 0) {
            $scope.dashboard.tabs.forEach(function(tab) {
                tab.active = false;
            });
        }

        tab.active = true;

        $location.search(DASHBOARD_QUERY_PARAM, tab.alias);
    };

    function initActiveTab () {
        // check the query param for a dashboard alias
        const dashboardAlias = $location.search()[DASHBOARD_QUERY_PARAM];
        const dashboardIndex = $scope.dashboard.tabs.findIndex(tab => tab.alias === dashboardAlias);
        // set the first dashboard to active if there is no query parameter or we can't find a matching dashboard for the alias
        const activeIndex = dashboardIndex !== -1 ? dashboardIndex : 0;

        const tab = $scope.dashboard.tabs[activeIndex];
        
        tab.active = true;
        $location.search(DASHBOARD_QUERY_PARAM, tab.alias);
    }

}


//register it
angular.module('umbraco').controller("Umbraco.DashboardController", DashboardController);
