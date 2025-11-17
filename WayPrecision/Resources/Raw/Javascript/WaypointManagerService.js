var WaypointManagerService = (function () {
    var waypoints = L.markerClusterGroup({
        spiderfyOnMaxZoom: false,
        showCoverageOnHover: false,
        zoomToBoundsOnClick: true
    });
    var waypointsList = {};

    var getLayer = function () {
        return waypoints;
    }

    var clearWaypoints = function () {
        console.log("Clear Layer Waypoints");
        waypoints.clearLayers();
        waypointsList = {};
    }

    var addWaypoint = function (waypoint) {
        console.log("Add Waypoint in Layer", waypoint);

        var latLang = L.latLng(waypoint.lat, waypoint.lng);
        waypointsList[waypoint.id] = L.marker(latLang).on('click', function () {
            waypointOnClick(waypoint);
        }).bindTooltip(waypoint.name);

        if (waypoint.visible)
            waypointsList[waypoint.id].addTo(waypoints);

        console.log("layer waypoints", waypoints);
    }

    var waypointOnClick = function (waypoint) {
        MapBackendManagerService.PostMessage('editWaypoint;' + waypoint.id);
    }

    var fitWaypoint = function (id) {
        MapBackendManagerService.PostMessage("disableCenterLocation");
        MapManagerService.FitEstate();

        console.log("Fit Waypoint", id);
        var latlng = waypointsList[id].getLatLng();
        var map = MapManagerService.GetMap();
        map.setView(latlng, map.getMaxZoom());
    }

    var removeLayer = function () {
        waypoints.remove();
    }

    //return plublic methods of service
    return {
        //Waypoints
        GetLayer: getLayer,
        RemoveLayer: removeLayer,
        ClearWaypoints: clearWaypoints,
        AddWaypoint: addWaypoint,
        FitWaypoint: fitWaypoint,
        //Waypoints
    };
})();