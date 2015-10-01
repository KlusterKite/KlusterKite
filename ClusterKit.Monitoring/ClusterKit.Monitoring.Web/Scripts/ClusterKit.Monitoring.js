(function ($) {
    var clusterState = {
        Connected: ko.observable(false)
    };

    $(document).ready(function () {
        ko.applyBindings(clusterState, $("#clusterMonitoringView")[0]);
    });
})(jQuery);