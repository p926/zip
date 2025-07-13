<%@ Page Title="Factors Page" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_Factors_New" Codebehind="Factors_New.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        .AlteredField {
            font-weight:bold;
            color: blue;
        }
        .OTable .Size.XXSmall
        {
            width:40px;
        }
        .gridView
		{
			max-height: 600px; 
			/*overflow:auto;*/   
			margin-top: 10px;  
            overflow-x:scroll;
            overflow-y:auto;
		}

		/* Fixed gridview header*/
		.FixedHeader {
			position:absolute;
			margin:-10px 0px 0px 0px;
			/*z-index:99;*/           
		}	
    </style>
    <script type="text/javascript" src="/scripts/jquery.placeholder.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
        });
        
        function JQueryControlsLoad() {
            $("#TabContainer1").tabs();

            // Account Settings Modal/Dialog.
            $('#divAccountSettingsDialog').dialog({
                autoOpen: false,
                width: 565,
                resizable: false,
                modal: true,
                title: "Account Settings",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Update": function () {
                        $('#<%= btnUpdate_Account.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancel_Account.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });

            // Location Settings Modal/Dialog.
            $('#divLocationSettingsDialog').dialog({
                autoOpen: false,
                width: 565,
                resizable: false,
                modal: true,
                title: "Location Settings",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Update": function () {
                        $('#<%= btnUpdate_Location.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancel_Location.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });

            // History Modal/Dialog.
            $('#divFactorsHistoryDialog').dialog({
                autoOpen: false,
                width: 1000,
                resizable: false,
                modal: true,
                title: "Factors Account History",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Print": function () {
                        $('#<%= btnFactorsHistoryPrint.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnFactorsHistoryCancel.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });

            // HTML5 for IE9 Placeholder Override.
            $(':input[placeholder]').placeholder();
        }

        // Open Modal/Dialog.
        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        }

        // Close Modal/Dialog.
        function closeDialog(id) {
            $('#' + id).dialog("close");
        }

        // Allows User to type in Numeric values only (no decimal allowed).
        function checkForWholeNumber(evt) {
            var charCode = (evt.which) ? evt.which : event.keyCode;

            if (charCode > 31 && (charCode < 48 || charCode > 57))
                return false;

            return true;
        }

        // Allows on Numeric values and one decimal (onkeyup).
        function checkForExtraDecimal(elmnt) {
            //var ex = /^[0-9]+\.?[0-9]*$/;
            //if (ex.test(elmnt.value) == false) {
            //    elmnt.value = elmnt.value.substring(0, elmnt.value.length - 1);
            //    return false;
            //}         

            if (elmnt.value.charAt(0) == "." && elmnt.value.length == 1) return true;   
            
            var n = Number(elmnt.value);            
            if (isNaN(n)) {                
                elmnt.value = elmnt.value.substring(0, elmnt.value.length - 1);
                return false;
            }

            return true;
        }

        // Allows User to type in Alpha Characters (2) only (used for Algorithm Factor).
        function checkForAlphaCharacters(elmnt) {           
            //var ex=new RegExp("[^a-z|^A-Z]");
            var ex=/[^a-z|^A-Z]/;
            if(ex.test(elmnt.value))
            {                
                elmnt.value = elmnt.value.substring(0, elmnt.value.length - 1);
                return false;
            }            
            return true;
        }       

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <%--ACCOUNT # & COMPANY NAME & HISTORY LINK--%>
    <div id="divAccountIDAndCompanyName" style="padding: 0px 10px 0px 10px;">
        <asp:UpdatePanel ID="updtpnlMainArea" runat="server" >
            <ContentTemplate>
                <table style="width:100%">
                    <tr>
                        <td>
                            <h2>Account:&nbsp;<asp:Label ID="lblAccountIDAndCompanyName" runat="server" Font-Bold="true"></asp:Label></h2>
                        </td>
                        <td align="right">
                            <asp:LinkButton ID="lnkbtnFactorHistoryLog" runat="server" ToolTip="Factors History Log"
                                CommandName="ShowFactorHistoryLog" CommandArgument='<%= Request.QueryString["AccountID"] %>'  
                                CssClass="Icon Export" OnClick="lnkbtnFactorHistoryLog_Click" Text="History" />                   
                        </td>
                    </tr>
                </table>  
            </ContentTemplate>
        </asp:UpdatePanel>                         
    </div>     
    <%--END--%>

    <%--ERROR MESSAGE MODAL/DIALOG--%>
    <asp:UpdatePanel ID="updtpnlErrorOrSuccessMessage" runat="server">
        <ContentTemplate>
            <%--ERROR MESSAGE(S)--%>
            <div class="FormError" id="divErrorMessage" runat="server" visible="false">
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong>&nbsp;<span id="spnErrorMessage" runat="server">An error was encountered.</span></p>
            </div>
            <%--END--%>
            <%--SUCCESS MESSAGE(S)--%>
            <div class="FormMessage" id="divSuccessMessage" runat="server" visible="false">
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong>&nbsp;<span id="spnSuccessMessage" runat="server">An error was encountered.</span></p>
            </div>
            <%--END--%>
        </ContentTemplate>
    </asp:UpdatePanel>

    <%--ACCOUNT SETTINGS MODAL/DIALOG--%>
    <div id="divAccountSettingsDialog" >
        <asp:UpdatePanel ID="updtpnlAccountSettings" runat="server">
            <ContentTemplate>
                <table style="width:100%;">
                    <tr>
                        <td>
                            <asp:ValidationSummary ID ="ValidationSummary_Account" runat="server" CssClass="FormError" ForeColor="#B94A48" DisplayMode="BulletList"
                                ValidationGroup="UPDATE_ACCOUNT" EnableClientScript="true" HeaderText="<p><span class='MessageIcon'></span><strong>Messages:</strong> Please correct the following fields:</p>" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <h2>Account #:&nbsp;<asp:Label ID="lblAccountID_Account" runat="server" Font-Bold="true"></asp:Label></h2>
                        </td>                        
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lblCompanyName" runat="server"  />
                        </td>                        
                    </tr>
                    <tr>
                        <td>
                            <table class="OTable" style="width: auto;">
                                <tr>
                                    <th>ProductType</th>
                                    <th>MRD</th>
                                    <th>MRDIncr</th>
                                    <th>Deep</th>
                                    <th>Eye</th>
                                    <th>Shallow</th>
                                    <th>Bkgd</th>
                                    <th>Algo Path</th>
                                </tr>
                                <tr>
                                    <td>ID1</td>
                                    <td>
                                        <asp:TextBox ID="txtMRD_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="1" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRD_Account" runat="server" ErrorMessage="MRD(ID1): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRD_Account" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMRDIncrement_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="2" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDIncrement_Account" runat="server" ErrorMessage="MRDIncr(ID1): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrement_Account" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td><asp:TextBox ID="txtDeepFactor_Account" runat="server" CssClass="Size XXSmall" TabIndex="3" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtEyeFactor_Account" runat="server" CssClass="Size XXSmall" TabIndex="4" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtShallowFactor_Account" runat="server" CssClass="Size XXSmall" TabIndex="5" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtBackgroundRate_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="6" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtAlgoPath_Account" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="7"/></td>
                                </tr>
                                <tr>
                                    <td>ID+</td>
                                    <td>
                                        <asp:TextBox ID="txtMRDPlus_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="8" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDPlus_Account" runat="server" ErrorMessage="MRD(ID+): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDPlus_Account" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMRDIncrementPlus_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="9" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDIncrementPlus_Account" runat="server" ErrorMessage="MRDIncr(ID+): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrementPlus_Account" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td><asp:TextBox ID="txtDeepFactorPlus_Account" runat="server" CssClass="Size XXSmall" TabIndex="10" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtEyeFactorPlus_Account" runat="server" CssClass="Size XXSmall" TabIndex="11" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtShallowFactorPlus_Account" runat="server" CssClass="Size XXSmall" TabIndex="12" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtBackgroundRatePlus_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="13" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtAlgoPathPlus_Account" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="14"/></td>
                                </tr>
                                <tr>
                                    <td>ID2</td>
                                    <td>
                                        <asp:TextBox ID="txtMRD2New_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="15" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRD2New_Account" runat="server" ErrorMessage="MRD(ID2): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRD2New_Account" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMRDIncrement2New_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="16" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDIncrement2New_Account" runat="server" ErrorMessage="MRDIncr(ID2): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrement2New_Account" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td><asp:TextBox ID="txtDeepFactor2New_Account" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="17" /></td>
                                    <td><asp:TextBox ID="txtEyeFactor2New_Account" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="18" /></td>
                                    <td><asp:TextBox ID="txtShallowFactor2New_Account" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="19" /></td>
                                    <td><asp:TextBox ID="txtBackgroundRate2New_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="20" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtAlgoPath2New_Account" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="21" /></td>
                                </tr>
                                <tr>
                                    <td>IDElite</td>
                                    <td>
                                        <asp:TextBox ID="txtMRD2_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="22" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRD2_Account" runat="server" ErrorMessage="MRD(IDElite): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRD2_Account" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMRDIncrement2_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="23" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDIncrement2_Account" runat="server" ErrorMessage="MRDIncr(IDElite): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrement2_Account" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td><asp:TextBox ID="txtDeepFactor2_Account" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="24" /></td>
                                    <td><asp:TextBox ID="txtEyeFactor2_Account" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="25" /></td>
                                    <td><asp:TextBox ID="txtShallowFactor2_Account" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="26" /></td>
                                    <td><asp:TextBox ID="txtBackgroundRate2_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="27" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtAlgorithmFactor_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="28" onkeyup="return checkForAlphaCharacters(this)" MaxLength="2" /></td>
                                </tr>  
                                  <tr>
                                    <td>IDVue</td>
                                    <td>
                                        <asp:TextBox ID="txtMRDVue" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="29" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDVue" runat="server" ErrorMessage="MRDVue(IDVue): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDVue" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMRDIncrementVue_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="30" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDIncrementVue_Account" runat="server" ErrorMessage="MRDIncr(IDVue): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrementVue_Account" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td><asp:TextBox ID="txtDeepfactorVue" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="31" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtEyeFactorVue_Account" runat="server" CssClass="Size XXSmall" TabIndex="32" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtShallowFactorVue_Account" runat="server" CssClass="Size XXSmall" TabIndex="33" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtBackgroundRateVue" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="34" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtAlgoPathVue_Account" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="35"/></td>
                                </tr>     

                            </table>
                        </td>                        
                    </tr>
                    <tr>
                        <td>
                            Read Limit:&nbsp;<asp:TextBox ID="txtReadLimit_Account" runat="server" CssClass="Size XSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="29" onkeypress="return checkForWholeNumber(event)" MaxLength="6" />
                            <asp:RangeValidator ID="rngvalReadLimit_Account" runat="server" ErrorMessage="Read Limit: Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100000" ControlToValidate="txtReadLimit_Account" Display="None" Type="Integer"></asp:RangeValidator>
                        </td>                        
                    </tr>
                    <tr>
                        <td>
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Notes:&nbsp;<asp:TextBox ID="txtNotes_Account" runat="server" CssClass="Size XSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="30" TextMode="MultiLine" Width="400px" Height="80px"/>                            
                        </td>                        
                    </tr>
                </table>                

                <%--ACCOUNT-LEVEL UPDATE MODAL/DIALOG FUNCTION BUTTONS--%>
                <asp:Button ID="btnUpdate_Account" runat="server" style="display: none;" Text="Update" OnClick="btnUpdate_Account_Click" ValidationGroup="UPDATE_ACCOUNT" TabIndex="31" />
                <asp:Button ID="btnCancel_Account" runat="server" style="display:none;" Text="Cancel" TabIndex="32" />
                <%--END--%>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>   
    <%--END--%>
    <%--LOCATION SETTINGS MODAL/DIALOG--%>
    <div id="divLocationSettingsDialog">
        <asp:UpdatePanel ID="updtpnlLocationSettings" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldLocationID" runat="server" Value="0" />
                <table style="width:100%;">
                    <tr>
                        <td>
                            <asp:ValidationSummary ID ="ValidationSummary_Location" runat="server" CssClass="FormError" ForeColor="#B94A48" DisplayMode="BulletList"
                                ValidationGroup="UPDATE_LOCATION" EnableClientScript="true" HeaderText="<p><span class='MessageIcon'></span><strong>Messages:</strong> Please correct the following fields:</p>" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <h2>Account #:&nbsp;<asp:Label ID="lblAccountID_Location" runat="server" Font-Bold="true"></asp:Label></h2>
                        </td>                        
                    </tr>
                    <tr>
                        <td>
                            Location: <asp:Label ID="lblLocationName" runat="server"  />
                        </td>                        
                    </tr>
                    <tr>
                        <td>
                            <table class="OTable" style="width: auto;">
                                <tr>
                                    <th>ProductType</th>
                                    <th>MRD</th>
                                    <th>MRDIncr</th>
                                    <th>Deep</th>
                                    <th>Eye</th>
                                    <th>Shallow</th>
                                    <th>Bkgd</th>
                                    <th>Algo Path</th>
                                </tr>
                                <tr>
                                    <td>ID1</td>
                                    <td>
                                        <asp:TextBox ID="txtMRD_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="1" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>                                                        
                                        <asp:RangeValidator ID="rngvalMRD_Location" runat="server" ErrorMessage="MRD(ID1): Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRD_Location" Display="None" Type="Integer"></asp:RangeValidator>                            
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMRDIncrement_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="2" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>                                                        
                                        <asp:RangeValidator ID="rngvalMRDIncrement_Location" runat="server" ErrorMessage="MRDIncr(ID1): Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrement_Location" Display="None" Type="Integer"></asp:RangeValidator>                            
                                    </td>
                                    <td><asp:TextBox ID="txtDeepFactor_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="3" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtEyeFactor_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="4" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtShallowFactor_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="5" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtBackgroundRate_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="6" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtAlgoPath_Location" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="7" /></td>
                                </tr>
                                <tr>
                                    <td>ID+</td>
                                    <td>
                                        <asp:TextBox ID="txtMRDPlus_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="8" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDPlus_Location" runat="server" ErrorMessage="MRD(ID+): Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDPlus_Location" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMRDIncrementPlus_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="9" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDIncrementPlus_Location" runat="server" ErrorMessage="MRDIncr(ID+): Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrementPlus_Location" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td><asp:TextBox ID="txtDeepFactorPlus_Location" runat="server" CssClass="Size XXSmall" TabIndex="10" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtEyeFactorPlus_Location" runat="server" CssClass="Size XXSmall" TabIndex="11" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtShallowFactorPlus_Location" runat="server" CssClass="Size XXSmall" TabIndex="12" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtBackgroundRatePlus_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="13" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtAlgoPathPlus_Location" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="14" /></td>
                                </tr>
                                <tr>
                                    <td>ID2</td>
                                    <td>
                                        <asp:TextBox ID="txtMRD2New_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="15" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRD2New_Location" runat="server" ErrorMessage="MRD(ID2): Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRD2New_Location" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMRDIncrement2New_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="16" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDIncrement2New_Location" runat="server" ErrorMessage="MRDIncr(ID2): Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrement2New_Location" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td><asp:TextBox ID="txtDeepFactor2New_Location" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="17" /></td>
                                    <td><asp:TextBox ID="txtEyeFactor2New_Location" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="18" /></td>
                                    <td><asp:TextBox ID="txtShallowFactor2New_Location" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="19" /></td>
                                    <td><asp:TextBox ID="txtBackgroundRate2New_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="20" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtAlgoPath2New_Location" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="21" /></td>
                                </tr>
                                <tr>
                                    <td>IDElite</td>
                                    <td>
                                        <asp:TextBox ID="txtMRD2_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="22" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRD2_Location" runat="server" ErrorMessage="MRD(IDElite): Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRD2_Location" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMRDIncrement2_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="23" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDIncrement2_Location" runat="server" ErrorMessage="MRDIncr(IDElite): Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrement2_Location" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td><asp:TextBox ID="txtDeepFactor2_Location" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="24" /></td>
                                    <td><asp:TextBox ID="txtEyeFactor2_Location" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="25" /></td>
                                    <td><asp:TextBox ID="txtShallowFactor2_Location" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="26" /></td>
                                    <td><asp:TextBox ID="txtBackgroundRate2_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="27" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtAlgorithmFactor_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="28" onkeyup="return checkForAlphaCharacters(this)" MaxLength="2" /></td>
                                </tr>  
                                
                                  <td>IDVue</td>
                                    <td>
                                        <asp:TextBox ID="txtMRDVue_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="29" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDVue_Location" runat="server" ErrorMessage="MRDVue_Location(IDVue): Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDVue_Location" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMRDIncrementVue_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="30" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                                        <asp:RangeValidator ID="rngvalMRDIncrementVue_Location" runat="server" ErrorMessage="MRDIncr(IDVue): Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrementVue_Location" Display="None" Type="Integer"></asp:RangeValidator>
                                    </td>
                                    <td><asp:TextBox ID="txtDeepFactorVue_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="31" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtEyeFactorVue_Location" runat="server" CssClass="Size XXSmall" TabIndex="32" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtShallowFactorVue_Location" runat="server" CssClass="Size XXSmall" TabIndex="33" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" /></td>
                                    <td><asp:TextBox ID="txtBackgroundRateVue_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="34" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" /></td>
                                    <td><asp:TextBox ID="txtAlgoPathVue_Location" runat="server" CssClass="Size XXSmall" Text="N/A" Enabled="false" TabIndex="35" /></td>
                                 
                                </tr>   
                            </table>
                        </td>                        
                    </tr>
                    <tr>
                        <td>
                            Read Limit:&nbsp;<asp:TextBox ID="txtReadLimit_Location" runat="server" CssClass="Size XSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="29" onkeypress="return checkForWholeNumber(event)" MaxLength="6" />
                            <asp:RangeValidator ID="rngvalReadLimit_Location" runat="server" ErrorMessage="Read Limit: Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100000" ControlToValidate="txtReadLimit_Location" Display="None" Type="Integer"></asp:RangeValidator>
                        </td>                        
                    </tr>
                    <tr>
                        <td>
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Notes:&nbsp;<asp:TextBox ID="txtNotes_Location" runat="server" CssClass="Size XSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="30" TextMode="MultiLine" Width="400px" Height="80px"/>                            
                        </td>                        
                    </tr>
                </table>                

                <%--LOCATION-LEVEL UPDATE MODAL/DIALOG FUNCTION BUTTONS--%>
                <asp:Button ID="btnUpdate_Location" runat="server" style="display: none;" Text="Update" OnClick="btnUpdate_Location_Click" ValidationGroup="UPDATE_LOCATION" TabIndex="31" />
                <asp:Button ID="btnCancel_Location" runat="server" style="display:none;" Text="Cancel" TabIndex="32" />
                <%--END--%>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>   
    <%--END--%>
    <%--FACTORS HISTORY MODAL/DIALOG--%>
    <div id="divFactorsHistoryDialog" >
        <asp:UpdatePanel ID="updtpnlFactorsHistoryLog" runat="server"  >            
            <ContentTemplate>
                <table style="width:100%;">                                        
                    <tr>
                        <td>
                            FACTORS ACCOUNT HISTORY
                        </td>                        
                    </tr>
                    <tr>
                        <td>
                            <b>ACCOUNT #:&nbsp;<asp:Label ID="lblAccountID" runat="server" Font-Bold="true"></asp:Label></b>
                        </td>                        
                    </tr>
                    <tr>
                        <td>     
                            <div id="scrollTopGridView" class="gridView" >
                                <asp:GridView ID="gv_FactorHistoryLog" runat="server" CssClass="OTable" BorderWidth="1px" Style="margin:-1px 0" Width="960px" AutoGenerateColumns="False"
                                    OnRowDataBound="gv_FactorHistoryLog_RowDataBound" 
								    HeaderStyle-CssClass="FixedHeader">                                     
                                    <Columns>
                                   
                                        <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" HeaderStyle-Width="70px" ItemStyle-Width="70px"/>
                                        <asp:BoundField DataField="Time" HeaderText="Time" SortExpression="Time" HeaderStyle-Width="60px" ItemStyle-Width="60px" HeaderStyle-Wrap="true"/>
					                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" HeaderStyle-Width="80px" ItemStyle-Width="80px" HeaderStyle-Wrap="true" />  
                                        <asp:BoundField DataField="ProductType" HeaderText="Product Type" SortExpression="ProductType" HeaderStyle-Width="50px" ItemStyle-Width="50px" HeaderStyle-Wrap="true" />
					                    <asp:BoundField DataField="FactorField" HeaderText="Factor Field" SortExpression="FactorField" HeaderStyle-Width="120px" ItemStyle-Width="120px" HeaderStyle-Wrap="true" /> 
                                        <asp:BoundField DataField="PrevValue" HeaderText="Previous Value" SortExpression="PrevValue" HeaderStyle-Width="60px" ItemStyle-Width="60px" HeaderStyle-Wrap="true" />
                                        <asp:BoundField DataField="AdjValue" HeaderText="Adjusted Value" SortExpression="AdjValue" HeaderStyle-Width="60px" ItemStyle-Width="60px" HeaderStyle-Wrap="true" />
					                    <asp:BoundField DataField="LocationName" HeaderText="Location Name" SortExpression="LocationName" HeaderStyle-Width="100px" ItemStyle-Width="100px" HeaderStyle-Wrap="true"  />  
                                        <asp:BoundField DataField="Note" HeaderText="Note" SortExpression="Note" HeaderStyle-Width="260px" ItemStyle-Width="260px" HeaderStyle-Wrap="true" />                                    

                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div class="NoData">There are no records found!</div>
                                    </EmptyDataTemplate>
                                    <AlternatingRowStyle CssClass="Alt" />
                                    <PagerStyle CssClass="Footer" />
                                </asp:GridView>
                            </div>                            
                        </td>                        
                    </tr>
                    
                </table>                

                <%--ACCOUNT-LEVEL UPDATE MODAL/DIALOG FUNCTION BUTTONS--%>
                <asp:Button ID="btnFactorsHistoryPrint" runat="server" style="display: none;" Text="Print" OnClick="btnFactorsHistoryPrint_Click" TabIndex="0" />
                <asp:Button ID="btnFactorsHistoryCancel" runat="server" style="display:none;" Text="Close" TabIndex="22" />
                <%--END--%>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>   
    <%--END--%>


    <%--Start TabsContainer Section--%>
    <div id="TabContainer1" >

	    <ul>	
            <li><a href="#TabPanel1" id="A1" runat ="server">ID1</a></li>	
            <li><a href="#TabPanel2" id="A2" runat ="server">ID+</a></li>	
            <li><a href="#TabPanel3" id="A3" runat ="server">ID2</a></li>	
            <li><a href="#TabPanel4" id="A4" runat ="server">IDElite</a></li>	
            <li><a href="#TabPanel5" id="A5" runat ="server">IDVue</a></li>	   
	    </ul>

        <!-- Begin ID1  -->   
        <div id="TabPanel1">
            <asp:Panel ID="Panel1" runat="server">

            <asp:UpdatePanel ID="updtpnlGridViewArea_ID1" runat="server"  >
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab1" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                                        
                    
                    <%--ACCOUNT-LEVEL SETTINGS/FACTORS--%>
                    <div id="divToolbar_ID1" runat="server" class="OToolbar JoinTable">
                        <ul>
                            <li>
                                <%--VIEW/EDIT ACCOUNT-LEVEL SETTINGS (MODAL/DIALOG)--%>
                                <asp:LinkButton ID="lnkbtnAccountSettings_ID1" runat="server" ToolTip="Account Settings"
                                CommandName="OverrideAccountLevelSettings" CommandArgument='<%= Request.QueryString["AccountID"] %>'  
                                CssClass="Icon AccountEdit" OnClick="lnkbtnAccountSettings_Click">
                                Account Settings</asp:LinkButton>
                                <%--END--%>
                            </li>
                        </ul>
                    </div>   
                    <%--END--%>

                    <%--LOCATION-LEVEL SETTINGS/FACTORS--%>
                    <asp:GridView ID="gvLocationSettings_ID1" runat="server"
                    AllowPaging="true" 
                    PageSize="20"
                    PagerSettings-Mode="Numeric"
                    CssClass="OTable JoinTable" 
                    AllowSorting="false"
                    DataKeyNames="LocationID"  
                    OnRowCommand="gvLocationSettings_ID1_RowCommand"
                    OnRowDataBound="gvLocationSettings_ID1_RowDataBound" 
                    OnPageIndexChanging="gvLocationSettings_ID1_PageIndexChanging" 
                    AutoGenerateColumns="False"
                    Width="100%" CellPadding="5">
                        <AlternatingRowStyle CssClass="Alt" />
                        <Columns>
                            <asp:BoundField DataField="LocationID" ShowHeader="false" Visible="false" />
                            <asp:TemplateField ShowHeader="false" HeaderStyle-Width="25px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%--VIEW/EDIT LOCATION-LEVEL SETTINGS (MODAL/DIALOG)--%>
                                    <asp:LinkButton ID="lnkbtnUpdateLocationSettings" runat="server" CssClass="Icon LocationEdit" Text="" 
                                    ToolTip='<%# string.Format("Update {0}", Eval("LocationName")) %>' CommandArgument='<%# Eval("LocationID") %>' CommandName="UpdateLocationSettings" />
                                    <%--END--%>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="LocationName" HeaderText="Location" NullDisplayText="*" />
                            <asp:BoundField DataField="MRD" HeaderText="MRD" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="MRDIncrement" HeaderText="MRDIncr" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="DeepFactor" HeaderText="Deep" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="EyeFactor" HeaderText="Eye" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="ShallowFactor" HeaderText="Shallow" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="BackgroundRate" HeaderText="Bckgrd" DataFormatString="{0:f3}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                                    
                            <asp:BoundField DataField="ReadLimit" HeaderText="Read Limit" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <%--<asp:BoundField DataField="AlgorithmFactor" HeaderText="Alg Factor" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField> --%>                            
                            <asp:TemplateField HeaderText="Alg Path" >
								<ItemTemplate>                                    
									<asp:Label ID="lblAlgoPath" runat="server" Text='N/A' />									
								</ItemTemplate> 
                                 <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
							</asp:TemplateField> 

                        </Columns>
                        <EmptyDataTemplate>
                            <div>There are no records associated with this account.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <%--END--%> 
                                                                                         
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                      
        </div>
        <!-- End ID1  -->  
        
        <!-- Begin ID+  -->   
        <div id="TabPanel2">
            <asp:Panel ID="Panel2" runat="server" >

            <asp:UpdatePanel ID="updtpnlGridViewArea_IDPlus" runat="server" >
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab2" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    
                    
                    <%--ACCOUNT-LEVEL SETTINGS/FACTORS--%>
                    <div id="divToolbar_IDPlus" runat="server" class="OToolbar JoinTable">
                        <ul>
                            <li>
                                <%--VIEW/EDIT ACCOUNT-LEVEL SETTINGS (MODAL/DIALOG)--%>
                                <asp:LinkButton ID="lnkbtnAccountSettings_IDPlus" runat="server" ToolTip="Account Settings"
                                CommandName="OverrideAccountLevelSettings" CommandArgument='<%= Request.QueryString["AccountID"] %>'  
                                CssClass="Icon AccountEdit" OnClick="lnkbtnAccountSettings_Click">
                                Account Settings</asp:LinkButton>
                                <%--END--%>
                            </li>
                        </ul>
                    </div>   
                    <%--END--%>

                    <%--LOCATION-LEVEL SETTINGS/FACTORS--%>
                    <asp:GridView ID="gvLocationSettings_IDPlus" runat="server"
                    AllowPaging="true" 
                    PageSize="20"
                    PagerSettings-Mode="Numeric"
                    CssClass="OTable JoinTable" 
                    AllowSorting="false"
                    DataKeyNames="LocationID"  
                    OnRowCommand="gvLocationSettings_IDPlus_RowCommand"
                    OnRowDataBound="gvLocationSettings_IDPlus_RowDataBound" 
                    OnPageIndexChanging="gvLocationSettings_IDPlus_PageIndexChanging" 
                    AutoGenerateColumns="False"
                    Width="100%" CellPadding="5">
                        <AlternatingRowStyle CssClass="Alt" />
                        <Columns>
                            <asp:BoundField DataField="LocationID" ShowHeader="false" Visible="false" />
                            <asp:TemplateField ShowHeader="false" HeaderStyle-Width="25px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%--VIEW/EDIT LOCATION-LEVEL SETTINGS (MODAL/DIALOG)--%>
                                    <asp:LinkButton ID="lnkbtnUpdateLocationSettings" runat="server" CssClass="Icon LocationEdit" Text="" 
                                    ToolTip='<%# string.Format("Update {0}", Eval("LocationName")) %>' CommandArgument='<%# Eval("LocationID") %>' CommandName="UpdateLocationSettings" />
                                    <%--END--%>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="LocationName" HeaderText="Location" NullDisplayText="*" />                                    
                                    
                            <asp:BoundField DataField="MRDPlus" HeaderText="MRD" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="MRDIncrementPlus" HeaderText="MRDIncr" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="DeepFactorPlus" HeaderText="Deep" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="EyeFactorPlus" HeaderText="Eye" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="ShallowFactorPlus" HeaderText="Shallow" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="BackgroundRatePlus" HeaderText="Bckgrd" DataFormatString="{0:f3}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="ReadLimit" HeaderText="Read Limit" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <%--<asp:BoundField DataField="AlgorithmFactor" HeaderText="Alg Factor" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>  --%>  
                            <asp:TemplateField HeaderText="Alg Path" >
								<ItemTemplate>                                    
									<asp:Label ID="lblAlgoPath" runat="server" Text='N/A' />									
								</ItemTemplate>  
                                 <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
							</asp:TemplateField> 

                        </Columns>
                        <EmptyDataTemplate>
                            <div>There are no records associated with this account.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <%--END--%>
                                                                                         
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                       
        </div>
        <!-- End ID+  --> 

        <!-- Begin ID2  -->   
        <div id="TabPanel3">
            <asp:Panel ID="Panel3" runat="server" >

            <asp:UpdatePanel ID="updtpnlGridViewArea_ID2" runat="server" >
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab3" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    

                    <%--ACCOUNT-LEVEL SETTINGS/FACTORS--%>
                    <div id="divToolbar_ID2" runat="server" class="OToolbar JoinTable">
                        <ul>
                            <li>
                                <%--VIEW/EDIT ACCOUNT-LEVEL SETTINGS (MODAL/DIALOG)--%>
                                <asp:LinkButton ID="lnkbtnAccountSettings_ID2" runat="server" ToolTip="Account Settings"
                                CommandName="OverrideAccountLevelSettings" CommandArgument='<%= Request.QueryString["AccountID"] %>'  
                                CssClass="Icon AccountEdit" OnClick="lnkbtnAccountSettings_Click">
                                Account Settings</asp:LinkButton>
                                <%--END--%>
                            </li>
                        </ul>
                    </div>   
                    <%--END--%>

                    <%--LOCATION-LEVEL SETTINGS/FACTORS--%>
                    <asp:GridView ID="gvLocationSettings_ID2" runat="server"
                    AllowPaging="true" 
                    PageSize="20"
                    PagerSettings-Mode="Numeric"
                    CssClass="OTable JoinTable" 
                    AllowSorting="false"
                    DataKeyNames="LocationID"  
                    OnRowCommand="gvLocationSettings_ID2_RowCommand"
                    OnRowDataBound="gvLocationSettings_ID2_RowDataBound" 
                    OnPageIndexChanging="gvLocationSettings_ID2_PageIndexChanging" 
                    AutoGenerateColumns="False"
                    Width="100%" CellPadding="5">
                        <AlternatingRowStyle CssClass="Alt" />
                        <Columns>
                            <asp:BoundField DataField="LocationID" ShowHeader="false" Visible="false" />
                            <asp:TemplateField ShowHeader="false" HeaderStyle-Width="25px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%--VIEW/EDIT LOCATION-LEVEL SETTINGS (MODAL/DIALOG)--%>
                                    <asp:LinkButton ID="lnkbtnUpdateLocationSettings" runat="server" CssClass="Icon LocationEdit" Text="" 
                                    ToolTip='<%# string.Format("Update {0}", Eval("LocationName")) %>' CommandArgument='<%# Eval("LocationID") %>' CommandName="UpdateLocationSettings" />
                                    <%--END--%>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="LocationName" HeaderText="Location" NullDisplayText="*" />                                    

                            <asp:BoundField DataField="MRD2New" HeaderText="MRD" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="MRDIncrement2New" HeaderText="MRDIncr" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField> 
                            
                            <asp:TemplateField HeaderText="Deep" >
								<ItemTemplate>                                    
									<asp:Label ID="lblDeepFactor2New" runat="server" Text='N/A' />									
								</ItemTemplate> 
                                 <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
							</asp:TemplateField> 
                            <asp:TemplateField HeaderText="Eye" >
								<ItemTemplate>                                    
									<asp:Label ID="lblEyeFactor2New" runat="server" Text='N/A' />									
								</ItemTemplate> 
                                 <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
							</asp:TemplateField> 
                            <asp:TemplateField HeaderText="Shallow" >
								<ItemTemplate>                                    
									<asp:Label ID="lblShallowFactor2New" runat="server" Text='N/A' />									
								</ItemTemplate> 
                                 <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
							</asp:TemplateField> 

                            <asp:BoundField DataField="BackgroundRate2New" HeaderText="Bckgrd" DataFormatString="{0:f3}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="ReadLimit" HeaderText="Read Limit" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <%--<asp:BoundField DataField="AlgorithmFactor" HeaderText="Alg Factor" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>--%>
                            <asp:TemplateField HeaderText="Alg Path" >
								<ItemTemplate>                                    
									<asp:Label ID="lblAlgoPath" runat="server" Text='N/A' />									
								</ItemTemplate>  
                                 <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
							</asp:TemplateField> 
                                    
                        </Columns>
                        <EmptyDataTemplate>
                            <div>There are no records associated with this account.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <%--END--%>
                                                                                                             
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                      
        </div>
        <!-- End ID2  --> 

        <!-- Begin IDElite  -->   
        <div id="TabPanel4">
            <asp:Panel ID="Panel4" runat="server">

            <asp:UpdatePanel ID="updtpnlGridViewArea_ID2Elite" runat="server" >
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab4" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    
                    
                    <%--ACCOUNT-LEVEL SETTINGS/FACTORS--%>
                    <div id="divToolbar_ID2Elite" runat="server" class="OToolbar JoinTable">
                        <ul>
                            <li>
                                <%--VIEW/EDIT ACCOUNT-LEVEL SETTINGS (MODAL/DIALOG)--%>
                                <asp:LinkButton ID="lnkbtnAccountSettings_ID2Elite" runat="server" ToolTip="Account Settings"
                                CommandName="OverrideAccountLevelSettings" CommandArgument='<%= Request.QueryString["AccountID"] %>'  
                                CssClass="Icon AccountEdit" OnClick="lnkbtnAccountSettings_Click">
                                Account Settings</asp:LinkButton>
                                <%--END--%>
                            </li>
                        </ul>
                    </div>   
                    <%--END--%>

                    <%--LOCATION-LEVEL SETTINGS/FACTORS--%>
                    <asp:GridView ID="gvLocationSettings_ID2Elite" runat="server"
                    AllowPaging="true" 
                    PageSize="20"
                    PagerSettings-Mode="Numeric"
                    CssClass="OTable JoinTable" 
                    AllowSorting="false"
                    DataKeyNames="LocationID"  
                    OnRowCommand="gvLocationSettings_ID2Elite_RowCommand"
                    OnRowDataBound="gvLocationSettings_ID2Elite_RowDataBound" 
                    OnPageIndexChanging="gvLocationSettings_ID2Elite_PageIndexChanging" 
                    AutoGenerateColumns="False"
                    Width="100%" CellPadding="5">
                        <AlternatingRowStyle CssClass="Alt" />
                        <Columns>
                            <asp:BoundField DataField="LocationID" ShowHeader="false" Visible="false" />
                            <asp:TemplateField ShowHeader="false" HeaderStyle-Width="25px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%--VIEW/EDIT LOCATION-LEVEL SETTINGS (MODAL/DIALOG)--%>
                                    <asp:LinkButton ID="lnkbtnUpdateLocationSettings" runat="server" CssClass="Icon LocationEdit" Text="" 
                                    ToolTip='<%# string.Format("Update {0}", Eval("LocationName")) %>' CommandArgument='<%# Eval("LocationID") %>' CommandName="UpdateLocationSettings" />
                                    <%--END--%>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="LocationName" HeaderText="Location" NullDisplayText="*" />
                                    
                            <asp:BoundField DataField="MRD2" HeaderText="MRD" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="MRDIncrement2" HeaderText="MRDIncr" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>  
                            
                            <asp:TemplateField HeaderText="Deep" >
								<ItemTemplate>                                    
									<asp:Label ID="lblDeepFactor2" runat="server" Text='N/A' />									
								</ItemTemplate> 
                                 <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
							</asp:TemplateField> 
                            <asp:TemplateField HeaderText="Eye" >
								<ItemTemplate>                                    
									<asp:Label ID="lblEyeFactor2" runat="server" Text='N/A' />									
								</ItemTemplate> 
                                 <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
							</asp:TemplateField> 
                            <asp:TemplateField HeaderText="Shallow" >
								<ItemTemplate>                                    
									<asp:Label ID="lblShallowFactor2" runat="server" Text='N/A' />									
								</ItemTemplate> 
                                 <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
							</asp:TemplateField> 

                            <asp:BoundField DataField="BackgroundRate2" HeaderText="Bckgrd" DataFormatString="{0:f3}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                                                                        
                            <asp:BoundField DataField="ReadLimit" HeaderText="Read Limit" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="AlgorithmFactor" HeaderText="Alg Path" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>                                    

                        </Columns>
                        <EmptyDataTemplate>
                            <div>There are no records associated with this account.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <%--END--%>
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                      
        </div>
        <!-- End IDElite  -->    
        
        <!-- Begin IDVue  -->   
        <div id="TabPanel5">
            <asp:Panel ID="Panel5" runat="server">

            <asp:UpdatePanel ID="updtpnlGridViewArea_IDVue" runat="server" >
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab5" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    
                    
                    <%--ACCOUNT-LEVEL SETTINGS/FACTORS--%>
                    <div id="divToolbar_IDVue" runat="server" class="OToolbar JoinTable">
                        <ul>
                            <li>
                                <%--VIEW/EDIT ACCOUNT-LEVEL SETTINGS (MODAL/DIALOG)--%>
                                <asp:LinkButton ID="lnkbtnAccountSettings_IDVue" runat="server" ToolTip="Account Settings"
                                CommandName="OverrideAccountLevelSettings" CommandArgument='<%= Request.QueryString["AccountID"] %>'  
                                CssClass="Icon AccountEdit" OnClick="lnkbtnAccountSettings_Click">
                                Account Settings</asp:LinkButton>
                                <%--END--%>
                            </li>
                        </ul>
                    </div>   
                    <%--END--%>

                    <%--LOCATION-LEVEL SETTINGS/FACTORS--%>
                    <asp:GridView ID="gvLocationSettings_IDVue" runat="server"
                    AllowPaging="true" 
                    PageSize="20"
                    PagerSettings-Mode="Numeric"
                    CssClass="OTable JoinTable" 
                    AllowSorting="false"
                    DataKeyNames="LocationID"  
                    OnRowCommand="gvLocationSettings_IDVue_RowCommand"
                    OnRowDataBound="gvLocationSettings_IDVue_RowDataBound" 
                    OnPageIndexChanging="gvLocationSettings_IDVue_PageIndexChanging" 
                    AutoGenerateColumns="False"
                    Width="100%" CellPadding="5">
                        <AlternatingRowStyle CssClass="Alt" />
                        <Columns>
                            <asp:BoundField DataField="LocationID" ShowHeader="false" Visible="false" />
                            <asp:TemplateField ShowHeader="false" HeaderStyle-Width="25px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%--VIEW/EDIT LOCATION-LEVEL SETTINGS (MODAL/DIALOG)--%>
                                    <asp:LinkButton ID="lnkbtnUpdateLocationSettings" runat="server" CssClass="Icon LocationEdit" Text="" 
                                    ToolTip='<%# string.Format("Update {0}", Eval("LocationName")) %>' CommandArgument='<%# Eval("LocationID") %>' CommandName="UpdateLocationSettings" />
                                    <%--END--%>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="LocationName" HeaderText="Location" NullDisplayText="*" />
                                    
                            <asp:BoundField DataField="MRDVue" HeaderText="MRD" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            
                            <asp:BoundField DataField="MRDIncrementVue" HeaderText="MRDIncr" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="DeepFactorVue" HeaderText="Deep" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                        
                            <asp:BoundField DataField="EyeFactorVue" HeaderText="Eye" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="ShallowFactorVue" HeaderText="Shallow" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                        
                            <asp:BoundField DataField="BackgroundRateVue" HeaderText="Bckgrd" DataFormatString="{0:f3}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>         

                            <asp:BoundField DataField="ReadLimit" HeaderText="Read Limit" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:BoundField>

                            <asp:TemplateField HeaderText="Alg Path" >
								<ItemTemplate>                                    
									<asp:Label ID="lblAlgoPath" runat="server" Text='N/A' />									
								</ItemTemplate>  
                                 <HeaderStyle CssClass="CenterAlign" />
                                <ItemStyle HorizontalAlign="Center" />
							</asp:TemplateField> 

                        </Columns>
                        <EmptyDataTemplate>
                            <div>There are no records associated with this account.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <%--END--%>
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                      
        </div>
        <!-- End IDElite  --> 

        <%--BACK TO FACTORS SEARCH (BY ACCOUNT #) PAGE--%>
        <div class="Buttons">
            <div class="ButtonHolder">
                <asp:Button ID="Button1" runat="server" CssClass="OButton" Text="Back to Search Page" OnClick="btnBackToFactorsSearch_Click" />
            </div>
        </div>
        <%--END--%>  
                       
    </div>
    <%--End TabsContainer Section--%>

      

</asp:Content>

