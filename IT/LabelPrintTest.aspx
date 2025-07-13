<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_LabelPrintTest" Codebehind="LabelPrintTest.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="server" >
    <act:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server">
    </act:toolkitscriptmanager>

    <div style="color: #0000FF">This page is used to print a custom label with your inputs. Please enter your input for a label print-out.</div>
    <br />        

    <div >
        <asp:UpdatePanel ID="upnlPrinting" runat="server" UpdateMode="Conditional">
            <ContentTemplate>                            
                
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
                        <div class="Label Medium2">Logo:</div>
                        <div class="Control">
                                    <asp:DropDownList ID="ddlLogo" runat="server" AutoPostBack ="true"
                                        onselectedindexchanged="ddlLogo_SelectedIndexChanged" >
                                        <asp:ListItem Text="" Value="" />
                                        <asp:ListItem Text="ICCare" Value="ICCare" />                                        
                                        <asp:ListItem Text="Mirion" Value="Mirion" />
                                        <asp:ListItem Text="Quantum" Value="Quantum" />                                        
                                    </asp:DropDownList>              
                                </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Medium2">Company Name:</div>
                        <div class="Control"><asp:TextBox runat="server" ID="txtAccountName" 
                                CssClass="Size Large " ValidationGroup="form" /></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Medium2">Acc #:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtAccountID" 
                               CssClass="Size Small " ValidationGroup="form" />                            
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Medium2">Wearer Name:</div>
                        <div class="Control"><asp:TextBox runat="server" ID="txtWearerName" 
                                CssClass="Size Large " ValidationGroup="form" /></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Medium2">Wearer Region:</div>
                        <div class="Control"><asp:DropDownList ID="ddlBodyRegion" runat="server" >
                                        <asp:ListItem Text="" Value="" />
                                        <asp:ListItem Text="Torso" Value="Torso" />                                        
                                        <asp:ListItem Text="Fetal" Value="Fetal" />
                                        <asp:ListItem Text="Area" Value="Area" />
                                        <asp:ListItem Text="Collar" Value="Collar" />                                         
                                    </asp:DropDownList>    
                                </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Medium2">Wearer Location:</div>
                        <div class="Control"><asp:TextBox runat="server" ID="txtWearerLocation" 
                                CssClass="Size Small " ValidationGroup="form" /></div>
                        <div class="Clear"></div>
                    </div>                    

                    <div class="Row">
                        <div class="Label Medium2">Wearer #:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtWearerID" 
                               CssClass="Size Small " ValidationGroup="form" />                            
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Medium2">Barcode #:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtBarcode" 
                               CssClass="Size Small " ValidationGroup="form" />                            
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Medium2">Quantity<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtQuantity" 
                                MaxLength="20" CssClass="Size XXSmall" ValidationGroup="form" />                            
                        </div>
                        <div class="Clear"></div>
                    </div>   

                </div>    
                           
                <div class="Row">
                    <div class="Label Medium2">&nbsp;</div>
                    <div class="Control">
                        <asp:Button Text="Print" ID="btnPrint" runat="server" 
                               ValidationGroup="form" cssClass="OButton" 
                            onclick="btnPrint_Click" /> &nbsp;&nbsp;&nbsp;
                        <asp:Button Text="Reset" ID="btnReset" runat="server" 
                               ValidationGroup="form" cssClass="OButton" 
                            onclick="btnReset_Click" />
                    </div>
                    <div class="Clear"></div>
                </div> 
                                  
                
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

</asp:Content>

