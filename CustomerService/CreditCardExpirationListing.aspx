<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_CreditCardExpirationListing" Codebehind="CreditCardExpirationListing.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<script type="text/javascript">
    function pageLoad(sender, args) {
        // Maintains jQuery datepicker(s) after PostBack.
        if (args.get_isPartialLoad()) {
            loadDatePickers();
        }
        $(document).ready(function () {
            loadDatePickers();
        });
    }

    function loadDatePickers() {
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
</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>   
    <div class="FormError" id="creditCardExpirationError" runat="server" visible="false">
	    <p><span class="MessageIcon"></span>
	    <strong>Messages:</strong>&nbsp;<span id="creditCardExpirationErrorMsg" runat="server">An error was encountered.</span></p>
    </div>
    <asp:Panel ID="pnlSearchToolbard" runat="server">
    <div class="OToolbar JoinTable">
        <ul>
            <li>Credit Card Type:</li>
            <li>
                <asp:DropDownList ID="ddlCreditCardTypes" runat="server" AutoPostBack="true" OnSelectedIndexChanged="gridviewBinder" AppendDataBoundItems="true">
                    <asp:ListItem Text="---Select CC Type---" Value="0"></asp:ListItem>
                </asp:DropDownList>
            </li>
            <li>Expiration Date</li>
            <li>From:</li>
            <li>
                <asp:TextBox ID="txtFromExpirationDate" runat="server" CssClass="Size Small" ontextchanged="txtFromExpirationDate_TextChanged" AutoPostBack="true"></asp:TextBox>   
            </li>
            <li>To:</li>
            <li>
                <asp:TextBox ID="txtToExpirationDate" runat="server" CssClass="Size Small" ontextchanged="txtToExpirationDate_TextChanged" AutoPostBack="true"></asp:TextBox>    
            </li>         
        </ul>
    </div>
    </asp:Panel>
    <asp:UpdatePanel ID="updtpnlCreditCardExpirations" runat="server">
        <ContentTemplate>
            <ec:GridViewEx ID="gvCreditCardExpirations" runat="server" AutoGenerateColumns="False" 
            AllowPaging="True" AllowSorting="True" PageSize="20" 
            DataKeyNames="AccountID" CurrentSortedColumn="DT_ExpirationDate" CurrentSortDirection="Ascending"
            CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
            PagerSettings-Mode="NextPreviousFirstLast"
            OnSorting="gvCreditCardExpirations_Sorting" OnPageIndexChanging="gvCreditCardExpirations_PageIndexChanging"
            EmptyDataText="There are no records available." Width="100%" 
            SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
            SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">
            <AlternatingRowStyle CssClass="Alt" />
                <Columns>            
                    <asp:TemplateField HeaderText="Acct. #" ItemStyle-HorizontalAlign="Left" SortExpression="AccountID">
                    <ItemTemplate>
                        <asp:HyperLink ID="hyprlnkOrderIDLink" runat="server" 
                        NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"AccountID","~/InformationFinder/Details/Account.aspx?ID={0}") %>' 
                        Target="_blank" Text='<%# Bind("AccountID") %>'>
                        </asp:HyperLink>  
                    </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="CompanyName" HeaderText="Company Name" ItemStyle-Wrap="false" SortExpression="CompanyName">
                    </asp:BoundField>
                    <asp:BoundField DataField="CreditCardName" HeaderText="CC Type" ItemStyle-Wrap="false">
                    </asp:BoundField>
                    <asp:BoundField DataField="NumberEncrypted" HeaderText="CC #" ItemStyle-Wrap="false">
                    </asp:BoundField>
                    <asp:BoundField DataField="DT_ExpirationDate" HeaderText="Expriation Date" SortExpression="DT_ExpirationDate" DataFormatString="{0:MM/yyyy}">
                    </asp:BoundField>
                </Columns>	
                <PagerSettings Mode="NextPreviousFirstLast" />
                <RowStyle CssClass="Row" />
                <SelectedRowStyle CssClass="Row Selected" />
            </ec:GridViewEx>   
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

