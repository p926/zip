<%@ Page Title="Device Reads Tracking" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="DeviceCommInformation.aspx.cs" Inherits="IT_DeviceCommInformation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="../css/rad-controls/RadGrid.css" />
    <style type="text/css">
        /* CSS for Right Aligning "Clear Filter" link. */
        li.RightAlign
        {
            float: right;
            padding-right: 10px;
        }
    </style>
    <telerik:RadCodeBlock ID="RadCodeBlock0" runat="server">
        <script type="text/javascript">
            var column = null;
            function MenuShowing(sender, args) {
                if (column == null) return;
                var menu = sender; var items = menu.get_items();
                if (column.get_dataType() == "System.String") {
                    var i = 0;
                    while (i < items.get_count()) {
                        if (!(items.getItem(i).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
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
                if (column.get_dataType() == "System.Int32") {
                    var j = 0; while (j < items.get_count()) {
                        if (!(items.getItem(j).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            var item = items.getItem(j); if (item != null)
                                item.set_visible(false);
                        }
                        else { var item = items.getItem(j); if (item != null) item.set_visible(true); } j++;
                    }
                }
                if (column.get_dataType() == "System.DateTime") {
                    var h = 0;
                    while (h < items.get_count()) {
                        if (!(items.getItem(h).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            var item = items.getItem(h);
                            if (item != null)
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
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
    <telerik:RadScriptManager ID="RadScriptManager1" runat="server" />
    <telerik:RadStyleSheetManager ID="RadStyleSheetManager1" runat="server"></telerik:RadStyleSheetManager>

    <%--RADAJAXLOADINGPANEL ANIMATION (Enclosed in a HTML Table to Center on RadGrid)--%>
    <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server" SkinID="Default" EnableSkinTransparency="true"
    Transparency="20" BackColor="#FFFFFF" ZIndex="100" BackgroundPosition="Center">
        <table style="width:100%; height:100%;">
            <tr style="height:100%">
                <td style="width: 100%; vertical-align: central; text-align: center;">
                    <asp:Label ID="lblLoading1" runat="server" ForeColor="Black" Text="Loading..." Font-Bold="true" Font-Size="Medium" />
                    <br /><br />
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
            <telerik:AjaxSetting AjaxControlID="rgDeviceReadsTracking">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgDeviceReadsTracking" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="lnkbtnClearFilters">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgDeviceReadsTracking" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
            <%--END--%>
        </AjaxSettings> 
    </telerik:RadAjaxManager>
    <%--END--%>

    <%--MAIN PAGE CONTENT--%>
    <asp:Panel ID="pnlDeviceReadsTracking" runat="server">
        <div class="OToolbar JoinTable">
            <ul>
                <li>
                    <asp:LinkButton ID="lnkbtnExportToExcel" runat="server" CssClass="Icon Export" OnClick="lnkbtnExportToExcel_Click" ToolTip="Export to Excel">Export to Excel</asp:LinkButton>
                </li>
                <li class="RightAlign">
                    <asp:LinkButton ID="lnkbtnClearFilters" runat="server" OnClick="lnkbtnClearFilters_Click" CssClass="Icon Remove" ToolTip="Clear Filters">Clear Filters</asp:LinkButton>
                </li>
                <li class="Clear"></li>
            </ul>
        </div>
        <telerik:RadGrid ID="rgDeviceReadsTracking" runat="server" 
        CssClass="OTable"
        SkinID="Default"
        DataSourceID="sqlDeviceReadsTracking"
        AllowFilteringByColumn="true"
        AllowSorting="true"
        AllowPaging="true" PageSize="20"
        AutoGenerateColumns="false"
        EnableLinqExpressions="false" 
        OnItemCommand="rgDeviceReadsTracking_ItemCommand" 
        OnPreRender="rgDeviceReadsTracking_PreRender"
        OnHTMLExporting="rgDeviceReadsTracking_HTMLExporting" 
        OnExportCellFormatting="rgDeviceReadsTracking_ExportCellFormatting"
        Width="99.8%">
            <PagerStyle Mode="NumericPages" />
            <GroupingSettings CaseSensitive="false" />
            <ClientSettings EnableRowHoverStyle="true">
                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                <Selecting AllowRowSelect="true" />
                <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
            </ClientSettings>
            <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
            <MasterTableView DataKeyNames="DeviceCommID" TableLayout="Fixed">
                <Columns>
                    <telerik:GridBoundColumn DataField="DeviceCommID" HeaderText="ID" UniqueName="DeviceCommID" Visible="false" />
                    <telerik:GridBoundColumn DataField="ReaderName" HeaderText="Reader" UniqueName="ReaderName" SortExpression="ReaderName" 
                    AllowFiltering="false" AllowSorting="true" HeaderStyle-Width="125px" ItemStyle-Width="125px" />
                    <telerik:GridBoundColumn DataField="SerialNumber" HeaderText="Serial #" UniqueName="SerialNumber" SortExpression="SerialNumber"
                    AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="100px" ItemStyle-Width="100px" FilterControlWidth="65px"
                    CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" />
                    <telerik:GridBoundColumn DataField="CommunicatedOn" HeaderText="Communicated On" UniqueName="CommunicatedOn" DataFormatString="{0:G}"
                    AllowSorting="true" HeaderStyle-Width="275px" ItemStyle-Width="275px">
                        <FilterTemplate>
                            From:
                            <telerik:RadDatePicker ID="FromDatePicker" runat="server" ClientEvents-OnDateSelected="FromDateSelected" DbSelectedDate='<%# fromDate %>' Width="100px" />
                            To:
                            <telerik:RadDatePicker ID="ToDatePicker" runat="server" ClientEvents-OnDateSelected="ToDateSelected" DbSelectedDate='<%# toDate %>' Width="100px" />
                            <telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
                                <script type="text/javascript">
                                    function FromDateSelected(sender, args) {
                                        var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                        var ToPicker = $find('<%# ((GridItem)Container).FindControl("ToDatePicker").ClientID %>');
                                        var fromDate = FormatSelectedDate(sender);
                                        var toDate = FormatSelectedDate(ToPicker);
                                        tableView.filter("CommunicatedOn", fromDate + " " + toDate, "Between");
                                    }

                                    function ToDateSelected(sender, args) {
                                        var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                        var FromPicker = $find('<%# ((GridItem)Container).FindControl("FromDatePicker").ClientID %>');
                                        var fromDate = FormatSelectedDate(FromPicker);
                                        var toDate = FormatSelectedDate(sender);
                                        tableView.filter("CommunicatedOn", fromDate + " " + toDate, "Between");
                                    }

                                    function FormatSelectedDate(picker) {
                                        var date = picker.get_selectedDate();
                                        var dateInput = picker.get_dateInput();
                                        var formattedDate = dateInput.get_dateFormatInfo().FormatDate(date, dateInput.get_displayDateFormat());
                                        return formattedDate;
                                    }
                                </script>
                            </telerik:RadScriptBlock>
                        </FilterTemplate>
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DoseRecordsSent" HeaderText="Records Sent" UniqueName="DoseRecordsSent" 
                    SortExpression="DoseRecordsSent" AllowSorting="true" AllowFiltering="false" HeaderStyle-Width="100px" ItemStyle-Width="100px" />
                    <telerik:GridBoundColumn DataField="BatteryPercentage" HeaderText="Battery %" UniqueName="BatteryPercentage" EmptyDataText="*" 
                    SortExpression="BatteryPercentage" AllowSorting="true" AllowFiltering="false" HeaderStyle-Width="75px" ItemStyle-Width="75px" />
                    <telerik:GridBoundColumn DataField="CPUTemperature" HeaderText="CPU Temp." UniqueName="CPUTemperature" EmptyDataText="*" 
                    SortExpression="CPUTemperature" AllowSorting="true" AllowFiltering="false" HeaderStyle-Width="85px" ItemStyle-Width="85px" />
                    <telerik:GridBoundColumn DataField="AccountID" HeaderText="Account #" UniqueName="AccountID" FilterControlWidth="65px" 
                    SortExpression="AccountID" AllowSorting="true" AllowFiltering="true" HeaderStyle-Width="100px" ItemStyle-Width="100px"
                    CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" />
                    <telerik:GridBoundColumn DataField="CompanyName" HeaderText="Company" UniqueName="CompanyName" 
                    SortExpression="CompanyName" AllowSorting="true" AllowFiltering="false" HeaderStyle-Width="100px" ItemStyle-Width="100px" />
                    <telerik:GridBoundColumn DataField="FullName" HeaderText="Full Name" UniqueName="FullName" 
                    SortExpression="FullName" AllowSorting="true" AllowFiltering="false" HeaderStyle-Width="100px" ItemStyle-Width="100px" />
                </Columns>
            </MasterTableView>
        </telerik:RadGrid> 
    </asp:Panel>
    <%--END--%>
    <asp:SqlDataSource ID="sqlDeviceReadsTracking" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="SELECT * FROM vw_DeviceReadsTracking ORDER BY DeviceCommID DESC" 
    SelectCommandType="Text">
    </asp:SqlDataSource>
</asp:Content>
