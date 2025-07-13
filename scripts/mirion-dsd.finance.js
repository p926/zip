var FinanceAging = FinanceAging || {
    ARScript: {
        greeting: "<p>Thank you for calling Mirion Technologies this is _____ May I please have your account number? Who am I speaking with today?</p>",
        green: {
            backgroundColor: "green",
            dialogTitle: "10-45 Days Past Due - No Action",
            script: "Hi _____ in reviewing your account there is an open A/R for <strong>{ARBalance}</strong> would you like to take care of that now with a credit card payment?",
            noPayment: "<span class='heading'>- If no payment made</span><br/>Ok thank you ______ this was a courtesy update balance on your account, at this point we can move forward how may I help you today?",
            textColor: "white"
        },
        orange: {
            backgroundColor: "orange",
            dialogTitle: "Greater Than 30 Days Past Due",
            script: "Hi _____ in reviewing your account there is an open A/R for <strong>{ARBalance}</strong> would you like to take care of that now with a credit card payment?",
            noPayment: "<span class='heading'>- If no payment made</span><br/>Ok thank you ______ this was a courtesy update balance on your account, at this point we can move forward how may I help you today?",
            textColor: "white"
        },
        yellow: {
            backgroundColor: "yellow",
            dialogTitle: "46-149 Days Past Due",
            script: "Hi _____ in reviewing your account there is an open A/R for <strong>{ARBalance}</strong> would you like to take care of that now with a credit card payment?",
            noPayment: "<span class='heading'>- If no payment made</span><br/>Your account is over 45 days past due our terms are Net 30 we highly recommend making a payment to bring your account current, further delay in payment risks disruption in service.<br/><span class='heading'>- no</span><br/>How may I help you?",
            textColor: "black"
        },
        red: {
            backgroundColor: "red",
            dialogTitle: "150+ Days Past Due",
            script: "Hi _____  our records show <strong>{ARBalance}</strong> is 150 days delinquent so a payment by phone will be needed to proceed with your request. What credit card will you be using today?",
            noPayment: "<span class='heading'>- If no payment made</span><br/>Unfortunately our system will not allow me to proceed with your request until payment is received.",
            textColor: "white"
        },
        blue: {
            backgroundColor: "blue",
            dialogTitle: "150+ Days Past Due",
            script: "Hi _____ in reviewing your account there is an open A/R for <strong>{ARBalance}</strong>, could you work with your A/P dept to get this issue resolved?",
            noPayment: "",
            textColor: "white"
        },
    },
    instadoseServiceUrl: "https://portal.instadose.com",
    arAccountInfo: [],
    ccTypeID: 0,
    useOnce: true,
    arTransactions: [],
    pendingTransactions: [],
    previousARType: -1,
    AROverCheck: true,
    userADGroups: [],
    username: "",
    groupsToShow: ["IRV-CustomerSvc", "IRV-IT"], // "IRV-IT" // IRV-CustomerSvc
    accountOverrideUser: [],
    arAgingShow: false,
    arOverride: false,
    configure: function (account) {
        var _this = this;
        //Get the list of users that can override the AR popup.
        this.getAROverrideUsers(function (users) {
            _this.accountOverrideUser = users.split(";");
        });
        //Get AR Aging Information if user/adGroup has permission
        if (FinanceAging.AROverCheck) {

            FinanceAging.getUserName(function() {
                FinanceAging.getADGroups(function () {
                    FinanceAging.setARAgingSecurity(function () {
                        FinanceAging.runARAging(function () {
                    });
                });
                });
            });
        }
    },
    getAROverrideUsers: function (callback) {
        Common.wsCall("/Services/Finance.asmx/GetAROverrideUsers", {}, "POST", function (response) {
            
            if (response.Success) {
                var data = JSON.parse(response.Data);
                var users = data[0].AppSettingValue;
                callback(users);
            } else {
                callback("");
            }
        });
    },
    runARAging: function () {
        if (FinanceAging.arAgingShow) {
            Common.wsCall("../../Services/Account.asmx/GetARAging", { account: Common.account }, "POST", function (response) {
                if (response.Success) {
                    if (response.Data == "false") {
                        //do Nothing????
                    }
                    else {
                        FinanceAging.arAccountInfo = response.Data;
                        FinanceAging.arTransactions = response.Data.AgingInvoices;

                        if (FinanceAging.arTransactions) {

                            FinanceAging.pastDueAR.configure();

                            FinanceAging.pastDueAR.buildPastDueInvoiceTable(FinanceAging.arTransactions, function (html) {
                                $("div.ar-dialog-invoices table tbody").append(html);
                            });
                        }
                    }
                }
                FinanceAging.AROverCheck = false;
            });
        }
    },
    getUserName: function (callback) {
        if (FinanceAging.username.length < 2) {
            Common.wsCall("/Services/Misc.asmx/GetUserName", { username: "Unknown" }, "POST", function (response) {
                FinanceAging.username = response.Data
                callback(response);
            });
        } else {
            callback({ Success: true, Data: FinanceAging.username });
        }
    },
    getADGroups: function (callback) {
        if (FinanceAging.userADGroups.length == 0) {
            Common.wsCall("/Services/Misc.asmx/GetUserRole", { username: "Unknown" }, "POST", function (response) {
                FinanceAging.userADGroups = JSON.parse(response.Data);
                callback(response);
            });
        } else {
            callback({ Success: true, Data: FinanceAging.userADGroups });
        }
    },
    setARAgingSecurity: function (callback)
    {
        var answer;
        var check = false;

        for (i = 0; i < FinanceAging.groupsToShow.length; i++)
        {
            if (FinanceAging.userADGroups.indexOf(FinanceAging.groupsToShow[i]) >= 0)
                check = true;
        }

            var checkOverride = $(FinanceAging.accountOverrideUser.indexOf(FinanceAging.username));

            if (check) {
                answer = true;
            }
            else {
                answer = false;
            }
            if (checkOverride[0] > -1) {
                answer = true;
                FinanceAging.arOverride = true;
            }
            
            FinanceAging.arAgingShow = answer;
            callback(answer);
    },
    //For AR Aging Invoices Popup and payment
    pastDueAR: {
        mustPay: false,
        mustPayInvoices: [],
        paidInvoices: [],
        currencyCode: "",
        accountType: "",
        showDialog: false,
        showBlueDialogOverride: false,

        configure: function () {
            var _this = this;
            //Account Info from Database
            _this.currencyCode = FinanceAging.arAccountInfo.CurrencyCode;
            _this.accountType = FinanceAging.arAccountInfo.AccountType;
            if (FinanceAging.arAccountInfo.PastDueMax === 150 && !FinanceAging.arAccountInfo.ShowBlueDialog
                && FinanceAging.arAccountInfo.ShowDialog && !FinanceAging.arAccountInfo.IsRestricted)
                FinanceAging.arAccountInfo.ShowDialog = false;
            _this.showDialog = FinanceAging.arAccountInfo.ShowDialog;
            _this.showBlueDialogOverride = FinanceAging.arAccountInfo.ShowBlueDialog;

            var keepOnFileYes = $("#makePaymentYes");
            var keepOnFileNo = $("#makePaymentNo");
            $("input[name='rdoMakePayment']").on("click", function () {
                keepOnFileYes.hide();
                keepOnFileNo.hide();
                if ($(this).val() == "Yes")
                    keepOnFileYes.show();
                else
                    keepOnFileNo.show();
            });
            $("#ar-dialog")
                .on("click", ".ar-dialog-invoices input", function () {
                    var checkbox = $(this);
                    if (checkbox.attr("title") === "Select All Invoices") {
                        $("tbody tr input").not(":disabled").prop("checked", checkbox.is(":checked"));
                    }
                    //Manage Footer Total and enable/disable Pay Invoices button
                    _this.footerTotal();
                })
                .on("blur", "#payment-card-number", function () {
                    _this.validateCC();
                })
                .on("blur", "#payment-card-cvv", function () {
                    _this.validateCVV();
                })
                .on("change", "#payment-card-exp-year,#payment-card-exp-month", function () {
                    _this.validateExp();
                })
                .dialog({
                    autoOpen: false,
                    modal: true,
                    width: 750,
                    open: function () {
                        if (_this.mustPay && FinanceAging.arAccountInfo.IsRestricted)
                            _this.selectInvoicesMustPay();
                        _this.overrideCheckbox();
                    },
                    buttons: {
                        "Close": function () {
                            $(this).dialog("close");
                        }
                    }
                });

            // TODO: The past due pop-up is disabled until AX Invoice integration is completed. After AX Invoice integration is completed, uncomment following lines.
            //_this.get(function (dialogOptions) {
            //    if (dialogOptions.dialogTitle !== "") {
            //        $("#ar-dialog").dialog("open").prev(".ui-dialog-titlebar").text(dialogOptions.dialogTitle);

            //        $("#ar-dialog").parent("div").addClass(dialogOptions.backgroundColor);

            //        //Handle Blue Dialog Changes - TH
            //        if (dialogOptions.backgroundColor === "blue") {
            //            $(".ar-dialog-invoices table tfoot").hide();
            //            $(".non-enterprise").hide();
            //        } else if (dialogOptions.backgroundColor === "red" ) {
            //            $("#btnARAccountOverride").parent().show();

            //            //Only show override button if the user is allowed to override and the account is restricted
            //            if (FinanceAging.arOverride && FinanceAging.arAccountInfo.IsRestricted){
            //                $("#btnARAccountOverride").show();
            //            }
            //        } else {
            //            $("#btnARAccountOverride").parent().hide();
            //            $("#btnARAccountOverride").hide();
            //            $("#overridenote").css('display', 'none');
            //            $("#overridetext").css('display', 'none');
                       
            //        }
            //    }
            //});

            $("#btnARPayInvoice").on("click", function () {
                FinanceAging.pastDueAR.errors = false;
                //alert("Epay");
                _this.validatePaymentInfo(function (valid) {
                    if (valid) {
                        //disble btnARPayInvoice so it can't be clicked twice
                        $("#btnARPayInvoice").prop("disabled", true);
                        $("#ar-dialogLoader").show();
                        _this.processPayment();
                    }
                });
            });

            //Certain managers can override a flagged account
            //***********************************************
            $("#overrideARAccount").dialog({
                autoOpen: false,
                modal: true,
                width: 450,
                resizable: false,
                title: "AR Account Override",
                open: function () {

                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                },
                buttons: {
                    "Override": function () {
                        Common.toggleDialogError("#overrideARAccount", "");

                        var arOverrideNote = $("#txtOverrideARAccountNote").val();

                        if (!arOverrideNote || arOverrideNote == "") {
                            Common.toggleDialogError("#overrideARAccount", "Note is required to override the account.");
                        }
                        else
                        {
                            Common.toggleLoading("#overrideARAccount", true);

                            _this.overrideRestrictedAccount(arOverrideNote, function (ftn)
                            {
                                if (ftn == "success") {
                                    $("#overrideARAccount").dialog("close");
                                    $.when(Common.openAlert("AR Account Override", "Account Lock Down is overridden. It may take several minutes to apply to the system.<br />The page will be refreshed.", 400))
                                        .then(function () {
                                            window.location.reload();
                                        });
                                } else {
                                    Common.toggleLoading("#overrideARAccount", false);
                                    Common.toggleDialogError("#overrideARAccount", "Unknown error occured while overriding locked account.");
                                }
                            });
                        }
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                }
            })

            $("#btnARAccountOverride").off("click").on("click", function (e) {
                e.preventDefault();

                _this.openARAccountOverrideDialog();
            });
        },

        errors: false,
        toggleValidationError: function (field, message) {
            var _this = this;
            var td = field.closest("td");
            if (message == "")
                td.find("span").remove();
            else {
                _this.errors = true;
                td.append("<span class='Required'><br/>" + message + "</span>");
            }
        },
        validatePaymentInfo: function (callback) {
            Common.toggleDialogError("#cc-error", "");
            var _this = this;
            _this.validateName();
            _this.validateCC();
            _this.validateCVV();
            _this.validateExp();
            _this.validateEmail();
            callback(!_this.errors);
        },
        validateName: function () {
            var _this = this;
            var pName = $("#payment-card-name");
            _this.toggleValidationError(pName, "");
            var message = "Name on card is required.";
            if (pName.val() != "")
                message = "";

            _this.toggleValidationError(pName, message);
        },
        validateExp: function () {
            var expYear = $("#payment-card-exp-year");
            var message = "";
            var selectedYear = expYear.val();
            var selectedMonth = $("#payment-card-exp-month").val();
            this.toggleValidationError(expYear, "");
            var today = new Date();
            var currentMonth = new Date(today.getFullYear(), today.getMonth(), 1);
            var expDate = new Date(selectedYear, selectedMonth - 1, 1);
            if (expDate < currentMonth) {
                message = "Expiration date is in the past.";
            }
            this.toggleValidationError(expYear, message);
        },
        validateCVV: function () {
            var cvv = $("#payment-card-cvv");
            var message = "";
            var reqLength = cvv.data("required-length");
            this.toggleValidationError(cvv, "");
            if (reqLength != undefined && reqLength != cvv.val().length) {
                message = "Must be " + reqLength + " characters long.<span>";
            }

            this.toggleValidationError(cvv, message);
        },
        validateCC: function () {
            var _this = this;
            var ccField = $("#payment-card-number");
            var cc = ccField.val();
            if (cc == "") {
                _this.toggleValidationError(ccField, false);
                _this.toggleValidationError(ccField, "Card number is required.");
            }
            else
            {
                Common.wsCall("/Services/Invoice.asmx/ValidateCreditCard", { creditCardNumber: cc }, "POST", function (response) {
                    if (response.Success && JSON.parse(response.Data) > 0) {
                        FinanceAging.ccTypeID = JSON.parse(response.Data);
                        var reqLength = 3;
                        _this.toggleValidationError(ccField, false);
                        if (FinanceAging.ccTypeID == 3)
                            reqLength = 4;
                        $("#payment-card-cvv").data("required-length", reqLength);
                    }
                    else
                    {
                        _this.toggleValidationError(ccField, false);
                        _this.toggleValidationError(ccField, "Invalid card number.");
                    }
                });
            }
        },
        validateEmail: function () {
            var _this = this;
            var pEmail = $("#payment-email");
            _this.toggleValidationError(pEmail, "");
            var message = "Email is required.";
            if (pEmail.val() != "")
                message = "";

            _this.toggleValidationError(pEmail, message);
        },
        insertScript: function (arLevel, openAR) {
            var mainScript = $("#script-main");
            var keepOnFileScript = $("#script-keep-on-file");
            var noPaymentScript = $("#script-no-payment");
            var checkboxTD = $(".ar-dialog-invoices .invoice-checkbox");
            mainScript.html(arLevel.script.replace(/{ARBalance}/g, openAR));
            if (arLevel.noPayment != "") {
                keepOnFileScript.show();
                noPaymentScript.show();
                noPaymentScript.html(arLevel.noPayment);
                checkboxTD.show();
            } else {
                checkboxTD.hide();
                noPaymentScript.hide();
                keepOnFileScript.hide();
            }
        },
        get: function (callback) {
            var _this = this;
            
            if (FinanceAging.arTransactions != "null")
            {
                var pastDue = FinanceAging.arAccountInfo; 
                var scriptHtml = ""; //FinanceAging.ARScript.greeting will be held here...

                //Set currency code on page
                var currencyCode = _this.currencyCode;
                $("span.ar-currency").text(currencyCode);

                $("span.ar-currency").text(currencyCode);

                    var selectedScript = { dialogTitle: "" };

                    //Decide which script and dialog window to show
                    if (_this.showDialog)
                    {
                        if (_this.showBlueDialogOverride){
                            selectedScript = FinanceAging.ARScript.blue;
                        }
                        else if (pastDue.Past150 > 0){
                                selectedScript = FinanceAging.ARScript.red;
                                _this.mustPay = true;
                                $("#btnARPayInvoice").show();
                                $("#btnARPayInvoice").prop('disabled', false);
                        }
                        else if (pastDue.Past46 > 0){
                            selectedScript = FinanceAging.ARScript.yellow;
                            _this.mustPay = false;
                            $("#btnARPayInvoice").show();
                            $("#btnARPayInvoice").prop('disabled', true);
                        }
                        else if (pastDue.Past10 > 0){
                            selectedScript = FinanceAging.ARScript.green;
                            _this.mustPay = false;
                            $("#btnARPayInvoice").show();
                            $("#btnARPayInvoice").prop('disabled', true);
                        }
                    }

                    //Send Script and Total to be processed
                    if (selectedScript.dialogTitle != "") {
                        if (_this.mustPay == true) {
                            _this.insertScript(selectedScript, FinanceAging.arAccountInfo.Past150);
                        }
                        else {
                            _this.insertScript(selectedScript, FinanceAging.arAccountInfo.Total);
                        }
                    }
                    callback(selectedScript);
            }
        },
        footerTotal: function () {
            var _this = this;
            var rows = $(".ar-dialog-invoices tbody tr");
            var footer = $(".ar-dialog-invoices tfoot tr th").eq(2);
            var totalToCharge = 0;
            rows.each(function () {
                var tr = $(this);
                var checkbox = tr.find("input");
                if (checkbox.is(":checked")) {
                    totalToCharge += checkbox.data("balance");
                }
            });

            var textAmount = totalToCharge.toFixed(2);
            var currencyCode = _this.currencyCode;

            footer.text(textAmount);

            if (textAmount > 0)
            {
                $("#btnARPayInvoice span").text(textAmount + " " + currencyCode);
                $("#btnARPayInvoice").prop("disabled", false);
            }
            else
            {
                $("#btnARPayInvoice span").text(textAmount);
                $("#btnARPayInvoice").prop("disabled", true);
            }
        },
        selectInvoicesMustPay: function () {
            var _this = this;
            //footerTotal
            var textAmount = "0.00";
            if (FinanceAging.arAccountInfo.Past150 > 0) {
                var textAmount = FinanceAging.arAccountInfo.Past150;
            }
            var currencyCode = FinanceAging.arAccountInfo.CurrencyCode;

            var rows = $(".ar-dialog-invoices tbody tr");
            if (rows.length == 0) {
                setTimeout(_this.selectInvoicesMustPay, 150);
            } else {
                rows.each(function () {
                    var tr = $(this);
                    var checkbox = tr.find("input");
                    if (checkbox.data("age") >= 150) {
                        checkbox.prop({ checked: true, disabled: true });
                        tr.addClass("must-pay");
                        $(".must-pay-note").show();
                    }
                });

                var rows = $(".ar-dialog-invoices tbody tr");
                var footer = $(".ar-dialog-invoices tfoot tr th").eq(2);
               
                footer.text(textAmount);

                $("#btnARPayInvoice span").text(textAmount + " " + currencyCode);
            }
        },
        buildPastDueInvoiceTable: function (arData, callback) {
            var _this = this;
            var invoiceHtml = "";
            
            $.each(arData, function (i, item) {
                if (item.InvoiceBalance != null && item.InvoiceBalance > 0) {
                    var invoiceRow =
                                "<tr>"
                    if (_this.showBlueDialogOverride == false) {
                        invoiceRow +=
                                    "<td class='invoice-checkbox'>" +
                                    "<input type='checkbox' data-invoice='" + item.InvoiceNo +
                                    "' data-balance='" + item.InvoiceBalance + "' title='Select Invoice To Pay' data-age='" + item.InvoiceAge + "'/></td>";
                    }
                        invoiceRow += 
                                    "<td>" + item.InvoiceNo + "</td>" +
                                    "<td>" + moment(item.InvoiceDate).format("MM/DD/YYYY") + "</td>" +
                                    "<td style='text-align:right'>" + item.InvoiceBalance.toFixed(2) + "</td>" +
                                    "<td style='text-align:right;display:none;'>" + item.InvoiceAge + "</td>" +
                                "</tr>";
                    invoiceHtml += invoiceRow;
                }
            });

            callback(invoiceHtml);
        },
        lockDown: function () {
            //Need to hide links the SMOP
            if (FinanceAging.arAccountInfo.IsRestricted) {
                $(".OToolbar").find("li").not(".allow-always").addClass("ar-over-150");
                if (Common.location !== "" || $("a[id*=hlUpdateStatus]").text() === "Reinstate")
                    $("a[id*=hlUpdateStatus]").addClass("ar-over-150");
                $(ManageInstadose.controls.btnINSReady).addClass("ar-over-150");
            }
        },
        flagAsPaid: function (callback) {
            var _this = this;
            if (_this.mustPay) {
                Common.wsCall("/Services/Account.asmx/ReinstateOnlineAccessViaPayment", { account: Common.account, userName: FinanceAging.username }, "POST", function (response) {
                    if (!response.Success)
                        Common.toggleDialogError((JSON.parse(response.Data)).Message);

                    callback(response.Success);
                });
            } else {
                callback(true);
            }
        },
        openARAccountOverrideDialog: function () {
            Common.toggleDialogError("#overrideARAccount", "");

            $("#txtOverrideARAccountNote").empty();

            $("#overrideARAccount").dialog("open");
        },
        overrideRestrictedAccount: function (note, callback) {
            return $.ajax({
                url: "/Services/Account.asmx/RestrictedAccountOverride",
                type: "POST",
                datatype: "json",
                contentType: "application/json",
                data: JSON.stringify({
                    account: Common.account,
                    noteText: note,
                    userName: FinanceAging.username
                }),
                dataFilter: function (data) {
                    result = FinanceAging.ajaxDataFilter(data);
                    callback(result); 
                }
            });
        },
        processResponse: function (obj) {
            var _this = this;
            
            //if not error was thrown
            if (obj.ErrorMessages == undefined || obj.ErrorMessages.length == 0) {
                //If success messages
                var successInvoices = _this.parseInvoices(obj.SuccessMessages);
                //Store something to let the ar block be overriden
                if (successInvoices.length > 0) {
                    _this.flagAsPaid(function (flagged) {
                        if (flagged) {
                            _this.updateAccountCC(function (updated, message) {
                                if (updated) {
                                    _this.completeProcess(message, obj.SuccessMessages, function (response) {
                                        Common.openAlert("Payment processed", response, 500)
                                            .then(function () { document.location.reload(); });
                                    });
                                }
                            });
                        }
                    });
                }
            } else {
                _this.completeProcess("", obj.ErrorMessages, function (response) {
                    Common.toggleDialogError("#payment-error", response, true);
                });
            }
            $("#ar-dialogLoader").hide();
        },
        parseInvoices: function (messages) {
            var arrInvoices = [];
            $.each(messages, function (i, item) {
                var idxInvoice = item.indexOf("invoice: ") + 9;
                var invoice = item.substring(idxInvoice, idxInvoice + 8);
                arrInvoices.push(invoice);
            });
            return arrInvoices;
        },
        processPayment: function () {
            var _this = this;
            var holderName = $("#payment-card-name").val();
            var cardNumber = $("#payment-card-number").val();
            var cardCVV = $("#payment-card-cvv").val();
            var expMonth = $("#payment-card-exp-month").val();
            var expYear = $("#payment-card-exp-year").val();
            var invoiceFields = $(".ar-dialog-invoices input:checked");
            var email = $("#payment-email").val();
            var invoices = [];
            invoiceFields.each(function () {
                var invoice = $(this).data("invoice");
                var amount = $(this).data("balance");
                invoices.push(invoice + "=" + amount);
            });
            if (!holderName || !cardNumber || !cardCVV || !expMonth || !expYear)
                Common.toggleDialogError("#ar-dialog", "Enter all credit card fields");
            else if (invoiceFields.length == 0)
                Common.toggleDialogError("#ar-dialog", "Select at least 1 invoice.");
            else {

                if ($("#payment-card-keep").is(":checked")) {
                    FinanceAging.useonce = false;
                } else {
                    FinanceAging.useonce = true;
                }
                  

                var dObj = {
                    accountID: Common.account,
                    companyName: FinanceAging.arAccountInfo.AccountName,
                    email: email,
                    ccNumber: cardNumber,
                    expMonth: expMonth,
                    expYear: expYear,
                    cvv: cardCVV,
                    holderName: holderName,
                    cardTypeID: FinanceAging.ccTypeID,
                    invoices: invoices.toString(),
                    currencyCode: FinanceAging.arAccountInfo.CurrencyCode,
                    username: FinanceAging.username,
                    useonce: FinanceAging.useonce
                };

               $.ajax({
                    //url: FinanceAging.instadoseServiceUrl + "/Services/Invoice.asmx/Payment",
                    url: "/Services/Invoice.asmx/InsPayment",
                    type: "POST",
                    data: dObj,
                   dataType: "json",
                   success: function (data) {
                       console.log("Success");
                       console.log(data);
                       //result = FinanceAging.ajaxDataFilter(data.responseText);
                       //FinanceAging.pastDueAR.processResponse(result); 
                       FinanceAging.pastDueAR.processResponse(data); 
                   }, 
                   error: function (jqXHR, textStatus, errorThrown) {
                       console.log("jqXHR:");
                       console.log(jqXHR);
                       console.log("textStatus:");
                       console.log(textStatus);
                       console.log("errorThrown:");
                       console.log(errorThrown);
                       _this.completeProcess("", ["Unknown error was thrown"], function (response) {
                           Common.toggleDialogError("#payment-error", response, true);
                       });
                   }
                });
            }
        },
        completeProcess: function (message, invoiceMesages, callback) {
            var html = "<ul>";
            //Show thrown error
            $.each(invoiceMesages, function (i, item) {
                html += "<li>" + item + "</li>";
            });
            callback(html + message + "</ul>");
        },
        updateAccountCC: function (callback)
        {
            if ($("#payment-card-keep").is(":checked")) {
                //callback(response.Success, "<li>Credit card info saved.</li></ul>");
                callback(true, "<li>Credit card info saved.</li></ul>");
            }
            else {
                //callback(response.Success, "");
                callback(true, "");
            }
        },
        getPayments: function () {
            var _this = this;
            var endDate = moment();
            var startDate = moment().subtract(4, "days");
            try {
                FinanceAging.payments.get(Common.account, "", startDate, endDate, function (response) {
                    $.each(response, function (i, item) {
                        _this.paidInvoices.push(parseInt(item.InvoiceNo));
                    });
                });
            } catch (err) {
                console.log(err);
            }
        },
        overrideCheckbox: function () {
            var _this = this;
            $("td.invoice-checkbox").each(function () {
                var td = $(this);
                var invoice = td.find("input").data("invoice");
                var paid = $.grep(_this.paidInvoices, function (c) { return c == invoice; });
                if (paid.length > 0)
                    td.html("<div title='Pending Payment'>P</span>");
            });
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
    },
};
