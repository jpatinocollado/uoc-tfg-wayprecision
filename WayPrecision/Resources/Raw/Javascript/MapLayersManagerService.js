var MapLayersManagerService = (function () {
    var baseMapsArr;
    var baseMaps;
    var overlayMaps;
    var layerControls = L.control.layers(baseMaps, overlayMaps, { collapsed: false });
    var IsModeEdicio = false;
    var currentLayer;
    var layerControlsAdicionals;
    var layerControlsWms;
    var layerControlsWmsProject;

    var chkWaypoints = true;
    var chkTracks = true;

    var baseLayerName = "OpenStreetMap";
    var baseLayerOpacity = 1;

    var addLayers = function (idLayer) {
        baseMaps = new Object();
        baseMapsArr = [];

        console.log("creando capa openstreetmaps");
        addOpenStreetMaps();

        console.log("creando controles de vista");
        createControlsView();
    }

    var addOpenStreetMaps = function () {
        var openStreetMaps = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '© OpenStreetMap'
        });

        baseMaps["OpenStreetMap"] = openStreetMaps;
        baseMapsArr.push(openStreetMaps);
    }

    var createControlsView = function () {
        var map = MapManagerService.GetMap();

        //Layer tracks
        var layerTracks = TrackManagerService.GetLayer();
        layerTracks.options.className = "OVLY#Tracks";

        //Layer waypoints
        var layerWaypoints = WaypointManagerService.GetLayer();
        layerWaypoints.options.className = "OVLY#Waypoints";

        //Layer actual position
        var layerActualPosition = MapGpsManagerService.GetLayer();

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

        var map = MapManagerService.GetMap();

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