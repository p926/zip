<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="RenewalProcess.aspx.cs" Inherits="portal_instadose_com_v3.Finance.Renewal.RenewalProcess" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>
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
			$("#btnGenerate").prop("disabled", true);

			// Set the date picker objects
			$(".date-picker").datepicker();
			// Select all items in the gridview
			//$(".select-all input").change(function () {
			//	// Select or unselect all checkbox items in the closest table where they are not disabled.
			//	$(this).closest("table").find('td :checkbox:not(:disabled)').prop("checked", $(this).is(":checked"));
			//});

			$("#confirm-dialog").dialog({
				autoOpen: false,
				modal: true,
				title: "Renewal Process Confirmation",
				resizable: false,
				width: 350,
				open: function (type, data) {

				},
				buttons: {
					"Confirm": function () {
						$("#confirm-dialog").dialog("close");
						$("#btnGenerate").prop("disabled", false).trigger("click");
					},
					"Cancel": function () {
						$(this).dialog("close");
					}
				},
				close: function () {
					$(".ui-overlay").fadeOut();
				}
			});

			$("#btnGenerateDummy").off();
			$("#btnGenerateDummy").on("click", function () {
				openRenewalProcessConfirmation();
			});
		}

		var openRenewalProcessConfirmation = function () {
			$("#confirm-heading").html("Renewal Process");
			$("#confirm-message").html("Renewal process will be submitted.");
			$("#confirm-dialog").dialog("open");
		};
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
	<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

	<div id="confirm-dialog">
		<div id="confirm-heading" style="font-weight:bold; font-size:12pt;"></div>
		<br />
		<div id="confirm-message"></div>
	</div>

	<asp:UpdatePanel ID="upRenewalProcess" runat="server">
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

			<asp:Panel CssClass="FormError" ID="pnErrorMessage" runat="server" visible="false">
				<p>
					<span class="MessageIcon"></span>
					<strong>Messages:</strong>
					<asp:Label ID="lblError" runat="server">An error was encountered.</asp:Label>
				</p>
			</asp:Panel>

			<asp:Panel runat="server" ID="pnSuccessMessages" class="FormMessage" Visible="false">
				<p>
					<span class="MessageIcon"></span>
					<strong>Success:</strong>
					<asp:Label ID="lblSuccess" runat="server"></asp:Label>
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
						<asp:LinkButton ID="lnkbtnDisplay" runat="server" ToolTip="Search" CssClass="btn btn-success" OnClick="lnkbtnDisplay_Click">Apply Filter</asp:LinkButton>
					</li>
					<li>
						<asp:LinkButton ID="lnkClear" runat="server" ToolTip="Clear" CssClass="btn btn-danger" OnClick="lnkClear_Click">Clear Filters</asp:LinkButton>
					</li>
				</ul>
			</div>
			<ec:GridViewEx ID="gvUpcomingRenewals" runat="server" AutoGenerateColumns="False"
				AllowPaging="True" AllowSorting="True" PageSize="25"
				DataKeyNames="AccountID" CurrentSortedColumn="RenewalDate" CurrentSortDirection="Ascending"
				CssClass="OTable" RowStyle-CssClass="Row" AlternatingRowStyle-CssClass="Alt" SelectedRowStyle-CssClass="Row Selected"
				PagerSettings-Mode="NextPreviousFirstLast"
				OnSorting="gvUpcomingRenewals_Sorting" 
				OnPageIndexChanging="gvUpcomingRenewals_PageIndexChanging"
				OnRowDataBound="gvUpcomingRenewals_RowDataBound"
				EmptyDataText="There are no renewals that meet the criteria you specified."
				SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
				SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">

				<Columns>
					<asp:TemplateField HeaderText="" ItemStyle-HorizontalAlign="Center">
						<ItemTemplate>
							<asp:HiddenField runat="server" ID="hfAccountID" Value='<%# Eval("AccountID") %>' />
							<asp:CheckBox runat="server" ID="chkAccountID" />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="AccountID" HeaderText="Account#" ReadOnly="True" SortExpression="AccountID" />
					<asp:BoundField DataField="AccountName" HeaderText="Account Name" SortExpression="AccountName" />
					<asp:BoundField DataField="BillingMethod" HeaderText="Method" ReadOnly="True" SortExpression="BillingMethod" />
					<asp:BoundField DataField="RenewalDate" HeaderText="Renewal Date" SortExpression="RenewalDate" DataFormatString="{0:d}" />
					<asp:TemplateField HeaderText="Contract Ends" SortExpression="ContractEndDate">
						<ItemTemplate>
							<asp:Literal ID="ltContractEndDate" runat="server" />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="CustomerType" HeaderText="Type" SortExpression="CustomerType" />
					<asp:BoundField DataField="LastBilled" HeaderText="Last Billed" SortExpression="LastBilled" DataFormatString="{0:d}" />
				</Columns>
			</ec:GridViewEx>

			<div style="text-align: right; padding: 10px;">
				<input type="button" id="btnGenerateDummy" class="OButton" value="Generate Renewals" />
				<asp:Button ID="btnGenerate" runat="server" ClientIDMode="Static" Text="Generate Renewals" OnClick="btnGenerate_Click" CssClass="OButton hide" />
			</div>
		</ContentTemplate>
	</asp:UpdatePanel>

</asp:Content>
