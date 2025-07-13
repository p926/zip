<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Reports_Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">
        $(function () {
            // Date pickers
            $("#<%= txtStartDate.ClientID %>").datepicker({
                defaultDate: "+1w",
                numberOfMonths: 2,
                onClose: function (selectedDate) {
                    $("#<%= txtEndDate.ClientID %>").datepicker("option", "minDate", selectedDate);
                }
            });
            $("#<%= txtEndDate.ClientID %>").datepicker({
                defaultDate: "+1w",
                numberOfMonths: 2,
                onClose: function (selectedDate) {
                    $("#<%= txtStartDate.ClientID %>").datepicker("option", "maxDate", selectedDate);
                }
            });
        });

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <div class="OForm" style="width: 400px;">
        <div class="Row">
            <div class="FormError" runat="server" id="divFormError" visible="false">
                <p>
                    <span class="MessageIcon"></span>
                    <strong>Messages:</strong>
                    <asp:Label Text="Error" CssClass="errors" ID="lblError" runat="server" />
                </p>
            </div>
        </div>
        <div class="Row">
            <label for="<%= txtAccountID.ClientID %>" class="Label">Account #:</label>
            <asp:TextBox runat="server" ID="txtAccountID" Text="2086" placeholder="required" />
        </div>

        <div class="Row">
            <label for="<%= txtLocationID.ClientID %>" class="Label">Location #:</label>
            <asp:TextBox runat="server" ID="txtLocationID" Text="" placeholder="no param passed" />
        </div>

        <div class="Row">
            <label for="<%= txtGroupID.ClientID %>" class="Label">Group #:</label>
            <asp:TextBox runat="server" ID="txtGroupID" Text="" placeholder="no param passed" />
        </div>

        <div class="Row">
            <label for="<%= txtUserID.ClientID %>" class="Label">User #:</label>
            <asp:TextBox runat="server" ID="txtUserID" Text="" placeholder="no param passed" />
        </div>

        <div class="Row">
            <label for="<%= txtStartDate.ClientID %>" class="Label">Start Date:</label>
            <asp:TextBox runat="server" ID="txtStartDate" Text="" placeholder="mm/dd/yyyy" />
        </div>

        <div class="Row">
            <label for="<%= txtEndDate.ClientID %>" class="Label">End Date:</label>
            <asp:TextBox runat="server" ID="txtEndDate" Text="" placeholder="mm/dd/yyyy" />
        </div>

        <div class="ButtonHolder">
            <asp:Button Text="Generate" ID="btnGenerate" OnClick="btnGenerate_Click" CssClass="OButton" runat="server" />
        </div>
    </div>
</asp:Content>

