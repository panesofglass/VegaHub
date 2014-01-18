(function (root, document, $, d3, vg, undefined) {
    "use strict";

    function parse(spec) {
        vg.parse.spec(spec, function (chart) {
            //d3.select('#vis').selectAll('*').remove();
            chart({ el: '#vis' }).update();
        });
    }

    $(document).ready(function () {
        var chartHub;

        $.connection.hub.url = 'http://localhost:8081/signalr';

        chartHub = $.connection.chartHub;
        if (chartHub) {
            chartHub.client.parse = function (spec) {
                root.console.log(spec);
                parse(JSON.parse(spec));
            };

            // Kick off the hub.
            $.connection.hub.start();
        }
        else {
            console.log('No hub found by the name of chartHub');
        }
    });
})(window, document, jQuery, d3, vg);
