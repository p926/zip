<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_EditCRMAccounts" Codebehind="EditCRMAccounts.aspx.cs" %>

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
        function pageLoad(sender, args) {
            $(document).ready(function () {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
                JQueryControlsLoad();
            });
        }

        function JQueryControlsLoad() {
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
            var addeditIndex = parseInt($('#<%= hdnfldAccordionIndex.ClientID %>').val());

            // Accordion for Add Location Information.
            $("#accordionLocationInformation").accordion({
                header: "h3",
                autoHeight: false,
                active: addeditIndex,
                change: function (event, ui) {
                    var index = $(this).accordion("option", "active");
                    $('#<%= hdnfldAccordionIndex.ClientID %>').val(index);
                }
            });

            // Modal/Dialog for Adding/Editing Location Information.
            $('#divLocationInformationForm').dialog({
                modal: true,
                autoOpen: false,
                width: 700,
                resizable: false,
                title: "Location Information",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('#<%= txtBillingCompanyName.ClientID %>').focus();
                },
                buttons: {
                    "Save": function () {
                        $('#<%= btnSave_Location.ClientID %>').click();
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

            // Modal/Dialog for Adding/Editing Location Information.
            $('#divOrderInformationForm').dialog({
                modal: true,
                autoOpen: false,
                width: 400,
                resizable: false,
                title: "Order Information",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('#<%= ddlProductGroupID.ClientID %>').focus();
                },
                buttons: {
                    "Save": function () {
                        $('#<%= btnSave_Order.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancel_Order.ClientID %>').click();
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
    <asp:HiddenField ID="hdnfldAccordionIndex" runat="server" Value="0" />

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

    <%--ADD/EDIT LOCATION INFORMATION FORM MODAL/DIALOG--%>
    <div id="divLocationInformationForm">
        <asp:UpdatePanel ID="updtpnlLocationInformationForm" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldCommandName" runat="server" Value="" />
                <asp:HiddenField ID="hdnfldCRMLocationID" runat="server" Value="0" />
                <div id="accordionLocationInformation" style="margin-top: 10px;">
                    <div id="divBillingInformation" runat="server">
			            <h3><a href="#">Billing Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--BILLING COMPANY NAME (COMPANY)--%>
                            <div class="Row">
                                <div class="Label">Company<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtBillingCompanyName" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalBillingCompanyName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtBillingCompanyName" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING FIRST NAME--%>
                            <div class="Row">
                                <div class="Label">First Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlBillingPrefix" runat="server">
                                        <asp:ListItem Text="" Value="" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="Dr." Value="Dr."></asp:ListItem>
                                        <asp:ListItem Text="Miss" Value="Miss"></asp:ListItem>
                                        <asp:ListItem Text="Mr." Value="Mr."></asp:ListItem>
                                        <asp:ListItem Text="Mrs." Value="Mrs."></asp:ListItem>
                                        <asp:ListItem Text="Ms." Value="Ms."></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:TextBox ID="txtBillingFirstName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalBillingFirstName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtBillingFirstName" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING LAST NAME--%>
                            <div class="Row">
                                <div class="Label">Last Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtBillingLastName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalBillingLastName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtBillingLastName" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING ADDRESS LINE 1--%>
                            <div class="Row">
                                <div class="Label">Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtBillingAddressLine1" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalBillingAddressLine1" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtBillingAddressLine1" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING ADDRESS LINE 2--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtBillingAddressLine2" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING ADDRESS LINE 3--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtBillingAddressLine3" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING COUNTRY--%>
                            <div class="Row">
                                <div class="Label">Country<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlBillingCountry" runat="server" AutoPostBack="true" 
                                    AppendDataBoundItems="true" OnSelectedIndexChanged="ddlBillingCountry_SelectedIndexChanged">
                                        <asp:ListItem Text="-Select Country-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalBillingCountry" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlBillingCountry" ValidationGroup="ADDEDITLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING CITY--%>
                            <div class="Row">
                                <div class="Label">City<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtBillingCity" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalBillingCity" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtBillingCity" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING STATE/POSTAL CODE--%>
                            <div class="Row">
                                <div class="Label">State/Postal<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlBillingState" runat="server" AppendDataBoundItems="true">
                                        <asp:ListItem Text="-Select State-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalBillingState" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlBillingState" ValidationGroup="ADDEDITLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                    <asp:TextBox ID="txtBillingPostalCode" runat="server" Text="" CssClass="Size Small" MaxLength="15"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalBillingPostalCode" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtBillingPostalCode" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING TELEPHONE--%>
                            <div class="Row">
                                <div class="Label">Telephone<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtBillingTelephone" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalBillingTelephone" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtBillingTelephone" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="regexpvalBillingTelephone" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtBillingTelephone" ValidationGroup="ADDEDITLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING FAX--%>
                            <div class="Row">
                                <div class="Label">Fax:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtBillingFax" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="regexpvalBillingFax" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtBillingFax" ValidationGroup="ADDEDITLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--BILLING E-MAIL ADDRESS--%>
                            <div class="Row">
                                <div class="Label">E-Mail Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtBillingEmailAddress" runat="server" Text="" CssClass="Size Medium" MaxLength="60"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalBillingEmailAddress" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtBillingEmailAddress" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="regexpvalBillingEmailAddress" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtBillingEmailAddress" ValidationGroup="ADDEDITLOCATION"     
                                    ValidationExpression="[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </div>
                    <div id="divShippingInformation" runat="server">
			            <h3><a href="#">Shipping Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--THE SAME AS BILLING INFORMATION--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxSameAsBillingInformation" runat="server" Text="The same as billing information." Checked="false" 
                                    AutoPostBack="true" OnCheckedChanged="chkbxSameAsBillingInformation_CheckedChanged" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING COMPANY NAME (COMPANY)--%>
                            <div class="Row">
                                <div class="Label">Company<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtShippingCompanyName" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalShippingCompanyName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtShippingCompanyName" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING FIRST NAME--%>
                            <div class="Row">
                                <div class="Label">First Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlShippingPrefix" runat="server">
                                        <asp:ListItem Text="" Value="" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="Dr." Value="Dr."></asp:ListItem>
                                        <asp:ListItem Text="Miss" Value="Miss"></asp:ListItem>
                                        <asp:ListItem Text="Mr." Value="Mr."></asp:ListItem>
                                        <asp:ListItem Text="Mrs." Value="Mrs."></asp:ListItem>
                                        <asp:ListItem Text="Ms." Value="Ms."></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:TextBox ID="txtShippingFirstName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalShippingFirstName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtShippingFirstName" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING LAST NAME--%>
                            <div class="Row">
                                <div class="Label">Last Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtShippingLastName" runat="server" Text="" CssClass="Size Medium" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalShippingLastName" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtShippingLastName" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING ADDRESS LINE 1--%>
                            <div class="Row">
                                <div class="Label">Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtShippingAddressLine1" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalShippingAddressLine1" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtShippingAddressLine1" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING ADDRESS LINE 2--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtShippingAddressLine2" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING ADDRESS LINE 3--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtShippingAddressLine3" runat="server" Text="" CssClass="Size Large" MaxLength="255"></asp:TextBox>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING COUNTRY--%>
                            <div class="Row">
                                <div class="Label">Country<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlShippingCountry" runat="server" AutoPostBack="true" 
                                    AppendDataBoundItems="true" OnSelectedIndexChanged="ddlShippingCountry_SelectedIndexChanged">
                                        <asp:ListItem Text="-Select Country-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalShippingCountry" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlShippingCountry" ValidationGroup="ADDEDITLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING CITY--%>
                            <div class="Row">
                                <div class="Label">City<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtShippingCity" runat="server" Text="" CssClass="Size Medium" MaxLength="100"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalShippingCity" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtShippingCity" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING STATE/POSTAL CODE--%>
                            <div class="Row">
                                <div class="Label">State/Postal<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlShippingState" runat="server" AppendDataBoundItems="true">
                                        <asp:ListItem Text="-Select State-" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="reqfldvalShippingState" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="ddlShippingState" ValidationGroup="ADDEDITLOCATION" InitialValue="0"></asp:RequiredFieldValidator>
                                    <asp:TextBox ID="txtShippingPostalCode" runat="server" Text="" CssClass="Size Small" MaxLength="15"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalShippingPostalCode" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtShippingPostalCode" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING TELEPHONE--%>
                            <div class="Row">
                                <div class="Label">Telephone<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtShippingTelephone" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalShippingTelephone" runat="server" CssClass="InlineError" Display="Dynamic" 
                                    ErrorMessage="Required" ControlToValidate="txtShippingTelephone" ValidationGroup="ADDEDITLOCATION"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="regexpvalShippingTelephone" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtShippingTelephone" ValidationGroup="ADDEDITLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING FAX--%>
                            <div class="Row">
                                <div class="Label">Fax:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtShippingFax" runat="server" Text="" CssClass="Size Medium" MaxLength="24"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="reqfldvalShippingFax" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtShippingFax" ValidationGroup="ADDEDITLOCATION"     
                                    ValidationExpression="^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--SHIPPING E-MAIL ADDRESS--%>
                            <div class="Row">
                                <div class="Label">E-Mail Address:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtShippingEmailAddress" runat="server" Text="" CssClass="Size Medium" MaxLength="60"></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="regexpvalShippingEmailAddress" runat="server" CssClass="InlineError" Display="Dynamic"     
                                    ErrorMessage="Invalid format." ControlToValidate="txtShippingEmailAddress" ValidationGroup="ADDEDITLOCATION"     
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
                <asp:button ID="btnSave_Location" runat="server" Text="Save" OnClick="btnSave_Location_Click" ValidationGroup="ADDEDITLOCATION" style="display: none;" />
				<asp:button ID="btnCancel_Location" runat="server" Text="Cancel" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--ADD/EDIT ORDER INFORMATION FORM MODAL/DIALOG--%>
    <div id="divOrderInformationForm">
        <asp:UpdatePanel ID="updtpnlOrderInformationForm" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldOrderCommandName" runat="server" Value="" />
                <asp:HiddenField ID="hdnfldCRMOrderID" runat="server" Value="0" />
                <div class="OForm">
                    <%--PRODUCT GROUP ID--%>
                    <div class="Row">
                        <div class="Label">Product Group<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlProductGroupID" runat="server" AutoPostBack="true" DataTextField="ProductName" DataValueField="ProductGroupID"
                            AppendDataBoundItems="true" OnSelectedIndexChanged="ddlProductGroupID_SelectedIndexChanged">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalProductGroupID" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="ddlProductGroupID" ValidationGroup="ADDEDITORDER" InitialValue="0"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--PRODUCT ID--%>
                    <div class="Row">
                        <div class="Label">Product<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlProductID" runat="server" AppendDataBoundItems="true" DataTextField="ProductDescription" DataValueField="ProductID">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalProductID" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="ddlProductID" ValidationGroup="ADDEDITORDER" InitialValue="0"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--QUANTITY--%>
                    <div class="Row">
                        <div class="Label">Quantity<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtQuantity" runat="server" Text="" CssClass="Size XSmall"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqfldvalQuantity" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="txtQuantity" ValidationGroup="ADDEDITORDER"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--UNIT PRICE--%>
                    <div class="Row">
                        <div class="Label">Unit Price<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtUnitPrice" runat="server" Text="" CssClass="Size XSmall"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqfldvalUnitPrice" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="txtUnitPrice" ValidationGroup="ADDEDITORDER"></asp:RequiredFieldValidator>
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
                <asp:button ID="btnSave_Order" runat="server" Text="Save" ValidationGroup="ADDEDITORDER" style="display: none;" OnClick="btnSave_Order_Click" />
				<asp:button ID="btnCancel_Order" runat="server" Text="Cancel" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--MAIN FORM--%>
    <div id="accordionMainForm" style="margin-top:10px;">
        <div>
			<h3><a href="#">Account Information</a></h3>
            <div class="OForm AccordionPadding">
                <asp:UpdatePanel ID="upnlAccountInformation" runat="server">
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
                                <asp:RadioButton ID="rdobtnCreditCard" runat="server" Text="Credit Card" AutoPostBack="true" GroupName="rdobtnPaymentMethod" />                 
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
                <asp:UpdatePanel ID="updtpnlBillingAndShippingInformation" runat="server">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lnkbtnAddLocation" EventName="Click" />
                    </Triggers>
                    <ContentTemplate>
                        <div style="margin: 0 auto; width: 800px;">
                            <%--TOOLBAR (ADD LOCATION INFORMATION)--%>
                            <div id="divAddLocationToolbar" class="OToolbar JoinTable">
                                <ul> 
                                    <li>
                                        <asp:LinkButton ID="lnkbtnAddLocation" runat="server" CssClass="Icon Add" OnClick="lnkbtnAddLocation_Click" CommandName="Add" ToolTip="Add Location">Add Location</asp:LinkButton>
                                    </li>
                                </ul>
                            </div>
                            <%--END--%>
                            <div id="divLocationsHTMLTable">
                                <%--LOCATIONS HTML DATATABLE--%>
                                <asp:GridView ID="gvLocations" runat="server" CssClass="OTable" 
                                AlternatingRowStyle-CssClass="Alt" AllowSorting="False" AllowPaging="True"
                                AutoGenerateColumns="false" DataKeyNames="CRMLocationID" 
                                PageSize="10" Width="100%">
                                    <AlternatingRowStyle CssClass="Alt" />
                                    <Columns>
                                        <asp:BoundField DataField="CRMLocationID" Visible="false" />
                                        <asp:BoundField DataField="CRMAccountID" Visible="false" />
                                        <asp:TemplateField HeaderText="Billing Address">
                                            <ItemTemplate>
                                                <asp:Label ID="lblBillingCompanyName" runat="server" Text='<%# Eval("BillingCompanyName") %>'></asp:Label><br />                
                                                <asp:Label ID="lblBillingContactName" runat="server" Text='<%# Eval("BillingContactName") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingAddress" runat="server" Text='<%# Eval("BillingAddress") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingCityStatePostalCode" runat="server" Text='<%# Eval("BillingCityStatePostalCode") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingCountry" runat="server" Text='<%# Eval("BillingCountry") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingEmailAddress" runat="server" Text='<%# Eval("BillingEmailAddress") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingTelephone" runat="server" Text='<%# Eval("BillingTelephone") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingFax" runat="server" Text='<%# Eval("BillingFax") %>'></asp:Label>
                                            </ItemTemplate>
                                            <ItemStyle VerticalAlign="Top" />
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Shipping Address">
                                            <ItemTemplate>
                                                <asp:Label ID="lblShippingCompanyName" runat="server" Text='<%# Eval("ShippingCompanyName") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingContactName" runat="server" Text='<%# Eval("ShippingContactName") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingAddress" runat="server" Text='<%# Eval("ShippingAddress") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingCityStatePostalCode" runat="server" Text='<%# Eval("ShippingCityStatePostalCode") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingCountry" runat="server" Text='<%# Eval("ShippingCountry") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingEmailAddress" runat="server" Text='<%# Eval("ShippingEmailAddress") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingTelephone" runat="server" Text='<%# Eval("ShippingTelephone") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingFax" runat="server" Text='<%# Eval("ShippingFax") %>'></asp:Label>
                                            </ItemTemplate>
                                            <ItemStyle VerticalAlign="Top" />
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="Active" Visible="false" />
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <asp:LinkButton ID="btnDeleteLocation" runat="server" CssClass="Icon Remove" ToolTip="Delete Location" CommandName="DeleteLocation" CommandArgument='<%# Eval("CRMLocationID") %>' OnClick="lnkbtnDeleteLocation_Click" />
                                                <asp:LinkButton ID="btnEditLocation" runat="server" CssClass="Icon Edit" ToolTip="Edit Location" CommandName="EditLocation" CommandArgument='<%# Eval("CRMLocationID") %>' OnClick="lnkbtnEditLocation_Click" /> 
                                            </ItemTemplate>
                                            <ItemStyle HorizontalAlign="Right" CssClass="RightAlign" Width="125px" VerticalAlign="Top" />
                                            <HeaderStyle HorizontalAlign="Right" CssClass="RightAlign" Width="125px" />
                                        </asp:TemplateField>
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div style="text-align: center;">There are no records found!</div>
                                    </EmptyDataTemplate>
                                </asp:GridView>
                                <%--END--%>
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <h3><a href="#">Order Information</a></h3>
            <div class="OForm AccordionPadding">
                <asp:UpdatePanel ID="updtpnlOrderInformation" runat="server">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lnkbtnAddOrder" EventName="Click" />
                    </Triggers>
                    <ContentTemplate>
                        <div style="margin: 0 auto; width: 800px;">
                            <%--TOOLBAR (ADD ORDER INFORMATION)--%>
                            <div id="divAddOrderToolbar" class="OToolbar JoinTable">
                                <ul> 
                                    <li>
                                        <asp:LinkButton ID="lnkbtnAddOrder" runat="server" CssClass="Icon Add" ToolTip="Add Order" OnClick="lnkbtnAddOrder_Click" CommandName="Add">Add Order</asp:LinkButton>
                                    </li>
                                </ul>
                            </div>
                            <%--END--%>
                            <div id="divOrdersHTMLTable">
                                <%--ORDERS HTML DATATABLE--%>
                                <asp:GridView ID="gvOrders" runat="server" CssClass="OTable" 
                                AlternatingRowStyle-CssClass="Alt" AllowSorting="False" AllowPaging="true"
                                AutoGenerateColumns="false" DataKeyNames="CRMOrderID" 
                                PageSize="10" Width="100%">
                                    <AlternatingRowStyle CssClass="Alt" />
                                    <Columns>
                                        <asp:BoundField DataField="CRMOrderID" Visible="false" />
                                        <asp:BoundField DataField="CRMAccountID" Visible="false" />
                                        <asp:BoundField DataField="ProductGroupName" HeaderText="Group" />
                                        <asp:BoundField DataField="ProductName" HeaderText="Name" />
                                        <asp:TemplateField HeaderText="Color & SKU" HeaderStyle-HorizontalAlign="Left">
                                            <ItemTemplate>
                                                <asp:Label ID="lblProductColor" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"ProductSKU","" )%>'
                                                CssClass='<%#string.Format("productColor {0}", DataBinder.Eval(Container.DataItem,"Color","" ))%>' ToolTip='<%# string.Format("{0} {1}", Eval("Color"), Eval("ProductSKU")) %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="Quantity" HeaderText="Qty.">
                                            <ItemStyle HorizontalAlign="Center" CssClass="CenterAlign" />
                                            <HeaderStyle HorizontalAlign="Center" CssClass="CenterAlign" />
                                        </asp:BoundField>
                                        <asp:BoundField DataField="UnitPrice" HeaderText="Unit Price" DataFormatString="{0:c}">
                                            <ItemStyle HorizontalAlign="Right" CssClass="RightAlign" />
                                            <HeaderStyle HorizontalAlign="Right" CssClass="RightAlign" />
                                        </asp:BoundField>
                                        <asp:BoundField DataField="Active" Visible="false" />
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <asp:LinkButton ID="btnDeleteOrder" runat="server" CssClass="Icon Remove" ToolTip="Delete Order" CommandName="DeleteOrder" CommandArgument='<%# Eval("CRMOrderID") %>' OnClick="lnkbtnDeleteOrder_Click" />
                                                <asp:LinkButton ID="btnEditOrder" runat="server" CssClass="Icon Edit" ToolTip="Edit Order" CommandName="EditOrder" CommandArgument='<%# Eval("CRMOrderID") %>' OnClick="lnkbtnEditOrder_Click" /> 
                                            </ItemTemplate>
                                            <ItemStyle HorizontalAlign="Right" CssClass="RightAlign" Width="125px" />
                                            <HeaderStyle HorizontalAlign="Right" CssClass="RightAlign" Width="125px" />
                                        </asp:TemplateField>
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div style="text-align: center;">There are no records found!</div>
                                    </EmptyDataTemplate>
                                    <PagerSettings PageButtonCount="10" />
                                </asp:GridView>
                                <%--END--%>
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
    <%--END--%>

    <div class="Buttons">
        <div class="ButtonHolder">
            <asp:Button ID="btnEditCRMAccount" runat="server" CssClass="OButton" Text="Edit CRM Account" OnClick="btnEditCRMAccount_Click" />
            <asp:Button ID="btnCancelCRMAccount" runat="server" CssClass="OButton" Text="Cancel" OnClick="btnCancelCRMAccount_Click" />
        </div>
    </div>
</asp:Content>

