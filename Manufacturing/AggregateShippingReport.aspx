<%@ Page Title="Home" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" EnableEventValidation="false" Inherits="Manufacturing_AggregateShippingReport" Codebehind="AggregateShippingReport.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        .riDisplay {
            text-shadow: none;
        }
    </style>
    <script type="text/javascript">
        var monthlyChart, dailyChart;

        function OnDateSelected(sender, e) {
            getMonthlyData(e.get_newDate());
        }

        function getMonthlyData(beginPeriod) {

            if (beginPeriod == null) return false;

            $.ajax({
                url: 'AggregateShippingReport.aspx',
                dataType: 'json',
                data: {
                    chart: 'monthly',
                    beginPeriod: beginPeriod.toJSON(),
                },
                success: loadMonthlyChart
            });
        }

        function getDailyData(date) {
            $.ajax({
                url: 'AggregateShippingReport.aspx',
                dataType: 'json',
                data: {
                    chart: 'daily',
                    summary_date: date
                },
                success: loadDailyChart
            });
        }

        function loadMonthlyChart(data) {

            monthlyChart = new Highcharts.Chart({
                chart: {
                    renderTo: 'monthlyContainer',
                    type: 'column',
                    marginRight: 0,
                    marginBottom: 50
                },
                title: {
                    text: 'Products Shipped per Month',
                    x: -20 //center
                },
                xAxis: {
                    categories: data.axis.categories,
                    labels: {
                        rotation: 90
                    }
                },
                yAxis: {
                    title: {
                        text: ''
                    },
                    min: 0,
                    plotLines: [{
                        value: 0,
                        width: 1,
                        color: '#808080'
                    }]
                },
                tooltip: {
                    formatter: function () {
                        return '<b>' + this.series.name + '</b><br/>' +
                        this.x + ': ' + this.y + '';
                    }
                },
                plotOptions: {
                    column: {
                        stacking: 'normal'
                    }
                },
                legend: {
                    enabled: false
                },
                colors: ["black", "blue", "green", "pink", "silver"],
                series: data.series
            });
        }

        function loadDailyChart(data) {

            dailyChart = new Highcharts.Chart({
                chart: {
                    renderTo: 'dailyContainer',
                    type: 'column',
                    marginRight: 35,
                    marginBottom: 20
                },
                title: {
                    text: 'Daily Shipments'
                },
                xAxis: {
                    categories: data.axis.categories
                },
                yAxis: {
                    title: {
                        text: ''
                    }
                },
                tooltip: {
                    formatter: function () {
                        return '<b>' + this.y + '</b> ' + this.series.name;
                    }
                },
                legend: {
                    layout: 'vertical',
                    align: 'right',
                    verticalAlign: 'top',
                    y: 35,
                    borderWidth: 1,
                    borderColor: 'silver',
                    backgroundColor: 'white'
                },
                colors: ["black", "blue", "green", "pink", "silver"],
                series: data.series
            });
        }
        function RowSelected(sender, eventArgs) {

            getDailyData(eventArgs.getDataKeyValue("ActualDate"));
        }

    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
<script src="/scripts/highcharts.js"></script>
<script src="/scripts/modules/exporting.js"></script>
<telerik:RadScriptManager ID="rsManager" runat="server" />
    
    <asp:UpdatePanel ID="upnlPicker" runat="server">
        <ContentTemplate>

            <ul class="OToolbar">
                <li> 
                    Select a Month: 
                    <telerik:RadMonthYearPicker ID="radPicker" MinDate="01/2008" ShowPopupOnFocus ="true" runat="server"
                        OnSelectedDateChanged="radPicker_SelectedDateChanged" AutoPostBack="true">
                        <ClientEvents OnDateSelected="OnDateSelected"></ClientEvents>
                    </telerik:RadMonthYearPicker>
                </li>

            </ul>
        </ContentTemplate>
    </asp:UpdatePanel>

<div style="width: 620px; min-height: 610px; float:left;">
    <asp:UpdatePanel runat="server" ID="upnlGrid">
        <ContentTemplate>


            <telerik:RadGrid ID="shipGrid" runat="server" AutoGenerateColumns="false" ShowFooter="true">
                <ClientSettings>
                    <Selecting AllowRowSelect="true" />
                    <ClientEvents OnRowSelected="RowSelected" />
                </ClientSettings>
                <MasterTableView ClientDataKeyNames="ActualDate" AllowPaging="true" PageSize="31">
                    <Columns>
                        <telerik:GridBoundColumn DataField="DisplayDate" HeaderText="Shipment Date" FooterText="Total Products: " />
                        <telerik:GridBoundColumn DataField="Black" HeaderText="Black" Aggregate="Sum" FooterAggregateFormatString="{0:#,##0}" />
                        <telerik:GridBoundColumn DataField="Blue" HeaderText="Blue" Aggregate="Sum" FooterAggregateFormatString="{0:#,##0}" />
                        <telerik:GridBoundColumn DataField="Green" HeaderText="Green" Aggregate="Sum" FooterAggregateFormatString="{0:#,##0}" />
                        <telerik:GridBoundColumn DataField="Pink" HeaderText="Pink" Aggregate="Sum" FooterAggregateFormatString="{0:#,##0}" />
                        <telerik:GridBoundColumn DataField="Silver" HeaderText="Silver" Aggregate="Sum" FooterAggregateFormatString="{0:#,##0}" />
                        <telerik:GridBoundColumn DataField="Total" HeaderText="Total" Aggregate="Sum" FooterAggregateFormatString="{0:#,##0}" />

                    </Columns>
                    <NoRecordsTemplate>
                        <div class="NoData">There are no shipments for this month.</div>
                    </NoRecordsTemplate>
                </MasterTableView>
            </telerik:RadGrid>
            
        </ContentTemplate>
    </asp:UpdatePanel>
    </div>
<div id="monthlyContainer" style="width: 450px; height: 260px; float:right;"></div>
<div id="dailyContainer" style="width: 450px; height: 260px; float:right; margin-top: 10px;"></div>
<div class="Clear"></div>
</asp:Content>

  