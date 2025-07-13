<%@ Page Title="Master Basline Upload Detail" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_MasterBaselineUploadDetail" Codebehind="MasterBaselineUploadDetail.aspx.cs" %>

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
	
<%--    <div id="myDialog">
        <asp:UpdatePanel ID="upnlAddNotes" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
            
                <div class="FormError" id="groupDialogErrors" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="groupDialogErrorMsg" runat="server" >An error was encountered.</span></p>
				</div>

                <div class="OForm" > 
                             
                    <div class="Row">
                        <div class="Label Small">Location #:</div>
                        <div class="LabelValue"><asp:Label runat="server" ID="lblGroupLocationID" CssClass="StaticLabel" /></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Location Name:</div>
                        <div class="LabelValue"><asp:Label runat="server" ID="lblGroupLocationName" CssClass="StaticLabel" /></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Group #:</div>
                        <div class="LabelValue"><asp:Label runat="server" ID="lblGroupID" CssClass="StaticLabel" /></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Active:</div>
                        <div class="Control">
                            <asp:CheckBox ID="chkBoxGroupActive" runat="server" Text=""  />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Group Name<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtGroupName" 
                                MaxLength="40" CssClass="Size Medium" ValidationGroup="form" />
                            <span class="InlineError" id="lblGroupNameValidate" runat="server" visible="false"></span>
                        </div>
                        <div class="Clear"></div>
                    </div>

                         

                </div>               
                <asp:button text="Save" style="display:none;" id="btnAddGroup" OnClick="btnAddGroup_Click" runat="server" />
                <asp:button text="Close" style="display:none;" id="btnCancelGroup" OnClick="btnCancelGroup_Click" runat="server" />
                <asp:button text="Load" style="display:none;" id="btnLoadGroup" OnClick="btnLoadGroup_Click" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>--%>
        
    <div style="width:100%" >
    
        <div class="FormError" id="errors" runat="server" visible="false">
		    <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
	    </div>

        <%--<div id="failDataSection" runat="server" style="width: 850px">
            <table cellpadding="0" cellspacing="0" class="OTable" style="width: 850px;">
                <tr>
                    <td style="color: blue">
                        If you like to upload a failed BL2 
                        instadose, then select it and click Upload button again.
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:GridView ID="gvFailBL2" runat="server" CssClass="OTable"
                            AllowSorting="true" AutoGenerateColumns="False" >                                      

                            <Columns>
                
                                <asp:TemplateField HeaderText="Select">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="cbRow" runat="server" />
                                    </ItemTemplate>
                                    <HeaderStyle HorizontalAlign="Left" />
                                    <ItemStyle HorizontalAlign="Left" />
                                </asp:TemplateField>
                                
                                <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="Sn"
                                    HeaderText="SN#" SortExpression="Sn" />
                                <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="ErrorMessage"
                                    HeaderText="Error Message" SortExpression="ErrorMessage" />
                                <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="ValidationType"
                                    HeaderText="Validation Type" SortExpression="ValidationType" Visible="false" />
                                        
                            </Columns>

                            <RowStyle Font-Size="Small"  />
                            <HeaderStyle Font-Bold="true" Font-Size="Small" />

                            <EmptyDataTemplate>
							    <div class="NoData">
								    No BL2s out of range.
							    </div>
							</EmptyDataTemplate>
							<AlternatingRowStyle CssClass="Alt" />
							<PagerStyle CssClass="Footer" />
                        </asp:GridView>
                    </td>
                </tr>           
            </table>        
        </div>--%>


        <div class="OForm" >

            <div class="Row">
                <div class="Label Medium">Master File <span class="Required">*</span>:</div>
                <div class="Control">
                    <asp:FileUpload ID="MasterFile" runat="server" Width="400px" />                            
                </div>
                <div class="Clear"></div>
            </div>                                              

            <div class="Row">
                <div class="Label Medium">Baseline File <span class="Required">*</span>:</div>
                <div class="Control">
                    <asp:FileUpload ID="BaselineFile" runat="server" Width="400px" />                            
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

