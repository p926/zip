<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MirionDSD.master" AutoEventWireup="true" Inherits="InformationFinder_Details_AccountBadgesExport" EnableEventValidation ="false"  Codebehind="AccountBadgesExport.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">
        $(document).ready(function () {
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
        });

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
<div style="margin: 10px auto; width: 100%;">    
    <asp:Panel ID="pnlAccountBadges" runat="server">
          <div class="OToolbar JoinTable">      
             <ul>
                <li>Body Region:</li>
                <li>
                    <asp:DropDownList ID="ddlBodyRegion" runat="server" AutoPostBack="true" OnSelectedIndexChanged="GridViewBinder"></asp:DropDownList>
                </li>
                <li>Product Color:</li>
                <li>        
                    <asp:DropDownList ID="ddlProductColor" runat="server" AutoPostBack="true" OnSelectedIndexChanged="GridViewBinder"></asp:DropDownList>
                </li>
                <li>Initailized:</li>
				<li>
                    <asp:DropDownList ID="ddlInitailized" runat="server" AutoPostBack="true" OnSelectedIndexChanged="GridViewBinder">
                        <asp:ListItem Value="0" Text="All" />
                        <asp:ListItem Value="Yes" Text="Yes" />
                        <asp:ListItem Value="No" Text="No" />
                    </asp:DropDownList>
                </li>
                <li>Active:</li>
				<li>
                    <asp:DropDownList ID="ddlActive" runat="server" AutoPostBack="true" OnSelectedIndexChanged="GridViewBinder">
                        <asp:ListItem Value="0" Text="All" />
                        <asp:ListItem Value="Yes" Text="Yes" />
                        <asp:ListItem Value="No" Text="No" />
                    </asp:DropDownList>
                </li>
                <li class="RightAlign">
                    <asp:LinkButton ID="lnkbtnClearFilters" runat="server" OnClick="lnkbtnClearFilters_Click" Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                </li>
                <li class="RightAlign">
                     <asp:LinkButton ID="lnkbtnExport" runat="server" CommandName="Export" CssClass="Icon Export" OnClick="lnkbtnExport_Click" >Export to Excel</asp:LinkButton>
                </li>
                <li class="Clear"></li>
            </ul>
		 </div>
        <ec:GridViewEx ID="gvAccountBadges" runat="server" CssClass="OTable" 
        AlternatingRowStyle-CssClass="Alt"  AutoGenerateColumns="False" 
        AllowPaging="True" meta:resourcekey="Grid" AllowSorting="True" PageSize="20" 
        DataKeyNames="AccountID" CurrentSortedColumn="AccountID" CurrentSortDirection="Ascending" 
        PagerStyle-CssClass="mt-hd" PagerSettings-Mode="Numeric"
        OnSorting="gvAccountBadges_Sorting" OnPageIndexChanging="gvAccountBadges_PageIndexChanging"
        EmptyDataText="There are no badges that meet the criteria you specified." 
        SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
        SortOrderDescendingImagePath="~/images/icon_sort_descending.gif" 
        Width="100%">
        <AlternatingRowStyle CssClass="Alt" />
            <Columns>
                <asp:BoundField DataField="AccountID" HeaderStyle-CssClass="mt-hd" HeaderText="Account#" meta:resourcekey="AccountID" ItemStyle-CssClass="mt-itm" ReadOnly="True" >
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="CompanyName" HeaderStyle-CssClass="mt-hd" HeaderText="Company Name" ItemStyle-CssClass="mt-itm" >
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="SerialNumber" HeaderStyle-CssClass="mt-hd" HeaderText="Serial#" ItemStyle-CssClass="mt-itm" SortExpression="SerialNumber">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="FullName" HeaderStyle-CssClass="mt-hd" HeaderText="Assigned To" ItemStyle-CssClass="mt-itm" ReadOnly="True" SortExpression="FullName">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="BodyRegion" HeaderStyle-CssClass="mt-hd" HeaderText="Body Region" ItemStyle-CssClass="mt-itm" ReadOnly="True" SortExpression="BodyRegion">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="ProductColor" HeaderStyle-CssClass="mt-hd" HeaderText="Product Color" ItemStyle-CssClass="mt-itm" ReadOnly="True" SortExpression="ProductColor">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="OrderID" DataFormatString="{0:d}" HeaderStyle-CssClass="mt-hd" HeaderText="Order#" ItemStyle-CssClass="mt-itm" ReadOnly="True" SortExpression="OrderID">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="Location" HeaderStyle-CssClass="mt-hd" HeaderText="Location" ItemStyle-CssClass="mt-itm" ReadOnly="True" SortExpression="Location">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="ServiceStart" DataFormatString="{0:d}" HeaderStyle-CssClass="mt-hd" HeaderText="Service Start" ItemStyle-CssClass="mt-itm" SortExpression="ServiceStart">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="ServiceEnd" DataFormatString="{0:d}" HeaderStyle-CssClass="mt-hd" HeaderText="Service End" ItemStyle-CssClass="mt-itm" SortExpression="ServiceEnd">
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
                <asp:BoundField DataField="FormattedInitialized" HeaderStyle-CssClass="mt-hd" HeaderText="Initialized" ItemStyle-CssClass="mt-hd" SortExpression="Initialized" HeaderStyle-HorizontalAlign="Right">      
                    <HeaderStyle CssClass="mt-itm" />   
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>   
                <asp:BoundField DataField="Active" HeaderStyle-CssClass="mt-hd" HeaderText="Active" ItemStyle-CssClass="mt-itm" SortExpression="Active" >
                    <HeaderStyle CssClass="mt-hd" />
                    <ItemStyle CssClass="mt-itm" />
                </asp:BoundField>
            </Columns>
            <PagerSettings Mode="Numeric" />
            <PagerStyle CssClass="mt-hd" />
            <RowStyle CssClass="Row" />
            <SelectedRowStyle CssClass="Row Selected" />
        </ec:GridViewEx>
        <div style="text-align: right;">
            <asp:Button ID="btnReturn" runat="server" Text="Return to Account" OnClick="btnReturn_Click" CssClass="OButton" />
        </div>     
    </asp:Panel>
</div>
</asp:Content>