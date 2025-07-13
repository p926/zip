<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_InformationFinder_Details_Device" Codebehind="Device.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<script type="text/javascript">
    $(document).ready(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
        JQueryControlsLoad();
    });

    function JQueryControlsLoad() {
        // Modal/Dialog for Editing Badge Assignment.
        $('#editBadgeAssignmentModal').dialog({
            autoOpen: false,
            width: 475,
            height: 425,
            resizable: false,
            title: "Edit Badge Assignment",
            open: function (type, data) {
                $(this).parent().appendTo("form");
                $('#<%= ddlLocation.ClientID %>').focus();
            },
            buttons: {
                "Save": function () {
                    $('#<%= btnSave.ClientID %>').click();
                    document.getElementById('<%= btnSave.ClientID%>').disabled = true;
                },
                "Cancel": function () {
                    $('#<%= btnCancel.ClientID %>').click();
                    $(this).dialog("close");
                }
            },
            close: function () {
                $('#<%= btnCancel.ClientID %>').click();
                $('.ui-overlay').fadeOut();
            }
        });

        $('#activateDeactivateModal').dialog({
            autoOpen: false,
            width: 450,
            height: 330,
            resizable: false,
            title: "Change Badge Status",
            open: function (type, data) {
                $(this).parent().appendTo("form");
            },
            buttons: {
                "Save": function () {
                    $('#<%= btnActivateDeactivate.ClientID %>').click();
                },
                "Cancel": function () {
                    $('#<%= btnActivateDeactivateCancel.ClientID %>').click();
                    $(this).dialog("close");
                }
            },
            close: function () {
                $('#<%= btnActivateDeactivateCancel.ClientID %>').click();
                $('.ui-overlay').fadeOut();
            }
        });
    }

    function openDialog(id) {
        $('.ui-overlay').fadeIn();
        $('#' + id).dialog("open");
    }

    function closeDialog(id) {
        $('#' + id).dialog("close");
    }
