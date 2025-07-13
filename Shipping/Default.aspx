<%@ Page Title="Shipping Dashboard" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Shipping_Default" Codebehind="Default.aspx.cs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style type="text/css">
li {
  padding: 4px 0;
}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
  <ol style="padding-top:0; margin-top: 0;">
    <li><a href="ReceiveInventory.aspx">Receive Returned Badges</a>: Process badges that have been returned by customers.</li>
    <li><a href="NewReceivingInventory.aspx">New Receive Returned Badges</a>: Process badges that have been returned by customers.</li>
    <li><a href="POReceipt.aspx">Receipt of Good Entry</a>: Enter the details of Shipment Received from Vendor.</li>
    <li><a href="ReleaseToWIP.aspx">Release QA Instadoses to Tech-Ops</a>: Release Tech-Ops's Request of Transfering of QA Instadoses Ready-for-Testing.</li>  
    <li><a href="DeviceTracking.aspx">Instadose Physical Count.</a>:Track the physical location and count of the Badges</li> 
    <li><a href="ReceivingBinManagement.aspx">Receiving Bin Setup</a>:Setup receiving bins for RMA return reasons</li> 
    <li><a href="SpecialInstruction.aspx">Special Instructions</a>:Enter the special instruction for a badge</li> 
  </ol>
</asp:Content>
