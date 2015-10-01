(function ($) {
    var signalrConnection;
    var hubProxy;
    var timeout = 1000 * 60;

    var clusterState = {
        Connected: ko.observable(false),
        Members: ko.observableArray([])
    };

    function onDisconnected() {
        clusterState.Connected(false);
    }

    function onConnected() {
        clusterState.Connected(true);
        $.ajax({
            type: 'get',
            url: '/MonitoringApi/GetClusterMemberList',
            data: {
            },
            dataType: 'json',
            timeout: timeout,
            success: function (data) {
                clusterState.Connected(true);
                clusterState.Members(data);
            },
            error: function () {
                onDisconnected();
            }
        });
    }

    $(document).ready(function () {
        ko.applyBindings(clusterState, $("#clusterMonitoringView")[0]);
        signalrConnection = $.hubConnection();
        hubProxy = signalrConnection.createHubProxy("monitoring");

        // todo: register events
        signalrConnection.reconnected(function () {
            onConnected();
        });

        signalrConnection.reconnecting(function () {
            clusterState.Connected(false);
        });

        signalrConnection.disconnected(function () {
            onDisconnected();
        });

        signalrConnection.start()
            .done(function () { onConnected(); })
            .fail(function () { onDisconnected(); });
    });
})(jQuery);