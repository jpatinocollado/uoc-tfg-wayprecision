let MapEventsManagerService = (function () {
    let register = function () {
        let map = MapManagerService.GetMap();

        //set events
        map.on('dblclick', onMapDblClick);
        map.on('zoomstart', onMapZoomStart);
        map.on('zoomend', onMapZoomEnd);
        map.on('move', onMapMove);
    }

    let onMapZoomStart = function (e) {
        let map = MapManagerService.GetMap();
        console.log('[onMapZoomStart]:', e);
        MapBackendManagerService.PostMessage('setLastZoom;' + map.getZoom());
    }

    let onMapZoomEnd = function (e) {
        let map = MapManagerService.GetMap();
        console.log('[onMapZoomEnd] Layer:', e);

        MapBackendManagerService.PostMessage('setZoom;' + map.getZoom());
    }

    let onMapMove = function (e) {
        let map = MapManagerService.GetMap();
        let latlng = map.getCenter();
        console.log('[onMapMove]:', latlng);
        MapBackendManagerService.PostMessage('setLastCenter;' + latlng.lat + ';' + latlng.lng);
    }

    let onMapDblClick = function (e) {
        MapBackendManagerService.PostMessage('createWaypoint;' + e.latlng.lat + ';' + e.latlng.lng);
    }

    //return plublic methods of service
    return {
        Register: register,
    };
})();