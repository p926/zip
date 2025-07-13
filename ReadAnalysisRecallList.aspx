<%@ Page Title="Read Analysis" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="ReadAnalysisRecallList.aspx.cs" Inherits="TechOps_ReadAnalysisRecallList" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <link rel="stylesheet" type="text/css" href="../css/rad-controls/RadGrid.css" />
    <style type="text/css">
        /* CSS for Progress Bar area. */
         .ui-progressbar {
            position: relative;
        }

        .progress-label {
            position: absolute;
            text-align: center;
            left: 50%;
            top: 4px;
            font-weight: bold;
            text-shadow: 1px 1px 0 #fff;
        }

        /*.ui-dialog-titlebar-close {
            visibility: hidden;
        }*/
    </style>
    <%--JQUERY HERE--%>
    <script type="text/javascript">
        // Declare [Global] Variables.
        var logID = 0;
        var workerStatus = '';
        var totalReads = 0;
        var readsCompleted = 0;
        var readErrors = 0;
        var messageDesc = '';

        var selectedTab;

        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
        });

        function JQueryControlsLoad() {
            $(".datePicker").datepicker();
        };

        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        };

        function closeDialog(id) {
            $('#' + id).dialog("close");
        };

        function pageLoad(sender, args) {
            
        };

    </script>
    <%--END--%>
    <%--RADSCRIPT/JAVASCRIPT that limits the type of FILTERS--%>
    <telerik:RadCodeBlock ID="RadCodeBlock0" runat="server">
        <script type="text/javascript">
            var column = null;
            function MenuShowing(sender, args) {
                if (column == null) return;
                var menu = sender; var items = menu.get_items();
                if (column.get_dataType() == "System.String") {
                    var i = 0;
                    while (i < items.get_count()) {
                        if (!(items.getItem(i).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'IsNull': '' })) {
                            var item = items.getItem(i);
                            if (item != null)
                                item.set_visible(false);
                        }
                        else {
                            var item = items.getItem(i);
                            if (item != null)
                                item.set_visible(true);
                        } i++;
                    }
                }
                if (column.get_dataType() == "System.DateTime") {
                    var h = 0; while (h < items.get_count()) {
                        if (!(items.getItem(h).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'GreaterThanOrEqualTo': '', 'LessThan': '', 'LessThanOrEqualTo': '', 'IsNull': '' })) {
                            var item = items.getItem(h); if (item != null)
                                item.set_visible(false);
                        }
                        else {
                            var item = items.getItem(h);
                            if (item != null)
                                item.set_visible(true);
                        } h++;
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
    <telerik:RadScriptManager ID="RadScriptManager1" runat="server" />
    <telerik:RadStyleSheetManager ID="RadStyleSheetManager1" runat="server"></telerik:RadStyleSheetManager>

    <%------------------------------------------ BEGIN MAIN PAGE CONTENT ------------------------------------------%>
    <div id="divMainContent">
        <asp:UpdatePanel ID="updtpnlMainContent" runat="server">
            <ContentTemplate>
                <div class="FormError" id="divErrorMessage" runat="server" visible="false" style="text-align: left;">
	                <p><span class="MessageIcon"></span>
	                <strong>Message:</strong>&nbsp;<span id="spnErrorMessage" runat="server">An error as occurred.</span></p>
                </div>
                <%--TAB CONTAINERS SECTION - HEADER--%>
                <div id="divTabsContainerMain">
                    <div id="tabsContainer" class="ui-tabs ui-widget ui-widget-content ui-corner-all">
                        <ul class="ui-tabs-nav ui-helper-reset ui-helper-clearfix ui-widget-header ui-corner-all">
                            <li class="ui-state-default ui-corner-top"><a href="ReadAnalysis.aspx" id="Tab1" tabindex="0">Perform Read Analysis</a></li>
                            <li class="ui-state-default ui-corner-top ui-tabs-selected ui-state-active"><a href="#RecallList_tab" id="Tab2" tabindex="1">Recall List</a></li>                            
                        </ul>
                        <%--RECALL LIST TAB--%>
                        <div id="RecallList_tab">
                            <asp:UpdatePanel ID="updtpnlRecallListTab" runat="server">
                                <Triggers>
                                    <asp:PostBackTrigger ControlID="lnkbtnExportToExcel_RecallList" />
                                </Triggers>
                                <ContentTemplate>
                                    <div class="OToolbar JoinTable">
                                        <ul>
                                            <li>
                                                <asp:LinkButton ID="lnkbtnExportToExcel_RecallList" runat="server" OnClick="lnkbtnExportToExcel_RecallList_Click" 
                                                Text="Export to Excel" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Export" ToolTip="Export to Excel" />
                                            </li>
                                            <li>
                                                <asp:LinkButton ID="lnkbtnRefreshData_RecallList" runat="server" OnClick="lnkbtnRefreshData_RecallList_Click" 
                                                Text="Refresh Data" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Refresh" ToolTip="Refresh Data" />
                                            </li>
                                            <li>
                                                <asp:LinkButton ID="lnkbtnClearFilters_RecallList" runat="server" OnClick="lnkbtnClearFilters_RecallList_Click" 
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                            </li>
                                        </ul>
                                    </div>
                                    <asp:Panel ID="pnlRecallList" runat="server" SkinID="Default">
                                        <telerik:RadGrid ID="rgRecallList" runat="server"
                                        CssClass="OTable"  
                                        AllowMultiRowSelection="false" 
                                        AutoGenerateColumns="false"
                                        AllowFilteringByColumn="true"
                                        EnableLinqExpressions="false"
                                        EnableViewState="false"
                                        OnNeedDataSource="rgRecallList_NeedDataSource"
                                        OnItemCommand="rgRecallList_ItemCommand" 
                                        OnItemDataBound="rgRecallList_ItemDataBound"  
                                        AllowPaging="true" AllowSorting="true" 
                                        Style="border: 1px solid #D6712D;"
                                        Width="99.8%" PageSize="20"
                                        GridLines="None" SkinID="Default">
                                            <PagerStyle Mode="NumericPages" />
                                            <GroupingSettings CaseSensitive="false" />
                                            <ClientSettings EnableRowHoverStyle="false">
                                                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                                <Selecting AllowRowSelect="false" />
                                                <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                            </ClientSettings>
                                            <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
                                            <MasterTableView DataKeyNames="SerialNumber" TableLayout="Fixed" AllowMultiColumnSorting="true">
                                                <SortExpressions>
                                                    <telerik:GridSortExpression FieldName="DaysSinceReviewed" SortOrder="Descending" />
                                                </SortExpressions>
                                                <Columns>
                                                    <telerik:GridTemplateColumn DataField="SerialNumber" UniqueName="SerialNumber" AllowFiltering="true" HeaderText="Serial #" 
                                                    SortExpression="SerialNumber" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains">
                                                        <ItemTemplate>
                                                            <asp:HyperLink ID="hyprlnkSerialNumber" runat="server" 
                                                            NavigateUrl='<%# string.Format("BadgeReview.aspx?SerialNo={0}&RID=0", Eval("SerialNumber")) %>' 
                                                            Target="_blank" Text='<%# Eval("SerialNumber")%>' ToolTip='<%# string.Format("Serial #{0}", Eval("SerialNumber")) %>'>
                                                            </asp:HyperLink>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn DataField="AccountID" UniqueName="AccountID" AllowFiltering="true" HeaderText="Account #"
                                                    SortExpression="AccountID" AutoPostBackOnFilter="true" CurrentFilterFunction="EqualTo">
                                                        <ItemTemplate>
                                                            <asp:HyperLink ID="hyprlnkAccountID" runat="server" 
                                                            NavigateUrl='<%# string.Format("../InformationFinder/Details/Account.aspx?ID={0}", Eval("AccountID")) %>' 
                                                            Target="_blank" Text='<%# Eval("AccountID")%>' ToolTip='<%# string.Format("{0}", Eval("AccountID")) %>'>
                                                            </asp:HyperLink>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn DataField="CompanyName" UniqueName="CompanyName" SortExpression="CompanyName" HeaderText="Company" AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" />
                                                    <telerik:GridTemplateColumn DataField="AssignedTo" UniqueName="AssignedTo" AllowFiltering="true" HeaderText="Assigned To"
                                                    SortExpression="AssignedTo" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains">
                                                        <ItemTemplate>
                                                            <asp:HyperLink ID="hyprlnkAssignedTo" runat="server" 
                                                            NavigateUrl='<%# string.Format("../InformationFinder/Details/UserMaintenance.aspx?AccountID={0}&UserID={1}", Eval("AccountID"), Eval("UserID")) %>' 
                                                            Target="_blank" Text='<%# Eval("AssignedTo")%>' ToolTip='<%# string.Format("{0}", Eval("AssignedTo")) %>'>
                                                            </asp:HyperLink> 
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <%--COLOR--%>
                                                    <telerik:GridBoundColumn DataField="Color" UniqueName="Color" HeaderText="Color" HeaderStyle-Width="110px" ItemStyle-Width="110px" EmptyDataText="N/A">
                                                        <FilterTemplate>
                                                            <telerik:RadComboBox ID="rcbColor" DataSourceID="SQLDSColors" DataTextField="Color"
                                                            DataValueField="Color" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("Color").CurrentFilterValue %>'
                                                            runat="server" OnClientSelectedIndexChanged="ColorsIndexChanged" Width="100px">
                                                                <Items>
                                                                    <telerik:RadComboBoxItem Text="-Select-" />
                                                                </Items>
                                                            </telerik:RadComboBox>
                                                            <telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
                                                                <script type="text/javascript">
                                                                    function ColorsIndexChanged(sender, args) {
                                                                        var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                        tableView.filter("Color", args.get_item().get_value(), "EqualTo");
                                                                    }
                                                                </script>
                                                            </telerik:RadScriptBlock>
                                                        </FilterTemplate>
                                                    </telerik:GridBoundColumn>
                                                    <%--END--%>
                                                    <telerik:GridDateTimeColumn DataField="MostRecentReviewedDate" UniqueName="MostRecentReviewedDate" HeaderText="Reviewed Date" DataFormatString="{0:d}" 
                                                    AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="135px" ItemStyle-Width="135px" FilterControlWidth="100px" 
                                                    SortExpression="MostRecentReviewedDate" CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" ItemStyle-Wrap="false" HeaderStyle-Wrap="false" />
                                                    <telerik:GridBoundColumn DataField="ReviewedBy" UniqueName="ReviewedBy" SortExpression="ReviewedBy" HeaderText="Reviewed By" AllowFiltering="true" AllowSorting="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" />
                                                    <telerik:GridBoundColumn DataField="DaysSinceReviewed" UniqueName="DaysSinceReviewed" SortExpression="DaysSinceReviewed" HeaderText="Duration" AllowFiltering="false" AllowSorting="false" />
                                                    <telerik:GridTemplateColumn DataField="SerialNumber" UniqueName="SerialNumber" HeaderText="" AllowFiltering="false">
                                                        <ItemTemplate>
                                                            <asp:HyperLink ID="hyprlnkRMA" runat="server" 
                                                            NavigateUrl='<%# string.Format("ReturnAddNewDeviceRMA.aspx?SerialNo={0}", Eval("SerialNumber")) %>' 
                                                            Target="_blank" Text="Create RMA" ToolTip='<%# string.Format("RMA: Badge #{0}", Eval("SerialNumber")) %>'>
                                                            </asp:HyperLink>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                </Columns>
                                            </MasterTableView>
                                        </telerik:RadGrid>
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>
                <%--END--%>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%---------------------------------- END MAIN PAGE CONTENT ------------------------------------------------------%>
    <%--SQL OBJECTS--%>
    <asp:SqlDataSource ID="SQLDSColors" runat="server"
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
    SelectCommand="SELECT DISTINCT [Color] FROM [Products] WHERE [Color] <> 'N/A' ORDER BY [Color] ASC"></asp:SqlDataSource>
    <%--END--%>    
</asp:Content>

