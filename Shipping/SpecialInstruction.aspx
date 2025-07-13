<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="SpecialInstruction.aspx.cs" Inherits="Shipping_SpecialInstruction" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">

    <div class="FormError" id="error" runat="server" visible="false">
		<p><span class="MessageIcon"></span>
	    <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
	</div>
    <div class="FormMessage" id="success" runat="server" visible="false"> 
	    <p><span class="MessageIcon"></span>
	    <strong>Messages:</strong> <span id="successMsg" runat ="server" >Commit successfully!.</span></p>
    </div> 

    <div class="OForm" > 
        
        <div class="Row">
            <div class="Label Medium">Serial No<span class="Required">*</span>:</div>
            <div class="Control">
                <asp:TextBox runat="server" ID="txtSerialNo" 
                    MaxLength="10" CssClass="Size XSmall" />                            
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label Medium">Special Instruction<span class="Required">*</span>:</div>
            <div class="Control">                
                <asp:TextBox ID="txtSpecialInstruction" TextMode="MultiLine" CssClass="Notes" 
                            runat="server" Height="70px" MaxLength="1024" ></asp:TextBox>
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label Medium">&nbsp;</div>
            <div class="Control">
                <asp:button text="Submit" id="btnSubmit" OnClick="btnSubmit_Click" runat="server" cssClass="OButton" Font-Size="Large"/>  &nbsp;&nbsp;&nbsp;&nbsp;
                <asp:button text="Reset" id="btnReset" OnClick="btnReset_Click" runat="server" cssClass="OButton" Font-Size="Large"/>
            </div>
            <div class="Clear"></div>
        </div>  
        
                         
    </div>       
    
        
</asp:Content>
