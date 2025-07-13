<%@ Page Title="Pressure Test Result" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_TechOps_PressureTestResults" Codebehind="PressureTestResults.aspx.cs" %>

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

                <asp:Panel ID="pnlSearch" runat="server" DefaultButton="btnPrint" >                                        

                    <div class="OToolbar" id="CreateLocation" runat ="server">
						<ul>								
							<li>     
								Badge Group: 
                                <asp:DropDownList ID="ddlSelectGroup" runat="server" AutoPostBack="True" 
                                    DataSourceID="ldsGroupNames"
                                    DataTextField="DeviceGroupName" 
                                    DataValueField="DeviceGroupID" 
                                    OnSelectedIndexChanged="DDLSelectGroup_SelectedIndexChanged" >
                                </asp:DropDownList>
                            </li>
                            <li>
                                <asp:LinkButton ID="btnPrint" runat="server"   
						            CssClass="Icon Print" onclick="btnPrint_Click" >Print</asp:LinkButton>  								
							</li>
						</ul>
					</div>    
         
                </asp:Panel>
        
                <asp:Panel ID="PrintPanel" runat="server">

                    <asp:Panel ID="pnlSelect" runat="server" >

                      <table class="OTable">
                                    
                        <tr>
                          <td  style="width:12%" class="mt-itm-bold leftalign" >
                            <asp:Label ID="Label2" runat="server" text="Group:"> </asp:Label>
                          </td>
              
                          <td style="width:38%"  class="mt-itm leftalign" >
                             <asp:Label   ID="lblInitialGroup" runat="server" />
                          </td>
              
                          <td style="width:8%" class="mt-itm-bold leftalign" >
                            <asp:Label ID="Label3" runat="server" text="Passed P1:" > </asp:Label>
                          </td>

                          <td class="mt-itm leftalign" >
                            <asp:Label   ID="lblQtyPassedP1" runat="server" />
                          </td>                                        
                        </tr>
              
                        <tr class="Alt">
                          <td class="mt-itm-bold leftalign">
                               <asp:Label ID="Label51" runat="server" text="Report Date:" > </asp:Label>
                          </td>
              
                            <td class="mt-itm leftalign" >
                               <asp:Label   ID="lblReportDate" runat="server" Width="100px"/>
                            </td>                                        
              
                          <td class="mt-itm-bold leftalign" >
                            <asp:Label ID="Label4" runat="server" text="Failed P1:" > </asp:Label>
                          </td>
                         
                            <td class="mt-itm leftalign" >
                                <asp:Label   ID="lblQtyFailedP1" runat="server" />
                          </td>
             
                          
                        </tr>
            
                        <tr>
                          <td class="mt-itm-bold leftalign" >
                               <asp:Label ID="Label5" runat="server" text="No. of Badges:" /> 
                          </td>
              
                          <td class="mt-itm leftalign" >
                             <asp:Label ID="lblNumberOfDevices" runat="server"  />
                          </td>
              
                          <td class="mt-itm-bold leftalign" >
                               <asp:Label ID="Label6" runat="server" text="Passed P2:" /> 
                          </td>
              
                          <td class="mt-itm leftalign" >
                            <asp:Label   ID="lblQtyPassedP2" runat="server" />
                          </td>

                          
                        </tr>
            
                        <tr class="Alt">
                          <td class="mt-itm-bold leftalign" >
                               <asp:Label ID="Label70" runat="server" text="Irradiation Value:" > </asp:Label>
                          </td>
                          <td class="mt-itm leftalign" >
                             <asp:Label   ID="lblIrradiationValue" runat="server" />
                          </td>
                          <td class="mt-itm-bold leftalign" >
                            <asp:Label ID="Label1" runat="server" text="Failed P2:" > </asp:Label>
                          </td>
                          <td class="mt-itm leftalign" >
                                <asp:Label   ID="lblQtyFailedP2" runat="server" />
                          </td>

                        </tr>
            
                      </table>  
                       
                    </asp:Panel>
    
                    <asp:Panel ID="pnlReads" runat="server" >                                                

                        <asp:GridView ID="gvPressureTests" runat="server" 
                            AutoGenerateColumns="False" 
                            CssClass="OTable" >

                            <Columns>
                                <asp:TemplateField HeaderText="Chamber#" 
                                    ItemStyle-CssClass="mt-itm rightalign"  HeaderStyle-CssClass="mt-hd " >
                                    <ItemTemplate>
                                        <asp:Label ID="lblChamber" runat="server" Text='<%# Bind("ChamberNumber") %>'></asp:Label>
                                    </ItemTemplate>                                
                                </asp:TemplateField>
                      
                                <asp:TemplateField HeaderText="Slot"  
                                    ItemStyle-CssClass="mt-itm rightalign"  HeaderStyle-CssClass="mt-hd ">
                                    <ItemTemplate>
                                        <asp:Label ID="lblSlot" runat="server" Text='<%# Bind("SlotNumber") %>'></asp:Label>
                                    </ItemTemplate>                                
                                </asp:TemplateField>
                     
                                <asp:TemplateField HeaderText="Atmosphere"  
                                    ItemStyle-CssClass="mt-itm rightalign"  HeaderStyle-CssClass="mt-hd " >
                                    <ItemTemplate>
                                        <asp:Label ID="lblAtmospherePressure" runat="server" Text='<%# Bind("AtmospherePressure") %>'></asp:Label>
                                    </ItemTemplate>                                
                                </asp:TemplateField>
                     
                                <asp:TemplateField HeaderText="Chamber Pressure" HeaderStyle-Wrap="false"
                                    ItemStyle-CssClass="mt-itm rightalign"  HeaderStyle-CssClass="mt-hd " >
                                    <ItemTemplate>
                                        <asp:Label ID="lblChamberPressure" runat="server" Text='<%# Bind("ChamberPressure") %>'></asp:Label>
                                    </ItemTemplate>                                
                                </asp:TemplateField>
                      
                                <asp:TemplateField HeaderText="Serial#"  
                                    ItemStyle-CssClass="mt-itm rightalign"  HeaderStyle-CssClass="mt-hd " >
                                    <ItemTemplate>
                                        <asp:Label ID="lblSerialNo" runat="server" Text='<%# Bind("SerialNo") %>'></asp:Label>
                                    </ItemTemplate>                                
                                </asp:TemplateField>
                     
                                <asp:TemplateField HeaderText="P1 Date"   
                                    ItemStyle-CssClass="mt-itm "  HeaderStyle-CssClass="mt-hd" >
                                    <ItemTemplate>
                                        <asp:Label ID="lblP1Date" runat="server" Text='<%# Bind("P1CreatedDate") %>'></asp:Label>
                                    </ItemTemplate>                                
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="P1 Calc Read"  
                                    ItemStyle-CssClass="mt-itm rightalign"  HeaderStyle-CssClass="mt-hd " >
                                    <ItemTemplate>
                                        <asp:Label ID="lblP1CalcRead" runat="server" Text='<%# Bind("P1DeliveredDose") %>'></asp:Label>
                                    </ItemTemplate>                                
                                </asp:TemplateField>
                      
                                <asp:TemplateField HeaderText="P2 Date"  
                                    ItemStyle-CssClass="mt-itm "  HeaderStyle-CssClass="mt-hd " >
                                    <ItemTemplate>
                                        <asp:Label ID="lblP2Date" runat="server" Text='<%# Bind("P2CreatedDate") %>'></asp:Label>
                                    </ItemTemplate>                                
                                </asp:TemplateField>                                                                                                          
                      
                                <asp:TemplateField HeaderText="P2 Calc Read"   
                                    ItemStyle-CssClass="mt-itm rightalign"  HeaderStyle-CssClass="mt-hd " >
                                    <ItemTemplate>
                                        <asp:Label ID="lblP2CalcRead" runat="server" Text='<%# Bind("P2DeliveredDose") %>'></asp:Label>
                                    </ItemTemplate>                                
                                </asp:TemplateField>

                                 <asp:TemplateField HeaderText="Corrected Value"   
                                    ItemStyle-CssClass="mt-itm rightalign"  HeaderStyle-CssClass="mt-hd " >
                                    <ItemTemplate>
                                        <asp:Label ID="lblCorrectedValue" runat="server" Text='<%# Bind("ComparedDose") %>'></asp:Label>
                                    </ItemTemplate>                                
                                </asp:TemplateField>
                      
                                <asp:TemplateField HeaderText="Status"  
                                    ItemStyle-CssClass="mt-itm "  HeaderStyle-CssClass="mt-hd " >
                                <ItemTemplate>
                                    <asp:Label ID="lblPassFail" runat="server" Text='<%# Bind("Status") %>'></asp:Label>
                                </ItemTemplate>                                
                                </asp:TemplateField>
                 
                            </Columns>

                            <EmptyDataTemplate>
						        <div class="NoData">
							        There are no Pressure Test in this group!
						        </div>
					        </EmptyDataTemplate>  
                                                  
                            <AlternatingRowStyle CssClass="Alt" />
					        <PagerStyle CssClass="Footer" />
                  
                        </asp:GridView>
                      
                    </asp:Panel>

                 </asp:Panel>  

            <%--</ContentTemplate>--%>
        <%--</asp:UpdatePanel> --%>   
    </div> 
    
    <asp:SqlDataSource ID="ldsGroupNames" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand=" 
                        Select 0 as DeviceGroupID, '' as  DeviceGroupName
                        Union
                        select distinct b.DeviceGroupID, b.DeviceGroupName   
                        from dbo.CalibPressureTestHeaders a
                        Join DeviceGroups b On a.DeviceGroupID = b.DeviceGroupID  
                        Order by DeviceGroupName
                      " >

    </asp:SqlDataSource>        
 
</asp:Content>
