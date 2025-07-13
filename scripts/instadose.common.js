var Common = Common || {
    account: 0,
    locationListItems: [],
    configure: function () {
        if (Common.username == "ylee")
            Common.developer = true;
        if ($("#common-alert-dialog").length == 0) {
            $("form").append("<div id='common-alert-dialog' style='display:none;'><div id='common-alert-message'></div></div>");
        }

        if ($("#common-confirm-dialog").length == 0) {
            $("form").append("<div id='common-confirm-dialog' style='display:none;'><div id='common-confirm-message'></div></div>");
        }
    },
    toggleDialogLoading: function (dialog, show) {
        if (show) {
            $(dialog).parent().find(".loading").show();
        } else {
            $(dialog).parent().find(".loading").hide();
        }
    },
    toggleDialogError: function (dialog, errMessage, isHtml) {
        var errForm = $(dialog).find(".FormError");

        if (errMessage == "") {
            errForm.hide();
        } else {
            errForm.show();
        }

        if (isHtml != undefined && isHtml) {
            errForm.find("span.Message").html(errMessage);
        } else {
            errForm.find("span.Message").text(errMessage);
        }
    },
    refreshGrid: function (gridID) {
        if (gridID != "") {
            var grid = $find(gridID);

            if (grid != null) {
                var masterTable = grid.get_masterTableView();
                masterTable.rebind();
            }
        }
    },
    wsCall: function (wsURL, dObj, method, callback) {
        var dataObj = dObj;
        if (method == "POST")
            dataObj = JSON.stringify(dObj);
        $.ajax({
            url: wsURL,
            data: dataObj,
            contentType: "application/json",
            dataType: "json",
            type: method,
            success: function (data) {
                callback({ Success: true, Data: data.d });
            },
            error: function (xhr, status, err) {
                callback({ Success: false, Data: xhr.responseText });
            }
        });
    },
    toggleLoading: function (dialog, show) {
        if (show)
            $(dialog).parent().find(".loading").show();
        else
            $(dialog).parent().find(".loading").hide();
    },
    //toggleDialogError: function (dialog, errMessage, isHtml) {
    //    var errForm = $(dialog).find(".FormError");
    //    if (errMessage == "")
    //        errForm.hide();
    //    else
    //        errForm.show();
    //    if (isHtml != undefined && isHtml)
    //        errForm.find("span.Message").html(errMessage);
    //    else
    //        errForm.find("span.Message").text(errMessage);
    //},
    openAlert: function (title, message, width) {
        var defer = $.Deferred();

        if (width < 300)
            width = 300;

        $("#common-alert-message").html(message);
        $("#common-alert-dialog").dialog({
            width: width,
            resizable: false,
            title: title,
            modal: true,
            open: function (type, data) {

            },
            close: function () {
                defer.resolve();
                $('.ui-overlay').fadeOut();
            },
            buttons: {
                "Close": function () {
                    $(this).dialog("close");
                }
            }
        });

        return defer.promise();
    },
    openConfirm: function (title, message, width) {
        var defer = $.Deferred();

        if (width < 300)
            width = 300;

        $("#common-confirm-message").html(message);
        $("#common-confirm-dialog").dialog({
            width: width,
            resizable: false,
            title: title,
            modal: true,
            open: function (type, data) {

            },
            close: function () {
                $('.ui-overlay').fadeOut();
            },
            buttons: {
                "OK, Continue": function () {
                    $(this).dialog("close");
                    defer.resolve();
                },
                "Close": function () {
                    $(this).dialog("close");
                    if (Common.dailyConfirm)
                        $("#lblWearDates input[type=checkbox]").prop("checked", false);
                }
            }
        });

        return defer.promise();
    },
    getLocationListItems: function (account, active, callback) {
        if (account != "") {
            if (Common.locationListItems.length == 0 || Common.account != account) {
                $.ajax({
                    url: '/Services/Account.asmx/GetLocationListItems',
                    data: { account: '"' + account + '"', activeOnly: false },
                    datatype: "json",
                    type: "GET",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        if (Common.account == account) {
                            Common.locationListItems = data.d;
                        }

                        var filteredLocations = data.d;

                        if (active) {
                            filteredLocations = $.grep(data.d, function (i) { return i.Active == 1; });
                        }

                        callback(filteredLocations);
                    }
                });
            } else {
                var filteredLocations = Common.locationListItems;

                if (active) {
                    filteredLocations = $.grep(Common.locationListItems, function (i) { return i.Active == 1; });
                }

                callback(filteredLocations);
            }
        }
    },
    ajaxDataFilter: function (data) {
        var response;
        if (typeof (JSON) !== 'undefined' && typeof (JSON.parse) === 'function')
            response = JSON.parse(data);
        else
            response = eval('(' + data + ')');

        if (response.hasOwnProperty('d'))
            return response.d;
        else
            return response;
    }
};
