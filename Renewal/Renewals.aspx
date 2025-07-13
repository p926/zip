<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Admin_Renewal_Renewals" Codebehind="Renewals.aspx.cs" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">
        $(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
            EndRequestHandler(null, null);
        });
        function EndRequestHandler(sender, args) {
            // Set the date picker objects
            $("#<%= txtPeriodFrom.ClientID %>").datepicker();
        $("#<%= txtPeriodTo.ClientID %>").datepicker();
    }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>

    <div class="FormError" id="renewalRowError" runat="server" visible="false">
        <p>
            <span class="MessageIcon"></span>
            <strong>Messages:</strong>
            <span id="renewalRowErrorMsg" runat="server">An error was encountered.</span>
        </p>
    </div>

    <asp:Panel runat="server" ID="pnlRenewalsTable">
        <div class="OToolbar JoinTable">
            <ul>
                <li>Billing Term:</li>
                <li>
                    <asp:DropDownList ID="ddlBillingTerm" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="gridviewBinder">
                        <asp:ListItem Text="All" Value="" />
                        <asp:ListItem Text="Yearly" Value="YEARLY" />
                        <asp:ListItem Text="Quarterly" Value="QUARTERLY" />
                    </asp:DropDownList>
                </li>
                <li>Brand:</li>
                <li>
                    <asp:DropDownList ID="ddlBrand" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="gridviewBinder">
                        <asp:ListItem Text="All" Value="" />
                        <asp:ListItem Text="Quantum" Value="Quantum" />
                        <asp:ListItem Text="Mirion" Value="Mirion" />
                        <asp:ListItem Text="IC Care" Value="IC Care" />
                    </asp:DropDownList>
                </li>
                <li>Billing Method:</li>
                <li>
                    <asp:DropDownList ID="ddlBillingMethod" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="gridviewBinder">
                        <asp:ListItem Text="All" Value="" />
                        <asp:ListItem Text="Purchase Order" Value="Purchase Order" />
                        <asp:ListItem Text="Credit Card" Value="Credit Card" />
                    </asp:DropDownList>
                </li>
                <li>Period From:</li>
                <li>
                    <asp:TextBox ID="txtPeriodFrom" runat="server" AutoPostBack="true"
                        OnTextChanged="txtPeriodFrom_TextChange" Style="width: 100px" />
                </li>
                <li>To:</li>
                <li>
                    <asp:TextBox ID="txtPeriodTo" runat="server" AutoPostBack="true"
                        OnTextChanged="txtPeriodTo_TextChange" Style="width: 100px" />
                </li>
            </ul>
        </div>
        <ec:GridViewEx ID="gvUpcomingRenewals" runat="server"
            AutoGenerateColumns="False" AllowPaging="True" AllowSorting="True" PageSize="20"
            DataKeyNames="AccountID" CurrentSortedColumn="WhenToBill"
            CurrentSortDirection="Ascending" CssClass="OTable"
            AlternatingRowStyle-CssClass="Alt" PagerSettings-Mode="NextPreviousFirstLast"
            OnSorting="gvUpcomingRenewals_Sorting"
            OnRowCommand="gvUpcomingRenewals_RowCommand" OnPageIndexChanging="gvUpcomingRenewals_PageIndexChanging"
            EmptyDataText="There are no renewals that meet the criteria you specified."
            SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
            SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button Text="Renew" CommandArgument='<%# Eval("AccountID") %>' CommandName="Renew" runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="AccountID" HeaderText="Acc#" SortExpression="AccountID" />
                <asp:BoundField DataField="AccountName" HeaderText="Account Name" SortExpression="AccountName" />
                <asp:BoundField DataField="WhenToBill" HeaderText="Renew On" SortExpression="WhenToBill" DataFormatString="{0:d}" />
                <asp:BoundField DataField="BillingMethod" HeaderText="Method" SortExpression="BillingMethod" />
                <asp:BoundField DataField="ContractEndDate" HeaderText="Contract Ends" SortExpression="ContractEndDate" DataFormatString="{0:d}" />
                <asp:BoundField DataField="CustomerType" HeaderText="Type" SortExpression="CustomerType" />
                <asp:BoundField DataField="LastBilled" HeaderText="Last Billed" SortExpression="LastBilled" DataFormatString="{0:d}" />
            </Columns>
        </ec:GridViewEx>
    </asp:Panel>

    <asp:Panel runat="server" ID="pnlReview" Visible="false">
        <asp:HiddenField runat="server" ID="hfRenewalNo" Value="0" />
        <div>
            <asp:Button Text="&lt;&lt; Back" runat="server" CssClass="OButton" OnClick="btnBack_Click" ID="Button1" />
            <br />
            <h3>Renewal Release Review</h3>
        </div>

        <ec:GridViewEx ID="GridViewEx1" runat="server" AutoGenerateColumns="false" CssClass="OTable"
            AlternatingRowStyle-CssClass="alt" EmptyDataText="There are no orders released.">
            <Columns>
                <asp:BoundField DataField="AccountID" HeaderText="Acc#" />
                <asp:BoundField DataField="OrderID" HeaderText="Order#" />
                <asp:BoundField DataField="OrderStatusName" HeaderText="Status" />
                <asp:BoundField ItemStyle-HorizontalAlign="Right" DataField="RenewalAmount" HeaderText="Amount" DataFormatString="{0:C}" />
                <asp:BoundField DataField="BrandName" HeaderText="Brand" />
                <asp:BoundField DataField="BillingTermDesc" HeaderText="Term" />
                <asp:BoundField DataField="PaymentMethodName" HeaderText="Method" />
                <asp:BoundField DataField="ErrorMessage" HeaderText="Status" />
            </Columns>
        </ec:GridViewEx>
    </asp:Panel>


    <asp:Panel runat="server" ID="pnlReviewOld" Visible="false">
        <asp:HiddenField runat="server" ID="hfAccountID" Value="0" />
        <div style="padding: 10px;">
            <asp:Button Text="&lt;&lt; Back" runat="server" CssClass="OButton" OnClick="btnBack_Click" ID="btnBack" />
            <br />
            <span style="font-weight: bold; font-size: 13pt;">Review the Renewal Order</span>
        </div>
        <div style="padding: 10px;">
            <div>
                <asp:Label Text="This account is set to be billed as a Purchase Order." ID="lblBillingMethod" runat="server" /></div>
            <asp:Label ID="lblCreditCard" Text="This account is set to pay with a credit card, and does not have one on file.  This renewal cannot be processed until then." runat="server" Visible="false" CssClass="error2" />
        </div>

        <ec:GridViewEx ID="gvReview" runat="server" AutoGenerateColumns="False"
            CssClass="OTable" AlternatingRowStyle-CssClass="alt"
            EmptyDataText="There are no instadose badges to renew for this account."
            CurrentSortDirection="Ascending" CurrentSortedColumn=""
            SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
            SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">

            <AlternatingRowStyle CssClass="alt" />
            <Columns>
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <asp:BoundField DataField="Type" HeaderText="Type" />
                <asp:BoundField DataField="UnitPrice" DataFormatString="{0:C}" HeaderText="Unit" />
                <asp:BoundField DataField="RenewalPrice" DataFormatString="{0:C}" HeaderText="Renewal" />
                <asp:BoundField DataField="Quantity" HeaderText="Quantity" />
                <asp:BoundField DataField="Total" DataFormatString="{0:C}" HeaderText="Renewal Total" ItemStyle-HorizontalAlign="Right" />
            </Columns>
            <RowStyle CssClass="Row" />
            <SelectedRowStyle CssClass="Row Selected" />
        </ec:GridViewEx>
        <table>
            <tr>
                <td>
                    <div style="float: right; text-align: right;">
                        <asp:Label Text="total" ID="lblRenewalTotal" runat="server" /></div>
                    <div style="float: right; padding-right: 5px; font-weight: bold;">Renewal Total: </div>
                    <div style="clear: both;"></div>
                </td>
            </tr>
            <tr>
                <td>
                    <div style="text-align: right; padding: 10px;">
                        <asp:Button Text="Generate Renewal" CssClass="OButton" runat="server" OnClick="btnGenerate_Click" ID="btnGenerate" />
                        <br />
                    </div>
                </td>
            </tr>
        </table>


    </asp:Panel>

    <asp:Panel runat="server" ID="pnlCompleted" Visible="false">

        <div style="padding: 10px;">
            <asp:Button Text="&lt;&lt; Back" runat="server" CssClass="OButton" OnClick="btnBack_Click" ID="btnBack2" />
            <br />
            <span style="font-weight: bold; font-size: 13pt;">Renewal Status</span>
        </div>

        <asp:Label ID="lblProcessingErrors" Text="" Visible="false" runat="server" CssClass="errorBox" Style="padding: 10px;" />

        <div class="review">
            <div class="review_container">
                <asp:Panel ID="respMessage" CssClass="message1" runat="server">Results</asp:Panel>
                <asp:Panel ID="respMessage2" CssClass="message2" runat="server">Order was not generated.</asp:Panel>
                <asp:Panel ID="respOrder" CssClass="response_error" runat="server">The order was not created.</asp:Panel>
                <asp:Panel ID="respPORequest" CssClass="response_error" runat="server">The PO request was not created.</asp:Panel>
                <asp:Panel ID="respPayment" CssClass="response_error" runat="server">The payment has not been processed.</asp:Panel>
                <asp:Panel ID="respUpdateAccount" CssClass="response_error" runat="server">The account contract and billings have not been created.</asp:Panel>
                <asp:Panel ID="respMAS" CssClass="response_error" runat="server">The order was not sent to MAS.</asp:Panel>
                <asp:Panel ID="respSoftrax" CssClass="response_error" runat="server">The order was not sent to Softrax.</asp:Panel>
            </div>
        </div>
    </asp:Panel>
</asp:Content>

