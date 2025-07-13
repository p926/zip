<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MirionDSD.master" AutoEventWireup="true" Inherits="InformationFinder_Details_AccountUsersExport" EnableEventValidation ="false"  Codebehind="AccountUsersExport.aspx.cs" %>
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
        <asp:Panel ID="pnlAccountUsersExport" runat="server">
              <div class="OToolbar JoinTable">      
                 <ul>
                    <li>User Role:</li>
                    <li>
                        <asp:DropDownList ID="ddlUserRoles" runat="server" AutoPostBack="true" OnSelectedIndexChanged="GridViewBinder"></asp:DropDownList>
                    </li>
                    <li>Location:</li>
                    <li>        
                        <asp:DropDownList ID="ddlLocation" runat="server" AutoPostBack="true" OnSelectedIndexChanged="GridViewBinder"></asp:DropDownList>
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
                        <asp:LinkButton ID="lnkbtnClearFilters" runat="server" 
                        OnClick="lnkbtnClearFilters_Click" ToolTip="Clear Filters" 
                        Text="Clear Filters" CssClass="Icon Remove" CommandName="ClearFilters" />
                    </li>
                    <li class="RightAlign">
                        <asp:LinkButton ID="lnkbtnExport" runat="server" Text="Export to Excel"  
                        CommandName="ExportToExcel" CssClass="Icon Export" 
                        OnClick="lnkbtnExport_Click" ToolTip="Export to Excel" />
                    </li>
                    <li class="Clear"></li>
                </ul>
		     </div>
            <ec:GridViewEx ID="gvAccountUsers" runat="server" CssClass="OTable" 
            AlternatingRowStyle-CssClass="Alt"  AutoGenerateColumns="False" 
            AllowPaging="True" meta:resourcekey="Grid" AllowSorting="True" PageSize="20" 
            DataKeyNames="AccountID" CurrentSortedColumn="AccountID" CurrentSortDirection="Ascending" 
            PagerStyle-CssClass="mt-hd" PagerSettings-Mode="Numeric"
            OnSorting="gvAccountUsers_Sorting" OnPageIndexChanging="gvAccountUsers_PageIndexChanging"
            EmptyDataText="There are no users that meet the criteria you specified." 
            SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
            SortOrderDescendingImagePath="~/images/icon_sort_descending.gif" 
            Width="100%">
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
                    <asp:BoundField DataField="UserName" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="UserName" ItemStyle-CssClass="mt-itm" 
                    SortExpression="UserName">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="UserRoleName" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="User Role" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                    SortExpression="UserRoleName">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="FullName" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="Full Name" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                    SortExpression="FullName">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="Email" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="Email" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                    SortExpression="Email">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="Location" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="Location" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                    SortExpression="Location">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField> 
                    <asp:BoundField DataField="Active" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="Active" ItemStyle-CssClass="mt-itm" SortExpression="Active" >
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