<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript" src="/scripts/highcharts.js"></script>
        <script type="text/javascript">

        var chart;
        $(document).ready(function () {

            var legendLayout = {
                layout: 'horizontal',
                align: 'center',
                verticalAlign: 'bottom',
                backgroundColor: (Highcharts.theme && Highcharts.theme.legendBackgroundColorSolid) || 'white',
                x: 0,
                y: 0
            };

            chart = new Highcharts.Chart({
                chart: {
                    renderTo: 'online_users',
                    type: 'bar',
                    backgroundColor: 'transparent',
                    animation: false
                },
                colors: [
	                '#f4a62b', 
	                '#509e54', 
	                '#0073af', 
	                '#00a5b5', 
	                '#76c3ed', 
	                '#00a5e5', 
	                '#a190c2', 
	                '#4c528b', 
	                '#656d9e'
                ],
                title: { text: 'Instadose User Usage Per Week Day' },
                legend: legendLayout,
                plotOptions: {
                    pie: {
	                    allowPointSelect: true,
	                    cursor: 'pointer',
                        dataLabels: {
                            enabled: false
                        },
                        showInLegend: true
                    }
                },
                xAxis: {
                    title: {
                        text: 'Day of the Week'
                    },
                    categories: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']

                },
                yAxis: {
                    title: {
                        text: 'Total Users'
                    }
                },
                series: [{
                    name: 'Users',
                    data: <%= lblUsagePerDay.Text %>
                }]
            });
        });
    </script>
    <style type="text/css">
        li { padding: 4px 0; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">

    <telerik:RadScriptManager ID="RadScriptManager1" runat="server" 
        EnableTheming="True">
        <Scripts>
            <asp:ScriptReference Assembly="Telerik.Web.UI" 
                Name="Telerik.Web.UI.Common.Core.js">
            </asp:ScriptReference>
            <asp:ScriptReference Assembly="Telerik.Web.UI" 
                Name="Telerik.Web.UI.Common.jQuery.js">
            </asp:ScriptReference>
            <asp:ScriptReference Assembly="Telerik.Web.UI" 
                Name="Telerik.Web.UI.Common.jQueryInclude.js">
            </asp:ScriptReference>
            <asp:ScriptReference Assembly="Telerik.Web.UI" 
                Name="Telerik.Web.UI.Common.Core.js">
            </asp:ScriptReference>
            <asp:ScriptReference Assembly="Telerik.Web.UI" 
                Name="Telerik.Web.UI.Common.jQueryInclude.js">
            </asp:ScriptReference>
            <asp:ScriptReference Assembly="Telerik.Web.UI" 
                Name="Telerik.Web.UI.Common.jQuery.js">
            </asp:ScriptReference>
            <asp:ScriptReference Assembly="Telerik.Web.UI" 
                Name="Telerik.Web.UI.Common.Core.js">
            </asp:ScriptReference>
        </Scripts>
    </telerik:RadScriptManager>

    <telerik:RadStyleSheetManager ID="RadStyleSheetManager1" runat="server">
    </telerik:RadStyleSheetManager>
    <telerik:RadAjaxManager ID="ajaxManager" runat="server">
    </telerik:RadAjaxManager>

    <div>
        <div style="float:left; width: 500px; height: 100%;">
          <ol style="padding-top:0; margin-top: 0;">
            <li><a href="MultiUsersInsert.aspx">Load Users to Account</a>: Load Multi Users to Account with CSV File.</li>
            <li><a href="RateCodes.aspx">Rate Code Maintenance</a>: Maintain Rate Codes and Rate Code Details.</li>
            <li><a href="LabelPrintTest.aspx">Label Printing</a>: Print labels.</li>
            <li><a href="BatteryAnalysis.aspx">Battery Analysis Graph</a>: View Battery Analysis Data.</li>
            <li><a href="CreateOntimeItem.aspx">IT Help Desk Ticket</a>: Create an IT help desk ticket for OnTime.</li>
            <li><a href="HardwareUpdates.aspx">Manage Hardware Updates</a>: Manage hardware updates for the InstaLink.</li>
              
              <li><a href="ScheduleControlBadge.aspx">Calendar Control Badge</a>: Set calendars for control badge .</li>
              <li><a href="DeviceCommInformation.aspx">Device Communications Information</a>: View device communications information.</li>
              <li><a href="HelpDeskTool.aspx">IT Help Desk Tool</a>: Use tool to resolve help desk tickets.</li>
              
          </ol>
        
        </div>
        <div style="float:left; margin: 0 0 0 20px;">
            <asp:GridView runat="server" ID="gvApplicationStatus" CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
                AutoGenerateColumns="False" DataSourceID="sqlApplicationStatus" OnRowDataBound="gvApplicationStatus_RowDataBound">
                <Columns>
                    
                    <asp:TemplateField ItemStyle-Width="20">
                        <ItemTemplate>
                            <asp:Image ImageUrl='<%# Eval("StatusMessage") %>' style="width: 16px; height: 16px;" ID="imgStatusIcon" runat="server" AlternateText='<%# Eval("ErrorMessage") %>' />
                            <telerik:RadToolTip ID="toolTip" runat="server" Skin="Default" RelativeTo="Element" HideEvent="LeaveTargetAndToolTip" Animation="Fade" ShowEvent="OnMouseOver" TargetControlID="imgStatusIcon">
                            </telerik:RadToolTip>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:BoundField HeaderText="Application" DataField="ApplicationName" SortExpression="ApplicationName" />
                    
                    <asp:BoundField DataField="LocalStatusDate" HeaderText="Last Ran" SortExpression="StatusDate" DataFormatString="{0:MM/dd/yyyy h:mm tt}" ItemStyle-Width="150" />


                </Columns>

            </asp:GridView>

            <asp:SqlDataSource ID="sqlApplicationStatus" runat="server" 
                ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>" 
                SelectCommand="sp_GetApplicationStatus" SelectCommandType="StoredProcedure">
            </asp:SqlDataSource>

            <div id="online_users" style="width: 480px; height: 300px;">
            </div>
            <asp:Label Text="" Visible="false" ID="lblUsagePerDay" runat="server" />
        </div>
        <div class="Clear"></div>
    </div>

</asp:Content>

