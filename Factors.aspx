<%@ Page Title="Factors Page" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_Factors" Codebehind="Factors.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        .AlteredField {
            font-weight:bold;
            color: blue;
        }
    </style>
    <script type="text/javascript" src="/scripts/jquery.placeholder.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
        });
        
        function JQueryControlsLoad() {
            // Account Settings Modal/Dialog.
            $('#divAccountSettingsDialog').dialog({
                autoOpen: false,
                width: 485,
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
                width: 485,
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
            var ex = /^[0-9]+\.?[0-9]*$/;

            if (ex.test(elmnt.value) == false) {
                elmnt.value = elmnt.value.substring(0, elmnt.value.length - 1);
                return true;
            }

            return false;
        }

        // Allows User to type in Alpha Characters (2) only (used for Algorithm Factor).
        function checkForAlphaCharacters(elmnt) {
            var ex = /[a-zA-z]/;

            if (ex.test(elmnt.value) == false)
                return false;

            return true;
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <%--ACCOUNT # & COMPANY NAME--%>
    <div id="divAccountIDAndCompanyName" style="padding: 0px 10px 0px 10px;">
        <h2>Account:&nbsp;<asp:Label ID="lblAccountIDAndCompanyName" runat="server" Font-Bold="true"></asp:Label></h2>
    </div>
    <%--END--%>
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
    <div id="divAccountSettingsDialog">
        <asp:UpdatePanel ID="updtpnlAccountSettings" runat="server">
            <ContentTemplate>
                <div class="OForm">
                    <%--ACCOUNT #--%>
                    <div class="Row">
                        <div class="Label">Account #:</div>
                        <div class="Control"><asp:Label ID="lblAccountID" runat="server" CssClass="LabelValue" /></div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--COMPANY NAME--%>
                    <div class="Row">
                        <div class="Label">Company Name:</div>
                        <div class="Control"><asp:Label ID="lblCompanyName" runat="server" CssClass="LabelValue" /></div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>

                    <%--MRD (ID1)--%>
                    <div class="Row">
                        <div class="Label">MRD:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRD_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="1" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRDFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRD_Account" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRD_Account" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--MRD INCREMENT (ID1)--%>
                    <div class="Row">
                        <div class="Label">MRD Increment:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRDIncrement_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="2" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRDIncrementFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRDIncrement_Account" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrement_Account" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                     <%--DEEP--%>
                    <div class="Row">
                        <div class="Label">Deep:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtDeepFactor_Account" runat="server" CssClass="Size XXSmall" TabIndex="3" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblDeepFactorFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--EYE--%>
                    <div class="Row">
                        <div class="Label">Eye:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEyeFactor_Account" runat="server" CssClass="Size XXSmall" TabIndex="4" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblEyeFactorFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label> 
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--SHALLOW--%>
                    <div class="Row">
                        <div class="Label">Shallow:</div>
                        <div class="Control">
							<asp:TextBox ID="txtShallowFactor_Account" runat="server" CssClass="Size XXSmall" TabIndex="5" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblShallowFactorFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label> 
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--BACKGROUND RATE--%>
                    <div class="Row">
                        <div class="Label">Background Rate:</div>
                        <div class="Control">
							<asp:TextBox ID="txtBackgroundRate_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="6" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" />
                            <asp:Label ID="lblBackgroundRateFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>

                    <%--MRD (ID+)--%>
                    <div class="Row">
                        <div class="Label">MRD+:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRDPlus_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="7" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRDPlusFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRDPlus_Account" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDPlus_Account" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--MRD INCREMENT (ID1+)--%>
                    <div class="Row">
                        <div class="Label">MRD Increment+:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRDIncrementPlus_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="8" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRDIncrementPlusFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRDIncrementPlus_Account" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrementPlus_Account" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                   
                    <%--DEEP ID+--%>
                    <div class="Row">
                        <div class="Label">Deep+:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtDeepFactorPlus_Account" runat="server" CssClass="Size XXSmall" TabIndex="9" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblDeepFactorPlusFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--EYE ID+--%>
                    <div class="Row">
                        <div class="Label">Eye+:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEyeFactorPlus_Account" runat="server" CssClass="Size XXSmall" TabIndex="10" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblEyeFactorPlusFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label> 
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--SHALLOW ID+--%>
                    <div class="Row">
                        <div class="Label">Shallow+:</div>
                        <div class="Control">
							<asp:TextBox ID="txtShallowFactorPlus_Account" runat="server" CssClass="Size XXSmall" TabIndex="11" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblShallowFactorPlusFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label> 
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--BACKGROUND RATE ID+--%>
                    <div class="Row">
                        <div class="Label">Background Rate+:</div>
                        <div class="Control">
							<asp:TextBox ID="txtBackgroundRatePlus_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="12" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" />
                            <asp:Label ID="lblBackgroundRatePlusFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>

                    <%--MRD 2 (ID2)--%>
                    <div class="Row">
                        <div class="Label">MRD 2:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRD2_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="13" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRD2From_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRD2_Account" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRD2_Account" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--MRD INCREMENT 2 (ID2)--%>
                    <div class="Row">
                        <div class="Label">MRD Increment 2:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRDIncrement2_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="14" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRDIncrement2From_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRDIncrement2_Account" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrement2_Account" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--BACKGROUND RATE 2--%>
                    <div class="Row">
                        <div class="Label">Background Rate 2:</div>
                        <div class="Control">
							<asp:TextBox ID="txtBackgroundRate2_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="15" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" />
                            <asp:Label ID="lblBackgroundRate2From_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>

                    <%--READ LIMIT--%>
                    <div class="Row">
                        <div class="Label">Read Limit:</div>
                        <div class="Control">
							<asp:TextBox ID="txtReadLimit_Account" runat="server" CssClass="Size XSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="16" onkeypress="return checkForWholeNumber(event)" MaxLength="6" />
                            <asp:Label ID="lblReadLimitFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalReadLimit_Account" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_ACCOUNT" MinimumValue="1" MaximumValue="100000" ControlToValidate="txtReadLimit_Account" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>

                    <%--ALGORITHM FACTOR (ID2)--%>
                    <div class="Row">
                        <div class="Label">Algorithm Factor:</div>
                        <div class="Control">
							<asp:TextBox ID="txtAlgorithmFactor_Account" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_ACCOUNT" TabIndex="17" onkeyup="return checkForAlphaCharacters(this)" MaxLength="2" />
                            <asp:Label ID="lblAlgorithmFactorFrom_Account" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
		        </div>
                <%--ACCOUNT-LEVEL UPDATE MODAL/DIALOG FUNCTION BUTTONS--%>
                <asp:Button ID="btnUpdate_Account" runat="server" style="display: none;" Text="Update" OnClick="btnUpdate_Account_Click" ValidationGroup="UPDATE_ACCOUNT" TabIndex="18" />
                <asp:Button ID="btnCancel_Account" runat="server" style="display:none;" Text="Cancel" TabIndex="19" />
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
                <div class="OForm">
                    <%--LOCATION NAME--%>
                    <div class="Row">
                        <div class="Label">Location Name:</div>
                        <div class="Control"><asp:Label ID="lblLocationName" runat="server" CssClass="LabelValue" /></div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>

                    <%--MRD (ID1)--%>
                    <div class="Row">
                        <div class="Label">MRD:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRD_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="1" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRDFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRD_Location" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRD_Location" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--MRD INCREMENT (ID1)--%>
                    <div class="Row">
                        <div class="Label">MRD Increment:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRDIncrement_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="2" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRDIncrementFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRDIncrement_Location" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrement_Location" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--DEEP--%>
                    <div class="Row">
                        <div class="Label">Deep:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtDeepFactor_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="3" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblDeepFactorFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--EYE--%>
                    <div class="Row">
                        <div class="Label">Eye:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEyeFactor_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="4" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblEyeFactorFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label> 
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--SHALLOW--%>
                    <div class="Row">
                        <div class="Label">Shallow:</div>
                        <div class="Control">
							<asp:TextBox ID="txtShallowFactor_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="5" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblShallowFactorFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label> 
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--BACKGROUND RATE--%>
                    <div class="Row">
                        <div class="Label">Background Rate:</div>
                        <div class="Control">
							<asp:TextBox ID="txtBackgroundRate_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="6" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" />
                            <asp:Label ID="lblBackgroundRateFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>

                    <%--MRD (ID1+)--%>
                    <div class="Row">
                        <div class="Label">MRD+:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRDPlus_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="7" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRDPlusFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRDPlus_Location" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDPlus_Location" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--MRD INCREMENT (ID1+)--%>
                    <div class="Row">
                        <div class="Label">MRD Increment+:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRDIncrementPlus_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="8" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRDIncrementPlusFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRDIncrementPlus_Location" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrementPlus_Location" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--DEEP ID+--%>
                    <div class="Row">
                        <div class="Label">Deep+:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtDeepFactorPlus_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="9" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblDeepFactorPlusFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--EYE ID+--%>
                    <div class="Row">
                        <div class="Label">Eye+:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEyeFactorPlus_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="10" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblEyeFactorPlusFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label> 
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--SHALLOW ID+--%>
                    <div class="Row">
                        <div class="Label">Shallow+:</div>
                        <div class="Control">
							<asp:TextBox ID="txtShallowFactorPlus_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="11" onkeyup="return checkForExtraDecimal(this)" MaxLength="4" />
                            <asp:Label ID="lblShallowFactorPlusFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label> 
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--BACKGROUND RATE ID+--%>
                    <div class="Row">
                        <div class="Label">Background Rate+:</div>
                        <div class="Control">
							<asp:TextBox ID="txtBackgroundRatePlus_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="12" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" />
                            <asp:Label ID="lblBackgroundRatePlusFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>

                    <%--MRD 2 (ID2)--%>
                    <div class="Row">
                        <div class="Label">MRD 2:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRD2_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="13" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRD2From_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRD2_Location" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRD2_Location" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--MRD INCREMENT 2 (ID2)--%>
                    <div class="Row">
                        <div class="Label">MRD Increment 2:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtMRDIncrement2_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="14" onkeypress="return checkForWholeNumber(event)" MaxLength="3"></asp:TextBox>
                            <asp:Label ID="lblMRDIncrement2From_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalMRDIncrement2_Location" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100" ControlToValidate="txtMRDIncrement2_Location" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>                    
                    <%--BACKGROUND RATE 2--%>
                    <div class="Row">
                        <div class="Label">Background Rate 2:</div>
                        <div class="Control">
							<asp:TextBox ID="txtBackgroundRate2_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="15" onkeyup="return checkForExtraDecimal(this)" MaxLength="5" />
                            <asp:Label ID="lblBackgroundRate2From_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>

                    <%--READ LIMIT--%>
                    <div class="Row">
                        <div class="Label">Read Limit:</div>
                        <div class="Control">
							<asp:TextBox ID="txtReadLimit_Location" runat="server" CssClass="Size XSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="16" onkeypress="return checkForWholeNumber(event)" MaxLength="6" />
                            <asp:Label ID="lblReadLimitFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                            <span class="InlineError">
                                <asp:RangeValidator ID="rngvalReadLimit_Location" runat="server" ErrorMessage="Out of range." ValidationGroup="UPDATE_LOCATION" MinimumValue="1" MaximumValue="100000" ControlToValidate="txtReadLimit_Location" Display="Dynamic" Type="Integer"></asp:RangeValidator>
                            </span>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>

                    <%--ALGORITHM FACTOR (ID2)--%>
                    <div class="Row">
                        <div class="Label">Algorithm Factor:</div>
                        <div class="Control">
							<asp:TextBox ID="txtAlgorithmFactor_Location" runat="server" CssClass="Size XXSmall" ValidationGroup="UPDATE_LOCATION" TabIndex="17" onkeyup="return checkForAlphaCharacters(this)" MaxLength="2" />
                            <asp:Label ID="lblAlgorithmFactorFrom_Location" runat="server" Text="" style="font-style: italic; color: #AAAAAA;"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
		        </div>
                <%--LOCATION-LEVEL UPDATE MODAL/DIALOG FUNCTION BUTTONS--%>
                <asp:Button ID="btnUpdate_Location" runat="server" style="display: none;" Text="Update" OnClick="btnUpdate_Location_Click" ValidationGroup="UPDATE_LOCATION" TabIndex="18" />
                <asp:Button ID="btnCancel_Location" runat="server" style="display:none;" Text="Cancel" TabIndex="19" />
                <%--END--%>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>   
    <%--END--%>
    <div id="divMainContentArea">
        <asp:UpdatePanel ID="updtpnlMainContentArea" runat="server">
            <ContentTemplate>
                <div id="divToolbar" runat="server" class="OToolbar JoinTable">
                    <ul>
                        <li>
                            <%--VIEW/EDIT ACCOUNT-LEVEL SETTINGS (MODAL/DIALOG)--%>
                            <asp:LinkButton ID="lnkbtnAccountSettings" runat="server" ToolTip="Account Settings"
                            CommandName="OverrideAccountLevelSettings" CommandArgument='<%= Request.QueryString["AccountID"] %>'  
                            CssClass="Icon AccountEdit" OnClick="lnkbtnAccountSettings_Click">
                            Account Settings</asp:LinkButton>
                            <%--END--%>
                        </li>
                    </ul>
                </div>
                <div id="divLocationSettingsGridView">
                    <asp:UpdatePanel ID="updtpnlGridViewArea" runat="server">
                        <ContentTemplate>
                            <%--LOCATION-LEVEL SETTINGS/FACTORS--%>
                            <asp:GridView ID="gvLocationSettings" runat="server"
                            AllowPaging="true" 
                            PageSize="20"
                            PagerSettings-Mode="Numeric"
                            CssClass="OTable JoinTable" 
                            AllowSorting="false"
                            DataKeyNames="LocationID"  
                            OnRowCommand="gvLocationSettings_RowCommand"
                            OnRowDataBound="gvLocationSettings_RowDataBound" 
                            OnPageIndexChanging="gvLocationSettings_PageIndexChanging" 
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

                                    <asp:BoundField DataField="MRD2" HeaderText="MRD2" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                        <HeaderStyle CssClass="CenterAlign" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="MRDIncrement2" HeaderText="MRDIncr2" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                        <HeaderStyle CssClass="CenterAlign" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>                                    
                                    <asp:BoundField DataField="BackgroundRate2" HeaderText="Bckgrd2" DataFormatString="{0:f3}" HtmlEncode="false" NullDisplayText="*">
                                        <HeaderStyle CssClass="CenterAlign" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    
                                    <asp:BoundField DataField="MRDPlus" HeaderText="MRD+" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                        <HeaderStyle CssClass="CenterAlign" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="MRDIncrementPlus" HeaderText="MRDIncr+" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                        <HeaderStyle CssClass="CenterAlign" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="DeepFactorPlus" HeaderText="Deep+" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                        <HeaderStyle CssClass="CenterAlign" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="EyeFactorPlus" HeaderText="Eye+" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                        <HeaderStyle CssClass="CenterAlign" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="ShallowFactorPlus" HeaderText="Shallow+" DataFormatString="{0:f2}" HtmlEncode="false" NullDisplayText="*">
                                        <HeaderStyle CssClass="CenterAlign" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="BackgroundRatePlus" HeaderText="Bckgrd+" DataFormatString="{0:f3}" HtmlEncode="false" NullDisplayText="*">
                                        <HeaderStyle CssClass="CenterAlign" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>

                                    <asp:BoundField DataField="ReadLimit" HeaderText="Read Limit" DataFormatString="{0:f0}" HtmlEncode="false" NullDisplayText="*">
                                        <HeaderStyle CssClass="CenterAlign" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="AlgorithmFactor" HeaderText="Alg Factor" HtmlEncode="false" NullDisplayText="*">
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
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <%--BACK TO FACTORS SEARCH (BY ACCOUNT #) PAGE--%>
        <div class="Buttons">
            <div class="ButtonHolder">
                <asp:Button ID="btnBackToFactorsSearch" runat="server" CssClass="OButton" Text="Back to Search Page" OnClick="btnBackToFactorsSearch_Click" />
            </div>
        </div>
        <%--END--%>
    </div>  
</asp:Content>

