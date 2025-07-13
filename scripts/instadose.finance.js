var Finance = Finance || {
    collections: {
        usersAllowed: ["jdizon", "jhuckabee", "dababon", "tdo", "irv-phl-coll11", "irv-phl-coll25", "irv-phl-coll05"],
        allowedToUse: false,
        outsideCollector: "",
        noticeText: "",
        checkUser: function (callback) {
            var _this = this;
            var username = $("#hfUsername").val().toLowerCase();
            $.each(_this.usersAllowed, function (idx, i) {
                if (i == username)
                    _this.allowedToUse = true;
            });
            callback(_this.allowedToUse);
        },
        configure: function () {
            var _this = this;
            
            var hlOutsideCollector = $("#btnOutsideCollector");
            _this.checkUser(function () {
                if (_this.allowedToUse) {
                    $(".collector").show();
                    hlOutsideCollector.show();
                }
            });
            _this.getOutsideCollectors(function (response) {
                if (_this.outsideCollector != "") {
                    if (_this.noticeText != "")
                        _this.noticeText += "<br/>";
                    if (_this.noticeText.indexOf("ACCOUNT WITH 3RD PARTY COLLECTION AGENCY") == -1) {
                        _this.noticeText += "ACCOUNT WITH 3RD PARTY COLLECTION AGENCY";
                        var collector = $.grep(_this.outsideCollectors, function (i) { return i.OutsideCollectorID == _this.outsideCollector; })
                        $("#lblOutsideCollector").text(collector[0].CollectorName);
                        hlOutsideCollector.text("X").prop("title","Remove outside collection flag");
                    }
                    Finance.creditHoldOrOutsideCollection(_this.outsideCollection, _this.noticeText);
                }
            });
            $("#outside-collector-dialog").dialog({
                autoOpen: false,
                width: 500,
                height: 210,
                title: "Outside Collection",
                modal: true,
                open: function () {
                    var ddl = $("#ddlOutsideCollector").closest(".Row");
                    if (_this.outsideCollector != "") {
                        $(".message").text("Click continue to remove the outside collector flag.");
                        ddl.hide();
                    } else {
                        $(".message").text("Select an outside collection company and click continue.");
                        ddl.show();
                    }
                },
                close: function () {

                },
                buttons: {
                    "Continue": function () {
                        $(".loading").show();
                        var collector = $("#ddlOutsideCollector").val();
                        if (!$("#ddlOutsideCollector").is(":visible"))
                            collector = null;
                        _this.updateOutsideCollection(accID, collector, function (response) {
                            if (response.Success)
                                document.location.reload();
                            else {
                                $(".loading").hide();
                                Finance.toggleDialogError("body", JSON.parse(response.Data).Message);
                            }
                        });
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                }
            });

            hlOutsideCollector.on("click", function () {
                $("#outside-collector-dialog").dialog("open");
            });

            
        },
        outsideCollectors: [],
        getOutsideCollectors: function (callback) {
            var _this = this;
            if (_this.outsideCollectors.length == 0) {
                Finance.wsCall("/Services/Finance.asmx/GetOutsideCollectors", {}, "POST", function (response) {
                    if (response.Success) {
                        _this.outsideCollectors = JSON.parse(response.Data);
                        //populate dropdown of collectors
                        var html = "";
                        if (_this.outsideCollectors.length > 1)
                            html += "<option value=''>Select</option>";
                        $.each(_this.outsideCollectors, function (idx, i) {
                            html += "<option value='" + i.OutsideCollectorID + "'>" + i.CollectorName + "</option>";
                        });
                        $("#ddlOutsideCollector").html(html);
                    }
                    callback();
                });
            } else {
                callback();
            }
        },
        updateOutsideCollection: function (account, collector, callback) {
            var _this = this;
            Finance.wsCall("/Services/Finance.asmx/UpdateOutsideCollector", { accountID: account, collectorID: collector }, "POST", function (response) {
                callback(response);
            });
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
    creditHoldOrOutsideCollection: function (fieldValue, message) {
        var _this = this;
        if (fieldValue != "" && message != "") {
            _this.displayMainNotice(message);
        }
    },
    displayMainNotice: function (noticeMessage) {
        var notice = $("#NoticeText");
        notice.html(noticeMessage);
        $(".NoticeContainer").show();
    },
    toggleDialogError: function (dialog, errMessage, isHtml) {
        var errForm = $(dialog).find(".FormError");
        if (errMessage == "")
            errForm.hide();
        else
            errForm.show();
        if (isHtml != undefined && isHtml)
            errForm.find("span.Message").html(errMessage);
        else
            errForm.find("span.Message").text(errMessage);
    },
}