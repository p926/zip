<%@ Page Title="Home" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_CustomerService_Default" Codebehind="Default.aspx.cs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style type="text/css">
li {
  padding: 4px 0;
}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <ol style="padding-top:0; margin-top: 0;">
    <li><a href="ICCareDealerMaintenance.aspx">IC Care Dealer Maintenance</a>: Allow Manual Entry of IC Care Dealers.</li>
    <li><a href="ShipmentAssign.aspx">Enterprise User Shipment Assignments</a>: Allow Manual Entry of Shipment Assignments for Enterprise Accounts</li>
    <li><a href="OrderQueue.aspx">Order Queue </a>: Release orders into MAS and view/update PO Request's for IC Care orders.</li>
    <li><a href="DealerPORequest.aspx">Dealer PO Request Report </a>: Dealer PO Request report for created orders.</li>
    <li><a href="CreateAccount.aspx">Register new Admin Account </a>: Allow register new Admin account.</li>
    <li><a href="ManageOrdAcknowledgement.aspx">Upload/View Signed Order Acknowledgement </a>: Allow upload or view order acknowledgement.</li>
    <li><a href="DealerAccountSales.aspx">Account and Invoice Listing</a>: Search by sales rep. to get accounts and invoice history.</li>
    <li><a href="RequestQueue.aspx">Customer Service Requests</a>: Maintain customer service requests.</li>
    <%--<li><a href="CreditCardExpirationListing.aspx">Credit Card Expiration Date Listing</a>: Search account credit cards by type and expiration date.</li>--%>
    <li><a href="CRMAccounts.aspx">Customer Index</a>: Review an CRM Account.</li>
  </ol>
</asp:Content>

