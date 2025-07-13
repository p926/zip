var ManageAXCreditRebill = ManageAXCreditRebill || {
    acmCancelReasons: [],
    creditInvoiceID: '',
    selectedAXInvoice: null,
    processAXInvoiceDetails: [],
    userId: "",
    searchButtonID: "",
    configure: function () {
        var _this = this;

        $("#axinvoice-info-dialog").dialog({
            autoOpen: false,
            modal: true,
            width: 1000,
            position: ["top", 50],
            closeOnEscape: false,
            title: "Invoice Info",
            open: function (event, ui) {
                _this.toggleAXInvoiceInfoError(false);
            },
            close: function () {
                _this.toggleAXInvoiceInfoError(false);
            },
            buttons: {
                "Close": function () {
                    $(this).dialog("close");
                }
            }
        });

        $("#axinvoice-details-dialog").dialog({
            autoOpen: false,
            modal: true,
            width: 1400,
            position: ["top", 50],
            closeOnEscape: false,
            title: "Credit Memo",
            open: function (event, ui) {
                _this.toggleAXInvoiceDetailError(false);
            },
            close: function () {
                _this.toggleAXInvoiceDetailError(false);
            },
            buttons: {
                "Submit": function () {
                    _this.toggleAXInvoiceDetailError(false);

                    if (!_this.creditInvoiceID || _this.creditInvoiceID === "") {
                        _this.toggleAXInvoiceDetailError(true, "Invoice Number is not provided.");
                    } else {
                        var validation = _this.generateCreditMemoDetailsFromGrid();

                        if (validation.valid) {
                            if (_this.processAXInvoiceDetails.length <= 0) {
                                _this.toggleAXInvoiceDetailError(true, "Please select item for credit memo process.");
                            } else {
                                _this.openCreditRebillConfirmDialog();
                            }
                        } else {
                            _this.processAXInvoiceDetails = [];
                            _this.toggleAXInvoiceDetailError(true, validation.errorMessage);
                        }
                    }
                },
                "Close": function () {
                    _this.creditInvoiceID = '';
                    _this.selectedAXInvoice = null;

                    $(this).dialog("close");
                }
            }
        });

        $("#axinvoice-rebill-confirm-dialog").dialog({
            autoOpen: false,
            modal: true,
            width: 1100,
            position: ["top", 50],
            closeOnEscape: false,
            title: "Credit & Rebill Confirmation",
            open: function (event, ui) {
                _this.toggleAXRebillConfirmLoading(false);
                _this.toggleAXRebillConfirmError(false);
                $(".btnAXRebillProcess").button("enable");
            },
            close: function () {
                _this.toggleAXRebillConfirmError(false);
            },
            buttons: [
                {
                    id: "btnRebillConfirm",
                    class: "btnAXRebillProcess",
                    text: "Confirm",
                    click: function () {
                        _this.toggleAXRebillConfirmError(false);
                        _this.toggleAXRebillConfirmLoading(true);

                        $(".btnAXRebillProcess").button("disable");

                        _this.processRebill(_this.creditInvoiceID, _this.processAXInvoiceDetails, _this.userId, _this.handleRebillResponse);
                    }
                },
                {
                    id: "btnRebillClose",
                    class: "btnAXRebillProcess",
                    text: "Close",
                    click: function () {
                        $(this).dialog("close");
                    }
                }
            ]
        });

        $("#axinvoice-credit-confirm-dialog").dialog({
            autoOpen: false,
            modal: true,
            width: 400,
            position: ["top", 50],
            closeOnEscape: false,
            title: "Credit Confirmation",
            open: function (event, ui) {
                _this.toggleCreditAXInvoiceLoading(false);
                _this.toggleCreditAXInvoiceError(false);
            },
            close: function () {
                _this.toggleCreditAXInvoiceError(false);
            },
            buttons: {
                "Confirm": function () {
                    _this.toggleCreditAXInvoiceError(false);
                    _this.toggleCreditAXInvoiceLoading(true);

                    _this.processCredit(_this.creditInvoiceID, _this.userId, function (response) {
                        _this.toggleCreditAXInvoiceLoading(false);
                        var responseData = JSON.parse(response.Data);

                        if (response.Success) {
                            $("#axinvoice-credit-confirm-dialog").dialog("close");
                            alert("The credit is processed successfully");
                            _this.refreshInvoiceListGrid();
                        } else {
                            _this.toggleCreditAXInvoiceError(true, "Error - " + responseData.Message);
                        }
                    });
                },
                "Close": function () {
                    $(this).dialog("close");
                }
            }
        });
    },
    refreshInvoiceListGrid: function () {
        $(this.searchButtonID).trigger("click");
    },
    toggleAXInvoiceInfoLoading: function (show) {
        Common.toggleDialogLoading("#axinvoice-info-dialog", show);
    },
    toggleAXInvoiceInfoError: function (show, message) {
        if (!show) {
            Common.toggleDialogError("#axinvoice-info-dialog", "");
        } else {
            Common.toggleDialogError("#axinvoice-info-dialog", message);
        }
    },
    toggleAXInvoiceDetailLoading: function (show) {
        Common.toggleDialogLoading("#axinvoice-details-dialog", show);
    },
    toggleAXInvoiceDetailError: function (show, message) {
        if (!show) {
            Common.toggleDialogError("#axinvoice-details-dialog", "");
        } else {
            Common.toggleDialogError("#axinvoice-details-dialog", message);
        }
    },
    handleRebillResponse: function (response) {
        Common.toggleDialogLoading("#axinvoice-rebill-confirm-dialog", false);
        $(".btnAXRebillProcess").button("enable");

        var responseData = JSON.parse(response.Data);

        if (response.Success) {
            $("#axinvoice-rebill-confirm-dialog").dialog("close");
            $("#axinvoice-details-dialog").dialog("close");
            alert("The credit & rebill is processed successfully");
            ManageAXCreditRebill.refreshInvoiceListGrid();
        } else {
            ManageAXCreditRebill.toggleAXRebillConfirmError(true, "Error - " + responseData.Message);
        }
    },
    generateCreditMemoDetailsFromGrid: function () {
        var _this = this;
        var rtn = {
            valid: true,
            errorMessage: "",
            details: []
        };

        _this.processAXInvoiceDetails = [];

        var tbl = $("#tblAXInvoiceDetails").dataTable();

        var isUpdateExist = false;
        var isNonZeroQtyExist = false;

        $(tbl.fnGetNodes()).each(function (idx, element) {
            var billableEventID = $(element).find(".hdnInvoiceDetailBillableEventID").val();
            var billableEventDetailID = $(element).find(".hdnInvoiceDetailBillableEventDetailID").val();
            var unitPrice = $(element).find(".axInvoiceDetailUnitPrice").val();
            var qty = $(element).find(".axInvoiceDetailQty").val();
            //By the updated rule, Shipment Qty cannot be updated. To revamp Ship Qty, uncomment below line.
            //var shipmentQty = $(element).find(".axInvoiceDetailShipmentQty").val();

            var $cancelReasonCd = $(element).find(".ddlACMCancelReason option:selected");
            var cancelReasonCd = $cancelReasonCd.val();
            var cancelReasonTxt = $cancelReasonCd.text();

            var axInvoiceDetail = null;

            if (unitPrice === "" || isNaN(unitPrice) || Number(unitPrice) < 0) {
                rtn.errorMessage = "Please enter valid unit price.";
                rtn.valid = false;
                return false;
            }

            if (qty === "" || isNaN(qty) || Number(qty) < 0) {
                rtn.errorMessage = "Please enter valid quantity.";
                rtn.valid = false;
                return false;
            }

            //By the updated rule, Shipment Qty cannot be updated. To revamp Ship Qty, uncomment below lines.
            //if (shipmentQty === "" || isNaN(shipmentQty) || Number(shipmentQty) < 0) {
            //    rtn.errorMessage = "Please enter valid shipment quantity.";
            //    rtn.valid = false;
            //    return false;
            //}

            // validate inputs
            axInvoiceDetail = _this.selectedAXInvoice.Details.find(function (detail) {
                return detail.MRNBillableEventDetailID == billableEventDetailID;
            });

            if (!axInvoiceDetail) {
                rtn.errorMessage = "Cannot find invoice detail for items.";
                rtn.valid = false;
                return false;
            }

            //By the updated rule, Shipment Qty cannot be updated. To revamp Ship Qty, uncomment below line, and remove line below that.
            //var isUpdate = axInvoiceDetail.UnitPrice != unitPrice || axInvoiceDetail.Quantity != qty || axInvoiceDetail.ShipmentQty != shipmentQty;
            var isUpdate = axInvoiceDetail.UnitPrice != unitPrice || axInvoiceDetail.Quantity != qty;
            if (isUpdate) {
                isUpdateExist = true;
            }

            //By the updated rule, Shipment Qty cannot be updated. To revamp Ship Qty, uncomment below lines, and remove lines below that.
            //if (Number(qty) > 0 && Number(shipmentQty) > 0) {
            //    isNonZeroQtyExist = true;
            //}
            if (Number(qty) > 0) {
                isNonZeroQtyExist = true;
            }

            _this.processAXInvoiceDetails.push({
                Checked: true,
                MRNBillableEventID: billableEventID,
                MRNBillableEventDetailID: billableEventDetailID,
                MRNOrderID: axInvoiceDetail.MRNOrderID,
                MRNOrderDetailID: axInvoiceDetail.MRNOrderDetailID,
                WearPeriod: axInvoiceDetail.WearPeriod,
                LocationName: axInvoiceDetail.LocationName,
                AXItemID: axInvoiceDetail.AXItemID,
                ItemDescription: axInvoiceDetail.InvItemDescription,
                ServiceStartDate: axInvoiceDetail.ServiceStartDate,
                ServiceEndDate: axInvoiceDetail.ServiceEndDate,
                OriginUnitPrice: axInvoiceDetail.UnitPrice,
                UnitPrice: unitPrice,
                OriginQuantity: axInvoiceDetail.Quantity,
                Quantity: qty,
                OriginShipmentQuantity: axInvoiceDetail.ShipmentQty,
                //By the updated rule, Shipment Qty cannot be updated. To revamp Ship Qty, uncomment below line
                //ShipmentQuantity: shipmentQty,
                ShipmentQuantity: axInvoiceDetail.ShipmentQty,
                ACMCancelReasonCode: isUpdate ? cancelReasonCd : "",
                ACMCancelReasonText: isUpdate ? cancelReasonTxt : ""
            });
        });

        if (!isUpdateExist) {
            rtn.errorMessage = "No difference found. Please input new price, quantity, or shipment quantity for any item.";
            rtn.valid = false;
        }

        if (!isNonZeroQtyExist) {
            rtn.errorMessage = "Every items have either zero quantity or zero shipment quantity. Please input non-zero quantity, or shipment quantity for any item.";
            rtn.valid = false;
        }

        return rtn;
    },
    initInvoiceInfoDialog: function () {
        this.renderInvoiceInfo(null);
        this.renderInvoiceInfoDetailsGrid([]);
    },
    openInvoiceInfoDialog: function (invoiceId) {
        var _this = this;

        _this.initInvoiceInfoDialog();

        $("#axinvoice-info-dialog").dialog("open");

        _this.toggleAXInvoiceInfoLoading(true);

        _this.getInvoice(invoiceId, function (res) {
            _this.toggleAXInvoiceInfoLoading(false);

            var invoice = JSON.parse(res.Data);

            if (res.Success) {
                _this.renderInvoiceInfo(invoice);
                _this.renderInvoiceInfoDetailsGrid(invoice.Details);
            } else {
                _this.renderInvoiceInfo(null);
                _this.renderInvoiceInfoDetailsGrid([]);
                _this.toggleAXInvoiceInfoError(true, "Error - " + invoice.Message);
            }
        });
    },
    renderInvoiceInfo: function (invoice) {
        if (invoice) {
            $("#lblAXInvoiceInfoInvoiceNo").text(invoice.InvoiceID);
            $("#lblAXInvoiceInfoAccount").text(invoice.Account);
            $("#lblAXInvoiceInfoInvoiceDate").text(moment(invoice.InvoiceDate).format("MM/DD/YYYY"));
            $("#lblAXInvoiceInfoAmount").text(invoice.InvoiceAmount + " " + invoice.CurrencyCode);
            $("#lblAXInvoiceInfoDueDate").text(moment(invoice.DueDate).format("MM/DD/YYYY"));
        } else {
            $("#lblAXInvoiceInfoInvoiceNo").text("");
            $("#lblAXInvoiceInfoAccount").text("");
            $("#lblAXInvoiceInfoInvoiceDate").text("");
            $("#lblAXInvoiceInfoAmount").text("");
            $("#lblAXInvoiceInfoDueDate").text("");
        }
    },
    renderInvoiceInfoDetailsGrid: function (invoiceDetails) {
        var _this = this;

        $("#tblAXInvoiceInfoDetails").dataTable({
            "oLanguage": {
                "sEmptyTable": "No invoice detail found."
            },
            "lengthChange": false,
            "paging": false,
            "scrollY": 500,
            "scrollCollapse": true,
            "data": invoiceDetails,
            "order": [[5, 'asc']],
            renderer: "bootstrap",
            destroy: true,
            "columns": [
                {
                    "data": "AXItemID",
                    "width": "70px"
                },
                { "data": "InvItemDescription" },
                {
                    "data": "WearPeriod",
                    "width": "80px"
                },
                {
                    "data": "ServiceStartDate",
                    "width": "80px",
                    "render": function (data, type, row) {
                        return moment(data).format("MM/DD/YYYY");
                    }
                },
                {
                    "data": "ServiceEndDate",
                    "width": "80px",
                    "render": function (data, type, row) {
                        return moment(data).format("MM/DD/YYYY");
                    }
                },
                {
                    "data": "LocationName",
                    "width": "55px"
                },
                {
                    "data": "UnitPrice",
                    "width": "65px",
                    "render": function (data, type, row) {
                        if (data && !isNaN(data)) {
                            return data.toFixed(2);
                        } else {
                            return "0.00";
                        }
                    }
                },
                {
                    "data": "Quantity",
                    "width": "65px"
                },
                {
                    "data": "ShipmentQty",
                    "width": "65px"
                }
            ]
        });
    },
    initInvoiceDetailsDialog: function () {
        this.renderInvoice(null);
        this.renderInvoiceDetailsGrid([]);
    },
    openInvoiceDetailsDialog: function (invoiceId) {
        var _this = this;

        _this.creditInvoiceID = invoiceId;
        _this.selectedAXInvoice = null;

        _this.initInvoiceDetailsDialog();

        $("#axinvoice-details-dialog").dialog("open");

        _this.toggleAXInvoiceDetailLoading(true);

        _this.getInvoice(invoiceId, function (res) {
            _this.toggleAXInvoiceDetailLoading(false);

            var invoice = JSON.parse(res.Data);

            if (res.Success) {
                _this.selectedAXInvoice = invoice;
                _this.renderInvoice(invoice);
                _this.renderInvoiceDetailsGrid(invoice.Details);
            } else {
                _this.renderInvoiceDetailsGrid(null);
                _this.renderInvoiceDetailsGrid([]);
                _this.toggleAXInvoiceDetailError(true, "Error - " + invoice.Message);
            }
        });
    },
    renderInvoice: function (invoice) {
        if (invoice) {
            $("#lblAXInvoiceNo").text(invoice.InvoiceID);
            $("#lblAXInvoiceAccount").text(invoice.Account);
            $("#lblAXInvoiceDate").text(moment(invoice.InvoiceDate).format("MM/DD/YYYY"));
            $("#lblAXInvoiceAmount").text(invoice.InvoiceAmount + " " + invoice.CurrencyCode);
            $("#lblAXInvoiceDueDate").text(moment(invoice.DueDate).format("MM/DD/YYYY"));
        } else {
            $("#lblAXInvoiceNo").text("");
            $("#lblAXInvoiceAccount").text("");
            $("#lblAXInvoiceDate").text("");
            $("#lblAXInvoiceAmount").text("");
            $("#lblAXInvoiceDueDate").text("");
        }
    },
    renderInvoiceDetailsGrid: function (invoiceDetails) {
        var _this = this;

        var acmCancelReasonsSelectBox = "";
        if (_this.acmCancelReasons.length > 0) {
            var options = _this.acmCancelReasons.reduce(function (rtn, cancelReason) {
                return rtn + "<option value='" + cancelReason.CancelReasonCode + "'>" + cancelReason.Description + "</option>";
            }, "");

            acmCancelReasonsSelectBox = "<select class='ddlACMCancelReason'>" + options + "</select>";
        }

        $("#tblAXInvoiceDetails").dataTable({
            "oLanguage": {
                "sEmptyTable": "No invoice detail found."
            },
            "lengthChange": false,
            "paging": false,
            "scrollY": 500,
            "scrollCollapse": true,
            "data": invoiceDetails,
            "order": [[5, 'asc']],
            renderer: "bootstrap",
            destroy: true,
            "columns": [
                {
                    "data": "AXItemID",
                    "width": "90px",
                    "render": function (data, type, row) {
                        return "<input type='hidden' class='hdnInvoiceDetailBillableEventID' value='" + row.MRNBillableEventID + "' /> <input type='hidden' class='hdnInvoiceDetailBillableEventDetailID' value='" + row.MRNBillableEventDetailID + "' />" + data;
                    }
                },
                { "data": "InvItemDescription" },
                {
                    "data": "WearPeriod",
                    "width": "75px"
                },
                {
                    "data": "ServiceStartDate",
                    "width": "55px",
                    "render": function (data, type, row) {
                        return moment(data).format("MM/DD/YYYY");
                    }
                },
                {
                    "data": "ServiceEndDate",
                    "width": "55px",
                    "render": function (data, type, row) {
                        return moment(data).format("MM/DD/YYYY");
                    }
                },
                {
                    "data": "LocationName",
                    "width": "55px"
                },
                {
                    "data": "UnitPrice",
                    "width": "65px",
                    "render": function (data, type, row) {
                        if (data && !isNaN(data)) {
                            return data.toFixed(2);
                        } else {
                            return "0.00";
                        }
                    }
                },
                {
                    "data": "UnitPrice",
                    "width": "75px",
                    "render": function (data, type, row) {
                        return "<input type='text' class='axInvoiceDetailUnitPrice' value='" + data.toFixed(2) + "' style='width: 65px;' />";
                    }
                },
                {
                    "data": "Quantity",
                    "width": "45px"
                },
                {
                    "data": "Quantity",
                    "width": "55px",
                    "render": function (data, type, row) {
                        return "<input type='text' class='axInvoiceDetailQty' value='" + data + "' style='width: 45px;' />";
                    }
                },
                {
                    "data": "ShipmentQty",
                    "width": "50px"
                },
                //{
                //By the updated rule, Shipment Qty cannot be updated. To revamp Ship Qty, uncomment below lines.
                //    "data": "ShipmentQty",
                //    "width": "55px",
                //    "render": function (data, type, row) {
                //        return "<input type='text' class='axInvoiceDetailShipmentQty' value='" + data + "' style='width: 45px;' />";
                //    }
                //},
                {
                    "data": null,
                    "width": "80px",
                    "render": function () {
                        return acmCancelReasonsSelectBox;
                    }
                }
            ]
        });
    },
    openCreditRebillConfirmDialog: function () {
        var _this = this;

        $("#axinvoice-rebill-confirm-dialog").dialog("open");
        _this.renderCreditConfirm(_this.selectedAXInvoice);

        var checkedDetails = [];

        if (_this.processAXInvoiceDetails && _this.processAXInvoiceDetails.length > 0)
            checkedDetails = _this.processAXInvoiceDetails.filter(function (dtl) {
                return dtl.Checked === true;
            });

        _this.renderCreditConfirmDetailsGrid(checkedDetails);
    },
    renderCreditConfirm: function (invoice) {
        if (invoice) {
            creditTypeText = "Credit & Rebill";

            $("#lblAXInvoiceNoConfirm").text(invoice.InvoiceID);
            $("#lblAXInvoiceAccountConfirm").text(invoice.Account);
            $("#lblAXInvoiceDateConfirm").text(moment(invoice.InvoiceDate).format("MM/DD/YYYY"));
            $("#lblAXInvoiceAmountConfirm").text(invoice.InvoiceAmount + " " + invoice.CurrencyCode);
            $("#lblAXInvoiceDueDateConfirm").text(moment(invoice.DueDate).format("MM/DD/YYYY"));
            $("#lblAXInvoiceCreditTypeConfirm").text(creditTypeText);
        } else {
            $("#lblAXInvoiceNoConfirm").text("");
            $("#lblAXInvoiceAccountConfirm").text("");
            $("#lblAXInvoiceDateConfirm").text("");
            $("#lblAXInvoiceAmountConfirm").text("");
            $("#lblAXInvoiceDueDateConfirm").text("");
            $("#lblAXInvoiceCreditTypeConfirm").text("");
        }
    },
    renderCreditConfirmDetailsGrid: function (details) {
        $("#tblAXInvoiceConfirmDetails").dataTable({
            "oLanguage": {
                "sEmptyTable": "No credit detail found."
            },
            "lengthChange": false,
            "paging": false,
            "scrollY": 500,
            "scrollCollapse": true,
            "data": details,
            "order": [[5, 'asc']],
            "bFilter": false,
            renderer: "bootstrap",
            destroy: true,
            "columns": [
                {
                    "data": "AXItemID",
                    "width": "80px"
                },
                { "data": "ItemDescription" },
                {
                    "data": "WearPeriod",
                    "width": "70px"
                },
                {
                    "data": "ServiceStartDate",
                    "width": "55px",
                    "render": function (data, type, row) {
                        return moment(data).format("MM/DD/YYYY");
                    }
                },
                {
                    "data": "ServiceEndDate",
                    "width": "55px",
                    "render": function (data, type, row) {
                        return moment(data).format("MM/DD/YYYY");
                    }
                },
                {
                    "data": "LocationName",
                    "width": "55px"
                },
                {
                    "data": "UnitPrice",
                    "width": "65px"
                },
                {
                    "data": "Quantity",
                    "width": "45px"
                },
                {
                    "data": "ShipmentQuantity",
                    "width": "50px"
                },
                {
                    "data": "ACMCancelReasonText",
                    "width": "80px"
                }
            ]
        });
    },
    toggleAXRebillConfirmLoading: function (show) {
        Common.toggleDialogLoading("#axinvoice-rebill-confirm-dialog", show);
    },
    toggleAXRebillConfirmError: function (show, message) {
        if (!show) {
            Common.toggleDialogError("#axinvoice-rebill-confirm-dialog", "");
        } else {
            Common.toggleDialogError("#axinvoice-rebill-confirm-dialog", message);
        }
    },
    toggleCreditAXInvoiceLoading: function (show) {
        Common.toggleDialogLoading("#axinvoice-credit-confirm-dialog", show);
    },
    toggleCreditAXInvoiceError: function (show, message) {
        if (!show) {
            Common.toggleDialogError("#axinvoice-credit-confirm-dialog", "");
        } else {
            Common.toggleDialogError("#axinvoice-credit-confirm-dialog", message);
        }
    },
    openCreditInvoiceConfirm: function (invoiceId) {
        var _this = this;

        _this.creditInvoiceID = invoiceId;

        _this.toggleCreditAXInvoiceLoading(false);
        _this.toggleCreditAXInvoiceError(false);

        $("#spnAXInvoiceCreditConfirmMessage").text("Would you process credit for invoice id " + invoiceId + "?");

        $("#axinvoice-credit-confirm-dialog").dialog("open");
    },
    getInvoice: function (invoiceId, callback) {
        Common.wsCall(
            '/Services/AXCreditRebill.asmx/GetInvoice',
            { invoiceId: invoiceId },
            'POST',
            callback
        );
    },
    processCredit: function (invoiceId, userId, callback) {
        Common.wsCall(
            '/Services/AXCreditRebill.asmx/ProcessCredit',
            { 
                invoiceId: invoiceId,
                userId: userId
            },
            'POST',
            callback
        );
    },
    processRebill: function (invoiceId, details, userId, callback) {
        Common.wsCall(
            '/Services/AXCreditRebill.asmx/ProcessRebill',
            {
                invoiceId: invoiceId,
                details: details,
                userId: userId
            },
            'POST',
            callback
        );
    }
};
