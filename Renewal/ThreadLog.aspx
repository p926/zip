<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="ThreadLog.aspx.cs" Inherits="portal_instadose_com_v3.Finance.Renewal.ThreadLog" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<style type="text/css">
		.date-picker {
			width: 70px;
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

		var toggleReprocessButton = function (show) {
			var $button = $("#renewal-thread-details").next(".ui-dialog-buttonpane").find("button:contains('Re-Process')");

			if (show)
				$button.show();
			else
				$button.hide();
		}

		var openRenewalthreadDetailDialog = function (renewalNo, isReProcessable) {
			$("#renewal-thread-details").dialog({
				autoOpen: true,
				modal: true,
				title: "Renewal Detail",
				resizable: false,
				width: 950,
				open: function (type, data) {
					$("#renewalThreadDetailMessage").hide();
					$("#renewalThreadDetailError").hide();
					$("#renewalThreadDetailRenewalNo").text(renewalNo);

					toggleReprocessButton(isReProcessable);
				},
				buttons: {
					"Re-Process": function () {
						reprocessRenewal()
						.done(function (rtn) {
							if (rtn.Processed) {
								toggleReprocessButton(false);
								reprocessRenewalSuccess();
							} else {
								reprocessRenewalError(rtn.Message);
							}
						});
					},
					"Close": function () {
						$(this).dialog("close");
					}
				},
				close: function () {
					$(".ui-overlay").fadeOut();
				}
			});
		};

		var reprocessRenewal = function () {
			return $.ajax({
				url: "/Services/Renewal.asmx/ReprocessRenewalByThread",
				type: "POST",
				datatype: "json",
				contentType: "application/json",
				data: JSON.stringify({
					renewalThreadLogID: $("#hdnThreadLogDetailDialog_ThreadLogID").val(),
					userName: $("#hdnUserName").val()
				}),
				dataFilter: function (data) {
					var response;
					if (typeof (JSON) !== 'undefined' && typeof (JSON.parse) === 'function')
						response = JSON.parse(data);
					else
						response = eval('(' + data + ')');

					if (response.hasOwnProperty('d'))
						return response.d;
					else
						return response;
				}
			});
		};

		var reprocessRenewalError = function (errMsg) {
			$("#renewalThreadDetailMessage").hide();
			$("#renewalThreadDetailError").show();
			$("#renewalThreadDetailErrorMessage").html(errMsg);
		};

		var reprocessRenewalSuccess = function () {
			$("#renewalThreadDetailError").hide();
			$("#renewalThreadDetailMessage").show();
		};

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
	<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

	<asp:HiddenField ID="hdnUserName" runat="server" ClientIDMode="Static" />
			
	<div id="renewal-thread-details" class="hide">
		<div id="renewalThreadDetailError" class="FormError hide">
			<p>
				<span class="MessageIcon"></span>
				<strong>Messages:</strong>
				<label id="renewalThreadDetailErrorMessage"></label>
			</p>
		</div>
		<div id="renewalThreadDetailMessage" class="FormMessage hide">
			<p>
				<span class="MessageIcon"></span>
				<strong>Messages:</strong>
				<label id="renewalThreadDetailMessages">Reprocess is submitted.</label>
			</p>
			<div class="Clear"></div>
		</div>
		<div class="OForm">
			<div class="Row">
				<div class="Label" style="width:90px;">Renewal No. : </div>
				<div class="Control">
					<div class="LabelValue">
						<span id="renewalThreadDetailRenewalNo"></span>
					</div>
				</div>
				<div class="Clear"></div>
			</div>
		</div>
		<asp:UpdatePanel ID="upRenewalThreadDetails" runat="server">
			<Triggers>
				<asp:AsyncPostBackTrigger ControlID="gvThreadLogs" EventName="RowCommand" />
			</Triggers>
			<ContentTemplate>
				<div class="Row">
					<ec:GridViewEx ID="gvThreadLogDetails" runat="server" AutoGenerateColumns="False"
						AllowPaging="True" AllowSorting="false" PageSize="15"
						DataKeyNames="RenewalThreadLogDetailID"
						CssClass="OTable" RowStyle-CssClass="Row" AlternatingRowStyle-CssClass="Alt"
						PagerSettings-Mode="NextPreviousFirstLast"
						OnPageIndexChanging="gvThreadLogDetails_PageIndexChanging"
						OnRowDataBound="gvThreadLogDetails_RowDataBound"
						EmptyDataText="There are no renewal thread log detail that meet the criteria you specified."
						SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
						SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">

						<Columns>
							<asp:TemplateField HeaderText="">
								<ItemTemplate>
									<asp:Image ID="imgComplete" runat="server" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Order #">
								<ItemTemplate>
									<asp:Literal ID="ltOrderID" runat="server" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:BoundField DataField="AccountID" HeaderText="Account #" />
							<asp:TemplateField HeaderText="Account">
								<ItemTemplate>
									<asp:Literal ID="ltAccountName" runat="server" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Status">
								<ItemTemplate>
									<asp:Literal ID="ltOrderStatus" runat="server" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Brand">
								<ItemTemplate>
									<asp:Literal ID="ltBrandName" runat="server" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Term">
								<ItemTemplate>
									<asp:Literal ID="ltBillingTermDesc" runat="server" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Type">
								<ItemTemplate>
									<asp:Literal ID="ltPaymentType" runat="server" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Amount">
								<ItemTemplate>
									<asp:Literal ID="ltRenewalAmount" runat="server" />
								</ItemTemplate>
							</asp:TemplateField>
						</Columns>
					</ec:GridViewEx>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</div>

	<asp:UpdatePanel ID="upThreadLogs" runat="server">
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

			<asp:HiddenField ID="hdnThreadLogDetailDialog_ThreadLogID" runat="server" ClientIDMode="Static" />

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
					<li>Renewal No.:</li>
					<li>
						<asp:TextBox runat="server" ID="txtRenewalNumber" Style="width: 75px" />
					</li>
					<li>
						<asp:LinkButton ID="lnkbtnDisplay" runat="server" ToolTip="Search" CssClass="btn btn-success" OnClick="lnkbtnDisplay_Click">Apply Filter</asp:LinkButton>
					</li>
					<li>
						<asp:LinkButton ID="lnkClear" runat="server" ToolTip="Clear" CssClass="btn btn-danger" OnClick="lnkClear_Click">Clear Filters</asp:LinkButton>
					</li>
				</ul>
			</div>
			<ec:GridViewEx ID="gvThreadLogs" runat="server" AutoGenerateColumns="False"
				AllowPaging="True" AllowSorting="True" PageSize="25"
				DataKeyNames="RenewalThreadLogID" CurrentSortDirection="Ascending"
				CssClass="OTable" RowStyle-CssClass="Row" AlternatingRowStyle-CssClass="Alt"
				PagerSettings-Mode="NextPreviousFirstLast"
				OnSorting="gvThreadLogs_Sorting"
				OnPageIndexChanging="gvThreadLogs_PageIndexChanging"
				OnRowDataBound="gvThreadLogs_RowDataBound"
				OnRowCommand="gvThreadLogs_RowCommand"
				EmptyDataText="There are no renewal thread log that meet the criteria you specified."
				SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
				SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">

				<Columns>
					<asp:TemplateField HeaderText="" HeaderStyle-Width="25px" ItemStyle-HorizontalAlign="Center">
						<ItemTemplate>
							<asp:Image ID="imgComplete" runat="server" />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Renewal No." SortExpression="RenewalNo">
						<ItemTemplate>
							<asp:LinkButton ID="btnRenewalNo" runat="server" CommandName="OpenRenewalThreadLogDetail" />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="StartDate" HeaderText="Process Start" SortExpression="StartDate" DataFormatString="{0:M/d/yyyy hh:mm tt}" />
					<asp:TemplateField HeaderText="Total Accounts">
						<ItemTemplate>
							<asp:Literal ID="ltTotalCount" runat="server" />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Processed Accounts">
						<ItemTemplate>
							<asp:Literal ID="ltProcessedCount" runat="server" />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Waiting Accounts">
						<ItemTemplate>
							<asp:Literal ID="ltWaitingCount" runat="server" />
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>
						
			</ec:GridViewEx>

			<table class="OTable" style="width: 400px;">
				<tr>
					<th>Status</th>
				</tr>
				<tr>
					<td>
						<img src="/images/icons/accept.png" />
						&nbsp;&nbsp; Renewal has been successfully generated.
					</td>
				</tr>
				<tr>
					<td>
						<img src="/images/icons/hourglass.png" />
						&nbsp;&nbsp; Renewal is processing.
					</td>
				</tr>
				<tr>
					<td>
						<img src="/images/icons/bullet_error.png" />
						&nbsp;&nbsp; Error occured while processing.
					</td>
				</tr>
			</table>

		</ContentTemplate>
	</asp:UpdatePanel>

</asp:Content>
