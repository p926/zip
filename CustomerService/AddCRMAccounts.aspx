<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_AddCRMAccounts" Codebehind="AddCRMAccounts.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        /* CSS for Instadose 1 & Instadose 2 Product Colors. */
        .productColor
        {
            background-color: #FFFFFF;
            color: #000000;
            padding: 7px 25px;
            text-align: center;
            text-shadow: 1px 1px 0px #333333;
            display: block;
            width: 100px;
        }

        .productColor.Blue
        {
            background-color: #357195;
            color: #FFFFFF;
        }

        .productColor.Green
        {
            background-color: #196445;
            color: #FFFFFF;
        }

        .productColor.Black
        {
            background-color: #000000;
            color: #FFFFFF;
        }

        .productColor.Pink
        {
            background-color: #dd82b2;
            text-shadow: 1px 1px 0px #FFFFFF;
            color: #000000;
        }

        .productColor.Silver
        {
            background-color: #C1C1C3;
            text-shadow: 1px 1px 0px #FFFFFF;
            color: #000000;
        }

        .productColor.Red
        {
            background-color: #C41230;
            text-shadow: 1px 1px 0px #FFFFFF;
            color: #FFFFFF;
        }

        .productColor.Orange
        {
            background-color: #F68933;
            text-shadow: 1px 1px 0px #FFFFFF;
            color: #FFFFFF;
        }

        .productColor.Gray
        {
            background-color: #3E3E3F;
            text-shadow: 1px 1px 0px #FFFFFF;
            color: #FFFFFF;
        }

        /* CSS for Username Validation Icons. */
        .validator {
            width:16px;
            height: 16px;
            display: inline-block;
            vertical-align: middle;
            margin-left: 5px;
        }

        .valid {
            background: url('/images/Success.png');
        }

        .invalid {
            background: url('/images/Fail.png');
        }
    </style>
    <script type="text/javascript">
        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ajaxLoad);
            ajaxLoad();
        });
        
        function ajaxLoad() {
            // Service Start Date.
            $('#<%= txtServiceStartDate.ClientID %>').datepicker();

            // Service End Date.
            $('#<%= txtServiceEndDate.ClientID %>').datepicker();

            // Accordion for Main Form.
            $("#accordionMainForm").accordion({
                header: "h3",
                autoHeight: false
            });

            // Accordion Indexes for Billing & Shipping Modal/Dialogs.
            var addIndex = parseInt($('#<%= hdnfldAddAccordionIndex.ClientID %>').val());
            var editIndex = parseInt($('#<%= hdnfldEditAccordionIndex.ClientID %>').val());

            // Accordion for Add Location Information.
            $("#addLocationAccordion").accordion({
                header: "h3",
                autoHeight: false,
                active: addIndex,
                change: function (event, ui) {
                    var index = $(this).accordion("option", "active");
                    $('#<%= hdnfldAddAccordionIndex.ClientID %>').val(index);
                }
            });

            // Accordion for Edit Location Information.
            $("#editLocationAccordion").accordion({
                header: "h3",
                autoHeight: false,
                active: editIndex,
                change: function (event, ui) {
                    var index = $(this).accordion("option", "active");
                    $('#<%= hdnfldEditAccordionIndex.ClientID %>').val(index);
                }
            });

            // Modal/Dialog for Adding Location Information.
            $('#divAddLocationInformationForm').dialog({
                modal: true,
                autoOpen: false,
                width: 700,
                resizable: false,
                title: "Add Location Information",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('#<%= txtAddBillingCompanyName.ClientID %>').focus();
                },
                buttons: {
                    "Add": function () {
                        $('#<%= btnAddLocation.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancelAddLocation.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });

            // Modal/Dialog for Editing Location Information.
            $('#divEditLocationInformationForm').dialog({
                modal: true,
                autoOpen: false,
                width: 700,
                resizable: false,
                title: "Edit Location Information",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('#<%= txtEditBillingCompanyName.ClientID %>').focus();
                },
                buttons: {
                    "Update": function () {
                        $('#<%= btnEditLocation.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancelEditLocation.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });

            // Modal/Dialog for Adding Order Information.
            $('#divAddOrderInformationForm').dialog({
                modal: true,
                autoOpen: false,
                width: 700,
                resizable: false,
                title: "Add Order Information",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Add": function () {
                        $('#<%= btnAddOrder.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancelAddOrder.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                    $('#<%= ddlAddProductGroupID.ClientID %>').focus();
                }
            });

            // Modal/Dialog for Editing Order Information.
            $('#divEditOrderInformationForm').dialog({
                modal: true,
                autoOpen: false,
                width: 700,
                resizable: false,
                title: "Edit Order Information",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-overlay').fadeOut();
                    $('#<%= ddlEditProductGroupID.ClientID %>').focus();
                },
                buttons: {
                    "Update": function () {
                        $('#<%= btnEditOrder.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancelEditOrder.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });
        }

        // Open jQuery Modal/Dialog.
        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        }

        // Close jQuery Modal/Dialog.
        function closeDialog(id) {
            $('#' + id).dialog("close");
        }

        // Reset Modal/Dialog Form Validators.
        function resetValidators() {
            if (typeof (Page_Validators) != "undefined") {
                for (var i = 0; i < Page_Validators.length; i++) {
                    var validator = Page_Validators[i];
                    validator.isvalid = true;
                    ValidatorUpdateDisplay(validator);
                }
            }
        }

        // Remove Array item/object.
        function removeByIndex(arrayName, arrayIndex) {
            arrayName.splice(arrayIndex, 1);
        }

        //============================== ADD/EDIT BILLING & SHIPPING LOCATION MODAL/DIALOGS ==============================//
        //================================================================================================================//
        var addLocation = {};
        var addLocationArray = [];
        var locationJSON = "";
        var i = 0;  // Displayed index of the JavaScript table.
        var editLocationIndex = 0;
        var myLocationTable;

        function addLocationRow() {
            resetValidators();

            // Get the values from each ASP.NET form control.
            var index = i;
            // Billing Location Information.
            var billingCompanyName = document.getElementById('<%= txtAddBillingCompanyName.ClientID%>').value;
            var abp = document.getElementById('<%= ddlAddBillingPrefix.ClientID%>');
            var billingPrefix = abp.options[abp.selectedIndex].value;
            var billingFirstName = document.getElementById('<%= txtAddBillingFirstName.ClientID%>').value;
            var billingLastName = document.getElementById('<%= txtAddBillingLastName.ClientID%>').value;
            var billingAddressLine1 = document.getElementById('<%= txtAddBillingAddressLine1.ClientID%>').value;
            var billingAddressLine2 = document.getElementById('<%= txtAddBillingAddressLine2.ClientID%>').value;
            var billingAddressLine3 = document.getElementById('<%= txtAddBillingAddressLine3.ClientID%>').value;
            var billingCity = document.getElementById('<%= txtAddBillingCity.ClientID%>').value;
            var abs = document.getElementById('<%= ddlAddBillingState.ClientID%>');
            var billingState = abs.options[abs.selectedIndex].value;
            var billingStateText = abs.options[abs.selectedIndex].text;     // Gets SelectedText value of DDL.
            var billingPostalCode = document.getElementById('<%= txtAddBillingPostalCode.ClientID%>').value;
            var abc = document.getElementById('<%= ddlAddBillingCountry.ClientID%>');
            var billingCountry = abc.options[abc.selectedIndex].value;
            var billingCountryText = abc.options[abc.selectedIndex].text;   // Gets SelectedText value of DDL.
            var billingTelephone = document.getElementById('<%= txtAddBillingTelephone.ClientID%>').value;
            var billingFax = document.getElementById('<%= txtAddBillingFax.ClientID%>').value;
            var billingEmailAddress = document.getElementById('<%= txtAddBillingEmailAddress.ClientID%>').value;

            // Shipping Location Information.
            var shippingCompanyName = document.getElementById('<%= txtAddShippingCompanyName.ClientID%>').value;
            var asp = document.getElementById('<%= ddlAddShippingPrefix.ClientID%>');
            var shippingPrefix = asp.options[asp.selectedIndex].value;
            var shippingFirstName = document.getElementById('<%= txtAddShippingFirstName.ClientID%>').value;
            var shippingLastName = document.getElementById('<%= txtAddShippingLastName.ClientID%>').value;
            var shippingAddressLine1 = document.getElementById('<%= txtAddShippingAddressLine1.ClientID%>').value;
            var shippingAddressLine2 = document.getElementById('<%= txtAddShippingAddressLine2.ClientID%>').value;
            var shippingAddressLine3 = document.getElementById('<%= txtAddShippingAddressLine3.ClientID%>').value;
            var shippingCity = document.getElementById('<%= txtAddShippingCity.ClientID%>').value;
            var ass = document.getElementById('<%= ddlAddShippingState.ClientID%>');
            var shippingState = ass.options[ass.selectedIndex].value;
            var shippingStateText = ass.options[ass.selectedIndex].text;    // Gets SelectedText value of DDL.
            var shippingPostalCode = document.getElementById('<%= txtAddShippingPostalCode.ClientID%>').value;
            var asc = document.getElementById('<%= ddlAddShippingCountry.ClientID%>');
            var shippingCountry = asc.options[asc.selectedIndex].value;
            var shippingCountryText = asc.options[asc.selectedIndex].text;  // Gets SelectedText value of DDL.
            var shippingTelephone = document.getElementById('<%= txtAddShippingTelephone.ClientID%>').value;
            var shippingFax = document.getElementById('<%= txtAddShippingFax.ClientID%>').value;
            var shippingEmailAddress = document.getElementById('<%= txtAddShippingEmailAddress.ClientID%>').value;

            if (Page_ClientValidate("ADDLOCATION")) {
                // Location Information Object.
                addLocation = {
                    index: index,
                    // Billing Location Information.
                    billingcompanyname: billingCompanyName,
                    billingprefix: billingPrefix,
                    billingfirstname: billingFirstName,
                    billinglastname: billingLastName,
                    billingaddressline1: billingAddressLine1,
                    billingaddressline2: billingAddressLine2,
                    billingaddressline3: billingAddressLine3,
                    billingcity: billingCity,
                    billingstate: billingState,
                    billingpostalcode: billingPostalCode,
                    billingcountry: billingCountry,
                    billingtelephone: billingTelephone,
                    billingfax: billingFax,
                    billingemailaddress: billingEmailAddress,
                    // Shipping Location Information.
                    shippingcompanyname: shippingCompanyName,
                    shippingprefix: shippingPrefix,
                    shippingfirstname: shippingFirstName,
                    shippinglastname: shippingLastName,
                    shippingaddressline1: shippingAddressLine1,
                    shippingaddressline2: shippingAddressLine2,
                    shippingaddressline3: shippingAddressLine3,
                    shippingcity: shippingCity,
                    shippingstate: shippingState,
                    shippingpostalcode: shippingPostalCode,
                    shippingcountry: shippingCountry,
                    shippingtelephone: shippingTelephone,
                    shippingfax: shippingFax,
                    shippingemailaddress: shippingEmailAddress
                };

                myLocationTable = document.getElementById("locationsDataTable");
                var rowCount = myLocationTable.rows.length;
                var row = myLocationTable.insertRow(rowCount);

                // Alternate InnerHTML for optional fields.
                var baLine2, baLine3, bFax;
                var saLine2, saLine3, sFax, sEmail;

                if (billingAddressLine2 !== "") {
                    baLine2 = billingAddressLine2 + "<br />";
                }
                else {
                    baLine2 = "";
                }

                if (billingAddressLine3 !== "") {
                    baLine3 = billingAddressLine3 + "<br />";
                }
                else {
                    baLine3 = "";
                }

                if (billingFax !== "") {
                    bFax = "<u>Fax</u>: " + billingFax + "<br />";
                }
                else {
                    bFax = "";
                }

                if (shippingAddressLine2 !== "") {
                    saLine2 = shippingAddressLine2 + "<br />";
                }
                else {
                    saLine2 = "";
                }

                if (shippingAddressLine3 !== "") {
                    saLine3 = shippingAddressLine3 + "<br />";
                }
                else {
                    saLine3 = "";
                }

                if (shippingFax !== "") {
                    sFax = "<u>Fax</u>: " + shippingFax + "<br />";
                }
                else {
                    sFax = "";
                }

                if (shippingEmailAddress !== "") {
                    sEmail = "<u>E-mail</u>: " + shippingEmailAddress + "<br />";
                }
                else {
                    sEmail = "";
                }

                // Only for display (acutal record index is i).
                var x = i + 1;

                // Formatted InnerHTML for dynamically generated table.
                row.style.verticalAlign = "top";
                //row.insertCell(0).innerHTML = x;
                // Add Billing Information.
                row.insertCell(0).innerHTML = billingCompanyName + "<br />" +
                                              "<u>Contact</u>: " + billingPrefix + " " + billingFirstName + " " + billingLastName + "<br />" +
                                              billingAddressLine1 + "<br />" +
                                              baLine2 +
                                              baLine3 +
                                              billingCity + " " + billingStateText + " " + billingPostalCode + "<br />" +
                                              billingCountryText + "<br />" +
                                              "<u>Phone</u>: " + billingTelephone + "<br />" +
                                              bFax +
                                              "<u>E-mail</u>: " + billingEmailAddress + "<br /><br />";
                // Add Shipping Information.
                row.insertCell(1).innerHTML = shippingCompanyName + "<br />" +
                                              "<u>Contact</u>: " + shippingPrefix + " " + shippingFirstName + " " + shippingLastName + "<br />" +
                                              shippingAddressLine1 + "<br />" +
                                              saLine2 +
                                              saLine3 +
                                              shippingCity + " " + shippingStateText + " " + shippingPostalCode + "<br />" +
                                              shippingCountryText + "<br />" +
                                              "<u>Phone</u>: " + shippingTelephone + "<br />" +
                                              sFax +
                                              sEmail + "<br /><br />";
                // Delete|Edit Buttons.
                row.insertCell(2).innerHTML = '<input type="button" class="Icon Remove" value="Delete" onClick="Javacsript:deleteLocationRow(this)">&nbsp;<input type="button" class="Icon Edit" value="Edit" onClick="Javacsript:editLocationRow(this)">';

                addLocationArray.push(addLocation);

                locationJSON = JSON.stringify({ addLocationArray: addLocationArray });

                document.getElementById('<%= hdnfldLocationJSONString.ClientID%>').value = locationJSON;

                // Increment index.
                i++;

                // Clear all Add Location Form controls.
                clearAddLocationFormControls();

                // Close jQuery Modal/Dialog.
                closeDialog("divAddLocationInformationForm");
            }
        }

        // Delete a given record/row (not yet in database).
        function deleteLocationRow(obj) {
            var index = (obj.parentNode.parentNode.rowIndex);
            myLocationTable = document.getElementById("locationsDataTable");
            myLocationTable.deleteRow(index);

            // Gets acutal index of Location object in JSON Array.
            var x = parseInt(index) - 1;

            removeByIndex(addLocationArray, (x));
            locationJSON = null;
            locationJSON = JSON.stringify({ addLocationArray: addLocationArray });
            document.getElementById('<%= hdnfldLocationJSONString.ClientID%>').value = "";
            document.getElementById('<%= hdnfldLocationJSONString.ClientID%>').value = locationJSON;
        }

        function editLocationRow(obj) {
            resetValidators();

            var index = (obj.parentNode.parentNode.rowIndex);

            // Get index of record (subtract 1 to the value since the table actually has 0 has the Header).
            editLocationIndex = index - 1;

            // Assign editIndex to HiddenField in Modal/Dialog.
            document.getElementById('<%= hdnfldEditLocationIndex.ClientID%>').value = editLocationIndex.toString();

            // Edit Billing Location Information.
            document.getElementById('<%= txtEditBillingCompanyName.ClientID%>').value = addLocationArray[editLocationIndex].billingcompanyname;
            document.getElementById('<%= ddlEditBillingPrefix.ClientID%>').value = addLocationArray[editLocationIndex].billingprefix;
            document.getElementById('<%= txtEditBillingFirstName.ClientID%>').value = addLocationArray[editLocationIndex].billingfirstname;
            document.getElementById('<%= txtEditBillingLastName.ClientID%>').value = addLocationArray[editLocationIndex].billinglastname;
            document.getElementById('<%= txtEditBillingAddressLine1.ClientID%>').value = addLocationArray[editLocationIndex].billingaddressline1;
            document.getElementById('<%= txtEditBillingAddressLine2.ClientID%>').value = addLocationArray[editLocationIndex].billingaddressline2;
            document.getElementById('<%= txtEditBillingAddressLine3.ClientID%>').value = addLocationArray[editLocationIndex].billingaddressline3;
            document.getElementById('<%= txtEditBillingCity.ClientID%>').value = addLocationArray[editLocationIndex].billingcity;
            document.getElementById('<%= txtEditBillingPostalCode.ClientID%>').value = addLocationArray[editLocationIndex].billingpostalcode;
            document.getElementById('<%= ddlEditBillingCountry.ClientID%>').value = addLocationArray[editLocationIndex].billingcountry;
            document.getElementById('<%= ddlEditBillingState.ClientID%>').value = addLocationArray[editLocationIndex].billingstate;
            document.getElementById('<%= txtEditBillingTelephone.ClientID%>').value = addLocationArray[editLocationIndex].billingtelephone;
            document.getElementById('<%= txtEditBillingFax.ClientID%>').value = addLocationArray[editLocationIndex].billingfax;
            document.getElementById('<%= txtEditBillingEmailAddress.ClientID%>').value = addLocationArray[editLocationIndex].billingemailaddress;

            // Edit Shipping Location Information.
            document.getElementById('<%= txtEditShippingCompanyName.ClientID%>').value = addLocationArray[editLocationIndex].shippingcompanyname;
            document.getElementById('<%= ddlEditShippingPrefix.ClientID%>').value = addLocationArray[editLocationIndex].shippingprefix;
            document.getElementById('<%= txtEditShippingFirstName.ClientID%>').value = addLocationArray[editLocationIndex].shippingfirstname;
            document.getElementById('<%= txtEditShippingLastName.ClientID%>').value = addLocationArray[editLocationIndex].shippinglastname;
            document.getElementById('<%= txtEditShippingAddressLine1.ClientID%>').value = addLocationArray[editLocationIndex].shippingaddressline1;
            document.getElementById('<%= txtEditShippingAddressLine2.ClientID%>').value = addLocationArray[editLocationIndex].shippingaddressline2;
            document.getElementById('<%= txtEditShippingAddressLine3.ClientID%>').value = addLocationArray[editLocationIndex].shippingaddressline3;
            document.getElementById('<%= txtEditShippingCity.ClientID%>').value = addLocationArray[editLocationIndex].shippingcity;
            document.getElementById('<%= ddlEditShippingState.ClientID%>').value = addLocationArray[editLocationIndex].shippingstate;
            document.getElementById('<%= txtEditShippingPostalCode.ClientID%>').value = addLocationArray[editLocationIndex].shippingpostalcode;
            document.getElementById('<%= ddlEditShippingCountry.ClientID%>').value = addLocationArray[editLocationIndex].shippingcountry;
            document.getElementById('<%= txtEditShippingTelephone.ClientID%>').value = addLocationArray[editLocationIndex].shippingtelephone;
            document.getElementById('<%= txtEditShippingFax.ClientID%>').value = addLocationArray[editLocationIndex].shippingfax;
            document.getElementById('<%= txtEditShippingEmailAddress.ClientID%>').value = addLocationArray[editLocationIndex].shippingemailaddress;

            // Open Edit Location Information Form Modal/Dialog.
            openDialog("divEditLocationInformationForm");
        }

        function updateLocationRow() {
            // Update JavaScript Table display.
            editLocationIndex = parseInt(document.getElementById('<%= hdnfldEditLocationIndex.ClientID%>').value);

            // Update each item in the Array (based on index).
            // Updated Billing Location Information.
            var updatedBillingCompanyName = document.getElementById('<%= txtEditBillingCompanyName.ClientID%>').value;
            addLocationArray[editLocationIndex].billingcompanyname = updatedBillingCompanyName;
            var ebp = document.getElementById('<%= ddlEditBillingPrefix.ClientID%>');
            var updatedBillingPrefix = ebp.options[ebp.selectedIndex].value;
            addLocationArray[editLocationIndex].billingprefix = updatedBillingPrefix;
            var updatedBillingFirstName = document.getElementById('<%= txtEditBillingFirstName.ClientID%>').value;
            addLocationArray[editLocationIndex].billingfirstname = updatedBillingFirstName
            var updatedBillingLastName = document.getElementById('<%= txtEditBillingLastName.ClientID%>').value;
            addLocationArray[editLocationIndex].billinglastname = updatedBillingLastName;
            var updatedBillingAddressLine1 = document.getElementById('<%= txtEditBillingAddressLine1.ClientID%>').value;
            addLocationArray[editLocationIndex].billingaddressline1 = updatedBillingAddressLine1;
            var updatedBillingAddressLine2 = document.getElementById('<%= txtEditBillingAddressLine2.ClientID%>').value;
            addLocationArray[editLocationIndex].billingaddressline2 = updatedBillingAddressLine2;
            var updatedBillingAddressLine3 = document.getElementById('<%= txtEditBillingAddressLine3.ClientID%>').value;
            addLocationArray[editLocationIndex].billingaddressline3 = updatedBillingAddressLine3;
            var updatedBillingCity = document.getElementById('<%= txtEditBillingCity.ClientID%>').value;
            addLocationArray[editLocationIndex].billingcity = updatedBillingCity;
            var ebs = document.getElementById('<%= ddlEditBillingState.ClientID%>');
            var updatedBillingState = ebs.options[ebs.selectedIndex].value;
            addLocationArray[editLocationIndex].billingstate = updatedBillingState;
            var updatedBillingStateText = ebs.options[ebs.selectedIndex].text;     // Gets SelectedText value of DDL.
            var updatedBillingPostalCode = document.getElementById('<%= txtEditBillingPostalCode.ClientID%>').value;
            addLocationArray[editLocationIndex].billingpostalcode = updatedBillingPostalCode;
            var ebc = document.getElementById('<%= ddlEditBillingCountry.ClientID%>');
            var updatedBillingCountry = ebc.options[ebc.selectedIndex].value;
            addLocationArray[editLocationIndex].billingcountry = updatedBillingCountry;
            var updatedBillingCountryText = ebc.options[ebc.selectedIndex].text;   // Gets SelectedText value of DDL.
            var updatedBillingTelephone = document.getElementById('<%= txtEditBillingTelephone.ClientID%>').value;
            addLocationArray[editLocationIndex].billingtelephone = updatedBillingTelephone;
            var updatedBillingFax = document.getElementById('<%= txtEditBillingFax.ClientID%>').value;
            addLocationArray[editLocationIndex].billingfax = updatedBillingFax;
            var updatedBillingEmailAddress = document.getElementById('<%= txtEditBillingEmailAddress.ClientID%>').value;
            addLocationArray[editLocationIndex].billingemailaddress = updatedBillingEmailAddress;

            // Updated Shipping Location Information.
            var updatedShippingCompanyName = document.getElementById('<%= txtEditShippingCompanyName.ClientID%>').value;
            addLocationArray[editLocationIndex].shippingcompanyname = updatedShippingCompanyName;
            var esp = document.getElementById('<%= ddlEditShippingPrefix.ClientID%>');
            var updatedShippingPrefix = esp.options[esp.selectedIndex].value;
            addLocationArray[editLocationIndex].shippingprefix = updatedShippingPrefix;
            var updatedShippingFirstName = document.getElementById('<%= txtEditShippingFirstName.ClientID%>').value;
            addLocationArray[editLocationIndex].shippingfirstname = updatedShippingFirstName;
            var updatedShippingLastName = document.getElementById('<%= txtEditShippingLastName.ClientID%>').value;
            addLocationArray[editLocationIndex].shippinglastname = updatedShippingLastName;
            var updatedShippingAddressLine1 = document.getElementById('<%= txtEditShippingAddressLine1.ClientID%>').value;
            addLocationArray[editLocationIndex].shippingaddressline1 = updatedShippingAddressLine1;
            var updatedShippingAddressLine2 = document.getElementById('<%= txtEditShippingAddressLine2.ClientID%>').value;
            addLocationArray[editLocationIndex].shippingaddressline2 = updatedShippingAddressLine2
            var updatedShippingAddressLine3 = document.getElementById('<%= txtEditShippingAddressLine3.ClientID%>').value;
            addLocationArray[editLocationIndex].shippingaddressline3 = updatedShippingAddressLine3;
            var updatedShippingCity = document.getElementById('<%= txtEditShippingCity.ClientID%>').value;
            addLocationArray[editLocationIndex].shippingcity = updatedShippingCity;
            var ess = document.getElementById('<%= ddlEditShippingState.ClientID%>');
            var updatedShippingState = ess.options[ess.selectedIndex].value;
            addLocationArray[editLocationIndex].shippingstate = updatedShippingState;
            var updatedShippingStateText = ess.options[ess.selectedIndex].text;    // Gets SelectedText value of DDL.
            var updatedShippingPostalCode = document.getElementById('<%= txtEditShippingPostalCode.ClientID%>').value;
            addLocationArray[editLocationIndex].shippingpostalcode = updatedShippingPostalCode;
            var esc = document.getElementById('<%= ddlEditShippingCountry.ClientID%>');
            var updatedShippingCountry = esc.options[esc.selectedIndex].value;
            addLocationArray[editLocationIndex].shippingcountry = updatedShippingCountry;
            var updatedShippingCountryText = esc.options[esc.selectedIndex].text;  // Gets SelectedText value of DDL.
            var updatedShippingTelephone = document.getElementById('<%= txtEditShippingTelephone.ClientID%>').value;
            addLocationArray[editLocationIndex].shippingtelephone = updatedShippingTelephone;
            var updatedShippingFax = document.getElementById('<%= txtEditShippingFax.ClientID%>').value;
            addLocationArray[editLocationIndex].shippingfax = updatedShippingFax;
            var updatedShippingEmailAddress = document.getElementById('<%= txtEditShippingEmailAddress.ClientID%>').value;
            addLocationArray[editLocationIndex].shippingemailaddress = updatedShippingEmailAddress;

            if (Page_ClientValidate("EDITLOCATION")) {
                // Updated Location JSON String.
                locationJSON = null;
                locationJSON = JSON.stringify({ addLocationArray: addLocationArray });

                // Update information (display) of HTML table.
                var tableIndex = editLocationIndex + 1; // The HeaderRow is TableIndex of 0.
                myLocationTable = document.getElementById("locationsDataTable");

                // Alternate InnerHTML for optional fields.
                var ubaLine2, ubaLine3, ubFax;
                var usaLine2, usaLine3, usFax, usEmail;

                if (updatedBillingAddressLine2 !== "") {
                    ubaLine2 = updatedBillingAddressLine2 + "<br />";
                }
                else {
                    ubaLine2 = "";
                }

                if (updatedBillingAddressLine3 !== "") {
                    ubaLine3 = updatedBillingAddressLine3 + "<br />";
                }
                else {
                    ubaLine3 = "";
                }

                if (updatedBillingFax !== "") {
                    ubFax = "<u>Fax</u>: " + updatedBillingFax + "<br />";
                }
                else {
                    ubFax = "";
                }

                if (updatedShippingAddressLine2 !== "") {
                    usaLine2 = updatedShippingAddressLine2 + "<br />";
                }
                else {
                    usaLine2 = "";
                }

                if (updatedShippingAddressLine3 !== "") {
                    usaLine3 = updatedShippingAddressLine3 + "<br />";
                }
                else {
                    usaLine3 = "";
                }

                if (updatedShippingFax !== "") {
                    usFax = "<u>Fax</u>: " + updatedShippingFax + "<br />";
                }
                else {
                    usFax = "";
                }

                if (updatedShippingEmailAddress !== "") {
                    usEmail = "<u>E-mail</u>: " + updatedShippingEmailAddress + "<br />";
                }
                else {
                    usEmail = "";
                }

                // Billing Information Table Cell.
                myLocationTable.rows[tableIndex].cells[0].innerHTML = updatedBillingCompanyName + "<br />" +
                                                              "<u>Contact</u>: " + updatedBillingPrefix + " " + updatedBillingFirstName + " " + updatedBillingLastName + "<br />" +
                                                              updatedBillingAddressLine1 + "<br />" +
                                                              ubaLine2 +
                                                              ubaLine3 +
                                                              updatedBillingCity + " " + updatedBillingStateText + " " + updatedBillingPostalCode + "<br />" +
                                                              updatedBillingCountryText + "<br />" +
                                                              "<u>Phone</u>: " + updatedBillingTelephone + "<br />" +
                                                              ubFax +
                                                              "<u>E-mail</u>: " + updatedBillingEmailAddress + "<br /><br />";
                myLocationTable.rows[tableIndex].cells[1].innerHTML = updatedShippingCompanyName + "<br />" +
                                                              "<u>Contact</u>: " + updatedShippingPrefix + " " + updatedShippingFirstName + " " + updatedShippingLastName + "<br />" +
                                                              updatedShippingAddressLine1 + "<br />" +
                                                              usaLine2 +
                                                              usaLine3 +
                                                              updatedShippingCity + " " + updatedShippingStateText + " " + updatedShippingPostalCode + "<br />" +
                                                              updatedShippingCountryText + "<br />" +
                                                              "<u>Phone</u>: " + updatedShippingTelephone + "<br />" +
                                                              usFax +
                                                              usEmail + "<br /><br />";

                // Clear/Reset HiddenField and set with new JSON string.
                document.getElementById('<%= hdnfldLocationJSONString.ClientID%>').value = "";
                document.getElementById('<%= hdnfldLocationJSONString.ClientID%>').value = locationJSON;

                // Clear all Edit Location Form controls.
                clearEditLocationFormControls();

                // Close jQuery Modal/Dialog.
                closeDialog("divEditLocationInformationForm");
            }
        }

        function clearAddLocationFormControls() {
            // Reset controls and get user ready to enter additional Locations information.
            // Billing Location Information.
            document.getElementById('<%= txtAddBillingCompanyName.ClientID%>').value = "";
            document.getElementById('<%= ddlAddBillingPrefix.ClientID%>').value = "";
            document.getElementById('<%= txtAddBillingFirstName.ClientID%>').value = "";
            document.getElementById('<%= txtAddBillingLastName.ClientID%>').value = "";
            document.getElementById('<%= txtAddBillingAddressLine1.ClientID%>').value = "";
            document.getElementById('<%= txtAddBillingAddressLine2.ClientID%>').value = "";
            document.getElementById('<%= txtAddBillingAddressLine3.ClientID%>').value = "";
            document.getElementById('<%= txtAddBillingCity.ClientID%>').value = "";
            document.getElementById('<%= ddlAddBillingState.ClientID%>').selectIndex = 1;
            document.getElementById('<%= txtAddBillingPostalCode.ClientID%>').value = "";
            document.getElementById('<%= ddlAddBillingCountry.ClientID%>').value = "1";
            document.getElementById('<%= txtAddBillingTelephone.ClientID%>').value = "";
            document.getElementById('<%= txtAddBillingFax.ClientID%>').value = "";
            document.getElementById('<%= txtAddBillingEmailAddress.ClientID%>').value = "";

            // Shipping Location Information.
            document.getElementById('<%= chkbxSameAsAddBillingInformation.ClientID%>').checked = false;
            document.getElementById('<%= txtAddShippingCompanyName.ClientID%>').value = "";
            document.getElementById('<%= ddlAddShippingPrefix.ClientID%>').value = "";
            document.getElementById('<%= txtAddShippingFirstName.ClientID%>').value = "";
            document.getElementById('<%= txtAddShippingLastName.ClientID%>').value = "";
            document.getElementById('<%= txtAddShippingAddressLine1.ClientID%>').value = "";
            document.getElementById('<%= txtAddShippingAddressLine2.ClientID%>').value = "";
            document.getElementById('<%= txtAddShippingAddressLine3.ClientID%>').value = "";
            document.getElementById('<%= txtAddShippingCity.ClientID%>').value = "";
            document.getElementById('<%= ddlAddShippingState.ClientID%>').selectIndex = 1;
            document.getElementById('<%= txtAddShippingPostalCode.ClientID%>').value = "";
            document.getElementById('<%= ddlAddShippingCountry.ClientID%>').value = "1";
            document.getElementById('<%= txtAddShippingTelephone.ClientID%>').value = "";
            document.getElementById('<%= txtAddShippingFax.ClientID%>').value = "";
            document.getElementById('<%= txtAddShippingEmailAddress.ClientID%>').value = "";

            resetValidators();
        }

        function clearEditLocationFormControls() {
            // Reset controls and get user ready to enter additional Locations information.
            // Billing Location Information.
            document.getElementById('<%= txtEditBillingCompanyName.ClientID%>').value = "";
            document.getElementById('<%= ddlEditBillingPrefix.ClientID%>').value = "";
            document.getElementById('<%= txtEditBillingFirstName.ClientID%>').value = "";
            document.getElementById('<%= txtEditBillingLastName.ClientID%>').value = "";
            document.getElementById('<%= txtEditBillingAddressLine1.ClientID%>').value = "";
            document.getElementById('<%= txtEditBillingAddressLine2.ClientID%>').value = "";
            document.getElementById('<%= txtEditBillingAddressLine3.ClientID%>').value = "";
            document.getElementById('<%= txtEditBillingCity.ClientID%>').value = "";
            document.getElementById('<%= ddlEditBillingState.ClientID%>').selectIndex = 1;
            document.getElementById('<%= txtEditBillingPostalCode.ClientID%>').value = "";
            document.getElementById('<%= ddlEditBillingCountry.ClientID%>').value = "1";
            document.getElementById('<%= txtEditBillingTelephone.ClientID%>').value = "";
            document.getElementById('<%= txtEditBillingFax.ClientID%>').value = "";
            document.getElementById('<%= txtEditBillingEmailAddress.ClientID%>').value = "";

            // Shipping Location Information.
            document.getElementById('<%= chkbxSameAsEditBillingInformation.ClientID%>').checked = false;
            document.getElementById('<%= txtEditShippingCompanyName.ClientID%>').value = "";
            document.getElementById('<%= ddlEditShippingPrefix.ClientID%>').value = "";
            document.getElementById('<%= txtEditShippingFirstName.ClientID%>').value = "";
            document.getElementById('<%= txtEditShippingLastName.ClientID%>').value = "";
            document.getElementById('<%= txtEditShippingAddressLine1.ClientID%>').value = "";
            document.getElementById('<%= txtEditShippingAddressLine2.ClientID%>').value = "";
            document.getElementById('<%= txtEditShippingAddressLine3.ClientID%>').value = "";
            document.getElementById('<%= txtEditShippingCity.ClientID%>').value = "";
            document.getElementById('<%= ddlEditShippingState.ClientID%>').selectIndex = 1;
            document.getElementById('<%= txtEditShippingPostalCode.ClientID%>').value = "";
            document.getElementById('<%= ddlEditShippingCountry.ClientID%>').value = "1";
            document.getElementById('<%= txtEditShippingTelephone.ClientID%>').value = "";
            document.getElementById('<%= txtEditShippingFax.ClientID%>').value = "";
            document.getElementById('<%= txtEditShippingEmailAddress.ClientID%>').value = "";

            resetValidators();
        }
        //================================================================================================================//
        //================================================================================================================//

        //=================================== ADD/EDIT ORDER INFORMATION MODAL/DIALOGS ===================================//
        //================================================================================================================//
        var addOrder = {};
        var addOrderArray = [];
        var orderJSON = "";
        var j = 0;  // Displayed index of the JavaScript table.
        var myOrderTable;

        function addOrderRow() {
            resetValidators();

            // Get the values from each ASP.NET form control.
            var index = j;
            var aopgid = document.getElementById('<%= ddlAddProductGroupID.ClientID%>');
            var productGroupID = aopgid.options[aopgid.selectedIndex].value;
            var productGroupName = aopgid.options[aopgid.selectedIndex].text;
            var aopid = document.getElementById('<%= ddlAddProductID.ClientID%>');
            var productID = aopid.options[aopid.selectedIndex].value;
            var productDescription = aopid.options[aopid.selectedIndex].text;
            var quantity = document.getElementById('<%= txtAddQuantity.ClientID%>').value;
            var unitPrice = document.getElementById('<%= txtAddUnitPrice.ClientID%>').value;

            var descriptionArray = productDescription.split('-');
            var productColor = descriptionArray[0];
            var productSKU = descriptionArray[1];

            if (Page_ClientValidate("ADDORDER")) {
                // Order Information Object.
                addOrder = {
                    index: index,
                    productgroupid: productGroupID,
                    productid: productID,
                    color: productColor,
                    productsku: productSKU,
                    quantity: quantity,
                    unitprice: unitPrice
                };

                myOrderTable = document.getElementById("ordersDataTable");
                var rowCount = myOrderTable.rows.length;
                var row = myOrderTable.insertRow(rowCount);

                // Only for display (acutal record index is i).
                var x = j + 1;

                // Formatted InnerHTML for dynamically generated table.
                //row.insertCell(0).innerHTML = x;
                row.insertCell(0).innerHTML = productGroupName;
                row.insertCell(1).innerHTML = '<input type="label" class="productColor ' + productColor + '" value="' + productSKU + '">';
                row.insertCell(2).innerHTML = quantity;
                row.insertCell(3).innerHTML = FormatToCurrency(unitPrice);
                // Delete|Edit Buttons.
                row.insertCell(4).innerHTML = '<input type="button" class="Icon Remove" value="Delete" onClick="Javacsript:deleteOrderRow(this)">&nbsp;<input type="button" class="Icon Edit" value="Edit" onClick="Javacsript:editOrderRow(this)">';

                addOrderArray.push(addOrder);

                orderJSON = JSON.stringify({ addOrderArray: addOrderArray });

                document.getElementById('<%= hdnfldOrderJSONString.ClientID%>').value = orderJSON;

                // Increment index.
                j++;

                // Clear all Add Order Form controls.
                clearAddOrderFormControls();

                // Close jQuery Modal/Dialog.
                closeDialog("divAddOrderInformationForm");
            }
        }

        function FormatToCurrency(num) {
            num = num.toString().replace(/\$|\,/g, '');
            if (isNaN(num))
                num = "0";
            sign = (num == (num = Math.abs(num)));
            num = Math.floor(num * 100 + 0.50000000001);
            cents = num % 100;
            num = Math.floor(num / 100).toString();
            if (cents < 10)
                cents = "0" + cents;
            for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3) ; i++)
                num = num.substring(0, num.length - (4 * i + 3)) + ',' + num.substring(num.length - (4 * i + 3));
            return (((sign) ? '' : '-') + '$' + num + '.' + cents);
        }

        // Delete a given record/row (not yet in database).
        function deleteOrderRow(obj) {
            var index = (obj.parentNode.parentNode.rowIndex);
            myOrderTable = document.getElementById("ordersDataTable");
            myOrderTable.deleteRow(index);

            // Gets acutal index of Order object in JSON Array.
            var y = parseInt(index) - 1;

            removeByIndex(addOrderArray, (y));
            orderJSON = null;
            orderJSON = JSON.stringify({ addOrderArray: addOrderArray });
            document.getElementById('<%= hdnfldOrderJSONString.ClientID %>').value = "";
            document.getElementById('<%= hdnfldOrderJSONString.ClientID %>').value = orderJSON;
        }

        function editOrderRow(obj) {
            resetValidators();

            var index = (obj.parentNode.parentNode.rowIndex);

            // Get index of record (subtract 1 to the value since the table actually has 0 has the Header).
            editOrderIndex = index - 1;

            // Assign editIndex to HiddenField in Modal/Dialog.
            document.getElementById('<%= hdnfldEditOrderIndex.ClientID%>').value = editOrderIndex.toString();

            document.getElementById('<%= ddlEditProductGroupID.ClientID %>').value = addOrderArray[editOrderIndex].productgroupid;
            document.getElementById('<%= ddlEditProductID.ClientID %>').value = addOrderArray[editOrderIndex].productid;
            document.getElementById('<%= txtEditQuantity.ClientID %>').value = addOrderArray[editOrderIndex].quantity;
            document.getElementById('<%= txtEditUnitPrice.ClientID %>').value = addOrderArray[editOrderIndex].unitprice;

            // Open Edit Order Information Form Modal/Dialog.
            openDialog("divEditOrderInformationForm");
        }

        function updateOrderRow() {
            // Update JavaScript Table display.
            editOrderIndex = parseInt(document.getElementById('<%= hdnfldEditOrderIndex.ClientID%>').value);

            // Update each item in the Array (based on index).
            var eopgid = document.getElementById('<%= ddlEditProductGroupID.ClientID%>');
            var updatedProductGroupID = eopgid.options[eopgid.selectedIndex].value;
            addOrderArray[editOrderIndex].productgroupid = updatedProductGroupID;
            var updatedProductGroupName = eopgid.options[eopgid.selectedIndex].text;
            var eopid = document.getElementById('<%= ddlEditProductID.ClientID%>');
            var updatedProductID = eopid.options[eopid.selectedIndex].value;
            addOrderArray[editOrderIndex].productid = updatedProductID;
            var updatedProductDescription = eopid.options[eopid.selectedIndex].text;
            var updatedQuantity = document.getElementById('<%= txtEditQuantity.ClientID%>').value;
            addOrderArray[editOrderIndex].quantity = updatedQuantity;
            var updatedUnitPrice = document.getElementById('<%= txtEditUnitPrice.ClientID%>').value;
            addOrderArray[editOrderIndex].unitprice = updatedUnitPrice;

            var updatedDescriptionArray = updatedProductDescription.split('-');
            var updatedProductColor = updatedDescriptionArray[0];
            addOrderArray[editOrderIndex].color = updatedProductColor;
            var updatedProductSKU = updatedDescriptionArray[1];
            addOrderArray[editOrderIndex].productsku = updatedProductSKU;

            if (Page_ClientValidate("EDITORDER")) {
                // Updated Order JSON String.
                orderJSON = null;
                orderJSON = JSON.stringify({ addOrderArray: addOrderArray });

                // Update information (display) of HTML table.
                var tableIndex = editOrderIndex + 1; // The HeaderRow is TableIndex of 0.
                myOrderTable = document.getElementById("ordersDataTable");

                myOrderTable.rows[tableIndex].cells[0].innerHTML = updatedProductGroupName;
                myOrderTable.rows[tableIndex].cells[1].innerHTML = '<input type="label" class="productColor ' + updatedProductColor + '" value="' + updatedProductColor + '">';
                myOrderTable.rows[tableIndex].cells[2].innerHTML = updatedQuantity;
                myOrderTable.rows[tableIndex].cells[3].innerHTML = FormatToCurrency(updatedUnitPrice);

                // Clear/Reset HiddenField and set with new JSON string.
                document.getElementById('<%= hdnfldOrderJSONString.ClientID%>').value = "";
                document.getElementById('<%= hdnfldOrderJSONString.ClientID%>').value = orderJSON;

                // Clear all Edit Order Form controls.
                clearEditOrderFormControls();

                // Close jQuery Modal/Dialog.
                closeDialog("divEditOrderInformationForm");
            }
        }

        function clearAddOrderFormControls() {
            // Reset controls and get user ready to enter additional Order information.
            document.getElementById('<%= ddlAddProductGroupID.ClientID%>').selectedIndex = 0;
            document.getElementById('<%= ddlAddProductID.ClientID%>').selectedIndex = 0;
            document.getElementById('<%= txtAddQuantity.ClientID%>').value = "";
            document.getElementById('<%= txtAddUnitPrice.ClientID%>').value = "";

            resetValidators();
        }

        function clearEditOrderFormControls() {
            // Reset controls and get user ready to enter additional Order information.
            document.getElementById('<%= ddlEditProductGroupID.ClientID%>').selectedIndex = 0;
            document.getElementById('<%= ddlEditProductID.ClientID%>').selectedIndex = 0;
            document.getElementById('<%= txtEditQuantity.ClientID%>').value = "";
            document.getElementById('<%= txtEditUnitPrice.ClientID%>').value = "";

            resetValidators();
        }

        // For testing only.
        function displayJSON() {
            var jsonString = document.getElementById('<%= hdnfldLocationJSONString.ClientID %>').value;
            document.getElementById("jsonString").innerHTML = jsonString.toString();
            //alert(jsonString);
        }
        //================================================================================================================//
        //================================================================================================================//
    </script>
 
    <script type="text/javascript">
        //========================================= CREDIT CARD NUMBER VALIDATION ========================================//
        //=================== Taken from CreateAccount.aspx (Functionality is the same for this page.) ===================//
        //================================================================================================================//
        var ccErrorNo = 0;
        var ccErrors = new Array()

        ccErrors[0] = "Unknown card type";
        ccErrors[1] = "No card number provided";
        ccErrors[2] = "Credit card number is in invalid format";
        ccErrors[3] = "Credit card number is invalid";
        ccErrors[4] = "Credit card number has an inappropriate number of digits";


        function testCreditCard() {
            var myCardType = '';

            myCardNo = document.getElementById('ctl00_primaryHolder_txtCreditCardNumber').value;

            var radiolist = document.getElementsByName('ctl00$primaryHolder$rdobtnlstCreditCardType');
            if (radiolist[0].checked) myCardType = radiolist[0].value;
            if (radiolist[1].checked) myCardType = radiolist[1].value;
            if (radiolist[2].checked) myCardType = radiolist[2].value;
            if (radiolist[3].checked) myCardType = radiolist[3].value;

            if (myCardNo != '' && myCardType != '') {
                if (checkCreditCard(myCardNo, myCardType)) {
                    //alert ("Credit card has a valid format");
                }
                else { alert(ccErrors[ccErrorNo]); }
            }
            else if (myCardNo != '' && myCardType == '') {
                alert("Select type of credit card");
            }
            else if (myCardNo == '' && myCardType != '') {
                //alert ("do nothing");
            }
            else { alert("Credit card number or credit card type is invalid") }

            //return false;
        }

        function checkCreditCard(cardnumber, cardtypename) {

            // Array to hold the permitted card characteristics.
            var cards = new Array();

            // Define the cards we support. You may add addtional card types as follows.

            //  name:         As in the selection box of the form - must be same as user's.
            //  length:       List of possible valid lengths of the card number for the card.
            //  prefixes:     List of possible prefixes for the card.
            //  checkdigit:   Boolean to say whether there is a check digit.

            cards[0] = {
                typename: '1',
                name: "Visa",
                length: "13,16",
                prefixes: "4",
                checkdigit: true
            };
            cards[1] = {
                typename: '2',
                name: "MasterCard",
                length: "16",
                prefixes: "51,52,53,54,55",
                checkdigit: true
            };
            cards[2] = {
                typename: '4',
                name: "AmEx",
                length: "15",
                prefixes: "34,37",
                checkdigit: true
            };
            cards[3] = {
                typename: '3',
                name: "Discover",
                length: "16",
                prefixes: "6011,622,64,65",
                checkdigit: true
            };
            // Establish card type.
            var cardType = -1;
            for (var i = 0; i < cards.length; i++) {

                // See if it is this card (ignoring the case of the string).
                if (cardtypename.toLowerCase() == cards[i].typename.toLowerCase()) {
                    cardType = i;
                    break;
                }
            }

            // If card type not found, report an error.
            if (cardType == -1) {
                ccErrorNo = 0;
                return false;
            }

            // Ensure that the user has provided a credit card number.
            if (cardnumber.length == 0) {
                ccErrorNo = 1;
                return false;
            }

            // Now remove any spaces from the credit card number.
            cardnumber = cardnumber.replace(/\s/g, "");

            // Check that the number is numeric.
            var cardNo = cardnumber;
            var cardexp = /^[0-9]{13,19}$/;
            if (!cardexp.exec(cardNo)) {
                ccErrorNo = 2;
                return false;
            }

            // Now check the modulus 10 check digit - if required.
            if (cards[cardType].checkdigit) {
                var checksum = 0;   // Running checksum total.
                var mychar = "";    // Next char to process.
                var j = 1;          // Takes value of 1 or 2.

                // Process each digit one by one starting at the right.
                var calc;
                for (i = cardNo.length - 1; i >= 0; i--) {

                    // Extract the next digit and multiply by 1 or 2 on alternative digits.
                    calc = Number(cardNo.charAt(i)) * j;

                    // If the result is in two digits add 1 to the checksum total.
                    if (calc > 9) {
                        checksum = checksum + 1;
                        calc = calc - 10;
                    }

                    // Add the units element to the checksum total.
                    checksum = checksum + calc;

                    // Switch the value of j
                    if (j == 1) { j = 2 } else { j = 1 }
                }

                // All done - if checksum is divisible by 10, it is a valid modulus 10.
                // If not, report an error.
                if (checksum % 10 != 0) {
                    ccErrorNo = 3;
                    return false;
                }
            }

            // The following are the card-specific checks we undertake.
            var LengthValid = false;
            var PrefixValid = false;
            var undefined;

            // We use these for holding the valid lengths and prefixes of a card type.
            var prefix = new Array();
            var lengths = new Array();

            // Load an array with the valid prefixes for this card.
            prefix = cards[cardType].prefixes.split(",");

            // Now see if any of them match what we have in the card number.
            for (i = 0; i < prefix.length; i++) {
                var exp = new RegExp("^" + prefix[i]);
                if (exp.test(cardNo)) PrefixValid = true;
            }

            // If it isn't a valid prefix there's no point at looking at the length.
            if (!PrefixValid) {
                ccErrorNo = 3;
                return false;
            }

            // See if the length is valid for this card.
            lengths = cards[cardType].length.split(",");
            for (j = 0; j < lengths.length; j++) {
                if (cardNo.length == lengths[j]) LengthValid = true;
            }

            // See if all is OK by seeing if the length was valid. We only check the length if all else was hunky dory.
            if (!LengthValid) {
                ccErrorNo = 4;
                return false;
            };

            // The credit card is in the required format.
            return true;
        }
        //================================================================================================================//
        //================================================================================================================//
    </script>

    <script type="text/javascript">
        //============================================== VALIDATE USERNAME ===============================================//
        //================================================================================================================//
        function ShowAvailability() {
            PageMethods.CheckUserName(document.getElementById("<%= txtUsername.ClientID %>").value, OnSuccess);
        }

        // Checks to see if WebMethod returns true/false/error.
        function OnSuccess(response) {
            var status = document.getElementById("status");
            switch (response) {
                case "true":
                    status.className = "validator valid";
                    break;
                case "false":
                    status.className = "validator invalid";
                    break;
                case "error":
                    status.style.color = "red";
                    status.innerHTML = "Error occured";
                    break;
                default:
                    break;
            }
        }

        // Resets TextBox and Status Icon/Message.
        function OnChange(txt) {
            document.getElementById("status").className = "";
            document.getElementById("status").innerHTML = "";
        }
        //================================================================================================================//
        //================================================================================================================//
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>

    <%--HIDDENFIELDS TO HOLD INDEXES AND JSON--%>
    <asp:HiddenField ID="hdnfldOrderJSONString" runat="server" Value="" />
    <asp:HiddenField ID="hdnfldLocationJSONString" runat="server" Value="" />
    <asp:HiddenField ID="hdnfldAddAccordionIndex" runat="server" Value="0" />
    <asp:HiddenField ID="hdnfldEditAccordionIndex" runat="server" Value="0" />
    <%--END--%>

    <%--FORM ERROR(S)--%>
    <div id="divErrorMessage" runat="server" class="FormError" visible="false">
		<p><span class="MessageIcon"></span>
	        <strong>Messages:</strong>
            <span id="spnErrorMessage" runat="server">               
            </span>
        </p>
	</div>
    <%--END--%>

    <%--VALIDATION SUMMARY (OF FORM ERRORS)--%>
    <div>
        <asp:ValidationSummary ID="ValidationSummary1" runat="server"
        HeaderText="<span class='MessageIcon'></span><strong>You must enter a valid value in the following fields:</strong><br/>"
        DisplayMode ="BulletList" 
        EnableClientScript="true"
        ValidationGroup="CRMREGISTRATION"
        ShowSummary="true" CssClass="FormError" />          
    </div>
    <%--END--%>

    <%--ADD LOCATION INFORMATION FORM MODAL/DIALOG--%>
    <div id="divAddLocationInformationForm">
        <asp:UpdatePanel ID="updtpnlAddLocationInformationForm" runat="server">
            <ContentTemplate>
                <div id="addLocationAccordion" style="margin-top: 10px;">
                    <div id="divAddBillingInformation" runat="server">
			            <h3><a href="#">Billing Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--BILLING COMPANY NAME (COMPANY)--%>
                            <div class="Row">
                                <div class="Label">Company<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddBillingCompanyName" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddBillingCompanyName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddBillingCompanyName" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING FIRST NAME--%>
                            <div class="Row">
                                <div class="Label">First Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlAddBillingPrefix" runat="server">
                                        <asp:ListItem Text="" Value="" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="Dr." Value="Dr."></asp:ListItem>
                                        <asp:ListItem Text="Miss" Value="Miss"></asp:ListItem>
                                        <asp:ListItem Text="Mr." Value="Mr."></asp:ListItem>
                                        <asp:ListItem Text="Mrs." Value="Mrs."></asp:ListItem>
                                        <asp:ListItem Text="Ms." Value="Ms."></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:TextBox ID="txtAddBillingFirstName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddBillingFirstName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddBillingFirstName" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING LAST NAME--%>
                            <div class="Row">
                                <div class="Label">Last Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddBillingLastName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddBillingLastName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddBillingLastName" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING ADDRESS LINE 1--%>
                            <div class="Row">
                                <div class="Label">Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddBillingAddressLine1" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddBillingAddressLine1" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddBillingAddressLine1" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING ADDRESS LINE 2--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddBillingAddressLine2" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING ADDRESS LINE 3--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddBillingAddressLine3" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING COUNTRY--%>
                            <div class="Row">
                                <div class="Label">Country<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlAddBillingCountry" runat="server" AutoPostBack="true" 
                                    AppendDataBoundItems="true" OnSelectedIndexChanged="ddlAddBillingCountry_SelectedIndexChanged">
                                        <asp:ListItem Text="-Select Country-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddBillingCountry" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlAddBillingCountry" ValidationGroup="ADDLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING CITY--%>
                            <div class="Row">
                                <div class="Label">City<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddBillingCity" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddBillingCity" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddBillingCity" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING STATE/POSTAL CODE--%>
                            <div class="Row">
                                <div class="Label">State/Postal<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlAddBillingState" runat="server" AppendDataBoundItems="true">
                                        <asp:ListItem Text="-Select State-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddBillingState" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlAddBillingState" ValidationGroup="ADDLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                    <asp:TextBox ID="txtAddBillingPostalCode" runat="server" Text="" CssClass="Size Small" MaxLength="15"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddBillingPostalCode" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddBillingPostalCode" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING TELEPHONE--%>
                            <div class="Row">
                                <div class="Label">Telephone<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddBillingTelephone" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddBillingTelephone" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddBillingTelephone" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="regexpvalAddBillingTelephone" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtAddBillingTelephone" ValidationGroup="ADDLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING FAX--%>
                            <div class="Row">
                                <div class="Label">Fax:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddBillingFax" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="regexpvalAddBillingFax" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtAddBillingFax" ValidationGroup="ADDLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING E-MAIL ADDRESS--%>
                            <div class="Row">
                                <div class="Label">E-Mail Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddBillingEmailAddress" runat="server" Text="" CssClass="Size Medium" MaxLength="60"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddBillingEmailAddress" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddBillingEmailAddress" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="regexpvalAddBillingEmailAddress" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtAddBillingEmailAddress" ValidationGroup="ADDLOCATION"     
                                    ValidationExpression="[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </div>
                    <div id="divAddShippingInformation" runat="server">
			            <h3><a href="#">Shipping Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--THE SAME AS BILLING INFORMATION--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxSameAsAddBillingInformation" runat="server" Text="The same as billing information." Checked="false" 
                                    AutoPostBack="true" OnCheckedChanged="chkbxSameAsAddBillingInformation_CheckedChanged" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING COMPANY NAME (COMPANY)--%>
                            <div class="Row">
                                <div class="Label">Company<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddShippingCompanyName" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddShippingCompanyName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddShippingCompanyName" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING FIRST NAME--%>
                            <div class="Row">
                                <div class="Label">First Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlAddShippingPrefix" runat="server">
                                        <asp:ListItem Text="" Value="" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="Dr." Value="Dr."></asp:ListItem>
                                        <asp:ListItem Text="Miss" Value="Miss"></asp:ListItem>
                                        <asp:ListItem Text="Mr." Value="Mr."></asp:ListItem>
                                        <asp:ListItem Text="Mrs." Value="Mrs."></asp:ListItem>
                                        <asp:ListItem Text="Ms." Value="Ms."></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:TextBox ID="txtAddShippingFirstName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddShippingFirstName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddShippingFirstName" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING LAST NAME--%>
                            <div class="Row">
                                <div class="Label">Last Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddShippingLastName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddShippingLastName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddShippingLastName" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING ADDRESS LINE 1--%>
                            <div class="Row">
                                <div class="Label">Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddShippingAddressLine1" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddShippingAddressLine1" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddShippingAddressLine1" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING ADDRESS LINE 2--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddShippingAddressLine2" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING ADDRESS LINE 3--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddShippingAddressLine3" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING COUNTRY--%>
                            <div class="Row">
                                <div class="Label">Country<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlAddShippingCountry" runat="server" AutoPostBack="true" 
                                    AppendDataBoundItems="true" OnSelectedIndexChanged="ddlAddShippingCountry_SelectedIndexChanged">
                                        <asp:ListItem Text="-Select Country-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddShippingCountry" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlAddShippingCountry" ValidationGroup="ADDLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING CITY--%>
                            <div class="Row">
                                <div class="Label">City<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddShippingCity" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddShippingCity" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddShippingCity" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING STATE/POSTAL CODE--%>
                            <div class="Row">
                                <div class="Label">State/Postal<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlAddShippingState" runat="server" AppendDataBoundItems="true">
                                        <asp:ListItem Text="-Select State-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddShippingState" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlAddShippingState" ValidationGroup="ADDLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                    <asp:TextBox ID="txtAddShippingPostalCode" runat="server" Text="" CssClass="Size Small" MaxLength="15"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddShippingPostalCode" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddShippingPostalCode" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING TELEPHONE--%>
                            <div class="Row">
                                <div class="Label">Telephone<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddShippingTelephone" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddShippingTelephone" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtAddShippingTelephone" ValidationGroup="ADDLOCATION"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="regexpvalAddShippingTelephone" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtAddShippingTelephone" ValidationGroup="ADDLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING FAX--%>
                            <div class="Row">
                                <div class="Label">Fax:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddShippingFax" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="reqfldvalAddShippingFax" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtAddShippingFax" ValidationGroup="ADDLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING E-MAIL ADDRESS--%>
                            <div class="Row">
                                <div class="Label">E-Mail Address:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAddShippingEmailAddress" runat="server" Text="" CssClass="Size Medium" MaxLength="60"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="regexpvalAddShippingEmailAddress" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtAddShippingEmailAddress" ValidationGroup="ADDLOCATION"     
                                    ValidationExpression="[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </div>
                </div>
                <%--FOOTER - REQUIRED FIELDS INDICATOR--%>
                <div class="RequiredIndicator" style="padding-top: 10px;">
                    <span class="Required">*</span>&nbsp;Indicates a required field.                   
                </div>
                <%--END--%>
                <asp:button ID="btnAddLocation" runat="server" OnClientClick="addLocationRow(); return false;" Text="Add" ValidationGroup="ADDLOCATION" style="display: none;" />
				<asp:button ID="btnCancelAddLocation" runat="server" OnClientClick="clearAddLocationFormControls(); return false;" Text="Cancel" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--EDIT LOCATION INFORMATION FORM MODAL/DIALOG--%>
    <div id="divEditLocationInformationForm">
        <asp:UpdatePanel ID="updtpnlEditLocationInformationForm" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldEditLocationIndex" runat="server" Value="0" />
                <div id="editLocationAccordion" style="margin-top: 10px;">
                    <div id="divEditBillingInformation" runat="server">
			            <h3><a href="#">Billing Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--BILLING COMPANY NAME (COMPANY)--%>
                            <div class="Row">
                                <div class="Label">Company<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditBillingCompanyName" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditBillingCompanyName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditBillingCompanyName" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING FIRST NAME--%>
                            <div class="Row">
                                <div class="Label">First Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlEditBillingPrefix" runat="server">
                                        <asp:ListItem Text="" Value="" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="Dr." Value="Dr."></asp:ListItem>
                                        <asp:ListItem Text="Miss" Value="Miss"></asp:ListItem>
                                        <asp:ListItem Text="Mr." Value="Mr."></asp:ListItem>
                                        <asp:ListItem Text="Mrs." Value="Mrs."></asp:ListItem>
                                        <asp:ListItem Text="Ms." Value="Ms."></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:TextBox ID="txtEditBillingFirstName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditBillingFirstName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditBillingFirstName" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING LAST NAME--%>
                            <div class="Row">
                                <div class="Label">Last Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditBillingLastName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditBillingLastName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditBillingLastName" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING ADDRESS LINE 1--%>
                            <div class="Row">
                                <div class="Label">Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditBillingAddressLine1" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditBillingAddressLine1" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditBillingAddressLine1" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING ADDRESS LINE 2--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditBillingAddressLine2" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING ADDRESS LINE 3--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditBillingAddressLine3" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING COUNTRY--%>
                            <div class="Row">
                                <div class="Label">Country<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlEditBillingCountry" runat="server" AutoPostBack="true" 
                                    AppendDataBoundItems="true" OnSelectedIndexChanged="ddlEditBillingCountry_SelectedIndexChanged">
                                        <asp:ListItem Text="-Select Country-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditBillingCountry" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlEditBillingCountry" ValidationGroup="EDITLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING CITY--%>
                            <div class="Row">
                                <div class="Label">City<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditBillingCity" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditBillingCity" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditBillingCity" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING STATE/POSTAL CODE--%>
                            <div class="Row">
                                <div class="Label">State/Postal<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlEditBillingState" runat="server" AppendDataBoundItems="true">
                                        <asp:ListItem Text="-Select State-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditBillingState" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlEditBillingState" ValidationGroup="EDITLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                    <asp:TextBox ID="txtEditBillingPostalCode" runat="server" Text="" CssClass="Size Small" MaxLength="15"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditBillingPostalCode" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditBillingPostalCode" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING TELEPHONE--%>
                            <div class="Row">
                                <div class="Label">Telephone<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditBillingTelephone" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditBillingTelephone" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditBillingTelephone" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="regexpvalEditBillingTelephone" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtEditBillingTelephone" ValidationGroup="EDITLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING FAX--%>
                            <div class="Row">
                                <div class="Label">Fax:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditBillingFax" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="regexpvalEditBillingFax" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtEditBillingFax" ValidationGroup="EDITLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING E-MAIL ADDRESS--%>
                            <div class="Row">
                                <div class="Label">E-Mail Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditBillingEmailAddress" runat="server" Text="" CssClass="Size Medium" MaxLength="60"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditBillingEmailAddress" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditBillingEmailAddress" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="regexpvalEditBillingEmailAddress" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtEditBillingEmailAddress" ValidationGroup="EDITLOCATION"     
                                    ValidationExpression="[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </div>
                    <div id="divEditShippingInformation" runat="server">
			            <h3><a href="#">Shipping Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--THE SAME AS BILLING INFORMATION--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxSameAsEditBillingInformation" runat="server" Text="The same as billing information." Checked="false" 
                                    AutoPostBack="true" OnCheckedChanged="chkbxSameAsEditBillingInformation_CheckedChanged" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING COMPANY NAME (COMPANY)--%>
                            <div class="Row">
                                <div class="Label">Company<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditShippingCompanyName" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditShippingCompanyName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditShippingCompanyName" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING FIRST NAME--%>
                            <div class="Row">
                                <div class="Label">First Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlEditShippingPrefix" runat="server">
                                        <asp:ListItem Text="" Value="" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="Dr." Value="Dr."></asp:ListItem>
                                        <asp:ListItem Text="Miss" Value="Miss"></asp:ListItem>
                                        <asp:ListItem Text="Mr." Value="Mr."></asp:ListItem>
                                        <asp:ListItem Text="Mrs." Value="Mrs."></asp:ListItem>
                                        <asp:ListItem Text="Ms." Value="Ms."></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:TextBox ID="txtEditShippingFirstName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditShippingFirstName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditShippingFirstName" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING LAST NAME--%>
                            <div class="Row">
                                <div class="Label">Last Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditShippingLastName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditShippingLastName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditShippingLastName" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING ADDRESS LINE 1--%>
                            <div class="Row">
                                <div class="Label">Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditShippingAddressLine1" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditShippingAddressLine1" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditShippingAddressLine1" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING ADDRESS LINE 2--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditShippingAddressLine2" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING ADDRESS LINE 3--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditShippingAddressLine3" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING COUNTRY--%>
                            <div class="Row">
                                <div class="Label">Country<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlEditShippingCountry" runat="server" AutoPostBack="true" 
                                    AppendDataBoundItems="true" OnSelectedIndexChanged="ddlEditShippingCountry_SelectedIndexChanged">
                                        <asp:ListItem Text="-Select Country-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditShippingCountry" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlEditShippingCountry" ValidationGroup="EDITLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING CITY--%>
                            <div class="Row">
                                <div class="Label">City<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditShippingCity" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditShippingCity" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditShippingCity" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING STATE/POSTAL CODE--%>
                            <div class="Row">
                                <div class="Label">State/Postal<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlEditShippingState" runat="server" AppendDataBoundItems="true">
                                        <asp:ListItem Text="-Select State-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditShippingState" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlEditShippingState" ValidationGroup="EDITLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                    <asp:TextBox ID="txtEditShippingPostalCode" runat="server" Text="" CssClass="Size Small" MaxLength="15"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditShippingPostalCode" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditShippingPostalCode" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING TELEPHONE--%>
                            <div class="Row">
                                <div class="Label">Telephone<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditShippingTelephone" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalEditShippingTelephone" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtEditShippingTelephone" ValidationGroup="EDITLOCATION"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="regexpvalEditShippingTelephone" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtEditShippingTelephone" ValidationGroup="EDITLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING FAX--%>
                            <div class="Row">
                                <div class="Label">Fax:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditShippingFax" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="regexpvalEditShippingFax" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtEditShippingFax" ValidationGroup="EDITLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING E-MAIL ADDRESS--%>
                            <div class="Row">
                                <div class="Label">E-Mail Address:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEditShippingEmailAddress" runat="server" Text="" CssClass="Size Medium" MaxLength="60"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="regexpvalShippingEmailAddress" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtEditShippingEmailAddress" ValidationGroup="EDITLOCATION"     
                                    ValidationExpression="[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </div>
                </div>
                <%--FOOTER - REQUIRED FIELDS INDICATOR--%>
                <div class="RequiredIndicator" style="padding-top: 10px;">
                    <span class="Required">*</span>&nbsp;Indicates a required field.                   
                </div>
                <%--END--%>
                <asp:button ID="btnEditLocation" runat="server" OnClientClick="updateLocationRow(); return false;" Text="Update" ValidationGroup="EDITLOCATION" style="display: none;" />
				<asp:button ID="btnCancelEditLocation" runat="server" OnClientClick="clearEditLocationFormControls(); return false;" Text="Cancel" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--ADD ORDER INFORMATION FORM MODAL/DIALOG--%>
    <div id="divAddOrderInformationForm">
        <asp:UpdatePanel ID="updtpnlAddOrderInformationForm" runat="server">
            <ContentTemplate>
                <div class="OForm">
                    <%--PRODUCT GROUP ID--%>
                    <div class="Row">
                        <div class="Label">Product Group<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlAddProductGroupID" runat="server" AutoPostBack="true" DataTextField="ProductName" DataValueField="ProductGroupID"
                            AppendDataBoundItems="true" OnSelectedIndexChanged="ddlAddProductGroupID_SelectedIndexChanged">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalAddProductGroupID" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="ddlAddProductGroupID" ValidationGroup="ADDORDER" InitialValue="0"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--PRODUCT ID--%>
                    <div class="Row">
                        <div class="Label">Product<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlAddProductID" runat="server" AppendDataBoundItems="true" DataTextField="ProductDescription" DataValueField="ProductID">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalAddProductID" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="ddlAddProductID" ValidationGroup="ADDORDER" InitialValue="0"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--QUANTITY--%>
                    <div class="Row">
                        <div class="Label">Quantity<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtAddQuantity" runat="server" Text=""></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqfldvalAddQuantity" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="txtAddQuantity" ValidationGroup="ADDORDER"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--UNIT PRICE--%>
                    <div class="Row">
                        <div class="Label">Unit Price<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtAddUnitPrice" runat="server" Text=""></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqfldvalAddUnitPrice" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="txtAddUnitPrice" ValidationGroup="ADDORDER"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                </div>    
                <%--FOOTER - REQUIRED FIELDS INDICATOR--%>
                <div class="RequiredIndicator" style="padding-top: 10px;">
                    <span class="Required">*</span>&nbsp;Indicates a required field.                   
                </div>
                <%--END--%>
                <asp:button ID="btnAddOrder" runat="server" Text="Add" OnClientClick="addOrderRow(); return false;" ValidationGroup="ADDORDER" style="display: none;" />
				<asp:button ID="btnCancelAddOrder" runat="server" OnClientClick="clearAddOrderFormControls(); return false;" Text="Cancel" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--EDIT ORDER INFORMATION FORM MODAL/DIALOG--%>
    <div id="divEditOrderInformationForm">
        <asp:UpdatePanel ID="updtpnlEditOrderInformationForm" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldEditOrderIndex" runat="server" Value="0" />
                <div class="OForm">
                    <%--PRODUCT GROUP ID--%>
                    <div class="Row">
                        <div class="Label">Product Group<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlEditProductGroupID" runat="server" AutoPostBack="true" DataTextField="ProductName" DataValueField="ProductGroupID"
                            AppendDataBoundItems="true" OnSelectedIndexChanged="ddlEditProductGroupID_SelectedIndexChanged">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalEditProductGroupID" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="ddlEditProductGroupID" ValidationGroup="EDITORDER" InitialValue="0"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--PRODUCT ID--%>
                    <div class="Row">
                        <div class="Label">Product<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlEditProductID" runat="server" AppendDataBoundItems="true" DataTextField="ProductDescription" DataValueField="ProductID">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalEditProductID" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="ddlEditProductID" ValidationGroup="EDITORDER" InitialValue="0"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--QUANTITY--%>
                    <div class="Row">
                        <div class="Label">Quantity<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEditQuantity" runat="server" Text=""></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqfldvalEditQuantity" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="txtEditQuantity" ValidationGroup="EDITORDER" InitialValue="0"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--UNIT PRICE--%>
                    <div class="Row">
                        <div class="Label">Unit Price<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEditUnitPrice" runat="server" Text=""></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqfldvalEditUnitPrice" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="txtEditUnitPrice" ValidationGroup="EDITORDER" InitialValue="0"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                </div>    
                <%--FOOTER - REQUIRED FIELDS INDICATOR--%>
                <div class="RequiredIndicator" style="padding-top: 10px;">
                    <span class="Required">*</span>&nbsp;Indicates a required field.                   
                </div>
                <%--END--%>
                <asp:button ID="btnEditOrder" runat="server" Text="Udpate" OnClientClick="updateOrderRow(); return false;" ValidationGroup="EDITORDER" style="display: none;" />
				<asp:button ID="btnCancelEditOrder" runat="server" OnClientClick="clearEditOrderFormControls(); return false;" Text="Cancel" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <div id="accordionMainForm" style="margin-top:10px;">
        <div>
			<h3><a href="#">Account Information</a></h3>
            <div class="OForm AccordionPadding">
                <asp:UpdatePanel ID="upnlAccountInformation" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <%--BRAND SOURCE--%>
                        <div class="Row">
                            <div class="Label">Brand Source<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlBrandSource" runat="server" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlBrandSource_SelectedIndexChanged" AppendDataBoundItems="true">
                                    <asp:ListItem Text="Mirion" Value="2" Selected="True" />
                                    <asp:ListItem Text="IC Care" Value="3" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="reqfldvalBrandSource" runat="server" ControlToValidate="ddlBrandSource" CssClass="InlineError"
                                ErrorMessage="Brand Source is required." Text="Brand Source is required." Display="Dynamic" InitialValue="0" ValidationGroup="CRMREGISTRATION">
                                </asp:RequiredFieldValidator>                    
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <%--DEALER (ICCARE only)--%>
                        <div id="divDealerInformation" runat="server" class="Row" visible="false">
                            <div class="Label">Dealer<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlDealer" runat="server" DataTextField="DealerName" DataValueField="DealerID" AppendDataBoundItems="true">
                                    <asp:ListItem Text="-Select Dealer-" Value="0" Selected="True" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="reqfldvalDealer" runat="server" ControlToValidate="ddlDealer" CssClass="InlineError"
                                ErrorMessage="Dealer is required." Text="Dealer is required." Display="Dynamic" InitialValue="0" ValidationGroup="CRMREGISTRATION">
                                </asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <%--ACCOUNT NAME--%>
                        <div class="Row">
                            <div class="Label">Account Name<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox ID="txtAccountName" runat="server" Text="" CssClass="Size Medium"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="reqfldvalAccountName" runat="server" ControlToValidate="txtAccountName" CssClass="InlineError"
                                ErrorMessage="Account Name is required." Text="Account Name is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                                </asp:RequiredFieldValidator>                    
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <%--COMPANY NAME--%>
                        <div class="Row">
                            <div class="Label">Company Name<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox ID="txtCompanyName" runat="server" Text="" CssClass="Size Medium"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="reqfldvalCompanyName" runat="server" ControlToValidate="txtCompanyName" CssClass="InlineError"
                                ErrorMessage="Company Name is required." Text="Company Name is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                                </asp:RequiredFieldValidator>                    
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <%--REFERRAL--%>
                        <div class="Row">
                            <div class="Label">Referral<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlReferral" runat="server" AppendDataBoundItems="true" DataValueField="SalesRepDistID" DataTextField="SalesCompanyName">
                                    <asp:ListItem Text="-Select Referral Code-" Value="0" Selected="True" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="reqfldvalReferral" runat="server" ControlToValidate="ddlReferral" CssClass="InlineError"
                                ErrorMessage="Referral is required." Text="Referral is required." Display="Dynamic" InitialValue="0" ValidationGroup="CRMREGISTRATION">
                                </asp:RequiredFieldValidator>                   
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <%--INDUSTRY TYPE--%>
                        <div class="Row">
                            <div class="Label">Industry Type<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlIndustryType" runat="server" AppendDataBoundItems="true" DataValueField="IndustryID" DataTextField="IndustryName">
                                    <asp:ListItem Text="-Select Industry-" Value="0" Selected="True" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="reqfldvalIndustryType" runat="server" ControlToValidate="ddlIndustryType" CssClass="InlineError"
                                ErrorMessage="Industry is required." Text="Industry is required." Display="Dynamic" InitialValue="0" ValidationGroup="CRMREGISTRATION">
                                </asp:RequiredFieldValidator>                   
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <%--CUSTOMER TYPE--%>
                        <div class="Row">
                            <div class="Label">Customer Type<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlCustomerType" runat="server" DataValueField="CustomerTypeID" DataTextField="CustomerTypeName" AppendDataBoundItems="true">
                                    <asp:ListItem Text="-Select Customer Type-" Value="0" Selected="True" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="reqfldvalCustomerType" runat="server" ControlToValidate="ddlCustomerType" CssClass="InlineError"
                                ErrorMessage="Customer Type is required." Text="Customer Type is required." Display="Dynamic" InitialValue="0" ValidationGroup="CRMREGISTRATION">
                                </asp:RequiredFieldValidator>                 
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <%--UNIX CUSTOMER TYPE (Industry)--%>
                        <div class="Row">
                            <div class="Label">Unix Customer Type:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlUnixCustomerType" runat="server" DataValueField="UnixCustomerTypeID" DataTextField="UnixCustomerDescription" AppendDataBoundItems="true">
                                    <asp:ListItem Text="-Select Unix Customer Type-" Value="0" Selected="True" />
                                </asp:DropDownList>                
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <%--SERVICE START & END DATES--%>
                        <div class="Row">
                            <div class="Label">Service Start & End<span class="Required">*</span>:</div>
                            <div  class="Control">
                                <asp:TextBox ID="txtServiceStartDate" runat="server" AutoPostBack ="true" CssClass="Size Small" 
                                OnTextChanged="txtServiceStartDate_TextChanged"></asp:TextBox>                                        
                                TO
                                <asp:TextBox ID="txtServiceEndDate" runat="server" CssClass="Size Small"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="reqfldvalServiceStartDate" runat="server" ControlToValidate="txtServiceStartDate" Display="Dynamic" CssClass="InlineError" 
                                ErrorMessage="Service Start & End Date is required." Text="Service Start & End Date is required." ValidationGroup="CRMREGISTRATION" />
                                <asp:CompareValidator ID="compvalServiceEndDate" runat="server" ControlToCompare="txtServiceStartDate" ControlToValidate="txtServiceEndDate" CssClass="InlineError" 
                                Type="Date" Operator="GreaterThan" ErrorMessage="Invalid Service End Date." ValidationGroup="CRMREGISTRATION"></asp:CompareValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <h3><a href="#">Billing Method Information</a></h3>
            <div class="OForm AccordionPadding">
                <asp:UpdatePanel ID="upnlBillingMethod" runat="server" UpdateMode="Conditional">
                    <Triggers>
                        <asp:AsyncPostBackTrigger controlid="ddlBrandSource" eventname="SelectedIndexChanged" />
                    </Triggers>
                    <ContentTemplate>
                        <%--BILLING FREQUENCY--%>
                        <div class="Row">
                            <div class="Label">Billing Frequency<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlBillingFrequency" runat="server" AppendDataBoundItems="true">
                                    <asp:ListItem Text="-Select Billing Frequency-" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Quarterly" Value="1" />
	                                <asp:ListItem Text="Yearly" Value="2" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="reqfldvalBillingFrequency" runat="server" ControlToValidate="ddlBillingFrequency" CssClass="InlineError"
                                ErrorMessage="Billing Frequency is required." Text="Billing Frequency is required." Display="Dynamic" InitialValue="0" ValidationGroup="CRMREGISTRATION">
                                </asp:RequiredFieldValidator>                 
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <%--BILLING METHOD--%>
                        <div class="Row">
                            <div class="Label">Billing Method<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:RadioButton ID="rdobtnPONumber" runat="server" Text="Purchase Order" AutoPostBack="true" GroupName="rdobtnPaymentMethod" Checked="true" />
                                <asp:RadioButton ID="rdobtnCreditCard" runat="server" Text="Credit Card" AutoPostBack="true" GroupName="rdobtnPaymentMethod"  />                 
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <%--PO NUMBER--%>
                        <div id="divPONumber" runat="server" class="Row" visible="false">
                            <div class="Label">PO Number<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:TextBox  ID="txtPONumber" runat="server" Text="" MaxLength="15" ></asp:TextBox>
                                <asp:RequiredFieldValidator ID="reqfldvalPONumber" runat="server" ControlToValidate= "txtPONumber" Display="Dynamic"  
                                ErrorMessage="PO Number is required."  Text="PO Number is required." ValidationGroup="CRMREGISTRATION" CssClass="InlineError" />
                                <asp:RegularExpressionValidator id="regexpvalPONumber" runat="server" Display="Dynamic" CssClass="InlineError" 
                                ControlToValidate="txtPONumber" ValidationExpression="[0-9a-zA-Z?\s~!\-@#$%^&amp;*/]{1,15}"
                                ErrorMessage="PO Number is max 15 characters or numerics." Text="PO Number is max 15 characters or numerics"
                                ValidationGroup="CRMREGISTRATION" />
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <%--END--%>
                        <div id="divCreditCardInformation" runat="server" class="Row" visible="false">
                            <%--CREDIT CARD TYPE--%>
                            <div class="Row">
                                <div class="Label">Credit Card Type<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:RadioButtonList ID="rdobtnlstCreditCardType" runat="server" RepeatColumns="4"> 
                                        <asp:ListItem id="option1" runat="server" Value="1" Text = "Visa" Selected="True"></asp:ListItem>
                                        <asp:ListItem id="option2" runat="server" Value="2" Text = "MasterCard"></asp:ListItem>
                                        <asp:ListItem id="option3" runat="server" Value="3" Text = "Discover"></asp:ListItem>
                                        <asp:ListItem id="option4" runat="server" Value="4" Text = "Amex"></asp:ListItem>
                                    </asp:RadioButtonList>
                                    <asp:RequiredFieldValidator ID="reqfldvalCreditCardType" runat="server" ControlToValidate="rdobtnlstCreditCardType" CssClass="InlineError"
                                    ErrorMessage="Credit Card Type is required." Text="Credit Card Type is required." Display="Dynamic" InitialValue="0" ValidationGroup="CRMREGISTRATION">
                                    </asp:RequiredFieldValidator>          
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--NAME ON CARD--%>
                            <div class="Row">
                                <div class="Label">Name On Card<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtNameOnCard" runat="server" Text="" CssClass="Size Medium"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalNameOnCard" runat="server" ControlToValidate="txtNameOnCard" CssClass="InlineError"
                                    ErrorMessage="Name On Card is required." Text="Name On Card is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                                    </asp:RequiredFieldValidator>          
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--CREDIT CARD NUMBER--%>
                            <div class="Row">
                                <div class="Label">Credit Card Number<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtCreditCardNumber" runat="server" Text="" CssClass="Size Medium" onblur="javascript:testCreditCard(); return false;"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalCreditCardNumber" runat="server" ControlToValidate="txtCreditCardNumber" CssClass="InlineError"
                                    ErrorMessage="Credit Card Number is required." Text="Credit Card Number is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                                    </asp:RequiredFieldValidator>          
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--EXPIRATION DATE (MONTH/YEAR)--%>
                            <div class="Row">
                                <div class="Label">Expiration Date<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlCCExpirationMonth" runat="server" AutoPostBack="false">
                                        <asp:ListItem Text="-Month-" Value="0" Selected="True" />
                                        <asp:ListItem Text="January" Value="1" />
	                                    <asp:ListItem Text="February" Value="2" />
	                                    <asp:ListItem Text="March" Value="3" />
	                                    <asp:ListItem Text="April" Value="4" />
	                                    <asp:ListItem Text="May" Value="5" />
	                                    <asp:ListItem Text="June" Value="6" />
	                                    <asp:ListItem Text="July" Value="7" />
	                                    <asp:ListItem Text="August" Value="8" />
	                                    <asp:ListItem Text="September" Value="9" />
	                                    <asp:ListItem Text="October" Value="10" />
	                                    <asp:ListItem Text="November" Value="11" />
	                                    <asp:ListItem Text="December" Value="12" />
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalExpirationMonth" runat="server" ControlToValidate="ddlCCExpirationMonth" CssClass="InlineError"
                                    ErrorMessage="Expiration Month is required." Text="Expiration Month is required." Display="Dynamic" InitialValue="0" ValidationGroup="CRMREGISTRATION">
                                    </asp:RequiredFieldValidator> 
                                    <asp:DropDownList ID="ddlCCExpirationYear" runat="server" AutoPostBack="false">
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalExpirationYear" runat="server" ControlToValidate="ddlCCExpirationYear" CssClass="InlineError"
                                    ErrorMessage="Expiration Year is required." Text="Expiration Year is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                                    </asp:RequiredFieldValidator>          
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--VERIFICATION CODE--%>
                            <div class="Row">
                                <div class="Label">CVC<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtCVC" runat="server" Text="" CssClass="Size XXSmall"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalCVC" runat="server" ControlToValidate="txtCVC" CssClass="InlineError" 
                                    ValidationGroup="CRMREGISTRATION" Display="Dynamic" ErrorMessage="CVC is required." Text="CVC is required.">
                                    </asp:RequiredFieldValidator>      
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <h3><a href="#">Account Administrator Information</a></h3>
            <div class="OForm AccordionPadding">
                <%--USERNAME--%>
                <div class="Row">
                    <div class="Label">Username<span class="Required">*</span>:</div>
                    <div class="Control">
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="Size Medium" onkeyup="OnChange(this);" onblur="ShowAvailability();"></asp:TextBox>&nbsp;<span id="status"></span>
                        <asp:RequiredFieldValidator ID="reqfldvalUsername" runat="server" ControlToValidate="txtUsername" CssClass="InlineError"
                        ErrorMessage="Username is required." Text="Username is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                        </asp:RequiredFieldValidator>          
                    </div>
                    <div class="Clear"></div>
                </div>
                <%--END--%>
                <%--SECURITY QUESTION--%>
                <div class="Row">
                    <div class="Label">Security Question<span class="Required">*</span>:</div>
                    <div class="Control">
                        <asp:DropDownList ID="ddlSecurityQuestion" runat="server" DataValueField="SecurityQuestionID" DataTextField="SecurityQuestionText" AppendDataBoundItems="true">
                            <asp:ListItem Text="-Select Security Question-" Value="0" Selected="True" />
	                    </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="reqfldvalSecurityQuestion" runat="server" ControlToValidate="ddlSecurityQuestion" CssClass="InlineError"
                        ErrorMessage="Security Question is required." Text="Security Question is required." Display="Dynamic" InitialValue="0" ValidationGroup="CRMREGISTRATION">
                        </asp:RequiredFieldValidator>          
                    </div>
                    <div class="Clear"></div>
                </div>
                <%--END--%>
                <%--SECURITY ANSWER--%>
                <div class="Row">
                    <div class="Label">Security Answer<span class="Required">*</span>:</div>
                    <div class="Control">
                        <asp:TextBox ID="txtSecurityAnswer" runat="server" Text="" CssClass="Size Large"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="reqfldvalSecurityAnswer" runat="server" ControlToValidate="txtSecurityAnswer" CssClass="InlineError"
                        ErrorMessage="Security Answer is required." Text="Security Answer is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                        </asp:RequiredFieldValidator>          
                    </div>
                    <div class="Clear"></div>
                </div>
                <%--END--%>
                <%--FIRST NAME--%>
                <div class="Row">
                    <div class="Label">First Name<span class="Required">*</span>:</div>
                    <div class="Control">
                        <asp:DropDownList ID="ddlPrefix" runat="server">
                            <asp:ListItem Text="" Value="" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Dr." Value="Dr."></asp:ListItem>
                            <asp:ListItem Text="Miss" Value="Miss"></asp:ListItem>
                            <asp:ListItem Text="Mr." Value="Mr."></asp:ListItem>
                            <asp:ListItem Text="Mrs." Value="Mrs."></asp:ListItem>
                            <asp:ListItem Text="Ms." Value="Ms."></asp:ListItem>
                        </asp:DropDownList>
                        <asp:TextBox ID="txtFirstName" runat="server" Text="" CssClass="Size Medium"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="reqfldvalFirstName" runat="server" ControlToValidate="txtFirstName" CssClass="InlineError"
                        ErrorMessage="First Name is required." Text="First Name is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                        </asp:RequiredFieldValidator>          
                    </div>
                    <div class="Clear"></div>
                </div>
                <%--END--%>
                <%--LAST NAME--%>
                <div class="Row">
                    <div class="Label">Last Name<span class="Required">*</span>:</div>
                    <div class="Control">
                        <asp:TextBox ID="txtLastName" runat="server" Text="" CssClass="Size Medium"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="reqfldvalLastName" runat="server" ControlToValidate="txtLastName" CssClass="InlineError"
                        ErrorMessage="Last Name is required." Text="Last Name is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                        </asp:RequiredFieldValidator>          
                    </div>
                    <div class="Clear"></div>
                </div>
                <%--END--%>
                <%--GENDER--%>
                <div class="Row">
                    <div class="Label">Gender:</div>
                    <div class="Control">
                        <asp:RadioButtonList ID="rdobtnlstGender" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Male" Value="M" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Female" Value="F"></asp:ListItem>
                        </asp:RadioButtonList>         
                    </div>
                    <div class="Clear"></div>
                </div>
                <%--END--%>
                <%--E-MAIL--%>
                <div class="Row">
                    <div class="Label">E-mail<span class="Required">*</span>:</div>
                    <div class="Control">
                        <asp:TextBox ID="txtEmail" runat="server" Text="" CssClass="Size Medium"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="reqfldvalEmail" runat="server" ControlToValidate="txtEmail" CssClass="InlineError"
                        ErrorMessage="E-mail is required." Text="E-mail is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                        </asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="regexpvalEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="CRMREGISTRATION" CssClass="InlineError"
                        ValidationExpression="[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b" 
                        Display="Dynamic" ErrorMessage="E-mail is incorrectly formatted." Text="E-mail is incorrectly formatted." />          
                    </div>
                    <div class="Clear"></div>
                </div>
                <%--END--%>
                <%--TELEPHONE--%>
                <div class="Row">
                    <div class="Label">Telephone<span class="Required">*</span>:</div>
                    <div class="Control">
                        <asp:TextBox ID="txtTelephone" runat="server" Text="" CssClass="Size Medium"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="reqfldvalTelephone" runat="server" ControlToValidate="txtTelephone" CssClass="InlineError"
                        ErrorMessage="Telephone is required." Text="Telephone is required." Display="Dynamic" InitialValue="" ValidationGroup="CRMREGISTRATION">
                        </asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="regexpvalTelephone" runat="server" ControlToValidate="txtTelephone" ValidationGroup="CRMREGISTRATION" CssClass="InlineError"
                        ValidationExpression="^(1?(-?\d{3})-?)?(\d{3})(-?\d{4})$" Display="Dynamic" ErrorMessage="Telephone is incorrectly formatted." Text="Telephone is incorrectly formatted." />        
                    </div>
                    <div class="Clear"></div>
                </div>
                <%--END--%>
                <%--FAX--%>
                <div class="Row">
                    <div class="Label">Fax:</div>
                    <div class="Control">
                        <asp:TextBox ID="txtFax" runat="server" Text="" CssClass="Size Medium"></asp:TextBox>
                        <asp:RegularExpressionValidator ID="regexpvalFax" runat="server" ControlToValidate="txtFax" ValidationGroup="CRMREGISTRATION" CssClass="InlineError"
                        ValidationExpression="^(1?(-?\d{3})-?)?(\d{3})(-?\d{4})$" Display="Dynamic" ErrorMessage="Fax is incorrectly formatted." Text="Fax is incorrectly formatted." />           
                    </div>
                    <div class="Clear"></div>
                </div>
                <%--END--%>
            </div>
            <h3><a href="#">Billing &amp; Shipping Information</a></h3>
            <div class="OForm AccordionPadding">
                <asp:UpdatePanel ID="updtpnlBillingAndShippingInformation" runat="server" UpdateMode="Conditional">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lnkbtnAddLocation" EventName="Click" />
                    </Triggers>
                    <ContentTemplate>
                        <div style="margin: 0 auto; width: 800px;">
                            <%--TOOLBAR (ADD LOCATION INFORMATION)--%>
                            <div id="divAddLocationToolbar" class="OToolbar JoinTable">
                                <ul> 
                                    <li>
                                        <asp:LinkButton ID="lnkbtnAddLocation" runat="server" CssClass="Icon Add"  
							            OnClientClick="openDialog('divAddLocationInformationForm'); return false;">Add Location</asp:LinkButton>
                                    </li>
                                </ul>
                            </div>
                            <%--END--%>
                            <div id="divLocationsHTMLTable">
                                <%--LOCATIONS HTML DATATABLE--%>
                                <div id="divDynamicLocationsHTMLTable">
                                    <table id="locationsDataTable" class="OTable">
                                        <tr>
                                            <th>Billing Address</th>
                                            <th>Shipping Address</th>
                                            <th style="width: 200px;">&nbsp;</th>
                                        </tr>
                                    </table>
                                </div>
                                <%--END--%>
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <h3><a href="#">Order Information</a></h3>
            <div class="OForm AccordionPadding">
                <asp:UpdatePanel ID="updtpnlOrderInformation" runat="server" UpdateMode="Conditional">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lnkbtnAddOrder" EventName="Click" />
                    </Triggers>
                    <ContentTemplate>
                        <div style="margin: 0 auto; width: 800px;">
                            <%--TOOLBAR (ADD ORDER INFORMATION)--%>
                            <div id="divAddOrderToolbar" class="OToolbar JoinTable">
                                <ul> 
                                    <li>
                                        <asp:LinkButton ID="lnkbtnAddOrder" runat="server" CssClass="Icon Add"  
							            OnClientClick="openDialog('divAddOrderInformationForm'); return false;">Add Order</asp:LinkButton>
                                    </li>
                                </ul>
                            </div>
                            <%--END--%>
                            <div id="divOrdersHTMLTable">
                                <%--ORDERS HTML DATATABLE--%>
                                <div id="divDynamicOrdersHTMLTable">
                                    <table id="ordersDataTable" class="OTable">
                                        <tr>
                                            <th>Product Name</th>
                                            <th>Badge Details</th>
                                            <th>Quantity</th>
                                            <th>Unit Price</th>
                                            <th style="width: 200px;">&nbsp;</th>
                                        </tr>
                                    </table>
                                </div>
                                <%--END--%>
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
    <div class="Buttons">
        <div class="ButtonHolder">
            <asp:Button ID="btnAddCRMAccount" runat="server" CssClass="OButton" Text="Add CRM Account" OnClick="btnAddCRMAccount_Click" />
            <asp:Button ID="btnCancelCRMAccount" runat="server" CssClass="OButton" Text="Cancel" OnClick="btnCancelCRMAccount_Click" />
        </div>
    </div>
</asp:Content>

