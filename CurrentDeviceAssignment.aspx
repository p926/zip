<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="CurrentDeviceAssignment.aspx.cs" Inherits="portal_instadose_com_v3.TechOps.CurrentDeviceAssignment" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        input[type=radio] {
            vertical-align: bottom;
            margin: 3px 0;
        }

        .radio-controls label {
            display: inline-block;
            vertical-align: top;
            padding-top: 1px;
        }
    </style>

    <script type="text/javascript">
        $(function () {
            $("#txtSerialNumbers").on("keyup", function (e) {
                if (e.keyCode == 13) {
                    $("#btnSubmit").trigger("click");
                }
            });

            $("#btnSubmit").on("click", function (e) {
                toggleError(false);
                toggleSuccess(false);

                validation = validateInputs();

                if (!validation.valid) {
                    toggleError(true, validation.message);
                } else {
                    toggleLoading(true);

                    var $serialNums = $("#txtSerialNumbers");
                    var serialNums = $serialNums.val();
                    var isAssign = $(".rblAssignment:checked").val();

                    deviceAssignmentAjaxCall(serialNums, isAssign)
                        .done(function (rtn) {
                            if (rtn.Success) {
                                var successSerialNums = rtn.SuccessSerialNumbers;
                                var serialNumErrorMsgs = rtn.ErrorSerialNumberMessages;

                                // show success message for assigned/unassigned device serial number
                                if (successSerialNums && successSerialNums.length > 0) {
                                    var successMsg = "Devices are updated for following serial numbers:<br>" + successSerialNums.join(", ");
                                    toggleSuccess(true, successMsg);
                                }

                                // show error message for assigned/unassigned device serial number with error
                                if (serialNumErrorMsgs && serialNumErrorMsgs.length > 0) {
                                    var errMsg = "Error occured while processing devices with following serial numbers:<br>" + serialNumErrorMsgs.join("<br>");
                                    toggleError(true, errMsg);
                                }
                            } else {
                                toggleError(true, rtn.Message);
                            }
                        })
                        .fail(function (jqXHR, status) {
                            toggleError(true, "Unknown error occured while processing.");
                        })
                        .always(function () {
                            $serialNums.val("");
                            $serialNums.focus();
                            toggleLoading(false);
                        });
                }
            });
        });

        function toggleLoading(toggle) {
            var $loading = $(".loading");

            if (toggle) {
                $loading.show();
            } else {
                $loading.hide();
            }
        }

        function toggleSuccess(display, message) {
            var $successPanel = $("#dialogSuccess");
            var $successMsg = $("#dialogSuccessMsg");

            if (display) {
                $successMsg.html(message);
                $successPanel.show();
            } else {
                $successMsg.html("");
                $successPanel.hide();
            }
        }

        function toggleError(display, message) {
            var $errPanel = $("#dialogErrors");
            var $errMsg = $("#dialogErrorMsg");

            if (display) {
                $errMsg.html(message);
                $errPanel.show();
            } else {
                $errMsg.html("");
                $errPanel.hide();
            }
        }

        function validateInputs() {
            var assign = $(".rblAssignment:checked").val();
            var serialNums = $("#txtSerialNumbers").val();

            if (!assign) {
                return {
                    valid: false,
                    message: "Please select Assignment option."
                };
            }

            if (!serialNums || serialNums == "") {
                return {
                    valid: false,
                    message: "Please enter Serial Number."
                };
            }

            return {
                valid: true,
                message: ""
            };
        }

        function deviceAssignmentAjaxCall(serialNumbers, isAssign) {
            return $.ajax({
                url: "/Services/AccountDevice.asmx/AssignAccountDevices",
                type: "POST",
                datatype: "json",
                contentType: "application/json",
                data: JSON.stringify({
                    SerialNumbers: serialNumbers.replace(/[\r\n]+/g, ""),
                    IsAssign: isAssign == "1" ? true : false
                }),
                dataFilter: function (data) {
                    return Common.ajaxDataFilter(data)
                }
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
    <div class="loading"></div>

    <div class="FormError" id="dialogErrors" style="display: none;">
		<p><span class="MessageIcon"></span>
	    <strong>Messages:</strong> <label id="dialogErrorMsg">An error was encountered.</label></p>
	</div>
    <div class="FormMessage" id="dialogSuccess" style="margin-top: 10px; display: none;"> 
	    <p><span class="MessageIcon"></span>
	    <strong>Messages:</strong> <label id="dialogSuccessMsg">Ready to search.</label></p>
    </div>

    <div class="OForm">
        <div class="Row">
            <div class="Label Medium">Assignment<span class="Required">*</span>:</div>
            <div class="Control radio-controls">
                <div style="padding-top: 5px;">
                    <input type="radio" name="rblAssignment" class="rblAssignment" id="rblAssignment_Assign" value="1" />
                    <label for="rblAssignment_Assign">Assign</label>
                </div>
                <div>
                    <input type="radio" name="rblAssignment" class="rblAssignment" id="rblAssignment_UnAssign" value="0" />
                    <label for="rblAssignment_UnAssign">Un-Assign</label>
                </div>
            </div>
            <div class="Clear"></div>
        </div>  

        <div class="Row">
            <div class="Label Medium">Serial #s<span class="Required">*</span>:</div>
            <div class="Control">
                <textarea id="txtSerialNumbers" style="width: 300px; height: 150px;"></textarea>
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label Medium"></div>
            <div class="Control" style="color: red;">
                The serial numbers must be comma separated.
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label Medium"></div>
            <div class="Control">
                <button type="button" id="btnSubmit" class="OButton">Submit</button>
            </div>
            <div class="Clear"></div>
        </div>
    </div>
</asp:Content>
