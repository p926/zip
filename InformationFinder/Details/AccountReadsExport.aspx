<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MirionDSD.master" AutoEventWireup="true" Inherits="InformationFinder_Details_AccountReadsExport" EnableEventValidation ="false"  Codebehind="AccountReadsExport.aspx.cs" %>
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
            $('#<%= txtExposureFrom.ClientID %>').datepicker({
                constrainInput: true,
                changeMonth: true,
                changeYear: true,
                dateFormat: 'mm/dd/yy',
                gotoCurrent: true,
                hideIfNoPrevNext: true,
                minDate: '-20y',
                maxDate: '+0y'
            });
            $('#<%= txtExposureTo.ClientID %>').datepicker({
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
    <div style="margin: 10px auto; width: 100%;">       
        <asp:Panel ID="pnlAccountReadsExport" runat="server">
            <div class="OToolbar JoinTable">      
                <ul>
                    <li>Body Region:</li>
                    <li>
                        <asp:DropDownList ID="ddlBodyRegion" runat="server" AutoPostBack="true" OnSelectedIndexChanged="gridviewBinder">
                        </asp:DropDownList>
                    </li>
                    <li>Exposure Date From:</li>
                    <li>
                         <asp:TextBox runat="server" ID="txtExposureFrom" CssClass="Size Small" OnTextChanged="txtExposureFrom_TextChange" AutoPostBack="true" />
                    </li>
                    <li>To:</li>
                    <li>
                         <asp:TextBox runat="server" ID="txtExposureTo" CssClass="Size Small" OnTextChanged="txtExposureTo_TextChange" AutoPostBack="true" />
                    </li>
                    <li class="RightAlign">
                        <asp:LinkButton ID="lnkbtnClearFilters" runat="server" 
                        OnClick="lnkbtnClearFilters_Click" 
                        Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" 
                        CssClass="Icon Remove" ToolTip="Clear Filters" />
                    </li>
                    <li class="RightAlign">
                        <asp:LinkButton ID="lnkbtnExport" runat="server"  
                        CommandName="Export" CssClass="Icon Export" 
                        OnClick="lnkbtnExport_Click" >Export to Excel
                        </asp:LinkButton>
                    </li>
                    <li class="Clear"></li>
                </ul>
            </div>
            <ec:GridViewEx ID="gvAccountReads" runat="server" CssClass="OTable" 
            AlternatingRowStyle-CssClass="Alt"  AutoGenerateColumns="False" 
            AllowPaging="True" meta:resourcekey="Grid" AllowSorting="True" PageSize="20" 
            DataKeyNames="AccountID" CurrentSortedColumn="AccountID" CurrentSortDirection="Ascending" 
            PagerStyle-CssClass="mt-hd" PagerSettings-Mode="Numeric"
            OnSorting="gvAccountReads_Sorting" OnPageIndexChanging="gvAccountReads_PageIndexChanging"
            EmptyDataText="There are no reads that meet the criteria you specified." 
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
                    <asp:BoundField DataField="CompleteExposureDate" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="Exposure Date" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                    SortExpression="CompleteExposureDate">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="TimeZoneDesc" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="TimeZoneDesc" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                    SortExpression="TimeZoneDesc">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="UserName" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="UserName" ItemStyle-CssClass="mt-itm" 
                    SortExpression="UserName">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="FullName" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="Full Name" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                    SortExpression="FullName">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="SerialNumber" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="Serial#" ItemStyle-CssClass="mt-itm" 
                    SortExpression="SerialNumber">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="BodyRegion" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="Body Region" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                    SortExpression="BodyRegion">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="Deep" HeaderStyle-CssClass="mt-hd"
                    DataFormatString="{0:N0}" 
                    HeaderText="Deep" ItemStyle-CssClass="mt-itm" SortExpression="Deep" >
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="Shallow" HeaderStyle-CssClass="mt-hd" 
                    DataFormatString="{0:N0}"
                    HeaderText="Shallow" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                    SortExpression="Shallow">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="Eye" HeaderStyle-CssClass="mt-hd" 
                    DataFormatString="{0:N0}" 
                    HeaderText="Eye" ItemStyle-CssClass="mt-itm" SortExpression="Eye" >
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                    </asp:BoundField>
                    <asp:BoundField DataField="UOMLocation" HeaderStyle-CssClass="mt-hd" 
                    HeaderText="UOM" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                    SortExpression="UOMLocation">
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
                <asp:Button ID="btnReturn" runat="server" Text="Return to Account" OnClick="btnReturn_Click" CssClass="OButton" ToolTip="Return to Account" />
            </div>     
        </asp:Panel>
       
    </div>
</asp:Content>