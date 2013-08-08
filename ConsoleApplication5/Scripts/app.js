(function (document, $, undefined) {
    "use strict";

    function parse(spec) {
        vg.parse.spec(spec, function (chart) { chart({ el: "#vis" }).update(); });
    }

    $(document).ready(function () {
        var chartHub;

        $.connection.hub.url = 'http://localhost:8081/signalr';

        chartHub = $.connection.chartHub;
        if (chartHub) {
            chartHub.client.parse = function (spec) {
                var data = JSON.parse(spec);
                parse(data);
                console.log('Server called parse(' + spec + ')');
            };

            // Kick off the hub.
            $.connection.hub.start();
        }
        else {
            console.log('No hub found by the name of chartHub');
        }
    });
})(document, jQuery);
