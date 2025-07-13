<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_InformationFinder_GenerateProformaInvoice" Codebehind="GenerateProformaInvoice.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
<div style="padding: 4px;"><asp:Label ID="lblResponse" runat="server"></asp:Label></div>
<table cellpadding="2" cellspacing="0" class="tbl1" style="width: 350px">
  <tr>
    <th colspan="2">
      Generate Proforma Invoice:
    </th>
  </tr>
  <tr>
    <td class="label" style="width: 80px;">
      Order #:</td>
    <td>
      <asp:TextBox ID="txtOrderNo" runat="server"></asp:TextBox>
      <asp:Button ID="cmdGenerateNew" runat="server" onclick="cmdGenerate_Click" Text="Generate" />
    </td>
  </tr>
</table>
</asp:Content>

