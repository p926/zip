<%@ Page Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Instadose_Shipping_DeviceTracking"
    Title="Device Tracking" Codebehind="DeviceTracking.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">


    <script type="text/javascript">

        var $intervalId = null;

        function resetFields()
        {
            $('#ctl00_primaryHolder_ddlActualWareHouse').val("").change();
            $('#ctl00_primaryHolder_txtLocation').val("").change();
            $('#ctl00_primaryHolder_txtUserName').val("").change();
            $('#ctl00_primaryHolder_txtScanSerial').val("").change();
            $('#ctl00_primaryHolder_lblErrmessage').html("").change();
            $('#ctl00_primaryHolder_hdnfldFlashRed').val("").change();

            $('#ctl00_primaryHolder_plInput')[0].style.backgroundColor = "LightYellow";
            clearInterval($intervalId);

        }

        function toggle_color(color1, color2, cycle_time, wait_time) {
            $intervalId =  setInterval(function first_color() {
                document.getElementById('ctl00_primaryHolder_plInput').style.backgroundColor = color1;
               // document.getElementById('ctl00_primaryHolder_lblErrmessage').style.color = "LightYellow";
                setTimeout(change_color, wait_time);
            }, cycle_time);

            function change_color() {
                document.getElementById('ctl00_primaryHolder_plInput').style.backgroundColor = color2;
               // document.getElementById('ctl00_primaryHolder_lblErrmessage').style.color = "Maroon";
            }
        }

        function copylocationToSearchlocation(obj)
        {
            $('#ctl00_primaryHolder_txtsearchLocation')[0].innerText = $('#ctl00_primaryHolder_txtLocation')[0].value;
        }

        $(document).ready(function () {

      
            //toggle_color("red", "lightyellow", 1500, 1000);
            //checkFields();

            if ($('#ctl00_primaryHolder_hdnfldFlashRed')[0].value == "yes")
                toggle_color("red", "lightyellow", 1500, 1000);

            if ($('#ctl00_primaryHolder_txtScanSerial')[0].value.trim().length > 0)
                $('#ctl00_primaryHolder_txtScanSerial').select();
          
        });


        // check for empty fields before saving to database
        function checkFields(obj)
        {
            var breturn = true;
            var lerrmessage = $('#ctl00_primaryHolder_lblErrmessage');

            if ($('#ctl00_primaryHolder_hdnfldFlashRed')[0].value != "yes" &&
                lerrmessage[0].innerText.indexOf("Error") < 0 &&
                lerrmessage[0].innerText.indexOf("success") < 0)
                lerrmessage[0].textContent = "";

            if ($('#ctl00_primaryHolder_txtLocation')[0].value.trim() == "")
            {
                lerrmessage[0].innerText = "*** please enter a value in the Location Field ***";
                $('#ctl00_primaryHolder_txtLocation').focus();
                breturn = false;
            }


            if ($('#ctl00_primaryHolder_ddlActualWareHouse')[0].value == "") {
                if (lerrmessage[0].innerText == "")
                    lerrmessage[0].innerText = "*** please select a value from the Actual WareHouse dropdown ***";
                else
                    lerrmessage[0].innerText = lerrmessage[0].innerText + "\r\n*** please select a value from the Actual Warehouse dropdown ***";


                $('#ctl00_primaryHolder_ddlActualWareHouse').focus();
                breturn = false;
            }

            if ($('#ctl00_primaryHolder_txtUserName')[0].value.trim() == "") {
                if (lerrmessage[0].innerText == "")
                    lerrmessage[0].innerText = "*** please enter a value in the UserName Field ***";
                else
                    lerrmessage[0].innerText = lerrmessage[0].innerText + "\r\n*** please enter a value in the User Field ***";

                $('#ctl00_primaryHolder_txtUserName').focus();

               // lerrmessage[0].textContent = lerrmessage[0].textContent.replace(/&lt;br&gt;/gi, "n").replace(/(&lt;([^&gt;]+)&gt;)/gi, "")

                breturn = false;
            }

            //if (breturn)
            //    $('#ctl00_primaryHolder_txtScanSerial').focusin();

            //return breturn;
        }

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

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">

    <asp:HiddenField ID="hdnfldFlashRed" runat="Server" value=""/>
 

    <asp:Panel ID="plInput" runat="server" BackColor="lightyellow">
        
        <table>
            <tr>
                <td style="width:150px"> <asp:Label ID="Label1" runat="server" Text="Location"></asp:Label></td>
                <td style="width:120px"> <asp:Label ID="Label2" runat="server" Text="Actual Warehouse"></asp:Label></td>
                <td style="width:150px"> <asp:Label ID="Label4" runat="server" Text="User"></asp:Label></td>
                <td style="width:150px"> <asp:Label ID="Label3" runat="server" Text="Scan Serial No"></asp:Label></td>
               
                <td></td>

            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="txtLocation" runat="server" MaxLength="50" onChange="javascript:copylocationToSearchlocation(this);"></asp:TextBox>

                </td>
                <td>
                    <asp:DropDownList ID="ddlActualWareHouse" runat="server" Width="120px"></asp:DropDownList>

                </td>
                <td>
                    <asp:TextBox ID="txtUserName" runat="server" maxlength="50"></asp:TextBox>
                    
                </td>
                <td>
                    <asp:TextBox ID="txtScanSerial" runat="server" Width="200" onfocus="javascript: checkFields(this);"  MaxLength="200" ></asp:TextBox>

                </td>
              
                <td>
                    <asp:Button ID="btnSave" runat="server" Text="Add to database" OnClick="btnSave_Click" />
                </td>
                <td>

                    <input id="btnReset" type="button" value="Clear Fields"  onclick="javascript: resetFields();"/>
                </td>
            </tr>
            
        </table>
       
        <br/>
        <asp:Label ID="lblErrmessage" runat="server" Text="" ForeColor="black" BackColor="White"></asp:Label>
        <br/>
        <br/>
       
        
    </asp:Panel>


    <asp:Panel ID="plReview" runat="server" Visible="true" >
        <div class="OToolbar JoinTable">
         <table>
            <tr>
                <td style="width:150px"> <asp:Label ID="Label5" runat="server" Text="Location"></asp:Label></td>
                <td style="width:150px"> <asp:Label ID="Label6" runat="server" Text="Actual Warehouse"></asp:Label></td>
                <td style="width:150px"> <asp:Label ID="Label8" runat="server" Text="User"></asp:Label></td>
                <td style="width:150px"> <asp:Label ID="Label7" runat="server" Text="Scan Serial No"></asp:Label></td>
             
                <td></td>

            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="txtsearchLocation" runat="server" MaxLength="50"></asp:TextBox>

                </td>
                <td>
                    <asp:DropDownList ID="ddlsearchActualWarehouse" runat="server" Width="120px"></asp:DropDownList>

                </td>
                <td>
                    <asp:TextBox ID="txtsearchUser" runat="server" maxlength="50"></asp:TextBox>
                    
                </td>
                <td>
                    <asp:TextBox ID="txtsearchScanSerial" runat="server" Width="200"  MaxLength="200"></asp:TextBox>

                </td>
              
                <td>
                    <asp:Button ID="btnSearch" runat="server" Text="Filter Devices" OnClick="btnSearch_Click"  />
                </td>

            </tr>
            
        </table>
       
       </div> <!-- div from after panel -->


        <asp:GridView ID="gvDeviceInfo" runat="server" CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
            AutoGenerateColumns="false" AllowPaging="True" OnPageIndexChanging="gvDeviceInfo_PageIndexChanging" PageSize="10" AllowSorting="True" OnSorting="gvDeviceInfo_Sorting">
            <Columns>
                <asp:BoundField DataField="DeviceTrackingID" HeaderText="DeviceTrackingID" SortExpression="DeviceTrackingID" Visible="false"  />
                <asp:BoundField DataField="Createddate" HeaderText="Created Date" SortExpression="Createddate" />
                <asp:BoundField DataField="Location" HeaderText="Location" SortExpression="Location" />
                <asp:BoundField DataField="ActualWareHouse" HeaderText="Actual Warehouse"  SortExpression="ActualWareHouse"/>
                <asp:BoundField DataField="ScanSerial" HeaderText="Scanned Serial No" SortExpression="ScanSerial"/>
                <asp:BoundField DataField="UserName" HeaderText="User" SortExpression="UserName"/>
                <asp:BoundField DataField="ScannedWareHouse" HeaderText="Scanned Warehouse" SortExpression="ScannedWareHouse"/>
                <asp:BoundField DataField="DeviceID" HeaderText="Device ID" SortExpression="DeviceID"/>

               <%-- <asp:TemplateField HeaderText="Notes">
                    <ItemTemplate>
                        <asp:TextBox ID="txtNotes" runat="server" Text="Good Condition" Width="325"
                            TextMode="MultiLine" Height="35" />
                    </ItemTemplate>
                </asp:TemplateField>--%>
            </Columns>
             <EmptyDataTemplate>
					<div class="NoData">
						There are no items!
					</div>
				</EmptyDataTemplate>  
            <PagerStyle CssClass="Footer" />
        </asp:GridView>

        <div class="OToolbar"> 
            Total: <asp:Label ID="lbltotalCount" runat="server" Text=""></asp:Label>
        
            <div style="float:right;">
                <asp:Localize runat="server" meta:resourceKey="ExportTo" Text="Export To:" Visible="false" />
                <asp:DropDownList ID="ddlExport" runat="server"  Visible="false"
                    meta:resourcekey="ddlExportResource1">
                    <asp:ListItem Text="CSV File Format" Value="CSV" meta:resourceKey="CSV" />
                    <asp:ListItem Text="Excel Document" Value="XLS" meta:resourceKey="XLS" />
                    <asp:ListItem Text="Web Page" Value="HTML" meta:resourceKey="HTML" />
                </asp:DropDownList>
                <asp:Button Text="Export to Excel" ID="btnExport" OnClick="btnExport_Click" 
                    runat="server" meta:resourcekey="btnExportResource1" />
            </div>

        </div>
       
        <div class="Clear"></div>
    </asp:Panel>
</asp:Content>
