<%@ Page Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Instadose_Shipping_PrintYellowLabels"
    Title="Print Yellow Labels" Codebehind="PrintYellowLabels.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">
        $(document).ready(function () {

            updateCount();

            // Set up the serial number validation
            $('#<%=txtSerialNos.ClientID %>').keyup(function () {
                updateCount();
            }).keypress(function (event) {

                // Validate the keys being pressed to ensure only numbers are entered.
                if (("0123456789").indexOf(String.fromCharCode(event.keyCode)) <= -1 && event.keyCode !== '13') {
                    // If not, cancel the event.
                    event.preventDefault();
                }
            });
        });

        function updateCount() {
            // A new item has been scanned.
            var countDevices = countLines();
            $('#count').text(countDevices);

            // Enable the continue button if the serials numbers counter is greater than 0.
            if (countDevices <= 0)
                $('#<%=btnContinue.ClientID %>').attr("disabled", "true");
            else
                $('#<%=btnContinue.ClientID %>').removeAttr('disabled');
        }

        // Count lines of a text box.
        function countLines(id) {
            var text = $('#<%=txtSerialNos.ClientID %>').val();
            if (text === "") return 0;

            // trim trailing return char if exists
            text = text.replace(/\s+$/g, "")
            var split = text.split("\n")
            return split.length
        }

    </script>
    <style type="text/css">
        .center {
            text-align: center;
        }
        .left {
            text-align: left;
        }
        .cell {
        }
        
        .lg_cell {
            text-align: left;
            width: 550px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:Panel ID="plAdd" runat="server">
        <table class="OTable" style="width: auto;">
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
                        <div class="Right" style="width: auto;">
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
                <asp:BoundField DataField="OrderNo" HeaderText="Order#" />
                <asp:BoundField DataField="AccountNo" HeaderText="Account#" />
                <asp:BoundField DataField="AccountName" HeaderText="Account Name" />
                <asp:BoundField DataField="UserName" HeaderText="UserName" />
                <asp:BoundField DataField="Password" HeaderText="Password" />
            </Columns>
        </asp:GridView>

        <div class="Right">
            <asp:Button ID="btnPrintGroup" runat="server" CssClass="OButton" OnClick="btnPrintGroup_Click"
                Text="Print Label" />
            <asp:Button ID="btnBack" runat="server" CssClass="OButton" OnClick="btnBack_Click"
                Text="Back" />
        </div>
        <div class="Clear"></div>

    </asp:Panel>
</asp:Content>
