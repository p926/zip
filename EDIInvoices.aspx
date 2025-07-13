<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Finance_EDIInvoices" Codebehind="EDIInvoices.aspx.cs" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript" src="/scripts/edi.common.js"></script>
    <script type="text/javascript">
        $(function () {
            edi.invoices.init();
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <telerik:RadScriptManager runat="server"></telerik:RadScriptManager>

    <asp:Panel runat="server" ID="pnlInvoices">
        <!-- Invoices -->
        <asp:HiddenField runat="server" ID="hfInvoiceType" Value="GDS" />
        <asp:HiddenField runat="server" ID="hfInvoiceFilterStatus" Value="1" />
        <ul class="OToolbar JoinTable">
            <li>

                <div class="btn-group">
                    <asp:Button Text="GDS" runat="server" CssClass="btn active" ID="btnInvoiceTypeGDS"
                        OnClick="btnInvoiceType_Click" CommandArgument="GDS" />
                    <asp:Button Text="Instadose" runat="server" CssClass="btn" ID="btnInvoiceTypeInstadose"
                        OnClick="btnInvoiceType_Click" CommandArgument="Instadose" />
                </div>
            </li>
            <li>
                <asp:TextBox runat="server" ID="txtInvoiceSearch" Style="width: 300px;" placeholder="Search invoices..." />
                <div class="btn-group">
                    <asp:Button Text="Search" CssClass="btn btn-primary" runat="server" ID="btnInvoiceSearch"
                        OnClick="btnInvoiceSearch_Click" />
                    <asp:Button Text="Clear" CssClass="btn btn-danger" runat="server" ID="btnInvoiceClear"
                        OnClick="btnInvoiceClear_Click" />
                </div>
            </li>
            <li style="float: right;">

                <div class="btn-group">
                    <asp:Button Text="Transferred" runat="server" CssClass="btn active" ID="btnInvoiceFilterApproved"
                        OnClick="btnInvoiceFilter_Click" CommandArgument="1" />
                    <asp:Button Text="Failed" runat="server" CssClass="btn btn-danger" ID="btnInvoiceFilterErrors"
                        OnClick="btnInvoiceFilter_Click" CommandArgument="0" />
                </div>
            </li>
        </ul>

        <ec:GridViewEx ID="gvInvoices" runat="server" AutoGenerateColumns="False" CssClass="OTable"
            AlternatingRowStyle-CssClass="Alt" EmptyDataText="There are no invoices." AllowPaging="true"
            PageSize="20" OnSorting="gvInvoices_Sorting" GridLines="None" AllowSorting="true"
            CurrentSortDirection="Descending" CurrentSortedColumn="InvoiceDate" 
            OnPageIndexChanging="gvInvoices_PageIndexChanging">
            <Columns>
                <asp:TemplateField HeaderStyle-Width="20" HeaderStyle-CssClass="CenterAlign" ItemStyle-CssClass="CenterAlign">
                    <HeaderTemplate>
                        <input type="checkbox" id="invoice_status" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox Text="" ID="cbStatus" Checked="false" CssClass="invoice_status" runat="server" />
                        <asp:HiddenField runat="server" ID="hfInvoiceID" Value='<%# Eval("InvoiceID") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="InvoiceNumber" HeaderText="Invoice#" SortExpression="InvoiceNumber" />
                <asp:BoundField DataField="OrderID" HeaderText="Order#" SortExpression="OrderID" />
                <asp:BoundField DataField="InvoiceDate" HeaderText="Invoiced" SortExpression="InvoiceDate" DataFormatString="{0:M/d/yyyy}" />
                <asp:BoundField DataField="PONumber" HeaderText="PO#" SortExpression="PONumber" />
                <asp:BoundField DataField="BillingCompanyName" HeaderText="Billing Company" />
                <asp:BoundField DataField="ShippingCompanyName" HeaderText="Shipping Company" />
                <asp:BoundField DataField="TotalInvoiceAmount" HeaderText="Invoice Total" HeaderStyle-CssClass="RightAlign"
                    ItemStyle-CssClass="RightAlign" DataFormatString="{0:$#,###,##0.00}" />

                <asp:BoundField DataField="ReviewedBy" HeaderText="Reviewed By" SortExpression="ReviewedBy" />
                <asp:BoundField DataField="ReviewedDate" HeaderText="Reviewed Date" SortExpression="ReviewedDate" DataFormatString="{0:M/d/yyyy}" />
                <asp:TemplateField HeaderText="Error" HeaderStyle-Width="40">
                    <ItemTemplate>
                        <asp:Label ID="lblReleaseNotes" Text="" CssClass="Icon Comment" runat="server" />

                        <telerik:RadToolTip ID="RadToolTip1" runat="server" TargetControlID="lblReleaseNotes"
                            RelativeTo="Element" Position="BottomCenter" AutoCloseDelay="0" Width="400" RenderInPageRoot="true">
                            <div><strong>Error Message:</strong></div>
                            <%# DataBinder.Eval(Container, "DataItem.TransferErrorMessage")%>
                        </telerik:RadToolTip>

                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </ec:GridViewEx>
        <ul class="OToolbar JoinTableBottom">
            <li>
                <div class="btn-group">
                    <asp:Button Text="Approve" runat="server" CssClass="btn btn-success invoice_confirm"
                        data-action="approve" ID="btnInvoiceReviewApprove"
                        OnClick="btnInvoiceReview_Click" CommandArgument="2" />
                    <asp:Button Text="Reject" runat="server" CssClass="btn btn-info invoice_confirm"
                        data-action="reject" ID="btnInvoiceReviewReject"
                        OnClick="btnInvoiceReview_Click" CommandArgument="3" />
                </div>
            </li>
        </ul>
    </asp:Panel>
    
</asp:Content>

