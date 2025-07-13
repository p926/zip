<%@ Page Title="Reports" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" EnableEventValidation="false" Inherits="Reports" CodeBehind="Reports.aspx.cs" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>

<asp:Content runat="server" ID="Content1" ContentPlaceHolderID="head">
    <script type="text/javascript" lang="javascript">
        function pageLoad(sender, args) {
            // Maintains jQuery datepicker(s) after PostBack.
            if (args.get_isPartialLoad()) {
                loadCCEDatePickers();
                loadDBDatePickers();
            }
            $(document).ready(function () {
                loadCCEDatePickers();
                loadDBDatePickers();
            });
        }

        function loadCCEDatePickers() {
            $('#<%= txtFromExpirationDate.ClientID %>').datepicker({
                constrainInput: true,
                changeMonth: true,
                changeYear: true,
                dateFormat: 'mm/dd/yy',
                gotoCurrent: true,
                hideIfNoPrevNext: true,
                minDate: '-5y',
                maxDate: '+5y'
            });
            $('#<%= txtToExpirationDate.ClientID %>').datepicker({
                constrainInput: true,
                changeMonth: true,
                changeYear: true,
                dateFormat: 'mm/dd/yy',
                gotoCurrent: true,
                hideIfNoPrevNext: true,
                minDate: '-5y',
                maxDate: '+5y'
            });

            $('#ui-datepicker-div').css("z-index",
                        $(this).parents(".ui-dialog").css("z-index") + 1);
        }

        function loadDBDatePickers() {
            $('#<%= txtFromPaymentDate.ClientID %>').datepicker({
                constrainInput: true,
                changeMonth: true,
                changeYear: true,
                dateFormat: 'mm/dd/yy',
                gotoCurrent: true,
                hideIfNoPrevNext: true,
                minDate: '-5y',
                maxDate: '+5y'
            });
            $('#<%= txtToPaymentDate.ClientID %>').datepicker({
                constrainInput: true,
                changeMonth: true,
                changeYear: true,
                dateFormat: 'mm/dd/yy',
                gotoCurrent: true,
                hideIfNoPrevNext: true,
                minDate: '-5y',
                maxDate: '+5y'
            });

            $('#ui-datepicker-div').css("z-index",
                        $(this).parents(".ui-dialog").css("z-index") + 1);
        }
    </script>
</asp:Content>

