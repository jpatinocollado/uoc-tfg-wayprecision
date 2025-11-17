let TrackManagerService = (function () {
    //Layer group for all tracks
    let tracks = L.layerGroup();
    let trackList = {};

    let getLayer = function () {
        return tracks;
    }

    const setStyleTrack = (weight, color, fillColor, opacity, fillopacity) => {
        return {
            "opacity": opacity,     //1
            "weight": weight,
            "fillColor": fillColor,
            "color": color,
            "fillOpacity": fillopacity  //0.5
        }
    }

    let addTrack = function (track) {
        console.log('Add Track', track);

        let type;
        let lineLength;
        let polygonArea;
        let polygonLength;
        let alt = '<bold>' + track.name + '</bold>';
        if (track.type == "LineString") {
            type = track.lineString;
            lineLength = turf.length(track.lineString, { units: 'meters' });

            if (track.length)
                alt += '<br /> Longitud: ' + track.length;
        } else {
            type = track.polygon;
            polygonArea = turf.area(track.polygon);
            polygonLength = turf.length(track.polygon, { units: 'meters' });

            if (track.area)
                alt += '<br /> Área: ' + track.area;
            if (track.length)
                alt += '<br /> Perímetro: ' + track.length;
        }

        trackList[track.id] = L.geoJSON(type, {
            style: setStyleTrack(track.weight, track.color, track.fillColor, track.opacity, track.fillopacity),
            onEachFeature: function onEachFeature(feature, layer) {
                layer.on('dblclick', function (e) {
                    e.originalEvent.preventDefault();
                    MapBackendManagerService.PostMessage('editTrack;' + track.id);
                });
            }
        }).bindTooltip(alt);

        if (track.visible)
            trackList[track.id].addTo(tracks);

        MapBackendManagerService.PostMessage('updateTrack;' + track.id + ';' + lineLength + ';' + polygonArea + ';' + polygonLength);
    }

    let clearTracks = function () {
        tracks.clearLayers();
        trackList = {};
    }

    let fitTrack = function (id) {
        MapBackendManagerService.PostMessage("disableCenterLocation");
        MapManagerService.FitEstate();

        console.log('Fit Track', id);

        let bounds = trackList[id].getBounds();
        let map = MapManagerService.GetMap();
        map.setZoom(map.getMaxZoom());
        map.fitBounds(bounds);

        console.log('bounds', bounds);
    }

    let removeLayer = function () {
        tracks.remove();
    }

    //return plublic methods of service
    return {
        GetLayer: getLayer,
        RemoveLayer: removeLayer,
        ClearTracks: clearTracks,
        AddTrack: addTrack,
        FitTrack: fitTrack
    };
})();