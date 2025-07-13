var User = User || {
    configure: function() {
        var _this = this;
        _this.updateForm.init();

        $("input[name*=chkUserSelectCheckBox]").on("change", function () {
            var visible = "hidden";
            if ($(this).prop("checked"))
                visible = "visible";
            $("#btnUpdate").css("visibility", visible);
        });
    },
    updateForm: {
        dialog: null,
        init: function () {
            var _this = this;
            _this.dialog = $("#mass-update-dialog").dialog({
                autoOpen: false,
                modal: true,
                title: "Update Users",
                width: 500,
                buttons: {
                    "Continue": function () {
                        User.displayError($("#mass-update-dialog"), "");
                        var selectedVal = $("#ddlAction").val();
                        if (selectedVal != "") {
                            $(".loading").show();
                            _this.collectUsers(function (users) {
                                if (users.length > 0) {
                                    switch (selectedVal) {
                                        case "Activate":
                                            _this.updateStatus(users, true);
                                            break;
                                        case "Deactivate":
                                            _this.updateStatus(users, false);
                                            break;
                                        case "Resend":
                                            _this.resendEmails(users);
                                            break;
                                        case "Reset":
                                            _this.resetPasswords(users);
                                            break;
                                        case "InstadoseAccess":
                                            _this.sendWelcomeEmails(users, true);
                                            break;
                                        case "Instadose":
                                            _this.sendWelcomeEmails(users, false);
                                            break;
                                        case "InstadoseSite":
                                            _this.sendWelcomeEmails(users, true);
                                            break;
                                        case "InstadoseSiteNo":
                                            _this.sendWelcomeEmails(users, false);
                                            break;
                                    }
                                } else {
                                    User.displayError($("#mass-update-dialog"), "No users are selected.");
                                }
                            });
                        } else {
                            User.displayError($("#mass-update-dialog"), "Select an action.");
                        }
                    },
                    "Close": function () {
                        $(this).dialog("close");
                    },
                }
            });

            $("body").on("click", "#btnUpdate", function () {
                _this.dialog.dialog("open");
            });
        },
        collectUsers: function (callback) {
            var checkedUsers = $("table[id*=rgUsers] tbody").find("input[type=checkbox]:checked");
            var arrAuthUserID = [];
            checkedUsers.each(function (i, item) {
                var userid = $(item).closest("tr").find("#hdnUserID").val();
                arrAuthUserID.push(userid);
            });
            callback(arrAuthUserID);
        },
        updateStatus: function (arrUsers, status) {
            Common.wsCall("/Services/User.asmx/ToggleStatus", { users: arrUsers, active: status }, "POST", function (response) {
                $(".loading").hide();
                if (response.Success) {
                    User.resetChecked();
                } else
                    User.displayError($("#mass-update-dialog"), response.Data);
            });
        },
        resendEmails: function (arrUsers) {
            Common.wsCall("/Services/User.asmx/ResendSetupEmail", { users: arrUsers }, "POST", function (response) {
                $(".loading").hide();
                if (response.Success) {
                    User.resetChecked();
                } else
                    User.displayError($("#mass-update-dialog"), response.Data);
            });
        },
        sendWelcomeEmails: function (arrUsers, access) {
            Common.wsCall("/Services/User.asmx/SendWelcomeEmail", { users: arrUsers, access: access }, "POST", function (response) {
                $(".loading").hide();
                if (response.Success) {
                    User.resetChecked();
                } else
                    User.displayError($("#mass-update-dialog"), response.Data);
            });
        },
        resetPasswords: function (arrUsers) {
            Common.wsCall("/Services/User.asmx/ResetPasswords", { users: arrUsers }, "POST", function (response) {
                $(".loading").hide();
                if (response.Success) {
                    User.resetChecked();
                } else
                    User.displayError($("#mass-update-dialog"), response.Data);
            });
        }
    },
    displayError: function (dialogForm, errMessage) {
        var errorHolder = dialogForm.find(".FormError");
        var errorText = errorHolder.show().find("span[id]");
        if (errMessage === "")
            errorHolder.hide();
        else
            errorHolder.show();
        errorText.text(errMessage);
    },
    resetChecked: function () {
        $("#mass-update-dialog").dialog("close");
        $("div[id*=rgUsers] table input[type=checkbox]:checked").trigger("click");
        $("#btnUpdate").css("visibility", "hidden");
    },
}