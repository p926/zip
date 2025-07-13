<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InformationFinder_Details_Account" EnableEventValidation="false" CodeBehind="Account.aspx.cs" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.3500.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>
<%@ Register Src="~/Controls/ARDialog.ascx" TagPrefix="bc" TagName="ARDialog" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <%--CSS FILE THAT OVERRIDES DEFAULT RADTABSTRIP--%>
    <script type="text/javascript" src="../../scripts/datatables/jquery.dataTables.min.js"></script>
    <script type="text/javascript" src="../../scripts/datatables/dataTables.jqueryui.js"></script>
    <script type="text/javascript" src="../../scripts/moment.js"></script>
    <script src="../../scripts/instadose.finance.js"></script>
    <script src="../../scripts/instadose.calendar.js" type="text/javascript"></script>
    <script src="../../scripts/mirion-dsd.users.js" type="text/javascript"></script>
    <script type="text/javascript" src="../../scripts/mirion-dsd.finance.js"></script>


    <link href="../../css/rad-controls/TabStrip.MyMirion.css" rel="stylesheet" />
    <link href="../../css/datatables/css/jquery.dataTables_themeroller.css" rel="stylesheet" />

    <style type="text/css">
        /* Override OTable Class */
        /* Orange border around RadTabContainer */
        #radTabContainer.OTable {
            margin: 0px 0px 0px 0px;
            padding: 5px 5px 5px 5px;
            width: auto;
        }

        /* Display CSS Class for Loading Animation. */
        #updateProgressBar {
            text-align: center;
            font-family: Verdana;
            font-size: 14px;
            line-height: 25px;
            vertical-align: top;
            font-weight: bold;
            color: #FF4E00;
            height: 50px;
            width: 100%;
        }

        /* Local override of CreditMemo CSS Class. */
        .OToolbar .Icon.CreditMemo {
            background-image: url('../../images/icons/creditcards.png');
        }

        /* CSS for Right Aligning "Clear Filter" link. */
        li.RightAlign {
            float: right;
            padding-right: 10px;
        }

        .gridView {
            max-height: 350px;
            overflow: auto;
            margin-top: 10px;
        }

        /* Fixed gridview header*/
        .FixedHeader {
            position: absolute;
            margin: -10px 0px 0px 0px;
            /*z-index:99;*/
        }
        .Hidden {
            display:none;
        }
        /* CSS Override for RadGrid_Default. */
        div.RadGrid_Default {
            border: 1px solid #D6712D;
            color: #333333;
        }

        div.RadGrid_Default th.rgHeader {
            background: url('../../css/dsd-default/images/o-toolbar.png') repeat-x #D6712D;
            background-color: #D6712D;
            border-bottom: 1px solid #D6712D;
            border-right: 1px solid #D6712D;
            font-family: Arial, sans-serif;
            font-weight: bold;
            color: #FFFFFF;
        }

        div.RadGrid_Default th.rgHeader a {
            font-style: normal;
            color: #FFFFFF;
        }

        div.RadGrid_Default th.rgHeader a:hover {
            text-decoration: underline;
        }

        div.RadGrid_Default .rgAltRow {
            background-color: #f9e4cb;
            color: #333333;
        }

        /* Fixes Background Color of Hover state for Alternating Rows. */
        div.RadGrid_Default tr.rgAltRow:hover,
        div.RadGrid_Default tr.rgAltRow:active {
            background-color: #C3C3C3 !important;
        }

        div.RadGrid_Default tr.rgSelectedRow {
            background-color: #808080 !important;
            color: #FFFFFF;
        }

        .distributor-highlight {
            font-size: 1.3em;
            font-weight: bold;
            background-color: rgba(162,232,6,1);
        }

        .collector {
            display: none;
        }

        .OverrideLabel .Label {
            width: 105px !important;
        }

        .ar-indicator {
            background-image: none;
            text-shadow: 0 -1px 0 rgba(0, 0, 0, 0.25);
            line-height: 11px !important;
            font-size: 12px;
        }

        .ar-indicator-green,
        .ar-indicator-green:hover,
        .ar-indicator-green:focus,
        .ar-indicator-green:active {
            background-color: #33cc33;
            color: #ffffff;
        }

        .ar-indicator-yellow,
        .ar-indicator-yellow:hover,
        .ar-indicator-yellow:focus,
        .ar-indicator-yellow:active {
            background-color: #ffea00;
            color: #000000;
        }

        .ar-indicator-red,
        .ar-indicator-red:hover,
        .ar-indicator-red:focus,
        .ar-indicator-red:active {
            background-color: #ff3300;
            color: #ffffff;
        }
    </style>
    <script type="text/javascript" lang="javascript">
        var cookies = ["Notes", "CSRequests", "Orders", "Invoices", "Locations", "Users", "Badges", "Returns", "Reads", "Documents", "AssociatedAccounts", "Calendars", "ActiveLocations", "ActiveUsers", "ActiveBadges"];
        var tabLabels = ["Notes", "CSRs", "Orders", "Invoices", "Locations", "Users", "Badges", "Returns", "Reads", "Documents", "AssociatedAccounts", "Calendars"];
        var tabFields = ["#rtNotes .rtsTxt", "#rtCSRequests .rtsTxt", "#rtOrders .rtsTxt", "#rtInvoices .rtsTxt", "#rtLocations .rtsTxt", "#rtUsers .rtsTxt", "#rtBadges .rtsTxt", "#rtReturns .rtsTxt", "#rtReads .rtsTxt", "#rtDocuments .rtsTxt", "#rtAssociatedAccounts .rtsTxt", "#rtCalendars .rtsTxt",
            "[id*=lblActiveTotalLocations]", "[id*=lblActiveTotalUsers]", "[id*=lblActiveTotalBadges]"];
        var accID = document.location.search.replace("?ID=", "");
        var running = false;

        Common.account = accID;

        ManageCalendar.accountID = accID;
        ManageCalendar.calendarListGrid = "<%= rgCalendars.ClientID %>";

        function pageLoad(sender, args) {
            $(document).ready(function () {

                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
                JQueryControlsLoad();
                $(scrollTopGridView).scrollTop(0);

                //FinanceAging.getARTransactionsAging(Common.account);
                User.configure();
            });
        }
        function getCounts() {
            var cookieUpdated = false;
            if (!running) {
                running = true;
                $.get("/services/misc.asmx/GetAccountCounts", { accountid: accID }, function (data) {
                    for (i = 0; i < cookies.length; i++) {
                        var currentCookie = getCookie(cookies[i]);
                        if (currentCookie !== data[cookies[i]]) {
                            createCookie(cookies[i], data[cookies[i]], 0.0006);
                            cookieUpdated = true;
                        }
                    }
                    if (cookieUpdated) {
                        running = false;
                        updateCounts();
                    }
                });
            }
        }
        function loadCounts() {
            var accCookie = getCookie("account");
            if (accCookie != accID) {
                for (i = 0; i < cookies.length; i++) {
                    createCookie(cookies[i], 0, 0.0006);
                }
            }
            if (getCookie("Locations") == 0) {
                getCounts();
                createCookie("account", accID, 0);
            }
            updateCounts();
        }
        function updateCounts() {
            for (i = 0; i < cookies.length; i++) {
                var data = getCookie(cookies[i]);
                var text = tabLabels[i] + " (" + data + ")";
                if (i >= tabLabels.length) {
                    var total = getCookie(cookies[i].replace("Active", ""));
                    var active = data;
                    text = "Active: " + active + "/" + (total - active) + " Total: " + total;
                }
                $(tabFields[i]).text(text);
            }
        }
        function createCookie(name, value, days) {
            var expires;
            if (days) {
                var date = new Date();
                date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                expires = "; expires=" + date.toGMTString();
            }
            else {
                expires = "";
            }
            document.cookie = name + "=" + value + expires + "; path=/";
        }
        function getCookie(c_name) {
            if (document.cookie.length > 0) {
                var c_start = document.cookie.indexOf(c_name + "=");
                if (c_start !== -1) {
                    c_start = c_start + c_name.length + 1;
                    var c_end = document.cookie.indexOf(";", c_start);
                    if (c_end === -1) {
                        c_end = document.cookie.length;
                    }
                    return unescape(document.cookie.substring(c_start, c_end));
                }
            }
            return "";
        }

        function JQueryControlsLoad() {
            loadCounts();
            Common.configure();
            Finance.collections.configure();
            ManageCalendar.configure();
            //added funtionality - TH
            FinanceAging.configure(Common.account);

            var hfUsername = $("#hfUsername").val();

            if (hfUsername && hfUsername.length >= 2 && hfUsername !== "Unknown") {
                ManageCalendar.initial = $("#hfUsername").val().substring(0, 2).toLowerCase();
            }

            $(".has-date-picker").datepicker({
                changeMonth: true,
                changeYear: true
            });

            var orderAckSent = false;

            // Accordion
            $("#accordion").accordion({
                header: "h3",
                autoHeight: false
            });

            $('#createNoteDialog').dialog({
                autoOpen: false,
                width: 390,
                resizable: false,
                title: "Create Note",
                open: function (type, data) {

                    $(this).parent().appendTo("form");
                    //$('.ui-dialog :input').focus();
                },
                buttons: {
                    "Ok": function () {
                        $('#<%= btnAddNote.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancelNote.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });


            $('#createAssociatedAccountDialog').dialog({
                autoOpen: false,
                width: 450,
                resizable: false,
                title: "Create Associated Accounts",
                open: function (type, data) {

                    $(this).parent().appendTo("form");
                    // $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Search": function () {
                        $('#<%= btnSearchAssociatedAccount.ClientID %>').click();
                    },
                    "Add": function () {
                        $('#<%= btnAddAssociatedAccount.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $('#<%= btnCancelAssociatedAccount.ClientID %>').click();

                    }
                },
                close: function () {
                    $('#<%= btnCancelAssociatedAccount.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });

            var custCode = $(".customer-code");
            if (custCode.text() === "DIST" || custCode.text() === "IDST")
                custCode.closest("td").addClass("distributor-highlight")

            $('#addDocumentDialog').dialog({
                autoOpen: false,
                width: 550,
                height: 350,
                resizable: false,
                title: "Add Document",
                modal: true,
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog textarea').focus();
                },
                buttons: {
                    "Add": function () {
                        $('#<%= btnAddDocument.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            $('#emailOrderAcknowledgementDialog').dialog({
                autoOpen: false,
                width: 500,
                resizable: false,
                title: "Email Order Acknowledgement",
                open: function (type, data) {
                    orderAckSent = false;
                    $(this).parent().appendTo("form");
                    $('#<%= btnLoadEmailOrderAck.ClientID %>').click();
                    console.log(date);
                    //$('.ui-dialog :input').focus();
                },
                buttons: {
                    "Send": function () {
                        $('#<%= btnExecuteSendEmail.ClientID %>').click();

                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    },
                },
                close: function () {
                    $('#<%= btnCancelSendEmail.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });

            $('#emailOrderAcknowledgementSuccessDialog').dialog({
                autoOpen: false,
                width: 500,
                resizable: false,
                title: "Email Order Acknowledgement",
                open: function (type, data) {
                    orderAckSent = false;
                    $(this).parent().appendTo("form");
                    $('#<%= btnLoadEmailOrderAckSuccess.ClientID %>').click();
                    $('.ui-dialog :input').focus();

                },
                buttons: {
                    "Close Window": function () {
                        $(this).dialog("close");
                    },
                },
                close: function () {
                    $('#<%= btnCancelSendEmail.ClientID %>').click();
                    if (!orderAckSent) $('.ui-overlay').fadeOut();
                }
            });

            $('#assignedWearerDialog').dialog({
                autoOpen: false,
                width: 500,
                resizable: false,
                modal: true,
                title: "Select Wearer for Replaced Device",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Assign": function () {
                        $('#<%= btnAssignedWearer.ClientID %>').click();
                    },
                    "Close": function () {
                        $(this).dialog("close");
                    },
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            $("#ax-invoice-orders-dialog").dialog({
                autoOpen: false,
                width: 200,
                resizable: false,
                modal: true,
                title: "Invoice Orders",
                open: function (type, data) {

                },
                buttons: {
                    "Close": function () {
                        $(this).dialog("close");
                    },
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            // selected check box for assignning devices.
            $('.chkbxHeaderDeviceList').click(function () {
                var ischecked_HeaderDeviceList = ($('#ctl00_primaryHolder_gv_DeviceList_ctl01_chkbxHeaderDeviceList').is(":checked"));

                var hfRMAType = $("#HidRMAType").val();

                if (hfRMAType === "Recall") {
                    $('#ctl00_primaryHolder_gv_DeviceList input:checkbox[id$=chkbxSelectDeviceList]').attr("checked", ischecked_HeaderDeviceList);
                    $('#ctl00_primaryHolder_gv_DeviceList_ctl01_chkbxHeaderReplaceDeviceList').attr("checked", ischecked_HeaderDeviceList);
                    $('#ctl00_primaryHolder_gv_DeviceList input:checkbox[id$=chkbxSelectReplaceDeviceList]').attr("checked", ischecked_HeaderDeviceList)
                }
                else {
                    // add attribute 
                    $('#ctl00_primaryHolder_gv_DeviceList input:checkbox[id$=chkbxSelectDeviceList]').attr("checked", ischecked_HeaderDeviceList);

                    if (ischecked_HeaderDeviceList === false) {
                        $('#ctl00_primaryHolder_gv_DeviceList_ctl01_chkbxHeaderReplaceDeviceList').attr("checked", ischecked_HeaderDeviceList);
                        $('#ctl00_primaryHolder_gv_DeviceList input:checkbox[id$=chkbxSelectReplaceDeviceList]').attr("checked", ischecked_HeaderDeviceList)
                    }
                }

            });

            // selected check box for assignning replacement devices.
            $('.chkbxHeaderReplaceDeviceList').click(function () {
                var ischecked_HeaderReplaceDeviceList = ($('#ctl00_primaryHolder_gv_DeviceList_ctl01_chkbxHeaderReplaceDeviceList').is(":checked"));
                var ischecked_HeaderDeviceList = ($('#ctl00_primaryHolder_gv_DeviceList_ctl01_chkbxHeaderDeviceList').is(":checked"));

                var hfRMAType = $("#HidRMAType").val();

                if (hfRMAType === "Recall") {
                    $('#ctl00_primaryHolder_gv_DeviceList input:checkbox[id$=chkbxSelectReplaceDeviceList]').attr("checked", ischecked_HeaderReplaceDeviceList);

                    $('#ctl00_primaryHolder_gv_DeviceList_ctl01_chkbxHeaderDeviceList').attr("checked", ischecked_HeaderReplaceDeviceList);
                    $('#ctl00_primaryHolder_gv_DeviceList input:checkbox[id$=chkbxSelectDeviceList]').attr("checked", ischecked_HeaderReplaceDeviceList)
                }
                else {
                    if (ischecked_HeaderDeviceList === true) {
                        // add attribute 
                        $('#ctl00_primaryHolder_gv_DeviceList input:checkbox[id$=chkbxSelectReplaceDeviceList]').attr("checked", ischecked_HeaderReplaceDeviceList);
                    }
                    else {
                        $('#ctl00_primaryHolder_gv_DeviceList_ctl01_chkbxHeaderReplaceDeviceList').attr("checked", false)
                        $('#ctl00_primaryHolder_gv_DeviceList input:checkbox[id$=chkbxSelectReplaceDeviceList]').attr("checked", false)
                    }
                }

            });

        }

        function validateNote() {

            $("#noteValidation").empty();
            var note = $('#<%= txtNote.ClientID %>').val();            

            let invalidCharacters = ["<", ">"];
            let hasInvalidCharacters = checkCharactersInText(note, invalidCharacters);

            if (hasInvalidCharacters) {
                $("#noteValidation").text("Some characters are not allowed, for example: < >.");
                return false;
            }

            if (note.length > 1450) {
                $("#noteValidation").text("Note cannot be more than 1450 characters.");
                return false;
            }

            return true;
        }

        function checkCharactersInText(text, characters) {
            var result = false;
            $.each(characters, function (key, val) {
                if (text.includes(val)) {
                    result = true;
                }                    
            });

            return result;
        }

        function chkbxSelectDeviceList_Click(objRef) {

            var hfRMAType = $("#HidRMAType").val();

            ////Get the reference of row
            var row = objRef.parentNode.parentNode.parentNode;

            //Get all input elements in row
            var inputList = row.getElementsByTagName("input");
            var curSelectDevice = inputList[0];
            var curSelectReplaceDevice = inputList[3];

            if (hfRMAType === "Recall") {
                curSelectReplaceDevice.checked = curSelectDevice.checked;
            }
            else {
                if (!curSelectDevice.checked) {
                    curSelectReplaceDevice.checked = false;
                }
            }
        }

        function chkbxSelectReplaceDeviceList_Click(objRef) {

            var hfRMAType = $("#HidRMAType").val();

            ////Get the reference of row
            var row = objRef.parentNode.parentNode.parentNode;

            //Get all input elements in row
            var inputList = row.getElementsByTagName("input");
            var curSelectDevice = inputList[0];
            var curSelectReplaceDevice = inputList[3];

            if (hfRMAType === "Recall") {
                curSelectDevice.checked = curSelectReplaceDevice.checked;
            }
            else {
                if (curSelectReplaceDevice.checked) {
                    curSelectDevice.checked = true;
                }
            }
        }

        // Function that displayes Invoice Details.
        // Executes only when Magnifying Glass is clicked in Invoice RadGrid.
        function openInvoiceDetailsDialog(id) {
            $('#invoiceDetailsDialog').dialog({
                autoOpen: false,
                width: 500,
                resizable: false,
                title: "Invoice Details for: " + id,
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "OK": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });
            openDialog('invoiceDetailsDialog');
        }

        // Function that displayes Lost/Replacement/Recall order creation.
        // Executes only when clicking on the linkbutton.
        function openIDPlusLostReplacementRecallOrderDialog(pTitle) {
            $('#IDPlusLostReplacementRecallOrderDialog').dialog({
                autoOpen: false,
                width: 650,
                resizable: false,
                modal: true,
                title: pTitle,
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Create Order": function () {
                        $('#<%= btnCreateLostReplacementRecallOrder.ClientID %>').click();
                    },
                    "Close": function () {
                        $(this).dialog("close");
                    },
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            openDialog('IDPlusLostReplacementRecallOrderDialog');
        }

        function RadioSelectUserCheck(rb) {
            var gv = document.getElementById("<%=gv_WearerList.ClientID%>");
            var rbs = gv.getElementsByTagName("input");

            var row = rb.parentNode.parentNode;
            for (var i = 0; i < rbs.length; i++) {
                if (rbs[i].type === "radio") {
                    if (rbs[i].checked && rbs[i] !== rb) {
                        rbs[i].checked = false;
                        break;
                    }
                }
            }
        }

        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        }

        function closeDialog(id) {
            $('#' + id).dialog("close");
        }

        function ViewRMApdf(returnID) {
            var screenW = screen.width - 200;
            var screenH = screen.height - 100;
            var winProperty = 'width=' + screenW + ',height=' + screenH + ',scrollbars=yes,resizable=yes,menubar=no,status=no,location=no';
            window.open('Return_ViewRmaLetter.aspx?id=' + returnID, 'PDFletter', winProperty)
        }

        function EmailOrderAck(AccountID) {
            var screenW = 500;
            var screenH = 350;
            var winProperty = 'width=' + screenW + ',height=' + screenH + ',linkbar=no,toolbar=no,scrollbars=no,resizable=yes,menubar=no,status=no,location=no';
            window.open('../EmailOrderAck.aspx?id=' + AccountID, 'OrderAck', winProperty)
        }

        // Open Link into New Window.
        function openNewWindow(pageUrl) {
            window.open(pageUrl, '_blank');
        }

        function onTabSelecting(sender, args) {
            if (args.get_tab().get_pageViewID()) {
                args.get_tab().set_postBack(false);
            }
        }

        // below functions used by the rgAssociatedAccount grid to save the selected row
        function RowSelected(sender, eventArgs) {
            var grid = sender;
            var MasterTable = grid.get_masterTableView();
            var row = MasterTable.get_dataItems()[eventArgs.get_itemIndexHierarchical()];
            var cell = MasterTable.getCellByColumnUniqueName(row, "accountid");


            $('#<% =hselectedID.ClientID %>').val(cell.innerHTML);
            //here cell.innerHTML holds the value of the cell  
        }

        function checkSelecteditem() {
            try {
                var grid = $find("<%=rgAssociatedAccount.ClientID %>");
                var MasterTable = grid.get_masterTableView();
                var selectedRows = MasterTable.get_selectedItems();
                if (selectedRows.length === 0)
                    return false;
                else
                    return confirm('confirm to remove the selected account association?');

            }
            catch (err) { return false; }

        }

        function GetSelectedNames() {
            var grid = $find("<%=rgAssociatedAccount.ClientID %>");
            var MasterTable = grid.get_masterTableView();
            var selectedRows = MasterTable.get_selectedItems();
            for (var i = 0; i < selectedRows.length; i++) {
                var row = selectedRows[i];
                var cell = MasterTable.getCellByColumnUniqueName(row, "accountid");
                //here cell.innerHTML holds the value of the cell    
            }
        }

        function openAXInvoiceOrderDialog(invoiceID) {
            $("#ax-invoice-orders-dialog").dialog("open");
            Common.toggleDialogError("#ax-invoice-orders-dialog", "");
            Common.toggleDialogLoading("#ax-invoice-orders-dialog", true);

            axInvoiceOrderListAjaxCall(invoiceID)
                .done(function (rtn) {
                    if (rtn.Success) {
                        loadAXInvoiceOrderListGrid(rtn.Orders);
                    } else {
                        Common.toggleDialogError("#ax-invoice-orders-dialog", "Erorr orccured while getting orders for the invoice.");
                    }
                })
                .fail(function (jqXHR, status) {
                    Common.toggleDialogError("#ax-invoice-orders-dialog", "Erorr orccured while getting orders for the invoice.");
                })
                .always(function () {
                    Common.toggleDialogLoading("#ax-invoice-orders-dialog", false);
                });
        }

        function axInvoiceOrderListAjaxCall(invoiceID) {
            return $.ajax({
                url: "/Services/Invoice.asmx/GetAXInvoiceOrderList",
                type: "POST",
                datatype: "json",
                contentType: "application/json",
                data: JSON.stringify({
                    invoiceID: invoiceID
                }),
                dataFilter: function (data) {
                    return Common.ajaxDataFilter(data)
                }
            });
        }

        function loadAXInvoiceOrderListGrid(orderIDs) {
            var $tbody = $("#tbl-ax-invoice-orders tbody");

            if ($tbody) {
                $tbody.empty();

                var tbodyHTML = "";

                if (orderIDs && orderIDs.length > 0) {
                    orderIDs.forEach(function (item, idx) {
                        tbodyHTML += "<tr><td><a href='/CustomerService/ReviewOrder.aspx?ID=" + item + "' target='_blank'>" + item + "</a></td></tr>";
                    });
                } else {
                    tbodyHTML = "<tr><td>No Order</td></tr>";
                }

                $tbody.append(tbodyHTML);
            }
        }

    </script>
    <%--RADSCRIPT/JAVASCRIPT that limits the type of FILTERS--%>
    <telerik:RadCodeBlock ID="RadCodeBlock0" runat="server">
        <script type="text/javascript">
            var column = null;
            function MenuShowing(sender, args) {
                if (column === null) return;
                var menu = sender;
                var items = menu.get_items();
                if (column.get_dataType() === "System.String") {
                    var i = 0;
                    while (i < items.get_count()) {
                        if (!(items.getItem(i).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            item = items.getItem(i);
                            if (item !== null)
                                item.set_visible(false);
                        }
                        else {
                            item = items.getItem(i);
                            if (item !== null)
                                item.set_visible(true);
                        } i++;
                    }
                }
                if (column.get_dataType() === "System.Int32") {
                    var j = 0;
                    while (j < items.get_count()) {
                        if (!(items.getItem(j).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            item = items.getItem(j);
                            if (item !== null)
                                item.set_visible(false);
                        }
                        else {
                            item = items.getItem(j);
                            if (item !== null)
                                item.set_visible(true);
                        } j++;
                    }
                }
                if (column.get_dataType() === "System.DateTime") {
                    var h = 0;
                    while (h < items.get_count()) {
                        if (!(items.getItem(h).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            item = items.getItem(h);
                            if (item !== null)
                                item.set_visible(false);
                        }
                        else {
                            item = items.getItem(h);
                            if (item !== null)
                                item.set_visible(true);
                        } h++;
                    }
                }
                if (column.get_dataType() === "System.Boolean") {
                    var k = 0;
                    while (k < items.get_count()) {
                        if (!(items.getItem(k).get_value() in { 'NoFilter': '', 'EqualTo': '' })) {
                            item = items.getItem(k);
                            if (item !== null)
                                item.set_visible(false);
                        }
                        else {
                            item = items.getItem(k);
                            if (item !== null)
                                item.set_visible(true);
                        } k++;
                    }
                }

                column = null;
                menu.repaint();
            }

            function filterMenuShowing(sender, eventArgs) {
                column = eventArgs.get_column();
            }
        </script>
    </telerik:RadCodeBlock>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:UpdateProgress id="UpdateProgress1" runat="server" DynamicLayout="true" DisplayAfter="0" >
        <ProgressTemplate>
            <div class="page-overlay" style="display: block">
                <div class="overlay-spinner"></div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <telerik:RadScriptManager ID="rsManager" runat="server" AsyncPostBackTimeout="1800" />
    <telerik:RadStyleSheetManager ID="rssManager" runat="server"></telerik:RadStyleSheetManager>
    <telerik:RadWindowManager ID="rwManager" runat="server"></telerik:RadWindowManager>
    <%--RADAJAXLOADINGPANEL ANIMATION (Enclosed in a HTML Table to Center on MultiPage)--%>
    <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server" SkinID="Default" EnableSkinTransparency="true"
        Transparency="20" BackColor="#FFFFFF" ZIndex="100" BackgroundPosition="Center">
        <table style="width: 100%; height: 100%;">
            <tr style="height: 100%">
                <td style="width: 100%; vertical-align: central; text-align: center;">
                    <asp:Label ID="lblLoading" runat="server" ForeColor="Black" Text="Loading..." Font-Bold="true" Font-Size="Medium" />
                    <br />
                    <br />
                    <asp:Image ID="imgLoading" runat="server" Width="128px" Height="12px" ImageUrl="../../images/orangebarloader.gif" />
                </td>
            </tr>
        </table>
    </telerik:RadAjaxLoadingPanel>
    <%--END--%>

    <%--RAD AJAX MANAGER - Handles RAD CONTROLS/LOADING PANELS for Ajax Updating--%>
    <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server">
        <AjaxSettings>
            <telerik:AjaxSetting AjaxControlID="RadTabStrip1">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="RadTabStrip1" />
                    <telerik:AjaxUpdatedControl ControlID="RadMultiPage1" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--NOTES RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgNotes">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgNotes" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="btnAddNote">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="RadMultiPage1" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="btnAddNote">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="RadTabStrip1" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>

            <%--AssociatedAccount RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgAssociatedAccount">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgAssociatedAccount" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="btnAddAssociatedAccount">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="RadMultiPage1" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="btnAddAssociatedAccount">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="RadTabStrip1" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>


            <%--CS REQUESTS RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgCSRequests">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgCSRequests" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
            <%--ORDERS RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgOrders">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgOrders" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
            <%--INVOICES RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgInvoices">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgInvoices" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
            <%--LOCATIONS RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgLocations">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgLocations" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
            <%--USERS RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgUsers">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgUsers" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
            <%--BADGES RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgBadges">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgBadges" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
            <%--RETURNS RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgReturns">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgReturns" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
            <%--READS RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgReads">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgReads" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
            <%--DOCUMENTS RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgDocuments">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgDocuments" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
            <%--CALENDARS RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgCalendars">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rgCalendars" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
        </AjaxSettings>
    </telerik:RadAjaxManager>
    <%--END--%>

    <div id="mass-update-dialog" style="display: none;">
        <div class="OForm">
            <div id="errUpdate" class="FormError Hidden">
                <p>
                    <span class="MessageIcon"></span><strong>Messages:</strong>
                    <span id="errUpdateText"></span>
                </p>
            </div>
            <div class="Row">
                <div class="Label">Action: </div>
                <div class="Control">
                    <select id="ddlAction">
                        <option value="">Select an Action</option>
                        <option value="InstadoseAccess">Send welcome to instadose email</option>
                        <option value="Instadose">Send 'NO ACCESS' welcome to instadose email</option>
                        <!--<option value="Reset">Reset Password</option>-->
                    </select>
                </div>
                <div class="Clear"></div>
            </div>

        </div>
    </div>

    <%--CREATE [NEW] NOTE MODAL/DIALOG--%>
    <div id="createNoteDialog" style="display: none;">
        <asp:UpdatePanel ID="updtpnlCreateNote" runat="server">
            <ContentTemplate>
                <div class="FormError" id="divModalError_Notes" runat="server" visible="false">
                    <p>
                        <span class="MessageIcon"></span>
                        <strong>Messages:</strong><span id="spnModalError_Notes" runat="server">An error was encountered.</span>
                    </p>
                </div>
                <div class="OForm">
                    <div class="Row">
                        <div><strong>Note</strong><span class="Required">*</span>:</div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtNote" TextMode="MultiLine" CssClass="Size Large" MaxLength="1450" Height="75px" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Footer">
                        <span class="Required">*</span> Indicates a required field.
                        <div id="noteValidation" class="Required"></div>
                    </div>
                </div>
                <asp:Button ID="btnAddNote" runat="server" Style="display: none;" OnClick="btnAddNote_Click" Text="Create Note" OnClientClick="if (!validateNote()) return false;" />
                <asp:Button ID="btnCancelNote" runat="server" Style="display: none;" OnClick="btnCancelNote_Click" Text="Cancel Note" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--CREATE ASSOCIATED ACCOUNT MODAL/DIALOG--%>
    <div id="createAssociatedAccountDialog" style="display: none;">
        <asp:UpdatePanel ID="updtpnlAssociatedAccount" runat="server">
            <ContentTemplate>
                <div class="FormError" id="divModalError_AssociatedAccount" runat="server" visible="false">
                    <p>
                        <span class="MessageIcon"></span>
                        <strong>Messages:</strong><span id="spnModalError_AssociatedAccount" runat="server">An error was encountered.</span>
                    </p>
                </div>
                <div class="OForm OverrideLabel">
                    <div class="Row">
                        <div class="Label">Account ID<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtAccountID" CssClass="Size Small" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Account Name<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtAccountName" CssClass="Size Large2" />
                        </div>
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
					   
						<div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Control">
                            <asp:Button ID="btnSearchAssociatedAccount" runat="server" Style="display: none;" OnClick="btnSearchAssociatedAccount_Click" Text="Search Account" />
                            <div class="Clear"></div>
                        </div>
                    </div>

                    <div class="Footer">
                        <span class="Required">*</span> Indicates a required field.
                    </div>

                    <div class="Row">
                        <asp:GridView ID="gvSearchAccount" runat="server" CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
                            AutoGenerateColumns="false">
                            <Columns>
                                <asp:BoundField DataField="AccountID" HeaderText="Account ID" />
                                <asp:BoundField DataField="AccountName" HeaderText="Account Name" />
                                <asp:BoundField DataField="Status" HeaderText="Status" />
                                <asp:BoundField DataField="AssignNo" HeaderText="Assign No" />

                            </Columns>
                        </asp:GridView>
                        <div class="Clear"></div>
                    </div>

                </div>
                <asp:Button ID="btnAddAssociatedAccount" runat="server" Style="display: none;" OnClick="btnAddAssociatedAccount_Click" Text="Add Account" />
                <asp:Button ID="btnCancelAssociatedAccount" runat="server" Style="display: none;" OnClick="btnCancelAssociatedAccount_Click" Text="Cancel Add Account" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END CREATE ASSOCIATED ACCOUNT DIALOG--%>

    <%--UPLOAD/ADD [NEW] DOCUMENT MODAL/DIALOG--%>
    <div id="addDocumentDialog" style="display: none;">
        <asp:UpdatePanel ID="updtpnlAddDocument" runat="server">
            <Triggers>
                <asp:PostBackTrigger ControlID="btnAddDocument" />
            </Triggers>
            <ContentTemplate>
                <div class="FormError" id="divAddDocumentError" runat="server" visible="false">
                    <p>
                        <span class="MessageIcon"></span>
                        <strong>Messages:</strong><span id="spnAddDocumentErrorMessage" runat="server">An error was encountered.</span>
                    </p>
                </div>
                <div class="OForm">
                    <div class="Row">
                        <div class="Label">Document Type<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlDocumentTypes" runat="server" DataSourceID="sqlDocumentCategoriesDDL" DataTextField="DocumentCategory"
                                DataValueField="DocumentCategory" AppendDataBoundItems="true" TabIndex="1" AutoPostBack="true" OnSelectedIndexChanged="ddlDocumentTypes_SelectedIndexChanged">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalDDLDocumentTypes" runat="server" CssClass="InlineError" Display="Dynamic" InitialValue="0"
                                ErrorMessage="Required" ControlToValidate="ddlDocumentTypes" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div id="divOrderNumber" runat="server" visible="false">
                        <div class="Row">
                            <div class="Label">Order No.:</div>
                            <div class="Control">
                                <telerik:RadComboBox RenderMode="Lightweight" ID="rcbOrderNumbers" AllowCustomText="true" runat="server" Enabled="false"
                                    DataSourceID="SQLDSOrderNumbers" DataTextField="OrderID" EmptyMessage="Search of Order #..." EnableViewState="false"
                                    EnableAutomaticLoadOnDemand="True" ItemsPerRequest="10" ShowMoreResultsBox="true" EnableVirtualScrolling="true">
                                </telerik:RadComboBox>
                                <asp:RequiredFieldValidator ID="reqfldvalRCBOrderNumbers" runat="server" CssClass="InlineError" Display="Dynamic"
                                    ErrorMessage="Required" ControlToValidate="rcbOrderNumbers" ValidationGroup="UPLOAD" Enabled="false"></asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator ID="regexpvalRCBOrderNumbers" runat="server" ControlToValidate="rcbOrderNumbers" Display="Dynamic" CssClass="InlineError"
                                    ErrorMessage="Only numbers are allowed." ValidationExpression="\d+" ValidationGroup="UPLOAD" Enabled="false"></asp:RegularExpressionValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">Upload<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:FileUpload ID="flupldAddDocument" runat="server" TabIndex="3" />
                            <asp:RequiredFieldValidator ID="reqfldvalFlUpLdAddDocument" runat="server" CssClass="InlineError" Display="Dynamic"
                                ErrorMessage="Required" ControlToValidate="flupldAddDocument" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Description<span class="Required">*</span>:</div>
                        <div class="Control" style="vertical-align: top;">
                            <asp:TextBox ID="txtDocumentDescription" runat="server" Text="" TextMode="MultiLine" TabIndex="4" Width="234" Height="100" Wrap="true"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqfldvalTxtDocumentDescription" runat="server" CssClass="InlineError" Display="Dynamic"
                                ErrorMessage="Required" ControlToValidate="txtDocumentDescription" ValidationGroup="UPLOAD" InitialValue=""></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                </div>
                <asp:Button ID="btnAddDocument" runat="server" Style="display: none;" OnClick="btnAddDocument_Click" Text="Add Document" ValidationGroup="UPLOAD" TabIndex="5" />
                <asp:Button ID="btnCancelDocument" runat="server" Style="display: none;" Text="Cancel" TabIndex="6" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--E-MAIL ORDER ACKNOWLEDGEMENT MODAL/DIALOG--%>
    <div id="emailOrderAcknowledgementDialog" style="display: none;">
        <asp:UpdatePanel ID="upnlEmailOrderAcknowledgement" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="FormError" id="orderAckDialogErrors" runat="server" visible="false">
                    <p>
                        <span class="MessageIcon"></span>
                        <strong>Messages:</strong><span id="orderAckDialogErrorMsg" runat="server">An error was encountered.</span>
                    </p>
                </div>
                <div class="FormMessage" id="orderAckDialogMsgs" runat="server" visible="false">
                    <p>
                        <span class="MessageIcon"></span>
                        <strong>Messages:</strong>&nbsp;<span id="orderAckDialogMsg" runat="server">Ready to search.</span>
                    </p>
                </div>
                <div class="OForm">
                    <div class="Row">
                        Email the selected order acknowledgement in PDF format to customer.                                               
                    </div>
                    <div class="Row">
                        <div class="Label Small ">Account #:</div>
                        <div class="LabelValue">
                            <asp:Label runat="server" ID="lblAccountID" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label Small">Order #:</div>
                        <div class="Control">
                            <asp:DropDownList runat="server" ID="ddlEmailOrder" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlEmailOrder_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label Small">Email<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="Size Large2 " Text=""></asp:TextBox>
                        </div>
                        <div class="Clear"></div>
                    </div>
                </div>
                <asp:Button Text="Execute Email" Style="display: none;" ID="btnExecuteSendEmail" OnClick="btnExecuteSendEmail_Click" runat="server" />
                <asp:Button Text="Cancel Email" Style="display: none;" ID="btnCancelSendEmail" OnClick="btnCancelSendEmail_Click" runat="server" />
                <asp:Button Text="Load" Style="display: none;" ID="btnLoadEmailOrderAck" OnClick="btnLoadEmailOrderAck_Click" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div id="emailOrderAcknowledgementSuccessDialog" style="display: none;">
        <asp:UpdatePanel ID="upnlEmailOrderAcknowledgementSuccess" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="FormError" id="orderAckDialogSuccesses" runat="server" visible="false">
                    <p>
                        <span class="MessageIcon"></span>
                        <strong>Messages:</strong>&nbsp;<span id="orderAckDialogSuccessMsg" runat="server">An error was encountered.</span>
                    </p>
                </div>
                <div class="OForm">
                    <div id="Div4" runat="server" visible="false">
                        <p>
                            <span id="Span2" runat="server"></span>
                        </p>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            <asp:Label runat="server" ID="lblSentEmail" Visible="true" Width="350px" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                </div>
                <asp:Button Text="Close Window" Style="display: none;" ID="btnCloseWindow" OnClick="btnCloseWindow_Click" runat="server" />
                <asp:Button Text="Load" Style="display: none;" ID="btnLoadEmailOrderAckSuccess" OnClick="btnLoadEmailOrderAck_Click" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--INVOICE DETAILS MODAL/DIALOG--%>
    <div id="invoiceDetailsDialog" style="display: none;">
        <asp:UpdatePanel ID="updtpnlInvoiceDetailsDialog" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldInvoiceNo" runat="server" Value="0" />
                <div style="width: 100%;">
                    <asp:GridView ID="gvViewInvoiceDetails" runat="server" CssClass="OTable"
                        AlternatingRowStyle-CssClass="Alt" AutoGenerateColumns="False" DataKeyNames="InvoiceNo"
                        AllowPaging="True" PageSize="10" Width="100%" AllowSorting="True"
                        OnSorting="gvViewInvoiceDetails_Sorting"
                        OnPageIndexChanging="gvViewInvoiceDetails_PageIndexChanging">

                        <AlternatingRowStyle CssClass="Alt" />
                        <Columns>
                            <asp:BoundField DataField="InvoiceNo" HeaderText="Invoice No." ReadOnly="true" Visible="false" />
                            <asp:BoundField DataField="TransactionDate" HeaderText="Transaction Date" ReadOnly="true" DataFormatString="{0:d}" />
                            <asp:BoundField DataField="TransactionType" HeaderText="Type" ReadOnly="true" />
                            <asp:BoundField DataField="CheckNo" HeaderText="Check #" ReadOnly="true" />
                            <asp:BoundField DataField="AmountWithCurrency" HeaderText="Amount" ReadOnly="true">
                                <HeaderStyle CssClass="RightAlign" />
                                <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div>There are no records found!</div>
                        </EmptyDataTemplate>
                        <PagerSettings PageButtonCount="10" />
                    </asp:GridView>
                </div>
                <asp:Button ID="btnOK" runat="server" Text="OK" Style="display: none;" OnClick="btnOK_Click" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <div id="outside-collector-dialog" style="display: none;">
        <div class="OForm">
            <div class="Row">
                <span class="message"></span>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Collector:</div>
                <div class="Control">
                    <select id="ddlOutsideCollector"></select>
                </div>
                <div class="Clear"></div>
            </div>
        </div>
    </div>

    <%--ASSIGN WEARER MODAL/DIALOG--%>
    <div id="assignedWearerDialog">
        <asp:UpdatePanel ID="upnlAssignedWearer" runat="server">
            <ContentTemplate>
                <div class="FormError" id="assignedWearerDialogError" runat="server" visible="false">
                    <p>
                        <span class="MessageIcon"></span>
                        <strong>Messages:</strong> <span id="assignedWearerDialogErrorMsg" runat="server">An error was encountered.</span>
                    </p>
                </div>

                <div class="OForm">

                    <div class="Row" runat="server" visible="false">
                        <div class="Label Small">Replaced SerialNo:</div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblSerialNo" runat="server"></asp:Label>
                            </div>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Search By:</div>
                        <div class="Control">
                            <asp:RadioButtonList ID="rbtnSearchBy" runat="server" RepeatDirection="Horizontal"
                                AutoPostBack="true" OnSelectedIndexChanged="rbtnSearchBy_SelectedIndexChanged">
                                <asp:ListItem Value="all" Enabled="true" Selected="True">All</asp:ListItem>
                                <asp:ListItem Value="lastName" Enabled="true">Last Name</asp:ListItem>
                                <asp:ListItem Value="assigned" Enabled="true">Assigned</asp:ListItem>
                                <asp:ListItem Value="un-assigned" Enabled="true">Un-assigned</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row" id="searchFilter" runat="server" visible="false">
                        <div class="Label Small">Filter:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtOrderAssignSearch" runat="server" CssClass="Size Small"></asp:TextBox>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small"></div>
                        <div class="Control">
                            <asp:Button ID="btnWearerSearch" runat="server" CssClass="OButton" Text="Search" OnClick="btnWearerSearch_Click"></asp:Button>
                        </div>
                        <div class="Clear"></div>
                    </div>

                </div>

                <div style="overflow-x: no-display; overflow-y: scroll; height: 350px;">
                    <asp:GridView ID="gv_WearerList" runat="server" AutoGenerateColumns="False" CssClass="OTable Scroll" DataKeyNames="UserID">
                        <Columns>

                            <asp:TemplateField HeaderStyle-Width="25px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:RadioButton ID="radSelectUser" runat="server" onclick="RadioSelectUserCheck(this);" />
                                    <asp:HiddenField ID="hidUserID" runat="server" Value='<%# Eval("UserID")%>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="UserID" HeaderText="Wearer #" SortExpression="UserID" ItemStyle-Width="70px" />
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />

                        </Columns>
                        <EmptyDataTemplate>
                            <div class="NoData">
                                No wearer found.
                            </div>
                        </EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
                        <PagerStyle CssClass="Footer" />
                    </asp:GridView>
                </div>

                <asp:Button Text="Assign" Style="display: none;" ID="btnAssignedWearer"
                    OnClick="btnAssignedWearer_Click" runat="server" />

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--ASSIGN WEARER MODAL/DIALOG--%>

    <%--ID+ LOST REPLACEMENT ORDER MODAL/DIALOG--%>
    <div id="IDPlusLostReplacementRecallOrderDialog" style="display: none;">
        <asp:UpdatePanel ID="upnlIDPlusLostReplacementRecallOrder" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="HidRMAType" runat="server" Value="" ClientIDMode="Static" />

                <div class="FormError" id="IDPlusLostReplacementRecallOrderDialogError" runat="server" visible="false">
                    <p>
                        <span class="MessageIcon"></span>
                        <strong>Messages:</strong> <span id="IDPlusLostReplacementRecallOrderDialogErrorMsg" runat="server">An error was encountered.</span>
                    </p>
                </div>

                <div id="accordion" style="margin-top: 10px; width: 99.9%;">
                    <div>
                        <h3><a href="#" id="RMADevices" runat="server">Lost/Replacement Devices</a></h3>
                        <div id="Div1" class="OForm AccordionPadding" runat="server">
                            <asp:UpdatePanel ID="upnlReplacementDevices" runat="server">
                                <ContentTemplate>
                                    <div class="Row">
                                        <div class="Label Small">Location:</div>
                                        <div class="Control">
                                            <asp:DropDownList ID="ddlLocation" runat="server" DataTextField="LocationName" DataValueField="LocationID" AutoPostBack="true" OnSelectedIndexChanged="ddlLocation_OnSelectedIndexChange" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>
                                    <div class="Row" id="Div2" runat="server">
                                        <div class="Label Small">Product Group:</div>
                                        <div class="Control">
                                            <asp:RadioButtonList ID="rblProductGroup" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged ="rblProductGroup_OnSelectedIndexChange">
                                                <asp:ListItem Text="ID+" Value="10" Selected="True" />
                                                <asp:ListItem Text="ID2" Value="11" />
                                            </asp:RadioButtonList>
                                        </div>
                                        <div class="Clear"></div>
                                    </div>
                                    <div class="Row" id="lostRMAType" runat="server">
                                        <div class="Label Small">Reason:</div>
                                        <div class="Control">
                                            <asp:RadioButtonList ID="rblLostRMAReason" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged ="rblLostRMAReason_OnSelectedIndexChange">
                                                <asp:ListItem Text="Lost" Value="L" Selected="True" />
                                                <asp:ListItem Text="Broken" Value="B" />
                                            </asp:RadioButtonList>
                                        </div>
                                        <div class="Clear"></div>
                                    </div>
                                    <div class="Row" id="recallReason" runat="server">
                                        <div class="Label Small">Recall Reason:</div>
                                        <div class="Control">
                                            <asp:DropDownList ID="ddlRecallReason" runat="server" Visible="true" DataTextField="Description" DataValueField="ReasonID" />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>
                                    <div class="Row" id="recallNote" runat="server">
                                        <div class="Label Small">Recall Note<span class="Required">*</span>:</div>
                                        <div class="Control">
                                            <asp:TextBox ID="txtRecallNote" TextMode="MultiLine" CssClass="Size XLarge2" runat="server" Visible="true"></asp:TextBox>
                                        </div>
                                        <div class="Clear"></div>
                                    </div>

                                    <div class="Row" id="recallProRatePeriod" runat="server">
                                        <div class="Label Small">ProRate Period:</div>
                                        <div class="Control">
                                            <asp:RadioButtonList ID="radProratePeriod" runat="server" RepeatDirection="Horizontal" RepeatColumns="2">                                
                                            </asp:RadioButtonList>
                                        </div>
                                        <div class="Clear"></div>
                                    </div>

                                    <div class="OToolbar JoinTable" id="DeviceListToolBar" runat="server">
                                        <ul>
                                            <li>
                                                <asp:Label ID="lblSearch" runat="server" Text="Search:"></asp:Label>
                                                <asp:TextBox ID="txtDeviceSearch" runat="server" Width="150px" OnTextChanged="txtDeviceSearch_TextChanged" AutoPostBack="true"></asp:TextBox>
                                            </li>
                                        </ul>
                                    </div>

                                    <div id="scrollTopGridView" class="gridView">
                                        <asp:GridView ID="gv_DeviceList" runat="server" AutoGenerateColumns="False" CssClass="OTable" BorderWidth="0px" Style="margin: -1px 0" DataKeyNames="DeviceID"
                                            OnRowDataBound="gv_DeviceList_RowDataBound"
                                            HeaderStyle-CssClass="FixedHeader">

                                            <Columns>

                                                <asp:TemplateField HeaderStyle-Width="50px" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Center">
                                                    <HeaderTemplate>
                                                        <img src="/images/icons/package.png" style="border: 0px none; width: 16px; height: 16px;" />
                                                        <asp:CheckBox runat="server" ID="chkbxHeaderDeviceList" ToolTip="Select all lost devices" class="chkbxHeaderDeviceList" />
                                                    </HeaderTemplate>

                                                    <ItemTemplate>
                                                        <asp:CheckBox ID="chkbxSelectDeviceList" runat="server" class="chkbxItemDevice" onclick="chkbxSelectDeviceList_Click(this)" />
                                                        <asp:HiddenField runat="server" ID="HidDeviceID" Value='<%# Eval("DeviceID") %>' />
                                                        <asp:HiddenField runat="server" ID="HidAssignedUserID" Value='<%# Eval("UserID") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderStyle-Width="50px" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Center">
                                                    <HeaderTemplate>
                                                        <img src="/images/icons/arrow_refresh.png" style="border: 0px none; width: 16px; height: 16px;" />
                                                        <asp:CheckBox runat="server" ID="chkbxHeaderReplaceDeviceList" ToolTip="Select all replaced devices" class="chkbxHeaderReplaceDeviceList" />
                                                    </HeaderTemplate>

                                                    <ItemTemplate>
                                                        <asp:CheckBox ID="chkbxSelectReplaceDeviceList" runat="server" class="chkbxItemReplaceDevice" onclick="chkbxSelectReplaceDeviceList_Click(this)" />
                                                        <asp:HiddenField runat="server" ID="HidReplaceDeviceID" Value='<%# Eval("DeviceID") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:BoundField DataField="Color" HeaderText="Color" SortExpression="Color" HeaderStyle-Width="70px" ItemStyle-Width="70px" />

                                                <asp:TemplateField HeaderText="BodyRegion" HeaderStyle-Width="100px" ItemStyle-Width="100px">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblBodyRegionID" runat="server" Text='<%# Eval("BodyRegionID") %>' Visible="false" />
                                                        <asp:DropDownList ID="ddlBodyRegion" runat="server" DataTextField="BodyRegionName" DataValueField="BodyRegionID">
                                                        </asp:DropDownList>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:BoundField DataField="SerialNo" HeaderText="Serial #" SortExpression="SerialNo" HeaderStyle-Width="70px" ItemStyle-Width="70px" />

                                                <asp:TemplateField HeaderText="Assigned Wearer" HeaderStyle-Width="195px" ItemStyle-Width="195px">
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgAssignedWearerForDevice" runat="server" ImageUrl="~/images/icons/user_edit.png" Width="16px" Height="16px"
                                                            Style="position: relative; top: 4px"
                                                            AlternateText="" ToolTip="Edit wearer for replacement device"
                                                            OnClick="imgAssignedWearerForDevice_Click"
                                                            CommandName="AssignedWearerForDevice" CommandArgument='<%# Eval("SerialNo") %>' />

                                                        <asp:Label ID="lblFullName" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"FullName" )%>' />
                                                    </ItemTemplate>
                                                    <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Left" />
                                                    <ItemStyle CssClass="CenterAlignItemText" Wrap="False" HorizontalAlign="Left" />
                                                </asp:TemplateField>

                                            </Columns>
                                            <EmptyDataTemplate>
                                                <div class="NoData">
                                                    No device found.
                                                </div>
                                            </EmptyDataTemplate>
                                            <AlternatingRowStyle CssClass="Alt" />
                                            <PagerStyle CssClass="Footer" />
                                        </asp:GridView>
                                    </div>

                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>

                    <div id="PaymentSection" runat="server">
                        <h3><a href="#">Payment Method</a></h3>
                        <div class="OForm AccordionPadding">
                            <asp:UpdatePanel ID="upnlPaymentMethod" runat="server">
                                <ContentTemplate>
                                    <%--<div class="Row">
                                        <div class="Label">
                                            Billing Method:
                                        </div>
                                        <div class="Control">
                                            <asp:RadioButtonList ID="rblstPayMethod" runat="server" RepeatDirection="Horizontal"
                                                AutoPostBack="True" OnSelectedIndexChanged="rblstPayMethod_SelectedIndexChanged">
                                                <asp:ListItem Value="Credit Card" Enabled="true">Credit Card</asp:ListItem>
                                                <asp:ListItem Value="Purchase Order" Enabled="true">Purchase Order</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </div>
                                        <div class="Clear">
                                        </div>
                                    </div>--%>
                                    <%--<div class="Row" runat="server" id="divPaymentPO" visible="false">--%>
                                    <div class="Row" runat="server" id="divPaymentPO">
                                        <div class="Label">
                                            PO Number<span class="Required">*</span>:
                                        </div>
                                        <div class="Control">
                                            <asp:TextBox ID="txtPOno" runat="server" Text="" MaxLength="15"></asp:TextBox>
                                        </div>
                                        <div class="Clear">
                                        </div>
                                    </div>


                                    <%--<div runat="server" id="divPaymentCC" visible="false">
                                        <div class="Row">
                                            <div class="Label">
                                                Card Type<span class="Required">*</span>:
                                            </div>
                                            <div class="Control">
                                                <asp:RadioButtonList ID="rbtCardType" runat="server" RepeatColumns="4">
                                                    <asp:ListItem Value="1" Text="Visa" Selected="True"><img src='../images/ccvisa.gif' alt='Visa Card' width='30'></asp:ListItem>
                                                    <asp:ListItem Value="2" Text="MasterCard"><img src='../images/ccmastercard.gif' alt='MasterCard' width='30'></asp:ListItem>
                                                    <asp:ListItem Value="3" Text="Discover"><img src='../images/ccdiscover.gif' alt='Discover' width='30'></asp:ListItem>
                                                    <asp:ListItem Value="4" Text="Amex"><img src='../images/ccamex.gif' alt='American Express' width='30'></asp:ListItem>
                                                </asp:RadioButtonList>
                                            </div>
                                            <div class="Clear"></div>
                                        </div>
                                        <div class="Row">
                                            <div class="Label">
                                                Credit Card Number<span class="Required">*</span>:
                                            </div>
                                            <div class="Control">
                                                <asp:TextBox ID="txtCCno" runat="server" CssClass="Size Medium2"></asp:TextBox>
                                            </div>
                                            <div class="Clear">
                                            </div>
                                        </div>
                                        <div class="Row">
                                            <div class="Label">
                                                Name On Card<span class="Required">*</span>:
                                            </div>
                                            <div class="Control">
                                                <asp:TextBox ID="txtCCName" runat="server" CssClass="Size Medium"></asp:TextBox>
                                            </div>
                                            <div class="Clear">
                                            </div>
                                        </div>
                                        <div class="Row">
                                            <div class="Label">
                                                Expiration Month/Year<span class="Required">*</span>:
                                            </div>
                                            <div class="Control">
                                                <asp:DropDownList ID="ddlCCMonth" runat="server">
                                                    <asp:ListItem Value="0" Text="Month" />
                                                    <asp:ListItem Value="1" Text="Jan" />
                                                    <asp:ListItem Value="2" Text="Feb" />
                                                    <asp:ListItem Value="3" Text="Mar" />
                                                    <asp:ListItem Value="4" Text="Apr" />
                                                    <asp:ListItem Value="5" Text="May" />
                                                    <asp:ListItem Value="6" Text="Jun" />
                                                    <asp:ListItem Value="7" Text="Jul" />
                                                    <asp:ListItem Value="8" Text="Aug" />
                                                    <asp:ListItem Value="9" Text="Sep" />
                                                    <asp:ListItem Value="10" Text="Oct" />
                                                    <asp:ListItem Value="11" Text="Nov" />
                                                    <asp:ListItem Value="12" Text="Dec" />
                                                </asp:DropDownList>

                                                <asp:DropDownList ID="ddlCCYear" runat="server">
                                                </asp:DropDownList>
                                            </div>
                                            <div class="Clear">
                                            </div>
                                        </div>
                                        <div class="Row">
                                            <div class="Label">
                                                Security Code (CVC)<span class="Required">*</span>:
                                            </div>
                                            <div class="Control">
                                                <asp:TextBox CssClass="Size XXSmall" ID="txtCCcvc" runat="server"></asp:TextBox>
                                            </div>
                                            <div class="Clear">
                                            </div>
                                        </div>
                                    </div>--%>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>

                    <div id="ShippingSection" runat="server">
                        <h3><a href="#">Shipping Options</a></h3>
                        <div class="OForm AccordionPadding" runat="server">
                            <asp:UpdatePanel ID="upnlShippingOptions" runat="server">
                                <ContentTemplate>
                                    <div class="Row">
                                        <div class="Label">
                                            Package Type:
                                        </div>
                                        <div class="Control">
                                            <asp:DropDownList ID="ddlPackageType" runat="server"
                                                DataValueField="PackageTypeID"
                                                DataTextField="PackageDesc">
                                            </asp:DropDownList>
                                        </div>
                                        <div class="Clear">
                                        </div>
                                    </div>

                                    <div class="Row">
                                        <div class="Label">
                                            Shipping Option:
                                        </div>
                                        <div class="Control">
                                            <asp:DropDownList ID="ddlShippingOption" runat="server"
                                                DataValueField="ShippingOptionID"
                                                DataTextField="ShippingOptionDesc">
                                            </asp:DropDownList>
                                        </div>
                                        <div class="Clear">
                                        </div>
                                    </div>

                                    <div class="Row" id="divFedex" runat="server">
                                        <div class="Label">
                                            Shipping Method<span class="Required">*</span>:
                                        </div>
                                        <div class="Control">
                                            <asp:DropDownList ID="ddlShippingMethod" runat="server" DataValueField="ShippingMethodID" DataTextField="ShippingMethodDesc" />
                                        </div>
                                        <div class="Clear">
                                        </div>
                                    </div>

                                    <div class="Row" id="divMalvernCarrier" runat="server">
                                        <div class="Label">
                                            Shipping Carrier:
                                        </div>
                                        <div class="Control">
                                            <asp:DropDownList ID="ddShippingCarrier" runat="server" DataValueField="ShippingCarrier" DataTextField="ShippingCarrier" />
                                        </div>
                                        <div class="Clear">
                                        </div>
                                    </div>

                                    <div class="Row" id="divMalvernShipMethod" runat="server">
                                        <div class="Label">
                                            Shipping Method<span class="Required">*</span>:
                                        </div>
                                        <div class="Control">
                                            <asp:DropDownList ID="ddlShippingMethodMalvern" runat="server" DataValueField="ShippingMethodID" DataTextField="ShippingMethodDesc" />
                                        </div>
                                        <div class="Clear">
                                        </div>
                                    </div>

                                    <div class="Row">
                                        <div class="Label">
                                            Special Instructions:
                                        </div>
                                        <div class="Control">
                                            <asp:TextBox ID="txtSpecialInstruction"
                                                TextMode="MultiLine" Height="50" MaxLength="200"
                                                runat="server"></asp:TextBox>
                                        </div>
                                        <div class="Clear">
                                        </div>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>

                <asp:Button Text="Create Order" Style="display: none;" ID="btnCreateLostReplacementRecallOrder"
                    OnClick="btnCreateLostReplacementRecallOrder_Click" runat="server" />

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--CALENDAR ADD/EDIT MODAL/DIALOG--%>
    <div id="calendar-dialog" style="display: none;">
        <div class="loading"></div>
        <div class="FormError" style="display: none;">
            <p>
                <span class="MessageIcon"></span><strong>Messages:</strong>
                <span class="Message"></span>
            </p>
        </div>
        <div class="OForm">
            <div class="Row">
                <div class="Label">Name<span class="Required">*</span>:</div>
                <div class="Control">
                    <input type="text" id="calendar-name" name="calendar-name" style="width: 240px;" />
                </div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Description:</div>
                <div class="Control">
                    <textarea id="calendar-desc" name="calendar-desc" style="width: 240px; height: 40px;"></textarea>
                </div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">The read should occur:</div>
                <div class="Control">
                    <select id="calendar-freq" name="calendar-freq">
                        <option value="M">Monthly</option>
                        <option value="W">Weekly</option>
                        <option value="B">Every 2 weeks</option>
                        <option value="T">Every 2 Months</option>
                        <option value="Q">Quarterly</option>
                    </select>
                </div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">On the:</div>
                <div class="Control">
                    <select id="calendar-day" name="calendar-day">
                        <option value="31">Last</option>
                        <option value="1">1st</option>
                        <option value="5">5th</option>
                        <option value="10">10th</option>
                        <option value="15">15th</option>
                        <option value="20">20th</option>
                        <option value="25">25th</option>
                    </select>
                    <span id="calendar-day-statement">day of the month</span>
                </div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">At:</div>
                <div class="Control">
                    <input type="text" class="" maxlength="2" value="11" style="width: 20px;" id="calendar-time-hour" />:<input type="text" class="" maxlength="2" value="59" style="width: 20px;" id="calendar-time-min" />
                    <select id="calendar-time-ampm">
                        <option value="AM">AM</option>
                        <option value="PM" selected="selected">PM</option>
                    </select>
                </div>
                <div class="Clear"></div>
            </div>
        </div>
    </div>

    <div id="calendar-dates-dialog" style="display: none;">
        <div class="loading"></div>
        <div class="FormError" style="display: none;">
            <p>
                <span class="MessageIcon"></span><strong>Messages:</strong>
                <span class="Message"></span>
            </p>
            <div class="Clear"></div>
        </div>

        <div id="gridCalendarDetails" style="min-height: 250px; position: relative; width: 290px; float: left;">
            <h3>Schedule Dates</h3>
            <div class="OToolbar JoinTable" style="padding: 5px 10px 12px 10px;">
                <input type="text" style="position: absolute; top: -1000px;" />
                <input type="text" class="has-date-picker" id="calendar-date" style="width: 80px;" />

                <input type="text" class="" maxlength="2" value="11" style="width: 20px;" id="calendar-hour" />:<input type="text" class="" maxlength="2" value="59" style="width: 20px;" id="calendar-min" />
                <select id="calendar-ampm">
                    <option value="AM">AM</option>
                    <option value="PM" selected="selected">PM</option>
                </select>
                <input type="button" id="calendar-add" class="btn btn-small" value="Add" />
            </div>
            <table class="display" id="tblCalendarDetails">
                <thead>
                    <tr class="header">
                        <th>Schedule Date</th>
                    </tr>
                </thead>
                <tfoot>
                    <tr>
                        <td><span id="count-schedules">0</span> / 60</td>
                    </tr>
                </tfoot>
            </table>
        </div>
        <div id="gridCalendarLocations" style="min-height: 250px; position: relative; width: 450px; float: left; padding-left: 20px;">
            <h3>Locations</h3>
            <table class="display" id="tblCalendarLocations">
                <thead>
                    <tr class="header">
                        <th>
                            <input type="checkbox" id="chkAllLocations" /></th>
                        <th>Location</th>
                        <th>Name</th>
                        <th title="Already has a calendar assigned">Calendar?</th>
                    </tr>
                </thead>
                <tbody></tbody>
                <tfoot>
                    <tr>
                        <td colspan="4">Assigned: <span id="count-locations-assigned">0</span> of <span id="count-locations-total">0</span></td>
                    </tr>
                </tfoot>
            </table>
        </div>
    </div>

    <div id="overrideARAccount" class="Hidden" style="display: none;">
        <div class="loading"></div>
        <div class="OForm">
            <div class="FormError Hidden">
                <p>
                    <span class="MessageIcon"></span><strong>Messages:</strong>
                    <span class="Message"></span>
                </p>
                <div class="Clear"></div>
            </div>
            <div class="Row" id="overridenote">
                <div class="Label">Override Note:</div>
                <div class="Clear">
                </div>
            </div>
            <div class="Row" id="overridetext">
                <textarea id="txtOverrideARAccountNote" maxlength="630" style="width: 100%; height: 150px;"></textarea>
            </div>
        </div>
    </div>

    <%--END--%>


    <%--AR Aging Invoice Modal--%>
    <div id="ar-dialog" style="display: none;">
        <bc:ARDialog runat="server" ID="ARDialog" />
    </div>

    <%--AX Invoice Orders Modal--%>
    <div id="ax-invoice-orders-dialog" style="display: none;">
        <div class="loading"></div>
        <div class="FormError" style="display: none;">
            <p>
                <span class="MessageIcon"></span><strong>Messages:</strong>
                <span class="Message"></span>
            </p>
        </div>
        <table id="tbl-ax-invoice-orders" style="width: 100%;">
            <thead style="text-align: center;">
                <tr>
                    <th>Order #</th>
                </tr>
            </thead>
            <tbody style="text-align: center;">
                <tr>
                    <td>abcedfg</td>
                </tr>
            </tbody>
        </table>
    </div>


    <%------------------------------- BEGIN :: GENERAL INFORMATION FORMVIEW -------------------------------%>
    <asp:FormView ID="fvAccountGeneralInformation" runat="server" DataKeyNames="AccountID" DataSourceID="sqlAccountGeneralInformation" Width="100%">
        <ItemTemplate>
            <table class="OTable" style="border: 0px none; margin: 0px auto 10px auto; width: auto;">
                <tr>
                    <td class="Label" style="width: 120px; vertical-align: bottom;">Company:</td>
                    <td style="width: 180px; vertical-align: bottom;">
                        <asp:Label ID="lblCompanyName" runat="server" Text='<%# Bind("CompanyName") %>' />
                    </td>
                    <td class="Label" style="width: 175px; vertical-align: bottom;">Account #:</td>
                    <td style="width: 180px; vertical-align: bottom;">
                        <asp:Label ID="lblAccountID" Font-Bold="true" runat="server" Text='<%# Bind("AccountID") %>' />
                        <asp:HyperLink ID="hyprlnkEditAccountInformation" runat="server" ToolTip="Edit Account Information" NavigateUrl='<%# Eval("AccountID", "~/CustomerService/CreateAccount.aspx?ID={0}") %>'>
							<img src="/images/icons/pencil.png" style="border: 0px none;width: 16px; height: 16px;" />
                        </asp:HyperLink>
                    </td>
                    <td rowspan="8" style="padding: 10px; width: 300px; vertical-align: middle; text-align: center;">
                        <asp:Image ID="imgBrandLogo" Style="width: 256px; height: 80px;" ImageUrl="~/images/logos/mirion.png" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td class="Label">Account Name:</td>
                    <td>
                        <asp:Label ID="lblAccountName" runat="server" Text='<%# Bind("AccountName") %>' />
                    </td>
                    <td class="Label">Active:</td>
                    <td>
                        <asp:Label ID="lblIsActive" runat="server" Text='<%# YesNo(Eval("Active")) %>' CssClass='<%# Eval("Active", "lblActive{0}") %>' />
                    </td>

                </tr>
                <tr>
                    <td class="Label">Contact Name:</td>
                    <td>
                        <asp:Label ID="lblFirstName" runat="server" Text='<%# Bind("ContactName") %>' />
                    </td>
                    <td class="Label">Account Type:</td>
                    <td>
                        <asp:Label ID="lblAccountType" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td class="Label">Customer Group:</td>
                    <td>
                        <asp:Label ID="lblCustomerGroup" runat="server" />
                    </td>
                    <td class="Label">Customer Type:</td>
                    <td>
                        <asp:Label ID="lblCustomerType" runat="server" Text='<%# Bind("CustomerType") %>' />
                        <i>
                            <asp:Label ID="lblCustomerCode" CssClass="customer-code" runat="server" Text='<%# Bind("CustomerCode") %>' />
                        </i>
                    </td>
                </tr>
                <tr>
                    <td class="Label">Referral Code:</td>
                    <td>
                        <asp:Label ID="lblReferralCode" runat="server" Text='<%# Bind("ReferralCode") %>' />
                    </td>
                    <%--Added on 09/25/2013 by Anuradha Nandi - Dealer Name information.--%>
                    <td class="Label">Market:</td>
                    <td>
                        <asp:Label ID="lblIndustryName" runat="server" Text='<%# Bind("IndustryName") %>' />
                    </td>
                </tr>
                <tr>
                    <td class="Label">Created Date:</td>
                    <td>
                        <asp:Label ID="lblCreatedOn" runat="server" Text='<%# Bind("CreatedOn","{0:d}") %>' />
                    </td>
                    <td class="Label">Dealer Name:<%--Unix Customer Type:--%></td>
                    <td>
                        <asp:Label ID="lblDealerName" runat="server" Text='<%# Bind("DealerName") %>' />
                        <%--<asp:Label ID="lblUnixCustomerType" runat="server" Text='<%# Bind("UnixCustomerType") %>' />
						<asp:Label ID="lblUnixCustomerCode" runat="server" Text='<%# Bind("UnixCustomerCode") %>' /></i>--%>
                    </td>
                </tr>
                <tr>
                    <td class="Label">Collector Type:</td>
                    <td>
                        <asp:Label ID="lblCollectorType" runat="server" Text='<%# Bind("CollectorType") %>' />
                    </td>
                    <td class="Label">Contract Period:</td>
                    <td>
                        <asp:Label ID="lblContractPeriod" runat="server" Text='<%# Bind("ContractStartDate","{0:d}") %>' />
                        -
						<asp:Label ID="lblCancelledDate" runat="server" Text='<%# Bind("ContractEndDate","{0:d}") %>' />
                    </td>
                </tr>
                <tr>
                    <td class="Label">PO Number:</td>
                    <td>
                        <asp:Label ID="lblRenewalPONumber" runat="server" Text='<%# Bind("RenewalPONumber") %>' />
                    </td>
                    <td class="Label">PO Period:</td>
                    <td>
                        <asp:Label ID="lblRenewalPOStartDate" runat="server" Text='<%# Bind("RenewalPOStartDate", "{0:d}") %>' /> - <asp:Label ID="lblRenewalPOEndDate" runat="server" Text='<%# Bind("RenewalPOEndDate", "{0:d}") %>' />
                    </td>
                </tr>
                <tr>
                    <td class="Label"><span class="collector">Collector:</span></td>
                    <td>
                        <script>Finance.collections.outsideCollector ='<%# Eval("OutsideCollectorID") %>'</script>
                        <span id="lblOutsideCollector" class="collector"></span>
                        <button type="button" class="btn btn-mini btn-danger collector" style="margin: 0px 5px;" id="btnOutsideCollector">Outside Collection</button></td>
                    <td class="Label">Location Billing:</td>
                    <td>
                        <asp:Label ID="lblLocationBilling" runat="server" Text='<%# Bind("UseLocationBilling") %>' />
                    </td>
                </tr>
                <tr>
                    <td class="Label"><asp:Literal ID="ltARIndicator" runat="server">AR Indicator:</asp:Literal></td>
                    <td>
                        <asp:HyperLink ID="hlARIndicator" runat="server" CssClass="btn ar-indicator ar-indicator-green" Text="Pay Now" Target="_blank" />
                    </td>
                    <td class="Label">Sales Rep Commission:</td>
                    <td>
                        <asp:Label ID="lblIsICCareCommEligible" runat="server" />
                    </td>
                </tr>
            </table>
            <table class="OTable" style="border: 0px none; margin: 20px auto 10px auto; width: auto;">
                <tr>
                    <td style="font-weight: bold; width: 300px;">Billing Term & Rate:</td>
                    <td style="font-weight: bold; width: 300px;">Billing Address:</td>
                    <td style="font-weight: bold; width: 300px;">Shipping Address:</td>
                </tr>
                <tr>
                    <td style="vertical-align: top;">
                        <div>
                            Billing Term:&nbsp;<asp:Label ID="lblBillingTerm" runat="server" Text='<%# Bind("BillingTerms") %>' />
                        </div>
                        <div>
                            Billing Day Prior:&nbsp;<asp:Label ID="lblBillingDaysPrior" runat="server" Text='<%# Bind("BillingDaysPrior") %>' />
                        </div>
                        <div>
                            Rate Code:&nbsp;<asp:Label ID="lblRateCode" runat="server" Text='<%# Bind("RateCode") %>' />
                        </div>
                    </td>
                    <td>
                        <div>
                            <asp:Label ID="lblBillingCompany" runat="server" Text='<%# Bind("BillingCompany") %>' />
                        </div>
                        <div>
                            <asp:Label ID="lblBillingAddress1" runat="server" Text='<%# Bind("BillingAddress1") %>' />
                        </div>
                        <div>
                            <asp:Label ID="lblBillingAddress2" runat="server" Text='<%# Bind("BillingAddress2") %>' />
                        </div>
                        <div>
                            <asp:Label ID="lblBillingCity" runat="server" Text='<%# Bind("BillingCity") %>' />
                            <asp:Label ID="lblBillingState" runat="server" Text='<%# Bind("BillingState") %>' />
                            <asp:Label ID="lblBillingPostalCode" runat="server" Text='<%# Bind("BillingPostalCode") %>' />
                            <br />
                            <asp:Label ID="lblBillingCountry" runat="server" Text='<%# Bind("BillingCountry") %>' />
                        </div>
                        <div>
                            <asp:Label ID="lblBillingTelephone" runat="server" Text='<%# Bind("BillingTelephone") %>' />
                            <br />
                            <asp:Label ID="lblBillingFax" runat="server" Text='<%# Bind("BillingFax") %>' />
                        </div>
                    </td>
                    <td>
                        <div>
                            <asp:Label ID="lblShippingCompany" runat="server" Text='<%# Bind("ShippingCompany") %>' />
                        </div>
                        <div>
                            <asp:Label ID="lblShippingAddress1" runat="server" Text='<%# Bind("ShippingAddress1") %>' />
                        </div>
                        <div>
                            <asp:Label ID="lblShippingAddress2" runat="server" Text='<%# Bind("ShippingAddress2") %>' />
                        </div>
                        <div>
                            <asp:Label ID="lblShippingCity" runat="server" Text='<%# Bind("ShippingCity") %>' />
                            <asp:Label ID="lblShippingState" runat="server" Text='<%# Bind("ShippingState") %>' />
                            <asp:Label ID="lblShippingPostalCode" runat="server" Text='<%# Bind("ShippingPostalCode") %>' />
                            <br />
                            <asp:Label ID="lblShippingCountry" runat="server" Text='<%# Bind("ShippingCountry") %>' />
                        </div>
                        <div>
                            <asp:Label ID="lblShippingTelephone" runat="server" Text='<%# Bind("ShippingTelephone") %>' />
                            <br />
                            <asp:Label ID="lblShippingFax" runat="server" Text='<%# Bind("ShippingFax") %>' />
                        </div>
                    </td>
                </tr>
            </table>
        </ItemTemplate>
        <EmptyDataTemplate>
            <div style="padding: 20px; font-weight: bold; font-size: 1.1em; text-align: center; color: Maroon;">
                Account #<%# Request.QueryString["ID"].ToString() %>was not found.
            </div>
        </EmptyDataTemplate>
    </asp:FormView>

    <asp:SqlDataSource ID="sqlAccountGeneralInformation" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_if_GetAccountGeneralInformationByAccountID" SelectCommandType="StoredProcedure">
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="pAccountID" QueryStringField="ID" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    <%------------------------------- END :: GENERAL INFORMATION FORMVIEW -------------------------------%>

    <div>&nbsp;</div>

    <%------------------------------- BEGIN :: START MAIN PAGE CONTENT OVER HERE -------------------------------%>
    <div id="divTabbedContent">
        <div id="radTabContainer" class="OTable" style="border-radius: 5px;">
            <telerik:RadTabStrip ID="RadTabStrip1" runat="server" MultiPageID="RadMultiPage1" SelectedIndex="0" Skin="MyMirion"
                EnableEmbeddedSkins="false" AutoPostBack="true" OnTabClick="RadTabStrip1_TabClick">
                <Tabs>
                    <telerik:RadTab ID="rtNotes" PageViewID="Notes_Tab" runat="server" Text="Notes" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="Notes"></telerik:RadTab>
                    <telerik:RadTab ID="rtCSRequests" PageViewID="CSRequests_Tab" runat="server" Text="CSRs" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="CSRequests"></telerik:RadTab>
                    <telerik:RadTab ID="rtOrders" PageViewID="Orders_Tab" runat="server" Text="Orders" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="Orders"></telerik:RadTab>
                    <telerik:RadTab ID="rtInvoices" PageViewID="Invoices_Tab" runat="server" Text="Invoices" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="Invoices"></telerik:RadTab>
                    <telerik:RadTab ID="rtLocations" PageViewID="Locations_Tab" runat="server" Text="Locations" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="Locations"></telerik:RadTab>
                    <telerik:RadTab ID="rtUsers" PageViewID="Users_Tab" runat="server" Text="Users" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="Users"></telerik:RadTab>
                    <telerik:RadTab ID="rtBadges" PageViewID="Badges_Tab" runat="server" Text="Badges" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="Badges"></telerik:RadTab>
                    <telerik:RadTab ID="rtReturns" PageViewID="Returns_Tab" runat="server" Text="Returns" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="Returns"></telerik:RadTab>
                    <telerik:RadTab ID="rtReads" PageViewID="Reads_Tab" runat="server" Text="Reads" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="Reads"></telerik:RadTab>
                    <telerik:RadTab ID="rtDocuments" PageViewID="Documents_Tab" runat="server" Text="Documents" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="Documents"></telerik:RadTab>
                    <telerik:RadTab ID="rtCalendars" PageViewID="Calendars_Tab" runat="server" Text="Calendars" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="Calendars"></telerik:RadTab>
                    <telerik:RadTab ID="rtAssociatedAccounts" PageViewID="AssociatedAccounts_Tab" runat="server" Text="AssociatedAccounts" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="AssociatedAccounts"></telerik:RadTab>
                    <telerik:RadTab ID="rtBillingGroups" PageViewID="BillingGroups_Tab" runat="server" Text="Invoice Delivery Methods" SelectedCssClass="SelectedTab" HoveredCssClass="HoverTab" Value="InvoiceDeliveryMethods"></telerik:RadTab>
                </Tabs>
            </telerik:RadTabStrip>
            <telerik:RadMultiPage ID="RadMultiPage1" runat="server" RenderSelectedPageOnly="true" SelectedIndex="0">
                <telerik:RadPageView ID="Notes_Tab" runat="server">
                    <%--NOTES--%>
                    <div id="Note_Tab">
                        <asp:Panel ID="pnlNotes" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnCreateNote" runat="server" Font-Bold="true" ForeColor="#FFFFFF"
                                                CommandName="CreateNote" CommandArgument='<%= Request.QueryString["ID"] %>' ToolTip="Create Note"
                                                CssClass="Icon Add" OnClick="lnkbtnCreateNote_Click">Create Note</asp:LinkButton>
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_Notes" runat="server" OnClick="lnkbtnClearFilters_Notes_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>
                                <telerik:RadGrid ID="rgNotes" runat="server" CssClass="OTable"
                                    AllowMultiRowSelection="false"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    EnableLinqExpressions="false"
                                    AllowPaging="true" AllowSorting="true"
                                    OnNeedDataSource="rgNotes_NeedDataSource"
                                    Width="99.9%" PageSize="10"
                                    EnableViewState="false" GridLines="None" SkinID="Default">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <SortingSettings EnableSkinSortStyles="true" />
                                    <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>
                                            <telerik:GridDateTimeColumn DataField="CreatedDate" HeaderText="Created On" UniqueName="CreatedDate"
                                                AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="125px" FilterControlWidth="125px" DataFormatString="{0:d}"
                                                SortExpression="CreatedDate" />
                                            <telerik:GridBoundColumn DataField="CreatedBy" HeaderText="Created By" UniqueName="CreatedBy" HeaderStyle-Width="125px" AllowSorting="true"
                                                FilterControlWidth="125px" SortExpression="CreatedBy" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" AllowFiltering="true" />
                                            <telerik:GridBoundColumn DataField="NoteText" HeaderText="Note" UniqueName="NoteText" HeaderStyle-Width="300px" AllowFiltering="true"
                                                FilterControlWidth="300px" SortExpression="NoteText" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" AllowSorting="true" />
                                        </Columns>

                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="AssociatedAccounts_Tab" runat="server">
                    <%--ASSOCIATED ACCOUNT--%>
                    <div id="AssociatedAccount_Tab">
                        <asp:Panel ID="pnlAssociatedAccount" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <asp:HiddenField ID="hselectedID" runat="server" />
                                    <ul>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnCreateAssociatedAccount" runat="server" Font-Bold="true" ForeColor="#FFFFFF"
                                                CommandName="CreateAssociatedAccount" CommandArgument='<%= Request.QueryString["ID"] %>' ToolTip="Create AssociatedAccount"
                                                CssClass="Icon Add" OnClick="lnkbtnCreateAssociatedAccount_Click">Create Associated Account</asp:LinkButton>
                                        </li>

                                        <li>
                                            <asp:LinkButton ID="lnkbtnRemoveAssociatedAccount" runat="server" Font-Bold="true" ForeColor="#FFFFFF"
                                                CommandName="RemoveAssociatedAccount" CommandArgument='<%= Request.QueryString["ID"] %>' ToolTip="Remove Associated Account"
                                                Visible="false" CssClass="Icon Remove" OnClientClick="javascript:return checkSelecteditem();" OnClick="lnkbtnRemoveAssociatedAccount_Click">Remove Associated Account</asp:LinkButton>


                                        </li>

                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_AssociatedAccount" runat="server" OnClick="lnkbtnClearFilters_AssociatedAccount_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>
                                <telerik:RadGrid ID="rgAssociatedAccount" runat="server" CssClass="OTable"
                                    AutoGenerateColumns="False"
                                    AllowFilteringByColumn="True"
                                    EnableLinqExpressions="False" AllowSorting="True"
                                    OnNeedDataSource="rgAssociatedAccount_NeedDataSource" AllowPaging="true" PageSize="10"
                                    Width="99.9%"
                                    ShowFooter="True"
                                    EnableViewState="False" GridLines="None" SkinID="Default" CellSpacing="0">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                        <Selecting AllowRowSelect="True" />
                                        <ClientEvents OnRowSelected="RowSelected" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <SortingSettings EnableSkinSortStyles="true" />
                                    <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <EditFormSettings>
                                            <EditColumn FilterControlAltText="Filter EditCommandColumn column">
                                            </EditColumn>
                                        </EditFormSettings>
                                        <FooterStyle CssClass="Footer"></FooterStyle>
                                        <CommandItemSettings ExportToPdfText="Export to PDF" />
                                        <RowIndicatorColumn FilterControlAltText="Filter RowIndicator column" Visible="True">
                                        </RowIndicatorColumn>
                                        <ExpandCollapseColumn FilterControlAltText="Filter ExpandColumn column" Visible="True">
                                        </ExpandCollapseColumn>
                                        <Columns>
                                            <telerik:GridBoundColumn AllowFiltering="true" AllowSorting="true" DataField="accountid" DataFormatString="{0:d}" FilterControlWidth="100px" HeaderStyle-Width="100px" HeaderText="Account ID" SortExpression="accountid" UniqueName="accountid">
                                                <HeaderStyle Width="100px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" DataField="accountname" FilterControlWidth="125px" HeaderStyle-Width="125px" HeaderText="Account Name" SortExpression="accountname" UniqueName="accountname">
                                                <HeaderStyle Width="125px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridNumericColumn AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" DataField="id1count" FilterControlWidth="80px" FooterStyle-HorizontalAlign="Left" HeaderStyle-Width="80px" HeaderText="ID 1" SortExpression="id1count" UniqueName="id1count">
                                                <FooterStyle HorizontalAlign="Left" />
                                                <HeaderStyle Width="80px" />
                                            </telerik:GridNumericColumn>
                                            <telerik:GridBoundColumn AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" DataField="id2elitecount" FilterControlWidth="80px" FooterStyle-HorizontalAlign="Left" HeaderStyle-Width="80px" HeaderText="ID 2 Elite" SortExpression="id2elitecount" UniqueName="id2elitecount">
                                                <FooterStyle HorizontalAlign="Left" />
                                                <HeaderStyle Width="80px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" DataField="id2count" FilterControlWidth="80px" FooterStyle-HorizontalAlign="Left" HeaderStyle-Width="80px" HeaderText="ID 2" SortExpression="id2count" UniqueName="id2count">
                                                <FooterStyle HorizontalAlign="Left" />
                                                <HeaderStyle Width="80px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" DataField="idpluscount" FilterControlWidth="80px" FooterStyle-HorizontalAlign="Left" HeaderStyle-Width="80px" HeaderText="ID Plus" SortExpression="idpluscount" UniqueName="idpluscount">
                                                <FooterStyle HorizontalAlign="Left" />
                                                <HeaderStyle Width="80px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" DataField="idlinkcount" FilterControlWidth="80px" FooterStyle-HorizontalAlign="Left" HeaderStyle-Width="80px" HeaderText="ID Link" SortExpression="idlinkcount" UniqueName="idlinkcount">
                                                <FooterStyle HorizontalAlign="Left" />
                                                <HeaderStyle Width="80px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" DataField="idusbcount" FilterControlWidth="80px" FooterStyle-HorizontalAlign="Left" HeaderStyle-Width="80px" HeaderText="ID USB" SortExpression="idusbcount" UniqueName="idusbcount">
                                                <FooterStyle HorizontalAlign="Left" />
                                                <HeaderStyle Width="80px" />
                                            </telerik:GridBoundColumn>
                                        </Columns>

                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="CSRequests_Tab" runat="server">
                    <%--C/S REQUEST--%>
                    <div id="CSrequest_Tab">
                        <asp:Panel ID="pnlCSRequests" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnCreateCSRequest" runat="server" Font-Bold="true" ForeColor="#FFFFFF"
                                                CommandName="CreateRequest" CommandArgument='<%= Request.QueryString["ID"] %>' ToolTip="Create CS Request"
                                                CssClass="Icon Add" OnClick="lnkbtnCreateCSRequest_Click" Visible="false">Create CS Request</asp:LinkButton>
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_CSRequests" runat="server" OnClick="lnkbtnClearFilters_CSRequests_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>
                                <telerik:RadGrid ID="rgCSRequests" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateDeleteColumn="false"
                                    AllowMultiRowSelection="false"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="false"
                                    AllowPaging="true" AllowSorting="true"
                                    OnNeedDataSource="rgCSRequests_NeedDataSource"
                                    Width="99.9%" PageSize="10"
                                    GridLines="None" SkinID="Default">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>
                                            <telerik:GridTemplateColumn HeaderText="Case #" SortExpression="CaseID" DataField="CaseID"
                                                AllowFiltering="true" HeaderStyle-Width="100px" FilterControlWidth="50px" UniqueName="CaseID"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true">
                                                <ItemTemplate>
                                                    <a href='<%# String.Format("/CustomerService/CreateRequestCase.aspx?CaseID={0}&OpenFrom=Account", Eval("CaseID")) %>'
                                                        target="_top" title='<%# string.Format("Create CS Request for Case #{0}", Eval("CaseID")) %>'>
                                                        <img src="/images/icons/pencil.png" style="border: 0px none; width: 16px; height: 16px;" /><%# Eval("CaseID")%></a>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridDateTimeColumn DataField="RequestDate" HeaderText="Request On" UniqueName="RequestDate" DataFormatString="{0:d}"
                                                AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="175px" FilterControlWidth="125px" />
                                            <telerik:GridBoundColumn DataField="RequestedBy" HeaderText="Requestor" SortExpression="RequestedBy" UniqueName="RequestedBy" AllowSorting="true"
                                                HeaderStyle-Width="175px" FilterControlWidth="125px" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" AllowFiltering="true" />
                                            <telerik:GridBoundColumn UniqueName="CaseStatusDesc" DataField="CaseStatusDesc" HeaderText="Status" HeaderStyle-Width="150px" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbCaseStatus" DataSourceID="sqlStatus_CSRequests" DataTextField="CaseStatusDesc"
                                                        DataValueField="CaseStatusDesc" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("CaseStatusDesc").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="CaseStatusIndexChanged" Width="125px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock2" runat="server">
                                                        <script type="text/javascript">
                                                            function CaseStatusIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("CaseStatusDesc", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn UniqueName="RequestDesc" DataField="RequestDesc" HeaderText="Type" HeaderStyle-Width="200px" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbRequestType" DataSourceID="sqlType_CSRequests" DataTextField="RequestDesc"
                                                        DataValueField="RequestDesc" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("RequestDesc").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="RequestTypeIndexChanged" Width="175px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock3" runat="server">
                                                        <script type="text/javascript">
                                                            function RequestTypeIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("RequestDesc", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="CaseNote" HeaderText="Note" SortExpression="CaseNote" UniqueName="CaseNote"
                                                FilterControlWidth="200px" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" AllowFiltering="true" AllowSorting="true">
                                                <ItemStyle Wrap="true"></ItemStyle>
                                            </telerik:GridBoundColumn>
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="Orders_Tab" runat="server">
                    <%--ORDERS--%>
                    <div id="Order_Tab">
                        <asp:Panel ID="pnlOrders" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnCreateOrder" runat="server" ToolTip="Create Order"
                                                CommandName="CreateOrder" CommandArgument='<%# Request.QueryString["ID"] %>'
                                                CssClass="Icon Add" OnClick="lnkbtnCreateOrder_Click">Create Order</asp:LinkButton>
                                        </li>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnLostDamagedOrders" runat="server" ToolTip="Lost/Damaged Orders"
                                                CommandName="LostDamagedOrders" CommandArgument='<%# Request.QueryString["ID"] %>'
                                                CssClass="Icon Package" OnClick="lnkbtnLostDamagedOrders_Click">ID1 Lost/Damaged Order</asp:LinkButton>
                                        </li>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnIDPlusLostReplacementOrder" runat="server" ToolTip="ID+ / ID2 Lost Replacement Orders"
                                                CommandName="LostReplacementOrders" CommandArgument='<%# Request.QueryString["ID"] %>'
                                                CssClass="Icon Package" OnClick="lnkbtnIDPlusLostReplacementOrder_Click">ID+ / ID2 Lost Replace Order</asp:LinkButton>
                                        </li>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnIDPlusRecallOrder" runat="server" ToolTip="ID+ Recall Orders"
                                                CommandName="CreateRecall" CommandArgument='<%# Request.QueryString["ID"] %>'
                                                CssClass="Icon Package" OnClick="lnkbtnIDPlusRecallOrder_Click">ID+ Recall Order</asp:LinkButton>

                                            <%--<asp:LinkButton ID="lnkbtnIDPlusRecallOrder" runat="server" ToolTip="ID+ Recall Orders"
												CommandName="RecallOrders" CommandArgument='<%# Request.QueryString["ID"] %>'
												CssClass="Icon Package" OnClick="lnkbtnIDPlusRecallOrder_Click">ID+ Recall Order</asp:LinkButton>--%>
                                        </li>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnEmailOrderAck" runat="server" Height="18px" ToolTip="E-mail Order Acknowledgement"
                                                CommandName="EmailOrderAck" CommandArgument='<%# Request.QueryString["ID"] %>'
                                                CssClass="Icon EmailAttachment" OnClick="lnkbtnEmailOrderAck_Click">Email Order Acknowledgement</asp:LinkButton>
                                        </li>
                                        <li id="divCreditMemo" runat="server" visible='<%# DisplayCreditMemoButton() %>'>
                                            <asp:LinkButton ID="lnkbtnCreateCreditMemo" runat="server" ToolTip="Create Credit Memo"
                                                CommandName="CreateCreditMemo" CommandArgument='<%# Request.QueryString["ID"] %>'
                                                CssClass="Icon CreditMemo" OnClick="lnkbtnCreateCreditMemo_Click">Create Credit Memo</asp:LinkButton>
                                        </li>
                                        <%--<li class="RightAlign">
											<asp:LinkButton ID="lnkbtnClearFilters_Orders" runat="server" OnClick="lnkbtnClearFilters_Orders_Click"
												Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
										</li>--%>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>

                                <div class="OToolbar JoinTableExt" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="LinkButton7" runat="server" OnClick="lnkbtnClearFilters_Orders_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>

                                <telerik:RadGrid ID="rgOrders" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    AllowMultiRowSelection="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="false"
                                    OnNeedDataSource="rgOrders_NeedDataSource"
                                    Width="99.9%" PageSize="10"
                                    GridLines="None" SkinID="Default"
                                    AllowPaging="true" AllowSorting="true">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="OrderID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>

                                            <telerik:GridTemplateColumn HeaderText="Order #" SortExpression="OrderID" DataField="OrderID" AllowFiltering="true" UniqueName="OrderID" HeaderStyle-Width="100px" FilterControlWidth="50px"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="hyprlnkOrderID_Orders" runat="server" NavigateUrl='<%# String.Format("/CustomerService/ReviewOrder.aspx?ID={0}", Eval("OrderId")) %>'
                                                        Text='<%# Eval("OrderId")%>' Target="_top" ToolTip="Review Order Details"></asp:HyperLink>
                                                    &nbsp;
													<asp:ImageButton ID="imgBtnOrderReceipt" runat="server" AlternateText="Generate Receipt" ToolTip="Generate Receipt"
                                                        ImageUrl="/images/icons/page_white_acrobat.png"
                                                        OnClick="imgBtnOrderReceipt_OnClick"
                                                        Visible='<%# DataBinder.Eval(Container.DataItem, "DisplayReceipt") %>'
                                                        CommandName="GenerateReceipt" CommandArgument='<%# DataBinder.Eval(Container.DataItem,"OrderId","" ) %>' />
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>

                                            <telerik:GridBoundColumn UniqueName="OrderType" DataField="OrderType" HeaderText="Type" HeaderStyle-Width="110px" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbOrderType" DataSourceID="sqlType_Orders" DataTextField="OrderType"
                                                        DataValueField="OrderType" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("OrderType").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="OrderTypeIndexChanged" Width="100px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock4" runat="server">
                                                        <script type="text/javascript">
                                                            function OrderTypeIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("OrderType", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>
                                            <telerik:GridDateTimeColumn DataField="OrderDate" HeaderText="Order Date" UniqueName="OrderDate" DataFormatString="{0:d}"
                                                AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="130px" FilterControlWidth="100px" />
                                            <telerik:GridBoundColumn DataField="CreatedBy" HeaderText="Created By" SortExpression="CreatedBy" UniqueName="CreatedBy" AllowSorting="true"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" HeaderStyle-Width="100px" FilterControlWidth="60px" AllowFiltering="true" />
                                            <telerik:GridBoundColumn UniqueName="Status" DataField="Status" HeaderText="Status" HeaderStyle-Width="125px" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbOrderStatus" DataSourceID="sqlStatus_Orders" DataTextField="Status"
                                                        DataValueField="Status" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("Status").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="OrderStatusIndexChanged" Width="100px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock5" runat="server">
                                                        <script type="text/javascript">
                                                            function OrderStatusIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("Status", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="PONumber" HeaderText="PO#" SortExpression="PONumber" UniqueName="PONumber" HeaderStyle-Width="100px"
                                                FilterControlWidth="50px" CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" AllowFiltering="true" AllowSorting="true" />
                                            <telerik:GridBoundColumn UniqueName="PaymentMethod" DataField="PaymentMethod" HeaderText="Method" HeaderStyle-Width="100px" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbOrderPaymentMethod" DataSourceID="sqlPaymentMethod_Orders" DataTextField="PaymentMethod"
                                                        DataValueField="PaymentMethod" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("PaymentMethod").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="OrderPaymentMethodIndexChanged" Width="75px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock6" runat="server">
                                                        <script type="text/javascript">
                                                            function OrderPaymentMethodIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("PaymentMethod", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="Quantity" HeaderText="Qty." SortExpression="Quantity" UniqueName="Quantity" HeaderStyle-Width="45px" AllowFiltering="false" AllowSorting="true">
                                                <HeaderStyle HorizontalAlign="Center" />
                                                <ItemStyle HorizontalAlign="Center" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridTemplateColumn DataField="OrderTotal" HeaderText="Total" UniqueName="OrderTotal" AllowFiltering="true" FilterControlWidth="65px" HeaderStyle-Width="100px">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblOrderTotal" runat="server" Text='<%# String.Format("{0:#,0.00}", Eval("OrderTotal")) %>' />
                                                    <%# DataBinder.Eval(Container.DataItem, "CurrencyCode") %>
                                                </ItemTemplate>
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" />
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridBoundColumn DataField="USDOrderTotalWithCurrencyCode" UniqueName="USDOrderTotalWithCurrencyCode" HeaderText="USD Total" AllowFiltering="false" AllowSorting="true" HeaderStyle-Width="85px" ItemStyle-Width="85px">
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" />
                                            </telerik:GridBoundColumn>
                                        </Columns>
                                    </MasterTableView>
                                    <ClientSettings AllowColumnsReorder="false">
                                        <Selecting AllowRowSelect="false"></Selecting>
                                    </ClientSettings>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="Invoices_Tab" runat="server">
                    <%--INVOICES--%>
                    <div id="Invoice_Tab">
                        <asp:Panel ID="pnlInvoices" runat="server" SkinID="Default">
                            <div style="width: 100%;">

                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_Invoices" runat="server" OnClick="lnkbtnClearFilters_Invoices_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnInvoiceExport" runat="server"
                                                CommandName="Convert_Click" CssClass="Icon Export"
                                                OnClick="lnkbtnInvoiceExport_Click">Export to Excel</asp:LinkButton>
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>
                                <telerik:RadGrid ID="rgInvoices" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    AllowMultiRowSelection="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="true"
                                    OnNeedDataSource="rgInvoices_NeedDataSource"
                                    OnItemDataBound="rgInvoices_ItemDataBound"
                                    AllowPaging="true" AllowSorting="true"
                                    Width="99.9%" PageSize="10"
                                    GridLines="None" SkinID="Default">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="Account,InvoiceID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>
                                            <telerik:GridTemplateColumn AllowFiltering="false" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="25px">
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="imgbtnViewInvoiceDetails" runat="server" ImageUrl="~/images/icons/magnifier.png"
                                                        AlternateText="View Invoice Details" ToolTip="View Invoice Details" OnClick="imgbtnViewInvoiceDetails_OnClick"
                                                        CommandName="OpenDialog" CommandArgument='<%# Eval("InvoiceID") %>' />
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn HeaderText="Invoice #" SortExpression="InvoiceID" DataField="InvoiceID"
                                                AllowFiltering="true" HeaderStyle-Width="100px" FilterControlWidth="50px" UniqueName="InvoiceID"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="lnkInvoiceNumber" Target="_blank" runat="server" Text='<%# Eval("InvoiceID")%>' ToolTip="Generate Invoice" />
                                                    <asp:Label ID="lblInvoiceNumber" runat="server" Text='<%# Eval("InvoiceID")%>' />
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn HeaderText="Order #" SortExpression="OrderID" DataField="OrderID"
                                                AllowFiltering="true" HeaderStyle-Width="100px" FilterControlWidth="50px" UniqueName="OrderID"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="hyprlnkOrderID_Invoices" runat="server" NavigateUrl='<%# String.Format("/CustomerService/ReviewOrder.aspx?ID={0}", Eval("OrderId")) %>'
                                                        Text='<%# Eval("OrderId")%>' Target="_top" ToolTip="Review Order Details"></asp:HyperLink>
                                                    <asp:HyperLink ID="hlInvoiceOrders" runat="server" ToolTip="View Orders for Invoice">Orders</asp:HyperLink>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridDateTimeColumn DataField="InvoiceDate" DataFormatString="{0:d}"
                                                HeaderText="Invoice Date" SortExpression="InvoiceDate" UniqueName="InvoiceDate"
                                                HeaderStyle-Width="125px" FilterControlWidth="100px" AllowFiltering="true" AllowSorting="true">
                                            </telerik:GridDateTimeColumn>
                                            <telerik:GridDateTimeColumn DataField="LastTransaction" DataFormatString="{0:d}"
                                                HeaderText="Last Transaction" SortExpression="LastTransaction" UniqueName="LastTransaction" PickerType="DatePicker"
                                                HeaderStyle-Width="125px" FilterControlWidth="100px" AllowFiltering="true" AllowSorting="true">
                                            </telerik:GridDateTimeColumn>
                                            <telerik:GridTemplateColumn DataField="InvoiceAmount" HeaderText="Amount" UniqueName="InvoiceAmount" AllowFiltering="true"
                                                SortExpression="InvoiceAmount" HeaderStyle-Width="100px" FilterControlWidth="68px">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblAmount" runat="server" Text='<%# String.Format("{0:#,0.00}", Eval("InvoiceAmount")) %>' />
                                                    <%# DataBinder.Eval(Container.DataItem, "CurrencyCode") %>
                                                </ItemTemplate>
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" />
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn DataField="PaymentAmount" HeaderText="Payments" UniqueName="PaymentAmount" AllowFiltering="true"
                                                SortExpression="PaymentAmount" HeaderStyle-Width="100px" FilterControlWidth="68px">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblPayments" runat="server" Text='<%# String.Format("{0:#,0.00}", Eval("PaymentAmount")) %>' />
                                                    <%# DataBinder.Eval(Container.DataItem, "CurrencyCode") %>
                                                </ItemTemplate>
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" />
                                            </telerik:GridTemplateColumn>
                                            <%--<telerik:GridTemplateColumn DataField="Adjustments" HeaderText="Adjustments" UniqueName="Adjustments" AllowFiltering="true"
                                                SortExpression="Adjustments" HeaderStyle-Width="100px" FilterControlWidth="68px">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblAdjustments" runat="server" Text='<%# String.Format("{0:#,0.00}", Eval("Adjustments")) %>' />
                                                    <%# DataBinder.Eval(Container.DataItem, "CurrencyCode") %>
                                                </ItemTemplate>
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" />
                                            </telerik:GridTemplateColumn>--%>
                                            <telerik:GridTemplateColumn DataField="CreditAmount" HeaderText="Credits/Debits" UniqueName="CreditAmount" AllowFiltering="true"
                                                SortExpression="CreditAmount" HeaderStyle-Width="100px" FilterControlWidth="68px">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblCredits" runat="server" Text='<%# String.Format("{0:#,0.00}", Eval("CreditAmount")) %>' />
                                                    <%# DataBinder.Eval(Container.DataItem, "CurrencyCode") %>
                                                </ItemTemplate>
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" />
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn DataField="Balance" HeaderText="Balance" UniqueName="Balance" AllowFiltering="true"
                                                SortExpression="Balance" HeaderStyle-Width="100px" FilterControlWidth="68px">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblBalance" runat="server" Text='<%# String.Format("{0:#,0.00}", Eval("Balance")) %>' />
                                                    <%# DataBinder.Eval(Container.DataItem, "CurrencyCode") %>
                                                </ItemTemplate>
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" />
                                            </telerik:GridTemplateColumn>
                                        </Columns>
                                    </MasterTableView>
                                    <ClientSettings AllowColumnsReorder="false">
                                        <Selecting AllowRowSelect="false"></Selecting>
                                    </ClientSettings>
                                </telerik:RadGrid>
                                <%--OUTSTANDING BALANCE--%>
                                <table class="OTable">
                                    <tr>
                                        <th style="text-align: right !important; padding-right: 5px;">Outstanding Balance:</th>
                                        <th style="text-align: right !important; width: 200px">
                                            <asp:Label ID="lblOutstandingBalance" runat="server"></asp:Label></th>
                                    </tr>
                                </table>
                                <%--END --%>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="Locations_Tab" runat="server">
                    <%--LOCATIONS--%>
                    <div id="Location_Tab">
                        <asp:Panel ID="pnlLocations" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnCreateLocation" runat="server" Font-Bold="true" ForeColor="#FFFFFF"
                                                CommandName="CreateLocation" CommandArgument='<%= Request.QueryString["ID"] %>' ToolTip="Create Location"
                                                CssClass="Icon Add" OnClick="lnkbtnCreateLocation_Click">Create Location</asp:LinkButton>
                                        </li>
                                        <li>
                                            <asp:Label ID="lblActiveTotalLocations" runat="server"></asp:Label>
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_Locations" runat="server" OnClick="lnkbtnClearFilters_Locations_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>
                                <telerik:RadGrid ID="rgLocations" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    AllowMultiRowSelection="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="false"
                                    OnNeedDataSource="rgLocations_NeedDataSource"
                                    AllowPaging="True" AllowSorting="True"
                                    Style="border: 1px solid #D6712D;"
                                    Width="99.8%" PageSize="10"
                                    GridLines="None" SkinID="Default">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>
                                            <telerik:GridTemplateColumn DataField="LocationName" UniqueName="LocationName" HeaderText="Name" FilterControlWidth="200px" ItemStyle-Wrap="false"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" SortExpression="LocationName">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="hyprlnkLocationID" runat="server" NavigateUrl='<%# String.Format("EditLocation.aspx?AccountID={0}&LocationID={1}", Eval("AccountID"), Eval("LocationID")) %>'
                                                        Text='<%# Eval("LocationName")%>' Target="_top" ToolTip="Manage Location"></asp:HyperLink>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridBoundColumn DataField="ShippingAddress" HeaderText="Address" SortExpression="ShippingAddress" AllowFiltering="true" AllowSorting="true"
                                                UniqueName="ShippingAddress" FilterControlWidth="200px" ItemStyle-Wrap="false" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" />
                                            <telerik:GridBoundColumn DataField="City" HeaderText="City" SortExpression="City" UniqueName="City" AllowFiltering="true"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" FilterControlWidth="100px" AllowSorting="true">
                                                <HeaderStyle Width="175px" />
                                                <ItemStyle Width="175px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn UniqueName="StateAbbrev" DataField="StateAbbrev" HeaderText="States" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbLocationStates" DataSourceID="sqlState_Locations" DataTextField="StateAbbrev"
                                                        DataValueField="StateAbbrev" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("StateAbbrev").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="LocationStateIndexChanged" Width="100px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock7" runat="server">
                                                        <script type="text/javascript">
                                                            function LocationStateIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("StateAbbrev", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>

                                            <telerik:GridBoundColumn DataField="BillingGroupID" HeaderText="BillingGroup" SortExpression="BillingGroupID" AllowFiltering="true" AllowSorting="true"
                                                UniqueName="BillingGroupID" FilterControlWidth="80px" ItemStyle-Wrap="false" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" />

                                            <telerik:GridTemplateColumn DataField="Active" HeaderText="Is Active" UniqueName="Active"
                                                HeaderStyle-Width="100px" FilterControlWidth="100px" AllowFiltering="true" SortExpression="Active">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblActiveStatus_Locations" runat="server" Text='<%# Convert.ToBoolean(Eval("Active")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="Users_Tab" runat="server">
                    <%--USERS--%>
                    <div id="User_Tab">
                        <asp:Panel ID="pnlUsers" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnCreateUser" runat="server" Font-Bold="true" ForeColor="#FFFFFF"
                                                CommandName="CreateUser" CommandArgument='<%= Request.QueryString["ID"] %>' ToolTip="Create User"
                                                CssClass="Icon Add" OnClick="lnkbtnCreateUser_Click">Create User</asp:LinkButton>
                                        </li>
                                        <li>
                                            <asp:Label ID="lblActiveTotalUsers" runat="server"></asp:Label>
                                        </li>
                                        <li>
                                            <input type="button" value="Manage Selected" id="btnUpdate" style="visibility:hidden;" />
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_Users" runat="server" OnClick="lnkbtnClearFilters_Users_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnUserExport" runat="server"
                                                CommandName="Convert_Click" CssClass="Icon Export"
                                                OnClick="lnkbtnUserExport_Click">Export to Excel</asp:LinkButton>
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>
                                <telerik:RadGrid ID="rgUsers" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    AllowMultiRowSelection="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="false"
                                    OnNeedDataSource="rgUsers_NeedDataSource"
                                    AllowPaging="True" AllowSorting="True"
                                    Style="border: 1px solid #D6712D;"
                                    Width="99.8%" PageSize="10"
                                    GridLines="None" SkinID="Default">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>
                                            <telerik:GridClientSelectColumn UniqueName="chkUser" HeaderStyle-Width="35px" ItemStyle-Width="40px">
                                            </telerik:GridClientSelectColumn>
                                            <telerik:GridTemplateColumn DataField="UserName" UniqueName="UserName" HeaderText="Name" HeaderStyle-Width="175px" FilterControlWidth="125px"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" SortExpression="UserName">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="hyprlnkUserID" runat="server" NavigateUrl='<%# String.Format("UserMaintenance.aspx?AccountID={0}&UserID={1}", Eval("AccountID"), Eval("UserID")) %>'
                                                        Text='<%# Eval("UserName")%>' Target="_top" ToolTip="Manage User"></asp:HyperLink>
                                                    <asp:HiddenField Value='<%# Eval("UserID") %>' runat="server" ClientIDMode="Static" ID="hdnUserID" />
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridBoundColumn UniqueName="UserRoleName" DataField="UserRoleName" HeaderText="User Role" HeaderStyle-Width="150px" AllowSorting="true" SortExpression="UserRoleName">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbUserRoleNames" DataSourceID="sqlUserRoleName_Users" DataTextField="UserRoleName"
                                                        DataValueField="UserRoleName" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("UserRoleName").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="UserRoleNameIndexChanged" Width="125px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock8" runat="server">
                                                        <script type="text/javascript">
                                                            function UserRoleNameIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("UserRoleName", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="FullName" HeaderText="Full Name" SortExpression="FullName" UniqueName="FullName" HeaderStyle-Width="175px" FilterControlWidth="100px"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" AllowFiltering="true" AllowSorting="true" />
                                            <telerik:GridTemplateColumn HeaderText="Email" DataField="Email" FilterControlWidth="125px" HeaderStyle-Width="200px"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" AllowFiltering="false" SortExpression="Email">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="hyprlnkEmail" NavigateUrl='<%# DataBinder.Eval(Container.DataItem, "Email", "mailto:{0}" ) %>' runat="server" Text='<%# Eval("Email") %>' ToolTip="E-mail User"></asp:HyperLink>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridBoundColumn UniqueName="LocationName" DataField="LocationName" HeaderText="Location" AllowSorting="true" SortExpression="LocationName">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbUserLocationNames" DataSourceID="sqlLocationName_Users" DataTextField="LocationName"
                                                        DataValueField="LocationName" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("LocationName").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="UserLocationNameIndexChanged" Width="225px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock9" runat="server">
                                                        <script type="text/javascript">
                                                            function UserLocationNameIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("LocationName", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>
                                            <telerik:GridTemplateColumn DataField="Active" HeaderText="Is Active" UniqueName="Active"
                                                HeaderStyle-Width="100px" FilterControlWidth="100px" AllowFiltering="true" SortExpression="Active">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblActiveStatus_Users" runat="server" Text='<%# Convert.ToBoolean(Eval("Active")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="Badges_Tab" runat="server">
                    <%--BADGES--%>
                    <div id="Badge_Tab">
                        <asp:Panel ID="pnlBadges" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li>
                                            <asp:Label ID="lblActiveTotalBadges" runat="server"></asp:Label>
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_Badges" runat="server" OnClick="lnkbtnClearFilters_Badges_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnBadgeExport" runat="server"
                                                CommandName="Convert_Click" CssClass="Icon Export"
                                                OnClick="lnkbtnBadgeExport_Click">Export to Excel</asp:LinkButton>
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>
                                <telerik:RadGrid ID="rgBadges" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    AllowMultiRowSelection="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="false"
                                    OnNeedDataSource="rgBadges_NeedDataSource"
                                    AllowPaging="True" AllowSorting="True"
                                    Style="border: 1px solid #D6712D;"
                                    Width="99.8%" PageSize="10"
                                    GridLines="None" SkinID="Default">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>
                                            <telerik:GridTemplateColumn HeaderText="Serial #" SortExpression="SerialNumber" HeaderStyle-Width="100px" FilterControlWidth="50px"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" DataField="SerialNumber" AllowFiltering="true" UniqueName="SerialNumber">
                                                <ItemTemplate>
                                                    <a href='<%# String.Format("Device.aspx?ID={0}&AccountID={1}", Eval("SerialNumber"), Eval("AccountID")) %>'
                                                        target="_top" title="View Badge Details"><%# Eval("SerialNumber") %></a>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn HeaderText="Assigned To" SortExpression="FullName" HeaderStyle-Width="125px" FilterControlWidth="75px"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" DataField="FullName" AllowFiltering="true" UniqueName="FullName">
                                                <ItemTemplate>
                                                    <a href='<%# String.Format("UserMaintenance.aspx?AccountID={0}&UserID={1}", Eval("AccountID"), Eval("UserID")) %>'
                                                        target="_top" title="Manage User"><%# Eval("FullName") %></a>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridBoundColumn UniqueName="BodyRegion" DataField="BodyRegion" HeaderText="Body Region" HeaderStyle-Width="125px" SortExpression="BodyRegion" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbBadgeBodyRegions" DataSourceID="sqlBodyRegion_Badges" DataTextField="BodyRegion"
                                                        DataValueField="BodyRegion" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("BodyRegion").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="BadgeBodyRegionIndexChanged" Width="100px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock10" runat="server">
                                                        <script type="text/javascript">
                                                            function BadgeBodyRegionIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("BodyRegion", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn UniqueName="ProductColor" DataField="ProductColor" HeaderText="Product Color" HeaderStyle-Width="125px" SortExpression="ProductColor" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbBadgeProductColors" DataSourceID="sqlProductColor_Badges" DataTextField="ProductColor"
                                                        DataValueField="ProductColor" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("ProductColor").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="BadgeProductColorIndexChanged" Width="100px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock11" runat="server">
                                                        <script type="text/javascript">
                                                            function BadgeProductColorIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("ProductColor", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>
                                            <telerik:GridTemplateColumn DataField="OrderID" HeaderText="Order #" SortExpression="OrderID" HeaderStyle-Width="100px" FilterControlWidth="50px"
                                                UniqueName="OrderID" CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" AllowFiltering="true">
                                                <ItemTemplate>
                                                    <a href='<%# String.Format("/CustomerService/ReviewOrder.aspx?ID={0}", Eval("OrderID")) %>' target="_top" title="Review Order Details"><%# Eval("OrderID") %></a>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridBoundColumn UniqueName="LocationName" DataField="LocationName" HeaderText="Location" SortExpression="LocationName" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbBadgeLocationNames" DataSourceID="sqlLocation_Badges" DataTextField="LocationName"
                                                        DataValueField="LocationName" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("LocationName").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="BadgeLocationIndexChanged" Width="200px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock12" runat="server">
                                                        <script type="text/javascript">
                                                            function BadgeLocationIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("LocationName", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                            </telerik:GridBoundColumn>
                                            <telerik:GridTemplateColumn DataField="FormattedInitialized" HeaderText="Is Initialized" UniqueName="FormattedInitialized"
                                                HeaderStyle-Width="100px" FilterControlWidth="75px" AllowFiltering="true" SortExpression="FormattedInitialized">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblInitializedStatus_Users" runat="server" Text='<%# Convert.ToBoolean(Eval("FormattedInitialized")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn DataField="Active" HeaderText="Is Active" UniqueName="Active"
                                                HeaderStyle-Width="100px" FilterControlWidth="75px" AllowFiltering="true" SortExpression="Active">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblActiveStatus_Users" runat="server" Text='<%# Convert.ToBoolean(Eval("Active")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="Returns_Tab" runat="server">
                    <%--RETURNS--%>
                    <div id="Return_Tab">
                        <asp:Panel ID="pnlReturns" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnCreateReturn" runat="server" Font-Bold="true" ForeColor="#FFFFFF"
                                                CommandName="CreateReturn" CommandArgument='<%= Request.QueryString["ID"] %>'
                                                CssClass="Icon Add" OnClick="lnkbtnCreateReturn_Click">Create Return</asp:LinkButton>
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_Returns" runat="server" OnClick="lnkbtnClearFilters_Returns_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" />
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>
                                <telerik:RadGrid ID="rgReturns" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    AllowMultiRowSelection="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="false"
                                    OnNeedDataSource="rgReturns_NeedDataSource"
                                    AllowPaging="True" AllowSorting="True"
                                    Style="border: 1px solid #D6712D;"
                                    Width="99.8%" PageSize="10"
                                    GridLines="None" SkinID="Default">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>
                                            <telerik:GridTemplateColumn DataField="ReturnID" UniqueName="ReturnID" HeaderText="Request #" SortExpression="ReturnID" AllowFiltering="true"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" HeaderStyle-Width="125px" FilterControlWidth="50px">
                                                <ItemTemplate>
                                                    <asp:HyperLink runat="server" ID="lnkbtnPaperClip" NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"ReturnID","Return.aspx?ID={0}" ) %>'
                                                        Visible='<%# DataBinder.Eval(Container.DataItem, "DisplayClipButton") %>'
                                                        Target="_top">
														<img src="/images/icons/attach.png" width="16" height="16" border="0" />
                                                    </asp:HyperLink>
                                                    &nbsp;
													<a href='<%# String.Format("Return.aspx?ID={0}", Eval("ReturnID")) %>' target="_top" title="View Return Details">
                                                        <%# Eval("ReturnID") %>
                                                    </a>
                                                    &nbsp;
													<asp:ImageButton ID="ImageButton1" runat="server" AlternateText="View RMA letter"
                                                        ImageUrl="/images/icons/page_white_acrobat.png"
                                                        Visible='<%# DataBinder.Eval(Container.DataItem, "DisplayPdf") %>'
                                                        OnClientClick='<%# Eval("ReturnID", "ViewRMApdf(\"{0}\");") %>' />
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridDateTimeColumn DataField="CreatedDate" DataFormatString="{0:d}" AllowFiltering="true" AllowSorting="true"
                                                HeaderText="Created On" SortExpression="CreatedDate" UniqueName="CreatedDate" FilterControlWidth="100px" HeaderStyle-Width="150px">
                                            </telerik:GridDateTimeColumn>
                                            <telerik:GridBoundColumn UniqueName="Type" DataField="Type" HeaderText="Type" SortExpression="Type" HeaderStyle-Width="150px" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbBadgeLocationNames" DataSourceID="sqlType_Returns" DataTextField="Type"
                                                        DataValueField="Type" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("Type").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="TypeReturnIndexChanged" Width="150px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock13" runat="server">
                                                        <script type="text/javascript">
                                                            function TypeReturnIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("Type", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                                <ItemStyle Width="150px" Wrap="true" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="Reason" HeaderText="Reason" SortExpression="Reason" UniqueName="Reason" FilterControlWidth="125px"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" HeaderStyle-Width="150px" AllowFiltering="true" AllowSorting="true">
                                                <ItemStyle Width="150px" Wrap="true" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="Status" HeaderText="Status" SortExpression="Status" UniqueName="Status" FilterControlWidth="150px"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" HeaderStyle-Width="175px" AllowFiltering="true" AllowSorting="true">
                                                <ItemStyle Width="175px" Wrap="true" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn HeaderText="Serial #" SortExpression="MySerialNoString" AllowFiltering="true" FilterControlWidth="75px" AllowSorting="true"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" HeaderStyle-Width="100px" UniqueName="MySerialNoString" DataField="MySerialNoString">
                                                <ItemStyle Width="100px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="DeviceCount" HeaderText="Badge Count" SortExpression="DeviceCount" FilterControlWidth="75px" AllowSorting="true"
                                                UniqueName="DeviceCount" AllowFiltering="true" CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" HeaderStyle-Width="100px">
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" Width="100px" />
                                            </telerik:GridBoundColumn>
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="Reads_Tab" runat="server">
                    <%--READS--%>
                    <div id="Read_Tab">
                        <asp:Panel ID="pnlReads" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_Reads" runat="server" OnClick="lnkbtnClearFilters_Reads_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" />
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnReadExport" runat="server"
                                                CommandName="Convert_Click" CssClass="Icon Export"
                                                OnClick="lnkbtnReadExport_Click">Export to Excel</asp:LinkButton>
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>
                                <telerik:RadGrid ID="rgReads" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    AllowMultiRowSelection="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="true"
                                    OnNeedDataSource="rgReads_NeedDataSource"
                                    AllowPaging="True" AllowSorting="True"
                                    Width="99.9%" PageSize="10"
                                    GridLines="None" SkinID="Default">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>
                                            <telerik:GridDateTimeColumn DataField="CompleteExposureDate" AllowFiltering="true" AllowSorting="true"
                                                HeaderText="Exposure Date" SortExpression="CompleteExposureDate" UniqueName="CompleteExposureDate"
                                                HeaderStyle-Width="150px" FilterControlWidth="100px">
                                                <ItemStyle Width="150px" Wrap="false" />
                                            </telerik:GridDateTimeColumn>
                                            <telerik:GridBoundColumn DataField="TimeZoneDesc" UniqueName="TimeZoneDesc" AllowFiltering="false" AllowSorting="true">
                                                <ItemStyle Width="75px" />
                                                <HeaderStyle Width="75px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="Username" HeaderText="Username" SortExpression="Username" UniqueName="Username" AllowFiltering="true"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" HeaderStyle-Width="125px" FilterControlWidth="75px" AllowSorting="true">
                                                <ItemStyle Width="125px" Wrap="false" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="FullName" HeaderText="Full Name" SortExpression="FullName" UniqueName="FullName" AllowFiltering="true"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" HeaderStyle-Width="125px" FilterControlWidth="75px" AllowSorting="true">
                                                <ItemStyle Width="125px" Wrap="true" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridTemplateColumn HeaderText="Serial #" SortExpression="SerialNumber" AllowFiltering="true" DataField="SerialNumber"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" HeaderStyle-Width="100px" FilterControlWidth="50px" UniqueName="SerialNumber">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="lbkbtnSerialNumber_Reads" NavigateUrl='<%# String.Format("Device.aspx?ID={0}&AccountID={1}", Eval("SerialNumber"), Eval("AccountID")) %>'
                                                        runat="server" Text='<%# FuncTrimSerialNo(DataBinder.Eval(Container.DataItem, "SerialNumber", "")) %>' Target="_top" ToolTip="View Badge Details">
                                                    </asp:HyperLink>
                                                </ItemTemplate>
                                                <ItemStyle Width="100px" Wrap="false" />
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridBoundColumn UniqueName="BodyRegion" DataField="BodyRegion" HeaderText="Body Region" HeaderStyle-Width="100px" SortExpression="BodyRegion" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbReadBodyRegions" DataSourceID="sqlBodyRegion_Badges" DataTextField="BodyRegion"
                                                        DataValueField="BodyRegion" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("BodyRegion").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="ReadBodyRegionIndexChanged" Width="75px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock14" runat="server">
                                                        <script type="text/javascript">
                                                            function ReadBodyRegionIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("BodyRegion", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                                <ItemStyle Width="100px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="Deep" HeaderText="Deep" SortExpression="Deep" UniqueName="Deep" HeaderStyle-Width="100px" FilterControlWidth="60px"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" EmptyDataText="*" AllowFiltering="true" AllowSorting="true">
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="Shallow" HeaderText="Shallow" SortExpression="Shallow" UniqueName="Shallow" HeaderStyle-Width="100px" FilterControlWidth="60px"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" EmptyDataText="*" AllowFiltering="true" AllowSorting="true">
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="Eye" HeaderText="Eye" SortExpression="Eye" UniqueName="Eye" HeaderStyle-Width="100px" FilterControlWidth="60px"
                                                CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" EmptyDataText="*" AllowFiltering="true" AllowSorting="true">
                                                <HeaderStyle HorizontalAlign="Right" />
                                                <ItemStyle HorizontalAlign="Right" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="UOMLocation" HeaderText="UOM" SortExpression="UOMLocation" UniqueName="UOMLocation" AllowFiltering="false" AllowSorting="true">
                                                <HeaderStyle HorizontalAlign="Center" />
                                                <ItemStyle HorizontalAlign="Center" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="Anomaly" UniqueName="Anomaly" AllowFiltering="false" AllowSorting="true" Visible="false" />
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="Documents_Tab" runat="server">
                    <%--DOCUMENTS--%>
                    <div id="Document_Tab">
                        <asp:Panel ID="pnlDocuments" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnAddDocument" CssClass="Icon Add" runat="server" Text="Add Document" OnClick="lnkbtnAddDocument_Click" />
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_Documents" runat="server" OnClick="lnkbtnClearFilters_Documents_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" />
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>
                                <telerik:RadGrid ID="rgDocuments" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    AllowMultiRowSelection="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="false"
                                    OnNeedDataSource="rgDocuments_NeedDataSource"
                                    OnItemDataBound="rgDocuments_OnItemDataBound"
                                    AllowPaging="True" AllowSorting="True"
                                    Style="border: 1px solid #D6712D;"
                                    Width="99.8%" PageSize="10"
                                    GridLines="None" SkinID="Default">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>
                                            <telerik:GridTemplateColumn AllowFiltering="false" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="50px">
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="imgbtnOpenDocument" runat="server" ImageUrl="~/images/icons/page_white_acrobat.png"
                                                        AlternateText="Open Document" ToolTip="View Document"
                                                        CommandName="OpenDocument"
                                                        CommandArgument='<%# Eval("DocumentGUID") %>' />
                                                </ItemTemplate>
                                                <ItemStyle Width="50px" />
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn DataField="OrderID" AllowFiltering="true" HeaderText="Order #" CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true"
                                                HeaderStyle-Width="150px" FilterControlWidth="100" SortExpression="OrderID">
                                                <ItemTemplate>
                                                    <a href='<%# String.Format("../../CustomerService/ReviewOrder.aspx?ID={0}", Eval("OrderID")) %>' target="_top" title="Review Order Details"><%# Eval("OrderID") %></a>
                                                </ItemTemplate>
                                                <ItemStyle Width="150px" />
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridBoundColumn DataField="ReferenceKey" UniqueName="ReferenceKey" Visible="false" SortExpression="ReferenceKey" AllowSorting="true" />
                                            <telerik:GridBoundColumn UniqueName="DocumentCategory" DataField="DocumentCategory" HeaderText="Category" SortExpression="DocumentCategory" HeaderStyle-Width="200px" AllowSorting="true">
                                                <FilterTemplate>
                                                    <telerik:RadComboBox ID="rcbDocumentCategory" DataSourceID="sqlDocumentCategory_Documents" DataTextField="DocumentCategory"
                                                        DataValueField="DocumentCategory" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("DocumentCategory").CurrentFilterValue %>'
                                                        runat="server" OnClientSelectedIndexChanged="DocumentTypeIndexChanged" Width="175px">
                                                        <Items>
                                                            <telerik:RadComboBoxItem Text="All" />
                                                        </Items>
                                                    </telerik:RadComboBox>
                                                    <telerik:RadScriptBlock ID="RadScriptBlock15" runat="server">
                                                        <script type="text/javascript">
                                                            function DocumentTypeIndexChanged(sender, args) {
                                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                tableView.filter("DocumentCategory", args.get_item().get_value(), "EqualTo");
                                                            }
                                                        </script>
                                                    </telerik:RadScriptBlock>
                                                </FilterTemplate>
                                                <ItemStyle Width="200px" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="ReturnID" UniqueName="ReturnID" Visible="false" SortExpression="ReturnID" AllowSorting="true" />
                                            <telerik:GridBoundColumn DataField="DocumentDescription" HeaderText="Description" SortExpression="DocumentDescription" AllowSorting="true"
                                                UniqueName="DocumentDescription" HeaderStyle-Width="250px" FilterControlWidth="200px" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true">
                                                <ItemStyle Width="250px" Wrap="true" />
                                            </telerik:GridBoundColumn>
                                            <telerik:GridDateTimeColumn DataField="CreatedDate" HeaderText="Created On" UniqueName="CreatedDate" DataFormatString="{0:d}"
                                                AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="175px" FilterControlWidth="125px" SortExpression="CreatedDate">
                                                <ItemStyle Width="175px" />
                                            </telerik:GridDateTimeColumn>
                                            <telerik:GridBoundColumn DataField="CreatedBy" UniqueName="CreatedBy" HeaderText="Created By" SortExpression="CreatedBy" AllowSorting="true"
                                                HeaderStyle-Width="150px" FilterControlWidth="100px" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" AllowFiltering="true">
                                                <ItemStyle Width="150px" />
                                            </telerik:GridBoundColumn>
                                            <%--FEATURE ADDED 01/15/2016 - DELETED DOCUMENT--%>
                                            <telerik:GridTemplateColumn>
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="lnkbtnDeleteDocument" runat="server" OnClick="lnkbtnDeleteDocument_Click" CommandArgument='<%# Eval("DocumentID") %>' CausesValidation="False"
                                                        CommandName="DeleteDocument" Text="Delete" OnClientClick="return confirm('This action will delete this document. Are you sure you want to continue?');" />
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <%--END--%>
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
                <telerik:RadPageView ID="Calendars_Tab" runat="server">
                    <%--CALENDARS--%>
                    <div id="Calendar_Tab">
                        <asp:Panel ID="pnlCalendars" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li>
                                            <%--<a href="#" id="hlAddCalendar" class="Icon Add">Add Calendar</a>--%>
                                            <asp:HyperLink ID="hlAddCalendar" ClientIDMode="Static" runat="server" NavigateUrl="#" Text="Add Instadose+ Calendar" CssClass="Icon Add" />
                                        </li>
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_Calendars" runat="server" OnClick="lnkbtnClearFilters_Calendars_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>

                                <telerik:RadGrid ID="rgCalendars" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    AllowMultiRowSelection="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="false"
                                    OnNeedDataSource="rgCalendars_NeedDataSource"
                                    OnItemDataBound="rgCalendars_ItemDataBound"
                                    Width="99.9%" PageSize="10"
                                    GridLines="None" SkinID="Default"
                                    AllowPaging="true" AllowSorting="true">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="ID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>

                                            <telerik:GridTemplateColumn DataField="Name" UniqueName="Name" HeaderText="Name" HeaderStyle-Width="350px" FilterControlWidth="335px"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" SortExpression="Name">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="hlOpenCalendarDetail" runat="server" ToolTip="Review Calendar Details"></asp:HyperLink>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>

                                            <telerik:GridBoundColumn DataField="Description" HeaderText="Description" UniqueName="Description" AllowSorting="false"
                                                HeaderStyle-Width="350px" AllowFiltering="false" />

                                            <telerik:GridTemplateColumn DataField="Frequency" UniqueName="Frequency" HeaderText="Frequency" HeaderStyle-Width="100px" SortExpression="Frequency" AllowFiltering="false">
                                                <ItemTemplate>
                                                    <asp:Literal ID="ltCalendarFrequency" runat="server" />
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>

                                            <telerik:GridBoundColumn DataField="LocationCount" HeaderText="Location #" UniqueName="LocationCount" HeaderStyle-Width="75px" AllowFiltering="false" AllowSorting="false" />
                                        </Columns>
                                    </MasterTableView>
                                    <ClientSettings AllowColumnsReorder="false">
                                        <Selecting AllowRowSelect="false"></Selecting>
                                    </ClientSettings>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                </telerik:RadPageView>
                <telerik:RadPageView ID="BillingGroups_Tab" runat="server">
                    <%--ORDERS--%>
                    <div id="BillingGroup_Tab">
                        <asp:Panel ID="pnlBillingGroups" runat="server" SkinID="Default">
                            <div style="width: 100%;">
                                <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                                    <ul>
                                        <li>
                                            <asp:LinkButton ID="lnkbtnCreateBillingGroup" runat="server" Font-Bold="true" ForeColor="#FFFFFF"
                                                CommandName="CreateBillingGroup" CommandArgument='<%= Request.QueryString["ID"] %>' ToolTip="Create Invoice Delivery Method"
                                                CssClass="Icon Add" OnClick="lnkbtnCreateBillingGroup_Click">Create Invoice Delivery Method</asp:LinkButton>
                                        </li>                                        
                                        <li class="RightAlign">
                                            <asp:LinkButton ID="lnkbtnClearFilters_BillingGroups" runat="server" OnClick="lnkbtnClearFilters_BillingGroups_Click"
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                        </li>
                                        <li class="Clear"></li>
                                    </ul>
                                </div>                                

                                <telerik:RadGrid ID="rgBillingGroups" runat="server"
                                    CssClass="OTable"
                                    AutoGenerateColumns="false"
                                    AllowFilteringByColumn="true"
                                    AllowMultiRowSelection="true"
                                    EnableLinqExpressions="false"
                                    EnableViewState="false"
                                    OnNeedDataSource="rgBillingGroups_NeedDataSource"
                                    Width="99.9%" PageSize="10"
                                    GridLines="None" SkinID="Default"
                                    AllowPaging="true" AllowSorting="true">
                                    <PagerStyle Mode="NumericPages" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <ClientSettings EnableRowHoverStyle="true">
                                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                        <Selecting AllowRowSelect="true" />
                                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                    </ClientSettings>
                                    <FilterMenu OnClientShown="MenuShowing" />
                                    <GroupingSettings CaseSensitive="false" />
                                    <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" AllowMultiColumnSorting="false">
                                        <Columns>
                                            <telerik:GridTemplateColumn DataField="BillingGroupID" UniqueName="BillingGroupID" HeaderText="Billing Group" FilterControlWidth="60px" ItemStyle-Wrap="false"
                                                CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" SortExpression="BillingGroupID" HeaderStyle-Width="100px">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="hyprlnkBillingGroupID" runat="server" NavigateUrl='<%# String.Format("EditBillingGroup.aspx?AccountID={0}&BillingGroupID={1}", Eval("AccountID"), Eval("BillingGroupID")) %>'
                                                        Text='<%# Eval("BillingGroupID")%>' Target="_top" ToolTip="Manage Billing Group"></asp:HyperLink>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>

                                            <telerik:GridTemplateColumn DataField="IsAccountLevelBillingGroup" HeaderText="Is Default" UniqueName="IsAccountLevelBillingGroup"
                                                HeaderStyle-Width="100px" FilterControlWidth="100px" AllowFiltering="true" SortExpression="IsAccountLevelBillingGroup">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblIsAccountLevel_BillingGroup" runat="server" Text='<%# Convert.ToBoolean(Eval("IsAccountLevelBillingGroup")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>

                                            <telerik:GridBoundColumn DataField="ContactName" HeaderText="Contact Name" SortExpression="ContactName" AllowFiltering="true" AllowSorting="true"
                                                UniqueName="ContactName" FilterControlWidth="125px" ItemStyle-Wrap="false" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" />

                                            <telerik:GridBoundColumn DataField="CompanyName" HeaderText="Company Name" SortExpression="CompanyName" AllowFiltering="true" AllowSorting="true"
                                                UniqueName="CompanyName" FilterControlWidth="125px" ItemStyle-Wrap="false" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" />

                                            <telerik:GridTemplateColumn DataField="useMail" HeaderText="By Mail" UniqueName="useMail"
                                                HeaderStyle-Width="60px" FilterControlWidth="60px" AllowFiltering="true" SortExpression="useMail">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblUseMailStatus_BillingGroup" runat="server" Text='<%# Convert.ToBoolean(Eval("useMail")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>                                            
                                            <telerik:GridTemplateColumn DataField="useEmail" HeaderText="By Email" UniqueName="useEmail"
                                                HeaderStyle-Width="70px" FilterControlWidth="70px" AllowFiltering="true" SortExpression="useEmail">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblUseEmailStatus_BillingGroup" runat="server" Text='<%# Convert.ToBoolean(Eval("useEmail")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn DataField="useFax" HeaderText="By Fax" UniqueName="useFax"
                                                HeaderStyle-Width="60px" FilterControlWidth="60px" AllowFiltering="true" SortExpression="useFax">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblUseFaxStatus_BillingGroup" runat="server" Text='<%# Convert.ToBoolean(Eval("useFax")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn DataField="useEDI" HeaderText="By EDI" UniqueName="useEDI"
                                                HeaderStyle-Width="60px" FilterControlWidth="60px" AllowFiltering="true" SortExpression="useEDI">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblUseEDIStatus_BillingGroup" runat="server" Text='<%# Convert.ToBoolean(Eval("useEDI")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn DataField="useSpecialDelivery" HeaderText="By SpecialDelivery" UniqueName="useSpecialDelivery"
                                                HeaderStyle-Width="130px" FilterControlWidth="130px" AllowFiltering="true" SortExpression="useSpecialDelivery">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblUseSpecialDeliveryStatus_BillingGroup" runat="server" Text='<%# Convert.ToBoolean(Eval("useSpecialDelivery")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>                                            
                                            <telerik:GridTemplateColumn DataField="DeliverInvoice" HeaderText="Is Delivery" UniqueName="DeliverInvoice"
                                                HeaderStyle-Width="90px" FilterControlWidth="90px" AllowFiltering="true" SortExpression="DeliverInvoice">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblDeliverInvoiceStatus_BillingGroup" runat="server" Text='<%# Convert.ToBoolean(Eval("DeliverInvoice")) == true ? "Yes" : "No" %>'></asp:Label>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                        </Columns>
                                    </MasterTableView>
                                    <ClientSettings AllowColumnsReorder="false">
                                        <Selecting AllowRowSelect="false"></Selecting>
                                    </ClientSettings>
                                </telerik:RadGrid>
                            </div>
                        </asp:Panel>
                    </div>
                    <%--END--%>
                </telerik:RadPageView>
            </telerik:RadMultiPage>
        </div>
        <asp:HiddenField ID="hfUsername" runat="server" ClientIDMode="Static" />
    </div>
    <%------------------------------- END :: START MAIN PAGE CONTENT OVER HERE -------------------------------%>

    <%--BEGIN :: SQL DATASOURCES--%>
    <%--CS REQUESTS FILTERS--%>
    <asp:SqlDataSource ID="sqlStatus_CSRequests" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT CaseStatusDesc FROM CaseStatus"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlType_CSRequests" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT RequestDesc FROM Requests"></asp:SqlDataSource>
    <%--END--%>

    <%--ORDERS FILTERS--%>
    <asp:SqlDataSource ID="sqlType_Orders" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT OrderType FROM Orders"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlStatus_Orders" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT O.OrderStatusID, OS.OrderStatusName AS [Status] FROM Orders AS O INNER JOIN OrderStatus AS OS ON O.OrderStatusID = OS.OrderStatusID"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlPaymentMethod_Orders" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT CASE CONVERT(NVARCHAR, PaymentMethodID)
				   WHEN '1' THEN 'CC'
				   WHEN '2' THEN 'PO'
				   ELSE 'PO' END AS [PaymentMethod]
				   FROM PaymentMethods"></asp:SqlDataSource>
    <%--END--%>

    <%--LOCATIONS FILTERS--%>
    <asp:SqlDataSource ID="sqlState_Locations" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT S.StateAbbrev 
				   FROM States AS S 
				   INNER JOIN Locations AS L
				   ON S.StateID = L.ShippingStateID
				   WHERE L.AccountID = @AccountID">
        <SelectParameters>
            <asp:QueryStringParameter QueryStringField="ID" Name="AccountID" DefaultValue="0" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    <%--END--%>

    <%--USERS FILTERS--%>
    <asp:SqlDataSource ID="sqlUserRoleName_Users" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT UserRoleName FROM UserRoles WHERE Active = 1"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlLocationName_Users" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT LocationName FROM Locations WHERE AccountID = @AccountID">
        <SelectParameters>
            <asp:QueryStringParameter QueryStringField="ID" Name="AccountID" DefaultValue="0" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    <%--END--%>

    <%--BADGES FILTERS--%>
    <asp:SqlDataSource ID="sqlBodyRegion_Badges" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT BodyRegionName AS [BodyRegion] FROM BodyRegions"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlProductColor_Badges" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT Color AS [ProductColor] FROM Products WHERE Active = 1"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlLocation_Badges" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT LocationName FROM Locations WHERE AccountID = @AccountID">
        <SelectParameters>
            <asp:QueryStringParameter QueryStringField="ID" Name="AccountID" DefaultValue="0" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    <%--END--%>

    <%--RETURNS FILTERS--%>
    <asp:SqlDataSource ID="sqlType_Returns" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.AppConnectionString %>"
        SelectCommand="SELECT DISTINCT Type FROM rma_ref_ReturnTypes WHERE Active = 1 ORDER BY Type ASC"></asp:SqlDataSource>

    <%--DOCUMENTS CATEGORY DROPDOWNLIST (ADD DOCUMENT MODAL/DIALOG)--%>
    <asp:SqlDataSource ID="sqlDocumentCategoriesDDL" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT DocumentCategory FROM DocumentCategories ORDER BY DocumentCategory ASC"></asp:SqlDataSource>
    <%--END--%>

    <%--DOCUMENTS FILTERS--%>
    <asp:SqlDataSource ID="sqlDocumentCategory_Documents" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT DISTINCT DocumentCategory FROM Documents WHERE DocumentCategory NOT IN ('Order Doc') ORDER BY DocumentCategory ASC"></asp:SqlDataSource>
    <%--END--%>

    <%--ORDERID DDL--%>
    <asp:SqlDataSource ID="SQLDSOrderNumbers" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
        SelectCommand="SELECT OrderID FROM Orders WHERE AccountID = @AccountID ORDER BY OrderID ASC">
        <SelectParameters>
            <asp:QueryStringParameter QueryStringField="ID" Name="AccountID" DefaultValue="0" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    <%--END--%>
    <%--END :: SQL DATASOURCES--%>
</asp:Content>
