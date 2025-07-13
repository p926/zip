<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="RenewalExceptions.aspx.cs" Inherits="portal_instadose_com_v3.Finance.Renewal.RenewalExceptions" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<style type="text/css">
		.date-picker {
			width: 75px;
		}
	</style>
	<script type="text/javascript">
		$(function () {
			Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(showProgress);
			Sys.WebForms.PageRequestManager.getInstance().add_endRequest(hideProgress);
			Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequestHandler);
			endRequestHandler(null, null);
		});

		function showProgress(sender, args) {
			var c = '<%=modalExtender.ClientID %>';
			$find(c).show();
		}

		function hideProgress(sender, args) {
			var c = '<%=modalExtender.ClientID %>';
			$find(c).hide();
		}

		function endRequestHandler(sender, args) {
			// Set the date picker objects
			$(".date-picker").datepicker();
		}
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
	<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

	<asp:UpdatePanel ID="upRenewalException" runat="server">
		<ContentTemplate>

			<asp:UpdateProgress id="LoadingProgress" runat="server" DynamicLayout="true" DisplayAfter="0" >
				<ProgressTemplate>            
				</ProgressTemplate>
			</asp:UpdateProgress>

			<act:ModalPopupExtender ID="modalExtender" runat="server" TargetControlID="LoadingProgress"
				PopupControlID="pnLoader" BackgroundCssClass="modalBackground" Enabled="true" >
			</act:ModalPopupExtender>

			<asp:Panel ID="pnLoader" runat="server">
				<div style="width: 100%; vertical-align: middle; text-align: center">
					<img src="/images/orangebarloader.gif" />
				</div>
			</asp:Panel>

			<asp:Panel CssClass="FormError" ID="divErrorMessage" runat="server" visible="false">
				<p>
					<span class="MessageIcon"></span>
					<strong>Messages:</strong> <span id="errorMessage" runat="server">An error was encountered.</span>
				</p>
			</asp:Panel>

			<div class="OToolbar">
				<ul>
					<li>Billing Term:</li>
					<li>
						<asp:DropDownList ID="ddlBillingTerm" runat="server" DataValueField="BillingTermID" DataTextField="BillingTermDesc" />
					</li>
					<li>Brand:</li>
					<li>
						<asp:DropDownList ID="ddlBrand" runat="server" DataValueField="BrandName" DataTextField="BrandName" />
					</li>
					<li>Customer Type:</li>
					<li>
						<asp:DropDownList ID="ddlCustomerType" runat="server" DataValueField="CustomerTypeID" DataTextField="CustomerTypeName" />
					</li>
					<li>Billing Method:</li>
					<li>
						<asp:DropDownList ID="ddlBillingMethod" runat="server" DataValueField="PaymentMethodID" DataTextField="PaymentMethodName" />
					</li>
				</ul>
			</div>
			<div class="OToolbar JoinTable">
				<ul>
					<li>Period From:</li>
					<li>
						<asp:TextBox runat="server" ID="txtPeriodFrom" CssClass="date-picker" />
					</li>
					<li>To:</li>
					<li>
						<asp:TextBox runat="server" ID="txtPeriodTo" CssClass="date-picker" />
					</li>
					<li>
						<asp:DropDownList ID="ddlExceptionCode" runat="server">
							<asp:ListItem Value="" Text="All" />
							<asp:ListItem Value="NR" Text="Account without a rate code" />
							<asp:ListItem Value="ND" Text="Account without a device" />
							<asp:ListItem Value="RN" Text="Rate cannot be found by device quantity" />
							<asp:ListItem Value="DR" Text="100% Discount rate" />
							<asp:ListItem Value="RP" Text="$0.00 Rate price" />
						</asp:DropDownList>
					</li>
					<li>
						<asp:LinkButton ID="lnkbtnDisplay" runat="server" ToolTip="Search" CssClass="btn btn-success" OnClick="lnkbtnDisplay_Click">Apply Filter</asp:LinkButton>
					</li>
					<li>
						<asp:LinkButton ID="lnkClear" runat="server" ToolTip="Clear" CssClass="btn btn-danger" OnClick="lnkClear_Click">Clear Filters</asp:LinkButton>
					</li>
				</ul>
			</div>
			<ec:GridViewEx ID="gvRenewalExceptions" runat="server" AutoGenerateColumns="False"
				AllowPaging="True" AllowSorting="True" PageSize="25"
				DataKeyNames="AccountID" CurrentSortedColumn="RenewalDate" CurrentSortDirection="Ascending"
				CssClass="OTable" RowStyle-CssClass="Row" AlternatingRowStyle-CssClass="Alt"
				PagerSettings-Mode="NextPreviousFirstLast"
				OnSorting="gvRenewalExceptions_Sorting"
				OnPageIndexChanging="gvRenewalExceptions_PageIndexChanging"
				OnRowDataBound="gvRenewalExceptions_RowDataBound"
				EmptyDataText="There are no renewal exceptions that meet the criteria you specified."
				SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
				SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">

				<Columns>
					<asp:TemplateField HeaderText="Account #" SortExpression="AccountID">
						<ItemTemplate>
							<asp:HyperLink ID="hlAccountID" runat="server" ToolTip="Account Detail" />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="AccountName" HeaderText="Account Name" SortExpression="AccountName" />
					<asp:BoundField DataField="BillingMethod" HeaderText="Method" ReadOnly="True" SortExpression="BillingMethod" />
					<asp:BoundField DataField="RenewalDate" HeaderText="Renewal Date" SortExpression="RenewalDate" DataFormatString="{0:d}" />
					<asp:BoundField DataField="ContractEndDate" HeaderText="Contract Ends" SortExpression="ContractEndDate" DataFormatString="{0:d}" />
					<asp:BoundField DataField="CustomerType" HeaderText="Type" SortExpression="CustomerType" />
					<asp:BoundField DataField="LastBilled" HeaderText="Last Billed" SortExpression="LastBilled" DataFormatString="{0:d}" />
					<asp:TemplateField HeaderText="Exc Code" SortExpression="RenewalExceptionCode">
						<ItemTemplate>
							<asp:Literal ID="ltExceptionCode" runat="server" />
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>
				
			</ec:GridViewEx>

			<table class="OTable" style="width: 450px;">
				<tr>
					<th>Exception Codes</th>
				</tr>
				<tr>
					<td>&nbsp;&nbsp;NR - Account without a rate code</td>
				</tr>
				<tr>
					<td>&nbsp;&nbsp;ND - Account without a device</td>
				</tr>
				<tr>
					<td>&nbsp;&nbsp;RN - A rate cannot be found for the number of devices specified</td>
				</tr>
				<tr>
					<td>&nbsp;&nbsp;DR - 100% discount rate</td>
				</tr>
				<tr>
					<td>&nbsp;&nbsp;RP - $0.00 rate price</td>
				</tr>
			</table>

		</ContentTemplate>
	</asp:UpdatePanel>

</asp:Content>
