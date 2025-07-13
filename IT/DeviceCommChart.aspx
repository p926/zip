<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_DeviceCommChart" Codebehind="DeviceCommChart.aspx.cs" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script lang="javascript" type="text/javascript">
        var dcdChart;

        // On FormLoad, set Focus on txtSerialNumber.
        window.onload = function () {
            var txtBx = document.getElementById("txtSerialNumber");
            if (txtBx != null) {
                txtBx.focus();
            }
        };

        // If the "Go" button is clicked, render the HighChart.
        $(document).ready(function () {
            $('#btnGo').click(function () {
                var serialNumber = $('#txtSerialNumber').val();
                // Query the Data from the Code Behind.
                if (serialNumber != "") {
                    $.ajax({
                        dataType: "json",
                        url: 'DeviceCommChart.aspx',
                        data: { SerialNo: serialNumber },
                        success: loadDeviceCommunicationDataChart,
                        error: function () {
                            alert('Data did not load properly.')
                        }
                    });
                }
                else {
                    alert("Please enter a Serial #.");
                }
            });
        });

        // If the Enter Key is pressed, then render the HighChart.
        function handleKeyPress(e) {
            var evt = (e) ? e : window.event;
            var key = (evt.keyCode) ? evt.keyCode : evt.which;
            if (key == 13) {
                var serialNumber = $('#txtSerialNumber').val();
                // Query the Data from the Code Behind.
                if (serialNumber != "") {
                    $.ajax({
                        dataType: "json",
                        url: 'DeviceCommChart.aspx',
                        data: { SerialNo: serialNumber },
                        success: loadDeviceCommunicationDataChart,
                        error: function () {
                            alert('Data did not load properly.')
                        }
                    });
                }
                else {
                    alert("Please enter a Serial #.");
                }
                return false;
            }
        }

        function loadDeviceCommunicationDataChart(data) {
            // Render the HighChart once the Data is received/compiled.
            dcdChart = new Highcharts.Chart({
                chart: {
                    renderTo: 'deviceCommunicationChart',
                    type: 'line',
                    zoomType: 'x',
                    maxZoom: 10 * 24 * 360000,
                    marginRight: 130,
                    marginBottom: 140
                },
                title: {
                    text: 'Device Communication Data',
                    x: -20
                },
                xAxis: {
                    title: {
                        text: 'Read Date'
                    },
                    categories: data.axis.categories,
                    labels: {
                        rotation: 45,
                        step: 15
                    }
                },
                yAxis: {
                    title: {
                        text: 'Value'
                    },
                    min: null,
                    max: null,
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
                    },
                    crosshairs: {
                        color: '#CCCCCC',
                        dashStyle: 'Solid',
                        width: 0.5
                    }
                },
                plotOptions: {
                    column: {
                        stacking: 'normal'
                    }
                },
                legend: {
                    enabled: true
                },
                series: data.series
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <script src="/scripts/highcharts.js"></script>
    <script src="/scripts/modules/exporting.js"></script>
    <telerik:RadScriptManager ID="rsManager" runat="server" />
    <div style="width: 100%;">
        <ul class="OToolbar">
            <li> 
                Enter Serial #:
                <input id="txtSerialNumber" type="text" name="txtSerialNumber" onkeypress="return handleKeyPress(event);" />
            </li> 
            <li>
                <input id="btnGo" type="button" name="btnGo" value="Go" />
            </li>
        </ul>
        <div style="width: 100%; border: 1px solid #EEEEEE;">
            <asp:UpdatePanel ID="updtpnlMainContent" runat="server">	
                <ContentTemplate>
                    <div id="deviceCommunicationChart" style="height: 650px;"></div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</asp:Content>

