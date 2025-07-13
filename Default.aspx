<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style type="text/css">
li {
  padding: 4px 0;
}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <ol>
        <li runat="server" id="liInfoFinder"><a href="InformationFinder/Default.aspx">Information Finder</a>: Find account, user, device, order and other information.</li>
        <li runat="server" id="liFinancial"><a href="Finance/Default.aspx">Financial Dashboard</a>: Run Financial Operations on Instadose.</li>
        <li runat="server" id="liSales"><a href="Sales/Default.aspx">Sales Dashboard</a>: Run Sales Operations on Instadose.</li>
        <li runat="server" id="liShippingReceiving"><a href="Shipping/Default.aspx">Shipping &amp; Receiving Dashboard</a>: Run Shipping/Receiving Operations on Instadose.</li>
        <li runat="server" id="liTechnical"><a href="TechOps/Default.aspx">Technical Dashboard</a>: Run Technical Operations on Instadose.</li>
        <li runat="server" id="liManufacturing"><a href="Manufacturing/Default.aspx">Manufacturing Dashboard</a>: Run Manufacturing Operations on Instadose.</li>
        <li runat="server" id="liCustomerService"><a href="CustomerService/Default.aspx">Customer Service Dashboard</a>: Run Customer Service Operations on Instadose.</li>
        <li runat="server" id="liIT"><a href="IT/Default.aspx">IT Dashboard</a>: Run IT Operations on Instadose.</li>
    </ol>
</asp:Content>

