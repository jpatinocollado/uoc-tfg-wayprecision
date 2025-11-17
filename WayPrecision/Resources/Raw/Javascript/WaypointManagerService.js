let WaypointManagerService = (function () {
    let waypoints = L.markerClusterGroup({
        spiderfyOnMaxZoom: false,
        showCoverageOnHover: false,
        zoomToBoundsOnClick: true
    });
    let waypointsList = {};

    let getLayer = function () {
        return waypoints;
    }

    let clearWaypoints = function () {
        console.log("Clear Layer Waypoints");
        waypoints.clearLayers();
        waypointsList = {};
    }

    let addWaypoint = function (waypoint) {
        console.log("Add Waypoint in Layer", waypoint);

        let latLang = L.latLng(waypoint.lat, waypoint.lng);
        waypointsList[waypoint.id] = L.marker(latLang).on('click', function () {
            waypointOnClick(waypoint);
        }).bindTooltip(waypoint.name);

        if (waypoint.visible)
            waypointsList[waypoint.id].addTo(waypoints);

        console.log("layer waypoints", waypoints);
    }

    let waypointOnClick = function (waypoint) {
        MapBackendManagerService.PostMessage('editWaypoint;' + waypoint.id);
    }

    let fitWaypoint = function (id) {
        MapBackendManagerService.PostMessage("disableCenterLocation");
        MapManagerService.FitEstate();

        console.log("Fit Waypoint", id);
        let latlng = waypointsList[id].getLatLng();
        let map = MapManagerService.GetMap();
        map.setView(latlng, map.getMaxZoom());
    }

    let removeLayer = function () {
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