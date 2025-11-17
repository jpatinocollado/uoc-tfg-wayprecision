let LoadingManagerService = (function () {
    let container = '#map';
    let show = function (message) {
        $(container).loading({
            message: message,
            showClass: 'loadingShow'
        });
    }

    let hide = function () {
        if ($(container).is(':loading'))
            $(container).loading('stop');
    }

    //return plublic methods of service
    return {
        Show: show,
        Hide: hide
    };
})();