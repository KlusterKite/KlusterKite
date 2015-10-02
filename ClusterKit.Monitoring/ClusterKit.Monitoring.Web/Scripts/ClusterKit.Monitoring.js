(function ($) {
    var signalrConnection;
    var hubProxy;
    var timeout = 1000 * 60;

    var clusterState = {
        Connected: ko.observable(false),
        Members: ko.observableArray([])
    };

    function connect() {
        signalrConnection.start()
            .done(function () { onConnected(); })
            .fail(function () { onDisconnected(false); });
    }

    function onDisconnected(isOnStart) {
        clusterState.Connected(false);
        setTimeout(connect, isOnStart ? 5000 : 1000);
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
            }
        });
    }

    $(document).ready(function () {
        ko.applyBindings(clusterState, $("#clusterMonitoringView")[0]);
        signalrConnection = $.hubConnection();
        hubProxy = signalrConnection.createHubProxy("monitoringHub");

        // todo: register events
        signalrConnection.reconnected(function () {
            onConnected();
        });

        signalrConnection.reconnecting(function () {
            clusterState.Connected(false);
        });

        signalrConnection.disconnected(function () {
            onDisconnected(true);
        });

        hubProxy.on("memberUpdate", function (member) {
            console.dir(member);
        });

        hubProxy.on("reloadData", function (data) {
            clusterState.Members(data);
        });

        connect();
    });
})(jQuery);