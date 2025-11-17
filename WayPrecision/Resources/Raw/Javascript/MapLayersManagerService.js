let MapLayersManagerService = (function () {
    let baseMapsArr;
    let baseMaps;
    let overlayMaps;
    let layerControls = L.control.layers(baseMaps, overlayMaps, { collapsed: false });
    let IsModeEdicio = false;
    let currentLayer;
    let layerControlsAdicionals;
    let layerControlsWms;
    let layerControlsWmsProject;

    let chkWaypoints = true;
    let chkTracks = true;

    let baseLayerName = "OpenStreetMap";
    let baseLayerOpacity = 1;

    let addLayers = function (idLayer) {
        baseMaps = new Object();
        baseMapsArr = [];

        console.log("creando capa openstreetmaps");
        addOpenStreetMaps();

        console.log("creando controles de vista");
        createControlsView();
    }

    let addOpenStreetMaps = function () {
        let openStreetMaps = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '© OpenStreetMap'
        });

        baseMaps["OpenStreetMap"] = openStreetMaps;
        baseMapsArr.push(openStreetMaps);
    }

    let createControlsView = function () {
        let map = MapManagerService.GetMap();

        //Layer tracks
        let layerTracks = TrackManagerService.GetLayer();
        layerTracks.options.className = "OVLY#Tracks";

        //Layer waypoints
        let layerWaypoints = WaypointManagerService.GetLayer();
        layerWaypoints.options.className = "OVLY#Waypoints";

        //Layer actual position
        let layerActualPosition = MapGpsManagerService.GetLayer();

        overlayMaps = { 'Waypoints': layerWaypoints, 'Tracks': layerTracks, };

        layerControls.remove();
        layerControls = L.control.layers(baseMaps, overlayMaps, { collapsed: true });
        layerControls.addTo(map);

        if (chkTracks)
            layerTracks.addTo(map);
        else
            layerTracks.removeFrom(map);

        if (chkWaypoints)
            layerWaypoints.addTo(map);
        else
            layerWaypoints.removeFrom(map);

        layerActualPosition.addTo(map);

        if (baseMaps[baseLayerName]) {
            currentLayer = baseMaps[baseLayerName];
            map.addLayer(currentLayer);
        }
    }

    //return plublic methods of service
    return {
        AddLayers: addLayers,
        CreateControlsView: createControlsView,
    };
})();