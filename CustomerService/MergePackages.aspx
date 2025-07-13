<%@ Page Title="Merge Packages" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_MergePackages" Codebehind="MergePackages.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript">
    $(document).ready(function () {

        if ($('#ctl00_cpBody_txtMultiPackageID').text() == "") {
            // Disable the continue button
            $('#ctl00_cpBody_btnSave').attr("disabled", "true");
        }
        else {
            // A new item has been scanned.
            var countDevices = countLines('ctl00_cpBody_txtMultiPackageID');
            //$('#count').text("Devices scanned: " + countDevices);
        }
        // Set up the serial number validation
        $('#ctl00_cpBody_txtMultiPackageID').keyup(function (event) {

            // A new item has been scanned.
            var countDevices = countLines('ctl00_cpBody_txtMultiPackageID');
            //$('#count').text("Devices scanned: " + countDevices);

            // Enable the continue button if the serials numbers counter is greater than 0.
            if (countDevices <= 0)
                $('#ctl00_cpBody_btnSave').attr("disabled", "true");
            else
                $('#ctl00_cpBody_btnSave').removeAttr('disabled');

        }).keypress(function (event) {

            // Validate the keys being pressed to ensure only numbers are entered.
            if (("0123456789").indexOf(String.fromCharCode(event.keyCode)) <= -1 && event.keyCode != '13') {
                // If not, cancel the event.
                event.preventDefault();
            }
        });
    });

    /// Count lines of a text box.
    function countLines(id) {
        var area = document.getElementById(id)
        if (area.value == "") return 0;
        // trim trailing return char if exists
        var text = area.value.replace(/\s+$/g, "")
        var split = text.split("\n")
        return split.length
    }
</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">

    <act:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server">
    </act:toolkitscriptmanager>

    <table class="ManageTable" style="width:650px; border:0px">
        <tr>
            <td>
                <asp:Label Text="" CssClass="errorMessage" ID="lblError" runat="server" />
            </td>
        </tr>
    </table>

    <table class="ManageTable" style="width:650px">

        <tr>
            <td class="Title" colspan="2">
                Shipped Package Information
            </td>
        </tr>      
        <tr>
            <td class="Label">Package #<span class="Required">*</span>:</td>
            <td class="Control">
                <asp:TextBox runat="server" ID="txtPackageID" 
                    MaxLength="40" CssClass="Medium3" ValidationGroup="form" />
                <span class="error" id="lblPackageIDValidate" runat="server" visible="false"></span>
            </td>
        </tr>
        <tr>
            <td class="Label">Tracking #<span class="Required">*</span>:</td>
            <td class="Control">
                <asp:TextBox runat="server" ID="txtTrackingNo" MaxLength="50" 
                    CssClass="Large1" ValidationGroup="form"/>
                <span class="error" id="lblTrackingNoValidate" runat="server" visible="false"></span>
            </td>
        </tr>
        
        
        <tr>
            <td colspan="2" class="Title">Merged Packages:</td>
        </tr>
        <tr>
            <td class="Label">Package #<span class="Required">*</span>:</td>
            <td class="Control">
                <asp:TextBox runat="server" ID="txtMultiPackageID" TextMode="MultiLine" Height="100px"  
                    CssClass="Medium3" ValidationGroup="form" />
                <span class="error" id="lblMultiPackageIDValidate" runat="server" visible="false"></span>  
            </td>
        </tr>
        

        <tr>
            <td class="Buttons" colspan="2">
                <span class="Required">* Indicate required fields.</span>
                <asp:Button Text="Merge" ID="btnSave" runat="server" OnClick="btnSave_Click" 
                    ValidationGroup="form" style="margin-left: 0px" Width="100px" />
       
            </td>
        </tr>
    </table>

    <table style="width:650px; border:0px">
        <tr>
            <td style="text-align: right; color:Blue; font-size: 1.1em;">
                <asp:Label Text="" ID="lblSuccess" runat="server" />
            </td>
        </tr>
    </table>

</asp:Content>

