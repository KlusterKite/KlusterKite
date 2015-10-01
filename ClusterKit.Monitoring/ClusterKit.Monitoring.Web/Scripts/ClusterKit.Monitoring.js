(function ($) {
    var signalrConnection;
    var hubProxy;

    var clusterState = {
        Connected: ko.observable(false)
    };

    $(document).ready(function () {
        ko.applyBindings(clusterState, $("#clusterMonitoringView")[0]);
        signalrConnection = $.hubConnection();
        hubProxy = signalrConnection.createHubProxy("monitoring");

        // todo: register events
        signalrConnection.reconnected(function () {
            clusterState.Connected(true);
        });

        signalrConnection.reconnecting(function () {
            clusterState.Connected(false);
        });

        signalrConnection.disconnected(function () {
            clusterState.Connected(false);
        });

        signalrConnection.start()
            .done(function () { clusterState.Connected(true) })
            .fail(function () { clusterState.Connected(false) });
    });
})(jQuery);