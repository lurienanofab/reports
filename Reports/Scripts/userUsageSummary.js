(function ($) {
    $.fn.userUsageSummary = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opts = $.extend({}, { "apiUrl": "api/", "clientId": 0, "period": null }, options, $this.data());

            var showAlert = function (msg) {
                var alert = $(".alert-danger", $this);
                $(".alert-message", alert).html(msg);
                alert.show();
            };

            var showAlertWithXhr = function (xhr, url) {
                if (xhr.status == 404)
                    showAlert("Page not found: " + url);
                else if (xhr.responseJSON && xhr.responseJSON.ExceptionMessage)
                    showAlert(xhr.responseJSON.ExceptionMessage);
                else if (xhr.statusText)
                    showAlert(xhr.statusText);
                else
                    showAlert("An error occurred.");
            }

            var showMessage = function (msg) {
                var alert = $(".alert-success", $this);
                $(".alert-message", alert).html(msg);
                alert.show();
            }

            var getPeriod = function () {
                return moment(new Date($(".period", $this).val()));
            };

            var loadUsers = function () {
                var def = $.Deferred();

                var users = $("select.users", $this);

                if (users.length == 0) {
                    def.resolve();
                    return def.promise();
                }

                if (!$(".period", $this).val()) {
                    users.html("");
                    def.resolve();
                    return def.promise();
                }

                var period = getPeriod();

                if (period.isValid()) {
                    $(".run-report", $this).prop("disabled", true);
                    $(".email-report", $this).prop("disabled", true);

                    users.html($("<option/>").text("loading...")).prop("disabled", true);

                    var url = opts.apiUrl + "client?period=" + period.format("YYYY-MM-DD");

                    $.ajax({
                        "url": url
                    }).done(function (data, textStatus, jqXHR) {
                        users.html("")
                            .append($("<option/>").val(0).text("-- Select --"))
                            .append($.map(data, function (item, index) {
                                return $("<option/>").val(item.ClientID).text(item.DisplayName).prop("selected", item.ClientID == opts.clientId);
                            }));
                        def.resolve();
                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        showAlertWithXhr(jqXHR, url);
                        def.reject();
                    }).always(function () {
                        $(".run-report", $this).prop("disabled", false);
                        $(".email-report", $this).prop("disabled", false);
                        users.prop("disabled", false);
                    });
                } else {
                    showAlert("Invalid date.");
                    def.reject();
                }

                return def.promise();
            };

            var loadReport = function (period, clientId) {
                $(".alert-danger", $this).hide();
                $(".alert-success", $this).hide();

                if (clientId > 0) {
                    if (period.isValid()) {
                        $(".run-report", $this).prop("disabled", true);
                        $(".email-report", $this).prop("disabled", true);

                        $(".working", $this).show();

                        var url = opts.apiUrl + "report/user-usage-summary?period=" + period.format("YYYY-MM-DD") + "&clientId=" + clientId;

                        $.ajax({
                            "url": url
                        }).done(function (data, textStatus, jqXHR) {
                            displayReport(data);
                        }).fail(function (jqXHR, textStatus, errorThrown) {
                            showAlertWithXhr(jqXHR, url);
                        }).always(function () {
                            $(".run-report", $this).prop("disabled", false);
                            $(".email-report", $this).prop("disabled", false);
                            $(".working", $this).hide();
                        });
                    } else {
                        showAlert("Invalid date.");
                    }
                } else {
                    showAlert("Please select a user.");
                }
            }

            var displayReport = function (model) {
                var template = _.template($(".report-template", $this).html(), { variable: "model" });
                $(".report", $this).html(template(model));
            }

            var emailReport = function (period, clientId) {
                $(".alert-danger", $this).hide();
                $(".alert-success", $this).hide();

                if (clientId > 0) {
                    if (period.isValid()) {
                        $(".run-report", $this).prop("disabled", true);
                        $(".email-report", $this).prop("disabled", true);

                        $(".working", $this).show();

                        $.ajax({
                            "url": opts.apiUrl + "report/user-usage-summary/email",
                            "method": "POST",
                            "data": { "ClientID": clientId, "Period": period.format("YYYY-MM-DD") }
                        }).done(function (data, textStatus, jqXHR) {
                            if (data.ErrorMessage)
                                showAlert(data.ErrorMessage)
                            else
                                showMessage("Email report complete. Emails sent: " + data.Count);
                        }).fail(function (jqXHR, textStatus, errorThrown) {
                            showAlert(jqXHR.responseJSON.ExceptionMessage);
                        }).always(function () {
                            $(".run-report", $this).prop("disabled", false);
                            $(".email-report", $this).prop("disabled", false);
                            $(".working", $this).hide();
                        });
                    } else {
                        showAlert("Invalid date.");
                    }
                } else {
                    showAlert("Please select a manager.");
                }
            }

            $this.on("click", ".run-report", function (e) {
                var period = getPeriod();
                var clientId = $(".users", $this).val();
                loadReport(period, clientId);
            }).on("click", ".email-report", function (e) {
                var period = getPeriod();
                var clientId = $(".users", $this).val();
                emailReport(period, clientId);
            }).on("change", ".period", function (e) {
                loadUsers();
            });

            loadUsers().done(function () {
                var period = getPeriod();
                var clientId = $(".users", $this).val();
                if (clientId > 0 && period.isValid()) {
                    loadReport(period, clientId);
                }
            });
        });
    };
})(jQuery);