<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Finance_Renewal_Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
li {
  padding: 4px 0;
}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
 <ol style="padding-top:0; margin-top: 0;">
<%--    <li><a href="Renewals.aspx">Process Renewals</a>: View a list of accounts that are ready to be renewed and process renewal orders for those accounts.</li>
    <li><a href="RenewalReviewExport.aspx">Review Renewals</a>: View a list of accounts that are ready to be renewed for export to Excel spreadsheet.</li>
    <li><a href="Billings.aspx">Process Quarterly Billings</a>: View a list of quarterly accounts that are ready to be billed and process billing orders for those accounts.</li>
--%>
	<li><a href="RenewalProcess.aspx">Renewal Process</a>: Process renewal of accounts that are ready for renewal processing.</li>
	<li><a href="RenewalExceptions.aspx">Renewal Exception</a>: List of accounts which have exception for renewal processing.</li>
	<li><a href="ThreadLog.aspx">Renewal Progress</a>: List of renewal processing.</li>
    <%--<li><a href="RenewalGeneration.aspx">Renewal Generation</a>: Generate batch list of accounts that are ready for renewal processing.</li>--%>
    <li><a href="RenewalRelease.aspx">Release Renewals</a>: Review and release renewal hold (batch) list of accounts that are ready for renewal processing.</li>
    <li><a href="ReadyToInvoice.aspx">Ready to Invoice</a>: Review renewal hold (batch) list of accounts that are ready to be invoiced.</li>
    <li><a href="ReviewLog.aspx">Review Log</a>: Review renewal hold (batch) list of accounts that were processed.</li>
    <li><a href="RenewalForecast.aspx">Renewal Forecast</a>: View list of renewals for forecasting.</li> 
    <li><a href="ForecastChart.aspx">Renewal Forecast Chart</a>: View list of renewals chart for forecasting.</li> 
    <li><a href="ScheduledBillings.aspx">Scheduled Billings</a>: Processed scheduled billings for a list of accounts.</li>
 </ol>
</asp:Content>

