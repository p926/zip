<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Finance_Renewal_ForecastChart" Codebehind="ForecastChart.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript" src="/scripts/highcharts.js"></script>
        <script type="text/javascript">
        Number.prototype.formatMoney = function(c, d, t){
        var n = this, c = isNaN(c = Math.abs(c)) ? 2 : c, d = d == undefined ? "," : d, t = t == undefined ? "." : t, s = n < 0 ? "-" : "", i = parseInt(n = Math.abs(+n || 0).toFixed(c)) + "", j = (j = i.length) > 3 ? j % 3 : 0;
           return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
         };

        var chart;
        $(document).ready(function () {

            var legendLayout = {
                layout: 'vertical',
                align: 'right',
                verticalAlign: 'top',
                backgroundColor: (Highcharts.theme && Highcharts.theme.legendBackgroundColorSolid) || 'white',
                x: -50,
                y: 80,
                borderWidth: 2,
                floating: true
            };

            chart = new Highcharts.Chart({
                chart: {
                    renderTo: 'renewals_forecast',
                    type: 'areaspline',
                    height: 550
                },
                colors: [
	                '#509e54', 
	                '#0073af', 
	                '#f4a62b', 
	                '#00a5b5', 
	                '#76c3ed', 
	                '#00a5e5', 
	                '#a190c2', 
	                '#4c528b', 
	                '#656d9e'
                ],
                title: { text: 'Instadose Renewals Forecast' },
                legend: legendLayout,
                plotOptions: {
                    area: {
	                    allowPointSelect: true,
	                    cursor: 'pointer',
                        dataLabels: {
                            enabled: false
                        },
                        showInLegend: true
                    }
                },
                tooltip: {
                    formatter: function() {
                            return '<b>'+ this.series.name +'</b><br/>'+
                            '<b>' + this.x + '</b>: '+ (this.y).formatMoney(0, '.', ',') +' USD';
                    }
                },
                xAxis: {
                    title: {
                        text: 'Forecast Per Month'
                    },
                    categories: <%= lblCategories.Text %>

                },
                yAxis: {
                    title: {
                        text: 'Monthly Revenue in USD'
                    }
                },
                series: [{
                    name: 'Est. Revenue',
                    data: <%= lblResults.Text %>
                }]
            });
        });
    </script>
    <style type="text/css">
        li { padding: 4px 0; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:Label Text="" ID="lblResults" Visible="false" runat="server" />
    <asp:Label Text="" ID="lblCategories" Visible="false" runat="server" />
    <div id="renewals_forecast"></div>

</asp:Content>

