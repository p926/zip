<%@ Page Title="Read Analysis Summary Reports" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="ReadAnalysisSummaryReports.aspx.cs" Inherits="ReadAnalysisSummaryReports_TechOps" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="../css/rad-controls/RadGrid.css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
    <telerik:RadScriptManager ID="RadScriptManager1" runat="server" />
    <telerik:RadStyleSheetManager ID="RadStyleSheetManager1" runat="server"></telerik:RadStyleSheetManager>
    <%--MAIN PAGE CONTENT--%>
    <div id="divMainPageContent">
        <div style="width: 100%;">
            <asp:Panel ID="pnlMainPageContent" runat="server" SkinID="Default">
                <div class="FormError" id="divErrorMessage_MainPageContent" runat="server" visible="false" style="text-align: left;">
	                <p><span class="MessageIcon"></span>
	                <strong>Message:</strong>&nbsp;<span id="spnErrorMessage_MainPageContent" runat="server">An error as occurred.</span></p>
                </div>
                <div class="OToolbar JoinTable">
                    <ul>
                        <li>
                            <asp:LinkButton ID="lnkbtnExportToExcel" runat="server" CssClass="Icon Export" OnClick="lnkbtnExportToExcel_Click" ToolTip="Export to Excel">Export to Excel</asp:LinkButton>
                        </li>
                        <%--<li>
                            <asp:LinkButton ID="lnkbtnGenerateCustomSummaryReport" runat="server" CssClass="Icon Report" OnClick="lnkbtnGenerateCustomSummaryReport_Click">Generate Custom Summary Report</asp:LinkButton>
                        </li>--%>
                    </ul>
                </div>
                <telerik:RadGrid ID="rgExportToExcel" runat="server" 
                CssClass="OTable"
                SkinID="Default"
                AllowMultiRowSelection="false"
                AutoGenerateColumns="false"
                AllowPaging="true"
                AllowSorting="false"
                AllowFilteringByColumn="false"
                ShowStatusBar="false"
                EnableLinqExpressions="false" 
                OnNeedDataSource="rgExportToExcel_NeedDataSource"
                OnItemCommand="rgExportToExcel_ItemCommand" 
                OnItemDataBound="rgExportToExcel_ItemDataBound"
                OnHTMLExporting="rgExportToExcel_HTMLExporting" 
                OnExportCellFormatting="rgExportToExcel_ExportCellFormatting"
                PageSize="20" Width="99.9%">
                    <PagerStyle Mode="NumericPages" />
                    <GroupingSettings CaseSensitive="false" />
                    <ClientSettings EnableRowHoverStyle="true">
                        <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                        <Selecting AllowRowSelect="true" />
                        <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                    </ClientSettings>
                    <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
                    <MasterTableView DataKeyNames="ReadAnalysisLogDetailID" TableLayout="Fixed" AllowMultiColumnSorting="false" AutoGenerateColumns="false">
                        <Columns>
                            <telerik:GridNumericColumn DataField="ReadAnalysisLogDetailID" UniqueName="ReadAnalysisLogDetailID" HeaderText="Log ID" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridNumericColumn DataField="RID" UniqueName="RID" HeaderText="RID" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridBoundColumn DataField="SerialNumber" UniqueName="SerialNumber" HeaderText="Serial #" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridNumericColumn DataField="AccountID" UniqueName="AccountID" HeaderText="Account #" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridBoundColumn DataField="CompanyName" UniqueName="CompanyName" HeaderText="Company Name" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridNumericColumn DataField="LocationID" UniqueName="LocationID" HeaderText="Location #" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridBoundColumn DataField="LocationName" UniqueName="LocationName" HeaderText="Location Name" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridNumericColumn DataField="UserID" UniqueName="UserID" HeaderText="User #" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridBoundColumn DataField="AssignedTo" UniqueName="AssignedTo" HeaderText="Asigned To" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridDateTimeColumn DataField="CreatedDate" UniqueName="CreatedDate" HeaderText="Exposure Date" AllowFiltering="false" AllowSorting="false" DataFormatString="{0:MM/dd/yyyy hh:mm:ss tt}" /> 
                            <telerik:GridBoundColumn DataField="ReadTypeName" UniqueName="ReadTypeName" HeaderText="Read Type" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridBoundColumn DataField="InitialRead" UniqueName="InitialRead" HeaderText="Initial Read?" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridBoundColumn DataField="HasAnomaly" UniqueName="HasAnomaly" HeaderText="Has Anomaly?" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridNumericColumn DataField="DLDCalculation" UniqueName="DLDCalculation" HeaderText="DLD Calc." AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridNumericColumn DataField="CumulativeDose" UniqueName="CumulativeDose" HeaderText="Cumul. Dose" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridBoundColumn DataField="Deep" UniqueName="Deep" HeaderText="Deep" AllowFiltering="false" AllowSorting="false" HeaderStyle-Width="50px" ItemStyle-Width="50px" />
                            <telerik:GridBoundColumn DataField="Shallow" UniqueName="Shallow" HeaderText="Shallow" AllowFiltering="false" AllowSorting="false" HeaderStyle-Width="75px" ItemStyle-Width="75px" />
                            <telerik:GridBoundColumn DataField="Eye" UniqueName="Eye" HeaderText="Eys" AllowFiltering="false" AllowSorting="false" HeaderStyle-Width="50px" ItemStyle-Width="50px" />
                            <telerik:GridDateTimeColumn DataField="AnalyzedDate" UniqueName="AnalyzedDate" HeaderText="Run Date" AllowFiltering="false" AllowSorting="false" DataFormatString="{0:MM/dd/yyyy hh:mm:ss tt}" /> 
                            <telerik:GridBoundColumn DataField="AnalyzedBy" UniqueName="AnalyzedBy" HeaderText="Run By" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridDateTimeColumn DataField="ReviewedDate" UniqueName="ReviewedDate" HeaderText="Analyzed Date" AllowFiltering="false" AllowSorting="false" DataFormatString="{0:MM/dd/yyyy hh:mm:ss tt}" /> 
                            <telerik:GridBoundColumn DataField="ReviewedBy" UniqueName="ReviewedBy" HeaderText="Analyzed By" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridBoundColumn DataField="FailedTests" UniqueName="FailedTests" HeaderText="Failed Tests" AllowFiltering="false" AllowSorting="false" />
                            <telerik:GridBoundColumn DataField="ActionTaken" UniqueName="ActionTaken" HeaderText="Action Taken" AllowFiltering="false" AllowSorting="false" />
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>
            </asp:Panel>
            <div class="Buttons">
                <div class="ButtonHolder">
                    <asp:Button ID="btnReturnToReadAnalysisPage" runat="server" CssClass="OButton" Text="Return to Read Analysis Page" OnClick="btnReturnToReadAnalysisPage_Click" />
                </div>
            </div>
        </div>
    </div>
    <%--END--%>
</asp:Content>
