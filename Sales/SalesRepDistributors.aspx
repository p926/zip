<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Sales_SalesRepDistributors" Codebehind="SalesRepDistributors.aspx.cs" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style type="text/css">
    .ShrinkPanelSize
    {
        width: 99%;
        padding: 0px 0px 0px 0px;
        margin: 0px 0px 0px 0px;
    }
    
    /* CSS Definition for Label Values in Modal/Dialog. */
    .StaticLabel 
    {
        color: #0C5A6B; 
        text-align: left; 
        font-weight: normal;
        display: block;
        padding: 3px;
        margin: 2px 0;
    }
</style>
<script type="text/javascript">
    function pageLoad(sender, args) {
        // Maintains jQuery datepicker(s) after PostBack.
        if (args.get_isPartialLoad()) {
            // Modal/Dialog for Sales Representative/Distributor.
            $('#<%= txtContractStartDate.ClientID %>').datepicker({
                changeMonth: true,
                changeYear: true
            });
            $('#<%= txtContractEndDate.ClientID %>').datepicker({
                changeMonth: true,
                changeYear: true
            });
            $('#<%= txtContractStartDate_New.ClientID %>').datepicker({
                changeMonth: true,
                changeYear: true
            });
            $('#<%= txtContractEndDate_New.ClientID %>').datepicker({
                changeMonth: true,
                changeYear: true
            });
            $('#<%= txtContractSignDate.ClientID %>').datepicker({
                changeMonth: true,
                changeYear: true
            });
            $('#<%= txtContractSignDate_New.ClientID %>').datepicker({
                changeMonth: true,
                changeYear: true
            });
            $('#<%= txtShipDate.ClientID %>').datepicker({
                changeMonth: true,
                changeYear: true
            });
            $('#<%= txtShipDate_New.ClientID %>').datepicker({
                changeMonth: true,
                changeYear: true
            });
            $('#<%= txtDataSheetSentDate.ClientID %>').datepicker({
                changeMonth: true,
                changeYear: true
            });
            $('#<%= txtDataSheetSentDate_New.ClientID %>').datepicker({
                changeMonth: true,
                changeYear: true
            });
            $('#ui-datepicker-div').css("z-index",
                    $(this).parents(".ui-dialog").css("z-index") + 1);

            var startDate_SRDModal = {
                altField: '#<%= txtContractSignDate.ClientID %>'
            };
                $('#<%= txtContractStartDate.ClientID %>').datepicker(startDate_SRDModal);

            var startDate_CSModal = {
                altField: '#<%= txtContractSignDate_New.ClientID %>'
            };
            $('#<%= txtContractStartDate_New.ClientID %>').datepicker(startDate_CSModal);
        }
        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
        });
    }

    function JQueryControlsLoad() {
        // Accordion
        var activeIndex = parseInt($('#<%= hdnfldAccordionIndex.ClientID %>').val());
        var activeIndex_New = parseInt($('#<%= hdnfldAccordionIndex_New.ClientID %>').val());

        // Accordion for (Primary) Sales Rep. Distributor Dialog.
        $("#accordion").accordion({
            header: "h3",
            autoHeight: false,
            active: activeIndex,
            change: function (event, ui) {
                var index = $(this).accordion("option", "active");
                $('#<%= hdnfldAccordionIndex.ClientID %>').val(index);
            }
        });

        // Accordion for (Secondary) Add Commission Details & Distributor Information Dialog.
        $("#accordion_new").accordion({
            header: "h3",
            autoHeight: false,
            active: activeIndex_New,
            change: function (event, ui) {
                var index = $(this).accordion("option", "active");
                $('#<%= hdnfldAccordionIndex_New.ClientID %>').val(index);
            }
        });

        // Modal/Dialog for Sales Representative/Distributor.
        $('#divSalesRepDistributorDialog').dialog({
            autoOpen: false,
            width: 700,
            resizable: false,
            title: "Sales Rep. Information",
            open: function (type, data) {
                $(this).parent().appendTo("form");
                $('#<%= txtSalesRepDistID.ClientID %>').focus();
            },
            buttons: {
                "Save": function () {
                    $('#<%= btnSave.ClientID %>').click();
                },
                "Cancel": function () {
                    $(this).dialog("close");
                }
            },
            close: function () {
                $('#<%= btnCancel.ClientID %>').click();
                $('.ui-overlay').fadeOut();
            }
        });

        // Modal/Dialog for Commission Details.
        $('#divCommissionDetailsDialog').dialog({
            autoOpen: false,
            width: 700,
            resizable: false,
            title: "Add Commission Details",
            open: function (type, data) {
                $(this).parent().appendTo("form");
                $('#<%= txtCommissionPercentage_New.ClientID %>').focus();
            },
            buttons: {
                "Save": function () {
                    $('#<%= btnAddCommissionDetail_New.ClientID %>').click();
                },
                "Cancel": function () {
                    $(this).dialog("close");
                }
            },
            close: function () {
                $('#<%= btnCancel_New.ClientID %>').click();
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
<script type="text/javascript">
    function preventBackspace(e) {
        var evt = e || window.event;
        if (evt) {
            var keyCode = evt.charCode || evt.keyCode;
            if (keyCode === 8) {
                if (evt.preventDefault) {
                    evt.preventDefault();
                } else {
                    evt.returnValue = false;
                }
            }
        }
    }
</script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>
    <%--///////////////////////////////////////////// BEGIN - ADD/EDIT/DEACTIVATE "Sales Rep. Distributor Information" MODAL ////////////////////////////////////////--%>
    <div id="divSalesRepDistributorDialog">
        <asp:UpdatePanel ID="updtpnlSalesRepDistributor" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldAccordionIndex" runat="server" Value="0" />
                <asp:HiddenField ID="hdnfldSalesRepDistID" runat="server" Value="" />
                <asp:HiddenField ID="hdnfldFormMode" runat="server" Value="" />
                <div class="FormError" id="salesRepDistributorDialogErrors" runat="server" visible="false">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong>&nbsp;<span id="salesRepDistributorDialogErrorMsg" runat="server" >An error was encountered.</span></p>
                </div>
                <div id="accordion" style="margin-top: 10px;">
                    <div id="divMasterInformation" runat="server">
			            <h3><a href="#">Master Sales Rep. Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--ROW 1--%>
                            <div class="Row">
                                <div class="Label">Channel<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlChannelTypes" runat="server" TabIndex="1" AutoPostBack="true" OnSelectedIndexChanged="ddlChannelTypes_SelectedIndexChanged">
                                        <asp:ListItem Text="---Select---" Value="0"></asp:ListItem>
                                        <asp:ListItem Text="DISTRIBUTOR" Value="7"></asp:ListItem>
                                        <asp:ListItem Text="RESELLER" Value="8"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 2--%>
                            <div class="Row">
                                <div class="Label">Rep. #<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSalesRepDistID" CssClass="Size Small" TabIndex="2" onKeyDown="preventBackspace();" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 3--%>
                            <div class="Row">
                                <div class="Label">Sales Manager<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlSalesManagerID" runat="server" AppendDataBoundItems="true" TabIndex="3">
                                        <asp:ListItem Text="---Select---" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 4--%>
                            <div class="Row">
                                <div class="Label">Company Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtCompanyName" CssClass="Size Large" TabIndex="4" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 5--%>
                            <div class="Row">
                                <div class="Label">Prefix:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlPrefix" runat="server" TabIndex="5">
                                        <asp:ListItem Text="---Select---" Value="0"></asp:ListItem>
                                        <asp:ListItem Text="Dr." Value="Dr."></asp:ListItem>
                                        <asp:ListItem Text="Mr." Value="Mr."></asp:ListItem>
                                        <asp:ListItem Text="Mrs." Value="Mrs."></asp:ListItem>
                                        <asp:ListItem Text="Ms." Value="Ms."></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 6--%>
                            <div class="Row">
                                <div class="Label">First Name:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="Size Medium" TabIndex="6"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 7--%>
                            <div class="Row">
                                <div class="Label">Middle Name:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtMiddleName" runat="server" CssClass="Size Medium" TabIndex="7"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 8--%>
                            <div class="Row">
                                <div class="Label">Last Name:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtLastName" runat="server" CssClass="Size Medium" TabIndex="8"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 9--%>
                            <div class="Row">
                                <div class="Label">Contact Name:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtContactName" CssClass="Size Large" TabIndex="9" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 10--%>
                            <div class="Row">
                                <div class="Label Large">Notification Preference<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlNotificationPerference" runat="server" TabIndex="10"
                                    AutoPostBack="true" OnSelectedIndexChanged="ddlNotificationPerference_SelectedIndexChanged">
                                        <asp:ListItem Text="EMAIL" Value="EMAIL"></asp:ListItem>
                                        <asp:ListItem Text="FAX" Value="FAX"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 11--%>
                            <div class="Row">
                                <div class="Label">Phone:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtPhone" runat="server" CssClass="Size Medium" TabIndex="11"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 12--%>
                            <div class="Row">
                                <div class="Label">Fax<asp:Label ID="lblFaxRequired" runat="server" CssClass="Required">*</asp:Label>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtFax" runat="server" CssClass="Size Medium" TabIndex="12"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 13--%>
                            <div class="Row">
                                <div class="Label">Cust. Service Phone:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtCustServicePhone" runat="server" CssClass="Size Medium" TabIndex="13"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 14--%>
                            <div class="Row">
                                <div class="Label">E-mail<asp:Label ID="lblEmailRequired" runat="server" CssClass="Required">*</asp:Label>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtEmail" CssClass="Size Large" TabIndex="14" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 15--%>
                            <div class="Row">
                                <div class="Label">Address 1:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtAddress1" CssClass="Size Large" TabIndex="15" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 16--%>
                            <div class="Row">
                                <div class="Label">Address 2:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtAddress2" CssClass="Size Large" TabIndex="16" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 17--%>
                            <div class="Row">
                                <div class="Label">City:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtCity" CssClass="Size Large" TabIndex="17" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 18--%>
                            <div class="Row">
                                <div class="Label">Country:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlCountry" runat="server" TabIndex="18" AppendDataBoundItems="true"
                                    AutoPostBack="true" OnSelectedIndexChanged="ddlCountry_SelectedIndexChanged">
                                        <asp:ListItem Value="0" Text="---Select---"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 19--%>
                            <div class="Row">
                                <div class="Label">State:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlState" runat="server" TabIndex="19" AppendDataBoundItems="true">
                                        <asp:ListItem Value="0" Text="---Select---"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 20--%>
                            <div class="Row">
                                <div class="Label">Zip Code:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtZipCode" runat="server" CssClass="Size Small" TabIndex="20"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </div>   
                    <div id="divCommissionDetails" runat="server">
			            <h3><a href="#">Commission Details&nbsp;<asp:DropDownList ID="ddlCommissionSetupID" runat="server" 
                                                                DataSourceID="sqlGetCommissionDetails" DataTextField="ContractDateRange"
                                                                DataValueField="CommissionSetupID" AutoPostBack="true" 
                                                                onselectedindexchanged="ddlCommissionSetupID_SelectedIndexChanged">
                                                                </asp:DropDownList>&nbsp;</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--ROW 1--%>
                            <div class="Row">
                                <div class="Label Large">Commission %<asp:Label ID="lblCommissionPercentageRequired" runat="server" CssClass="Required">*</asp:Label>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtCommissionPercentage" CssClass="Size Small" TabIndex="21" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 2--%>
                            <div class="Row">
                                <div class="Label Large">Renewal Commission %<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtRenewalCommissionPercentage" CssClass="Size Small" TabIndex="22" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 3--%>
                            <div class="Row">
                                <div class="Label Large">Sales Mgr. Commission %<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSalesMgrCommissionPercentage" CssClass="Size Small" TabIndex="23" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 4--%>
                            <div class="Row">
                                <div class="Label Large">Commission Renewal Year.<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtCommissionRenewalYears" CssClass="Size Small" TabIndex="24" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 5--%>
                            <div class="Row">
                                <div class="Label Large">Contract Sign Date<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtContractSignDate" CssClass="Size Small" TabIndex="25" OnTextChanged="txtContractSignDate_OnTextChanged" AutoPostBack="true" onKeyDown="preventBackspace();" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 6--%>
                            <div class="Row">
                                <div class="Label Large">Contract Start Date<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtContractStartDate" runat="server" CssClass="Size Small" TabIndex="26" OnTextChanged="txtContractStartDate_OnTextChanged" AutoPostBack="true" onKeyDown="preventBackspace();"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 7--%>
                            <div class="Row">
                                <div class="Label Large">Contract End Date<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtContractEndDate" runat="server" CssClass="Size Small" TabIndex="27" onKeyDown="preventBackspace();"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 8--%>
                            <div class="Row">
                                <div class="Label Large">&nbsp;</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxPORequired" runat="server" Text="PO Required?" TabIndex="28" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 9--%>
                            <div class="Row">
                                <div class="Label Large">Old Quantum Code:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtOldQuantumCode" CssClass="Size Small" TabIndex="29" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 10--%>
                            <div class="Row">
                                <div class="Label Large">Agreement Type:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlAgreementType" runat="server" AppendDataBoundItems="true" TabIndex="30">
                                        <asp:ListItem Text="---Select---" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 11--%>
                            <div class="Row">
                                <div class="Label Large">Reseller Price:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtResellerPrice" CssClass="Size Small" TabIndex="31" />
                                    &nbsp;
                                    <asp:DropDownList ID="ddlResellerPriceCurrencyCode" runat="server" AppendDataBoundItems="true" TabIndex="32">
                                        <asp:ListItem Text="---Select---" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 12--%>
                            <div class="Row">
                                <div class="Label Large"># of Trifolds:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtNumberOfTrifolds" runat="server" CssClass="Size Small" TabIndex="33"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 13--%>
                            <div class="Row">
                                <div class="Label Large">&nbsp;</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxCodeOfEthicsYes" runat="server" Text="Code of Ethics?" TabIndex="34" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 14--%>
                            <div class="Row">
                                <div class="Label Large">Ship Date:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtShipDate" CssClass="Size Small" TabIndex="35" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 15--%>
                            <div class="Row">
                                <div class="Label Large">DataSheet Sent Date:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtDataSheetSentDate" CssClass="Size Small" TabIndex="36" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 16--%>
                            <div class="Row">
                                <div class="Label Large">&nbsp;</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxSpecialPriceOfTrifoldYes" runat="server" Text="Special Prices of Trifold?" TabIndex="37" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 17--%>
                            <div class="Row">
                                <div class="Label Large">Notes:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtNotes" runat="server" Rows="5" TextMode="MultiLine" CssClass="Size Large" TabIndex="38"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 18--%>
                            <div id="divCommissionDetailsButtons" runat="server" class="Row">
                                <div class="Label Large">&nbsp;</div>
                                <div class="Control">
                                    <asp:Button ID="btnCommissionDetailStatus" runat="server" CssClass="OButton" TabIndex="39" onclick="btnCommissionDetailStatus_Click" />
                                    &nbsp;|&nbsp;
                                    <asp:Button ID="btnAddCommissionDetails" runat="server" CssClass="OButton" 
                                    Text="Add Commission Details" TabIndex="40" 
                                    onclick="btnAddCommissionDetails_Click" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>            
                        </div>
                    </div>
                    <div id="divToggleSalesRepDistributorStatus" runat="server">
			            <h3><a href="#"><asp:Label ID="lblChangeStatusOfSalesRepDistributor" runat="server"></asp:Label></a></h3>
                        <div class="OForm AccordionPadding">
                            <%--ROW 1--%>
                            <div class="Row">
                                <div class="Control">
                                     <asp:Button ID="btnDeactivateSaleRepDistributorRecord" runat="server" 
                                     CssClass="OButton" TabIndex="41" 
                                     onclick="btnDeactivateSaleRepDistributorRecord_Click" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </div>
                    <div class="RequiredIndicator" style="padding-left: 10px; padding-top: 10px;">
                        <span class="Required">*</span>&nbsp;Indicates a required field.                        
                    </div>
                    <asp:Button ID="btnSave" runat="server" Text="Save" style="display:none;" OnClick="btnSave_Click" TabIndex="42"></asp:Button>
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" style="display:none;" OnClick="btnCancel_Click" TabIndex="43"></asp:Button>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--///////////////////////////////////////////// END - ADD/EDIT/DEACTIVATE "Sales Rep. Distributor Information" MODAL ////////////////////////////////////////--%>
    <%--///////////////////////////////////////////// BEGIN - "Add (New) Commission Details" MODAL ////////////////////////////////////////--%>
    <div id="divCommissionDetailsDialog">
        <asp:UpdatePanel ID="updtpnlAddCommissionDetails_New" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldAccordionIndex_New" runat="server" Value="0" />
                <div class="FormError" id="addCommissionDetailsDialogErrors" runat="server" visible="false">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong>&nbsp;<span id="addCommissionDetailsErrorMsg" runat="server" >An error was encountered.</span></p>
                </div>
                <div id="accordion_new" style="margin-top: 10px;">
                    <div id="divCommissionDetails_New" runat="server">
                        <h3><a href="#">Commission Details</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--ROW 1--%>
                            <div class="Row">
                                <div class="Label Large">Sales Rep. #:</div>
                                <div class="Control">
                                    <asp:Label ID="lblSalesRepDistID" runat="server" CssClass="StaticLabel"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%> 
                            <%--ROW 2--%>
                            <div class="Row">
                                <div class="Label Large">Commission %<asp:Label ID="lblCommissionPercentageRequired_New" runat="server" CssClass="Required">*</asp:Label>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtCommissionPercentage_New" CssClass="Size Small" TabIndex="1" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 3--%>
                            <div class="Row">
                                <div class="Label Large">Renewal Commission %<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtRenewalCommissionPercentage_New" CssClass="Size Small" TabIndex="2" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 4--%>
                            <div class="Row">
                                <div class="Label Large">Sales Mgr. Commission %<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSalesMgrCommissionPercentage_New" CssClass="Size Small" TabIndex="3" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 5--%>
                            <div class="Row">
                                <div class="Label Large">Commission Renewal Years<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtCommissionRenewalYears_New" CssClass="Size Small" TabIndex="4" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 6--%>
                            <div class="Row">
                                <div class="Label Large">Contract Sign Date<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtContractSignDate_New" CssClass="Size Small" TabIndex="5" OnTextChanged="txtContractSignDate_New_OnTextChanged" AutoPostBack="true" onKeyDown="preventBackspace();" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 7--%>
                            <div class="Row">
                                <div class="Label Large">Contract Start Date<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtContractStartDate_New" runat="server" CssClass="Size Small" TabIndex="6" OnTextChanged="txtContractStartDate_New_OnTextChanged" AutoPostBack="true" onKeyDown="preventBackspace();"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 8--%>
                            <div class="Row">
                                <div class="Label Large">Contract End Date<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtContractEndDate_New" runat="server" CssClass="Size Small" TabIndex="7" onKeyDown="preventBackspace();"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 9--%>
                            <div class="Row">
                                <div class="Label Large">&nbsp;</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxPORequired_New" runat="server" Text="PO Required?" TabIndex="8" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 10--%>
                            <div class="Row">
                                <div class="Label Large">Old Quantum Code:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtOldQuantumCode_New" CssClass="Size Small" TabIndex="9" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 11--%>
                            <div class="Row">
                                <div class="Label Large">Agreement Type:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlAgreementType_New" runat="server" AppendDataBoundItems="true" TabIndex="10">
                                        <asp:ListItem Text="---Select---" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 12--%>
                            <div class="Row">
                                <div class="Label Large">Reseller Price:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtResellerPrice_New" CssClass="Size Small" TabIndex="11" />
                                    &nbsp;
                                    <asp:DropDownList ID="ddlResellerPriceCurrencyCode_New" runat="server" AppendDataBoundItems="true" TabIndex="12">
                                        <asp:ListItem Text="---Select---" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 13--%>
                            <div class="Row">
                                <div class="Label Large"># of Trifolds:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtNumberOfTrifolds_New" runat="server" CssClass="Size Small" TabIndex="13"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 14--%>
                            <div class="Row">
                                <div class="Label Large">&nbsp;</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxCodeOfEthics_New" runat="server" Text="Code of Ethics?" TabIndex="14" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 15--%>
                            <div class="Row">
                                <div class="Label Large">Ship Date:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtShipDate_New" CssClass="Size Small" TabIndex="15" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 16--%>
                            <div class="Row">
                                <div class="Label Large">DataSheet Sent Date:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtDataSheetSentDate_New" CssClass="Size Small" TabIndex="16" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 17--%>
                            <div class="Row">
                                <div class="Label Large">&nbsp;</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxSpecialPricesOfTrifolds_New" runat="server" Text="Special Prices of Trifold?" TabIndex="17" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 18--%>
                            <div class="Row">
                                <div class="Label Large">Notes:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtNotes_New" runat="server" Rows="5" TextMode="MultiLine" CssClass="Size Large" TabIndex="18"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </div>
                    <div class="RequiredIndicator" style="padding-left: 10px; padding-top: 10px;">
                        <span class="Required">*</span>&nbsp;Indicates a required field.                        
                    </div>
                    <asp:Button ID="btnAddCommissionDetail_New" runat="server" Text="Save" style="display:none;" OnClick="btnAddCommissionDetail_New_Click" TabIndex="19"></asp:Button>
                    <asp:Button ID="btnCancel_New" runat="server" Text="Cancel" style="display:none;" OnClick="btnCancel_New_Click" TabIndex="20"></asp:Button>
                </div>    
            </ContentTemplate>   
        </asp:UpdatePanel>
    </div>
    <%--///////////////////////////////////////////// END - "Add (New) Commission Details" MODAL ////////////////////////////////////////--%>
    <div id="mainContentArea">
        <asp:UpdatePanel ID="updtpnlMainContentArea" runat="server" UpdateMode="Conditional">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnAddEditSalesRepDistributor" EventName="Click" />
            </Triggers>
            <ContentTemplate>
                <%--Sales Representative/Distributor Search Toolbar--%>
                <div id="divSearchSortToolbar" class="OToolbar JoinTable" runat="server">
                    <ul>
                        <li>
                            <asp:LinkButton ID="btnAddEditSalesRepDistributor" CssClass="Add Icon" runat="server"
                            CommandName="AddEditSalesRepDistributor" CommandArgument="" 
                            onclick="btnAddEditSalesRepDistributor_Click">Add Sales Rep.</asp:LinkButton>
                        </li>
                        <li>Rep. #:</li>
                        <li>                           
                            <asp:TextBox ID="txtSalesRepNo" runat="server" Style="width: 50px;" Text=""></asp:TextBox>    
                        </li>
                        <li>First Name:</li>
                        <li>                           
                            <asp:TextBox ID="txtSalesRepFirstName" runat="server" Style="width: 100px;" Text=""></asp:TextBox>    
                        </li>
                        <li>Last Name:</li>
                        <li>                           
                            <asp:TextBox ID="txtSalesRepLastName" runat="server" Style="width: 100px;" Text=""></asp:TextBox>    
                        </li>
                        <li>Channel:</li>
                        <li>
                            <asp:DropDownList ID="ddlFilterByChannel" runat="server" AutoPostBack="true">
                                <asp:ListItem Text="All" Value="" Selected="True"></asp:ListItem>
                                <asp:ListItem Text="DISTRIBUTOR" Value="DISTRIBUTOR"></asp:ListItem>
                                <asp:ListItem Text="RESELLER" Value="RESELLER"></asp:ListItem>
                            </asp:DropDownList> 
                        </li>
                        <li>
                            <asp:Button ID="btnFind" runat="server" Text="Find" Width="75" />
                        </li>
                    </ul>
                </div>
                <%--End--%>
                <%--Sales Representative/Distributor GridView--%>
                <asp:UpdatePanel ID="updtpnlSalesRepDistributorToolbarAndGridView" runat="server">
                    <ContentTemplate>
                        <asp:GridView ID="gvSalesRepDistributor" runat="server" CssClass="OTable" 
                        AlternatingRowStyle-CssClass="Alt" AllowSorting="True" AutoGenerateColumns="False" 
                        DataKeyNames="SalesRepDistID" DataSourceID="sqlGetSalesRepDistributors"
                        AllowPaging="True" PageSize="20" Width="100%" EnableViewState="false">
                            <AlternatingRowStyle CssClass="Alt" />
                            <Columns>
                                <asp:TemplateField ItemStyle-Width="20px" ShowHeader="False">
                                    <ItemTemplate>
                                        <asp:ImageButton ID="imgbtnEditSalesRepDistInfo" runat="server"
                                        ImageUrl="~/css/dsd-default/images/icons/pencil.png"
                                        CommandName="OpenDialog" 
                                        AlternateText="Edit Sales Rep. Distributor Information"
                                        ToolTip="Edit Sales Rep. Distributor Information"
                                        CommandArgument='<%# DataBinder.Eval(Container.DataItem,"SalesRepDistID","") %>'
                                        OnClick="imgbtnEditSalesRepDistInfo_Click" />
                                    </ItemTemplate>
                                    <ItemStyle Width="20px" />
                                </asp:TemplateField>
                                <asp:BoundField DataField="SalesRepDistID" HeaderText="Rep. #" ReadOnly="true" SortExpression="SalesRepDistID" />
                                <asp:BoundField DataField="ChannelType" HeaderText="Channel" ReadOnly="true" SortExpression="ChannelType" />
                                <asp:BoundField DataField="CompanyName" HeaderText="Company" ReadOnly="true" SortExpression="CompanyName" />
                                <asp:BoundField DataField="FullName" HeaderText="Sales Rep. Name" ReadOnly="true" SortExpression="FullName" />
                                <asp:BoundField DataField="Telephone" HeaderText="Phone" ReadOnly="true" SortExpression="Telephone" />
                                <asp:BoundField DataField="ContactName" HeaderText="Contact Name" ReadOnly="true" SortExpression="ContactName" />
                                <asp:BoundField DataField="SalesManager" HeaderText="Sales Mgr. #" ReadOnly="true" SortExpression="SalesManager" />
                                <asp:TemplateField HeaderText="Active" SortExpression="Active">
                                    <ItemTemplate>
                                        <asp:Label ID="lblIsActive" runat="server" CssClass='<%# Eval("Active", "lblActive{0}") %>' Text='<%# YesNo(Eval("Active")) %>' Font-Bold="false" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <%--<asp:CheckBoxField DataField="Active" HeaderStyle-Width="60px" HeaderText="Active" ReadOnly="true" SortExpression="Active"></asp:CheckBoxField>--%>
                            </Columns>
                            <EmptyDataTemplate>
                                <div>There are no records found!</div>
                            </EmptyDataTemplate>
                            <PagerSettings PageButtonCount="20" />
                        </asp:GridView>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <%--End--%>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--///////////////////////////////////////////// BEGIN - SQL Database Sources ////////////////////////////////////////--%>
    <%--Main GridView with Sales Rep. Distributor Information--%>
    <asp:SqlDataSource ID="sqlGetSalesRepDistributors" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="sp_GetAllSalesRepDistributorsBySearch" 
    SelectCommandType="StoredProcedure">
        <SelectParameters>
            <asp:ControlParameter ControlID="txtSalesRepNo" Name="pSalesRepDistIDSearch" ConvertEmptyStringToNull="false" PropertyName="Text" /> 
            <asp:ControlParameter ControlID="txtSalesRepFirstName" Name="pFirstNameSearch" ConvertEmptyStringToNull="false" PropertyName="Text" /> 
            <asp:ControlParameter ControlID="txtSalesRepLastName" Name="pLastNameSearch" ConvertEmptyStringToNull="false" PropertyName="Text" />  
            <asp:ControlParameter ControlID="ddlFilterByChannel" Name="pChannelFilter"  ConvertEmptyStringToNull="false" PropertyName="SelectedItem.Value" />
        </SelectParameters>
    </asp:SqlDataSource>
    <%--End--%>
    <%--Commission Details DropDownList--%>
    <asp:SqlDataSource ID="sqlGetCommissionDetails" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="SELECT [CommissionSetupID],
                   (CONVERT(VARCHAR, [ContractStartDate], 101) + '-' + CONVERT(VARCHAR, [ContractEndDate], 101)) AS ContractDateRange 
                   FROM [CommissionSetup] 
                   WHERE [SalesRepDistID] = @SalesRepDistID
                   ORDER BY [ContractStartDate] DESC">
        <SelectParameters>
            <asp:ControlParameter ControlID="hdnfldSalesRepDistID" Name="SalesRepDistID" PropertyName="Value" />
        </SelectParameters>
    </asp:SqlDataSource>
    <%--End--%>
    <%--///////////////////////////////////////////// END - SQL Database Sources ////////////////////////////////////////--%>
</asp:Content>


