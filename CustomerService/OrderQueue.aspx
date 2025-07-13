<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true"
    Inherits="InstaDose_CustomerService_OrderQueue" EnableEventValidation="false" Codebehind="OrderQueue.aspx.cs" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">
        $(function () {
            $(".select-all").click(function () { 
                $('.checkbox input:checkbox').attr("checked", $(this).is(":checked"));
            });
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <%--ERROR/SUCCESS MESSAGE(S)--%>
    <div id="divErrorMessage" runat="server" class="FormError" visible="false">
        <p><span class="MessageIcon"></span>
	    <strong>Messages:</strong>&nbsp;<span id="spnErrorMessage" runat="server">The following errors were encountered;</span></p>
        <asp:BulletedList ID="blltlstErrors" runat="server" BulletStyle="Disc"></asp:BulletedList>
    </div>
    <div id="divSuccessMessage" runat="server" class="FormMessage" visible="false">
        <p><span class="MessageIcon"></span>
        <strong>Messages:</strong>&nbsp;<span id="spnSuccessMessage" runat="server">The Order has been released successfully.</span>
        <asp:BulletedList ID="blltlstSuccesses" runat="server" BulletStyle="Disc"></asp:BulletedList>
        </p>
    </div>
    <%--END--%>
    <%--ORDER DETAILS TABLE/DATAGRID FILTER CONTROLS--%>
    <asp:Panel runat="server" ID="pnlOrdersTable">
        <div class="OToolbar JoinTable">
            <ul>
                <li>Order Type:</li>
                <li>
                    <asp:DropDownList ID="ddlOrderType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlOrderType_SelectedIndexChanged">
                        <asp:ListItem Value="" Text="All" />
                        <asp:ListItem Value="NEW" Text="New" />
                        <asp:ListItem Value="RENEWAL" Text="Renewals" />
                        <asp:ListItem Value="ADDON" Text="Add-On" />
                    </asp:DropDownList>
                </li>
                <li>Brand:</li>
                <li>
                    <asp:DropDownList ID="ddlBrand" runat="server" AutoPostBack="true" DataTextField="BrandName" DataValueField="BrandSourceID" 
                    OnSelectedIndexChanged="ddlBrand_SelectedIndexChanged">
                        <asp:ListItem Text="Mirion" Value="2" />
                        <asp:ListItem Text="IC Care" Value="3" />
                    </asp:DropDownList>
                </li>
                <li id="lblICCareDealers" visible="false" runat="server">IC Care Dealers:</li>
                <li>
                    <asp:DropDownList ID="ddlDealers" Visible="false" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlBrand_SelectedIndexChanged" 
                    DataTextField="DealerName" DataValueField="DealerID">
                    </asp:DropDownList>
                </li>
            </ul>
        </div>
        <ec:GridViewEx ID="gvOpenOrders" runat="server" AutoGenerateColumns="False"
        AllowPaging="True" AllowSorting="True" PageSize="20"
        DataKeyNames="OrderID" CurrentSortedColumn="WhenToBill" CurrentSortDirection="Ascending"
        CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
        PagerStyle-CssClass="mt-hd" PagerSettings-Mode="NextPreviousFirstLast"
        EmptyDataText="There are no open orders." OnPageIndexChanging="gvOpenOrders_PageIndexChanging"
        OnSorting="gvOpenOrders_Sorting"
        Width="100%" OnRowDataBound="gvOpenOrders_RowDataBound"
        SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
        SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <input type="checkbox" class="select-all" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="cbSelected" runat="server" CssClass="checkbox" />
                        <asp:HiddenField ID="hfOrderID" runat="server" Value='<%# Eval("OrderID") %>' />
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Left" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Order" SortExpression="OrderID">
                    <ItemTemplate>
                        <asp:HyperLink ID="hlOrder" runat="server" Text='<%# Eval("OrderID") %>' NavigateUrl='<%# Eval("OrderID", "CreateOrder.aspx?OrderID={0}") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="PO#" Visible="true">
                    <ItemTemplate>
                        <asp:TextBox ID="txtPONumber" runat="server" Text='<%# Eval("PONumber") %>' SkinID="none" Width="80" TextMode="SingleLine" />
                        <asp:Label ID="lblPONumber" runat="server" Text='<%# Eval("PONumber") %>' />
                    </ItemTemplate>
                    <HeaderStyle></HeaderStyle>
                    <ItemStyle></ItemStyle>
                </asp:TemplateField>
                <asp:TemplateField SortExpression="OrderType" HeaderText="Order Type" HeaderStyle-Wrap="false">
                    <ItemTemplate>
                        <asp:Label ID="lblOrderType" runat="server" Text='<%# Eval("OrderType") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="CreatedDate" HeaderText="Ordered" SortExpression="CreatedDate" DataFormatString="{0:M/d/yyyy}">
                </asp:BoundField>
                <asp:BoundField DataField="AccountID" HeaderText="Acc#" SortExpression="AccountID">
                </asp:BoundField>
                <asp:BoundField DataField="AccountName" HeaderText="Account Name" SortExpression="AccountName">
                </asp:BoundField>
                <asp:BoundField DataField="BillingCompany" HeaderText="Company" SortExpression="BillingCompany">
                </asp:BoundField>
                <asp:BoundField DataField="ReferralCode" HeaderText="Referral" SortExpression="ReferralCode">
                </asp:BoundField>
                <asp:TemplateField HeaderText="Dealer#">
                    <ItemTemplate>
                        <asp:Label ID="lblDealerID" runat="server" Text="" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Type" SortExpression="PaymentType">
                    <ItemTemplate>
                        <asp:Label ID="lblPaymentType" runat="server" Text="" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Total" ItemStyle-Wrap="false">
                    <ItemTemplate>
                        <asp:Label ID="lblOrderTotal" runat="server" Text="" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerSettings Mode="NextPreviousFirstLast" />
            <PagerStyle />
        </ec:GridViewEx>
        <%--RELEASE ORDER(S) BUTTON--%>
        <div style="text-align: right;">
            <asp:Button ID="btnReleaseOrders" Text="Update PO# / Release" runat="server" OnClick=" btnReleaseOrders_Click" CssClass="OButton" />
        </div>
        <%--END--%>
    </asp:Panel>
    <%--END--%>
    <%--RELEASED ORDER(S) DATAGRID W/ BACK BUTTON--%>
    <asp:Panel runat="server" ID="pnlReview" Visible="false">
        <br />
        <div class="title">
            <asp:Button Text="&lt;&lt; Back" runat="server" CssClass="OButton" OnClick="btnBack_Click" ID="btnBack" />
            <br />
            <h2>Released Orders</h2>
        </div>
        <ec:GridViewEx ID="gvReview" runat="server" AutoGenerateColumns="false" CssClass="OTable"
        AlternatingRowStyle-CssClass="Alt" EmptyDataText="There are no valid orders released.">
            <Columns>
                <asp:BoundField DataField="OrderID" HeaderText="Order#" />
                <asp:BoundField DataField="OrderStatusName" HeaderText="Status" />
                <asp:BoundField DataField="AccountID" HeaderText="Acc#" />
                <asp:BoundField DataField="AccountName" HeaderText="Account" />
                <asp:BoundField DataField="BillingCompany" HeaderText="Company" />
                <asp:BoundField DataField="BrandName" HeaderText="Brand" />
                <asp:BoundField DataField="BillingTermDesc" HeaderText="Term" />
                <asp:BoundField DataField="PaymentMethodName" HeaderText="Method" />
            </Columns>
        </ec:GridViewEx>
    </asp:Panel>
    <%--END--%>
</asp:Content>

