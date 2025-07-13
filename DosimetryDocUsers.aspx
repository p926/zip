<%@ Page Title="Manage Dosimetry Documents" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_DosimetryDocUsers" Codebehind="DosimetryDocUsers.aspx.cs" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" type="text/css" href="../css/rad-controls/RadGrid.css" />
    <style type="text/css">
        .validator {
            width:16px;
            height: 16px;
            display: inline-block;
            vertical-align: middle;
            margin-left: 5px;
        }
        .validating {
            background: url('/images/throbber.gif');
        }
        .valid {
            background: url('/images/Success.png');
        }
        .invalid {
            background: url('/images/Fail.png');
        }

        /* CSS for Right Aligning "Clear Filter" link. */
        li.RightAlign
        {
            float: right;
            padding-right: 10px;
        }

        /* Display CSS Class for Loading Animation. */
        #updateProgressBar
        {
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
    </style>
    <script type="text/javascript">
        var emailIsValid = false;

        function pageLoad(sender, args) {
            $(document).ready(function () {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(aJaxFunctions);
                aJaxFunctions();
            });
        }

        function aJaxFunctions() {
            $('#inviteUser').hide();

            // Opens Resend and Delete Modal/Dialogs
            $('#hyprlnkResendSetupEmail').click(function () {
                var email = $('#email').val();
                $('#lblResendSetupEmail').html(email);
                openDialog('divResendSetupEmail');
            });

            $('#hyprlnkDeleteUser').click(function () {
                var email = $('#email').val();
                $('#lblDeleteUser').html(email);
                openDialog('divDeleteUser');
            });

            // Modal/Dialog for Resend Set-Up E-mail.
            $('#divResendSetupEmail').dialog({
                modal: true,
                autoOpen: false,
                width: 400,
                resizable: false,
                title: "Resend Set-up Email",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "OK": function () {
                        $('#<%= btnYes.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            // Modal/Dialog for Delete User.
            $('#divDeleteUser').dialog({
                modal: true,
                autoOpen: false,
                width: 400,
                resizable: false,
                title: "Delete User",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "OK": function () {
                        $('#<%= btnOK.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            // Modal/Dialog for Excel Export (Doc User Activation History).
            $('#divExportToExcel').dialog({
                modal: true,
                autoOpen: false,
                width: 925,
                resizable: false,
                title: "Doc Users Activation History",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "Close": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            function validateAccount(control) {
                var account = $(control).val();
                var appID = $(control).parent().parent().find('.system').val();

                var validator = $(this).parent().find('.validator');
                validator.removeClass('invalid').removeClass('valid').removeClass('validating');

                if (account == "") {
                    control.data('valid', valid);
                    validator.addClass('valid');
                    return;
                }

                validator.addClass('validating');
                $.ajax({
                    data: {
                        action: 'validate-account',
                        appID: appID,
                        account: account
                    },
                    context: control,
                    dataType: 'json'
                }).done(function (valid) {

                    var validator = $(this).parent().find('.validator');
                    validator.removeClass('invalid').removeClass('valid').removeClass('validating');

                    if (valid) {
                        validator.addClass('valid');
                    } else {
                        validator.addClass('invalid');
                    }
                    $(control).addClass('danger').data('valid', valid);
                });
            }

            function validateEmail() {
                var email = $('#email').val();

                if (email !== "" && email === $('#existingEmail').val()) {
                    setEmailValidated(0);
                    return;
                }

                $.ajax({
                    data: {
                        action: 'validate-email',
                        email: email
                    },
                    dataType: 'json'
                }).done(function (data) {
                    console.log(data);

                    if (email === "") {
                        setEmailValidated(1);
                        return;
                    }

                    setEmailValidated(data.state);
                });
            }

            function setEmailValidated(validated) {
                var control = $('.validateEmail');
                control.removeClass('valid').removeClass('invalid');

                switch (validated) {
                    case 0:
                        control.addClass('valid');
                        break;
                    case 1:
                        control.addClass('invalid');
                        break;
                    case 2:
                        control.addClass('invalid');
                        break;
                    case 3:
                        control.addClass('invalid');
                        // In case of 3, then show the Message w/ Checkbox Invitation.
                        break;
                    default:
                        break;
                }
            }

            function loadAccounts(email) {
                // save the changes.
                $.ajax({
                    data: {
                        action: 'get-accounts',
                        email: email
                    },
                    dataType: 'json'
                }).done(function (data) {
                    resetRows();
                    $(data).each(function (index, row) {
                        addRow(row.AppID, row.AppName, row.Account, row.Active);
                    });
                    addRow(1, '', '', true);
                });
            }

            function resetRows() {
                var rows = $("#accounts tr:gt(0)").remove();
            }

            function addRow(app, appName, acc, active) {
                if (acc == "") {
                    // create new record.
                    $('#accounts').append(
                        '<tr><td><select class="system"><option value="1">GDS</option><option value="2">Instadose</option></select></td>' +
                        '<td><input type="text" value="" placeholder="account#" class="account Size XSmall" /><div class="validator"></div></td>' +
                        '<td><input type="checkbox" value="1" class="active" disabled="disabled" /></td></tr>');

                } else {
                    // create an existing record.
                    $('#accounts').append(
                        '<tr><td><input type="hidden" class="system" />' + appName + '</td>' +
                        '<td><input type="hidden" value="" class="account" />' + acc + '</td>' +
                        '<td><input type="checkbox" value="1" class="active" /></td></tr>');
                }

                var row = $('#accounts tr:last');
                $(row).find('.account').val(acc).blur(function () {
                    validateAccount(this);
                });
                $(row).find('.system').val(app).change(function () {
                    $(this).parent().parent().find('.account').trigger('blur');
                });
                $(row).find('.active').prop('checked', active);
            }

            function buildAccountsList() {
                // loop through each of the table rows...
                var rows = $("#accounts tr:gt(0)"); // skip the header row

                var accounts = "";
                rows.each(function (index) {
                    var app = $(this).closest('tr').find('.system').val();
                    var acc = $(this).closest('tr').find('.account').val();
                    var active = $(this).closest('tr').find('.active').is(':checked');
                    accounts += app + ',' + acc + ',' + active + '|';
                });

                return accounts.substring(0, accounts.length - 1);
            }

            $(function () {
                $("#email").blur(function () {
                    // validate
                    validateEmail();

                    var email = $('#email').val();

                    if (email === "") {
                        alert("Please enter an e-mail address.");
                    }
                    else {
                        PageMethods.IsUserInDosimetryDocuments(email, OnSuccess_Invite, OnError);

                        function OnSuccess_Invite(userExists) {
                            if (userExists === false) {
                                // Show Invitation Message and Checkbox.
                                $("#inviteUser").slideDown('fast');
                            }
                            else {
                                // Hide Invitation Message and Checkbox.
                                $("#inviteUser").slideUp('fast');
                            }
                        }

                        function OnError(userExists) {
                            alert('An error occurred.');
                        }
                    }
                });

                $('#userDialog').dialog({
                    autoOpen: false,
                    resizable: false,
                    modal: true,
                    width: 550,
                    title: 'User Management',
                    buttons: {
                        "Save": function () {
                            if ($('#email').val() == "") {
                                alert('The e-mail address is required.');
                                return;
                            }

                            $('#chkbxInviteUser').click(function () {
                                $(this).is(":checked");
                            });

                            if (!($('#chkbxInviteUser').is(":checked"))) {
                                if ($('.invalid').length > 0) {
                                    alert('Please correct all errors before saving.');
                                    return;
                                }
                            }

                            // save the changes.
                            $.ajax({
                                data: {
                                    action: 'save',
                                    active: $('#active').is(":checked"),
                                    existingEmail: $('#existingEmail').val(),
                                    email: $('#email').val(),
                                    accounts: buildAccountsList()
                                },
                                dataType: 'json'
                            }).done(function (data) {
                                // save the accounts to the user.
                                if (data != "true") {
                                    alert("A error occurred while saving: " + data.error);
                                    return;
                                }
                                else {
                                    alert("Changes have been saved successfully.");
                                }
                                $('#userDialog').dialog("close");
                                $('#<%= btnRebind.ClientID %>').click();
                            });
                        },
                        "Cancel": function () {
                            $(this).dialog("close");
                            $('#inviteUser').hide();
                            $('#chkbxInviteUser').not(":checked");
                        }
                    },
                    close: function () {
                        $('.ui-overlay').fadeOut();
                        $('#inviteUser').hide();
                        $('#chkbxInviteUser').not(":checked");
                    }
                }).parent().appendTo($("form"));

                // From Edit Icon in RadGrid, opens Account information for editing.
                $('.editUser').click(function () {
                    setEmailValidated(0);   // If editing User, their e-mail is already "Valid".
                    $("#inviteUser").hide();
                    var email = $(this).data("email");
                    var authUserID = $(this).data("authuserid");
                    var authUserActive = $(this).data("active");
                    loadAccounts(email);
                    $('#existingEmail').val(email);
                    $('#email').val(email);
                    // Checkes to see if AuthUser is Active/Inactive and Checks/Unchecks "Active" Checkbox. 
                    if (authUserActive == "Yes") {
                        authUserActive = true;
                    }
                    else {
                        authUserActive = false;
                    }
                    $("#active").attr("checked", authUserActive);

                    PageMethods.ShowHideResendAndDelete(email, authUserID, OnSuccess_Resend, OnError);

                    function OnSuccess_Resend(state) {
                        switch (state) {
                            case 1:
                                document.getElementById('hyprlnkResendSetupEmail').style.display = 'inherit';
                                document.getElementById('hyprlnkDeleteUser').style.display = 'inherit';
                                break;
                            case 2:
                                document.getElementById('hyprlnkResendSetupEmail').style.display = 'none';
                                document.getElementById('hyprlnkDeleteUser').style.display = 'none';
                                break;
                            case 3:
                                document.getElementById('hyprlnkResendSetupEmail').style.display = 'inherit';
                                document.getElementById('hyprlnkDeleteUser').style.display = 'none';
                                break;
                            case 4:
                                document.getElementById('hyprlnkResendSetupEmail').style.display = 'none';
                                document.getElementById('hyprlnkDeleteUser').style.display = 'none';
                                break;
                        }
                    }

                    function OnError(sqID, agCount) {
                        alert('An error occurred.');
                    }

                    $('#userDialog').dialog("open");
                });

                $('.createUser').click(function () {
                    setEmailValidated();
                    resetRows();
                    addRow(1, '', '', true);
                    $('#existingEmail').val('');
                    $('#email').val('');
                    $('#userDialog').dialog("open");
                    // Both Resend and Delete LinkButtons are hidden.
                    document.getElementById('hyprlnkResendSetupEmail').style.display = 'none';
                    document.getElementById('hyprlnkDeleteUser').style.display = 'none';
                });

                $('#addRow').click(function () {
                    addRow(1, '', '', true);
                });
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

        // Check or Uncheck Account Checkboxes based on Activate/Deactive AuthUser option.
        // This functionality is used when CS wants to Activate/Deactivate the AuthUser's E-Mail in the Dosimetry Docs Application/Website.
        function toggleCheckboxes(element) {
            var checkboxes = document.getElementsByClassName("active");
            if (element.checked) {
                for (var i = 0; i < (checkboxes.length - 1); i++) {
                    if (checkboxes[i].type == 'checkbox') {
                        checkboxes[i].checked = true;
                    }
                }
            } else {
                for (var i = 0; i < (checkboxes.length - 1); i++) {
                    console.log(i)
                    if (checkboxes[i].type == 'checkbox') {
                        checkboxes[i].checked = false;
                    }
                }
            }
        }
    </script>
    <telerik:RadCodeBlock ID="RadCodeBlock0" runat="server">
        <script type="text/javascript">
            var column = null;
            function MenuShowing(sender, args) {
                if (column == null) return;
                var menu = sender; var items = menu.get_items();
                if (column.get_dataType() == "System.String") {
                    var i = 0;
                    while (i < items.get_count()) {
                        if (!(items.getItem(i).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            var item = items.getItem(i);
                            if (item != null)
                                item.set_visible(false);
                        }
                        else {
                            var item = items.getItem(i);
                            if (item != null)
                                item.set_visible(true);
                        } i++;
                    }
                }
                if (column.get_dataType() == "System.Int32") {
                    var j = 0; while (j < items.get_count()) {
                        if (!(items.getItem(j).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            var item = items.getItem(j); if (item != null)
                                item.set_visible(false);
                        }
                        else { var item = items.getItem(j); if (item != null) item.set_visible(true); } j++;
                    }
                }
                if (column.get_dataType() == "System.DateTime") {
                    var h = 0;
                    while (h < items.get_count()) {
                        if (!(items.getItem(h).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            var item = items.getItem(h);
                            if (item != null)
                                item.set_visible(false);
                        }
                        else {
                            var item = items.getItem(h);
                            if (item != null)
                                item.set_visible(true);
                        } h++;
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
    <telerik:RadScriptManager ID="rsManager" runat="server" EnablePageMethods="true" />
    <telerik:RadStyleSheetManager ID="rssManager" runat="server"></telerik:RadStyleSheetManager>
    <asp:HiddenField ID="hfAuthUserID" runat="server" Value="0" />
    <asp:HiddenField ID="hfAuthUserActive" runat="server" Value="1" />

    <%--RADAJAXLOADINGPANEL ANIMATION (Enclosed in a HTML Table to Center on RadGrid)--%>
    <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server" SkinID="Default" EnableSkinTransparency="true"
    Transparency="20" BackColor="#FFFFFF" ZIndex="100" BackgroundPosition="Center">
        <table style="width:100%; height:100%;">
            <tr style="height:100%">
                <td style="width: 100%; vertical-align: central; text-align: center;">
                    <asp:Label ID="lblLoading1" runat="server" ForeColor="Black" Text="Loading..." Font-Bold="true" Font-Size="Medium" />
                    <br /><br />
                    <asp:Image ID="imgLoading" runat="server" Width="128px" Height="12px" ImageUrl="../../images/orangebarloader.gif" />
                </td>
            </tr>
        </table>
    </telerik:RadAjaxLoadingPanel>
    <%--END--%>

    <%--RAD AJAX MANAGER - Handles RAD CONTROLS/LOADING PANELS for Ajax Updating--%>
    <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server">  
        <AjaxSettings>
            <%--DOSIMETRY DOC USERS RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgDosimetryUsers">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgDosimetryUsers" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="lnkbtnClearFilters_DosimetryUsers">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgDosimetryUsers" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
            <%--END--%>
            <%--EXPORT TO EXCEL RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgExportToExcel">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgExportToExcel" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="lnkbtnClearFilters_ExportToExcel">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgExportToExcel" LoadingPanelID="RadAjaxLoadingPanel1anandi" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
            <%--END--%>
        </AjaxSettings> 
    </telerik:RadAjaxManager>
    <%--END--%>

    <asp:Button Text="Rebind" ID="btnRebind" OnClick="btnRebind_Click" style="display:none" runat="server" />
    
    <%--CREATE/EDIT DOSIMETRY USER MODAL/DIALOG--%>
    <div id="userDialog">
        <asp:UpdatePanel ID="updtpnlEditUser" runat="server">
            <ContentTemplate>
                <div>
                    <div class="OToolbar JoinTable">
                        <ul>
                            <li>
                                <a href="#" id="hyprlnkResendSetupEmail" class="Icon Email" style="font-weight: bold; color: #FFFFFF; display: none;" title="Resend Account Set-Up E-Mail">Resend Account Set-Up E-Mail</a>
                            </li>
                            <li>
                                <a href="#" id="hyprlnkDeleteUser" class="Icon Remove" style="font-weight: bold; color: #FFFFFF; display: none;" title="Delete User">Delete User</a>
                            </li>
                        </ul>
                    </div>
                    <div class="OTable OForm">
                        <div class="Row">
                            <div class="Label">Active:</div>
                            <div class="Control">
                                <label>
                                    <%--<input type="checkbox" id="active" checked="checked" />--%>
                                    <input type="checkbox" id="active" onchange="toggleCheckboxes(this)" />
                                    (Inactive users cannot log in.)
                                </label>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row">
                            <div class="Label">E-Mail Address<span class="Required">*</span>:</div>
                            <div class="Control">
                                <input type="hidden" id="existingEmail" />
                                <input type="text" id="email" name="email" class="Size Medium" placeholder="john.smith@example.com" />
                                <div class="validator validateEmail"></div>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div id="inviteUser" class="Row">
                            <div class="Label">&nbsp;</div>
                            <div class="Control FormMessage">
                                <p>
                                    <span class="MessageIcon"></span>
                                    <span>
                                        <input type="checkbox" id="chkbxInviteUser" name="chkbxInviteUser" />
                                        Yes, invite user to Dosimetry Documents.
                                    </span>
                                </p>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row">
                            <div class="Label"></div>
                            <div class="Control">
                                <table class="OTable" id="accounts" style="width: 340px;">
                                    <tr>
                                        <th>System</th>
                                        <th>Account</th>
                                        <th>Active</th>
                                    </tr>
                                </table>
                                <ul style="margin-top: -5px; padding-left: 0px;">
                                    <li><a href="#" class="Icon Add" id="addRow">Add Account</a></li>
                                </ul>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Footer">
                            <span class="Required">*</span> indicates a required field.
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--RESEND SET-UP E-MAIL CONFIRMATION MODAL/DIALOG--%>
    <div id="divResendSetupEmail">
        <asp:UpdatePanel ID="updtpnlResendSetupEmail" runat="server">
            <ContentTemplate>
                <div class="OForm">
                    <div class="Row">
                        <div class="Label">E-mail to send to:</div>
                        <div class="Control">
                            <div id="lblResendSetupEmail" class="LabelValue"></div>
                        </div>
                        <div class="Clear"></div>
                    </div>
                </div>
                <asp:button ID="btnYes" runat="server" Text="Yes" style="display: none;" OnClick="btnYes_Click" />
				<asp:button ID="btnNo" runat="server" Text="No" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--DELETE USER CONFIRMATION MODAL/DIALOG--%>
    <div id="divDeleteUser">
        <asp:UpdatePanel ID="updtpnlDeleteUser" runat="server">
            <ContentTemplate>
                <div class="OForm">
                    <div class="Row">
                        <div class="Label Size Large2">This User will be deleted:</div>
                        <div class="Control">
                            <div id="lblDeleteUser" class="LabelValue"></div>
                        </div>
                        <div class="Clear"></div>
                    </div>
                </div>
                <asp:button ID="btnOK" runat="server" Text="OK" style="display: none;" OnClick="btnOK_Click" />
				<asp:button ID="btnCancel" runat="server" Text="Cancel" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--EXPORT TO EXCEL MODAL/DIALOG--%>
    <div id="divExportToExcel">
        <div style="width: 100%;">
            <div class="OToolbar JoinTable">
                <ul>
                    <li>
                        <asp:LinkButton ID="lnkbtnExportToExcel" runat="server" CssClass="Icon Export" onclick="lnkbtnExportToExcel_Click" >Export to Excel</asp:LinkButton>
                    </li>
                    <li class="RightAlign">
                        <asp:LinkButton ID="lnkbtnClearFilters_ExportToExcel" runat="server" OnClick="lnkbtnClearFilters_ExportToExcel_Click" 
                        Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters">Clear Filters</asp:LinkButton>
                    </li>
                    <li class="Clear"></li>
                </ul>
            </div>
            <asp:Panel ID="pnlExportToExcel" runat="server" SkinID="Default">
                <telerik:RadGrid ID="rgExportToExcel" runat="server" 
                CssClass="OTable"
                SkinID="Default"
                AllowMultiRowSelection="false"
                AutoGenerateColumns="false"
                AllowPaging="true"
                AllowSorting="true"
                AllowFilteringByColumn="true"
                ShowStatusBar="false"
                EnableLinqExpressions="false" 
                OnNeedDataSource="rgExportToExcel_NeedDataSource"
                OnItemCommand="rgExportToExcel_ItemCommand"
                OnHTMLExporting="rgExportToExcel_HTMLExporting"
                PageSize="10" Width="99.8%">
                    <PagerStyle Mode="NumericPages" />
                    <GroupingSettings CaseSensitive="false" />
                    <ClientSettings EnableRowHoverStyle="true">
                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                        <Selecting AllowRowSelect="true" />
                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                    </ClientSettings>
                    <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
                    <MasterTableView DataKeyNames="Account" TableLayout="Fixed" AllowMultiColumnSorting="false" AutoGenerateColumns="false">
                        <Columns>
                            <telerik:GridBoundColumn DataField="ApplicationName" UniqueName="ApplicationName" SortExpression="ApplicationName" HeaderText="Application"
                            AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="EqualTo" HeaderStyle-Width="100px" ItemStyle-Width="100px">
                                <FilterTemplate>
                                    <telerik:RadComboBox ID="rcbApplicationName" runat="server" DataValueField="ApplicationName" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("ApplicationName").CurrentFilterValue %>'
                                    OnClientSelectedIndexChanged="ApplicationNameIndexChanged" Width="90px">
                                        <Items>
                                            <telerik:RadComboBoxItem Text="-All-" Value="" />
                                            <telerik:RadComboBoxItem Text="GDS" Value="GDS" />
                                            <telerik:RadComboBoxItem Text="Instadose" Value="Instadose" />
                                        </Items>
                                    </telerik:RadComboBox>
                                    <telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
                                        <script type="text/javascript">
                                            function ApplicationNameIndexChanged(sender, args) {
                                                var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                tableView.filter("ApplicationName", args.get_item().get_value(), "EqualTo");
                                            }
                                        </script>
                                    </telerik:RadScriptBlock>
                                </FilterTemplate>
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Account" UniqueName="Account" SortExpression="Account" HeaderText="Account #" AllowFiltering="true" AllowSorting="true" 
                            AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" HeaderStyle-Width="100px" ItemStyle-Width="100px" FilterControlWidth="60px" />
                            <telerik:GridNumericColumn DataField="Documents" UniqueName="Documents" SortExpression="Documents" HeaderText="Documents" AllowFiltering="true" AllowSorting="true"
                            AutoPostBackOnFilter="true" CurrentFilterFunction="EqualTo" HeaderStyle-Width="100px" ItemStyle-Width="100px" FilterControlWidth="60px" />
                            <telerik:GridNumericColumn DataField="TotalUsers" UniqueName="TotalUsers" SortExpression="TotalUsers" HeaderText="Total Users" AllowFiltering="true" AllowSorting="true"
                            AutoPostBackOnFilter="true" CurrentFilterFunction="EqualTo" HeaderStyle-Width="100px" ItemStyle-Width="100px" FilterControlWidth="60px" />
                            <telerik:GridNumericColumn DataField="ActivatedUsers" UniqueName="ActivatedUsers" SortExpression="ActivatedUsers" HeaderText="Activated Users" AllowFiltering="true" AllowSorting="true"
                            AutoPostBackOnFilter="true" CurrentFilterFunction="EqualTo" HeaderStyle-Width="100px" ItemStyle-Width="100px" FilterControlWidth="60px" />
                            <telerik:GridDateTimeColumn DataField="FirstDocumentAdded" UniqueName="FirstDocumentAdded" SortExpression="FirstDocumentAdded" HeaderText="First Added" AllowFiltering="true" 
                            AllowSorting="true" DataFormatString="{0:MM/dd/yyyy}" HeaderStyle-Width="150px" ItemStyle-Width="150px" FilterControlWidth="115px" CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" />
                            <telerik:GridDateTimeColumn DataField="LastDocumentAdded" UniqueName="LastDocumentAdded" SortExpression="LastDocumentAdded" HeaderText="Last Added" AllowFiltering="true" 
                            AllowSorting="true" DataFormatString="{0:MM/dd/yyyy}" HeaderStyle-Width="150px" ItemStyle-Width="150px" FilterControlWidth="115px" CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" />
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid> 
            </asp:Panel>
        </div>
		<asp:button ID="btnClose" runat="server" Text="No" style="display: none;" />
    </div>
    <%--END--%>

    <div class="FormError" id="divMainFormError" runat="server" visible="false">
        <p><span class="MessageIcon"></span>
	    <strong>Message:</strong>&nbsp;<span id="spnMainFormErrorMessage" runat="server"></span></p>
    </div>
    <div class="FormMessage" id="divMainFormSuccess" runat="server" visible="false">
        <p><span class="MessageIcon"></span>
	    <strong>Message:</strong>&nbsp;<span id="spnMainFormSuccessMessage" runat="server"></span></p>
    </div>

    <%--MAIN PAGE AREA W/ RADGRID--%>
    <div style="width: 100%;">
        <telerik:RadAjaxPanel ID="RadAjaxPanel1" runat="server" SkinID="Default">
                <div class="OToolbar JoinTable">
                    <ul>
                        <li><a href="#" class="Icon Add createUser">Create a User</a></li>
                        <li>
                            <asp:LinkButton ID="lnkbtnViewDocUserActivationHistory" runat="server" CssClass="Icon Report" OnClick="lnkbtnViewDocUserActivationHistory_Click">View Activation History</asp:LinkButton>
                        </li>
                        <%--<li class="RightAlign">
                            <asp:LinkButton ID="lnkbtnClearFilters_DosimetryUsers" runat="server" OnClick="lnkbtnClearFilters_DosimetryUsers_Click" 
                            Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters">Clear Filters</asp:LinkButton>
                        </li>--%>
                        <li class="Clear"></li>
                    </ul>
                </div>
            <telerik:RadGrid ID="rgDosimetryUsers" runat="server" 
            CssClass="OTable"
            SkinID="Default"
            AllowMultiRowSelection="false"
            AutoGenerateColumns="false"
            AllowPaging="true"
            AllowSorting="true"
            AllowFilteringByColumn="true"
            ShowStatusBar="false"
            EnableLinqExpressions="false" 
            OnNeedDataSource="rgDosimetryUsers_NeedDataSource"
            OnItemCommand="rgDosimetryUsers_ItemCommand"
            OnSortCommand="rgDosimetryUsers_SortCommand"
            PageSize="20" Width="99.9%">
                <PagerStyle Mode="NumericPages" />
                <GroupingSettings CaseSensitive="false" />
                <ClientSettings EnableRowHoverStyle="true">
                    <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                    <Selecting AllowRowSelect="true" />
                    <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                </ClientSettings>
                <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
                <MasterTableView DataKeyNames="UserName" TableLayout="Fixed" AllowMultiColumnSorting="false" AutoGenerateColumns="false">
                    <Columns>
                        <telerik:GridBoundColumn DataField="AuthUserID" UniqueName="AuthUserID" Visible="false" />
                        <telerik:GridTemplateColumn HeaderStyle-Width="50px" ItemStyle-Width="50px" AllowFiltering="false">
                            <ItemTemplate>
                                <a href="#" class="Icon Edit editUser" data-email='<%# DataBinder.Eval(Container.DataItem, "UserName") %>' title='<%# DataBinder.Eval(Container.DataItem, "UserName") %>' data-authuserid='<%# DataBinder.Eval(Container.DataItem, "AuthUserID") %>' data-active='<%# DataBinder.Eval(Container.DataItem, "Active") %>'></a>
                            </ItemTemplate>
                            <HeaderStyle HorizontalAlign="Center" CssClass="centeralign" />
                            <ItemStyle HorizontalAlign="Center" CssClass="centeralign" />
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn DataField="UserName" UniqueName="UserName" HeaderText="E-mail Address" AllowFiltering="true" AllowSorting="true" 
                        AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" FilterControlWidth="145px" SortExpression="UserName" />
                        <telerik:GridBoundColumn DataField="SecurityQuestionText" UniqueName="SecurityQuestionText" HeaderText="Security Question" AllowFiltering="true" 
                        AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" FilterControlWidth="145px" SortExpression="SecurityQuestionText" />
                        <telerik:GridBoundColumn DataField="SecurityAnswer1" UniqueName="SecurityAnswer1" HeaderText="Security Answer" AllowFiltering="true" 
                        AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" FilterControlWidth="145px" SortExpression="SecurityAnswer1" />
                        <telerik:GridBoundColumn DataField="Active" UniqueName="Active" HeaderText="Active" AllowFiltering="true" SortExpression="Active" 
                        AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" HeaderStyle-Width="100px" ItemStyle-Width="100px">
                            <FilterTemplate>
                                <telerik:RadComboBox ID="rcbActive" runat="server" DataValueField="Active" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("Active").CurrentFilterValue %>'
                                OnClientSelectedIndexChanged="ActiveIndexChanged" Width="85px">
                                    <Items>
                                        <telerik:RadComboBoxItem Text="-All-" Value="" />
                                        <telerik:RadComboBoxItem Text="Yes" Value="Yes" />
                                        <telerik:RadComboBoxItem Text="No" Value="No" />
                                    </Items>
                                </telerik:RadComboBox>
                                <telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
                                    <script type="text/javascript">
                                        function ActiveIndexChanged(sender, args) {
                                            var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                            tableView.filter("Active", args.get_item().get_value(), "EqualTo");
                                        }
                                    </script>
                                </telerik:RadScriptBlock>
                            </FilterTemplate>
                        </telerik:GridBoundColumn>
                        <telerik:GridDateTimeColumn DataField="CreatedDate" UniqueName="CreatedDate" HeaderText="Created" SortExpression="CreatedDate" AllowFiltering="true" 
                        AllowSorting="true" DataFormatString="{0:MM/dd/yyyy}" HeaderStyle-Width="150px" ItemStyle-Width="150px" FilterControlWidth="115px" />
                        <telerik:GridDateTimeColumn DataField="ModifiedDate" UniqueName="ModifiedDate" HeaderText="Last Modified" SortExpression="ModifiedDate" AllowFiltering="true" 
                        AllowSorting="true" DataFormatString="{0:MM/dd/yyyy}" HeaderStyle-Width="150px" ItemStyle-Width="150px" FilterControlWidth="115px" />
                        <%--<telerik:GridBoundColumn DataField="GroupCount" UniqueName="GroupCount" AllowFiltering="false" HeaderStyle-Width="25px" ItemStyle-Width="25px" />--%>
                    </Columns>
                </MasterTableView>
            </telerik:RadGrid>
        </telerik:RadAjaxPanel>
    </div>
    <%--END--%>
</asp:Content>
