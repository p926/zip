<%@ Page Title="Home" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Manufacturing_Default" Codebehind="Default.aspx.cs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style type="text/css">
li {
  padding: 4px 0;
}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <ol style="padding-top:0; margin-top: 0;">
 
        <li><a href="ShippingQueue.aspx">Shipping Queue </a>: Allow print pick sheet, packing list & view ship history.</li>
        <li><a href="ShippingQueueClear.aspx">Insert Tracking# and Merge Packages </a>: Allow clear packages in shipping queue.</li>
        <li><a href="AggregateShippingReport.aspx">Shipping Summary Report</a>: View a summary report of products shipped.</li>
        <li><a href="ConvertProductSKU.aspx">Convert Product SKU</a>: Convert old Instadose 1 Product SKU's to new Instadose IMI Product SKU's.</li>
  </ol>
</asp:Content>

  