var MapEventsManagerService = (function () {
    var register = function () {
        var map = MapManagerService.GetMap();

        //set events
        map.on('dblclick', onMapDblClick);
        map.on('zoomstart', onMapZoomStart);
        map.on('zoomend', onMapZoomEnd);
        map.on('move', onMapMove);
    }

    var onMapZoomStart = function (e) {
        var map = MapManagerService.GetMap();
        console.log('[onMapZoomStart]:', e);
        MapBackendManagerService.PostMessage('setLastZoom;' + map.getZoom());
    }

    var onMapZoomEnd = function (e) {
        let map = MapManagerService.GetMap();
        console.log('[onMapZoomEnd] Layer:', e);

        MapBackendManagerService.PostMessage('setZoom;' + map.getZoom());
    }

    var onMapMove = function (e) {
        var map = MapManagerService.GetMap();
        var latlng = map.getCenter();
        console.log('[onMapMove]:', latlng);
        MapBackendManagerService.PostMessage('setLastCenter;' + latlng.lat + ';' + latlng.lng);
    }

    var onMapDblClick = function (e) {
        MapBackendManagerService.PostMessage('createWaypoint;' + e.latlng.lat + ';' + e.latlng.lng);
    }

    //return plublic methods of service
    return {
        Register: register,
    };
})();