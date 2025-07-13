<%@ Page Title="ID2 Dose Emulator" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_TechOps_ID2DoseEmulator" Codebehind="ID2DoseEmulator.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">  
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" Runat="Server">

    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div>
        <%--<asp:UpdatePanel ID="udpn" runat="server"  >	--%>
            <%--<ContentTemplate> --%>
                
                <div class="FormError" id="errors" runat="server" visible="false">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
                </div>                

                <asp:Panel ID="pnlToolBar" runat="server" DefaultButton="btnCalculate" >                                        

                    <div class="OToolbar" id="ToolBar" runat ="server">
						<ul>								
							<li>  
                                Badge SN#:                                			                                 
                            </li>
                            <li>
                                <asp:TextBox ID = "sn" runat = "server" Text = "99999999" Enabled = "false" Width = "80px"/>
                            </li>
                            <li>  
                                Algorithm Factor:                                			                                 
                            </li>
                            <li>
                                <asp:TextBox ID = "txtAlgFactor" runat = "server" Text = "G" Width = "20px"/>
                            </li>
                            <li>
                                <asp:Button ID="btnCalculate" CssClass="OButton" runat="server" 
                                    Text="Calculate" onclick="btnCalculate_Click" />								
							</li>
						</ul>
					</div>    
         
                </asp:Panel>
        
                <asp:Panel ID="pnlInput" runat="server" >

                    <table class="OTable">
                        
                        <tr>
                            <td colspan="8" class="mt-itm-bold leftalign">
                                <asp:Label ID="Label20" runat="server" text="Default raw values to calculate 100 mrem"> </asp:Label>
                            </td>
                        </tr> 
                                  
                        <tr class="Alt">
                            <td  style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label2" runat="server" text="DL:"> </asp:Label>
                            </td>              
                            <td style="width:19%"  class="mt-itm leftalign" >                        
                                <asp:TextBox ID="txtDL" runat="server" />
                            </td>
              
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label3" runat="server" text="DLT:" > </asp:Label>
                            </td>
                            <td style="width:19%" class="mt-itm leftalign" >
                                <asp:TextBox ID="txtDLT" runat="server" />
                            </td>    
                        
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label23" runat="server" text="DLDac:" > </asp:Label>
                            </td>
                            <td style="width:19%" class="mt-itm leftalign" >
                                <asp:TextBox ID="txtDLDac" runat="server" />
                            </td> 
                            
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label32" runat="server" text="DLT1:" > </asp:Label>
                            </td>
                            <td class="mt-itm leftalign" >
                                <asp:TextBox ID="txtDLT1" runat="server" />
                            </td>                                      
                        </tr>
              
                        <tr >
                            <td  style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label4" runat="server" text="DH:"> </asp:Label>
                            </td>              
                            <td style="width:19%"  class="mt-itm leftalign" >                        
                                <asp:TextBox ID="txtDH" runat="server" />
                            </td>
              
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label24" runat="server" text="DHT:" > </asp:Label>
                            </td>
                            <td style="width:19%" class="mt-itm leftalign" >
                                <asp:TextBox ID="txtDHT" runat="server" />
                            </td>    
                        
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label25" runat="server" text="DHDac:" > </asp:Label>
                            </td>
                            <td style="width:19%" class="mt-itm leftalign" >
                                <asp:TextBox ID="txtDHDac" runat="server" />
                            </td>  
                            
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label36" runat="server" text="DHT1:" > </asp:Label>
                            </td>
                            <td class="mt-itm leftalign" >
                                <asp:TextBox ID="txtDHT1" runat="server" />
                            </td>    
                                    
                        </tr>
            
                        <tr class="Alt">
                            <td  style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label5" runat="server" text="SL:"> </asp:Label>
                            </td>              
                            <td style="width:19%"  class="mt-itm leftalign" >                        
                                <asp:TextBox ID="txtSL" runat="server" />
                            </td>
              
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label6" runat="server" text="SLT:" > </asp:Label>
                            </td>
                            <td style="width:19%" class="mt-itm leftalign" >
                                <asp:TextBox ID="txtSLT" runat="server" />
                            </td>    
                        
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label26" runat="server" text="SLDac:" > </asp:Label>
                            </td>
                            <td style="width:19%" class="mt-itm leftalign" >
                                <asp:TextBox ID="txtSLDac" runat="server" />
                            </td>  
                            
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label37" runat="server" text="SLT1:" > </asp:Label>
                            </td>
                            <td class="mt-itm leftalign" >
                                <asp:TextBox ID="txtSLT1" runat="server" />
                            </td>         
                        </tr>
            
                        <tr >
                             <td  style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label1" runat="server" text="SH:"> </asp:Label>
                            </td>              
                            <td style="width:19%"  class="mt-itm leftalign" >                        
                                <asp:TextBox ID="txtSH" runat="server" />
                            </td>
              
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label27" runat="server" text="SHT:" > </asp:Label>
                            </td>
                            <td style="width:19%" class="mt-itm leftalign" >
                                <asp:TextBox ID="txtSHT" runat="server" />
                            </td>    
                        
                            <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label28" runat="server" text="SHDac:" > </asp:Label>
                            </td>
                            <td style="width:19%" class="mt-itm leftalign" >
                                <asp:TextBox ID="txtSHDac" runat="server" />
                            </td> 
                            
                             <td style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label38" runat="server" text="SHT1:" > </asp:Label>
                            </td>
                            <td class="mt-itm leftalign" >
                                <asp:TextBox ID="txtSHT1" runat="server" />
                            </td>    
                                
                        </tr>
            
                    </table>  
                       
                </asp:Panel>   

                <asp:Panel ID="pnlResult" runat="server" >

                    <table class="OTable">
                                    
                        <tr>
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label7" runat="server" text="DLDoseRaw:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblDLDoseRaw" runat="server" ForeColor="Blue" />
                            </td>
              
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label8" runat="server" text="DLDoseCalc:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblDLDoseCalc" runat="server" ForeColor="Blue" />
                            </td>
                        
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label10" runat="server" text="DLCumDose:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblDLCumDose" runat="server" ForeColor="Blue" />
                            </td>
                        
                            <td  style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label30" runat="server" text="DLDose:"> </asp:Label>
                            </td>              
                            <td style="width:19%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblDLDose" runat="server" ForeColor="Blue" />
                            </td>                                      
                        </tr>
              
                        <tr class="Alt">                        
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label9" runat="server" text="DHDoseRaw:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblDHDoseRaw" runat="server" ForeColor="Blue" />
                            </td>
              
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label12" runat="server" text="DHDoseCalc:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblDHDoseCalc" runat="server" ForeColor="Blue" />
                            </td>
                        
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label14" runat="server" text="DHCumDose:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblDHCumDose" runat="server" ForeColor="Blue" />
                            </td>
                        
                            <td  style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label31" runat="server" text="DHDose:"> </asp:Label>
                            </td>              
                            <td style="width:19%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblDHDose" runat="server" ForeColor="Blue" />
                            </td>     
                        </tr>
            
                        <tr>
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label11" runat="server" text="SLDoseRaw:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblSLDoseRaw" runat="server" ForeColor="Blue" />
                            </td>
              
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label15" runat="server" text="SLDoseCalc:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblSLDoseCalc" runat="server" ForeColor="Blue" />
                            </td>
                        
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label17" runat="server" text="SLCumDose:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblSLCumDose" runat="server" ForeColor="Blue" />
                            </td>
                        
                            <td  style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label19" runat="server" text="SLDose:"> </asp:Label>
                            </td>              
                            <td style="width:19%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblSLDose" runat="server" ForeColor="Blue" />
                            </td>                                      
                        </tr>
              
                        <tr class="Alt">                        
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label21" runat="server" text="SHDoseRaw:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblSHDoseRaw" runat="server" ForeColor="Blue" />
                            </td>
              
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label29" runat="server" text="SHDoseCalc:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblSHDoseCalc" runat="server" ForeColor="Blue" />
                            </td>
                        
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label33" runat="server" text="SHCumDose:"> </asp:Label>
                            </td>              
                            <td style="width:17%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblSHCumDose" runat="server" ForeColor="Blue" />
                            </td>
                        
                            <td  style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label35" runat="server" text="SHDose:"> </asp:Label>
                            </td>              
                            <td style="width:19%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblSHDose" runat="server" ForeColor="Blue" />
                            </td>     
                        </tr>
            
                    </table>  

                     <table class="OTable">
                                    
                        <tr>
                            <td  style="width:10%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label13" runat="server" text="SLDose/DLDose:"> </asp:Label>
                            </td>              
                            <td style="width:10%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblSLD_DLD_Ratio" runat="server" ForeColor="Blue"   />
                            </td>
              
                            <td  style="width:6%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label18" runat="server" text="Pathway:"> </asp:Label>
                            </td>              
                            <td style="width:20%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblPathway" runat="server" ForeColor="Blue" />
                            </td>
                        
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label22" runat="server" text="Deep Dose:"> </asp:Label>
                            </td>              
                            <td style="width:10%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblDeepDose" runat="server" ForeColor="Blue" />
                            </td>
                        
                            <td  style="width:8%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label34" runat="server" text="Eye Dose:"> </asp:Label>
                            </td>              
                            <td style="width:10%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblEyeDose" runat="server" ForeColor="Blue" />
                            </td>   
                            
                            <td  style="width:9%" class="mt-itm-bold leftalign" >
                                <asp:Label ID="Label16" runat="server" text="Shallow Dose:"> </asp:Label>
                            </td>              
                            <td style="width:9%"  class="mt-itm leftalign" >
                                <asp:Label   ID="lblShallowDose" runat="server" ForeColor="Blue" />
                            </td>                                     
                        </tr>                                                                                                                
            
                    </table>
                                           
                </asp:Panel>  

            <%--</ContentTemplate>--%>
        <%--</asp:UpdatePanel> --%>   
    </div> 
    
      
 
</asp:Content>
