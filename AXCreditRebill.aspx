<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="AXCreditRebill.aspx.cs" Inherits="portal_instadose_com_v3.Finance.AXCreditRebill" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript" src="../../scripts/moment.js"></script>
    <script type="text/javascript" src="../../scripts/datatables/jquery.dataTables.min.js"></script>
    <script type="text/javascript" src="../../scripts/datatables/dataTables.jqueryui.js"></script>
    <script src="../../scripts/instadose.ax-credit-rebill.js" type="text/javascript"></script>

    <link href="../../css/datatables/css/jquery.dataTables_themeroller.css" rel="stylesheet" />

    <style>
        .LabelValue {
            width: 150px;
        }

        li.RightAlign {
            float: right;
            padding-right: 10px;
        }
        
        /* CSS Override for RadGrid_Default. */
        div.RadGrid_Default {
            border: 1px solid #D6712D;
            color: #333333;
        }

        div.RadGrid_Default th.rgHeader {
            background: url('../../css/dsd-default/images/o-toolbar.png') repeat-x #D6712D;
            background-color: #D6712D;
            border-bottom: 1px solid #D6712D;
            border-right: 1px solid #D6712D;
            font-family: Arial, sans-serif;
            font-weight: bold;
            color: #FFFFFF;
        }

        div.RadGrid_Default th.rgHeader a {
            font-style: normal;
            color: #FFFFFF;
        }

        div.RadGrid_Default th.rgHeader a:hover {
            text-decoration: underline;
        }

        div.RadGrid_Default .rgAltRow {
            background-color: #f9e4cb;
            color: #333333;
        }

        /* Fixes Background Color of Hover state for Alternating Rows. */
        div.RadGrid_Default tr.rgAltRow:hover,
        div.RadGrid_Default tr.rgAltRow:active {
            background-color: #C3C3C3 !important;
        }

        div.RadGrid_Default tr.rgSelectedRow {
            background-color: #808080 !important;
            color: #FFFFFF;
        }
    </style>

    <script type="text/javascript">
        $(document).ready(function () {
            ManageAXCreditRebill.acmCancelReasons = JSON.parse($("#hdnACMCancelReasons").val());
            ManageAXCreditRebill.userId = $("#hdnUserID").val();

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
        });

        function JQueryControlsLoad() {
            ManageAXCreditRebill.configure();
            ManageAXCreditRebill.searchButtonID = '#<%= lnkbtnDisplay.ClientID %>';
        }
    </script>

    <%--RADSCRIPT/JAVASCRIPT that limits the type of FILTERS--%>
    <telerik:RadCodeBlock ID="RadCodeBlock0" runat="server">
        <script type="text/javascript">
            var column = null;
            function MenuShowing(sender, args) {
                if (column === null) return;
                var menu = sender;
                var items = menu.get_items();
                if (column.get_dataType() === "System.String") {
                    var i = 0;
                    while (i < items.get_count()) {
                        if (!(items.getItem(i).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            item = items.getItem(i);
                            if (item !== null)
                                item.set_visible(false);
                        }
                        else {
                            item = items.getItem(i);
                            if (item !== null)
                                item.set_visible(true);
                        } i++;
                    }
                }
                if (column.get_dataType() === "System.Int32") {
                    var j = 0;
                    while (j < items.get_count()) {
                        if (!(items.getItem(j).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            item = items.getItem(j);
                            if (item !== null)
                                item.set_visible(false);
                        }
                        else {
                            item = items.getItem(j);
                            if (item !== null)
                                item.set_visible(true);
                        } j++;
                    }
                }
                if (column.get_dataType() === "System.DateTime") {
                    var h = 0;
                    while (h < items.get_count()) {
                        if (!(items.getItem(h).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            item = items.getItem(h);
                            if (item !== null)
                                item.set_visible(false);
                        }
                        else {
                            item = items.getItem(h);
                            if (item !== null)
                                item.set_visible(true);
                        } h++;
                    }
                }
                if (column.get_dataType() === "System.Boolean") {
                    var k = 0;
                    while (k < items.get_count()) {
                        if (!(items.getItem(k).get_value() in { 'NoFilter': '', 'EqualTo': '' })) {
                            item = items.getItem(k);
                            if (item !== null)
                                item.set_visible(false);
                        }
                        else {
                            item = items.getItem(k);
                            if (item !== null)
                                item.set_visible(true);
                        } k++;
                    }
                }

                column = null;
                menu.repaint();
            }

            function filterMenuShowing(sender, eventArgs) {
                column = eventArgs.get_column();
            }
        </script>
    </telerik:RadCodeBlock>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <telerik:RadScriptManager ID="RadScriptManager1" runat="server" AsyncPostBackTimeout="600" />
    <telerik:RadStyleSheetManager ID="RadStyleSheetManager1" runat="server"></telerik:RadStyleSheetManager>

    <%--RADAJAXLOADINGPANEL ANIMATION (Enclosed in a HTML Table to Center on MultiPage)--%>
    <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server" SkinID="Default" EnableSkinTransparency="true"
        Transparency="20" BackColor="#FFFFFF" ZIndex="100" BackgroundPosition="Center">
        <table style="width: 100%; height: 100%;">
            <tr style="height: 100%">
                <td style="width: 100%; vertical-align: central; text-align: center;">
                    <asp:Label ID="lblLoading" runat="server" ForeColor="Black" Text="Loading..." Font-Bold="true" Font-Size="Medium" />
                    <br />
                    <br />
                    <asp:Image ID="imgLoading" runat="server" Width="128px" Height="12px" ImageUrl="../../images/orangebarloader.gif" />
                </td>
            </tr>
        </table>
    </telerik:RadAjaxLoadingPanel>
    <%--END--%>

    <%--RAD AJAX MANAGER - Handles RAD CONTROLS/LOADING PANELS for Ajax Updating--%>
    <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server">  
        <AjaxSettings>
            <%--DEVICE READS TRACKING RADGRID--%>
            <telerik:AjaxSetting AjaxControlID="rgInvoices">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgInvoices" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="lnkbtnDisplay">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgInvoices" LoadingPanelID="RadAjaxLoadingPanel1" />
                    <telerik:AjaxUpdatedControl ControlID="pnErrorMessage" />
                </UpdatedControls> 
            </telerik:AjaxSetting>
            <%--END--%>
        </AjaxSettings> 
    </telerik:RadAjaxManager>
    <%--END--%>

    <asp:HiddenField ID="hdnACMCancelReasons" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hdnUserID" runat="server" ClientIDMode="Static" />

    <div id="axinvoice-info-dialog" style="display: none;">
        <div class="loading"></div>
        <div class="FormError" style="display: none;">
            <p>
                <span class="MessageIcon"></span><strong>Messages:</strong>
                <span class="Message"></span>
            </p>
        </div>

        <div class="OForm">
            <div class="Row">
                <div class="Label">Invoice #:</div>
                <div class="LabelValue"><label id="lblAXInvoiceInfoInvoiceNo" /></div>
                <div class="Label">Account:</div>
                <div class="LabelValue"><label id="lblAXInvoiceInfoAccount" /></div>
                <div class="Label">Invoice Date:</div>
                <div class="LabelValue"><label id="lblAXInvoiceInfoInvoiceDate" /></div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Invoice Amount:</div>
                <div class="LabelValue"><label id="lblAXInvoiceInfoAmount" /></div>
                <div class="Label">Due Date:</div>
                <div class="LabelValue"><label id="lblAXInvoiceInfoDueDate" /></div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="OToolbar JoinTable" style="padding: 5px 10px 12px 10px;">
                    
                </div>
                <table class="display" id="tblAXInvoiceInfoDetails">
                    <thead>
                        <tr class="header">
                            <th>Item ID</th>
                            <th>Item</th>
                            <th>Wear Period</th>
                            <th>Start Date</th>
                            <th>End Date</th>
                            <th>Location</th>
                            <th>Unit Price</th>
                            <th>Inv Qty</th>
                            <th>Ship Qty</th>
                        </tr>
                    </thead>
                </table>
            </div>
        </div>
    </div>

    <div id="axinvoice-details-dialog" style="display: none;">
        <div class="loading"></div>
        <div class="FormError" style="display: none;">
            <p>
                <span class="MessageIcon"></span><strong>Messages:</strong>
                <span class="Message"></span>
            </p>
        </div>

        <div class="OForm">
            <div class="Row">
                <div class="Label">Invoice #:</div>
                <div class="LabelValue"><label id="lblAXInvoiceNo" /></div>
                <div class="Label">Account:</div>
                <div class="LabelValue"><label id="lblAXInvoiceAccount" /></div>
                <div class="Label">Invoice Date:</div>
                <div class="LabelValue"><label id="lblAXInvoiceDate" /></div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Invoice Amount:</div>
                <div class="LabelValue"><label id="lblAXInvoiceAmount" /></div>
                <div class="Label">Due Date:</div>
                <div class="LabelValue"><label id="lblAXInvoiceDueDate" /></div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="OToolbar JoinTable" style="padding: 5px 10px 12px 10px;">
                    
                </div>
                <table class="display" id="tblAXInvoiceDetails">
                    <thead>
                        <tr class="header">
                            <th>Item ID</th>
                            <th>Item</th>
                            <th>Wear Period</th>
                            <th>Start Date</th>
                            <th>End Date</th>
                            <th>Location</th>
                            <th>Unit Price</th>
                            <th>New Price</th>
                            <th>Inv Qty</th>
                            <th>New Qty</th>
                            <th>Ship Qty</th>
                            <%-- By the updated rule, Shipment Qty cannot be updated. To revamp Ship Qty, uncomment below line. --%>
                            <%--<th>New Ship Qty</th>--%>
                            <th>Reason</th>
                        </tr>
                    </thead>
                </table>
            </div>
        </div>
    </div>

    <div id="axinvoice-rebill-confirm-dialog">
        <div class="loading"></div>
        <div class="FormError" style="display: none;">
            <p>
                <span class="MessageIcon"></span><strong>Messages:</strong>
                <span class="Message"></span>
            </p>
        </div>

        <div class="FormMessage"> 
	        <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="submitMsg" runat ="server" >The process may take several minutes. Please do not close the window.</span></p>
        </div>

        <div class="OForm">
            <div class="Row">
                <div class="Label">Invoice #:</div>
                <div class="LabelValue"><label id="lblAXInvoiceNoConfirm" /></div>
                <div class="Label">Account:</div>
                <div class="LabelValue"><label id="lblAXInvoiceAccountConfirm" /></div>
                <div class="Label">Invoice Date:</div>
                <div class="LabelValue"><label id="lblAXInvoiceDateConfirm" /></div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Invoice Amount:</div>
                <div class="LabelValue"><label id="lblAXInvoiceAmountConfirm" /></div>
                <div class="Label">Due Date:</div>
                <div class="LabelValue"><label id="lblAXInvoiceDueDateConfirm" /></div>
                <div class="Label">Type:</div>
                <div class="LabelValue"><label id="lblAXInvoiceCreditTypeConfirm" /></div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <table class="display" id="tblAXInvoiceConfirmDetails">
                    <thead>
                        <tr class="header">
                            <th>Item ID</th>
                            <th>Item</th>
                            <th>Wear Period</th>
                            <th>Start Date</th>
                            <th>End Date</th>
                            <th>Location</th>
                            <th>Price</th>
                            <th>Qty</th>
                            <th>Ship Qty</th>
                            <th>Reason</th>
                        </tr>
                    </thead>
                </table>
            </div>
        </div>
    </div>

    <div id="axinvoice-credit-confirm-dialog">
        <div class="loading"></div>
        <div class="FormError" style="display: none;">
            <p>
                <span class="MessageIcon"></span><strong>Messages:</strong>
                <span class="Message"></span>
            </p>
        </div>

        <div>
            <span id="spnAXInvoiceCreditConfirmMessage"></span>
        </div>
    </div>

    <asp:Panel CssClass="FormError" ID="pnErrorMessage" runat="server" visible="false">
		<p>
			<span class="MessageIcon"></span>
			<strong>Messages:</strong>
			<asp:Label ID="lblError" runat="server">An error was encountered.</asp:Label>
		</p>
	</asp:Panel>

    <div class="OToolbar JoinTable">
		<ul>
            <li>Account Type:</li>
            <li>
                <asp:DropDownList ID="ddlAccountType" runat="server">
                    <asp:ListItem Value="gds" Selected="True">GDS</asp:ListItem>
                    <asp:ListItem Value="portal">Portal</asp:ListItem>
                </asp:DropDownList>
            </li>
            <li>Account:</li>
            <li>
                <asp:TextBox ID="txtAccountID" runat="server" CssClass="Size Small" />
            </li>
            <li>PO Number:</li>
            <li>
                <asp:TextBox ID="txtPONumber" runat="server" CssClass="Size Small" />
            </li>
			<li>
				<asp:Button ID="lnkbtnDisplay" runat="server" ToolTip="Search" CssClass="btn btn-success" OnClick="lnkbtnDisplay_Click" Text="Search"></asp:Button>
			</li>
            <li class="RightAlign">
                <asp:LinkButton ID="lnkbtnClearFilters_Invoices" runat="server" OnClick="lnkbtnClearFilters_Invoices_Click"
                    Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
            </li>
            <li class="Clear"></li>
		</ul>
	</div>
	<telerik:RadGrid ID="rgInvoices" runat="server"
        CssClass="OTable"
        AutoGenerateColumns="false"
        AllowFilteringByColumn="true"
        AllowMultiRowSelection="false"
        EnableLinqExpressions="false"
        EnableViewState="true"
        OnNeedDataSource="rgInvoices_NeedDataSource"
        OnItemDataBound="rgInvoices_ItemDataBound"
        AllowPaging="true" AllowSorting="true"
        Width="99.9%" PageSize="25"
        GridLines="None" SkinID="Default">
        <PagerStyle Mode="NumericPages" />
        <GroupingSettings CaseSensitive="false" />
        <ClientSettings EnableRowHoverStyle="true">
            <Scrolling AllowScroll="false" UseStaticHeaders="true" />
            <Selecting AllowRowSelect="true" />
            <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
        </ClientSettings>
        <FilterMenu OnClientShown="MenuShowing" />
        <GroupingSettings CaseSensitive="false" />
        <MasterTableView DataKeyNames="INVOICEID" TableLayout="Fixed" AllowMultiColumnSorting="false">
            <Columns>
                <telerik:GridTemplateColumn AllowFiltering="false" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="75px">
                    <ItemTemplate>
                        <asp:HyperLink ID="hlCreditAXInvoice" runat="server" Visible="false">Credit</asp:HyperLink>
                        &nbsp;
                        <asp:HyperLink ID="hlRebillAXInvoice" runat="server">Rebill</asp:HyperLink>
                    </ItemTemplate>
                </telerik:GridTemplateColumn>
                <telerik:GridTemplateColumn HeaderText="Invoice #" SortExpression="INVOICEID" DataField="INVOICEID"
                    AllowFiltering="true" HeaderStyle-Width="110px" FilterControlWidth="100px" UniqueName="INVOICEID"
                    CurrentFilterFunction="Contains" AutoPostBackOnFilter="true">
                    <ItemTemplate>
                        <asp:HyperLink ID="hlAXInvoiceDetail" runat="server" />
                    </ItemTemplate>
                </telerik:GridTemplateColumn>
                <telerik:GridBoundColumn DataField="INVOICEACCOUNT" HeaderText="Account #" UniqueName="INVOICEACCOUNT" SortExpression="INVOICEACCOUNT" 
                    HeaderStyle-Width="110px" FilterControlWidth="100px" ItemStyle-Wrap="false"
                    CurrentFilterFunction="Contains" AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" />
                <telerik:GridBoundColumn DataField="INVOICINGNAME" HeaderText="Account" UniqueName="INVOICINGNAME" SortExpression="INVOICINGNAME" 
                    CurrentFilterFunction="Contains" FilterControlWidth="101%" AutoPostBackOnFilter="true"
                    AllowFiltering="true" AllowSorting="true" />
                <telerik:GridBoundColumn DataField="PURCHASEORDER" HeaderText="PO #" UniqueName="PURCHASEORDER" SortExpression="PURCHASEORDER" 
                    HeaderStyle-Width="110px" FilterControlWidth="100px"
                    AllowFiltering="true" AllowSorting="true" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" />
                <telerik:GridDateTimeColumn DataField="INVOICEDATE" HeaderText="Invoice Date" UniqueName="INVOICEDATE" SortExpression="INVOICEDATE" 
                    HeaderStyle-Width="125px" FilterControlWidth="118px"
                    AllowFiltering="true" AllowSorting="true" DataFormatString="{0:d}" AutoPostBackOnFilter="true" />
                <telerik:GridDateTimeColumn DataField="DUEDATE" HeaderText="Due Date" UniqueName="DUEDATE" SortExpression="DUEDATE" 
                    HeaderStyle-Width="125px" FilterControlWidth="118px"
                    AllowFiltering="true" AllowSorting="true" DataFormatString="{0:d}" AutoPostBackOnFilter="true" />
                <telerik:GridTemplateColumn HeaderText="Amount" SortExpression="INVOICEAMOUNT" DataField="INVOICEAMOUNT"
                    AllowFiltering="false" HeaderStyle-Width="110px" UniqueName="INVOICEAMOUNT">
                    <ItemTemplate>
                        <asp:Literal ID="ltInvoiceAmount" runat="server" />
                    </ItemTemplate>
                </telerik:GridTemplateColumn>
            </Columns>
        </MasterTableView>
        <ClientSettings AllowColumnsReorder="false">
            <Selecting AllowRowSelect="false"></Selecting>
        </ClientSettings>
    </telerik:RadGrid>
</asp:Content>
