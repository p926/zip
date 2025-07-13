<%@ Page Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Instadose_Shipping_ReceiveInventory"
    Title="Receive Returned Inventory" Codebehind="ReceiveInventory.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">
        $(document).ready(function () {

            $('#ctl00_primaryHolder_txtSerialNos').focus();

            if ($('#ctl00_primaryHolder_txtSerialNos').text() == "") {
                // Disable the continue button
                $('#ctl00_primaryHolder_btnContinue').attr("disabled", "true");                
            }
            else {
                // A new item has been scanned.
                var countDevices = countLines('ctl00_primaryHolder_txtSerialNos');
                $('#count').text(countDevices);
            }
            // Set up the serial number validation
            $('#ctl00_primaryHolder_txtSerialNos').keyup(function (event) {

                // A new item has been scanned.
                var countDevices = countLines('ctl00_primaryHolder_txtSerialNos');
                $('#count').text(countDevices);

                // Enable the continue button if the serials numbers counter is greater than 0.
                if (countDevices <= 0) {
                    $('#ctl00_primaryHolder_btnContinue').attr("disabled", "true");                    
                }
                else {
                    $('#ctl00_primaryHolder_btnContinue').removeAttr('disabled');                    
                }

            }).keypress(function (event) {
                // Get the keycode between FireFox and other browsers.
                var key = (event.keyCode ? event.keyCode : event.which);

                // Allow: backspace, delete, tab, escape, and enter
                if (key == 46 || key == 8 || key == 9 || key == 27 || key == 13 ||
                // Allow: Ctrl+A
                  (key == 65 && event.ctrlKey === true) ||
                // Allow: home, end, left, right
                  (key >= 35 && key <= 39)) {
                    // let it happen, don't do anything
                    return;
                }
                else {
                    // Ensure that it is a number and stop the keypress
                    if (event.shiftKey || (key < 48 || key > 57)) {

                        event.preventDefault();
                    }
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

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:Panel ID="plAdd" runat="server">
        <table class="OTable" style="width: auto;" >
            <tr>
                <th>
                    Received Badge Serial Numbers:
                </th>
            </tr>
            <tr>
                <td>
                    <div>
                        <asp:TextBox ID="txtSerialNos" TextMode="MultiLine" runat="server" Style="width: 210px;
                            height: 320px; margin: 5px;"></asp:TextBox>
                    </div>
                    <div class="">
                        <div class="Left" style="padding-top: 6px; vertical-align: middle;">
                            <span style="font-weight: bold;">Badges: </span><span id="count">0</span></div>
                        <div class="Right" style="width: auto;" >
                            <asp:Button ID="btnContinue" CssClass="OButton" runat="server" Text="Add Badges"
                                OnClick="btnContinue_Click" />
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                </td>
            </tr>
        </table>
    </asp:Panel>

    <asp:Panel ID="plReview" runat="server" Visible="false">

        <asp:Panel runat="server" ID="pnlErrorMessages" class="FormError" Visible="false">
            <p>
                <span class="MessageIcon"></span>
                <strong>Errors:</strong>
                <asp:Label ID="lblErrorReview" runat="server"></asp:Label>
            </p>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlSuccessMessages" class="FormMessage" Visible="false">
            <p>
                <span class="MessageIcon"></span>
                <strong>Success:</strong>
                <asp:Label ID="lblSuccess" runat="server"></asp:Label>
            </p>
        </asp:Panel>
        
        <asp:GridView ID="gvReview" runat="server" CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
            AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="SerialNo" HeaderText="Serial#" />
                <asp:TemplateField HeaderText="Account#">
                    <ItemTemplate>
                        <asp:Label ID="lbl_GvAccountNo" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"AccountNo","" ) %>'
                            BackColor='<%# FuncCheckAccountNo(DataBinder.Eval(Container.DataItem,"AccountNo","" ))%>'> </asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Request#">
                    <ItemTemplate>
                        <asp:Label ID="lbl_GvRequestNo" runat="server" Text='<%# Eval("RequestNo") %>' BackColor='<%#FuncCheckRequestNo(DataBinder.Eval(Container.DataItem,"RequestNo","" ))%>'> </asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="DepartmentRequest" HeaderText="Department" />
                <asp:BoundField DataField="Action" HeaderText="Action" />
                <asp:TemplateField HeaderText="Notes">
                    <ItemTemplate>
                        <asp:TextBox ID="txtNotes" runat="server" Text="Good Condition" Width="325"
                            TextMode="MultiLine" Height="35" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <table class="OTable" style="width: 500px;">
            <tr>
                <th>Upload Attachments</th>
            </tr>
            <tr>
                <td>
                    <div class="OForm">
                        <div class="Row">
                            <div class="Label">Attachment 1:</div>
                            <div class="Control"><asp:FileUpload ID="FileUpload1" runat="server" /></div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row">
                            <div class="Label">Attachment 2:</div>
                            <div class="Control"><asp:FileUpload ID="FileUpload2" runat="server" /></div>
                            <div class="Clear"></div>
                        </div>
                    </div>
                </td>
            </tr>
        </table>

        <div class="Left">
            <strong>User Name:</strong>
            <asp:Label ID="lblusername" runat="server"></asp:Label>
        </div>
        <div class="Right">
            <asp:Button ID="btnFinish" runat="server" CssClass="OButton" OnClick="btnFinish_Click"
                Text="Accept Badges" />
            <asp:Button ID="btnBack" runat="server" CssClass="OButton" OnClick="btnBack_Click"
                Text="Cancel" />
        </div>
        <div class="Clear"></div>
    </asp:Panel>
</asp:Content>
