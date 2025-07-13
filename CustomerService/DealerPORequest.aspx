<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_CustomerService_DealerPORequest" EnableEventValidation="false" Codebehind="DealerPORequest.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript">

    $(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(postBack);
        postBack();
    });

    function postBack() {
        $('.datepicker').datepicker({
            constrainInput: true,
            changeMonth: true,
            changeYear: true,
            dateFormat: 'mm/dd/yy',
            gotoCurrent: true,
            hideIfNoPrevNext: true,
            minDate: '-5y',
            maxDate: 0
        });

        // selected serialno boxes.
        $('.cbHeader > input').click(function () {
            $('#<%= gvOpenOrders.ClientID %> input:checkbox').attr("checked", $(this).is(":checked"));
        });

    }
    
</script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>   
    <div class="FormError" id="error" runat="server" visible="false">
		<p><span class="MessageIcon"></span>
	        <strong>Messages:</strong>
            <span id="errorMsg" runat="server" >               
            </span>
        </p>
	</div>

    <asp:Panel ID="pnlSearchToolbard" runat="server">
    <div class="OToolbar">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
        <ul>
             <li>
                <asp:RadioButton Text="Account" ID="rbAccountBilling" GroupName="BillingType" OnCheckedChanged="BillingType_CheckedChanged" AutoPostBack="true" Checked="true"   runat="server" /> or
                <asp:RadioButton Text="Location" ID="rbLocationBilling" GroupName="BillingType" OnCheckedChanged="BillingType_CheckedChanged" AutoPostBack="true" runat="server" /> Billing
            </li>    
            <li>
                IC Care Dealers:&nbsp;
                <asp:DropDownList ID="ddlDealers" runat="server" AutoPostBack="true"  DataTextField="DisplayName" DataValueField="DealerID"
                        OnSelectedIndexChanged="bindGridView" AppendDataBoundItems="true">
                </asp:DropDownList>

            </li>
            <li>Order Type:
                <asp:DropDownList ID="ddlOrderType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="bindGridView">
                    <asp:ListItem Value="" Text="All"></asp:ListItem>
                    <asp:ListItem Value="NEW" Text="New"></asp:ListItem>
                    <asp:ListItem Value="ADDON" Text="Add-Ons"></asp:ListItem>
                    <asp:ListItem Value="RENEWAL" Text="Renewals"></asp:ListItem>
                </asp:DropDownList>
            </li>
                     
        </ul>
        </ContentTemplate>
    </asp:UpdatePanel>
    </div>


    </asp:Panel>
    <asp:UpdatePanel ID="upnlOpenOrders" runat="server">
        <ContentTemplate>

            <div class="OToolbar JoinTable">
                <ul>
                    <li>From:&nbsp;<asp:TextBox ID="txtOrderDateFrom" CssClass="datepicker" runat="server" Width="70" ontextchanged="DateRange_TextChanged" AutoPostBack="true"></asp:TextBox></li>
                    <li>To:&nbsp;<asp:TextBox ID="txtOrderDateTo" CssClass="datepicker" runat="server" Width="70" ontextchanged="DateRange_TextChanged" AutoPostBack="true"></asp:TextBox></li>
                    <li><asp:LinkButton ID="lbtnShowAll" runat="server" OnClick="lbtnShowAll_Click" Text="Show All" CssClass="Icon Table" /></li>
                    <li><asp:LinkButton ID="lnkbtnClearFilters" runat="server" OnClick="lnkbtnClearFilters_Click" Text="Reset Filters" CssClass="Icon Remove" /></li>
                </ul>
            </div>

            <ec:GridViewEx ID="gvOpenOrders" runat="server" AutoGenerateColumns="False" 
            AllowPaging="True" AllowSorting="True" PageSize="20" 
            DataKeyNames="OrderID" CurrentSortedColumn="CreatedDate" CurrentSortDirection="Ascending" 
            CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
            PagerSettings-Mode="NextPreviousFirstLast"
            OnSorting="gvOpenOrders_Sorting" OnPageIndexChanging="gvOpenOrders_PageIndexChanging"
            EmptyDataText="There are no open orders."
            SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
            SortOrderDescendingImagePath="~/images/icon_sort_descending.gif" >
            <AlternatingRowStyle CssClass="Alt" />
                <Columns>     
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <asp:CheckBox ID="cbHeader" CssClass="cbHeader" runat="server" Checked="false"
                                ToolTip="Select ALL Orders" class="chkbxOrderHeader" />
                        </HeaderTemplate>
                        <HeaderStyle HorizontalAlign="Left" Width="20px"
                            Wrap="false" />
                        <ItemTemplate>
                            <asp:CheckBox ID="cbSelected" runat="server" />
                            <asp:HiddenField runat="server" ID="hfOrderID" Value='<%# Eval("OrderID") %>' />
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Left" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Order#" ItemStyle-HorizontalAlign="Left" SortExpression="OrderID">
                        <ItemTemplate>
                            <asp:HyperLink ID="hlOrderID" runat="server" 
                            NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"OrderID","ReviewOrder.aspx?ID={0}" ) %>' 
                            Target="_blank" Text='<%# Bind("OrderID") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="PO#" ItemStyle-HorizontalAlign="Left" SortExpression="PONumber">
                        <ItemTemplate>
                            <asp:Label  runat="server" Text='<%# Eval("PONumber") %>' ID="lblPONumber" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="OrderType" HeaderText="Type" SortExpression="OrderType" ItemStyle-Wrap="false">
                    </asp:BoundField>
                    <asp:BoundField DataField="OrderDate" HeaderText="Ordered" SortExpression="OrderDate" ItemStyle-Wrap="false" DataFormatString="{0:d}">
                    </asp:BoundField>
                    <asp:TemplateField HeaderText="Account" ItemStyle-HorizontalAlign="Left" SortExpression="AccountName">
                        <ItemTemplate>
                            <asp:HyperLink ID="hlAccountName" runat="server" 
                            NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"AccountID","~/InformationFinder/Details/Account.aspx?ID={0}" ) %>' 
                            Target="_blank" Text='<%# Bind("AccountName") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderStyle-HorizontalAlign="Center" DataField="BillingCompany" HeaderText="Company" SortExpression="BillingCompany">
                    </asp:BoundField>
                    <asp:BoundField DataField="ReferralCode" HeaderText="Referral" SortExpression="ReferralCode">
                    </asp:BoundField>
                    <asp:BoundField DataField="PaymentMethod" HeaderText="Method" ItemStyle-Wrap="false" SortExpression="PaymentType">
                    </asp:BoundField>
                    <asp:BoundField DataField="OrderTotal" HeaderStyle-HorizontalAlign="Right" HeaderText="Total" DataFormatString="${0:#,##0.00}" ItemStyle-HorizontalAlign="Right" ItemStyle-Wrap="false" SortExpression="OrderTotal">
                    </asp:BoundField>
                </Columns>	
                <PagerSettings Mode="NextPreviousFirstLast" />
                <RowStyle CssClass="Row" />
                <SelectedRowStyle CssClass="Row Selected" />
            </ec:GridViewEx>
        </ContentTemplate>
    </asp:UpdatePanel>
            
        <div style="text-align: right;">
            <asp:Button ID="btnViewDealerPORequest"  CssClass="OButton" runat="server" Text="View Dealer PO Request" OnClick="btnViewDealerPORequest_Click" />
        </div>
</asp:Content>

