<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MirionDSD.master" AutoEventWireup="true" Inherits="InformationFinder_Details_AccountInvoicesExport" EnableEventValidation ="false"  Codebehind="AccountInvoicesExport.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">
        function pageLoad(sender, args) {
            // Maintains jQuery datepicker(s) after PostBack.
            if (args.get_isPartialLoad()) {
                loadDatePickers();
            }
            $(document).ready(function () {
                jQueryModal();
                loadDatePickers();
            });
        }

        function jQueryModal() {
            $('#resultsDialog').dialog({
                autoOpen: false,
                width: 700,
                resizable: false,
                title: "Search Results",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });
        }

        function loadDatePickers() {
            $('#<%= txtInvoiceFrom.ClientID %>').datepicker({
                constrainInput: true,
                changeMonth: true,
                changeYear: true,
                dateFormat: 'mm/dd/yy',
                gotoCurrent: true,
                hideIfNoPrevNext: true,
                minDate: '-20y',
                maxDate: '+0y'
            });
            $('#<%= txtInvoiceTo.ClientID %>').datepicker({
                constrainInput: true,
                changeMonth: true,
                changeYear: true,
                dateFormat: 'mm/dd/yy',
                gotoCurrent: true,
                hideIfNoPrevNext: true,
                minDate: '-20y',
                maxDate: '+0y'
            });

            $('#ui-datepicker-div').css("z-index", $(this).parents(".ui-dialog").css("z-index") + 1);
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

<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" runat="Server">
<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
<div id="divFormError" runat="server" class="FormError" visible="false">
	    <p><span class="MessageIcon"></span>
	    <strong>Messages:</strong>&nbsp;<span id="spnFormErrorMessage" runat="server">An error was encountered.</span></p>
    </div>     
    <asp:Panel ID="pnlAccountInvoices" runat="server">
        <div class="OToolbar JoinTable">      
            <ul>
                <li>Invoice Date From:</li>
                <li>
                     <asp:TextBox ID="txtInvoiceFrom" runat="server" CssClass="Size Small" OnTextChanged="txtInvoiceFrom_TextChange" AutoPostBack="true" />
                </li>
                <li>To:</li>
                <li>
                     <asp:TextBox runat="server" ID="txtInvoiceTo" CssClass="Size Small" OnTextChanged="txtInvoiceTo_TextChange" AutoPostBack="true" />
                </li>
                <li class="RightAlign">
                    <asp:LinkButton ID="lnkbtnClearFilters" runat="server" OnClick="lnkbtnClearFilters_Click" 
                    Text="Clear Filters" CssClass="Icon Remove" ToolTip="Clear Filters" CommandName="ClearFilters" />
                </li>
                <li>
                    <asp:LinkButton ID="lnkbtnExportToExcel" runat="server" CssClass="Icon Export" CommandName="ExportToExcel" 
                    OnCLick="lnkbtnExportToExcel_Click" ToolTip="Export to Excel">Export to Excel</asp:LinkButton>
                </li>
                <li class="Clear"></li>
            </ul>
		</div>
        <ec:GridViewEx ID="gvAccountInvoices" runat="server" CssClass="OTable" 
        AlternatingRowStyle-CssClass="Alt" 
        AutoGenerateColumns="false"
        AllowPaging="true" PageSize="20"
        PagerStyle-CssClass="mt-hd" 
        PagerSettings-Mode="Numeric"
        OnPageIndexChanging="gvAccountInvoices_PageIndexChanging" 
        meta:resourcekey="GridView" 
        AllowSorting="true" 
        CurrentSortedColumn="AccountID" 
        CurrentSortDirection="Ascending" 
        OnSorting="gvAccountInvoices_Sorting" 
        SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
        SortOrderDescendingImagePath="~/images/icon_sort_descending.gif" 
        EmptyDataText="There are no invoices that meet the criteria you specified." 
        Width="100%" DataKeyNames="AccountID" >
            <AlternatingRowStyle CssClass="Alt" />
            <Columns>
                <asp:BoundField DataField="AccountID" HeaderStyle-CssClass="mt-hd" 
                HeaderText="Account#" meta:resourcekey="AccountID" 
                ItemStyle-CssClass="mt-itm" ReadOnly="True" >
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="CompanyName" HeaderStyle-CssClass="mt-hd" 
                HeaderText="Company Name" ItemStyle-CssClass="mt-itm" >
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="InvoiceNo" HeaderStyle-CssClass="mt-hd" 
                HeaderText="Invoice#" ItemStyle-CssClass="mt-itm" 
                SortExpression="InvoiceNo">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="OrderID" DataFormatString="{0:d}" 
                HeaderStyle-CssClass="mt-hd" HeaderText="Order#" ItemStyle-CssClass="mt-itm" 
                SortExpression="OrderID">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="InvoiceDate" DataFormatString="{0:d}" HeaderStyle-CssClass="mt-hd" 
                HeaderText="Invoice Date" ItemStyle-CssClass="mt-itm"
                SortExpression="InvoiceDate">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="Amount" HeaderStyle-CssClass="mt-hd" 
                HeaderText="Amount" ItemStyle-CssClass="mt-itm" SortExpression="Amount" 
                DataFormatString="{0:C}">
                    <HeaderStyle CssClass="mt-hd" HorizontalAlign="Right" />
                    <ItemStyle CssClass="mt-itm" HorizontalAlign="Right" />
                </asp:BoundField>
                <asp:BoundField DataField="Payments" HeaderStyle-CssClass="mt-hd" 
                HeaderText="Payments" ItemStyle-CssClass="mt-itm" SortExpression="Payments" 
                DataFormatString="{0:C}">
                    <HeaderStyle CssClass="mt-hd" HorizontalAlign="Right" />
                    <ItemStyle CssClass="mt-itm" HorizontalAlign="Right" />
                </asp:BoundField>
                <asp:BoundField DataField="PayDate" DataFormatString="{0:d}" 
                HeaderStyle-CssClass="mt-hd" HeaderText="Last Transaction" ItemStyle-CssClass="mt-itm" 
                SortExpression="PayDate">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="Credits" HeaderStyle-CssClass="mt-hd" 
                HeaderText="Credits" ItemStyle-CssClass="mt-itm" SortExpression="Credits" 
                DataFormatString="{0:C}">
                    <HeaderStyle CssClass="mt-hd" HorizontalAlign="Right" />
                    <ItemStyle CssClass="mt-itm" HorizontalAlign="Right" />
                </asp:BoundField>
                <asp:BoundField DataField="Balance" HeaderStyle-CssClass="mt-hd" 
                HeaderText="Balance" ItemStyle-CssClass="mt-itm" SortExpression="Balance" 
                DataFormatString="{0:C}">
                    <HeaderStyle CssClass="mt-hd" HorizontalAlign="Right" />
                    <ItemStyle CssClass="mt-itm" HorizontalAlign="Right" />
                </asp:BoundField>
            </Columns>
            <PagerSettings Mode="Numeric" />
            <PagerStyle CssClass="mt-hd" />
            <RowStyle CssClass="Row" />
            <SelectedRowStyle CssClass="Row Selected" />
        </ec:GridViewEx>
    </asp:Panel>
    <div style="text-align: right;">
        <asp:Button ID="btnReturn" runat="server" CssClass="OButton" Text="Return to Account" OnClick="btnReturn_Click" />
    </div>     
</asp:Content>