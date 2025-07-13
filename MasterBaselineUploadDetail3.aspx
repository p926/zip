<%@ Page Title="Master Basline Upload Detail" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="True" Inherits="TechOps_MasterBaselineUploadDetail3" Codebehind="MasterBaselineUploadDetail3.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    
    <script type="text/javascript" language ="JavaScript">

        function openRunBatchWindow() {
            var screenW = 500;
            var screenH = 350;
            var winProperty = 'width=' + screenW + ',height=' + screenH + ',scrollbars=yes,resizable=yes,menubar=no,status=no,location=no';
            window.open('RunBatch.aspx', 'RunBatchWindow', winProperty)
        }

        function executeBatch() {
            if (window.ActiveXObject) {
                try {
                    //alert("i am here.");
                    var oShell = new ActiveXObject("Shell.Application");
                    var prog = "C:\\Program Files\\POReceipts\\ImportPOReceipts.bat";                    
                    oShell.ShellExecute(prog, "", "", "open", "1");
                }
                catch (e) {
                    alert(e.message);
                }
            }
            else {
                alert("Your browser does not support the MAS upload functionality. Must try with IE.");
            }
        }

        function executeBatch_STG() {
            if (window.ActiveXObject) {
                try {
                    //alert("i am here.");
                    var oShell = new ActiveXObject("Shell.Application");
                    var prog = "C:\\Program Files\\POReceipts\\ImportPOReceipts_Stg.bat";
                    oShell.ShellExecute(prog, "", "", "open", "1");
                }
                catch (e) {
                    alert(e.message);
                }
            }
            else {
                alert("Your browser does not support the MAS upload functionality. Must try with IE.");
            }
        }

    </script>
    
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>       	
        
    <div style="width:100%" >
    
        <div class="FormError" id="errors" runat="server" visible="false">
		    <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
	    </div>       

        <div class="OForm" >

            <div class="Row" id="MasterFileSection" runat = "server" >
                <div class="Label Large2 ">Master File <span class="Required">*</span>:</div>
                <div class="Control">
                    <asp:FileUpload ID="MasterFile" runat="server" Width="400px" />                            
                </div>
                <div class="Clear"></div>
            </div>                                                                                                                         

        </div>            

        <div class="Buttons">
            <div class="RequiredIndicator"><span class="Required">*</span> Indicates a required field.</div>
            <div class="ButtonHolder">                    
                <asp:Button ID="btnUpdate" CssClass="OButton" runat="server" Text="Upload" onclick="btnUpload_Click" />
                <asp:Button ID="btnCancel" CssClass="OButton" runat="server" Text="Back" onclick="btnCancel_Click" />                
                        
            </div>
            <div class="Clear"> </div>
        </div> 
                                                                     
    </div>

</asp:Content>

