<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InformationFinder_Compose_ReturnDeviceDetails" Codebehind="ReturnDeviceDetails.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">

  <style type="text/css">
    .style1
    {
      font-weight: bold;
      text-align: right;
      width: 134px;
    }
    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
  
  <div style="width: 650px" >
      <%--<div style="padding: 4px"><asp:Label ID="lblError" runat="server" CssClass="error2" ></asp:Label></div>--%>

      <div class="FormError" id="errors" runat="server" visible="false">
		<p><span class="MessageIcon"></span>
	    <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
	  </div>

      <div class="OForm">

        <div class="Row">
          <div class="Label ">Account #:</div>
          <div class="LabelValue"><asp:Label ID="lblAccountNo" runat="server" ></asp:Label></div>
          <div class="Clear"></div>
        </div>

        <div class="Row">
          <div class="Label ">Return #:</div>
          <div class="LabelValue"><asp:Label ID="lblReturnID" runat="server" ></asp:Label></div>
          <div class="Clear"></div>
        </div>

        <div class="Row">
          <div class="Label ">Serial #:</div>
          <div class="LabelValue"><asp:Label ID="lblSerialNo" runat="server" ></asp:Label></div>
          <div class="Clear"></div>
        </div>

        <div class="Row">
          <div class="Label ">Device Received:</div>
          <div class="LabelValue">
              <asp:Label ID="lblReceived" runat="server" Text="No"></asp:Label>
          </div>
          <div class="Clear"></div>
        </div>

        <div class="Row">
          <div class="Label">Status:</div>
          <div class="LabelValue">
                <asp:Label ID="lblStatus" runat="server" Text="No"></asp:Label>
          </div>
          <div class="Clear"></div>
        </div>

        <div class="Row">
          <div class="Label">Notes:</div>
          <div class="Control">
            <asp:TextBox ID="txtDeviceNotes" runat="server" Height="115px" 
              TextMode="MultiLine" CssClass = "Size XLarge2" ></asp:TextBox>
          </div>
          <div class="Clear"></div>
        </div>

        


      </div>

     

     <div class="Buttons">
        
        <div class="ButtonHolder">
            <asp:Button ID="btnDelete" CssClass="OButton" runat="server" Text="Delete" onclick="btnDelete_Click" /> 
            <asp:Button ID="btnUpdate" CssClass="OButton" runat="server" Text="Update" onclick="btnUpdate_Click" /> 
            <asp:Button ID="btnCancel" CssClass="OButton" runat="server" Text="Back to Return Page" onclick="btnCancel_Click" />
        </div>
        <div class="Clear"> </div>
    </div>

  </div>
</asp:Content>

