<%@ Page Title="Badge Review" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_TechOps_SerialNoReview" Codebehind="SerialNoReview.aspx.cs" %>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">

</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div>                

	    <asp:UpdatePanel ID="udpn" runat="server"  >	
            <ContentTemplate> 
                
                <div class="FormError" id="errors" runat="server" visible="false">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
                </div>

                <asp:Panel ID="pnlSearch" DefaultButton="btnFind" runat="server">
                
                    <div class="OToolbar" id="CreateLocation" runat ="server">
						<ul>								
							<li>     
								Serial#: <asp:TextBox id="txtSearchSerialNo" runat="server"> </asp:TextBox>
								<asp:Button runat="server" id="btnFind" Text="Go"  onclick="btnFind_Click" CssClass="OButton" />
							</li>
						</ul>
					</div>                 

                </asp:Panel>
       
                <asp:Panel ID="pnlSelect" runat="server" >

                  <table class="OTable">

                    <tr>
                        <th  style="width:100px" class="mt-hd centeralign" >Initial Group                        
                        </th>
                        <th style="width:100px" class="mt-hd centeralign" >Current Group                        
                        </th>
                        <th style="width:100px" class="mt-hd centeralign" >Color                        
                        </th>
                        <th style="width:100px" class="mt-hd centeralign" >Status                           
                        </th>
                        <th style="width:150px" class="mt-hd centeralign" >Calibrate from IMI                           
                        </th>          
                    </tr>
            
                    <tr>
                        <td  class="mt-itm centeralign">
                            <asp:Label   ID="lblInitalGroup" runat="server"  />
                        </td>
          
                        <td  class="mt-itm centeralign" >
                            <asp:Label ID="lblCurrentGroup" runat="server"  />
                        </td>
           
                        <td  class="mt-itm centeralign" >       
                            <asp:Label ID="lblColor" runat="server"  />               
                        </td>
            
                        <td  class="mt-itm centeralign">    
                            <asp:Label ID="lblStatus" runat="server"  />
                        </td>
             
                        <td  class="mt-itm centeralign">       
                            <asp:Label ID="lblCalibDate" runat="server"   />                                
                        </td>
            
                    </tr>

                  </table>   
                  
                </asp:Panel>
    
                <asp:Panel ID="pnlReads" runat="server" >

                  <table class="OTable" >

                    <tr>
                        <th style="width:70px;" class="mt-hd centeralign" ></th>
                        <th style="width:100px" class="mt-hd centeralign" >IMI Initial Read                        
                        </th>
                        <th style="width:100px" class="mt-hd centeralign" >Pressure Read 1                        
                        </th>
                        <th style="width:100px" class="mt-hd centeralign">Pressure Read 2                        
                        </th>
                        <th style="width:100px" class="mt-hd centeralign">Verification                            
                        </th>
                        <th style="width:150px" class="mt-hd centeralign" >PreDrift                            
                        </th>
                        <th style="width:100px" class="mt-hd centeralign" >PostDrift                            
                        </th>
                    </tr>
            
                    <tr >
                      <td  class="Label" >
                        <asp:Label ID="Label20" runat="server" text="Date:" style="width:50px"> </asp:Label>
                      </td>
                      <td  class="mt-itm centeralign" >
                        <asp:Label ID="lblDateIMIInitialRead" runat="server" > </asp:Label>
                      </td>
                      <td  class="mt-itm centeralign" >
                        <asp:Label ID="lblDatePressureRead1" runat="server" > </asp:Label>
                      </td> 
                      <td  class="mt-itm centeralign" >
                        <asp:Label ID="lblDatePressureRead2" runat="server" > </asp:Label>
                      </td>
                      <td  class="mt-itm centeralign" >
                        <asp:Label ID="lblDateVerification" runat="server" > </asp:Label>
                      </td> 
                      <td  class="mt-itm centeralign" >
                        <asp:Label ID="lblDatePreDrift" runat="server" > </asp:Label>
                      </td> 
                      <td  class="mt-itm centeralign" >
                        <asp:Label ID="lblDatePostDrift" runat="server" > </asp:Label>
                      </td> 
                    </tr>
            
                    <tr class="Alt">
                      <td class="Label" >
                        <asp:Label ID="Label14" runat="server" text="Deep Low:" style="width:50px"> </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLIMIInitialRead" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLPressureRead1" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLPressureRead2" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLVerification" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLPreDrift" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLPostDrift" runat="server" > </asp:Label>
                      </td> 
                    </tr>
            
                    <tr >
                      <td class="Label" >
                        <asp:Label ID="Label15" runat="server" text="Deep Low Temp:" style="width:50px"> </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLtIMIInitialRead" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLtPressureRead1" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLtPressureRead2" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLtVerification" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLtPreDrift" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLtPostDrift" runat="server" > </asp:Label>
                      </td> 
                    </tr>
            
                    <tr class="Alt">
                      <td class="Label">
                        <asp:Label ID="Label16" runat="server" text="Deep Low Dose:" style="width:50px"> </asp:Label>
                      </td>
                      <td style="width:100px; text-align:center">
                        <asp:Label ID="lblDLDoseIMIInitialRead" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLDosePressureRead1" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLDosePressureRead2" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLDoseVerification" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLDosePreDrift" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDLDosePostDrift" runat="server" > </asp:Label>
                      </td> 
                    </tr>
            
                    <tr >
                      <td class="Label" >
                        <asp:Label ID="Label17" runat="server" text="Deep High:" style="width:50px"> </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHIMIInitialRead" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHPressureRead1" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHPressureRead2" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHVerification" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHPreDrift" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHPostDrift" runat="server" > </asp:Label>
                      </td> 
                    </tr>
            
                    <tr class="Alt">
                      <td class="Label" >
                        <asp:Label ID="Label18" runat="server" text="Deep High Temp:" style="width:50px"> </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHtIMIInitialRead" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHtPressureRead1" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHtPressureRead2" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHtVerification" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHtPreDrift" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHtPostDrift" runat="server" > </asp:Label>
                      </td> 
                    </tr>
            
                    <tr >
                      <td class="Label" >
                        <asp:Label ID="Label19" runat="server" text="Deep High Dose:" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHDoseIMIInitialRead" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHDosePressureRead1" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHDosePressureRead2" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHDoseVerification" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHDosePreDrift" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign" >
                        <asp:Label ID="lblDHDosePostDrift" runat="server" > </asp:Label>
                      </td>
                    </tr>
           
                   <%-- <tr> 
                        <td class="mt-itm centeralign" ></td>
                        <th class="mt-hd centeralign" >
                        <asp:Label ID="Label13" runat="server" text="DLcomp" > </asp:Label>
                        </th>
                        <th class="mt-hd centeralign" >
                        <asp:Label ID="Label21" runat="server" text="Irradiation" > </asp:Label>
                        </th>
                        <td class="mt-itm centeralign" >
                        <asp:Label ID="Label22" runat="server" text="" > </asp:Label>
                        </td>
                        <td class="mt-itm centeralign" >
                            <asp:Label ID="Label23" runat="server" text="" > </asp:Label>
                        </td>
                        <th class="mt-hd centeralign" >
                            <asp:Label ID="Label24" runat="server" text="DL Diff" > </asp:Label>
                        </th>
                        <td class="mt-itm centeralign" >
                            <asp:Label ID="Label25" runat="server" text="" > </asp:Label>
                        </td>
                    </tr>
            
                     <tr>
                        <td class="mt-itm centeralign" >
                        <asp:Label ID="Label26" runat="server" text="" > </asp:Label>
                        </td>
                        <td class="mt-itm centeralign">
                        <asp:Label ID="lblDLcomp" runat="server" > </asp:Label>
                        </td>
                        <td class="mt-itm centeralign">
                        <asp:Label ID="lblIrradiation" runat="server" style="width:100px; text-align:center"> </asp:Label>
                        </td> 
                        <td class="mt-itm centeralign">
                        <asp:Label ID="Label29" runat="server" > </asp:Label>
                        </td>
                        <td class="mt-itm centeralign">
                        <asp:Label ID="Label30" runat="server" > </asp:Label>
                        </td> 
                        <td class="mt-itm centeralign">
                        <asp:Label ID="lblDLDiff" runat="server" > </asp:Label>
                        </td> 
                        <td class="mt-itm centeralign">
                        <asp:Label ID="Label32" runat="server" > </asp:Label>
                        </td> 
                    </tr>
            
                    <tr>
                     <td class="mt-itm centeralign"></td>
                     <th class="mt-hd centeralign">
                       <asp:Label ID="Label27" runat="server" text="DHcomp" > </asp:Label>
                     </th>
                     <th class="mt-hd centeralign">
                       <asp:Label ID="Label28" runat="server" text="Atmosphere" > </asp:Label>
                     </th>
                     <td class="mt-itm centeralign">
                       <asp:Label ID="Label31" runat="server" text="" > </asp:Label>
                     </td>
                     <td class="mt-itm centeralign">
                          <asp:Label ID="Label33" runat="server" text="" > </asp:Label>
                     </td>
                     <th class="mt-hd centeralign">
                          <asp:Label ID="Label34" runat="server" text="Z DL" > </asp:Label>
                     </th>
                      <td class="mt-itm centeralign">
                          <asp:Label ID="Label35" runat="server" text="" > </asp:Label>
                     </td>
                    </tr>
            
                     <tr>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label36" runat="server" text=""> </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="lblDHcomp" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="lblAtmosphere" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label39" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label40" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="lblZDL" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label42" runat="server" > </asp:Label>
                      </td> 
                    </tr>
            
                    <tr>
                        <td class="mt-itm centeralign"></td>
                        <td  class="mt-itm centeralign">
                        <asp:Label ID="Label37" runat="server" text="" > </asp:Label>
                        </td>
                        <th class="mt-hd centeralign">
                        <asp:Label ID="Label38" runat="server" text="Chamber Press" > </asp:Label>
                        </th>
                        <td class="mt-itm centeralign">
                        <asp:Label ID="Label41" runat="server" text="" > </asp:Label>
                        </td>
                        <td class="mt-itm centeralign">
                            <asp:Label ID="Label43" runat="server" text="" > </asp:Label>
                        </td>
                        <th class="mt-hd centeralign">
                            <asp:Label ID="Label44" runat="server" text="DH Diff" > </asp:Label>
                        </th>
                        <td class="mt-itm centeralign">
                            <asp:Label ID="Label45" runat="server" text="" > </asp:Label>
                        </td>
                    </tr>
            
                     <tr>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label46" runat="server" text=""> </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="xlblChamberPressure" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="lblChamberPressure" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label49" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="xlblDHDiff" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="lblDHDiff" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label52" runat="server" > </asp:Label>
                      </td> 
                    </tr>
            
                    <tr>
                     <td class="mt-itm centeralign"></td>
                     <td  class="mt-itm centeralign">
                       <asp:Label ID="Label47" runat="server" text="" > </asp:Label>
                     </td>
                     <th class="mt-hd centeralign">
                       <asp:Label ID="Label50" runat="server" text="Chamber #" > </asp:Label>
                     </th>
                     <td class="mt-itm centeralign">
                       <asp:Label ID="Label53" runat="server" text="" > </asp:Label>
                     </td>
                     <td class="mt-itm centeralign">
                          <asp:Label ID="Label54" runat="server" text="" > </asp:Label>
                     </td>
                     <th class="mt-hd centeralign">
                          <asp:Label ID="Label55" runat="server" text="Z DH" > </asp:Label>
                     </th>
                      <td class="mt-itm centeralign">
                          <asp:Label ID="Label56" runat="server" text="" > </asp:Label>
                     </td>
                    </tr>
            
                     <tr>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label57" runat="server" text="" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="xlblChamberNo" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="lblChamberNo" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label60" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="xlblZDH" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="lblZDH" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label63" runat="server" > </asp:Label>
                      </td> 
                    </tr>
            
                    <tr>
                     <td class="mt-itm centeralign"></td>
                     <td class="mt-itm centeralign">
                       <asp:Label ID="Label58" runat="server" text="" > </asp:Label>
                     </td>
                     <th class="mt-hd centeralign">
                       <asp:Label ID="Label61" runat="server"  text="Slot #" > </asp:Label>
                     </th>
                     <td class="mt-itm centeralign">
                       <asp:Label ID="Label64" runat="server" text="" > </asp:Label>
                     </td>
                     <td class="mt-itm centeralign">
                          <asp:Label ID="Label65" runat="server" text="" > </asp:Label>
                     </td>
                     <td class="mt-itm centeralign">
                          <asp:Label ID="Label66" runat="server" text="" > </asp:Label>
                     </td>
                      <td class="mt-itm centeralign">
                          <asp:Label ID="Label67" runat="server" text="" > </asp:Label>
                     </td>
                    </tr>
            
                     <tr>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label68" runat="server" text="" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="xlblSlotNo" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="lblSlotNo" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label71" runat="server" > </asp:Label>
                      </td>
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label72" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label73" runat="server" > </asp:Label>
                      </td> 
                      <td class="mt-itm centeralign">
                        <asp:Label ID="Label74" runat="server" > </asp:Label>
                      </td> 
                    </tr>       --%>                             
            
                  </table> 

                  <table class="OTable">
                    <tr>
                        <td class="Label" style="width:13%">
                            Irradiation:
                        </td>
                        <td class="mt-itm" style="width:20%">
                            <asp:Label ID="lblIrradiation" runat="server" style="width:100px; text-align:center"> </asp:Label>
                        </td>
                        <td class="Label" style="width:13%">
                            DL Diff:
                        </td>
                        <td class="mt-itm" style="width:20%">
                            <asp:Label ID="lblDLDiff" runat="server" > </asp:Label>
                        </td>
                        <td class="Label" style="width:13%">
                            DL Comp:
                        </td>
                        <td class="mt-itm">
                            <asp:Label ID="lblDLcomp" runat="server" > </asp:Label>
                        </td>
                    </tr>
                    <tr class="Alt">
                        <td class="Label">
                            Atmosphere:
                        </td>
                        <td class="mt-itm">
                            <asp:Label ID="lblAtmosphere" runat="server" > </asp:Label>
                        </td>
                        <td class="Label">
                            Z DL:
                        </td>
                        <td class="mt-itm">
                            <asp:Label ID="lblZDL" runat="server" > </asp:Label>
                        </td>
                        <td class="Label">
                            DH Comp:
                        </td>
                        <td class="mt-itm">
                            <asp:Label ID="lblDHcomp" runat="server" > </asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">
                            Chamber Pressure:
                        </td>
                        <td class="mt-itm">
                            <asp:Label ID="lblChamberPressure" runat="server" > </asp:Label>
                        </td>
                        <td class="Label">
                            DH Diff:
                        </td>
                        <td class="mt-itm">
                            <asp:Label ID="lblDHDiff" runat="server" > </asp:Label>
                        </td>
                        <td class="Label">
                        
                        </td>
                        <td class="mt-itm">
                        
                        </td>
                    </tr>
                    <tr class="Alt">
                        <td class="Label">
                            Chamber#:
                        </td>
                        <td class="mt-itm">
                            <asp:Label ID="lblChamberNo" runat="server" > </asp:Label>
                        </td>
                        <td class="Label">
                            Z DH:
                        </td>
                        <td class="mt-itm">
                            <asp:Label ID="lblZDH" runat="server" > </asp:Label>
                        </td>
                        <td class="Label">
                        
                        </td>
                        <td class="mt-itm">
                        
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">
                            Slot#:
                        </td>
                        <td class="mt-itm">
                            <asp:Label ID="lblSlotNo" runat="server" > </asp:Label>
                        </td>
                        <td class="Label">
                        
                        </td>
                        <td class="mt-itm">
                        
                        </td>
                        <td class="Label">
                        
                        </td>
                        <td class="mt-itm">
                        
                        </td>
                    </tr>
                  </table>
                                 
            </asp:Panel>

            </ContentTemplate>
        </asp:UpdatePanel> 
                
    </div>		



  
      
</asp:Content>
