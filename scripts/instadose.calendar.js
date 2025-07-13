var ManageCalendar = ManageCalendar || {
    accountID: 0,
    scheduleCounts: [],
    initial: "",
    calDetails: {},
    calendarListGrid: "",
    monthlyOptions: [[31, "Last"], [1, "1st"], [5, "5th"], [10, "10th"], [15, "15th"], [20, "20th"], [25, "25th"]],
    weeklyOptions: [[0, "Sunday"], [1, "Monday"], [2, "Tuesday"], [3, "Wednesday"], [4, "Thursday"], [5, "Friday"], [6, "Saturday"]],
    frequencies: ["None", "Weekly", "Every 2 Weeks", "Monthly", "Quarterly", "Every 2 Months"],
    counts: {
        schedules: 0,
        locationsTotal: 0,
        locationsAssigned: 0
    },
    changesMade: false,
    locations: [],
    editCal: 0,
    configure: function () {
        var _this = this;
        
        $("#hlAddCalendar").on("click", function (e) {
            e.preventDefault();

            $("#calendar-dialog").dialog("open");
        });

        $("#calendar-dialog").dialog({
            autoOpen: false,
            width: 550,
            modal: true,
            resizable: false,
            title: "Add Schedule",
            buttons: {
                "Add": function () {
                    Common.toggleDialogError("#calendar-dialog", "");
                    Common.toggleDialogLoading("#calendar-dialog", true);

                    _this.addSchedule(function (response) {
                        Common.toggleDialogLoading("#calendar-dialog", false);

                        if (response.Success) {
                            $("#calendar-dialog").dialog("close");

                            //_this.loadCalendarCounts();
                            Common.refreshGrid(_this.calendarListGrid);
                        } else {
                            Common.toggleDialogError("#calendar-dialog", response.Data);
                        }
                    });
                },
                "Close": function () {
                    $(this).dialog("close");
                }
            },
            open: function () {
                Common.toggleDialogError("#calendar-dialog", "");
                Common.toggleDialogLoading("#calendar-dialog", false);

                $("#calendar-freq").val("M").trigger("change");
                $("#calendar-time-hour").val("11");
                $("#calendar-time-min").val("59");
                $("#calendar-time-ampm").val("PM");
            },
            close: function () {
                Common.toggleDialogError("#calendar-dialog", "");
                $("#calendar-dialog").find("input,textarea").val("");
            }
        });

        $("#calendar-freq").off("change").on("change", function () {
            var selection = $(this).val();
            var ddDay = $("#calendar-day");
            //monthly is the default
            var option = _this.monthlyOptions;
            var optionText = " day of the month."
            var optionDefault = "31";

            switch (selection) {
                case "T":
                    optionText = " day of the 2nd month.";
                    break;
                case "B":
                    option = _this.weeklyOptions;
                    optionText = " of the 2nd week.";
                    optionDefault = "5";
                    break;
                case "W":
                    option = _this.weeklyOptions;
                    optionText = " of the week.";
                    optionDefault = "5";
                    break;
                case "Q":
                    optionText = " day of the last month of the quarter.";
                    break;
            }

            _this.populateDays(option, optionText);
            ddDay.val(optionDefault);
        });

        $("#calendar-dates-dialog").dialog({
            autoOpen: false,
            modal: true,
            width: 790,
            closeOnEscape: false,
            title: "Details",
            open: function (event, ui) {
                $("#calendar-dates-dialog").parent().find(".ui-dialog-titlebar-close").hide();
                Common.toggleDialogError("#calendar-dates-dialog", "");
                Common.toggleDialogLoading("#calendar-dates-dialog", false);

                //_this.changesMade = false;
                //_this.getCalendarDetails(ManageCalendar.editCal);
            },
            close: function () {
                Common.toggleDialogError("#calendar-dates-dialog", "");
                $("#calendar-date").val("");
                $("#calendar-hour").val("11");
                $("#calendar-min").val("59");
                $("#calendar-ampm").val("PM");
            },
            buttons: {
                "Save Changes": function () {
                    Common.toggleDialogError("#calendar-dates-dialog", "");
                    Common.toggleDialogLoading("#calendar-dates-dialog", true);

                    _this.updateSchedule(function (response) {
                        Common.toggleDialogLoading("#calendar-dates-dialog", false);

                        if (response.Success) {
                            $("#calendar-dates-dialog").dialog("close");
                            Common.refreshGrid(_this.calendarListGrid);
                        } else {
                            Common.toggleDialogError("#calendar-dates-dialog", "Error occured while updating the calendar.");
                        }
                    });
                },
                "Close": function () {
                    if (ManageCalendar.changesMade) {
                        var confirmationMessage = "You have unsaved changes that will be lost.  Continue?";
                        $.when(Common.openConfirm("Unsaved Changes", confirmationMessage, 500))
                            .then(function () {
                                $("#calendar-dates-dialog").dialog("close");
                            });
                    } else {
                        $(this).dialog("close");
                        $("#manage-instadose-dialog").dialog({ closeOnEscape: true });
                    }
                }
            }
        });

        $("#calendar-add").off("click").on("click", function () {
            var err = "";
            Common.toggleDialogError("#calendar-dates-dialog", err);

            var calDate = $("#calendar-date").val();
            var calHour = parseInt($("#calendar-hour").val());
            var calMin = parseInt($("#calendar-min").val());
            var calAMPM = $("#calendar-ampm").val();

            if (calDate == "") {
                err += "Calendar date cannot be blank.";
            }

            if (isNaN(calHour) || calHour < 1 || calHour > 12) {
                err += "Hour must be between 1 and 12.";
            } else {
                calHour = calHour < 10 ? "0" + calHour.toString() : calHour.toString();
            }

            if (isNaN(calMin) || calMin < 0 || calMin > 59) {
                err += "Minute must be between 0 and 59.";
            } else {
                calMin = calMin < 10 ? "0" + calMin.toString() : calMin.toString();
            }

            if (err != "") {
                Common.toggleDialogError("#calendar-dates-dialog", err);
            } else {
                Common.toggleDialogError("#calendar-dates-dialog", "");

                _this.calDetails.Dates.push({ Date: calDate + " " + calHour + ":" + calMin + " " + calAMPM.toLowerCase(), ScheduleID: null });
                _this.loadCalendarDetails(_this.calDetails);

                $("#calendar-date").val("");
                _this.changesMade = true;
            }
        });

        $("#gridCalendarLocations").off("click").on("click", "[name=chkLocation]", function () {
            var clickedObj = $(this);
            var selectedLoc = $.grep(ManageCalendar.locations, function (i) {
                return i.LocationID == clickedObj.attr("id").split("-")[1];
            });
            if (clickedObj.is(":checked")) {
                selectedLoc[0].CompanyNo = 0;
                ManageCalendar.counts.locationsAssigned++;
            } else {
                selectedLoc[0].CompanyNo = 1;
                ManageCalendar.counts.locationsAssigned--;
            }
            ManageCalendar.changesMade = true;
            $("Table.display.dataTable tfoot #count-locations-assigned").html(ManageCalendar.counts.locationsAssigned);
        });

        $("#chkAllLocations").off("click").on("click", function () {
            var checkAll = $(this).is(":checked");
            $.each(ManageCalendar.locations, function (i, item) {
                item.CompanyNo = checkAll ? 0 : 1;
            });
            $("input[name=chkLocation]").prop("checked", checkAll);
            ManageCalendar.counts.locationsAssigned = checkAll ? ManageCalendar.counts.locationsTotal : 0;
            $("Table.display.dataTable tfoot #count-locations-assigned").html(ManageCalendar.counts.locationsAssigned);
        });

        _this.getLocations();
        //_this.loadCalendarCounts();
    },
    openCalendarDatesDialog: function (calendarID) {
        this.changesMade = false;
        this.getCalendarDetails(calendarID);

        $("#calendar-dates-dialog").dialog("open");
    },
    populateDays: function (arr, addText) {
        var dd = $("#calendar-day");
        dd.empty();

        $("#calendar-day-statement").text(addText);

        $.each(arr, function (i, item) {
            dd.append("<option value='" + item[0] + "'>" + item[1] + "</option>");
        });
    },
    updateSchedule: function (callback) {
        var _this = this;
        
        var dates = [];
        var locs = [];

        $.each(_this.calDetails.Dates, function (i, item) {
            dates.push(item.Date);
        });

        $("[name=chkLocation]:checked").each(function () {
            var id = $(this).attr("id");
            locs.push(id.split("-")[1]);
        });

        var dObj = {
            calendarID: ManageCalendar.calDetails.CID,
            account: ManageCalendar.accountID,
            scheduleDates: dates,
            lstLocations: locs,
            noteInitials: ManageCalendar.initial
        };

        Common.wsCall("/Services/Calendar.asmx/UpdateCalendar", dObj, "POST", function (response) { callback(response); });
    },
    addSchedule: function (callback) {
        var _this = this;
        
        var calHour = parseInt($("#calendar-time-hour").val());
        var calMin = parseInt($("#calendar-time-min").val());
        var calAMPM = $("#calendar-time-ampm").val();
        var err = "";

        if (isNaN(calHour) || calHour < 1 || calHour > 12) {
            err += "Hour must be between 1 and 12.";
        } else {
            calHour = calHour < 10 ? "0" + calHour.toString() : calHour.toString();
        }

        if (isNaN(calMin) || calMin < 0 || calMin > 59) {
            err += "  Minute must be between 0 and 59.";
        } else {
            calMin = calMin < 10 ? "0" + calMin.toString() : calMin.toString();
        }

        if (err != "") {
            callback({ Success: false, Data: err });
        } else {
            var dObj = {
                account: _this.accountID,
                calendarName: $("#calendar-name").val(),
                calendarDesc: $("#calendar-desc").val(),
                frequency: $("#calendar-freq").val(),
                readDay: $("#calendar-day").val(),
                readTime: calHour + ":" + calMin + " " + calAMPM.toUpperCase(),
                noteInitials: _this.initial,
                location: ""
            };

            if (dObj.calendarName == "") {
                callback({ Success: false, Data: "A schedule name is required." });
            } else {
                Common.wsCall("/Services/Calendar.asmx/AddCalendar", dObj, "POST", function (response) { callback(response); });
            }
        }
    },
    getLocations: function () {
        Common.getLocationListItems(ManageCalendar.accountID, false, function (response) {
            ManageCalendar.locations = response;
            ManageCalendar.loadLocations(ManageCalendar.locations);
        });
    },
    loadLocations: function (locations) {
        var lTable = $("#tblCalendarLocations").dataTable({
            "dom": "tir",
            "paging": false,
            "oLanguage": {
                "sEmptyTable": "No locations assigned."
            },
            "fixedHeader": {
                header: true,
                footer: true
            },
            "footerCallback": function (row, data, start, end, display) {
                var api = this.api(), data;

                ManageCalendar.locationsAssigned = $.grep(locations, function (i) { return i.CompanyNo == 0; });
                ManageCalendar.counts.locationsTotal = locations.length;
                ManageCalendar.counts.locationsAssigned = ManageCalendar.locationsAssigned.length;

                $("#chkAllLocations").prop("checked", ManageCalendar.counts.locationsAssigned == ManageCalendar.counts.locationsTotal);
                $("#count-locations-assigned").html(ManageCalendar.counts.locationsAssigned);
                $("#count-locations-total").html(ManageCalendar.counts.locationsTotal);
            },
            "bInfo": false,
            "scrollY": "200px",
            "scrollCollapse": true,
            "data": locations,
            renderer: "bootstrap",
            destroy: true,
            "columns": [
                {
                    "data": "CompanyNo", "width": "20px", "render": function (data, type, row) {
                        if (type === 'display') {
                            var addChecked = (data == 0 ? "checked='checeked'" : "");
                            return "<input type='checkbox' name='chkLocation' id='location-" + row.LocationID + "' " + addChecked + "'/>";
                        } else {
                            return data;
                        }
                    }, "orderSequence": ["asc"]
                },
                { "data": "LocationID", "width": "80px" },
                { "data": "LocationName" },
                {
                    "data": "CalendarID", "width": "20px", "render": function (data, type, row) {
                        if (type === "display") {
                            return (data != null && data != "") ? "X" : "";
                        } else {
                            return data;
                        }
                    }
                }

            ]
        });

        //lTable.columns().every(function () {
        //    var that = this;
        //    console.log(that);
        //    $('.grid-filter', "#tblCalendarLocations").on('keyup change', function () {
        //        if (that.search() !== this.value) {
        //            that
        //                .search(this.value)
        //                .draw();
        //        }
        //    });
        //});
    },
    getCalendarDetails: function (calendarID) {
        calDetails = {};
        Common.wsCall('/Services/Calendar.asmx/GetCalendarDetails', { "calendarID": calendarID, "account": Common.account }, "POST", function (response) {
            if (response.Success) {
                var details = (JSON.parse(response.Data));

                ManageCalendar.calDetails = details;
                ManageCalendar.calDetails.CID = calendarID;
                ManageCalendar.loadCalendarDetails(details);
            }
        });
    },
    loadCalendarDetails: function (details) {
        var _this = this;

        _this.counts = {
            schedules: 0,
            locationsTotal: 0,
            locationsAssigned: 0
        };

        var oTable = $('#tblCalendarDetails').dataTable({
            "dom": 'tir<"footer">',
            "paging": false,
            "oLanguage": {
                "sEmptyTable": "No calendar details found."
            },
            "fixedHeader": {
                header: true,
                footer: true
            },
            "footerCallback": function (row, data, start, end, display) {
                var api = this.api(), data;
                ManageCalendar.counts.schedules = details.Dates.length;
                $("#count-schedules").html(ManageCalendar.counts.schedules);
            },
            "bInfo": false,
            "scrollY": "200px",
            "scrollCollapse": true,
            "data": details.Dates,
            renderer: "bootstrap",
            destroy: true,
            "columns": [
                {
                    "data": "Date", "render": function (data, type, row) {
                        if (type == "display" || type == "filter")
                            return moment(data).format("MM/DD/YYYY hh:mm a");
                        else
                            return moment(data).format("YYYYMMDDhhmm");
                    }
                }
            ]
        });

        $("input[name=chkLocation]").prop("checked", false);

        var tmpLocations = [];
        $.each(ManageCalendar.locations, function (i, item) {
            var loc = $.grep(details.Locations, function (i) {
                return i.LocationID == item.LocationID;
            });

            item.CompanyNo = loc.length > 0 ? 0 : 1;

            tmpLocations.push(item);

            $("#location-" + item.LocationID).prop("checked", true);
        });

        ManageCalendar.locations = tmpLocations;
        ManageCalendar.loadLocations(tmpLocations);
    },
    loadCalendarCounts: function () {
        var _this = this;
        
        var $calendarTabFields = $("#rtCalendars .rtsTxt");

        if ($calendarTabFields) {
            if (_this.scheduleCounts.length == 0) {
                Common.wsCall("/Services/Calendar.asmx/GetScheduleCounts", { account: _this.accountID }, "POST", function (response) {
                    if (response.Success) {
                        _this.scheduleCounts = response.Data;
                        $calendarTabFields.text("Calendars (" + response.Data + ")");
                    } else {
                        $calendarTabFields.text("Calendars");
                    }
                });
            }
        }
    }
}