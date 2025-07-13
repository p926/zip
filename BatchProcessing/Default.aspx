<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Admin_BatchProcessing_Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
  <div>Select an order below to process:</div>
<div style="width: 150px; text-align:right;">
<asp:ListBox ID="lstBatches" runat="server" Height="250px" Width="150px">
</asp:ListBox>
<div>&nbsp;</div>
<asp:Button ID="cmdViewDetails" runat="server" Text="View Details" 
    onclick="cmdViewDetails_Click" />
</div>
</asp:Content>

