<%@ Page Title="Chase Process Credit Card" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master"
    AutoEventWireup="true" EnableEventValidation="true"
    Inherits="Finance_ChaseProcessCreditCard" Codebehind="ChaseProcessCreditCard.aspx.cs" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style type="text/css">
        /* CSS Definition for Label Values. */
        .StaticLabel {
            color: #0C5A6B;
            text-align: left;
            font-weight: normal;
            display: block;
            padding: 3px;
            margin: 2px 0;
        }
        /* CSS Definition/Override for Buttons Class. */
        .Buttons {
            width: 100%;
            padding: 0px 0px 0px 0px;
            margin: 10px 0px 0px 0px;
        }

        .InitialMessage {
            padding: 0px 5px 0px 0px;
            margin: 0px 0px 0px 0px;
        }
        /* CSS Definition/Override for HTML-rendered <TH> in GridView. */
        th.CenterHeaderText {
            text-align: center;
            vertical-align: middle;
        }

        th.RightJustifyHeaderText {
            text-align: right;
            vertical-align: middle;
        }
    </style>
    <script type="text/javascript">
    	var activeButton;
    	var activeButtonValue;

    	$(document).ready(function () {
    		JQueryControlsLoad();
    		Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(BeginRequestHandler);
    		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
    	});

    	function BeginRequestHandler(sender, args) {
    		activeButton = args.get_postBackElement();
    		activeButtonValue = $(activeButton).val();
    		$(activeButton).val('Please Wait...');
    		activeButton.disabled = true;

    	}
    	function EndRequestHandler(sender, args) {
    		$(activeButton).val(activeButtonValue);
    		activeButton.disabled = false;
    	}
    	function clientSideClick(btn) {
    		if (typeof (Page_ClientValidate) == 'function') {
    			if (Page_ClientValidate() == false) { return false; }
    		}

    		$(btn).attr('disabled', 'disabled');
    		$(btn).val('Please wait...');

    		return true;
    	}
    	function JQueryControlsLoad() {
    		// AJAX Modal/Dialog.
    		$('#divNewCreditCardInformation').dialog({
    			autoOpen: false,
    			width: 700,
    			resizable: false,
    			title: "New Credit Card Information",
    			open: function (type, data) {
    				$(this).parent().appendTo("form");
    				$('.ui-dialog :input').focus();
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
    	// Alternate function do that OnClick Event is triggered by Enter key.
    	function doClick(buttonName, e) {
    		var key;

    		if (window.event)
    			key = window.event.keyCode;
    		else
    			key = e.which;

    		if (key == 13) {
    			var btn = document.getElementById(buttonName);
    			if (btn != null) {
    				btn.click();
    				event.keyCode = 0;
    			}
    		}
    	}
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>

    <%--NEW CREDIT CARD INFORMATION MODAL/DIALOG--%>
    <div id="divNewCreditCardInformation">
        <asp:UpdatePanel ID="udpnlNewCreditCardInformation" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hfAccountID" runat="server" Value='<%# Eval("AccountID") %>' />
                <div class="FormError" id="modalDialogErrors" runat="server" visible="false">
                    <p>
                        <span class="MessageIcon"></span>
                        <strong>Messages:</strong>&nbsp;<span id="modalDialogErrorMsg" runat="server">An error
                            was encountered.</span>
                    </p>
                </div>
                <div class="OForm">
                    <div class="Row">
                        <div class="Label">Account Name:</div>
                        <div class="Control">
                            <div class="Control">
                                <asp:Label ID="lblAccountNameAndNumber" runat="server" CssClass="StaticLabel" />
                            </div>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Name On Card<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtNameOnCard" runat="server" CssClass="Size Medium" TabIndex="1" />
                            <asp:RequiredFieldValidator ID="reqfldvalNameOnCard" runat="server" CssClass="InlineError"
                                EnableClientScript="false" Display="Dynamic" ErrorMessage="Please enter Name on Card."
                                ControlToValidate="txtNameOnCard" ValidationGroup="SAVE" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Credit Card Number<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtCreditCardNumber" runat="server" CssClass="Size Medium" TabIndex="2" />
                            <asp:RequiredFieldValidator ID="reqfldvalCardNumber" runat="server" CssClass="InlineError"
                                EnableClientScript="false" Display="Dynamic" ErrorMessage="Please enter Card Number."
                                ControlToValidate="txtCreditCardNumber" ValidationGroup="SAVE" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Expires On:</div>
                        <div class="Control">
                            <asp:DropDownList runat="server" ID="ddlExpirationMonth" TabIndex="3">
                                <asp:ListItem Text="01 January" Value="1" />
                                <asp:ListItem Text="02 February" Value="2" />
                                <asp:ListItem Text="03 March" Value="3" />
                                <asp:ListItem Text="04 April" Value="4" />
                                <asp:ListItem Text="05 May" Value="5" />
                                <asp:ListItem Text="06 June" Value="6" />
                                <asp:ListItem Text="07 July" Value="7" />
                                <asp:ListItem Text="08 August" Value="8" />
                                <asp:ListItem Text="09 September" Value="9" />
                                <asp:ListItem Text="10 October" Value="10" />
                                <asp:ListItem Text="11 November" Value="11" />
                                <asp:ListItem Text="12 December" Value="12" />
                            </asp:DropDownList>
                            &nbsp;
                            <asp:DropDownList runat="server" ID="ddlExpirationYear" TabIndex="4"></asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">CVV<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtSecurityCode" runat="server" CssClass="Size Small" TabIndex="5" />
                            <asp:RequiredFieldValidator ID="reqfldvalSecurityCode" runat="server" CssClass="InlineError"
                                EnableClientScript="false" Display="Dynamic" ErrorMessage="Please enter CVV."
                                ControlToValidate="txtSecurityCode" ValidationGroup="SAVE" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <asp:Button ID="btnSave" runat="server" Text="Save" CommandName="Update" ValidationGroup="SAVE"
                        OnClick="btnSave_Click" Style="display: none;"></asp:Button>
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click"
                        Style="display: none;"></asp:Button>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <asp:UpdatePanel ID="updtpnlCreditCardDDL" runat="server">
        <ContentTemplate>

            <asp:HiddenField ID="hdnfldAccordionIndex" runat="server" Value="0" />
            <div id="errors" class="FormError" runat="server" visible="false">
                <p>
                    <span class="MessageIcon"></span>
                    <strong>Messages:</strong>&nbsp;<span id="errorMsg" runat="server">An error was encountered.</span>
                </p>
            </div>
            <div id="successes" class="FormMessage" runat="server" visible="false">
                <p>
                    <span class="MessageIcon"></span>
                    <strong>Messages:</strong>&nbsp;<span id="successMsg" runat="server">Submission of information
                        has been successful.</span>
                </p>
                <div class="OForm">
                    <div class="Row">
                        <div class="Label">Account #:</div>
                        <div class="Control">
                            <asp:Label Text="" ID="lblReviewAccountNo" runat="server" class="StaticLabel" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Order #:</div>
                        <div class="Control">
                            <asp:Label Text="" ID="lblReviewOrderNo" runat="server" class="StaticLabel" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Invoice #:</div>
                        <div class="Control">
                            <asp:Label Text="" ID="lblReviewInvoiceNo" runat="server" class="StaticLabel" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Order Total:</div>
                        <div class="Control">
                            <asp:Label Text="" ID="lblReviewTotal" runat="server" class="StaticLabel" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <hr />
                    <div class="Buttons">
                        <div class="ButtonHolder">
                            <asp:Button runat="server" ID="btnProcessAnotherOrder" CssClass="OButton" Text="Process Another Order"
                                OnClick="btnProcessAnotherOrder_Click" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                </div>
            </div>

            <%--SEARCH TOOLBAR--%>
            <asp:Panel ID="pnlSearch" runat="server" DefaultButton="btnGo">
                <div id="divSearchToolbar" runat="server" class="OToolbar">
                    <ul>
                        <li>Order #:
                    <asp:TextBox ID="txtOrderID" runat="server"></asp:TextBox>
                        </li>
                        <li>
                            <asp:Button ID="btnGo" runat="server" OnClick="btnGo_Click" Text="Go" CssClass="OButton" />
                        </li>
                    </ul>
                </div>
            </asp:Panel>
            <%--END--%>

            <%--PROCESS CREDIT CARD & TRANSACTION HISTORY--%>

            <asp:Panel ID="pnlPaymentProcessing" runat="server">
                <div id="divPaymentProcessing" style="margin-top: 10px;">
                    <%--BEGIN :: ORDER INFORMATION & PAYMENT OPTIONS--%>
                    <div id="divOrderInformation">
                        <div class="OForm">
                            <div class="Row">
                                <div class="Label">Account Name:</div>
                                <div class="Control">
                                    <asp:Label ID="lblAccountName" runat="server" CssClass="StaticLabel" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">Order #:</div>
                                <div class="Control">
                                    <asp:Label ID="lblOrderID" runat="server" CssClass="StaticLabel" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">Ordered On:</div>
                                <div class="Control">
                                    <asp:Label ID="lblOrderDate" runat="server" CssClass="StaticLabel" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">Credit Card<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlCreditCardNumbers" runat="server" AppendDataBoundItems="true">
                                    </asp:DropDownList>
                                </div>
                                <div class="Control">
                                    &nbsp;
                                    <asp:Button ID="btnNewCreditCard" runat="server" CssClass="OButton"
                                        Text="New Credit Card" OnClientClick="openDialog('divNewCreditCardInformation'); return false;" />
                                    <asp:RequiredFieldValidator ID="reqfldvalDdlCreditCard" runat="server"
                                        ErrorMessage="Please select a credit card." Display="Dynamic"
                                        ControlToValidate="ddlCreditCardNumbers" CssClass="InlineError"
                                        InitialValue="0" ValidationGroup="PROCESS"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">Order Total<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtOrderTotal" runat="server" CssClass="Size Small"></asp:TextBox>
                                    <asp:Label ID="lblCurrencyCode" Width="235px" runat="server"></asp:Label>
                                    <asp:RequiredFieldValidator ID="reqfldvalTxtOrderTotal" runat="server"
                                        ErrorMessage="Please enter a valid amount." Display="Dynamic" CssClass="InlineError"
                                        InitialValue=".00" SetFocusOnError="True" ValidationGroup="PROCESS"
                                        ControlToValidate="txtOrderTotal"></asp:RequiredFieldValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <hr />
                            <div class="Buttons">
                                <div class="RequiredIndicator" style="padding-left: 10px;">
                                    <span class="Required">*</span>&nbsp;Indicates a required field.                        
                                </div>
                                <div class="ButtonHolder" style="padding-right: 10px;">
                                    <asp:Button runat="server" ID="btnProcess" CssClass="OButton" Text="Process" OnClick="btnProcess_Click"
                                        ValidationGroup="PROCESS" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                        </div>
                    </div>
                    <%--END--%>
                    <%--BEGIN :: PAYMENT HISTORY GRIDVIEW--%>
                    <div id="divPaymentHistory">
                        <asp:GridView ID="gvPaymentHistory" runat="server" AutoGenerateColumns="False"
                            DataKeyNames="PaymentID" CssClass="OTable" EmptyDataText="There are no payment records." Width="80%">
                            <AlternatingRowStyle CssClass="Alt" />
                            <Columns>
                                <asp:TemplateField HeaderText="" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="30">
                                    <ItemTemplate>
                                        <a href="/ViewDocuments.aspx?GUID=<%# Eval("DocumentGUID") %>"><img src="/images/icons/printer.png" style="width:16;height:16px;border:0;"alt="Print Receipt" /></a>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="CompanyName" HeaderText="Company Name"></asp:BoundField>
                                <asp:BoundField DataField="DateOfPayment" HeaderText="Payment Date" DataFormatString="{0:M/d/yyyy h:mm:ss tt}" />
                                <asp:BoundField DataField="CreditCardNumber" HeaderText="Credit Card" />
                                <asp:BoundField DataField="TypeOfCreditCard" HeaderText="Card Type" />
                                <asp:BoundField DataField="TotalAmount" HeaderText="Amount" DataFormatString="{0:$#,##0.00}" ItemStyle-HorizontalAlign="Right">
                                    <HeaderStyle CssClass="RightJustifyHeaderText" />
                                    <ItemStyle HorizontalAlign="Right" />
                                </asp:BoundField>
                            </Columns>
                            <RowStyle CssClass="Row" />
                        </asp:GridView>
                    </div>
                    <%--END :: PAYMENT HISTORY GRIDVIEW--%>
                </div>
                <%--END--%>
            </asp:Panel>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>



