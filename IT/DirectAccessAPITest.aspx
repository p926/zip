<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_DirectAccessAPITest" Codebehind="DirectAccessAPITest.aspx.cs" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">

    <p>This page uses minimal code to display location and group data using the Instadose Direct Access APIs.</p>

    <strong>Account #: </strong><asp:TextBox runat="server" ID="txtAccountNo" Text="" Width="80" /> <asp:Button runat="server" ID="btnGo" OnClick="btnGo_Click" Text="Go" /> <b>Step 1:</b> <span style="font-style:italic;">User supplies an Account ID.</span>
    
    <asp:Panel runat="server" ID="pnlStep2" Visible="false">
        <p><b>Step 2:</b> <span style="font-style:italic;">During Post Back, a DALocations object is constructed and the Account ID is passed to the List method. The data is then bound to an Extended GridView.</span></p>
    </asp:Panel>

    <ec:GridViewEx ID="gvLocations" runat="server" CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
        AutoGenerateColumns="True" EmptyDataText="There are no locations to display."
        SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" SortOrderDescendingImagePath="~/images/icon_sort_descending.gif"
        Width="100%" onrowcommand="gvLocations_RowCommand">
        <Columns>
            <asp:ButtonField CommandName="DisplayGroup" ButtonType="Button" Text="Groups" />
        </Columns>
        <PagerSettings Mode="NextPreviousFirstLast" />
        <RowStyle CssClass="Row" />
        <SelectedRowStyle CssClass="Row Selected" />
    </ec:GridViewEx>
    
    <asp:Panel runat="server" ID="pnlStep3" Visible="false">
        <p><b>Step 3:</b> <span style="font-style:italic;">When the Groups button is clicked, a DAGroups object is constructed and Account ID and Location ID are passed to the List method.  The data is then bound to an Extended GridView.</span></p>
    </asp:Panel>

    <ec:GridViewEx ID="gvGroups" runat="server" CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
        AutoGenerateColumns="True" EmptyDataText="There are no groups to display."
        SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" SortOrderDescendingImagePath="~/images/icon_sort_descending.gif"
        Width="100%">
        <PagerSettings Mode="NextPreviousFirstLast" />
        <RowStyle CssClass="Row" />
        <SelectedRowStyle CssClass="Row Selected" />
    </ec:GridViewEx>
</asp:Content>
