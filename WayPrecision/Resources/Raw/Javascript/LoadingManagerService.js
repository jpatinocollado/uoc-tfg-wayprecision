var LoadingManagerService = (function () {
    var container = '#map';
    var show = function (message) {
        $(container).loading({
            message: message,
            showClass: 'loadingShow'
        });
    }

    var hide = function () {
        if ($(container).is(':loading'))
            $(container).loading('stop');
    }

    //return plublic methods of service
    return {
        Show: show,
        Hide: hide
    };
})();