<%@ Page Title="Home" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Admin_TechOps_Default" Codebehind="Default.aspx.cs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style type="text/css">
li {
  padding: 4px 0;
}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
  <ol style="padding-top:0; margin-top: 0;">
     <li runat="server" id="liAccountRenewal"><a href="Renewal/Default.aspx">Account Renewals</a>: Run an account to renew it's devices for another term.</li>
     <li runat="server" id="liBatchProcessing"><a href="BatchProcessing/Default.aspx">Batch Processing</a>: Run orders through the batch processing to send data to Softrax, PayPal and MAS.</li>         
      <%--Hide Credit Memo for now. It will be worked on after ERP deployment--%>
     <li runat="server" id="liAXInvoiceCreditRebill"><a href="AXCreditRebill.aspx">AX Invoice Credit & Rebill</a>: Create AX Invoice Credit & Rebill By Account ID.</li>
     <%--<li><a href="CreditMemoSearch.aspx">Credit Memo</a>: Create Credit Memo By Account ID.</li>--%>
     <%--<li><a href="NewProcessCreditCard.aspx">Orbital Paymentech Process a Credit Card</a>: Manually process a credit card through Orbital Paymentech that is associated to an invoice.</li>--%>
     <li runat="server" id="liAXOrbitalPayment"><a href="AXNewProcessCreditCard.aspx">AX Orbital Paymentech Process a Credit Card</a>: Manually process a credit card through Orbital Paymentech that is associated to an AX invoice.</li>
     <li runat="server" id="liOMockProcessCC"><a href="MockProcessCreditCard.aspx">Orbital Paymentech Process Mock Invoices Payment</a>: Manually process a credit card through Orbital Paymentech for mock invoices.</li>
     <li runat="server" id="liGenerateReport"><a href="Reports.aspx">Generate Reports</a>: Generate Credit Card Expiration and Daily Batch Reports</li>
  </ol>
</asp:Content>
  