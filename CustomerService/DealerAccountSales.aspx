<%@ Page Title="Dealer Account Sales" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_DealerAccountSales" Codebehind="DealerAccountSales.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<script type="text/javascript">
    $(document).ready(function () {
		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
		JQueryControlsLoad();
	});

    function JQueryControlsLoad() {
        // jQuery Tabs.
        // Also maintains Tab Index/Selected Tab.
        var activeTabIndex = parseInt($('#<%= hdnfldTabIndex.ClientID %>').val());

        $("#tabsContainer").tabs({
            selected: activeTabIndex,
            show: function() {
                var selectedTab = $('#tabsContainer').tabs('option', 'selected');
                $("#<%= hdnfldTabIndex.ClientID %>").val(selectedTab);
            }
        });
    }
    //$('#tabsContainer').tabs();
</script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
<act:toolkitscriptmanager id="ToolkitScriptManager1" runat="server"></act:toolkitscriptmanager>
    <asp:HiddenField ID="hdnfldTabIndex" runat="server" Value="0" />
    <div class="FormError" id="formErrors" runat="server" visible="false">
		<p><span class="MessageIcon"></span>
		<strong>Messages:</strong>&nbsp;<span id="errorMsg" runat="server" >An error was encountered.</span></p>
    </div>
    <div id="mainFormArea" runat="server">
        <%--Toolbar Component for Dealer ID Search.--%>
        <div id="divDealerIDSearchToolbar" class="OToolbar JoinTable" runat="server">
            <ul>
                <li>Dealer #:</li>
                <li>                           
                    <asp:DropDownList ID="ddlDealerID" runat="server" AutoPostBack="true" 
                        AppendDataBoundItems="true" 
                        onselectedindexchanged="ddlDealerID_SelectedIndexChanged">
                        <asp:ListItem Text="---Select---" Value="" Selected="True"></asp:ListItem>
                    </asp:DropDownList>
                </li>
            </ul>
        </div>
        <%--End--%>
        <%--ACCOUNTS|INVOICE HISTORY TABS--%>
        <table id="tblMainContentArea" class="OTable JoinTable" runat="server" cellpadding="0" cellspacing="0">
            <tr>
                <td width="100%">
                    <div id="tabsContainer">
                        <ul>
				            <li><a href="#AccountListing_tab" id="AccountListingTabHeader" runat="server">Account Listing</a></li>
				            <li><a href="#InvoiceHistory_tab" id="InvoiceHistoryTabHeader" runat="server">Invoice History</a></li>
			            </ul>
			            <div id="AccountListing_tab">
				            <asp:UpdatePanel ID="updtpnlAccountListing" runat="server"  UpdateMode="Conditional">
                                <ContentTemplate>
                                    <ec:GridViewEx ID="gvAccountListing" CssClass="OTable" runat="server"
							        AutoGenerateColumns="false" AlternatingRowStyle-CssClass="Alt"
                                    DataKeyName="SalesRepDistID" meta:resourcekey="Grid" CurrentSortedColumn="SalesRepDistID"
                                    CurrentSortedDirection="Ascending" SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
                                    SortOrderDescendingImagePath="~/images/icon_sort_descending.gif" AllowPaging="True" 
							        AllowSorting="True" PageSize="20" OnSorting="gvAccountListing_Sorting"
                                    EmptyDataText="This Sales Rep. # does not have any associated Accounts." 
                                    OnPageIndexChanging="gvAccountListing_PageIndexChanging">
							            <Columns>
                                            <asp:BoundField DataField="SalesRepDistID" HeaderText="Dealer #" SortExpression="SalesRepDistID" />
                                            <asp:BoundField DataField="ReferralCode" ShowHeader="false" ReadOnly="true" Visible="false" SortExpression="ReferralCode" />
                                            <asp:BoundField DataField="SalesRepCompanyName" HeaderText="Dealer Company Name" SortExpression="SalesRepCompanyName" />
                                            <asp:BoundField DataField="AccountID" HeaderText="Account #" SortExpression="AccountID" />
                                            <asp:BoundField DataField="CompanyName" HeaderText="Company Name" SortExpression="CompanyName" />
                                            <asp:BoundField DataField="ContractStartDate" HeaderText="Contract Start Date" SortExpression="ContractStartDate" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="ContractEndDate" HeaderText="Contract End Date" SortExpression="ContractEndDate" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="NumberOfActiveDevices" HeaderText="Active Devices" SortExpression="NumberOfActiveDevices" />
                                            <asp:CheckBoxField DataField="Active" HeaderText="Active" SortExpression="Active" />
					                    </Columns>
							            <EmptyDataTemplate>
								            <div class="NoData">
									            This Sales Rep. # does not have any associated Accounts.
								            </div>
							            </EmptyDataTemplate>
							            <AlternatingRowStyle CssClass="Alt" />
							            <PagerStyle CssClass="Footer" />
						            </ec:GridViewEx>
                                </ContentTemplate>
				            </asp:UpdatePanel>
                            <hr />
                            <p style="text-align: right;">
                                <asp:Button ID="btnExportToExcel_AccountListing" runat="server" 
                                CssClass="OButton" Text="Export to Excel" 
                                onclick="btnExportToExcel_AccountListing_Click" />
                            </p>
                        </div>
                        <div id="InvoiceHistory_tab">
				            <asp:UpdatePanel ID="updtpnlInvoiceHistory" runat="server"  UpdateMode="Conditional">
                                <ContentTemplate>
                                    <ec:GridViewEx ID="gvInvoiceHistory" CssClass="OTable" runat="server"
							        AutoGenerateColumns="false" AlternatingRowStyle-CssClass="Alt"
                                    DataKeyName="SalespersonNo" meta:resourcekey="Grid" CurrentSortedColumn="SalespersonNo"
                                    CurrentSortedDirection="Ascending" SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
                                    SortOrderDescendingImagePath="~/images/icon_sort_descending.gif" AllowPaging="True" 
							        AllowSorting="True" PageSize="20" OnSorting="gvInvoiceHistory_Sorting"
                                    EmptyDataText="This Sales Rep. # does not have any associated Invoice History." 
                                    OnPageIndexChanging="gvInvoiceHistory_PageIndexChanging">
							            <Columns>
                                            <asp:BoundField DataField="SalesPersonNo" HeaderText="Dealer #" SortExpression="SalespersonNo" />
                                            <asp:BoundField DataField="SalesPersonName" HeaderText="Dealer Company Name" SortExpression="SalesPersonName" />
                                            <asp:BoundField DataField="CustomerNo" HeaderText="Customer #" SortExpression="CustomerNo" />
                                            <asp:BoundField DataField="InvoiceNo" HeaderText="Invoice No." SortExpression="InvoiceNo" />
                                            <asp:BoundField DataField="BillToName" HeaderText="Bill To Name" SortExpression="BillToName" />
                                            <asp:BoundField DataField="InvoiceDate" HeaderText="Invoice Date" SortExpression="InvoiceDate" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="InvoiceAmount" HeaderText="Invoice Amount" SortExpression="InvoiceDate" />
					                    </Columns>
							            <EmptyDataTemplate>
								            <div class="NoData">
									            This Sales Rep. # does not have any associated Invoice History.
								            </div>
							            </EmptyDataTemplate>
							            <AlternatingRowStyle CssClass="Alt" />
							            <PagerStyle CssClass="Footer" />
						            </ec:GridViewEx>
                                </ContentTemplate>
				            </asp:UpdatePanel>
                            <hr />
                            <p style="text-align: right;">
                            <asp:Button ID="btnExportToExcel_InvoiceHistory" runat="server" 
                            CssClass="OButton" Text="Export to Excel" 
                            onclick="btnExportToExcel_InvoiceHistory_Click" />
                            </p>
                        </div>
                    </div>
                </td>
            </tr>
        </table>
        <%--END--%>
    </div>  
</asp:Content>

