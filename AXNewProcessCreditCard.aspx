<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="AXNewProcessCreditCard.aspx.cs" Inherits="portal_instadose_com_v3.Finance.AXNewProcessCreditCard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
		newcard.input[type=checkbox] {
			/* Double-sized Checkboxes */
			-ms-transform: scale(2); /* IE */
			-moz-transform: scale(2); /* FF */
			-webkit-transform: scale(2); /* Safari and Chrome */
			-o-transform: scale(2); /* Opera */
			margin-left: 9px;
			margin-top: 5px;
			float: left;
		}

		.holder {
			float: left;
		}

		.gridspacer {
			margin-left: 28px;
		}

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

		.GoButton {
			padding-left: 95px;
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

		.abouttoexpire {
			background-color: #dd0000;
			color: #ffffff;
		}

		/* The is to remove the padding from OButton*/
		.ButtonSlim {
			padding: .4em 0 .4em 0;
		}
	</style>
	<script type="text/javascript">
	    var activeButton;
	    var activeButtonValue;

	    $(document).ready(function () {
	        JQueryControlsLoad();
	        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(BeginRequestHandler);
	        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
	        if (document.location.search != "") {
	            var params = document.location.search.substring(1);
	            var arr = params.split("&");
	            for (var i = 0; i < arr.length; i++) {
	                var arp = arr[i].split("=");
	                if (arp[0] == "Account")
	                    $("input[id*=txtAccountNumber]").val(arp[1]);
	                else if(arp[0] == "System")
	                    $("input[id*=rdobtn"+arp[1]+"]").prop("checked",true);
	            }
	            $("input[id*=btnFin]").click();
	        }
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
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <%--ADD NEW CREDIT CARD MODAL/DIALOG--%>
	<div id="divNewCreditCardInformation">
		<asp:UpdatePanel ID="updtpnlNewCreditCardInformation" runat="server" >
			<ContentTemplate>
				<div class="FormError" id="modalDialogErrors" runat="server" visible="false">
					<p>
                        <span class="MessageIcon"></span>
                        <strong>Messages:</strong>&nbsp;<span id="modalDialogErrorMsg" runat="server">An error was encountered.</span>
					</p>
				</div>
				<div class="OForm">
					<div class="Row">
						<div class="Label">Account #:</div>
						<div class="Control">
							<asp:Label ID="lblAccountNumber" runat="server" CssClass="LabelValue" Value='<%# Eval("AcountID") %>'/>
						</div>
                        <div class="Clear"></div>
					</div>
                    <div class="Row">
						<div class="Label">Company Name:</div>
						<div class="Control">
							<asp:Label ID="lblCompanyName" runat="server" CssClass="LabelValue" Value='<%# Eval("CompanyName") %>'/>
						</div>
						<div class="Clear"></div>
					</div>
					<div class="Row">
						<div class="Label">Name On Card<span class="Required">*</span>:</div>
						<div class="Control">
							<asp:TextBox ID="txtNameOnCard" runat="server" MaxLength="30" CssClass="Size Medium" TabIndex="1" />
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
						<div class="Label">CVV/CVC:</div>
						<div class="Control">
							<asp:TextBox ID="txtSecurityCode" runat="server" CssClass="Size Small" TabIndex="3" />
							<%--<asp:RequiredFieldValidator ID="reqfldvalSecurityCode" runat="server" CssClass="InlineError"
							EnableClientScript="false" Display="Dynamic" ErrorMessage="Please enter CVC."
							ControlToValidate="txtSecurityCode" ValidationGroup="SAVE" />--%>
						</div>
						<div class="Clear"></div>
					</div>
					<div class="Row">
						<div class="Label">Expires On<span class="Required">*</span>:</div>
						<div class="Control">
							<asp:DropDownList runat="server" ID="ddlExpirationMonth" TabIndex="4">
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
                            <asp:DropDownList runat="server" ID="ddlExpirationYear" TabIndex="5"></asp:DropDownList>
						</div>
						<div class="Clear"></div>
					</div>
                    <div class="Row">
						<div class="Label">&nbsp;</div>
						<div class="Control">
							<asp:CheckBox ID="chkbxUseOnlyOnce" runat="server" Text="Use Only Once" />
						</div>
						<div class="Clear"></div>
					</div>
					<asp:Button ID="btnSave" runat="server" Text="Save" CommandName="Save" ValidationGroup="SAVE" OnClick="btnSave_Click" Style="display: none;"></asp:Button>
					<asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" Style="display: none;"></asp:Button>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</div>
    <%--END--%>
    <%--MAIN PAGE CONTENT--%>
    <asp:HiddenField ID="hdnfldAccountID" runat="server" Value="0" />    
    
	<asp:UpdatePanel ID="updtpnlMainPageContent" runat="server">
		<ContentTemplate>
            
            <asp:UpdateProgress id="UpdateProgress_Tab1" runat="server" DynamicLayout="true" DisplayAfter="0" >
                <ProgressTemplate>
                    <div style="width: 850px" align="center">
                        <img src="../images/loading11.gif" alt=""/>
                    </div>
                </ProgressTemplate>
            </asp:UpdateProgress>

            <%--ERROR MESSAGE--%>
	        <div id="errors" class="FormError" runat="server" visible="false">
		        <p>
			        <span class="MessageIcon"></span>
			        <strong>Messages:</strong>
                    <span id="errorMsg" runat="server">An error was encountered.</span>
		        </p>
	        </div>
            <%--END--%>
            <%--SUCCESS MESSAGE--%>
	        <div id="successes" class="FormMessage" runat="server" visible="false">
		        <p>
			        <span class="MessageIcon"></span>
			        <strong>Messages:</strong>
			        <span id="successMsg" runat="server">Submission of information has been successful.</span>
		        </p>
            </div>
            <%--END--%>

            <%--SEARCH TOOLBAR--%>
	        <asp:Panel ID="pnlSearchToolbar" runat="server">
		        <div class="OToolbar">
			        <ul>
				        <li>Account #:</li>
                        <li><asp:TextBox ID="txtAccountNumber" runat="server" Text=""></asp:TextBox></li>
				        <li><asp:RadioButton ID="rdobtnInstadose" runat="server" Text="Instadose" GroupName="AccountDivision"   /></li>
                        <li><asp:RadioButton ID="rdobtnUnix" runat="server" Text="Unix" GroupName="AccountDivision"   /></li>
				        <li><asp:Button ID="btnFind" runat="server" CssClass="OButton" OnClick="btnFind_Click" Text="Find" /></li>
                        <li><asp:Button ID="btnPrintSummaryStatement" runat="server" CssClass="OButton" OnClick="btnPrintSummaryStatement_Click" Text="Export to File" Enabled="false" /></li>
			        </ul>
		        </div>
	        </asp:Panel>

			<asp:Panel ID="pnlActiveInvoicesAndCreditCardInformation" runat="server" Visible="false">
				<div style="display: inline-block;">
                    <%--ACTIVE/UNPAID INVOICES GRIDVIEW--%>
					<span id="spnActiveInvoices" style="display: inline-block; float: left; margin-left: 10px;">
                        <div>
                            <asp:GridView ID="gvActiveInvoices" runat="server" CssClass="OTable"                             
                            AutoGenerateColumns="false"                           
                            DataKeyNames="InvoiceNo"
                            AllowPaging="false" Width="750px"
                            EmptyDataText="There are no balance on this account to process credit card payment." >
							    <AlternatingRowStyle CssClass="Alt" />
							    <Columns>
								    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="CenterAlign">
									    <ItemTemplate>
										    <asp:CheckBox runat="server" ID="chkbxInvoiceSelect" Enabled='<%# Eval("Balance").ToString() == "0.00" ? false : true %>' OnCheckedChanged="chkbxInvoiceSelect_CheckedChanged" AutoPostBack="true" />
                                            <asp:HiddenField runat="server" ID="HidCurrencyCode"  Value='<%# Eval("CurrencyCode") %>' />
                                            <asp:HiddenField runat="server" ID="HidExponentCode"  Value='<%# Eval("ExponentCode") %>' />
                                            <asp:HiddenField runat="server" ID="HidOrderID"  Value='<%# Eval("OrderID") %>' />
                                            <asp:HiddenField runat="server" ID="HidDays" Value='<%# Eval("DaysSinceInvoice") %>' />
                                            <asp:HiddenField runat="server" ID="HidBalanceDue" Value='<%# Eval("Balance") %>' />
									    </ItemTemplate>
									    <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
								    </asp:TemplateField>
                                    <asp:BoundField DataField="AccountID" HeaderText="Account #" ReadOnly="True" HeaderStyle-CssClass="CenterAlign">
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="CompanyName" HeaderText="Company Name" ReadOnly="True" HeaderStyle-CssClass="CenterAlign">
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="InvoiceNo" HeaderText="Invoice #" ReadOnly="True" HeaderStyle-CssClass="CenterAlign">
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
								    <asp:BoundField DataField="InvoiceDate" HeaderText="Invoice Date" ReadOnly="True" HeaderStyle-CssClass="CenterAlign" DataFormatString="{0:d}">
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="InvoiceAmt" HeaderText="Invoice Amount" ReadOnly="True" HeaderStyle-CssClass="CenterAlign" ItemStyle-CssClass="RightAlign" DataFormatString="{0:0.00}" >
                                        <HeaderStyle HorizontalAlign="Right" />
                                        <ItemStyle HorizontalAlign="Right" />
                                    </asp:BoundField>
                                    <asp:TemplateField HeaderText="Balance" HeaderStyle-CssClass="CenterAlign" ItemStyle-CssClass="RightAlign" >
                                        <ItemTemplate>
                                            <asp:Label ID="lblBalance" runat="server" Text='<%# decimal.Parse(Eval("Balance").ToString()).ToString("0.00") %>'></asp:Label>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Right" />
                                        <ItemStyle HorizontalAlign="Right" />
                                    </asp:TemplateField>
								    <asp:TemplateField HeaderText="Payment" HeaderStyle-CssClass="RightAlign" ItemStyle-CssClass="RightAlign">
									    <ItemTemplate>
										    <asp:TextBox ID="txtPayment" runat="server" Text="" Enabled='<%# Eval("Balance").ToString() == "0.00" ? false : true %>' Width="70" style="text-align: right;"
                                                AutoPostBack="true" OnTextChanged ="txtPayment_TextChanged" ></asp:TextBox>
										    <asp:CompareValidator ID="cmpvalPayment" runat="server" ControlToValidate="txtPayment" Type="Double" ErrorMessage="Invalid entry." ForeColor="#FF0000" Operator="DataTypeCheck" Visible="false"></asp:CompareValidator>
									    </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Right" />
                                        <ItemStyle HorizontalAlign="Right" />
								    </asp:TemplateField>                                    
                                    
							    </Columns>
							    <RowStyle CssClass="Row" />
						    </asp:GridView>
                            <%--TOTAL INVOICE AMOUNT--%>
                            <table id="totalBar" runat ="server" class="OTable">
							    <tr>
								    <th style="text-align: right !important; padding-right: 5px; width: 85%;">TOTAL:</th>
								    <th style="text-align: right !important;"><asp:Label ID="lblTotal" runat="server" Text=""></asp:Label></th>
							    </tr>
						    </table>
                            <%--END --%>
                        </div>
					</span>
                    <%--END--%>
                    <%--CREDIT CARD INFORMATION/PROCESS BOXES--%>
					<span id="spnCreditCardInformation" runat ="server" style="display: inline-block; margin-left: 10px;">
						<table class="OTable" style="width: 300px; text-align: center;">
							<tr>
								<th>Credit Card Info</th>
							</tr>
                            <tr>
                                <td style="padding-top: 10px;"><asp:Button ID="btnAddNewCreditCard" runat="server" CssClass="OButton" CausesValidation="false" Text="Add New Credit Card" OnClientClick="openDialog('divNewCreditCardInformation'); return false;"  /></td>
                            </tr>
                            <tr>
                                <td><center>- OR -</center></td>
                            </tr>
							<tr>
								<td style="padding-bottom: 10px;">
                                    <asp:DropDownList ID="ddlAccountCreditCards" runat="server" OnSelectedIndexChanged="ddlAccountCreditCards_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
								</td>
							</tr>
						</table>
                        <hr />
                        <div id="divProcessPayment" runat="server" visible="false">
                            <table class="OTable" style="width: 300px;">
							    <tr>
								    <th>Process Payment</th>
							    </tr>
							    <tr>
								    <td>
									    <table class="OTable">
                                            <tr>
                                                <th style="text-align: left !important; padding-left: 5px;">Total:</th>
                                                <td style="text-align: right;">
                                                    <asp:TextBox ID="txtTotalToBeChargedToCC" runat="server" CssClass="Size Medium"  Text="" ReadOnly="true" style="text-align: right;" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th style="text-align: left !important; padding-left: 5px;">CC #:</th>
                                                <td style="text-align: right;"><asp:Label ID="lblCreditCardBeingUsed" runat="server" Text=""></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <th style="text-align: left !important; padding-left: 5px;">Email:</th>
                                                <td style="text-align: right;"><asp:TextBox ID="txtSuccessEmail" Width="210" runat="server" CssClass="Size Medium " ></asp:TextBox></td>                                                
                                            </tr>
									    </table>
								    </td>
							    </tr>
                                <tr>
                                    <td>
                                        <div class="Buttons">
					                        <div class="ButtonHolder">
						                        <asp:Button ID="btnProcesssInvoicePayment" runat="server" CssClass="OButton" Text="Process" OnClick="btnProcesssInvoicePayment_Click" Visible="True" />
					                        </div>
					                        <div class="Clear"></div>
				                        </div>
                                    </td>
                                </tr>
						    </table>
                        </div>

                        <div id="divPaymentDecline" runat="server" visible="false">
                            <table class="OTable" style="width: 300px;">
							    <tr>
								    <th>Send Decline Payment Email</th>
							    </tr>
                                <tr>
								    <td>
									    <table class="OTable">
                                            <tr>
                                                <th style="text-align: left !important; padding-left: 5px;">Email:</th>
                                                <td style="text-align: right;">
                                                    <asp:TextBox ID="txtUnsuccessEmail" Width="210" runat="server" CssClass="Size Medium " />
                                                </td>
                                            </tr>
                                            
									    </table>
								    </td>
							    </tr>							   
                                <tr>
                                    <td>
                                        <div class="Buttons">
					                        <div class="ButtonHolder">
						                        <asp:Button ID="btnCCDeclineEmail" runat="server" CssClass="OButton" Text="Send" OnClick="btnCCDeclineEmail_Click" Visible="True" />
					                        </div>
					                        <div class="Clear"></div>
				                        </div>
                                    </td>
                                </tr>
						    </table>
                        </div>
					</span>
                    <%--END--%>
				</div>                
			</asp:Panel>
            
            <asp:Panel ID="pnlPaymentHistory" runat="server" Visible="false">
                <hr />
                
                <table class="OTable" >
					<tr>
						<th>Payment History:</th>
					</tr>                    
				</table>

                <%--ACCOUNT PAYMENTS (MADE) GRIDVIEW--%>            
			    <div id="divPaymentHistory" >
				    <asp:GridView ID="gvPaymentHistory" runat="server" CssClass="OTable" Width="100%"
                    AutoGenerateColumns="False" DataKeyNames="PaymentID" EmptyDataText="There are no payment history records." 
                    AllowPaging="false" >
					    <AlternatingRowStyle CssClass="Alt" />
					    <Columns>
						    <asp:TemplateField HeaderText="" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center"  HeaderStyle-Width="30">
							    <ItemTemplate>
								    <a href="/ViewDocuments.aspx?GUID=<%# Eval("DocumentGUID") %>">
									    <img src="/images/icons/printer.png" style="width: 16px; height: 16px; border: 0;" alt="Print Receipt" />
								    </a>
							    </ItemTemplate>							
						    </asp:TemplateField>
                            <asp:BoundField DataField="CompanyName" HeaderText="Company Name" ReadOnly="True" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />                                                  
						    <asp:BoundField DataField="InvoiceNo" HeaderText="Invoice #" ReadOnly="True" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />                            
                            <asp:BoundField DataField="DateOfPayment" HeaderText="Payment Date" ReadOnly="True" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" DataFormatString="{0:MM/dd/yyyy hh:mm:ss tt}" />                            
                            <asp:BoundField DataField="CreditCardNumber" HeaderText="Credit Card" ReadOnly="True" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />                            
						    <asp:BoundField DataField="TypeOfCreditCard" HeaderText="Card Type" ReadOnly="True" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />                            
                            <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />                           
					    </Columns>
					    <RowStyle CssClass="Row" />
				    </asp:GridView>
			    </div>
                <%--END--%>
            </asp:Panel>
            
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
