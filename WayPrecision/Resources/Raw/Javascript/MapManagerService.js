let MapManagerService = (function () {
    let LeafletMap;
    let IsDevelop;
    let IdProyecto;
    let buttonNavigation;
    let IsEnableLocation = false;
    let IsEnableCenterLocation = false;

    let getIsDevelop = function () {
        return IsDevelop;
    }

    let getMap = function () {
        return LeafletMap;
    }

    let getIdProyecto = function () {
        return IdProyecto;
    }

    let iniciarPantalla = function (idLastLayer, isDevelop, idProyecto) {
        IsDevelop = isDevelop;
        IdProyecto = idProyecto;
        //instance the map
        LeafletMap = L.map('map',
            {
                zoomControl: true,
                doubleClickZoom: false,
                boxZoom: false,
                maxZoom: 19,
            });

        //set events
        MapEventsManagerService.Register();

        //center map
        LeafletMap.fitWorld();

        //add layers
        console.log("añadiendo capas");
        MapLayersManagerService.AddLayers(idLastLayer);

        //Custom Buttons to edit
        console.log("añadiendo botones laterales");

        //boton de navegación
        buttonNavigation = L.easyButton({
            states: [{
                stateName: 'location-disable',
                icon: '<img width="24" src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAGHaVRYdFhNTDpjb20uYWRvYmUueG1wAAAAAAA8P3hwYWNrZXQgYmVnaW49J++7vycgaWQ9J1c1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCc/Pg0KPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyI+PHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj48cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0idXVpZDpmYWY1YmRkNS1iYTNkLTExZGEtYWQzMS1kMzNkNzUxODJmMWIiIHhtbG5zOnRpZmY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIj48dGlmZjpPcmllbnRhdGlvbj4xPC90aWZmOk9yaWVudGF0aW9uPjwvcmRmOkRlc2NyaXB0aW9uPjwvcmRmOlJERj48L3g6eG1wbWV0YT4NCjw/eHBhY2tldCBlbmQ9J3cnPz4slJgLAAAA+0lEQVRYR72UwRKEMAhD6/7/P7sXcbJIgLR134wXhCRw6BhrnNc3zccX/s3hCwnqpi3tTpNq7Ek90p8bzA3qQ39sNDdCr7AomNu82s8LDbFoBpHmvVg27HsrWlrdd0A1H90ZDMASt4QA1GGzd0/3AhX2JJswW+ZBFYBtYKApgnOpRhUgwm+7hBJgmylyJKLqI2Owk4c6ygVeAdNGCdk2Q+ynvSsXYGYSKwG2UAWITsdgF0k1qgCvgwGmNrhQZ+/+7gWYUEZrJkpeDUYziDTPxCoRQ30tH36PAtAV7RJ6hUVgVwjqQ38AqyFSj/SnQw2iaE9zTgT7ofsOvMYXVUIwIVOMQ/UAAAAASUVORK5CYII=" />',
                title: 'Habilitar localización',
                onClick: function (btn, map) {
                    LoadingManagerService.Show('Activando localización...');

                    MapBackendManagerService.PostMessage("enableLocation");
                    MapBackendManagerService.PostMessage("enableCenterLocation");

                    LoadingManagerService.Hide();
                    btn.state('location-enable');

                    IsEnableLocation = true;
                    IsEnableCenterLocation = true;
                }
            }, {
                stateName: 'location-enable', // name the state
                icon: '<img width="24" src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAGHaVRYdFhNTDpjb20uYWRvYmUueG1wAAAAAAA8P3hwYWNrZXQgYmVnaW49J++7vycgaWQ9J1c1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCc/Pg0KPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyI+PHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj48cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0idXVpZDpmYWY1YmRkNS1iYTNkLTExZGEtYWQzMS1kMzNkNzUxODJmMWIiIHhtbG5zOnRpZmY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIj48dGlmZjpPcmllbnRhdGlvbj4xPC90aWZmOk9yaWVudGF0aW9uPjwvcmRmOkRlc2NyaXB0aW9uPjwvcmRmOlJERj48L3g6eG1wbWV0YT4NCjw/eHBhY2tldCBlbmQ9J3cnPz4slJgLAAABCklEQVRYR72WwRWEMAhEcQvYErc4S7QB94RiZGBIov+IMDNwyFNkhHXbZd32tlzh0xbeZmkLkOqmvy+lnTdVjVuSIOHHYXMlCAE/TDNXQAi3SJuraLXfcCukYo7IheL8VSwazoxbSC3uHaiaCz9zBkCJSaEDq4NmTQ93gQx9klUYLeMQB0AbKNbUYucSjTiAR7vtIHyAiaaWBYpWHxkFnRzo8Bd4iDOtlxBtI8X+oLf/AsisSH+AScQBvNMh0EUSjTjAC5wBOjcQ6Zg1/dwFkFAEOXNPng2ibZXivC+WiSjV19IJfyscsKIsjrmEAWRiCGAuaQCZECIwFyqAUg2SGM9hHf9H4N6BB/kDiZmKpZ8LOc0AAAAASUVORK5CYII=" />',
                title: 'Seguimiento sin centrar', // like its title
                onClick: function (btn, map) {       // and its callback
                    LoadingManagerService.Show('Desactivando centrar en el mapa...');

                    MapBackendManagerService.PostMessage("disableCenterLocation");

                    LoadingManagerService.Hide();
                    btn.state('location-enable-move');

                    IsEnableLocation = true;
                    IsEnableCenterLocation = true;
                }
            }, {
                stateName: 'location-enable-move', // name the state
                icon: '<img width="24" src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAGHaVRYdFhNTDpjb20uYWRvYmUueG1wAAAAAAA8P3hwYWNrZXQgYmVnaW49J++7vycgaWQ9J1c1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCc/Pg0KPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyI+PHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj48cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0idXVpZDpmYWY1YmRkNS1iYTNkLTExZGEtYWQzMS1kMzNkNzUxODJmMWIiIHhtbG5zOnRpZmY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIj48dGlmZjpPcmllbnRhdGlvbj4xPC90aWZmOk9yaWVudGF0aW9uPjwvcmRmOkRlc2NyaXB0aW9uPjwvcmRmOlJERj48L3g6eG1wbWV0YT4NCjw/eHBhY2tldCBlbmQ9J3cnPz4slJgLAAABDklEQVRYR72WPRaDQAiEMVdIa+n9T5QyrWcwFfsIMvys6NdlhZmBgheiC+zrduzrduj3Ci/98DSLfkBUJ31/PyntsKhqrImCuB+vmjNeCPihy5xBIczHrDmLVuslp4dIzBKRVPv/fnjNujEiq5W6A1VzKvSMAChxVoiROqhX1qQ2EMEnmYXRMBZuADQBI00lsi/ScANY6Gmvkg7QaSpZkGj1yDBo5UgnvYG7GGmthGgaKtZ7tdMbQGZVpgN04QawVodAG4k03ABPMALMTkATvbI+tQEk5JHtOSWPGtG0TLXfFItEmOq11OaEAlBBNItlTl4AagyBzCkKQA0hPHPKBGCqQSLjFvaG/wipO3AnPzElqGojdMIzAAAAAElFTkSuQmCC" />',
                title: 'deshabilitar localización', // like its title
                onClick: function (btn, map) {       // and its callback
                    LoadingManagerService.Show('Desactivando localización...');

                    MapBackendManagerService.PostMessage("disableLocation");

                    LoadingManagerService.Hide();
                    MapGpsManagerService.ClearGpsPosition();
                    btn.state('location-disable');

                    IsEnableLocation = true;
                    IsEnableCenterLocation = false;
                }
            }
            ]
        });
        buttonNavigation.addTo(MapManagerService.GetMap());

        //add scale
        L.control.scale({
            imperial: false
        }
        ).addTo(LeafletMap);

        MapBackendManagerService.PostMessage('mapLoaded');
        console.log("ocultando loader");
        LoadingManagerService.Hide();
    }

    let setView = function (lat, lng) {
        let latLng = L.latLng(lat, lng);
        LeafletMap.setView(latLng, LeafletMap.getZoom());
    }

    let setBounds = function (center1, center2, center3, center4) {
        LeafletMap.fitBounds([
            [center1, center2],
            [center3, center4]
        ]);
    }

    let disableZoom = function () {
        LeafletMap.touchZoom.disable();
        LeafletMap.doubleClickZoom.disable();
        LeafletMap.scrollWheelZoom.disable();
        LeafletMap.boxZoom.disable();
        LeafletMap.keyboard.disable();
    }
    let enableZoom = function () {
        LeafletMap.touchZoom.enable();
        LeafletMap.doubleClickZoom.enable();
        LeafletMap.scrollWheelZoom.enable();
        LeafletMap.boxZoom.enable();
        LeafletMap.keyboard.enable();
    }

    let setZoom = function (zoom) {
        LeafletMap.setZoom(zoom);
    }

    let setZoomMax = function () {
        LeafletMap.setZoom(LeafletMap.getMaxZoom());
    }

    let enableDisableMapMove = function (enable) {
        if (enable)
            LeafletMap.dragging.enable();
        else
            LeafletMap.dragging.disable();
    }

    let fitEstate = function () {
        console.log('fitEstate');
        console.log('IsEnableLocation:', IsEnableLocation);
        console.log('IsEnableCenterLocation:', IsEnableCenterLocation);
        if (IsEnableLocation && IsEnableCenterLocation) {
            console.log('fitEstate');
            buttonNavigation.state('location-enable-move');
            IsEnableCenterLocation = false;
        }
    }

    //return plublic methods of service
    return {
        GetIdProyecto: getIdProyecto,
        GetIsDevelop: getIsDevelop,
        IniciarPantalla: iniciarPantalla,
        GetMap: getMap,

        //Map controls
        SetView: setView,
        SetZoom: setZoom,
        SetBounds: setBounds,
        SetZoomMax: setZoomMax,
        EnableDisableMapMove: enableDisableMapMove,
        //Map controls

        EnableZoom: enableZoom,
        DisableZoom: disableZoom,

        //fit
        FitEstate: fitEstate
    };
})();