<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="OrderQueue.aspx.cs" Inherits="portal_instadose_com_v3.TechOps.OrderQueue" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<style>
		.Size.XSmall {
			width:80px;
		}

        #tblBadgeSummary th,
        #tblBadgeSummary td {
            width: 17%;
        }

        #tblBadgeSummary th {
            text-align: center;
        }

        #tblBadgeSummary td {
            text-align: right;
        }
	</style>
	<script>

		$(document).ready(function () {
			Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(showProgress);
			Sys.WebForms.PageRequestManager.getInstance().add_endRequest(hideProgress);
			Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
			JQueryControlsLoad();

		});

		function showProgress(sender, args) {
			var c = '<%=modalExtender.ClientID %>';
			$find(c).show();
		}

		function hideProgress(sender, args) {
			var c = '<%=modalExtender.ClientID %>';
			$find(c).hide();
		}

		var convertRawDateToFormattedDate = function (rawDate) {
			var dt = new Date(parseInt(rawDate.substr(6), 10));
			return (dt.getMonth() + 1) + "/" + dt.getDate() + "/" + dt.getFullYear();
		};

		var ajaxDataFilter = function (data) {
			var response;
			if (typeof (JSON) !== 'undefined' && typeof (JSON.parse) === 'function')
				response = JSON.parse(data);
			else
				response = eval('(' + data + ')');

			if (response.hasOwnProperty('d'))
				return response.d;
			else
				return response;
		};

		var orderAjaxHelper = function (methodName, data) {
			return $.ajax({
				url: "/Services/Order.asmx/" + methodName,
				type: "POST",
				datatype: "json",
				contentType: "application/json",
				data: JSON.stringify(data),
				dataFilter: function (data) {
					return ajaxDataFilter(data);
				}
			});
		};

		var getInstaOrderDetail = function (instaOrderID) {
			return orderAjaxHelper("GetInstaOrderDetails", { instaOrderID: instaOrderID });
		};

		var releaseInstaOrder = function (instaOrderID) {
			return orderAjaxHelper("UpdateInstadoseOrderStatus", { instaOrderID: instaOrderID, orderStatus: "R" });
		};

		function JQueryControlsLoad() {
			$(".date-picker").datepicker();

			$("#dialogInstaOrderDetail").dialog({
				autoOpen: false,
				modal: true,
				closeOnEscape: false,
				resizable: false,
				width: 750,
				title: "Order Detail",
				open: function (type, data) {
				},
				buttons: {
					"Close": function () {
						$(this).dialog("close");
					}
				},
				close: function () {
					$(".ui-overlay").fadeOut();
				}
            });

            $("#lnkExportToExcel").off("click").on("click", function (e) {
                e.preventDefault();

                $("#btnExportToExcel").trigger("click");
            })
		}

		var loadInstaOrderDetailInfos = function (order) {
			var $instaOrderID = $("#instaOrderDetailOrderID");
			var $plOrderID = $("#instaOrderDetailPLOrderID");
			var $orderDate = $("#instaOrderDetailOrderDate");
			var $account = $("#instaOrderDetailAccount");
			var $location = $("#instaOrderDetailLocation");
			var $orderType = $("#instaOrderDetailOrderType");
			var $status = $("#instaOrderDetailStatus");
			var $mRC = $("#instaOrderDetailMRC");
			var $wearDate = $("#instaOrderDetailWearDate");

			if (order) {
				$instaOrderID.text(order.InstaOrderID);
				$plOrderID.text(order.PLOrderID);
				$orderDate.text(convertRawDateToFormattedDate(order.OrderDate));
				$account.text(order.GDSAccount);
				$location.text(order.GDSLocation);
				$orderType.text(order.OrderTypeName);
				$status.text(order.StatusName);
				$mRC.text(order.MfgRunCode);
				$wearDate.text(convertRawDateToFormattedDate(order.WearDate));
			} else {
				$instaOrderID.empty();
				$plOrderID.empty();
				$orderDate.empty();
				$account.empty();
				$location.empty();
				$orderType.empty();
				$status.empty();
				$mRC.empty();
				$wearDate.empty();
			}
		};

		var loadInstaOrderDetailBadges = function (badges) {
			var $tblBody = $("#tblInstaOrderDetailBadges tbody");
			$tblBody.empty();

			if (badges && badges.length > 0) {
				badges.forEach(function (item, idx) {
					var row = $("<tr>");

					if (idx % 2 == 1) {
						row.addClass("Alt");
					}

					var wearer = "";
					if (item.GDSWearer && item.GDSWearer != "" && item.GDSWearer != "0") {
						wearer += "(" + item.GDSWearer + ")";

						if (item.FirstName && item.FirstName != "") {
							wearer += " " + item.FirstName;
						}

						if (item.LastName && item.LastName != "") {
							wearer += " " + item.LastName;
						}
					}

					row.append($("<td>").text(item.BadgeType + "-" + item.BadgeDesc))
					.append($("<td>").text(item.BodyRegion ? item.BodyRegion : ""))
					.append($("<td>").text(item.Color ? item.Color : ""))
					.append($("<td>").text(wearer))
					.append($("<td>").text(item.SerialNo ? item.SerialNo : ""));

					$tblBody.append(row);
				});
			} else {
				$tblBody.append($("<tr>").append($("<td>").attr("colspan", 5)));
			}
		};

		var openInstaOrderDetailDialog = function (instaOrderID) {
			getInstaOrderDetail(instaOrderID)
			.done(function (rtn) {
				loadInstaOrderDetailInfos(rtn.InstaOrder);
				loadInstaOrderDetailBadges(rtn.InstaOrderBadges);

				$("#dialogInstaOrderDetail").dialog("open");
			})
			.fail(function (jqXHR, status) {
				alert("Error occured while getting order data.");
			});
		};

		var releaseInstadoseOrder = function (instaOrderID) {
			if (window.confirm("Instadose Order (Temp #: " + instaOrderID + ") will be released.")) {
				showProgress();

				releaseInstaOrder(instaOrderID)
				.done(function (rtn) {
					hideProgress();
					alert("Order will be released soon.");

					$("#btnOrderSearch").trigger("click");
				})
				.fail(function (jqXHR, status) {
					hideProgress();

					alert("Error occured while releasing the order.");
				});
			}
		};

	</script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
	<act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>

    <%--Order Detail Dialog--%>
	<div id="dialogInstaOrderDetail">
		<div class="OForm">
			<div class="Row Clear">
				<div class="Left">
					<div class="Label Small">Temp #:</div>
					<div class="LabelValue Small" id="instaOrderDetailOrderID"></div>
				</div>
				<div class="Left">
					<div class="Label Small">Order #:</div>
					<div class="LabelValue Small" id="instaOrderDetailPLOrderID"></div>
				</div>
				<div class="Left">
					<div class="Label Small">Order Date:</div>
					<div class="LabelValue Small" id="instaOrderDetailOrderDate"></div>
				</div>
			</div>
			<div class="Row Clear">
				<div class="Left">
					<div class="Label Small">Account:</div>
					<div class="LabelValue Small" id="instaOrderDetailAccount"></div>
				</div>
				<div class="Left">
					<div class="Label Small">Location:</div>
					<div class="LabelValue Small" id="instaOrderDetailLocation"></div>
				</div>
				<div class="Left">
					<div class="Label Small">Order Type:</div>
					<div class="LabelValue Small" id="instaOrderDetailOrderType"></div>
				</div>
			</div>
			<div class="Row Clear">
				<div class="Left">
					<div class="Label Small">Status:</div>
					<div class="LabelValue Small" id="instaOrderDetailStatus"></div>
				</div>
				<div class="Left">
					<div class="Label Small">MfgRunCode:</div>
					<div class="LabelValue Small" id="instaOrderDetailMRC"></div>
				</div>
				<div class="Left">
					<div class="Label Small">Wear Date:</div>
					<div class="LabelValue Small" id="instaOrderDetailWearDate"></div>
				</div>
			</div>
		
			<div class="Row Clear" style="padding-top: 10px; overflow-y: auto; max-height: 400px;">
				<table id="tblInstaOrderDetailBadges" class="OTable">
					<thead>
						<tr>
							<th>Badge Type</th>
							<th>Region</th>
							<th>Color</th>
							<th>Wearer</th>
							<th>Serial #</th>
						</tr>
					</thead>
					<tbody></tbody>
				</table>
			</div>
		</div>
	</div>
    <%--End Order Detail Dialog--%>

	<asp:UpdatePanel ID="udpnInstaOrders" runat="server">
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

            <%--Search Bar--%>
			<asp:Panel ID="pnOrderSearch" runat="server" CssClass="OToolbar" DefaultButton="btnOrderSearch">
				<ul>
                    <li>
                        <label>Order #:</label>
                        <asp:TextBox ID="txtOrderFilterOrderNumber" runat="server" ClientIDMode="Static" CssClass="Size XSmall"></asp:TextBox>
                    </li>
					<li>
						<label>Account:</label>
						<asp:TextBox ID="txtOrderFilterAccount" runat="server" ClientIDMode="Static" CssClass="Size XSmall"></asp:TextBox>
					</li>
					<li>
						<label>Location:</label>
						<asp:TextBox ID="txtOrderFilterLocation" runat="server" ClientIDMode="Static" CssClass="Size XSmall"></asp:TextBox>
					</li>
					<li>
						<label>Status:</label>
						<asp:DropDownList ID="ddlOrderFilterStatus" runat="server" ClientIDMode="Static">
						</asp:DropDownList>
					</li>
					<li>
						<label>Type:</label>
						<asp:DropDownList ID="ddlOrderFilterType" runat="server" ClientIDMode="Static">
							<asp:ListItem Value="" Text="ALL" />
							<asp:ListItem Value="4" Text="New" />
							<asp:ListItem Value="3" Text="Addon" />
							<asp:ListItem Value="1" Text="Recall" />
							<asp:ListItem Value="2" Text="Lost" />
						</asp:DropDownList>
					</li>
					<li>
						<label>Order Date:</label>
						<asp:TextBox ID="txtOrderFilterDateRangeFrom" runat="server" ClientIDMode="Static" CssClass="date-picker Size XSmall"></asp:TextBox>
							- 
						<asp:TextBox ID="txtOrderFilterDateRangeTo" runat="server" ClientIDMode="Static" CssClass="date-picker Size XSmall"></asp:TextBox>
					</li>
                    <li>
						<asp:Button ID="btnOrderSearch" runat="server" Text="Search" CssClass="btn btn-small btn-success" ClientIDMode="Static" OnClick="btnOrderSearch_Click" />
					</li>
				</ul>
            </asp:Panel>
			<%--End Search Bar--%>

            <%--Instadose Order Grid--%>
            <div class="OToolbar JoinTable">
                <ul>
                    <li>
                        <a href="#" id="lnkExportToExcel" class="Icon Export" title="Export to Excel">Export to Excel</a>
                    </li>
                </ul>
            </div>
			<ec:GridViewEx ID="gvInstaOrders" runat="server" AutoGenerateColumns="False"
				AllowPaging="True" AllowSorting="True" PageSize="20"
				DataKeyNames="InstaOrderID"
				CssClass="OTable" RowStyle-CssClass="Row" AlternatingRowStyle-CssClass="Alt"
				PagerSettings-Mode="NumericFirstLast"
				OnSorting="gvInstaOrders_Sorting"
				OnPageIndexChanging="gvInstaOrders_PageIndexChanging"
				OnRowDataBound="gvInstaOrders_RowDataBound"
				EmptyDataText="There are no orders that meet the criteria you specified."
				SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
				SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">

				<Columns>
					<asp:TemplateField HeaderText="Temp #" SortExpression="InstaOrderID" HeaderStyle-Width="60px">
						<ItemTemplate>
							<asp:HyperLink ID="hlInstaOrderID" runat="server" />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="PLOrderID" HeaderText="Order #" SortExpression="PLOrderID" HeaderStyle-Width="60px" />
					<asp:BoundField DataField="CompanyName" HeaderText="Company" SortExpression="CompanyName" />
					<asp:BoundField DataField="GDSAccount" HeaderText="Account" SortExpression="GDSAccount" />
					<asp:BoundField DataField="CreditHold" HeaderText="CH" SortExpression="CreditHold" />
					<asp:BoundField DataField="GDSLocation" HeaderText="Location" SortExpression="GDSLocation" />
					<asp:BoundField DataField="OrderTypeName" HeaderText="Type" SortExpression="OrderTypeName" />
					<asp:BoundField DataField="WearDate" HeaderText="WearDate" DataFormatString="{0:d}" SortExpression="WearDate" HeaderStyle-Width="70px" />
                    <asp:BoundField DataField="InstadosePlusCount" HeaderText="ID+" SortExpression="InstadosePlusCount" HeaderStyle-Width="30px" />
                    <asp:BoundField DataField="Instadose2Count" HeaderText="ID2" SortExpression="Instadose2Count" HeaderStyle-Width="30px" />
					<asp:BoundField DataField="Instadose3" HeaderText="ID Vue" SortExpression="Instadose3" HeaderStyle-Width="30px" />
					<asp:BoundField DataField="InstadoseVueBeta" HeaderText="Vue Beta" SortExpression="InstadoseVueBeta" HeaderStyle-Width="30px" />
					<asp:BoundField DataField="InstaLinkUSB" HeaderText="USB" SortExpression="InstaLinkUSB" HeaderStyle-Width="30px" />
					<asp:BoundField DataField="InstaLink" HeaderText="IL" SortExpression="InstaLink" HeaderStyle-Width="30px" />
					<asp:BoundField DataField="InstaLink3" HeaderText="IL3" SortExpression="InstaLink3" HeaderStyle-Width="30px" />
					<asp:BoundField DataField="OrderStatusName" HeaderText="Status" SortExpression="OrderStatusName" HeaderStyle-Width="75px" />
					<asp:BoundField DataField="OrderDate" HeaderText="Order Dt" HeaderStyle-Width="70px" DataFormatString="{0:d}" SortExpression="OrderDate" />
					<%--<asp:BoundField DataField="ShipDate" HeaderText="Ship Dt" HeaderStyle-Width="75px" DataFormatString="{0:d}" />--%>
					<asp:TemplateField HeaderText="Ship" HeaderStyle-Width="45px" SortExpression="ShippingMethodID">
						<ItemTemplate>
							<asp:Literal ID="ltShippingMethod" runat="server" />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Exp" HeaderStyle-Width="30px" SortExpression="ExpressShipment">
						<ItemTemplate>
							<asp:Image ID="imgExpress" runat="server" ImageUrl="/images/icons/accept.png" />
						</ItemTemplate>
					</asp:TemplateField>
					<%--<asp:TemplateField HeaderStyle-Width="30px" HeaderText="Rel">
						<ItemTemplate>
							<asp:HyperLink ID="hlReleaseOrder" runat="server" ImageUrl="/images/icons/accept.png" ToolTip="Release the order" />
						</ItemTemplate>
					</asp:TemplateField>--%>
				</Columns>
			</ec:GridViewEx>
            <%--End Instadose Order Grid--%>

            <%--Badge Summary--%>
            <div class="OToolbar JoinTable" style="width: 458px;">
                <span>Badge Summary By Search</span>
            </div>
            <asp:Repeater ID="rptBadgeSummaries" runat="server">
                <HeaderTemplate>
                    <table id="tblBadgeSummary" class="OTable" style="width: 460px;">
                        <thead>
					        <tr>
						        <th style="width: 32%;">Status</th>
						        <th>ID</th>
                                <th>ID+</th>
                                <th>ID2</th>
								<th>IDVue</th>
								<th>IDVueBeta</th>
                                <th>USB</th>
                                <th>InstaLink</th>
                                <th>InstaLink3</th>
					        </tr>
				        </thead>
                        <tbody>
                </HeaderTemplate>

                <ItemTemplate>
                    <tr>
                        <td style="text-align: left;"><asp:Literal ID="ltStatusName" runat="server" Text='<%# Eval("OrderStatusName") %>' /></td>
                        <td><asp:Literal ID="ltIDCount" runat="server" Text='<%# Eval("IDCount") %>' /></td>
                        <td><asp:Literal ID="ltIDPlusCount" runat="server" Text='<%# Eval("IDPlusCount") %>' /></td>
                        <td><asp:Literal ID="ltID2Count" runat="server" Text='<%# Eval("ID2Count") %>' /></td>
						<td><asp:Literal ID="ltIDVueCount" runat="server" Text='<%# Eval("IDVueCount") %>' /></td>
						<td><asp:Literal ID="ltIDVueBetaCount" runat="server" Text='<%# Eval("IDVueBetaCount") %>' /></td>
                        <td><asp:Literal ID="ltInstaUSBCount" runat="server" Text='<%# Eval("InstaLinkUSBCount") %>' /></td>
                        <td><asp:Literal ID="ltInstaLinkCount" runat="server" Text='<%# Eval("InstaLinkCount") %>' /></td>
                        <td><asp:Literal ID="ltInstaLink3" runat="server" Text='<%# Eval("InstaLink3") %>' /></td>
                    </tr>
                </ItemTemplate>

                <AlternatingItemTemplate>
                    <tr class="Alt">
                        <td style="text-align: left;"><asp:Literal ID="ltStatusName" runat="server" Text='<%# Eval("OrderStatusName") %>' /></td>
                        <td><asp:Literal ID="ltIDCount" runat="server" Text='<%# Eval("IDCount") %>' /></td>
                        <td><asp:Literal ID="ltIDPlusCount" runat="server" Text='<%# Eval("IDPlusCount") %>' /></td>
                        <td><asp:Literal ID="ltID2Count" runat="server" Text='<%# Eval("ID2Count") %>' /></td>
						<td><asp:Literal ID="ltIDVueCount" runat="server" Text='<%# Eval("IDVueCount") %>' /></td>
						<td><asp:Literal ID="ltIDVueBetaCount" runat="server" Text='<%# Eval("IDVueBetaCount") %>' /></td>
                        <td><asp:Literal ID="ltInstaUSBCount" runat="server" Text='<%# Eval("InstaLinkUSBCount") %>' /></td>
                        <td><asp:Literal ID="ltInstaLinkCount" runat="server" Text='<%# Eval("InstaLinkCount") %>' /></td>
                        <td><asp:Literal ID="ltInstaLink3" runat="server" Text='<%# Eval("InstaLink3") %>' /></td>
                    </tr>
                </AlternatingItemTemplate>

                <FooterTemplate>
                        </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
            <%--End Badge Summary--%>

		</ContentTemplate>
	</asp:UpdatePanel>

    <asp:Button ID="btnExportToExcel" runat="server" ClientIDMode="Static" OnClick="btnExportToExcel_Click" style="display: none;" Text="Export" />

</asp:Content>