<asp:Content runat="server" ID="Content2" ContentPlaceHolderID="primaryHolder">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div class="FormError" id="divError" runat="server" visible="false">
		<p><span class="MessageIcon"></span>
		<strong>Messages:</strong><span id="spnError" runat="server">An error was encountered.</span></p>
	</div>
    <div class="OForm">
        <div class="Row">                
			<div class="Label Medium">Type of Report:</div>
			<div class="Control">
                <asp:DropDownList ID="ddlTypeOfReport" runat="server" OnSelectedIndexChanged="ddlTypeOfReport_SelectedIndexChanged" AutoPostBack="true">
                    <asp:ListItem Text="-Select-" Value="NONE"></asp:ListItem>
                    <asp:ListItem Text="Credit Card Expirations" Value="CCE"></asp:ListItem>
                    <asp:ListItem Text="Daily Batches" Value="DB"></asp:ListItem>
                </asp:DropDownList>
			</div>
			<div class="Clear"></div>
		</div>
    </div>
    <asp:Panel ID="pnlReportCriteria" runat="server">
        <div id="divCreditCardExpirations" runat="server" visible="false">
		    <div class="OForm">
                <div class="Row">                
				    <div class="Label Medium">Account Type:</div>
				    <div class="Control">
                        <asp:DropDownList ID="ddlTypeOfAccount_CCE" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlTypeOfAccount_CCE_SelectedIndexChanged">
                            <asp:ListItem Text="Instadose" Value="Instadose" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Unix" Value="Unix"></asp:ListItem>
                        </asp:DropDownList>
				    </div>
				    <div class="Clear"></div>
			    </div>
                <div class="Row">                
				    <div class="Label Medium">Account #:</div>
				    <div class="Control"><asp:TextBox ID="txtAccountID_CCE" runat="server" CssClass="Size Small" Text=""></asp:TextBox></div>
				    <div class="Clear"></div>
			    </div>
                <div id="divCompanyName" runat="server" class="Row">                
				    <div class="Label Medium">Company Name:</div>
				    <div class="Control"><asp:TextBox ID="txtCompanyName_CCE" runat="server" CssClass="Size Medium" Text=""></asp:TextBox></div>
				    <div class="Clear"></div>
			    </div>
                <div class="Row">                
				    <div class="Label Medium">Type of CC:</div>
				    <div class="Control">
                        <asp:DropDownList ID="ddlTypeOfCreditCard" runat="server" DataSourceID="SQL_CreditCardTypes" 
                        DataTextField="CreditCardName" DataValueField="CreditCardName" AppendDataBoundItems="true">
                            <asp:ListItem Text="-Select-" Value=""></asp:ListItem>
                        </asp:DropDownList>
				    </div>
				    <div class="Clear"></div>
			    </div>
                <div class="Row">                
				    <div class="Label Medium">Exp. Date From:</div>
				    <div class="Control">
                        <asp:TextBox ID="txtFromExpirationDate" runat="server" CssClass="Size Small" OnTextChanged="txtFromExpirationDate_TextChanged" AutoPostBack="true"></asp:TextBox>   
				    </div>
                    <div class="Label Small">To:</div>
				    <div class="Control">
                        <asp:TextBox ID="txtToExpirationDate" runat="server" CssClass="Size Small"></asp:TextBox>
				    </div>
				    <div class="Clear"></div>
			    </div>
                <div class="Row">                
				    <div class="Label Medium">&nbsp;</div>
				    <div class="Control">
                        <asp:Button ID="btnViewCCEGridView" runat="server" CssClass="OButton" Text="View Results" OnClick="btnViewCCEGridView_Click" />
                        <asp:Button ID="btnClearFilters_CCE" runat="server" CssClass="OButton" Text="Clear Filters" OnClick="btnClearFilters_CCE_Click" />
				    </div>
				    <div class="Clear"></div>
			    </div>
		    </div>           
        </div>
        <div id="divDailyBatches" runat="server" visible="false">
            <div class="OForm">
                <div class="Row">                
				    <div class="Label Medium">Account Type:</div>
				    <div class="Control">
                        <asp:DropDownList ID="ddlTypeOfAccount_DB" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlTypeOfAccount_DB_SelectedIndexChanged">
                            <asp:ListItem Text="Instadose" Value="Instadose" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Unix" Value="Unix"></asp:ListItem>
                        </asp:DropDownList>
				    </div>
				    <div class="Clear"></div>
			    </div>
                <div class="Row">                
				    <div class="Label Medium">Account #:</div>
				    <div class="Control"><asp:TextBox ID="txtAccountID_DB" runat="server" CssClass="Size Small" Text=""></asp:TextBox></div>
				    <div class="Clear"></div>
			    </div>
                <div class="Row">                
				    <div class="Label Medium">Company Name:</div>
				    <div class="Control"><asp:TextBox ID="txtCompanyName_DB" runat="server" CssClass="Size Medium" Text=""></asp:TextBox></div>
				    <div class="Clear"></div>
			    </div>
                <div id="divOrderID" runat="server" class="Row">                
				    <div class="Label Medium">Order #:</div>
				    <div class="Control"><asp:TextBox ID="txtOrderID" runat="server" CssClass="Size Small" Text=""></asp:TextBox></div>
				    <div class="Clear"></div>
			    </div>
                <div class="Row">                
				    <div class="Label Medium">Invoice #:</div>
				    <div class="Control"><asp:TextBox ID="txtInvoiceNumber" runat="server" CssClass="Size Small" Text=""></asp:TextBox></div>
				    <div class="Clear"></div>
			    </div>
                <div class="Row">                
				    <div class="Label Medium">Payment Date From:</div>
				    <div class="Control">
                        <asp:TextBox ID="txtFromPaymentDate" runat="server" CssClass="Size Small" OnTextChanged="txtFromPaymentDate_TextChanged" AutoPostBack="true"></asp:TextBox>   
				    </div>
                    <div class="Label Small">To:</div>
				    <div class="Control">
                        <asp:TextBox ID="txtToPaymentDate" runat="server" CssClass="Size Small"></asp:TextBox>
				    </div>
				    <div class="Clear"></div>
			    </div>
                <div class="Row">                
				    <div class="Label Medium">&nbsp;</div>
				    <div class="Control">
                        <asp:Button ID="btnViewDBGridView" runat="server" CssClass="OButton" Text="View Results" OnClick="btnViewDBGridView_Click" />
                        <asp:Button ID="btnClearFilters_DB" runat="server" CssClass="OButton" Text="Clear Filters" OnClick="btnClearFilters_DB_Click" />
				    </div>
				    <div class="Clear"></div>
			    </div>
		    </div>
        </div>
    </asp:Panel>
    <hr />
    <%------------------------------- BEGIN :: CREDIT CARD EXPIRATIONS LISTING GRIDVIEW -------------------------------%>
    <asp:UpdatePanel ID="updtpnlCreditCardExpirations" runat="server" Visible="false">
        <ContentTemplate>
            <asp:UpdateProgress id="UpdateProgress_CCE" runat="server" DynamicLayout="true" DisplayAfter="0" >
                <ProgressTemplate>
                    <div style="width: 100%;">
                        <center><img src="../images/orangebarloader.gif" width="128" height="15" alt="Processing, please wait..." /></center>
                    </div>
                </ProgressTemplate>
            </asp:UpdateProgress>
            <ec:GridViewEx ID="gvCreditCardExpirationsListing" runat="server" 
            AutoGenerateColumns="False" 
            AllowPaging="True" 
            AllowSorting="True" 
            PageSize="20" 
            DataKeyNames="AccountID" 
            CurrentSortedColumn="DT_ExpirationDate" 
            CurrentSortDirection="Ascending"
            CssClass="OTable" 
            AlternatingRowStyle-CssClass="Alt"
            PagerSettings-Mode="Numeric"
            OnSorting="gvCreditCardExpirationsListing_Sorting" 
            OnRowDataBound="gvCreditCardExpirationsListing_RowDataBound"
            OnPageIndexChanging="gvCreditCardExpirationsListing_PageIndexChanging"
            EmptyDataText="There are no records available." Width="100%" 
            SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
            SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">
            <AlternatingRowStyle CssClass="Alt" />
                <Columns>            
                    <asp:TemplateField HeaderText="Acct. #" ItemStyle-HorizontalAlign="Left" SortExpression="AccountID">
                    <ItemTemplate>
                        <asp:HyperLink ID="hyprlnkAccountIDLink_CCE" runat="server" NavigateUrl='<%# GetNavigationURL(Eval("AccountID").ToString()) %>' Target="_blank" Text='<%# Bind("AccountID") %>' />  
                    </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="CompanyName" HeaderText="Company Name" ItemStyle-Wrap="false" SortExpression="CompanyName" />
                    <asp:BoundField DataField="CreditCardName" HeaderText="CC Type" ItemStyle-Wrap="false" ReadOnly="true" />
                    <asp:BoundField DataField="NumberEncrypted" HeaderText="CC #" ItemStyle-Wrap="false" ReadOnly="true" />
                    <asp:BoundField DataField="DT_ExpirationDate" HeaderText="Expriation Date" SortExpression="DT_ExpirationDate" DataFormatString="{0:MM/yyyy}" />
                    <asp:BoundField DataField="TypeOfAccount" HeaderText="Type" ItemStyle-Wrap="false" ReadOnly="true" />
                </Columns>	
                <PagerSettings Mode="Numeric" />
                <RowStyle CssClass="Row" />
                <SelectedRowStyle CssClass="Row Selected" />
            </ec:GridViewEx>
        </ContentTemplate>
    </asp:UpdatePanel>
    <%------------------------------- END :: CREDIT CARD EXPIRATIONS LISTING GRIDVIEW -------------------------------%>

    <%------------------------------- BEGIN :: DAILY BATCHES LISTING GRIDVIEW -------------------------------%>
    <asp:UpdatePanel ID="updtpnlDailyBatches" runat="server" Visible="false">
        <ContentTemplate>
            <asp:UpdateProgress id="UpdateProgress_DB" runat="server" DynamicLayout="true" DisplayAfter="0" >
                <ProgressTemplate>
                    <div style="width: 100%;">
                        <center><img src="../images/orangebarloader.gif" width="128" height="15" alt="Processing, please wait..." /></center>
                    </div>
                </ProgressTemplate>
            </asp:UpdateProgress>
            <ec:GridViewEx ID="gvDailyBatchesListing" runat="server" 
            AutoGenerateColumns="False" 
            AllowPaging="True" 
            AllowSorting="True" 
            PageSize="20" 
            DataKeyNames="PaymentID" 
            CurrentSortedColumn="DateOfPayment" 
            CurrentSortDirection="Ascending"
            CssClass="OTable" 
            AlternatingRowStyle-CssClass="Alt"
            PagerSettings-Mode="Numeric" 
            OnRowDataBound="gvDailyBatchesListing_RowDataBound"
            OnSorting="gvDailyBatchesListing_Sorting" 
            OnPageIndexChanging="gvDailyBatchesListing_PageIndexChanging"
            EmptyDataText="There are no records available." Width="100%" 
            SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
            SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">
            <AlternatingRowStyle CssClass="Alt" />
                <Columns>
                    <asp:BoundField DataField="PaymentID" HeaderText="Payment #" ReadOnly="true" Visible="false" />            
                    <asp:TemplateField HeaderText="Acct. #" SortExpression="AccountID">
                        <ItemTemplate>
                            <asp:HyperLink ID="hyprlnkAccountIDLink_DB" runat="server" NavigateUrl='<%# GetNavigationURL(Eval("AccountID").ToString()) %>' Target="_blank" Text='<%# Bind("AccountID") %>' />  
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="CompanyName" HeaderText="Company Name" SortExpression="CompanyName" ItemStyle-Wrap="true" ReadOnly="true" />
                    <asp:BoundField DataField="DateOfPayment" HeaderText="Date" SortExpression="DateOfPayment" ItemStyle-Wrap="false" DataFormatString="{0:MM/dd/yyyy}" />
                    <asp:BoundField DataField="Amount" HeaderText="Amount" ItemStyle-Wrap="false" ReadOnly="true" />
                    <asp:BoundField DataField="CurrencyCode" HeaderText="Currency Code" ItemStyle-Wrap="false" ReadOnly="true" />
                    <asp:TemplateField HeaderText="Order #" SortExpression="OrderID" ConvertEmptyStringToNull="true">
                        <ItemTemplate>
                            <asp:HyperLink ID="hyprlnkOrderIDLink" runat="server" 
                            NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"OrderID","~/CustomerService/ReviewOrder.aspx?ID={0}") %>' 
                            Target="_blank" Text='<%# DisplayBlankOrderID(Eval("OrderID").ToString()) %>'>
                            </asp:HyperLink>  
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Invoice #" SortExpression="InvoiceNumber" ConvertEmptyStringToNull="true">
                        <ItemTemplate>
                            <asp:Label ID="lblInvoiceNumber" runat="server" Text='<%# Eval("InvoiceNumber") %>' Visible="false"></asp:Label>
                            <asp:LinkButton ID="lnkbtnInvoiceNumber" runat="server" Text='<%# Eval("InvoiceNumber")%>' OnClick="lnkbtnInvoiceNumber_Click" ToolTip="Generate Invoice" Visible="false" /> 
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="CreditCardName" HeaderText="CC Type" ItemStyle-Wrap="false" ReadOnly="true" />
                    <asp:BoundField DataField="NumberEncrypted" HeaderText="CC Number" ItemStyle-Wrap="false" ReadOnly="true" />
                    <asp:BoundField DataField="ExpirationDate" HeaderText="Exp. Date" ItemStyle-Wrap="false" ReadOnly="true" />
                    <%--<asp:BoundField DataField="CreditCardInformation" HeaderText="CC Info." ItemStyle-Wrap="false" ReadOnly="true" />--%>
                </Columns>	
                <PagerSettings Mode="Numeric" />
                <RowStyle CssClass="Row" />
                <SelectedRowStyle CssClass="Row Selected" />
            </ec:GridViewEx>
        </ContentTemplate>
    </asp:UpdatePanel>
    <%------------------------------- END :: DAILY BATCHES LISTING GRIDVIEW -------------------------------%>

    <div class="Buttons">
        <div class="ButtonHolder">
            <asp:Button ID="btnExportToExcel" runat="server" CssClass="OButton" Text="Export to Excel" OnClick="btnExportToExcel_Click" />
            <asp:Button ID="btnReturnToFinanceDashboard" runat="server" CssClass="OButton" Text="Return to Finance Dashboard" OnClick="btnReturnToFinanceDashboard_Click" />
        </div>
    </div>   

    <%--CREDIT CARD TYPES FILTERS--%>
    <asp:SqlDataSource ID="SQL_CreditCardTypes" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
    SelectCommand="SELECT DISTINCT CreditCardName FROM CreditCardTypes ORDER BY CreditCardName ASC">
    </asp:SqlDataSource>
    <%--END--%>
</asp:Content>


