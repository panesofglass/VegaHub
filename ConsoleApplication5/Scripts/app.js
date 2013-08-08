(function (document, $, undefined) {
    "use strict";

    $(document).ready(function () {
        var chartHub;

        $.connection.hub.url = 'http://localhost:8081/signalr';

        chartHub = $.connection.chartHub;
        if (chartHub) {
            // Define the `addMessage` function on the client.
            chartHub.client.addMessage = function (message) {
                $('#results').append('<p>' + message + '</p>');
                console.log('Server called addMessage(' + message + ')');
            };

            // Define the `addData` function on the client.
            chartHub.client.addData = function (data) {
                console.log('Server called addData(' + data + ')');
                data.forEach(function (item) {
                    $('#results').append('<p>' + item + '</p>');
                });
            };

            // Kick off the hub.
            $.connection.hub.start().
              done(function () {
                  $('#submit').on('click', function () {
                      chartHub.server.send($('#source').val());
                  });
              });
        }
        else {
            console.log('No hub found by the name of chartHub');
        }
    });
})(document, jQuery);
