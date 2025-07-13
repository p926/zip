<%@ Page Title="Home" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
    li {
      padding: 4px 0;
    }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <ol id="menuContainer" style="padding-top:0; margin-top: 0;">
        <li><a href="ManageDose.aspx">Manage Dose</a>: Allow Manual Dose Adjustments or Input of Lifetime Dose History.</li>
        <li><a href="DriftAnalysis.aspx">Drift Analysis</a>: Perform a drift analysis on groups of devices and commit them into MAS inventory.</li>
        <li><a href="SerialNoSearch.aspx">Badge Review</a>: Allow Display of all Tech Ops Reads before release to inventory.</li>
        <li><a href="ReadAnalysis.aspx">Read Analysis</a>: Initiate Read Analysis process.</li>
        <li><a href="PressureTestResults.aspx">Pressure Test Results</a>: Display/Print results of Pressure Tests for a specific Group.</li>
        <li><a href="ReturnAddNewDeviceRMA.aspx">Add New Recall</a>: Add new Recall and send email to CS about Recalls.</li>
        <li><a href="RecallStatus.aspx">Recalls Status</a>: Display recalls status.</li>
        <li><a href="ReturnInventory.aspx">Receipt of Badges</a>: Receive Return Badge.</li>
        <%--<li><a href="OthersListing.aspx">Others Listing</a>: Display listing of 'Others' accounts.</li>--%>
        <li><a href="FactorsSearch.aspx">Factors</a>: Manage Account and Location Factors.</li>
        <li><a href="MasterBaselineFileUpload.aspx">PO Receipt Files Upload</a>: Upload the Master/Baseline files and Enter PO Receipts Journal in MAS.</li>
        <li><a href="RequestTransferQAToWIP.aspx">Request Badges for Testing</a>: Make a request to QA to issue badges for testing.</li>
        <li><a href="AssignGroupColor.aspx">Assign Group</a>: Assign a group to the devices.</li>
        <%--<li><a href="ID2MasterBaselineUpload.aspx">ID2 Baseline Upload</a>: Upload all Master, Temperature Compensation, and Sensitivity Coefficient files.</li>--%>
        <li><a href="ID2DoseEmulator.aspx">ID2 Dose Emulator</a>: Calculate Doses of ID2 Dummy Badge.</li>
        <li><a href="CalendarSetting.aspx">Calendar Setting</a>: Setting Scheduled Reads for a Group.</li>
        <li><a href="ID2Analysis.aspx">ID2 Analysis</a>: ID2 Reading Analysis for a Group.</li>
        <li><a href="DosimetryDocuments.aspx">Manage Dosimetry Docs</a>: Manage documents used on doc.dosimetry.com.</li>
        <li><a href="DosimetryDocUsers.aspx">Manage Dosimetry Doc Users</a>: Manage users on doc.dosimetry.com.</li>
        <li><a href="DoseCalcParameters.aspx">Dose Calculation Parameters</a>: Manage Dose Calculation Parameters.</li>
		<li><a href="OrderQueue.aspx">Instadose Plus Order Queue</a>: Manage Instadose Plus order queue.</li>        
        <li><a href="DeviceWarehouseTransfer.aspx">Device Warehouse Transfer</a>: Transfer a device to a different warehouse/status .</li>
        <li><a href="CurrentDeviceAssignment.aspx">Current Device Assignment</a>: Assign/UnAssign Account Device.</li>
      </ol>
</asp:Content>

