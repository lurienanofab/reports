(function ($) {
    $.fn.managerUsageSummary = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opts = $.extend({}, { "apiUrl": "api/", "clientId": 0, "period": null }, options, $this.data());

            var lastClientId = 0;

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

            var getDisplayName = function (client) {
                return client.LName + ", " + client.FName;
            }

            var getPeriod = function () {
                return moment(new Date($(".period", $this).val()));
            };

            var initTemplate = function () {
                var def = $.Deferred();

                $.ajax({
                    "url": opts.templateUrl
                }).done(function (data, textStatus, jqXHR) {
                    def.resolve(Handlebars.compile(data));
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    showAlertWithXhr(jqXHR, opts.templateUrl);
                    def.reject();
                });

                return def.promise();
            }

            var loadManagers = function () {
                var def = $.Deferred();

                var managers = $("select.managers", $this);

                if (managers.length == 0) {
                    $(".run-report", $this).prop("disabled", false);
                    $(".email-report", $this).prop("disabled", false);
                    def.resolve();
                    return def.promise();
                }

                if (!$(".period", $this).val()) {
                    managers.html("");
                    def.resolve();
                    return def.promise();
                }

                var period = getPeriod();

                if (period.isValid()) {
                    $(".run-report", $this).prop("disabled", true);
                    $(".email-report", $this).prop("disabled", true);

                    managers.html($("<option/>").text("loading...")).prop("disabled", true);

                    var url = opts.apiUrl + "client/manager?period=" + period.format("YYYY-MM-DD");

                    $.ajax({
                        "url": url
                    }).done(function (data, textStatus, jqXHR) {
                        managers.html("")
                            .append($("<option/>").val(0).text("-- Select --"))
                            .append($.map(data, function (item, index) {
                                return $("<option/>").val(item.ClientID).text(getDisplayName(item)).prop("selected", item.ClientID == lastClientId);
                            }));
                        def.resolve();
                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        showAlertWithXhr(jqXHR, url);
                        def.reject();
                    }).always(function () {
                        $(".run-report", $this).prop("disabled", false);
                        $(".email-report", $this).prop("disabled", false);
                        managers.prop("disabled", false);
                    });
                } else {
                    showAlert("Invalid date.");
                    def.reject();
                }

                return def.promise();
            };

            var emailReport = function (period, clientId) {
                $(".alert-danger", $this).hide();
                $(".alert-success", $this).hide();

                if (clientId > 0) {
                    if (period.isValid()) {
                        $(".run-report", $this).prop("disabled", true);
                        $(".email-report", $this).prop("disabled", true);

                        $(".working", $this).show();

                        var url = opts.apiUrl + "report/manager-usage-summary/email";

                        $.ajax({
                            "url": url,
                            "method": "POST",
                            "data": { "ClientID": clientId, "Period": period.format("YYYY-MM-DD") }
                        }).done(function (data, textStatus, jqXHR) {
                            if (data.ErrorMessage)
                                showAlert(data.ErrorMessage)
                            else
                                showMessage("Email report complete. Emails sent: " + data.Count);
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
                    showAlert("Please select a manager.");
                }
            }

            initTemplate().done(function (tpl) {
                var displayReport = function (model) {
                    $(".report", $this).html(tpl({
                        "model": model,
                        "class": "col-md-" + (model.ShowSubsidyColumn ? "8" : "5")
                    }));
                }

                var loadReport = function (period, clientId) {
                    $(".alert-danger", $this).hide();
                    $(".alert-success", $this).hide();

                    if (clientId > 0) {
                        if (period.isValid()) {
                            lastClientId = clientId;

                            $(".run-report", $this).prop("disabled", true);
                            $(".email-report", $this).prop("disabled", true);

                            $(".working", $this).show();

                            var url = opts.apiUrl + "report/manager-usage-summary?period=" + period.format("YYYY-MM-DD") + "&clientId=" + clientId;

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
                        showAlert("Please select a manager.");
                    }
                }

                $this.on("click", ".run-report", function (e) {
                    var period = getPeriod();
                    var clientId = $(".managers", $this).val();
                    loadReport(period, clientId);
                }).on("click", ".email-report", function (e) {
                    var period = getPeriod();
                    var clientId = $(".managers", $this).val();
                    emailReport(period, clientId);
                }).on("change", ".period", function (e) {
                    loadManagers();
                });

                loadManagers().done(function () {
                    var period = getPeriod();
                    var clientId = $(".managers", $this).val();
                    if (clientId > 0 && period.isValid()) {
                        loadReport(period, clientId);
                    }
                });
            });
        });
    };
})(jQuery);