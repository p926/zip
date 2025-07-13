<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_InformationFinder_EmailOrderAck2" Codebehind="EmailOrderAck.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
<asp:ScriptManager ID="ScriptManager1" runat="server">
</asp:ScriptManager>
<asp:Panel ID="Panel2" runat="server">
<asp:UpdatePanel ID="UpdatePanel4" runat="server" >
<ContentTemplate>  

<table class="OTable" style="width: 500px">

  <tr>
      <th colspan="2">
          Email Order Acknowledgement:</th>
      <tr>
          <td class="label" style="width: 80px;">
              Account ID:</td>
          <td>
              <asp:TextBox ID="txtAccountID" runat="server" AutoPostBack="true" 
                  OnTextChanged="txtAccountID_OnTextChanged" Width="50"></asp:TextBox>
              <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
                  ControlToValidate="txtAccountID" Display="Dynamic" ErrorMessage="*Requred. " 
                  ValidationGroup="EmailOrdAck" />
              <asp:RegularExpressionValidator ID="RegularExpressionValidator" runat="server" 
                  ControlToValidate="txtAccountID" Display="Dynamic" ErrorMessage="Numerics only" 
                  ValidationExpression="^\d*$" ValidationGroup="EmailOrdAck" />
          </td>
      </tr>
      <tr>
          <td class="label" style="width: 80px;">
              Email:</td>
          <td>
              <asp:TextBox ID="txtEmail" runat="server" Width="200"></asp:TextBox>
              <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" 
                  ControlToValidate="txtEmail" Display="Dynamic" ErrorMessage="*Required " 
                  ValidationGroup="EmailOrdAck" />
              <asp:RegularExpressionValidator ID="RegularExpressionValidEmail" runat="server" 
                  ControlToValidate="txtEmail" Display="Dynamic" 
                  ErrorMessage="Invalid Email address" 
                  ValidationExpression="^[\w-]+(?:\.[\w-]+)*@(?:[\w-]+\.)+[a-zA-Z]{2,7}$" 
                  ValidationGroup="EmailOrdAck" />
          </td>
      </tr>
      <tr>
        <td colspan="2" class="Clear" />
      </tr>
      <tr>
          <td align="right" class="Buttons" colspan="2">
              <asp:Button ID="btnSendEmail" runat="server" class="OButton" 
                  onclick="btnSendEmail_Click" Text="Send Email" ValidationGroup="EmailOrdAck" />
              <asp:Button ID="btnCloseWindow" runat="server" class="OButton" 
                  OnClientClick="javascript:window.close();" Text="Close Window" />
          </td>
      </tr>
      <tr>
          <td colspan="2">
              <asp:Label ID="lblResponse" runat="server" Text=""></asp:Label>
          </td>
      </tr>
  </tr>


</table>
</ContentTemplate>
</asp:UpdatePanel>
</asp:Panel>
</asp:Content>

