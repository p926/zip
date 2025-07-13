<%@ Page Title="Badge Review" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_BadgeReview" Codebehind="BadgeReview.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chartjs-adapter-date-fns/dist/chartjs-adapter-date-fns.bundle.min.js"></script>

    <link rel="stylesheet" type="text/css" href="../css/rad-controls/RadGrid.css" />
    <style type="text/css">
        #baselineTemperatureCalibrationChart, #doseRangeChart, #temperatureRangeChart, #roughDoseRangeChart, #temperatureDifferencesChart, #cumulativeDoseChart, #deepLowTempMinusDeepHighTempChart, #batteryPercentChart
        {
            margin: 0 auto;
            width: 1000px;
            height: 600px;
            border: 1px solid silver;
        }

        .Label
        {
            text-align: left;
        }

        /* CSS that creates small colored boxes for Reads Tab Legend. */
        .input-color 
        {
            position: relative;
        }

        .input-color .lbltxt
        {
            padding-left: 20px;
        }

        .input-color .color-box 
        {
            width: 10px;
            height: 10px;
            display: inline-block;
            position: absolute;
            left: 5px;
            top: 5px;
        }

        .motions-tab-container {
            overflow: auto;
            height: 75vh;
        }
    </style>
    <script type="text/javascript">
        <%--jQUERY HERE--%>
        var selectedTab;
        var drChart;
        var btcChart;
        var trChart;
        var cdChart;
        var rdChart;
        var tdChart;
        var dltdhtChart;
        var batteryPercentChart;
        var isIDVue = <%=ViewState["IsIDVue"].ToString().ToLower()%>;
        var productGroupID = <%=ViewState["ProductGroupID"].ToString().ToLower()%>;
        var isIDPlus = productGroupID == 10;

        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();

            // Character Counter for Technical Notes.
            $('#<%=txtTechnicalNotes.ClientID %>').keyup(function (e) {
                var limit = 255;
                var tbLength = $(this).val().length;
                if (tbLength > limit) {
                    this.value = this.value.substring(0, limit);
                    e.preventDefault();
                }
                $('#<%=lblCharacterCounter_TechnicalNotes.ClientID %>').show();
                // White-Space is not counted in the Character Counter total.
                $("#<%=lblCharacterCounter_TechnicalNotes.ClientID %>").text($(this).val().replace(/ /g, '').length + '/' + limit);
            });

            // Character Counter for Customer Service (CS) Notes.
            $('#<%=txtCSNotes.ClientID %>').keyup(function (e) {
                var limit = 255;
                var tbLength = $(this).val().length;
                if (tbLength > limit) {
                    this.value = this.value.substring(0, limit);
                    e.preventDefault();
                }
                $('#<%=lblCharacterCounter_CSNotes.ClientID %>').show();
                // White-Space is not counted in the Character Counter total.
                $("#<%=lblCharacterCounter_CSNotes.ClientID %>").text($(this).val().replace(/ /g, '').length + '/' + limit);
            });
        });

        function JQueryControlsLoad() {
            var tabs = $("#tabsContainer").tabs({
                show: function () {
                    // Get the selected Tab's index.
                    selectedTab = $('#tabsContainer').tabs('option', 'selected');
                }
            });

            $("#<%=Tab4.ClientID %>").click(function () {
                reloadCharts();
            })

            $('#<%=Tab1.ClientID %>').click(function () {
                // Baseline Temperature Calibration Chart.
                baselineTemperatureCalibrationChartLoad();
            })

            // Configure Technical Notes (Modal/Dialog).
            $('#divAddTechnicalNoteDialog').dialog({
                autoOpen: false,
                width: 400,
                resizable: false,
                title: "Add Technical Note",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Add": function () {
                        $('#<%= btnAddTechnicalNote.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $('#<%= btnCancel_TN.ClientID %>').click();
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            // Configure Complete Review Process (Modal/Dialog).
            $('#divCompleteReviewProcessDialog').dialog({
                autoOpen: false,
                width: 850,
                resizable: false,
                title: "Complete Review Process",
                open: function (type, data) {

                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Mark As Reviewed": function () {
                        $('#<%= btnCompleteReviewProcess.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $('#<%= btnCancel_CRP.ClientID %>').click();
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            var readDetailsDialogWidth = $(window).width() - 50;
            if (readDetailsDialogWidth < 1100)
                readDetailsDialogWidth = 1100;
            else if (readDetailsDialogWidth > 1436)
                readDetailsDialogWidth = 1436;

            // Read Details (Modal/Dialog).
            $('#divReadDetailsDialog').dialog({
                autoOpen: false,
                width: readDetailsDialogWidth,
                minHeight: 527,
                resizable: false,
                title: "Read Details",
                open: function (type, data) {
                    var $this = $(this);
                    var rid = $(this).data('rid');
                    var serialNo = $('#serialNumber').val();

                    $this.find('.dialog-body').text("Loading...");
                    $.get("BadgeReviewRIDDetails.aspx?RID=" + rid + "&serialNumber=" + serialNo, function (data) {
                        $this.find('.dialog-body').html(data);
                    });

                },
                buttons: {
                    "Export to CSV": function () {
                        var $this = $(this);

                        var $readDetailsTable = $this.find('table.read-details-table');

                        if (!$readDetailsTable.length)
                            return;

                        var data = "Read Details\n";
                        
                        data += tableToCsv($readDetailsTable);
                        data += "\n\nExceptions\n";
                        var $exceptionsTable = $this.find('table.exceptions-table');
                        data += tableToCsv($exceptionsTable);
                        data += "\n\nDaily Motion\n";
                        var $dailyMotionTable = $this.find('table.daily-motion-table');
                        data += tableToCsv($dailyMotionTable);

                        var d = new Date();
                        var serialNo = $('#serialNumber').val();
                        var datestring = d.getFullYear() + "-" + ("0" + (d.getMonth() + 1)).slice(-2) + "-" + ("0" + d.getDate()).slice(-2) +
                             " " + ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2);
                        var filename = `IDVue_ReadDetails_${serialNo}_${datestring}.csv`;

                        $(document.body).append('<a id="download-link" download="' + filename + '" href=' + URL.createObjectURL(new Blob([data], {
                            type: "text/csv"
                        })) + '/>');

                        $('#download-link')[0].click();
                        $('#download-link').remove();

                    },
                    "Close": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            $(".rid-link").click(function (event) {
                event.preventDefault();
                var rid = $(this).attr('data-value');
                $("#divReadDetailsDialog").data('rid', rid).dialog("open");
            });
            // Global Varibles used for Dose Range, Rough Dose, and Temperature charts.
            var serialNumber = $('#serialNumber').val();
            var rID = $('#rID').val();
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();

            reloadCharts();

            // Rerender Dose Range Chart based on Option.
            $('input:radio[name=chartType_Dose]').click(function () {
                var selectedOption = $(this).val();
                var includeSoftReads = $("#chartType_Dose_IncludeSoftReads").is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Dose', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadDoseRangeChart);
            });
            $('#chartType_Dose_IncludeSoftReads').change(function () {
                var selectedOption = $('input:radio[name=chartType_Dose]').val();
                var includeSoftReads = $(this).is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Dose', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadDoseRangeChart);
            });

            // Rerender Rough Dose Range Chart based on Option.
            $('input:radio[name=chartType_Rough]').click(function () {
                var selectedOption = $(this).val();
                var includeSoftReads = $("#chartType_Rough_IncludeSoftReads").is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Rough', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadRoughDoseRangeChart);
            });
            $('#chartType_Rough_IncludeSoftReads').change(function () {
                var selectedOption = $('input:radio[name=chartType_Rough]').val();
                var includeSoftReads = $(this).is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Rough', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadRoughDoseRangeChart);
            });


            // Rerender Temperature Range Chart based on Option.
            $('input:radio[name=chartType_Temp]').click(function () {
                var selectedOption = $(this).val();
                var includeSoftReads = $("#chartType_Temp_IncludeSoftReads").is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Temp', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadTemperatureRangeChart);
            });
            $('#chartType_Temp_IncludeSoftReads').change(function () {
                var selectedOption = $('input:radio[name=chartType_Temp]').val();
                var includeSoftReads = $(this).is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Temp', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadTemperatureRangeChart);
            });

            // Rerender Temperature Difference Chart based on Option.
            $('input:radio[name=chartType_Diff]').click(function () {
                var selectedOption = $(this).val();
                var includeSoftReads = $("#chartType_Diff_IncludeSoftReads").is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Difference', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadTemperatureDifferenceChart);
            });
            $('#chartType_Diff_IncludeSoftReads').change(function () {
                var selectedOption = $('input:radio[name=chartType_Diff]').val();
                var includeSoftReads = $(this).is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Difference', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadTemperatureDifferenceChart);
            });

            // Rerender Cumulative Dose Chart based on Option.
            $('input:radio[name=chartType_Cumul]').click(function () {
                var selectedOption = $(this).val();
                var includeSoftReads = $("#chartType_Cumul_IncludeSoftReads").is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Cumulative', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadCumulativeDoseChart);                
            });
            $('#chartType_Cumul_IncludeSoftReads').change(function () {
                var selectedOption = $('input:radio[name=chartType_Cumul]').val();
                var includeSoftReads = $(this).is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Cumulative', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadCumulativeDoseChart);
            });

            // Rerender DLT-DHT Chart based on Option.
            $('input:radio[name=chartType_DLTDHT]').click(function () {
                var selectedOption = $(this).val();
                var includeSoftReads = $("#chartType_DLTDHT_IncludeSoftReads").is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'DLTDHT', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadDLTMinusDHTChart);
            });
            $('#chartType_DLTDHT_IncludeSoftReads').change(function () {
                var selectedOption = $('input:radio[name=chartType_DLTDHT]').val();
                var includeSoftReads = $(this).is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'DLTDHT', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadDLTMinusDHTChart);
            });

            // Rerender Battery Percent Chart based on Option.
            $('input:radio[name=chartType_BatteryPercent]').click(function () {
                var selectedOption = $(this).val();
                var includeSoftReads = $("#chartType_BatteryPercent_IncludeSoftReads").is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'BatteryPercent', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadBatteryPercentChart);
            });
            $('#chartType_BatteryPercent_IncludeSoftReads').change(function () {
                var selectedOption = $('input:radio[name=chartType_BatteryPercent]').val();
                var includeSoftReads = $(this).is(':checked');
                var data = { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'BatteryPercent', Option: selectedOption, IncludeSoftReads: includeSoftReads };
                getAndLoadChartData(data, loadBatteryPercentChart);
            });            
        }

        function reloadCharts() {
            // Baseline Temperature Calibration Chart.
            baselineTemperatureCalibrationChartLoad();

            // Dose Range Chart (initial load).
            doseRangeHighChartLoad();

            if (!(isIDVue || isIDPlus)) {
                // Rough Dose Range Chart (initial load).
                roughDoseRangeHighChartLoad();

                // Temperature Difference Chart (initial load).
                temperatureDifferenceHighChartLoad();
            } else {
                $("#roughDoseRangeChartContainer").hide();
                $("#temperatureDifferencesChartContainer").hide();
            }

            if (isIDVue) {
                $("#temperatureRangeChartContainer .temprange-chart-option").hide();
                $("#temperatureRangeChartContainer input.default-chart-option").parent().show();
                $("#deepLowTempMinusDeepHighTempChartContainer").hide();
            } else {
                // DLT-DHT Chart.
                DLTMinusDHTHighChartLoad();
            }

            // Temperature Range Chart (initial load).
            temperatureRangeHighChartLoad();

            // Cumulative Dose Chart.
            cumulativeDoseHighChartLoad();

            // Battery Percent Chart.
            batteryPercentChartLoad();
        }

        function getAndLoadChartData(data, successCallback) {
            $.ajax({
                    dataType: "json",
                    url: 'BadgeReview.aspx',
                    data: data,
                    success: successCallback,
                    error: function () {
                        alert('Data did not load properly.')
                    }
            });
        }

        function tableToCsv($table) {
            var data = "";
            var tableData = [];
            var $rows = $table.find('tr');
            $rows.each(function (index, row) {
                var rowData = [];
                $(row).find("th, td").each(function (index, column) {
                    rowData.push(column.innerText);
                });
                tableData.push(rowData.join(","));
            });
            data += tableData.join("\n");
            return data;

        }

        function pageLoad(sender, args) {
            if (args.get_isPartialLoad()) {
                $("#tabsContainer").tabs({
                    show: function () {
                        // Get the selected Tab's index on Partial Postback.
                        selectedTab = $('#tabsContainer').tabs('option', 'selected');

                    }, selected: selectedTab
                });
            }
        };

        function textCounter(field, countfield, maxlimit) {
            if (field.value.length > maxlimit)
                field.value = field.value.substring(0, maxlimit);
            else
                countfield.value = maxlimit - field.value.length;
        };

        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        }

        function closeDialog(id) {
            $('#' + id).dialog("close");
        }

        //----- Begin :: Baseline Temperature Calibration Chart Code -----//
        function baselineTemperatureCalibrationChartLoad() {
            var serialNumber = $('#serialNumber').val();
            var rID = $('#rID').val();

            // Query the Data from the Code Behind.
            $.ajax({
                dataType: "json",
                url: 'BadgeReview.aspx',
                data: { SerialNo: serialNumber, RID: rID, Chart: 'Baseline' },
                success: loadBaselineTempCalibraChart,
                error: function () {
                    alert('Data did not load properly.')
                }
            });
        }

        function loadBaselineTempCalibraChart(data) {

            if (btcChart) {
                btcChart.clear();
                btcChart.destroy();
            }

            var baselineTemperatureCalibrationChart = document.getElementById('baselineTemperatureCalibrationChart');
            // Render the Chart once the Data is received/compiled.
            btcChart = new Chart(baselineTemperatureCalibrationChart, {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: "dTL to°C",
                            data: data.dlt,
                            borderColor: '#4572A7',
                            backgroundColor: '#4572A7',
                            yAxisID: 'y'
                        },
                        {
                            label: "dTH to°C",
                            data: data.dht,
                            borderColor: '#AA4643',
                            backgroundColor: '#AA4643',
                            yAxisID: 'y'
                        }
                    ]
                },
                options: {
                    responsive: true,
                    interaction: {
                        mode: 'index',
                        intersect: false,
                    },
                    plugins: {
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        },
                        title: {
                            display: true,
                            text: 'Baseline Temperature Calibration'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    },
                    stacked: false,
                    scales: {
                        x: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Baseline Row",
                                color: "#4572A7",
                            }
                        },
                        y: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Temperature (℃)",
                                color: "#4572A7",
                            },
                            position: 'left',
                        }
                    }
                }
            });            
        }
        //----- End :: Baseline Temperature Calibration Chart Code -----//

        //----- Begin :: Dose Range Chart Code -----//
        function doseRangeHighChartLoad() {
            var serialNumber = $('#serialNumber').val();
            var rID = $('#rID').val();
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();

            // Query the Data from the Code Behind.
            $.ajax({
                dataType: "json",
                url: 'BadgeReview.aspx',
                data: { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Dose' },
                success: loadDoseRangeChart,
                error: function () {
                    alert('Data did not load properly.')
                }
            });
        }

        function loadDoseRangeChart(data) {
            if (isIDVue) {
                $("#doseRangeChartContainer").hide();
            }

            if (drChart) {
                drChart.clear();
                drChart.destroy();
            }

            var doseRangeChart = document.getElementById('doseRangeChart');

            drChart = new Chart(doseRangeChart, {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: "Deep Low",
                            data: data.dl,
                            borderColor: '#4572A7',
                            backgroundColor: '#4572A7',
                            yAxisID: 'y',
                        },
                        {
                            label: "Deep High",
                            data: data.dh,
                            borderColor: '#AA4643',
                            backgroundColor: '#AA4643',
                            yAxisID: 'y1',
                        }
                    ]
                },
                options: {
                    responsive: true,
                    interaction: {
                        mode: 'index',
                        intersect: false,
                    },
                    plugins: {
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        },
                        title: {
                            display: true,
                            text: 'Dose Range Graph'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    },
                    stacked: false,
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                displayFormats: {
                                    quarter: 'MMM YYYY'
                                }
                            },
                            title: {
                                display: true,
                                text: "Exposure Date",
                                color: "#4572A7",
                            },
                        },
                        y: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Deep Low",
                                color: "#4572A7",
                            },
                            position: 'left',
                        },
                        y1: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Deep High",
                                color: "#AA4643",
                            },
                            position: 'right',

                            // grid line settings
                            grid: {
                                drawOnChartArea: false, // only want the grid lines for one axis to show up
                            }
                        }
                    }
                }
            });

        }
        //----- End :: Dose Range Chart Code -----//

        //----- Begin :: Rough Dose Range Chart Code -----//
        function roughDoseRangeHighChartLoad() {
            var serialNumber = $('#serialNumber').val();
            var rID = $('#rID').val();
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();

            // Query the Data from the Code Behind.
            $.ajax({
                dataType: "json",
                url: 'BadgeReview.aspx',
                // Option is set to 1 by default (forces chart to load/render first time).
                data: { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Rough', Option: 1 },
                success: loadRoughDoseRangeChart,
                error: function () {
                    alert('Data did not load properly.')
                }
            });
        }

        function loadRoughDoseRangeChart(data) {
            if (isIDVue || isIDPlus) {
                $("#roughDoseRangeChartContainer").hide();
            }

            if (rdChart) {
                rdChart.clear();
                rdChart.destroy();
            }

            var roughDoseRangeChart = document.getElementById('roughDoseRangeChart');
            // Render the Chart once the Data is received/compiled.
            rdChart = new Chart(roughDoseRangeChart, {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: "Deep Low",
                            data: data.dl,
                            borderColor: '#4572A7',
                            backgroundColor: '#4572A7',
                            yAxisID: 'y',
                        },
                        {
                            label: "Deep High",
                            data: data.dh,
                            borderColor: '#AA4643',
                            backgroundColor: '#AA4643',
                            yAxisID: 'y1',
                        }
                    ]
                },
                options: {
                    responsive: true,
                    interaction: {
                        mode: 'index',
                        intersect: false,
                    },
                    plugins: {
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        },
                        title: {
                            display: true,
                            text: 'Rough Dose Range Graph'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    },
                    stacked: false,
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                displayFormats: {
                                    quarter: 'MMM YYYY'
                                }
                            },
                            title: {
                                display: true,
                                text: "Exposure Date",
                                color: "#4572A7",
                            },
                        },
                        y: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Deep Low",
                                color: "#4572A7",
                            },
                            position: 'left',
                        },
                        y1: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Deep High",
                                color: "#AA4643",
                            },
                            position: 'right',

                            // grid line settings
                            grid: {
                                drawOnChartArea: false, // only want the grid lines for one axis to show up
                            }
                        }
                    }
                }
            });        
        }
        //----- End :: Rough Dose Range Chart Code -----//

        //----- Begin :: Temperature Range Chart Code -----//
        function temperatureRangeHighChartLoad() {
            var serialNumber = $('#serialNumber').val();
            var rID = $('#rID').val();
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();

            // Query the Data from the Code Behind.
            $.ajax({
                dataType: "json",
                url: 'BadgeReview.aspx',
                data: { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Temp' },
                success: loadTemperatureRangeChart,
                error: function () {
                    alert('Data did not load properly.')
                }
            });
        }

        function loadTemperatureRangeChart(data) {
            if (data == null) {
                $("#temperatureRangeChartContainer").hide();
                return;
            }
            if (isIDVue) {
                $("#temperatureRangeChartContainer .temprange-chart-option").hide();
                $("#temperatureRangeChartContainer input.default-chart-option").parent().show();

                loadTemperatureRangeIDVueChart(data);
                return;
            }

            if (trChart) {
                trChart.clear();
                trChart.destroy();
            }

            var temperatureRangeChart = document.getElementById('temperatureRangeChart');
            // Render the Chart once the Data is received/compiled.
            trChart = new Chart(temperatureRangeChart, {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: "Deep Low Temp",
                            data: data.dlt,
                            borderColor: '#4572A7',
                            backgroundColor: '#4572A7',
                            yAxisID: 'y'
                        }
                    ]
                },
                options: {
                    responsive: true,
                    interaction: {
                        mode: 'index',
                        intersect: false,
                    },
                    plugins: {
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        },
                        title: {
                            display: true,
                            text: 'Temperature Range Graph'
                        },
                        legend: {
                            position: 'bottom',
                            labels: {
                                filter: function (item, chart) {
                                    // Logic to remove a particular legend item goes here
                                    return !item.text.includes('Range');
                                }
                            }
                        }
                    },
                    stacked: false,
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                displayFormats: {
                                    quarter: 'MMM YYYY'
                                }
                            },
                            title: {
                                display: true,
                                text: "Exposure Date",
                                color: "#4572A7",
                            }
                        },
                        y: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Deep Low Temp",
                                color: "#4572A7"
                            },
                            position: 'left',
                        }
                    }
                }
            });
            
        }

        function loadTemperatureRangeIDVueChart(data) {

            if (trChart) {
                trChart.clear();
                trChart.destroy();
            }

            var temperatureRangeChart = document.getElementById('temperatureRangeChart');
            // Render the Chart once the Data is received/compiled.
            trChart = new Chart(temperatureRangeChart, {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: "Temperature Range",
                            data: data,
                            borderColor: '#4572A7',
                            backgroundColor: '#4572A7',
                            yAxisID: 'y'
                        }
                    ]
                },
                options: {
                    responsive: true,
                    interaction: {
                        mode: 'index',
                        intersect: false,
                    },
                    plugins: {
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        },
                        title: {
                            display: true,
                            text: 'Temperature Range Graph'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    },
                    stacked: false,
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                displayFormats: {
                                    quarter: 'MMM YYYY'
                                }
                            },
                            title: {
                                display: true,
                                text: "Exposure Date",
                                color: "#4572A7",
                            }
                        },
                        y: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Temperature Range",
                                color: "#4572A7",
                            },
                            position: 'left',
                        }
                    }
                }
            });
        }
        //----- End :: Temperature Range Chart Code -----//

        //----- Begin :: Temperature Difference Chart Code -----//
        function temperatureDifferenceHighChartLoad() {
            var serialNumber = $('#serialNumber').val();
            var rID = $('#rID').val();
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();

            // Query the Data from the Code Behind.
            $.ajax({
                dataType: "json",
                url: 'BadgeReview.aspx',
                data: { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Difference', Option: 1 },
                success: loadTemperatureDifferenceChart,
                error: function () {
                    alert('Data did not load properly.')
                }
            });
        }

        function loadTemperatureDifferenceChart(data) {
            if (isIDVue || isIDPlus) {
                $("#temperatureDifferencesChartContainer").hide();
            }

            if (tdChart) {
                tdChart.clear();
                tdChart.destroy();
            }

            var temperatureDifferenceChart = document.getElementById('temperatureDifferencesChart');
            // Render the Chart once the Data is received/compiled.
            tdChart = new Chart(temperatureDifferenceChart, {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: "Temperature Difference Range",
                            data: data,
                            borderColor: '#4572A7',
                            backgroundColor: '#4572A7',
                            yAxisID: 'y',
                        }
                    ]
                },
                options: {
                    responsive: true,
                    interaction: {
                        mode: 'index',
                        intersect: false,
                    },
                    plugins: {
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        },
                        title: {
                            display: true,
                            text: 'Temperature Difference Graph'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    },
                    stacked: false,
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                displayFormats: {
                                    quarter: 'MMM YYYY'
                                }
                            },
                            title: {
                                display: true,
                                text: "Exposure Date",
                                color: "#4572A7",
                            }
                        },
                        y: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Temperature Difference Range",
                                color: "#4572A7",
                            },
                            position: 'left',
                        }
                    }
                }
            });            
        }
        //----- End :: Temperature Difference Chart Code -----//

        //----- Conversion of Temperature values to Fahrenheit -----//
        function convertTemperature(temp) {
            if (temp > 40000)
                temp = temp - 31730;

            var convertTemp = (Math.round(((temp) / -165 + 22) * 9 / 5 + 32)).toString();
            return convertTemp;
        }
        //----- End -----//

        //----- Begin :: Cumulative Dose Chart Code -----//
        function cumulativeDoseHighChartLoad() {
            var serialNumber = $('#serialNumber').val();
            var rID = $('#rID').val();
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();

            // Query the Data from the Code Behind.
            $.ajax({
                dataType: "json",
                url: 'BadgeReview.aspx',
                data: { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'Cumulative', Option: 1 },
                success: loadCumulativeDoseChart,
                error: function () {
                    alert('Data did not load properly.')
                }
            });
        }

        function loadCumulativeDoseChart(data) {

            if (cdChart) {
                cdChart.clear();
                cdChart.destroy();
            }

            if (isIDVue) {
                loadCumulativeDoseChartIDVue(data);
                return;
            }

            var cumulativeDoseChart = document.getElementById('cumulativeDoseChart');
            // Render the Chart once the Data is received/compiled.
            cdChart = new Chart(cumulativeDoseChart, {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: "Cumulative Dose",
                            data: data,
                            borderColor: '#4572A7',
                            backgroundColor: '#4572A7',
                            yAxisID: 'y',
                        }
                    ]
                },
                options: {
                    responsive: true,
                    interaction: {
                        mode: 'index',
                        intersect: false,
                    },
                    plugins: {
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        },
                        title: {
                            display: true,
                            text: 'Cumulative Dose Graph'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    },
                    stacked: false,
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                displayFormats: {
                                    quarter: 'MMM YYYY'
                                }
                            },
                            title: {
                                display: true,
                                text: "Exposure Date",
                                color: "#4572A7",
                            },
                        },
                        y: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Cumulative Dose",
                                color: "#4572A7",
                            },
                            position: 'left',
                        }
                    }
                }
            });
        }

        function loadCumulativeDoseChartIDVue(data) {

            if (cdChart) {
                cdChart.clear();
                cdChart.destroy();
            }

            var cumulativeDoseChart = document.getElementById('cumulativeDoseChart');
            // Render the Chart once the Data is received/compiled.
            cdChart = new Chart(cumulativeDoseChart, {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: "DL Cumulative Dose",
                            data: data.dl,
                            borderColor: '#4572A7',
                            backgroundColor: '#4572A7',
                            yAxisID: 'y',
                        },
                        {
                            label: "DH Cumulative Dose",
                            data: data.dh,
                            borderColor: '#AA4643',
                            backgroundColor: '#AA4643',
                            yAxisID: 'y1',
                        }
                    ]
                },
                options: {
                    responsive: true,
                    interaction: {
                        mode: 'index',
                        intersect: false,
                    },
                    plugins: {
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        },
                        title: {
                            display: true,
                            text: 'Cumulative Dose Graph'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    },
                    stacked: false,
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                displayFormats: {
                                    quarter: 'MMM YYYY'
                                }
                            },
                            title: {
                                display: true,
                                text: "Exposure Date",
                                color: "#4572A7",
                            },
                        },
                        y: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "DL Cumulative Dose",
                                color: "#4572A7",
                            },
                            position: 'left',
                        },
                        y1: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "DH Cumulative Dose",
                                color: "#AA4643",
                            },
                            position: 'right',

                            // grid line settings
                            grid: {
                                drawOnChartArea: false, // only want the grid lines for one axis to show up
                            }
                        }
                    }
                }
            });
        }
        //----- End :: Cumulative Dose Chart Code -----//

        //----- Begin :: DLT-DHT Chart Code -----//
        function DLTMinusDHTHighChartLoad() {
            var serialNumber = $('#serialNumber').val();
            var rID = $('#rID').val();
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();

            // Query the Data from the Code Behind.
            $.ajax({
                dataType: "json",
                url: 'BadgeReview.aspx',
                data: { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'DLTDHT', Option: 1 },
                success: loadDLTMinusDHTChart,
                error: function () {
                    alert('Data did not load properly.')
                }
            });
        }

        function loadDLTMinusDHTChart(data) {            
            if (isIDVue) {
                $("#deepLowTempMinusDeepHighTempChartContainer").hide();
                return;
            }

            if (dltdhtChart) {
                dltdhtChart.clear();
                dltdhtChart.destroy();
            }

            var dltdht = document.getElementById('deepLowTempMinusDeepHighTempChart');
            // Render the Chart once the Data is received/compiled.
            dltdhtChart = new Chart(dltdht, {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: "DLT-DHT Range",
                            data: data,
                            borderColor: '#4572A7',
                            backgroundColor: '#4572A7',
                            yAxisID: 'y'
                        }
                    ]
                },
                options: {
                    responsive: true,
                    interaction: {
                        mode: 'index',
                        intersect: false
                    },
                    plugins: {
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        },
                        title: {
                            display: true,
                            text: 'DLT-DHT Graph'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    },
                    stacked: false,
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                displayFormats: {
                                    quarter: 'MMM YYYY'
                                }
                            },
                            title: {
                                display: true,
                                text: "Value",
                                color: "#4572A7"
                            }
                        },
                        y: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Cumulative Dose",
                                color: "#4572A7"
                            },
                            position: 'left'
                        }
                    }
                }
            });            
        }
        //----- End :: DLT-DHT Chart Code -----//

        //----- Begin :: Battery % Chart -----//
        function batteryPercentChartLoad() {
            var serialNumber = $('#serialNumber').val();
            var rID = $('#rID').val();
            var startDate = $('#startDate').val();
            var endDate = $('#endDate').val();

            // Query the Data from the Code Behind.
            $.ajax({
                dataType: "json",
                url: 'BadgeReview.aspx',
                data: { SerialNo: serialNumber, RID: rID, StartDate: startDate, EndDate: endDate, Chart: 'BatteryPercent', Option: 1 },
                success: loadBatteryPercentChart,
                error: function () {
                    alert('Data did not load properly.')
                }
            });
        }

        function loadBatteryPercentChart(data) {

            if (batteryPercentChart) {
                batteryPercentChart.clear();
                batteryPercentChart.destroy();
            }

            var isID1 = (productGroupID == 1 || productGroupID == 9);
            if (isID1) {
                $("#batteryPercentChartContainer").hide();
                return;
            }

            var btChart = document.getElementById('batteryPercentChart');
            // Render the Chart once the Data is received/compiled.
            batteryPercentChart = new Chart(btChart, {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: "Battery Percent",
                            data: data,
                            borderColor: '#4572A7',
                            backgroundColor: '#4572A7',
                            yAxisID: 'y'
                        }
                    ]
                },
                options: {
                    responsive: true,
                    interaction: {
                        mode: 'index',
                        intersect: false
                    },
                    plugins: {
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        },
                        title: {
                            display: true,
                            text: 'Battery Percent Graph'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    },
                    stacked: false,
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                displayFormats: {
                                    quarter: 'MMM YYYY'
                                }
                            },
                            title: {
                                display: true,
                                text: "Value",
                                color: "#4572A7"
                            }
                        },
                        y: {
                            type: 'linear',
                            display: true,
                            title: {
                                display: true,
                                text: "Read Date",
                                color: "#4572A7"
                            },
                            position: 'left'
                        }
                    }
                }
            });
        }
        //----- End :: Battery % Chart -----//
    </script>

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
                column = null;
                menu.repaint();
            }

            function filterMenuShowing(sender, eventArgs) {
                column = eventArgs.get_column();
            }
        </script>
    </telerik:RadCodeBlock>
    <%--END--%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <script src="/scripts/highcharts.js"></script>
    <script src="/scripts/modules/exporting.js"></script>
    <telerik:RadScriptManager ID="rsManager" runat="server" />
    <telerik:RadStyleSheetManager ID="rssManager" runat="server"></telerik:RadStyleSheetManager>
    <%--ADD TECHNICAL NOTES MODAL/DIALOG--%>
    <div id="divAddTechnicalNoteDialog">
        <asp:UpdatePanel ID="updtpnlAddTechnicalNotes" runat="server">
            <ContentTemplate>
                <div class="FormError" id="divErrorMessage_TechnicalNotes" runat="server" visible="false" style="text-align: left;">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong>&nbsp;<span id="spnErrorMessage_TechnicalNotes" runat="server">An error was encountered.</span></p>
                </div>
                <div class="OForm">
                    <%--TECHNICAL NOTES FORM--%>
                    <div class="Row">                
				        <div class="Label Medium">Technical Note<span class="Required">*</span>:</div>
				        <div class="Control">
                            <asp:TextBox ID="txtTechnicalNotes" runat="server" TextMode="MultiLine" Width="350px" Rows="5" MaxLength="255" />
                            <br />
                            <asp:Label ID="lblCharacterCounter_TechnicalNotes" runat="server" />    
				        </div>
				        <div class="Clear"></div>
			        </div>
                    <div class="Footer">
                        <span class="Required">*</span> Indicates a required field.
                    </div>
		        </div>
                <asp:Button ID="btnAddTechnicalNote" runat="server" style="display: none;" Text="Add" OnClick="btnAddTechnicalNote_Click" />
                <asp:Button ID="btnCancel_TN" runat="server" style="display:none;" Text="Cancel" OnClick="btnCancel_TN_Click" />
                <%--END--%>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>   
    <%--END--%>
    <%--READ DETAILS MODAL/DIALOG--%>
    <div id="divReadDetailsDialog">
        <div class="dialog-body">
            Read Details Dialog
        </div>
    </div>   
    <%--END--%>
    <%--COMPLETE REVIEW PROCESS MODAL/DIALOG--%>
    <div id="divCompleteReviewProcessDialog">
        <asp:UpdatePanel ID="updtpnlCompelteReviewProcessForm" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="FormError" id="divErrorMessage_Review" runat="server" visible="false" style="text-align: left;">
	                <p><span class="MessageIcon"></span>
	                <strong>Message:</strong>&nbsp;<span id="spnErrorMessage_Review" runat="server">There are no previous review records for this badge.</span></p>
                </div>
                <asp:UpdateProgress id="UpdateProgress1" runat="server" DynamicLayout="true" DisplayAfter="0" >
                    <ProgressTemplate>
                        <center>
                            <div style="width: 100%;">
                                <img src="../images/loading11.gif" alt="Processing, please wait..." width="128" height="15" />
                            </div>
                        </center>
                    </ProgressTemplate>
                </asp:UpdateProgress>     
                <div class="OForm">
                    <%--REVIEW FORM TITLE--%>
                    <div class="Row">
                        <div class="Label Medium"><asp:Label ID="lblReviewFormTitle" runat="server" Font-Bold="true" Font-Size="Large" Text="REVIEW FORM"></asp:Label></div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>
                    <%--READ ID--%>
                    <div class="Row">                
						<div class="Label Medium">Read ID:</div>
						<div class="Control">
                            <asp:Label ID="lblReadID" runat="server" CssClass="LabelValue"></asp:Label>
						</div>
						<div class="Clear"></div>
					</div>
                    <%--END--%>
                    <%--ANALYSIS ACTION TYPE--%>   							   
					<div class="Row">                
						<div class="Label Medium">Action<span class="Required">*</span>:</div>
						<div class="Control">
                            <asp:RadioButtonList ID="rdobtnlstAnalysisActionType" runat="server" RepeatDirection="Horizontal" 
                            OnSelectedIndexChanged="rdobtnlstAnalysisActionType_OnSelectedIndexChanged" AutoPostBack="true">
                            </asp:RadioButtonList>
						</div>
						<div class="Clear"></div>
					</div>
                    <%--END--%>
                    <%--NOTES--%>
                    <div class="Row">                
						<div class="Label Medium">CS Notes:</div>
						<div class="Control">
                            <asp:TextBox ID="txtCSNotes" runat="server" TextMode="MultiLine" Width="500px" Rows="5" MaxLength="255" />
                            <br />
                            <asp:Label ID="lblCharacterCounter_CSNotes" runat="server" />
						</div>
						<div class="Clear"></div>
					</div>
                    <%--END--%>
                    <%--E-MAIL CUSTOMER--%>
                    <div class="Row">                
						<div class="Label Medium">Send E-Mail:</div>
						<div class="Control">
                            <asp:CheckBox ID="chkbxSendEmail" runat="server" OnCheckedChanged="chkbxSendEmail_OnCheckedChanged" AutoPostBack="true" />
						</div>
						<div class="Clear"></div>
					</div>
                    <asp:Panel ID="pnlEmailForm" runat="server" SkinID="Default" Visible="false">                
                        <div class="Row">                
						    <div class="Label Medium">E-Mail Template<span class="Required">*</span>:</div>
						    <div class="Control">
                                <asp:DropDownList ID="ddlEmailTemplates" runat="server">
                                    <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                                </asp:DropDownList>
						    </div>
						    <div class="Clear"></div>
					    </div>
                        <asp:Panel ID="pnlShippingAddresses" runat="server" SkinID="Default">
                            <div class="Row">                
						        <div class="Label Medium">Shipping Addresses:</div>
						        <div class="Control">
                                    <table style="width: 800px;">
                                        <tr>
                                            <th style="text-align: left; width: 25%;"><u>Wearer</u></th>
                                            <th style="text-align: left; width: 25%;"><u>Location</u></th>
                                            <th style="text-align: left; width: 25%;"><u>Account</u></th>
                                            <th style="text-align: left; width: 25%;"><u>Other</u></th>
                                        </tr>
                                        <tr>
                                            <td style="vertical-align: top; width: 25%;">
                                                <asp:CheckBoxList ID="chkbxlstWearerAddresses" runat="server"></asp:CheckBoxList>
                                                <asp:Label ID="lblWearerAddressNotAvailable" runat="server" Text="Not Available"></asp:Label>
                                            </td>
                                            <td style="vertical-align: top; width: 25%;">
                                                <asp:CheckBoxList ID="chkbxlstLocationAddresses" runat="server"></asp:CheckBoxList>
                                                <asp:Label ID="lblLocationAddressNotAvailable" runat="server" Text="Not Available"></asp:Label>
                                            </td>
                                            <td style="vertical-align: top; width: 25%;">
                                                <asp:CheckBoxList ID="chkbxlstAccountAddresses" runat="server"></asp:CheckBoxList>
                                                <asp:Label ID="lblAccountAddressNotAvailable" runat="server" Text="Not Available"></asp:Label>
                                            </td>
                                            <td style="vertical-align: top; width: 25%;">
                                                <asp:CheckBox ID="chkbxOtherAddress" runat="server"></asp:CheckBox>
                                                <asp:TextBox ID="txtOtherAddress" runat="server" TextMode="MultiLine" Width="175px" Rows="5" MaxLength="255"></asp:TextBox>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                                <div class="Clear"></div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlEmailAddresses" runat="server" SkinID="Default">
                            <div class="Row">                
						        <div class="Label Medium">E-Mail Addresses<span class="Required">*</span>:</div>
						        <div class="Control">
                                    <table style="width: 800px;">
                                        <tr>
                                            <th style="text-align: left; width: 25%;" colspan="2"><u>Wearer</u></th>
                                            <th style="text-align: left; width: 25%;" colspan="2"><u>Location Admin.</u></th>
                                            <th style="text-align: left; width: 25%;" colspan="2"><u>Account Admin.</u></th>
                                            <th style="text-align: left; width: 25%;" colspan="2"><u>Other</u></th>
                                        </tr>
                                        <tr>
                                            <td style="vertical-align: top;">
                                                <asp:CheckBoxList ID="chkbxlstWearerEmailAddresses" runat="server"></asp:CheckBoxList>
                                            </td>
                                            <td style="vertical-align: top;">
                                                <asp:Label ID="lblWearerEmailsNotAvailable" runat="server" Text="Not Available"></asp:Label>
                                            </td>
                                            <td style="vertical-align: top;">
                                                <asp:CheckBoxList ID="chkbxlstLocationAdminEmailAddresses" runat="server"></asp:CheckBoxList>
                                            </td>
                                            <td style="vertical-align: top;">
                                                <asp:Label ID="lblLocationAdminEmailsNotAvailable" runat="server" Text="Not Available"></asp:Label>
                                            </td>
                                            <td style="vertical-align: top;">
                                                <asp:CheckBoxList ID="chkbxlstAccountAdminEmailAddresses" runat="server"></asp:CheckBoxList>
                                            </td>
                                            <td style="vertical-align: top;">
                                                <asp:Label ID="lblAccountAdminEmailsNotAvailable" runat="server" Text="Not Available"></asp:Label>
                                            </td>
                                            <td style="vertical-align: top;">
                                                <asp:CheckBox ID="chkbxOtherEmailAddress" runat="server" />
                                            </td>
                                            <td style="vertical-align: top;">
                                                <asp:TextBox ID="txtOtherEmailAddress" runat="server"></asp:TextBox>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                                <div class="Clear"></div>
                            </div>
                        </asp:Panel>
                        <div class="Row">
                            <div class="Label Medium">&nbsp;</div>
                            <div class="Control">
                                <table style="width: 800px;">
                                    <tr>
                                        <td>
                                            <asp:Label ID="lblEmailListFormattingInstructions" runat="server" Font-Bold="true" Font-Italic="true"><u>Note</u>: Please put a ';' after each e-mail address.</asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Button ID="btnTo" runat="server" Text="To:" OnClick="btnTo_OnClick" />
                                            &nbsp;
                                            <asp:TextBox ID="txtTo" runat="server" Columns="100"></asp:TextBox>
                                            &nbsp;
                                            <asp:Button ID="btnTo_ClearAll" runat="server" Text="Clear All" OnClick="btnTo_ClearAll_OnClick" />
                                            <br />
                                            <asp:Label ID="lblEmails" runat="server"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Button ID="btnCc" runat="server" Text="Cc:" OnClick="btnCc_OnClick" />
                                            &nbsp;
                                            <asp:TextBox ID="txtCc" runat="server" Columns="100"></asp:TextBox>
                                            &nbsp;
                                            <asp:Button ID="btnCc_ClearAll" runat="server" Text="Clear All" OnClick="btnCc_ClearAll_OnClick" />
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <div class="Clear"></div>
                        </div>
                    </asp:Panel>
                    <div class="Footer">
                        <span class="Required">*</span> Indicates a required field.
                    </div>
				</div>
                <asp:Button ID="btnCompleteReviewProcess" runat="server" Text="Complete Review" CssClass="OButton" OnClick="btnCompleteReviewProcess_Click" style="display: none;" />
                <asp:Button ID="btnCancel_CRP" runat="server" Text="Cancel" CssClass="OButton" OnClick="btnCancel_CRP_Click" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>
    <div id="divMainContent"> 
        <%---------------------------------- BEGIN MAIN PAGE CONTENT ------------------------------------------------------%>        
        <asp:UpdatePanel ID="updtpnlMainContent" runat="server">	
            <ContentTemplate>     
                <asp:HiddenField ID="hdnfldAccountID" runat="server" Value="" />
                <input type="hidden" id="serialNumber" value="<%= serialNumber %>" />
                <input type="hidden" id="rID" value="<%= rID.ToString() %>" />
                <input type="hidden" id="startDate" value="<%= startDate.ToString() %>" />
                <input type="hidden" id="endDate" value="<%= endDate.ToString() %>" />
                
                <div class="FormError" id="errors" runat="server" visible="false">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorMsg" runat="server">An error was encountered.</span></p>
                </div>
                <%--"BADGE SERIAL #: [SerialNo]--%>
                <div id="divBadgeSerialNo" runat="server">
                    <h2>Badge Serial #:&nbsp;<asp:Label ID="lblBadgeSerialNumber" runat="server" CssClass="Size Small"></asp:Label></h2>
                </div>
                <%--END--%>
                <%--TAB CONTAINERS SECTION - HEADER--%>
                <div id="divTabsContainerMain">
                    <div id="tabsContainer">
                        <ul>
                            <li><a href="#Account_tab" id="Tab2" runat="server" tabindex="1">Account</a></li>
                            <li><a href="#Baseline_tab" id="Tab1" runat="server" tabindex="0">Baseline</a></li>
                            <li><a href="#Reads_tab" id="Tab3" runat="server" tabindex="2">Reads</a></li>
                            <li><a href="#Graphs_tab" id="Tab4" runat="server" tabindex="3">Graphs</a></li>
                            <li><a href="#TechnicalNotes_tab" id="Tab5" runat="server" tabindex="4">Technical Notes</a></li>
                            <li><a href="#Review_tab" id="Tab6" runat="server" tabindex="5">Review</a></li>
                            <li><a href="#Exceptions_tab" id="Tab7" runat="server" tabindex="6">Exceptions</a></li>
                            <li><a href="#Motions_tab" id="Tab8" runat="server" tabindex="7">Motions</a></li>
                        </ul>
                        <%--ACCOUNTS TAB--%>
                        <div id="Account_tab">
                            <asp:UpdatePanel ID="updtpnlAccountTab" runat="server">
                                <ContentTemplate>
                                    <asp:Panel ID="pnlAccounts" runat="server" SkinID="Default">
                                        <telerik:RadGrid ID="rgAccounts_Parent" runat="server"
                                        SkinID="Default" CssClass="OTable"
                                        AllowMultiRowSelection="false"
                                        AutoGenerateColumns="true"
                                        AllowPaging="false"
                                        AllowSorting="false"
                                        AllowFilteringByColumn="false"
                                        ShowStatusBar="true"
                                        EnableLinqExpressions="false" 
                                        OnNeedDataSource="rgAccounts_Parent_NeedDataSource"
                                        OnItemCommand="rgAccounts_Parent_ItemCommand" 
                                        OnItemCreated="rgAccounts_Parent_ItemCreated" 
                                        Width="1050px">
                                            <PagerStyle Mode="NumericPages" />
                                            <GroupingSettings CaseSensitive="false" />
                                            <ClientSettings EnableRowHoverStyle="true">
                                                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                                <Selecting AllowRowSelect="true" />
                                            </ClientSettings>
                                            <MasterTableView DataKeyNames="AccountID" TableLayout="Fixed" HierarchyLoadMode="Client" AllowMultiColumnSorting="false" Width="100%">
                                                <NestedViewTemplate>
                                                    <asp:Panel runat="server" ID="InnerContainer" Visible="false">
                                                        <telerik:RadGrid ID="rgAccounts_Child" runat="server"
                                                        SkinID="Default" CssClass="OTable" 
                                                        AllowPaging="true" PageSize="25"
                                                        AllowSorting="true"
                                                        AllowFilteringByColumn="true"
                                                        OnNeedDataSource="rgAccounts_Child_NeedDataSource"   
                                                        MasterTableView-PageSize="20"
                                                        AutoGenerateColumns="false">
                                                            <ClientSettings EnableRowHoverStyle="true">
                                                                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                                                <Selecting AllowRowSelect="true" />
                                                                <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                                            </ClientSettings>
                                                            <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="false" />
                                                            <PagerStyle Mode="NumericPages" />
                                                            <GroupingSettings CaseSensitive="false" />
                                                            <MasterTableView AllowMultiColumnSorting="True" TableLayout="Fixed" DataKeyNames="AccountID">
                                                                <Columns>
                                                                    <telerik:GridTemplateColumn HeaderText="Account ID" UniqueName="AccountID"  Visible="false" SortExpression="AccountID">
                                                                      <ItemTemplate>                                                                          
                                                                          <%# (Eval("GDSAccount") == DBNull.Value ? Eval("AccountID").ToString() : Eval("GDSAccount")) %>                                                                          
                                                                      </ItemTemplate>
                                                                    </telerik:GridTemplateColumn>
                                                                    <telerik:GridHyperLinkColumn HeaderText="Serial No." DataTextField="SerialNumber" 
                                                                    ItemStyle-HorizontalAlign="Left" UniqueName="SerialNumber" HeaderStyle-Font-Bold="false" 
                                                                    HeaderStyle-Wrap="false" DataNavigateUrlFields="SerialNumber" Target="_blank" 
                                                                    DataNavigateUrlFormatString="BadgeReview.aspx?SerialNo={0}&RID=0" SortExpression="SerialNumber"
                                                                    CurrentFilterFunction="Contains" AutoPostBackOnFilter="true">
                                                                        <ItemStyle Wrap="false" Font-Underline="true" />
                                                                    </telerik:GridHyperLinkColumn>
                                                                    <telerik:GridHyperLinkColumn HeaderText="Full Name" DataTextField="FullName" SortExpression="FullName" 
                                                                    ItemStyle-HorizontalAlign="Left" UniqueName="FullName" HeaderStyle-Font-Bold="false" 
                                                                    HeaderStyle-Wrap="false" DataNavigateUrlFields="AccountID, UserID" Target="_blank"
                                                                    DataNavigateUrlFormatString="~/InformationFinder/Details/UserMaintenance.aspx?accountID={0}&userID={1}"
                                                                    CurrentFilterFunction="Contains" AutoPostBackOnFilter="true">
                                                                        <ItemStyle Wrap="false" Font-Underline="true" />
                                                                    </telerik:GridHyperLinkColumn>
                                                                    <telerik:GridBoundColumn DataField="Location" HeaderText="Location" SortExpression="Location" 
                                                                    UniqueName="Location" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true">
                                                                        <ItemStyle Wrap="false" />
                                                                    </telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="Color" HeaderText="Color" SortExpression="Color" 
                                                                    UniqueName="Color" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true">
                                                                        <ItemStyle Wrap="false" />
                                                                    </telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="Initialized" HeaderText="Initialized" SortExpression="Initialized" 
                                                                    UniqueName="Initialized" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true">
                                                                        <ItemStyle Wrap="false" />
                                                                    </telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="Active" HeaderText="Active" SortExpression="Active" 
                                                                    UniqueName="Active" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true">
                                                                        <ItemStyle Wrap="false" />
                                                                    </telerik:GridBoundColumn>
                                                                </Columns>
                                                            </MasterTableView>
                                                        </telerik:RadGrid>
                                                    </asp:Panel>
                                                </NestedViewTemplate>
                                            </MasterTableView>
                                        </telerik:RadGrid>
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <%--END--%>

                        <%--BASELINE TAB--%>
                        <div id="Baseline_tab">
                            <asp:UpdatePanel ID="updtpnlBaseline" runat="server">
                                <ContentTemplate>
                                    <div id="divBaseline" runat="server" class="JoinTable">
                                        <table style="width: 100%; border: 0px none; padding: 0px 0px 0px 0px; margin: 0px 0px 0px 0px;">
                                            <tr>
                                                <%--DL/DH SENSITIVITIES & COEFFICIENTS--%>
                                                <td style="width: 300px; vertical-align: top;">
                                                    <div class="OForm">
                                                        <%--ROW 1--%>
                                                        <div class="Row">
                                                            <div class="Label RightAlign">DL Sens.:</div>
                                                            <div class="LabelValue RightAlign" style="width: 100px;">
                                                                <asp:Label ID="lblDLSensitivity" runat="server"></asp:Label>
                                                            </div>
                                                            <div class="Clear"></div>
                                                            <div class="Label RightAlign">DH Sens.:</div>
                                                            <div class="LabelValue RightAlign" style="width: 100px;">
                                                                <asp:Label ID="lblDHSensitivity" runat="server"></asp:Label>
                                                            </div>
                                                            <div class="Clear"></div>
                                                            <div class="Label RightAlign">DLT Coeff. BL 1 and 2:</div>
                                                            <div class="LabelValue RightAlign" style="width: 100px;">
                                                                <asp:Label ID="lblDLTCoefficientBL1and2" runat="server"></asp:Label>
                                                            </div>
                                                            <div class="Clear"></div>
                                                            <div class="Label RightAlign">DLT Coeff. BL 2 and 3:</div>
                                                            <div class="LabelValue RightAlign" style="width: 100px;">
                                                                <asp:Label ID="lblDLTCoefficientBL2and3" runat="server"></asp:Label>
                                                            </div>
                                                            <div class="Clear"></div>
                                                            <div class="Label RightAlign">DHT Coeff. BL 1 and 2:</div>
                                                            <div class="LabelValue RightAlign" style="width: 100px;">
                                                                <asp:Label ID="lblDHTCoefficientBL1and2" runat="server"></asp:Label>
                                                            </div>
                                                            <div class="Clear"></div>
                                                            <div class="Label RightAlign">DHT Coeff. BL 2 and 3:</div>
                                                            <div class="LabelValue RightAlign" style="width: 100px;">
                                                                <asp:Label ID="lblDHTCoefficientBL2and3" runat="server"></asp:Label>
                                                            </div>
                                                            <div class="Clear"></div>
                                                            <div class="Label RightAlign">Age of Badge:</div>
                                                            <div class="LabelValue RightAlign" style="width: 100px;">
                                                                <asp:Label ID="lblAgeOfBadge" runat="server"></asp:Label>
                                                            </div>
                                                            <div class="Clear"></div>
                                                        </div>
                                                        <%--END--%>
                                                    </div>
                                                </td>
                                                <%--END--%>
                                                <%--BASELINE RADGRID--%>
                                                <td>
                                                    <telerik:RadGrid ID="rgBaseline" runat="server"
                                                    Width="100%" AllowSorting="false" GridLines="None"
                                                    PageSize="5" AllowPaging="false" AllowMultiRowSelection="false">
                                                        <MasterTableView Width="100%" Summary="Basline Information">
                                                        </MasterTableView>
                                                    </telerik:RadGrid>
                                                </td>
                                                <%--END--%>
                                            </tr>
                                            <tr><td colspan="2"><hr /></td></tr>
                                            <tr>
                                                <td colspan="2">
                                                    <%--BASELINE TEMPERATURE CALIBRATION CHART--%>
                                                    <div>
                                                        <canvas id="baselineTemperatureCalibrationChart"></canvas>
                                                    </div>
                                                    <%--END--%>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>

                                    <div id="divVueBaseline" runat="server" class="JoinTable">
                                        <table style="width: 100%; border: 0px none; padding: 0px 0px 0px 0px; margin: 0px 0px 0px 0px;">
                                            <tr>
                                                <%--BL DAC--%>
                                                <td style="width: 100%; vertical-align: top;">
                                                    <div class="OForm">
                                                        <%--ROW 1--%>
                                                        <table class="Centered">
                                                            <tr class=" WiderPadding">
                                                                <td class="Row LabelSmall">Dac 0</td>
                                                                <td class="Row LabelSmall">Dac 1</td>
                                                                <td class="Row LabelSmall">Dac 2</td>
                                                                <td class="Row LabelSmall">Dac 3</td>
                                                                <td class="Row LabelSmall">Dac 4</td>                                                               
                                                            </tr>
                                                            <tr>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac0" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac1" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac2" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac3" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac4" runat="server"></asp:Label></td>
                                                            </tr>
                                                            <tr class=" WiderPadding" style="padding-top: 10px;">
                                                                <td class="Row LabelSmall">Dac 5:</td>
                                                                <td class="Row LabelSmall">Dac 6:</td>
                                                                <td class="Row LabelSmall">Dac 7:</td>
                                                                <td class="Row LabelSmall">Dac 8:</td>
                                                                <td class="Row LabelSmall">Dac 9:</td>
                                                            </tr>
                                                            <tr>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac5" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac6" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac7" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac8" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac9" runat="server"></asp:Label></td>
                                                            </tr>
                                                             <tr class=" WiderPadding" style="padding-top: 10px;">
                                                                <td class="Row LabelSmall">Dac 10:</td>
                                                                <td class="Row LabelSmall">Dac 11:</td>
                                                                <td class="Row LabelSmall">Dac 12:</td>
                                                                <td class="Row LabelSmall">Dac 13:</td>
                                                                <td class="Row LabelSmall">Dac 14:</td>
                                                            </tr>
                                                            <tr class="">
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac10" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac11" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac12" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac13" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac14" runat="server"></asp:Label></td>
                                                                          
                                                            </tr>
                                                            <tr class=" WiderPadding" style="padding-top: 10px;">
                                                                <td class="Row LabelSmall">Dac 15:</td>
                                                                <td class="Row LabelSmall">Dac 16:</td>
                                                                <td class="Row LabelSmall">Dac 17:</td>
                                                                <td class="Row LabelSmall">Dac 18:</td>
                                                                <td class="Row LabelSmall">Dac 19:</td>
                                                            </tr>
                                                            <tr class="">
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac15" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac16" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac17" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac18" runat="server"></asp:Label></td>
                                                                <td class="LabelSmallBold"><asp:Label ID="lblDac19" runat="server"></asp:Label></td>
                                                               
                                                            </tr>
                                                        </table>
                                                        <%--END--%>
                                                    </div>
                                                </td>
                                                <%--END--%>
 
                                            </tr>
                                            <tr>
                                               <%--BASELINE CAL COEFFICIENTS--%>
                                                <td>
                                                    <telerik:RadGrid ID="rbVueBlCalCoeff" runat="server"
                                                    Width="100%" AllowSorting="false" GridLines="None" AutoGenerateColumns="false"
                                                    PageSize="5" AllowPaging="false" AllowMultiRowSelection="false">
                                                        <MasterTableView Width="100%" Summary="Baseline Information">
                                                            <Columns>
                                                                <telerik:GridBoundColumn DataField="Channel" HeaderText="Channel"></telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="A" HeaderText="A" HeaderTooltip="Quadratic sensitivity coefficient"></telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="B" HeaderText="B" HeaderTooltip="Linear sensitivity coefficient"></telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="C" HeaderText="C" HeaderTooltip="Zero point offset"></telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="Km0" HeaderText="Km0" HeaderTooltip="Temperature compensation offset"></telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="Km1" HeaderText="Km1" HeaderTooltip="Temperature coefficient 1st degree"></telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="Km2" HeaderText="Km2" HeaderTooltip="Temperature coefficient 2nd degree"></telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="Trefm" HeaderText="Trefm" HeaderTooltip="Dose element reference temperature"></telerik:GridBoundColumn>
                                                                <telerik:GridBoundColumn DataField="Trefc" HeaderText="Trefc" HeaderTooltip="Capacitance element reference temperature"></telerik:GridBoundColumn>                                                                    
                                                            </Columns>
                                                        </MasterTableView>
                                                    </telerik:RadGrid>
                                                </td>
                                                <%--END--%>
                                            </tr>
                                            <tr><hr /></td></tr>
                                            <tr>
                                                <td>
                                                    <%--BASELINE TEMPERATURE CALIBRATION CHART--%>
                                                   <%-- <div id="baselineTemperatureCalibrationChart"></div>--%>
                                                    <%--END--%>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <%--END--%>
                        
                        <%--READS TAB--%>
                        <div id="Reads_tab">
                            <asp:UpdatePanel ID="updtpnlReadsTab" runat="server">
                                <ContentTemplate>
                                    <asp:Panel ID="pnlReads" runat="server" SkinID="Default">
                                        <telerik:RadGrid ID="rgReads_Parent" runat="server"
                                        SkinID="Default" CssClass="OTable"
                                        AllowMultiRowSelection="false"
                                        AutoGenerateColumns="false"
                                        AllowPaging="false"
                                        AllowSorting="false"
                                        AllowFilteringByColumn="false"
                                        ShowStatusBar="true"
                                        EnableLinqExpressions="false" 
                                        OnNeedDataSource="rgReads_Parent_NeedDataSource"
                                        OnItemCommand="rgReads_Parent_ItemCommand" 
                                        OnItemCreated="rgReads_Parent_ItemCreated"
                                        OnItemDataBound="rgReads_Parent_ItemDataBound"
                                        Width="1050px">
                                            <PagerStyle Mode="NumericPages" />
                                            <ClientSettings EnableRowHoverStyle="true">
                                                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                                <Selecting AllowRowSelect="true" />
                                            </ClientSettings>
                                            <MasterTableView DataKeyNames="AccountID, UserID" TableLayout="Fixed" HierarchyLoadMode="Client" AllowMultiColumnSorting="false" Width="100%">
                                                <Columns>
                                                    <telerik:GridTemplateColumn HeaderText="Account ID" UniqueName="AccountID" ItemStyle-Width="100px" HeaderStyle-Width="100px">
                                                        <ItemTemplate>                                                                          
                                                          <%# (Eval("GDSAccount") == DBNull.Value ? Eval("AccountID") : Eval("GDSAccount")) %>   
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn DataField="CompanyName" HeaderText="Company" AllowSorting="false" UniqueName="CompanyName" ItemStyle-Width="200px" HeaderStyle-Width="200px"></telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="FullName" HeaderText="User Name" AllowSorting="false" UniqueName="FullName" ItemStyle-Width="200px" HeaderStyle-Width="200px"></telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="UserID" HeaderText="User ID" AllowSorting="false" UniqueName="UserID" ItemStyle-Width="100px" HeaderStyle-Width="100px"></telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="Active" HeaderText="Active" AllowSorting="false" UniqueName="Active" ItemStyle-Width="100px" HeaderStyle-Width="100px"></telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="IsRIDUser" HeaderText=" " AllowSorting="false" UniqueName="IsRIDUser" Visible="false"></telerik:GridBoundColumn>
                                                </Columns>
                                                <NestedViewTemplate>
                                                    <asp:Panel ID="pnlInnerDataContainer" runat="server" Visible="false">
                                                        <telerik:RadGrid ID="rgReads_Child" runat="server"
                                                            SkinID="Default" CssClass="OTable"
                                                            AllowPaging="false"
                                                            AllowSorting="true"
                                                            AllowFilteringByColumn="true"
                                                            AutoGenerateColumns="false"
                                                            OnNeedDataSource="rgReads_Child_NeedDataSource"
                                                            OnPreRender="rgReads_Child_PreRender"
                                                            OnItemDataBound="rgReads_Child_ItemDataBound"
                                                            OnItemCreated="rgReads_Child_ItemCreated"
                                                            >
                                                            <ClientSettings EnableRowHoverStyle="true">
                                                                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                                                <Selecting AllowRowSelect="true" />
                                                                <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                                            </ClientSettings>
                                                            <FilterMenu OnClientShowing="MenuShowing" />
                                                            <GroupingSettings CaseSensitive="false" />
                                                            <MasterTableView AllowMultiColumnSorting="true" TableLayout="Fixed" DataKeyNames="AccountID" ShowFooter="true">
                                                                <Columns>
                                                                    <telerik:GridTemplateColumn HeaderText="Account ID" UniqueName="AccountID" Visible="false">
                                                                        <ItemTemplate>                                                                          
                                                                          <%# (Eval("GDSAccount") == DBNull.Value ? Eval("AccountID") : Eval("GDSAccount")) %>   
                                                                        </ItemTemplate>
                                                                    </telerik:GridTemplateColumn>
                                                                    <telerik:GridBoundColumn DataField="RID" HeaderText="RID" HeaderStyle-Width="80" SortExpression="RID" UniqueName="RID" 
                                                                    AllowFiltering="false" AllowSorting="true"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="ReadType" HeaderText="Read Type" SortExpression="ReadType" AllowFiltering="false"
                                                                        UniqueName="ReadType" Aggregate="Count" FooterText="Total: " >
                                                                    </telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="CreatedDate" HeaderText="Exp. Date" SortExpression="CreatedDate" UniqueName="CreatedDate" 
                                                                    DataType="System.DateTime" AllowFiltering="false" HeaderStyle-Width="150px" ItemStyle-Width="150px"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="InitialRead" HeaderText="Initial Read" SortExpression="InitialRead" ShowFilterIcon="true"
                                                                    UniqueName="InitialRead" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" FilterControlWidth="25px">
                                                                        <ItemStyle Wrap="false" />
                                                                    </telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="HasAnomaly" HeaderText="Anomaly" SortExpression="HasAnomaly" ShowFilterIcon="true" 
                                                                    UniqueName="HasAnomaly" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" FilterControlWidth="25px">
                                                                        <ItemStyle Wrap="false" />
                                                                    </telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="Error" HeaderText="Error" SortExpression="Error" UniqueName="Error" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DL" HeaderText="DL" SortExpression="DL" UniqueName="DL" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DLT" HeaderText="DLT" SortExpression="DLT" UniqueName="DLT" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DH" HeaderText="DH" SortExpression="DH" UniqueName="DH" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DHT" HeaderText="DHT" SortExpression="DHT" UniqueName="DHT" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <%--DLT-DHT--%>
                                                                    <telerik:GridBoundColumn DataField="DLTMinusDHT" HeaderText="DLT-DHT" SortExpression="DLTMinusDHT" UniqueName="DLTMinusDHT" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <%--END--%>
                                                                    <telerik:GridBoundColumn DataField="DLDCalc" HeaderText="DLD Calc." SortExpression="DLDCalc" UniqueName="DLDCalc" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="BkgdExp" HeaderText="Bkgd. Exp." SortExpression="BkgdExp" UniqueName="BkgdExp" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="CumulDose" HeaderText="Cum. Dose" SortExpression="CumulDose" UniqueName="CumulDose" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    
                                                                    <telerik:GridBoundColumn DataField="DLDose" HeaderText="DL Dose" SortExpression="DLDose" UniqueName="DLDose" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                </Columns>
                                                            </MasterTableView>
                                                        </telerik:RadGrid>
                                                     </asp:Panel>
                                                    <asp:Panel ID="pnlInnerDataContainerIDVue" runat="server" Visible="false">
                                                        <telerik:RadGrid ID="rgReads_Child_IDVue" runat="server"
                                                            SkinID="Default" CssClass="OTable"
                                                            AllowPaging="false"
                                                            AllowSorting="true"
                                                            AllowFilteringByColumn="true"
                                                            AutoGenerateColumns="false"
                                                            OnNeedDataSource="rgReads_Child_IDVue_NeedDataSource"
                                                            OnItemDataBound="rgReads_Child_IDVue_ItemDataBound"
                                                            OnPreRender="rgReads_Child_IDVue_PreRender"
                                                            OnItemCreated="rgReads_Child_IDVue_ItemCreated"
                                                            >
                                                            <ClientSettings EnableRowHoverStyle="true">
                                                                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                                                <Selecting AllowRowSelect="true" />
                                                                <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                                            </ClientSettings>
                                                            <FilterMenu OnClientShowing="MenuShowing" />
                                                            <GroupingSettings CaseSensitive="false" />
                                                            <MasterTableView AllowMultiColumnSorting="true" TableLayout="Fixed" DataKeyNames="AccountID" ShowFooter="true">
                                                                <Columns>
                                                                    <telerik:GridTemplateColumn HeaderText="AccountID" UniqueName="AccountID"  Visible="false" SortExpression="AccountID">
                                                                      <ItemTemplate>                                                                          
                                                                         <%# (Eval("GDSAccount") == DBNull.Value ? Eval("AccountID") : Eval("GDSAccount")) %>
                                                                         <%-- <%# Eval("AccountID") %>--%>
                                                                      </ItemTemplate>
                                                                    </telerik:GridTemplateColumn>
                                                                    <%--<telerik:GridBoundColumn DataField="RID" HeaderText="RID" SortExpression="RID" UniqueName="RID" 
                                                                    AllowFiltering="false" AllowSorting="true" HeaderStyle-Width="80"></telerik:GridBoundColumn>  --%>   
                                                                    <telerik:GridTemplateColumn HeaderText="RID" UniqueName="RID"  HeaderStyle-Width="70" SortExpression="RID" AllowFiltering="false">
                                                                      <ItemTemplate>
                                                                          
                                                                          <a href="#" class="rid-link" data-value="<%# Eval("RID") %>">
                                                                              <%# Eval("RID") %>
                                                                          </a>
                                                                      </ItemTemplate>
                                                                    </telerik:GridTemplateColumn>
                                                                    <telerik:GridBoundColumn DataField="ReadType" HeaderText="Read Type" SortExpression="ReadType" UniqueName="ReadType" AllowFiltering="false"
                                                                        Aggregate="Count" FooterText="Total: " >
                                                                    </telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="CreatedDate" HeaderText="Exp. Date" SortExpression="CreatedDate" UniqueName="CreatedDate" 
                                                                    DataType="System.DateTime" AllowFiltering="false" HeaderStyle-Width="145px" ItemStyle-Width="145px"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="BatteryPercent" HeaderText="Main Battery MV" SortExpression="BatteryPercent" UniqueName="MainBatteryV" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DoseBatteryPercent" HeaderText="Dose Battery MV" SortExpression="DoseBatteryPercent" UniqueName="DoseBatteryV" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    
                                                                    <telerik:GridBoundColumn DataField="InitRead" HeaderText="Initial Read" HeaderStyle-Width="55px" SortExpression="InitRead" ShowFilterIcon="true"
                                                                    UniqueName="InitialRead" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" FilterControlWidth="25px">
                                                                        <ItemStyle Wrap="false" />
                                                                    </telerik:GridBoundColumn>
                                                                    
                                                                    <telerik:GridBoundColumn DataField="HasAnomaly" HeaderText="Anomaly" SortExpression="HasAnomaly" ShowFilterIcon="true" 
                                                                    UniqueName="HasAnomaly" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" FilterControlWidth="25px">
                                                                        <ItemStyle Wrap="false" />
                                                                    </telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="ErrorText" HeaderText="Exception" HeaderStyle-Width="50px" SortExpression="ErrorText" UniqueName="ErrorText" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    
                                                                    <telerik:GridBoundColumn DataField="DeepLowDose" HeaderText="DL Dose" HeaderStyle-Width="55px" SortExpression="DeepLowDose" UniqueName="DeepLowDose" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="CumulativeDose" HeaderText="Cum. Dose" HeaderStyle-Width="55px" SortExpression="CumulativeDose" UniqueName="CumulativeDose" AllowFiltering="false" DataFormatString="{0:0}"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DL_Rm" HeaderText="DL Rm" SortExpression="DL_Rm" UniqueName="DL_Rm" AllowFiltering="false" HeaderTooltip="DL Raw Dose Digits"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DL_Tm" HeaderText="DL Tm" SortExpression="DL_Tm" UniqueName="DL_Tm" AllowFiltering="false" HeaderTooltip="DL dose temp digits"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DL_Tc" HeaderText="DL Tc" HeaderStyle-Width="55px" SortExpression="DL_Tc" UniqueName="DL_Tc" HeaderTooltip="DL capacitance temp dose digits" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DL_DAC" HeaderText="DL DAC" HeaderStyle-Width="42px" SortExpression="DL_DAC" UniqueName="DL_DAC" HeaderTooltip="" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="TA_Pre" HeaderText="TA Pre" HeaderStyle-Width="55px" SortExpression="TA_Pre" UniqueName="TA_Pre" HeaderTooltip="Temp reading before dose measure" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="TA_Post" HeaderText="TA Post" SortExpression="TA_Post" UniqueName="TA_Post" HeaderTooltip="Temp reading after dose measure" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <%--
                                                                    <telerik:GridBoundColumn DataField="DeepHighDose" HeaderText="DH Dose" SortExpression="DeepHighDose" UniqueName="DeepHighDose" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DH_DAC" HeaderText="DH DAC" SortExpression="DH_DAC" UniqueName="DH_DAC" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DH_Rm" HeaderText="DH Rm" SortExpression="DH_Rm" UniqueName="DL_Tm" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DH_Tm" HeaderText="DH Tm" SortExpression="DH_Tm" UniqueName="DL_Tm" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DH_Tc" HeaderText="DH Tc" SortExpression="DH_Tc" UniqueName="DL_Tm" AllowFiltering="false"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="DeepLowDoseCalc" HeaderText="DLD Calc" SortExpression="DeepLowDoseCalc" UniqueName="DeepLowDoseCalc" AllowFiltering="false" DataFormatString="{0:0.00}"></telerik:GridBoundColumn>
                                                                    <telerik:GridBoundColumn DataField="BackgroundExposure" HeaderText="Bkgd. Exp." SortExpression="BackgroundExposure" UniqueName="BackgroundExposure" AllowFiltering="false" DataFormatString="{0:0.00}"></telerik:GridBoundColumn>
                                                                    --%>
                                                                </Columns>
                                                            </MasterTableView>
                                                        </telerik:RadGrid>
                                                    </asp:Panel>
                                                </NestedViewTemplate>
                                            </MasterTableView> 
                                        </telerik:RadGrid>
                                        <table class="OTable" style="width: 175px;">
                                            <tr class="Header" style="background-color: #D5702D;">
                                                <td>
                                                    <asp:Label ID="lblReadsRadGridLegend" runat="server" style="color: #FFFFFF;" Text="Legend" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <div class="input-color">
                                                        <asp:Label ID="lblAnomalies" runat="server" CssClass="lbltxt" Text="Anomalies"></asp:Label>
                                                        <div class="color-box" style="background-color: red;"></div>
                                                    </div>
                                                    <div class="input-color">
                                                        <asp:Label ID="lblGoodCustomerReads" runat="server" CssClass="lbltxt" Text="Good Customer Reads"></asp:Label>
                                                        <div class="color-box" style="background-color: blue;"></div>
                                                    </div>
                                                    <div class="input-color">
                                                        <asp:Label ID="lblInitializations" runat="server" CssClass="lbltxt" Text="Initializations"></asp:Label>
                                                        <div class="color-box" style="background-color: black;"></div>
                                                    </div>
                                                    <div class="input-color">
                                                        <asp:Label ID="lblModifications" runat="server" CssClass="lbltxt" Text="Modifications"></asp:Label>
                                                        <div class="color-box" style="background-color: green;"></div>
                                                    </div>
                                                    <div class="input-color">
                                                        <asp:Label ID="Label1" runat="server" CssClass="lbltxt" Text="Unknown"></asp:Label>
                                                        <div class="color-box" style="background-color: gray;"></div>
                                                    </div>
                                                </td>   
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <%--END--%>

                        <%--GRAPHS TAB--%>
                        <div id="Graphs_tab">
                            <asp:UpdatePanel ID="updtpnlGraphs" runat="server">
                                <ContentTemplate>
                                    <%--DOSE RANGE CHART--%>
                                    <div id="doseRangeChartContainer">
                                        <hr />
                                        <input type="radio" name="chartType_Dose" value="1" checked="checked"> Default Chart
                                        <input type="radio" name="chartType_Dose" value="2"> Exclude Initial Reads
                                        <input type="radio" name="chartType_Dose" value="3"> Exclude Anomalies
                                        <input type="radio" name="chartType_Dose" value="4"> Exclude Initial Reads & Anomalies
                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                                        <input type="checkbox" id="chartType_Dose_IncludeSoftReads" name="chartType_Dose_IncludeSoftReads"> Include Soft Reads
                                        <hr />
                                        <div>
                                            <canvas id="doseRangeChart"></canvas>
                                        </div>
                                    </div>
                                    
                                    <%--END--%>
                                    <%--ROUGH DOSE RANGE CHART--%>
                                    
                                    <div id="roughDoseRangeChartContainer">
                                        <hr />
                                        <input type="radio" name="chartType_Rough" value="1" checked="checked"> Default Chart
                                        <input type="radio" name="chartType_Rough" value="2"> Exclude Initial Reads
                                        <input type="radio" name="chartType_Rough" value="3"> Exclude Anomalies
                                        <input type="radio" name="chartType_Rough" value="4"> Exclude Initial Reads & Anomalies
                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                                        <input type="checkbox" id="chartType_Rough_IncludeSoftReads" name="chartType_Rough_IncludeSoftReads"> Include Soft Reads
                                        <hr />
                                        <div>
                                            <canvas id="roughDoseRangeChart"></canvas>
                                        </div>
                                    </div>
                                    
                                    <%--END--%>
                                    <%--TEMPERATURE RANGE CHART--%>
                                    <div id="temperatureRangeChartContainer">                                        
                                        <hr />
                                        <label class="temprange-chart-option">
                                            <input type="radio" name="chartType_Temp" class="default-chart-option" value="1" checked="checked"> Default Chart
                                        </label>
                                        <label class="temprange-chart-option">
                                            <input type="radio" name="chartType_Temp" value="2"> Exclude Initial Reads
                                        </label>
                                        <label class="temprange-chart-option">
                                            <input type="radio" name="chartType_Temp" value="3"> Exclude Anomalies
                                        </label>
                                        <label class="temprange-chart-option">
                                            <input type="radio" name="chartType_Temp" value="4"> Exclude Initial Reads & Anomalies
                                        </label>                                        
                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                                        <label class="temprange-chart-option">
                                            <input type="checkbox" id="chartType_Temp_IncludeSoftReads" name="chartType_Temp_IncludeSoftReads"> Include Soft Reads
                                        </label>                                        
                                        <hr />
                                        <div>
                                            <canvas id="temperatureRangeChart"></canvas>
                                        </div>
                                    </div>
                                    
                                    <%--END--%>
                                    <%--TEMPERATURE DIFFERENCES CHART--%>
                                    <div id="temperatureDifferencesChartContainer">
                                         <hr />
                                        <input type="radio" name="chartType_Diff" value="1" checked="checked"> Default Chart
                                        <input type="radio" name="chartType_Diff" value="2"> Exclude Initial Reads
                                        <input type="radio" name="chartType_Diff" value="3"> Exclude Anomalies
                                        <input type="radio" name="chartType_Diff" value="4"> Exclude Initial Reads & Anomalies
                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                                        <input type="checkbox" id="chartType_Diff_IncludeSoftReads" name="chartType_Diff_IncludeSoftReads"> Include Soft Reads
                                        <hr />
                                        <div>
                                            <canvas id="temperatureDifferencesChart"></canvas>
                                        </div>
                                    </div>
                                    
                                    <%--END--%>
                                    <%--DLT-DHT CHART--%>
                                    <div id="deepLowTempMinusDeepHighTempChartContainer">                                        
                                        <hr />
                                        <input type="radio" name="chartType_DLTDHT" value="1" checked="checked"> Default Chart
                                        <input type="radio" name="chartType_DLTDHT" value="2"> Exclude Initial Reads
                                        <input type="radio" name="chartType_DLTDHT" value="3"> Exclude Anomalies
                                        <input type="radio" name="chartType_DLTDHT" value="4"> Exclude Initial Reads & Anomalies
                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                                        <input type="checkbox" id="chartType_DLTDHT_IncludeSoftReads" name="chartType_DLTDHT_IncludeSoftReads"> Include Soft Reads
                                        <hr />
                                        <div>
                                            <canvas id="deepLowTempMinusDeepHighTempChart"></canvas>
                                        </div>
                                    </div>
                                    
                                    <%--END--%>
                                    <br />
                                    <%--CUMULATIVE DOSE CHART--%>
                                    <div id="cummulativeDoseChartContainer">
                                        <hr />
                                        <input type="radio" name="chartType_Cumul" value="1" checked="checked"> Default Chart
                                        <input type="radio" name="chartType_Cumul" value="2"> Exclude Initial Reads
                                        <input type="radio" name="chartType_Cumul" value="3"> Exclude Anomalies
                                        <input type="radio" name="chartType_Cumul" value="4"> Exclude Initial Reads & Anomalies
                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                                        <input type="checkbox" id="chartType_Cumul_IncludeSoftReads" name="chartType_Cumul_IncludeSoftReads"> Include Soft Reads
                                        <hr />
                                        <div>
                                            <canvas id="cumulativeDoseChart"></canvas>
                                        </div>
                                    </div>
                                    
                                    <%--END--%>
                                    <br />
                                    <%--DLT-DHT CHART--%>
                                    <div id="batteryPercentChartContainer">                                        
                                        <hr />
                                        <input type="radio" name="chartType_BatteryPercent" value="1" checked="checked"> Default Chart
                                        <input type="radio" name="chartType_BatteryPercent" value="2"> Exclude Initial Reads
                                        <input type="radio" name="chartType_BatteryPercent" value="3"> Exclude Anomalies
                                        <input type="radio" name="chartType_BatteryPercent" value="4"> Exclude Initial Reads & Anomalies
                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                                        <input type="checkbox" id="chartType_BatteryPercent_IncludeSoftReads" name="chartType_BatteryPercent_IncludeSoftReads"> Include Soft Reads
                                        <hr />
                                        <div>
                                            <canvas id="batteryPercentChart"></canvas>
                                        </div>
                                    </div>
                                    
                                    <%--END--%>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <%--END--%>

                        <%--TECHNICAL NOTES TAB--%>
                        <div id="TechnicalNotes_tab">
                            <asp:UpdatePanel ID="updtpnlTechnicalNotes" runat="server" UpdateMode="Conditional">
					                <Triggers>
						                <asp:AsyncPostBackTrigger ControlID="lnkbtnCreateNewTechnicalNote" EventName="Click" />
					                </Triggers>
                                <ContentTemplate>
                                    <div class="FormMessage" id="divFormMessage_TechnicalNotes" runat="server" visible="false" style="text-align: left;">
	                                    <p><span class="MessageIcon"></span>
	                                    <strong>Message:</strong>&nbsp;<span id="spnFormMessage_TechnicalNotes" runat="server">There are no previous review records for this badge.</span></p>
                                    </div>
						            <div class="OToolbar JoinTable" id="CreateNote" runat ="server">
							            <ul>
								            <li>
									            <asp:LinkButton ID="lnkbtnCreateNewTechnicalNote" runat="server"  
									            CommandName="NewNote" CommandArgument="" CssClass="Icon Add"  
									            OnClick="lnkbtnCreateNewTechnicalNote_Click" >Create Technical Note</asp:LinkButton>
								            </li>
							            </ul>
						            </div>
                                    <%--TECHNICAL NOTES RADGRID--%>   							   
					                <asp:Panel ID="pnlTechnicalNotes" runat="server" SkinID="Default">
                                        <telerik:RadGrid ID="rgTechnicalNotes" runat="server"
                                        SkinID="Default" CssClass="OTable" 
                                        AllowMultiRowSelection="false"
                                        AutoGenerateColumns="false"
                                        AllowPaging="true"
                                        AllowSorting="true"
                                        AllowFilteringByColumn="true"
                                        ShowStatusBar="true"
                                        EnableLinqExpressions="false" 
                                        OnNeedDataSource="rgTechnicalNotes_NeedDataSource"
                                        OnItemCommand="rgTechnicalNotes_OnItemCommand"
                                        PageSize="20" Width="99.8%">
                                            <PagerStyle Mode="NumericPages" />
                                            <GroupingSettings CaseSensitive="false" />
                                            <ClientSettings EnableRowHoverStyle="true">
                                                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                                <Selecting AllowRowSelect="true" />
                                                <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                            </ClientSettings>
                                            <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
                                            <MasterTableView DataKeyNames="DeviceID" TableLayout="Fixed" AllowMultiColumnSorting="false" AutoGenerateColumns="false">
                                                <Columns>
                                                    <%--RID--%>
                                                    <telerik:GridBoundColumn DataField="RID" UniqueName="RID" Visible="false" />
                                                    <%--END--%>
                                                    <%--DEVICE ID--%>
                                                    <telerik:GridBoundColumn DataField="DeviceID" UniqueName="DeviceID" Visible="false" />
                                                    <%--END--%>
                                                    <%--CREATED DATE--%>
                                                    <telerik:GridDateTimeColumn DataField="CreatedDate" HeaderText="Date" UniqueName="CreatedDate"
                                                    AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="150px" FilterControlWidth="100px"
                                                    SortExpression="CreatedDate" />
                                                    <%--END--%>
                                                    <%--CREATED BY--%>
                                                    <telerik:GridBoundColumn DataField="CreatedBy" HeaderText="Created By" SortExpression="CreatedBy" ItemStyle-Width="125px" 
                                                    UniqueName="CreatedBy" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" HeaderStyle-Width="150px" />
                                                    <%--END--%>
                                                    <%--TECHNICAL NOTES--%>
                                                    <telerik:GridBoundColumn DataField="NoteText" HeaderText="Technical Notes" SortExpression="NoteText" 
                                                    UniqueName="NoteText" CurrentFilterFunction="Contains" AutoPostBackOnFilter="true" FilterControlWidth="300px">
                                                        <ItemStyle HorizontalAlign="Left" Wrap="true" />
                                                        <HeaderStyle HorizontalAlign="Left" />
                                                    </telerik:GridBoundColumn>
                                                    <%--END--%>
                                                </Columns>
                                            </MasterTableView>
                                        </telerik:RadGrid>
                                    </asp:Panel>
                                    <%--END--%>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <%--END--%>

                        <%--REVIEW TAB--%>
                        <div id="Review_tab">
                            <asp:UpdatePanel ID="updtpnlCompelteReviewProcess" runat="server">
					                <Triggers>
						                <asp:AsyncPostBackTrigger ControlID="lnkbtnCompleteReviewProcess" EventName="Click" />
					                </Triggers>
                                <ContentTemplate>
                                    <div class="FormMessage" id="divFormMessage_Review" runat="server" visible="false" style="text-align: left;">
	                                    <p><span class="MessageIcon"></span>
	                                    <strong>Message:</strong>&nbsp;<span id="spnFormMessage_Review" runat="server">There are no previous review records for this badge.</span></p>
                                    </div>
						            <div class="OToolbar JoinTable" id="CompleteReviewProcess" runat ="server">
                                        <div class="Flex FlexRow PadRight-5">
                                            <ul class="FlexGrow">
								                <li>
									                <asp:LinkButton ID="lnkbtnCompleteReviewProcess" runat="server"  
									                CommandName="CompleteReview" CommandArgument="" CssClass="Icon Check"  
									                OnClick="lnkbtnCompleteReviewProcess_Click" >Complete Review Process</asp:LinkButton>
								                </li>
							                </ul>
                                            <asp:HyperLink ID="hyprlnkRecall" CssClass="btn btn-default" runat="server"                                                 
                                                Target="_blank" Text="Recall">                                    
                                            </asp:HyperLink>  
                                        </div>
						            </div>
                                    <%--PREVIOUS REVIEW DETAILS--%> 
                                    <asp:Panel ID="pnlBadgeReviewHistory" runat="server" SkinID="Default">
                                        <telerik:RadGrid ID="rgBadgeReviewHistory" runat="server"
                                        SkinID="Default" CssClass="OTable" 
                                        AllowMultiRowSelection="false"
                                        AutoGenerateColumns="false"
                                        AllowPaging="true"
                                        AllowSorting="true"
                                        AllowFilteringByColumn="true"
                                        ShowStatusBar="true"
                                        EnableLinqExpressions="false"
                                        OnNeedDataSource="rgBadgeReviewHistory_NeedDataSource" 
                                        OnItemCommand="rgBadgeReviewHistory_OnItemCommand"
                                        OnItemDataBound="rgBadgeReviewHistory_OnItemDataBound"
                                        PageSize="20" Width="99.8%">
                                            <PagerStyle Mode="NumericPages" />
                                            <GroupingSettings CaseSensitive="false" />
                                            <ClientSettings EnableRowHoverStyle="false">
                                                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                                <Selecting AllowRowSelect="true" />
                                                <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                            </ClientSettings>
                                            <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
                                            <MasterTableView DataKeyNames="DeviceID" TableLayout="Fixed" AllowMultiColumnSorting="true" AutoGenerateColumns="False">
                                                <Columns>
                                                    <%--RID--%>
                                                    <telerik:GridBoundColumn DataField="RID" UniqueName="RID" AllowFiltering="false" HeaderText="RID" 
                                                    HeaderStyle-Wrap="false" ItemStyle-Wrap="false" AllowSorting="false">
                                                        <ItemStyle HorizontalAlign="Center" />
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                    </telerik:GridBoundColumn>
                                                    <%--END--%>
                                                    <telerik:GridBoundColumn DataField="DeviceID" UniqueName="DeviceID" Visible="false" />
                                                    <telerik:GridBoundColumn DataField="SerialNumber" UniqueName="SerialNumber" Visible="false" />
                                                    <%--READ TYPE--%>
                                                    <telerik:GridBoundColumn DataField="ReadTypeName" UniqueName="ReadTypeName" HeaderText="Read Type" HeaderStyle-Width="110px" ItemStyle-Width="110px" EmptyDataText="Unknown">
                                                        <FilterTemplate>
                                                            <telerik:RadComboBox ID="rcbReadTypeName" DataSourceID="SQLDSReadTypeNames" DataTextField="ReadTypeName"
                                                            DataValueField="ReadTypeName" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("ReadTypeName").CurrentFilterValue %>'
                                                            runat="server" OnClientSelectedIndexChanged="ReadTypeNamesIndexChanged" Width="100px">
                                                                <Items>
                                                                    <telerik:RadComboBoxItem Text="-Select-" />
                                                                </Items>
                                                            </telerik:RadComboBox>
                                                            <telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
                                                                <script type="text/javascript">
                                                                    function ReadTypeNamesIndexChanged(sender, args) {
                                                                        var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                        tableView.filter("ReadTypeName", args.get_item().get_value(), "EqualTo");
                                                                    }
                                                                </script>
                                                            </telerik:RadScriptBlock>
                                                        </FilterTemplate>
                                                    </telerik:GridBoundColumn>
                                                    <%--END--%>
                                                    <%--DLT-DHT--%>
                                                    <%--<telerik:GridBoundColumn DataField="DLTMinusDHT" UniqueName="DLTMinusDHT" HeaderText="DLT-DHT" AllowFiltering="false" AllowSorting="false" />--%>
                                                    <%--END--%>
                                                    <%--READ DATE/CREATED DATE--%>
                                                    <telerik:GridDateTimeColumn DataField="CreatedDate" HeaderText="Exposure Date" UniqueName="CreatedDate"
                                                    AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="175px" FilterControlWidth="125px"
                                                    SortExpression="CreatedDate" />
                                                    <%--END--%>
                                                    <%--REVIEWED DATE--%>
                                                    <telerik:GridDateTimeColumn DataField="ReviewedDate" HeaderText=" Reviewed Date" UniqueName="ReviewedDate"
                                                    AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="175px" FilterControlWidth="125px"
                                                    SortExpression="ReviewedDate" />
                                                    <%--END--%>
                                                    <%--CONFIGURABLE VALUES--%>
                                                        <%--RECALL LIMIT--%>
                                                        <telerik:GridBoundColumn DataField="RecallLimit" UniqueName="RecallLimit" HeaderText="Recall" AllowFiltering="false" AllowSorting="false" />
                                                        <%--END--%>
                                                        <%--WATCHLIST LOW LIMIT--%>
                                                        <telerik:GridBoundColumn DataField="WatchlistLowLimit" UniqueName="WatchlistLowLimit" HeaderText="WL Low" AllowFiltering="false" AllowSorting="false" />
                                                        <%--END--%>
                                                        <%--WATCHLIST HIGH LIMIT--%>
                                                        <telerik:GridBoundColumn DataField="WatchlistHighLimit" UniqueName="WatchlistHighLimit" HeaderText="WL High" AllowFiltering="false" AllowSorting="false" />
                                                        <%--END--%>
                                                        <%--CUMULATIVE DOSE LIMIT--%>
                                                        <telerik:GridBoundColumn DataField="CumulativeDoseLimit" UniqueName="CumulativeDoseLimit" HeaderText="Cumul. Dose" AllowFiltering="false" AllowSorting="false" />
                                                        <%--END--%>
                                                        <%--EXPIRATION YEARS LIMIT--%>
                                                        <telerik:GridBoundColumn DataField="ExpirationYearsLimit" UniqueName="ExpirationYearsLimit" HeaderText="Exp. Yrs." AllowFiltering="false" AllowSorting="false" />
                                                        <%--END--%>
                                                    <%--END--%>
                                                    <%--FAILED TESTS--%>
                                                    <telerik:GridBoundColumn DataField="FailedTests" UniqueName="FailedTests" HeaderText="Failed Tests" HeaderStyle-Width="85px" ItemStyle-Width="85px" EmptyDataText="All Passed">
                                                        <FilterTemplate>
                                                            <telerik:RadComboBox ID="rcbFailedTests" DataSourceID="SQLDSAnalysisTypes" DataTextField="AnalysisTypeAbbrev"
                                                            DataValueField="AnalysisTypeAbbrev" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("FailedTests").CurrentFilterValue %>'
                                                            runat="server" OnClientSelectedIndexChanged="AnalysisTypeAbbrevsIndexChanged" Width="75px">
                                                                <Items>
                                                                    <telerik:RadComboBoxItem Text="-Select-" />
                                                                </Items>
                                                            </telerik:RadComboBox>
                                                            <telerik:RadScriptBlock ID="RadScriptBlock2" runat="server">
                                                                <script type="text/javascript">
                                                                    function AnalysisTypeAbbrevsIndexChanged(sender, args) {
                                                                        var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                        tableView.filter("FailedTests", args.get_item().get_value(), "Contains");
                                                                    }
                                                                </script>
                                                            </telerik:RadScriptBlock>
                                                        </FilterTemplate>
                                                    </telerik:GridBoundColumn>
                                                    <%--END--%>
                                                    <telerik:GridBoundColumn DataField="AnalysisActionTypeID" UniqueName="AnalysisActionTypeID" Visible="false"></telerik:GridBoundColumn>
                                                    <%--ACTION TAKEN--%>
                                                    <telerik:GridTemplateColumn DataField="AnalysisActionTypeID" UniqueName="AnalysisActionTypeID" HeaderText="Action Taken" HeaderStyle-Width="100px" ItemStyle-Width="100px"> 
                                                        <ItemTemplate>
                                                            <asp:Image ID="imgIcon" runat="server" ImageUrl='<%# Eval("IconURL") %>' />
                                                            &nbsp;
                                                            <asp:Hyperlink ID="hyprlnkRecallStatus" runat="server" NavigateUrl='<%# string.Format("ReturnAddNewDeviceRMA.aspx?SerialNo={0}&Reason=Fade", Eval("SerialNumber")) %>'
											                Visible="false" Target="_blank" Text="" /> 
                                                        </ItemTemplate>
                                                        <FilterTemplate>
                                                            <telerik:RadComboBox ID="rcbAnalysisActionTypeID" DataSourceID="SQLDSAnalysisActionTypes" DataTextField="AnalysisActionName"
                                                            DataValueField="AnalysisActionTypeID" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("AnalysisActionTypeID").CurrentFilterValue %>'
                                                            runat="server" OnClientSelectedIndexChanged="AnalysisActionTypesIndexChanged" Width="75px">
                                                                <Items>
                                                                    <telerik:RadComboBoxItem Text="-Select-" />
                                                                </Items>
                                                            </telerik:RadComboBox>
                                                            <telerik:RadScriptBlock ID="RadScriptBlock3" runat="server">
                                                                <script type="text/javascript">
                                                                    function AnalysisActionTypesIndexChanged(sender, args) {
                                                                        var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                                        tableView.filter("AnalysisActionTypeID", args.get_item().get_value(), "EqualTo");
                                                                    }
                                                                </script>
                                                            </telerik:RadScriptBlock>
                                                        </FilterTemplate> 
                                                    </telerik:GridTemplateColumn> 
                                                    <%--END--%>
                                                </Columns>
                                            </MasterTableView>
                                        </telerik:RadGrid>
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                            <asp:SqlDataSource ID="SQLDSAnalysisActionTypes" runat="server" 
                            ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
                            SelectCommand="SELECT [AnalysisActionTypeID], [AnalysisActionName] FROM [AnalysisActionTypes] ORDER BY [AnalysisActionTypeID] ASC">
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="SQLDSAnalysisTypes" runat="server"
                            ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
                            SelectCommand="SELECT [AnalysisTypeAbbrev] FROM [AnalysisTypes] WHERE [TestType] = 'R'"></asp:SqlDataSource>
                            <asp:SqlDataSource ID="SQLDSReadTypeNames" runat="server"
                            ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
                            SelectCommand="SELECT DISTINCT [ReadTypeName] FROM [ReadTypes] ORDER BY [ReadTypeName] ASC"></asp:SqlDataSource>
                        </div>
                        <%--END--%>

                         <%--EXCEPTIONS TAB--%>
                        <div id="Exceptions_tab" runat="server" ClientIDMode="Static">
                            <div>
                                <telerik:RadGrid ID="rgExceptions" runat="server"
                                    SkinID="Default" CssClass="OTable"
                                    AllowPaging="false"
                                    AllowFilteringByColumn="false" OnNeedDataSource="rgExceptions_NeedDataSource" 
                                    AutoGenerateColumns="false">
                                    <MasterTableView TableLayout="Fixed" DataKeyNames="SerialNo" CssClass="exceptions-table" ShowFooter="false">
                                        <Columns>
                                            <telerik:GridBoundColumn DataField="SerialNo" HeaderText="SerialNo"></telerik:GridBoundColumn> 
                                            <telerik:GridBoundColumn DataField="ExceptionDate" HeaderText="ExceptionDate" UniqueName="ExceptionDate" 
                                                DataType="System.DateTime" HeaderStyle-Width="150px" ItemStyle-Width="150px"></telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="CreatedDate" HeaderText="CreatedDate" UniqueName="CreatedDate" 
                                                DataType="System.DateTime" HeaderStyle-Width="150px" ItemStyle-Width="150px"></telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="Code" HeaderText="Code" UniqueName="Code"></telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="AdditionalInfo" DataFormatString="{0:#.##}" HeaderText="AdditionalInfo" UniqueName="AdditionalInfo"></telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="ExceptionDesc" HeaderText="ExceptionDesc" UniqueName="ExceptionDesc"></telerik:GridBoundColumn>
                                            <telerik:GridBoundColumn DataField="InfoDesc" HeaderText="InfoDesc" UniqueName="InfoDesc"></telerik:GridBoundColumn>

                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </div>
                        <%--END--%>

                        <%--MOTIONS TAB--%>
                        <div id="Motions_tab" runat="server" ClientIDMode="Static">
                            <div class="motions-tab-container">
                                <telerik:RadGrid ID="rgDailyMotions" runat="server"
                                    SkinID="Default" CssClass="OTable"
                                    AllowPaging="false" 
                                    Width="1600"  OnItemDataBound="rgDailyMotions_ItemDataBound"
                                    AllowFilteringByColumn="false" OnNeedDataSource="rgDailyMotions_NeedDataSource"
                                    AutoGenerateColumns="false">
                                    <MasterTableView TableLayout="Fixed" DataKeyNames="SerialNo" CssClass="daily-motion-table" ShowFooter="false">
                                        <Columns>
                                            <telerik:GridBoundColumn DataField="SerialNo" HeaderStyle-Width="80" HeaderText="Serial No"></telerik:GridBoundColumn> 
                                            <telerik:GridBoundColumn DataField="MotionDetectedDate" HeaderText="Motion Detected Date" UniqueName="MotionDetectedDate" 
                                                DataType="System.DateTime" HeaderStyle-Width="80px"  DataFormatString="{0:MM/dd/yyyy}"></telerik:GridBoundColumn>
                                            <telerik:GridTemplateColumn HeaderText="Hour0" UniqueName="Hour0">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour0")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour1" UniqueName="Hour1">
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour1")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour2" UniqueName="Hour2">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour2")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour3" UniqueName="Hour3">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour3")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour4" UniqueName="Hour4">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour4")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour5" UniqueName="Hour5">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour5")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour6" UniqueName="Hour6">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour6")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour7" UniqueName="Hour7">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour7")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour8" UniqueName="Hour8">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour8")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour9" UniqueName="Hour9">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour9")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour10" UniqueName="Hour10">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour10")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour11" UniqueName="Hour11">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour11")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour12" UniqueName="Hour12">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour12")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour13" UniqueName="Hour13">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour13")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour14" UniqueName="Hour14">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour14")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour15" UniqueName="Hour15">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour15")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour16" UniqueName="Hour16">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour16")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour17" UniqueName="Hour17">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour17")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour18" UniqueName="Hour18">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour18")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour19" UniqueName="Hour19">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour19")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour20" UniqueName="Hour20">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour20")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour21" UniqueName="Hour21">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour21")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour22" UniqueName="Hour22">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour22")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 
                                            <telerik:GridTemplateColumn HeaderText="Hour23" UniqueName="Hour23">  
                                                <ItemTemplate> <%# Convert.ToBoolean(Eval("Hour23")) == true ? "1" : "0" %> </ItemTemplate>  
                                            </telerik:GridTemplateColumn> 

                                            <telerik:GridBoundColumn DataField="CreatedDate" HeaderText="CreatedDate" UniqueName="CreatedDate" 
                                                DataType="System.DateTime" HeaderStyle-Width="150px" ItemStyle-Width="150px"></telerik:GridBoundColumn>
                    
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                            </div>
                        </div>
                        <%--END--%>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>  
        <%---------------------------------- END MAIN PAGE CONTENT ------------------------------------------------------%>
        <div class="Buttons">
            <div class="ButtonHolder">
                <asp:Button ID="btnBackToSerialNumberSearch" runat="server" CssClass="OButton" Text="Back to Search Page" OnClick="btnBackToSerialNumberSearch_OnClick" />
            </div>
        </div>
    </div>
</asp:Content>