</script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
<act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>
    <%-- BEGIN - MAIN PAGE CONTENT --%>
    <div style="width: 100%;">
        <asp:UpdatePanel ID="updtpnlFormView" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldUserDeviceID" runat="server" Value="" />
                <asp:HiddenField ID="hdnfldUserID" runat="server" Value="" />
                <asp:HiddenField ID="hdnfldLocationID" runat="server" Value="" />
                <asp:HiddenField ID="hdnfldBodyRegionID" runat="server" Value="" />
                <asp:HiddenField ID="hdnfldIsPrimary" runat="server" Value="" />
                <div id="divMainError" class="FormError" runat="server" visible="false">
		            <p><span class="MessageIcon"></span>
		            <strong>Messages:</strong>&nbsp;<span id="mainErrorMsg" runat="server" >An error was encountered.</span></p>
                </div>
                <div id="divMainSuccess" class="FormMessage" runat="server" visible="false">
		            <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong>&nbsp;<span id="mainSuccessMsg" runat="server">Submission of information has been successful.</span></p>
                </div>
                <div><asp:Label ID="lblInformation" runat="server"></asp:Label></div>
                <div id="divEditBadgeAssignmentToolbar" class="OToolbar JoinTable" runat="server">
                    <ul>
                        <li>
                            <asp:LinkButton ID="lnkbtnEditBadgeAssignment" CssClass="Icon Edit" runat="server"
                            CommandName="EditBadgeAssignment" CommandArgument='<%# Eval("SerialNo") %>' 
                            OnClientClick="openDialog('editBadgeAssignmentModal'); return false;" Text="">Edit Badge Assignment</asp:LinkButton>
                        </li>
                        <li>
                            <asp:LinkButton ID="lnkbtnChangeBadgeStatus" runat="server"
                            CommandName="ChangeBadgeStatus" CommandArgument='<%# Eval("SerialNo") %>' 
                            OnClientClick="openDialog('activateDeactivateModal'); return false;"></asp:LinkButton>
                        </li>
                    </ul>
                </div>
                <asp:FormView ID="fvBadgeDetails" runat="server" Width="100%" CssClass="OTable">
                    <ItemTemplate>
                        <table class="OTable">
                            <tr>
                                <th style="width: 100px; text-align: center;">Serial #</th>
                                <th style="width: 100px; text-align: center;">Account #</th>
                                <th style="width: 100px; text-align: center;">Account Name</th>
                                <th style="width: 100px; text-align: center;">Product Type</th>
                                <th style="width: 100px; text-align: center;">Active</th>
                                <th style="width: 100px; text-align: center;">Initialized</th>          
                            </tr>
                            <tr>
                                <td class="CenterAlign">
                                    <asp:Label ID="lblSerialNumber" runat="server" Text='<%# Bind("SerialNumber") %>' Font-Bold="true" />
                                </td>
                                <td class="CenterAlign">
                                    <asp:Label ID="lblAccountID" runat="server" Text='<%# Bind("AccountID") %>' Font-Bold="true" />
                                </td>
                                <td class="CenterAlign">
                                    <asp:Hyperlink ID="hyprlnkAccountName" runat="server" Text='<%# Bind("AccountName") %>' NavigateUrl='<%# Bind("AccountID", "Account.aspx?ID={0}#Device_tab") %>' Font-Bold="true" />
                                </td>
                                <td class="CenterAlign">
                                    <asp:Label ID="lblProductName" runat="server" Text='<%# Bind("ProductName") %>' Font-Bold="true" />
                                </td>
                                <td class="CenterAlign">
                                    <asp:Label ID="lblIsActive" runat="server" CssClass='<%# Eval("AccountDeviceActive", "lblActive{0}") %>' Text='<%# YesNo(Eval("AccountDeviceActive")) %>' />
                                </td>
                                <td class="CenterAlign">
                                    <asp:Label ID="lblIsIntialized" runat="server" CssClass='<%# Eval("FormatIsInitialized", "lblActive{0}") %>' Text='<%# YesNo(Eval("FormatIsInitialized")) %>' />
                                </td>
                            </tr>
                        </table>
                        <table class="OTable">
                            <tr>
                                <td class="Label" style="text-align: left; width: 100px;">Username:</td>
                                <td style="width: 350px;">
                                    <asp:Label ID="lblUserName" runat="server" Text='<%# Bind("UserName") %>' />
                                </td>
                                <td class="Label" style="text-align: left; width: 150px;">Firmware Version:</td>
                                <td>
                                    <asp:Label ID="lblFirmwareVersion" runat="server" Text='<%# Bind("FirmwareVersion") %>' />
                                </td>
                            </tr>
                            <tr>
                                <td class="Label" style="text-align: left;">Full Name:</td>
                                <td>
                                    <asp:Hyperlink ID="lblFullName" runat="server" Text='<%# Bind("FullName") %>' NavigateUrl='<%# String.Format("UserMaintenance.aspx?AccountID={0}&UserID={1}", Eval("AccountID"), Eval("UserID")) %>' Target="_blank" />
                                </td>
                                <td class="Label" style="text-align: left;">Calibration Version:</td>
                                <td>
                                    <asp:Label ID="lblCalibrationVersion" runat="server" Text='<%# Bind("CalibrationVersion") %>' />
                                </td>
                            </tr>
                            <tr>
                                <td class="Label" style="text-align: left;">Location:</td>
                                <td>
                                    <asp:Label ID="lblLocation" runat="server" Text='<%# Bind("LocationName") %>' />
                                </td>
                                <td class="Label" style="text-align: left;">Failed Calibration:</td>
                                <td>
                                    <asp:Label ID="lblFailedCalibration" runat="server" CssClass='<%# Eval("FailedCalibration", "lblActive{0}") %>' Text='<%# YesNo(Eval("FailedCalibration")) %>' />
                                </td>
                            </tr>
                            <tr>
                                <td class="Label" style="text-align: left;">Body Region:</td>
                                <td>
                                    <asp:Label ID="lblBodyRegion" runat="server" Text='<%# Bind("BodyRegionName") %>' />
                                </td>
                                <td class="Label" style="text-align: left;">Is Primary Device:</td>
                                <td>
                                    <asp:Label ID="lblIsPrimary" runat="server" CssClass='<%# Eval("IsPrimary", "lblActive{0}") %>' Text='<%# YesNo(Eval("IsPrimary")) %>' />
                                </td>
                            </tr>
                        </table>
                        <table style="width: 100%;"><tr><td><hr /></td></tr></table>
                        <table class="OTable">
				            <tr>
					            <td class="Label" style="text-align: left; width: 150px;">Manufacture Date:</td>
					            <td style="width: 300px;">
                                    <asp:Label ID="lblManufactureDate" runat="server" Text='<%# Bind("ManufactureDate", "{0:d}") %>' />
					            </td>
					            <td class="Label" style="text-align: left; width: 150px;">Service Start Date:</td>
					            <td>
						            <asp:Label ID="lblServiceStartDate" runat="server" Text='<%# Bind("ServiceStartDate", "{0:d}") %>' />       
					            </td>
				            </tr>
                            <tr>
					            <td class="Label" style="text-align: left; width: 150px;">Expiration Date:</td>
					            <td style="width: 300px;">
                                    <asp:Label ID="lblExpirationDate" runat="server" Text='<%# Bind("ExpirationDate", "{0:d}") %>' />
					            </td>
					            <td class="Label" style="text-align: left; width: 150px;">Service End Date:</td>
					            <td>
						            <asp:Label ID="lblServiceEndDate" runat="server" Text='<%# Bind("ServiceEndDate", "{0:d}") %>' />          
					            </td>
				            </tr>
                            <tr>
					            <td class="Label" style="text-align: left;">Deactivation Date:</td>
                                <td colspan="3">
                                    <asp:Label ID="lblDeactivationDate" runat="server" Text='<%# Bind("DeactivateDate") %>' />
                                </td>
				            </tr>
					            <td class="Label" style="text-align: left;" colspan="4">Deactivation Reason:</td>
				            </tr>
                            <tr>
                                <td colspan="4">
                                    <asp:Label ID="lblDeactivationReason" runat="server" Text='<%# Bind("DeactivationReason") %>' />
                                </td>
				            </tr>
			            </table>
		            </ItemTemplate>
                    <EmptyDataTemplate>
			            <div style="padding: 20px; font-weight: bold; font-size: 1.1em; text-align: center; color: Maroon;">
				            Badge #<%# Request.QueryString["ID"].ToString() %> was not found.
			            </div>
		            </EmptyDataTemplate>
                </asp:FormView> 
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%-- END - MAIN PAGE CONTENT --%>
    
    <%-- BEGIN - EDIT BADGE ASSIGNMENT MODAL/DIALOG --%>
    <div id="editBadgeAssignmentModal">
        <asp:UpdatePanel ID="updtpnlEditBadgeAssignmentModal" runat="server">
            <ContentTemplate>
                <div class="FormError" id="editBadgeAssignmentError" runat="server" visible="false">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong>&nbsp;<span id="editBadgeAssignmentErrorMsg" runat="server">An error was encountered.</span></p>
                </div>
                <div class="OForm">
                    <div class="Row">
                        <div class="Label">Serial #:</div>
                        <div class="Control">
                            <asp:Label ID="lblEditSerialNo" runat="server" CssClass="LabelValue"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <asp:UpdatePanel ID="updtpnlDynamicDDLLocationWearer" runat="server">
                        <ContentTemplate>
                            <div class="Row">
                                <div class="Label">Location<asp:Label ID="lblRequired" runat="server" Font-Bold="true" ForeColor="Red" Visible="false">*</asp:Label>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlLocation" runat="server" AutoPostBack="true"
                                    DataValueField="LocationID" DataTextField="LocationName" TabIndex="1" 
                                    OnSelectedIndexChanged="ddlLocation_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">Wearer<asp:Label ID="Label1" runat="server" Font-Bold="true" ForeColor="Red">*</asp:Label>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlWearer" runat="server" AutoPostBack="true"
                                    DataValueField="UserID" DataTextField="LastFirstName" TabIndex="2" OnSelectedIndexChanged="ddlWearer_SelectedIndexChanged">
                                    </asp:DropDownList> 
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">Body Region:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlBodyRegion" runat="server" TabIndex="3">
                                        <asp:ListItem Text ="Collar" Value="5" />
                                        <asp:ListItem Text ="Torso" Value="1" />
                                        <asp:ListItem Text ="Fetal" Value="2" />
                                        <asp:ListItem Text ="Area" Value="3" />
                                        <asp:ListItem Text ="Unassigned" Value="4" />
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div> 
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <div class="Row">   
                        <div class="Label">Primary Badge:</div>
                        <div class="Control">
                            <asp:CheckBox ID="chkbxIsPrimary" runat="server" TabIndex="4" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">&nbsp;</div>
                        <div class="Control">
                            <asp:Button ID="btnStatus" runat="server" CssClass="OButton" onclick="btnStatus_Click" TabIndex="5" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Footer">
                        <span class="Required">*</span> Indicates a required field.
                    </div>
                </div>
                <asp:Button ID="btnSave" runat="server" Text="Save" ClientIDMode="Static" OnClick="btnSave_Click" TabIndex="6" style="display: none;" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" TabIndex="7" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%-- END - EDIT BADGE ASSIGNMENT MODAL/DIALOG --%>

    <%-- BEGIN - ACTIVATE/DEACTIVATE MODAL/DIALOG --%>
    <div id="activateDeactivateModal">
        <asp:UpdatePanel ID="updtpnlActivateDeactivateModal" runat="server">
            <ContentTemplate>
                <div class="FormError" id="divActiveDeactivateError" runat="server" visible="false">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong>&nbsp;<span id="spnActivateDeactivateErrorMsg" runat="server">An error was encountered.</span></p>
                </div>
                <div class="OForm">
                    <div class="Row" id="divActivateDeactivateMessage" runat="server">   
                        <div class="Control">
                            <asp:Label ID="lblActivateDeactivateMessage" runat="server" Text="" CssClass="LabelValue"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row" id="divDeactivateReason" runat="server">   
                        <div class="Label">Reason<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:textbox ID="txtDeactivateReason" runat="server" TextMode="MultiLine" CssClass="Size Medium" Height="75px" TabIndex="1" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Footer" id="requiredFieldSpan" runat="server">
                        <span class="Required">*</span> Indicates a required field.
                    </div>
                </div>
                <asp:Button ID="btnActivateDeactivate" runat="server" Text="Save" OnClick="btnActivateDeactivate_Click" TabIndex="2" style="display: none;" />
                <asp:Button ID="btnActivateDeactivateCancel" runat="server" Text="Cancel" OnClick="btnActivateDeactivateCancel_Click" TabIndex="3" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%-- END - ACTIVATE/DEACTIVATE MODAL/DIALOG --%>

    <div style="text-align: right; padding-top: 0px;">
        <asp:Button runat="server" ID="btnBackToAccount" Text="Back to Account" CssClass="OButton" onclick="btnBackToAccount_Click" />
    </div>
</asp:Content>

