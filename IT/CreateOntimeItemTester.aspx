<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_CreateOntimeItemTester" Codebehind="CreateOntimeItemTester.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="server" >
    <act:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server">
    </act:toolkitscriptmanager>       

    <div >
                                              
        <div class="FormError" id="dialogErrors" runat="server" visible="false">
			<p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="dialogErrorMsg" runat="server" >An error was encountered.</span></p>
		</div>
        <div class="FormMessage" id="dialogSuccess" runat="server" visible="false" style="margin:10px" > 
	        <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="dialogSuccessMsg" runat ="server" >Ready to search.</span></p>
        </div> 
        
        <div class="OForm" >                                          
             
            <div class="Row">
                <div class="Label Medium2">Backlog Type:</div>
                <div class="Control">
                    <asp:RadioButtonList id="radBacklogType" runat="server" RepeatColumns="2" RepeatDirection="Horizontal">                        
                        <asp:ListItem selected="true">Help Desk</asp:ListItem>
                        <asp:ListItem >Defect</asp:ListItem>
                    </asp:RadioButtonList>                                        
                </div>
                <div class="Clear"></div>
            </div> 
                         
            <div class="Row">
                <div class="Label Medium2">Project<span class="Required">*</span>:</div>
                <div class="Control">
                    <asp:DropDownList ID="ddlProjects" runat="server"  />                                    
                </div>
                <div class="Clear"></div>
            </div> 

            <div class="Row">
                <div class="Label Medium2">Severity:</div>
                <div class="Control">
                    <asp:DropDownList ID="ddlSeverity" runat="server">
                        <asp:ListItem Text="" Value="0" Selected="True" />
                        <asp:ListItem Text="No Impact" Value="5"  />
                        <asp:ListItem Text="Low Impact" Value="3"  /> 
                        <asp:ListItem Text="Medium Impact" Value="4" />
                        <asp:ListItem Text="High Impact" Value="2" />
                        <asp:ListItem Text="Critical" Value="1" /> 
                    </asp:DropDownList>                                    
                </div>
                <div class="Clear"></div>
            </div> 

            <div class="Row">
                <div class="Label Medium2">Title<span class="Required">*</span>:</div>
                <div class="Control"><asp:TextBox runat="server" ID="txtTitle" 
                        CssClass="Size Large " ValidationGroup="form" /></div>
                <div class="Clear"></div>
            </div>

            <div class="Row">
                <div class="Label Medium2">Customer Contact:</div>
                <div class="Control">
                    <asp:DropDownList ID="ddlContacts" runat="server" Enabled = "false" />                                    
                </div>
                <div class="Clear"></div>
            </div> 

            <div class="Row">
                <div class="Label Medium2">Description<span class="Required">*</span>:</div>
                <div class="Control"><asp:TextBox runat="server" ID="txtDescription" TextMode="MultiLine"
                        CssClass="Size Large " ValidationGroup="form" Height="200px" 
                        Width="800px" /></div>
                <div class="Clear"></div>
            </div>                    

            <div class="Row">
                <div class="Label Medium2">Repro Steps:</div>
                <div class="Control"><asp:TextBox runat="server" ID="txtReproStep" TextMode="MultiLine"
                        CssClass="Size Large " ValidationGroup="form" Height="200px" 
                        Width="800px"/></div>
                <div class="Clear"></div>
            </div>

            <div class="Row">
                <div class="Label Medium2 ">Attachment 1:</div>
                <div class="Control">
                    <asp:FileUpload ID="attachment1" runat="server" Width="400px" />                            
                </div>
                <div class="Clear"></div>
            </div>                                              

            <div class="Row">
                <div class="Label Medium2">Attachment 2:</div>
                <div class="Control">
                    <asp:FileUpload ID="attachment2" runat="server" Width="400px" />                            
                </div>
                <div class="Clear"></div>
            </div>                                   

            <div class="Row" id = "deleteSection" runat="server" visible="true">
                <div class="Label Medium2">Delete Item#:</div>
                <div class="Control"><asp:TextBox runat="server" ID="txtItemNo" 
                        CssClass="Size Small" ValidationGroup="form" /></div>
                <div class="Clear"></div>
            </div>
                    
            <div class="Row">
                <div class="Label Medium2">&nbsp;</div>
                <div class="Control">
                    <asp:Button Text="Add" ID="btnAdd" runat="server" 
                            ValidationGroup="form" cssClass="OButton" 
                        onclick="btnAdd_Click" /> &nbsp;&nbsp;&nbsp;
                    <asp:Button Text="Reset" ID="btnReset" runat="server" 
                            ValidationGroup="form" cssClass="OButton" 
                        onclick="btnReset_Click" /> &nbsp;&nbsp;&nbsp;
                    <asp:Button Text="Delete" ID="btnDelete" runat="server" visible="true"
                            ValidationGroup="form" cssClass="OButton" onclick="btnDelete_Click" 
                            /> 
                </div>
                <div class="Clear"></div>
            </div>                                        

        </div>    
                                                                                             
            
    </div>

</asp:Content>

