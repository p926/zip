<%@ Page Title="Drift Analysis" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_DriftAnalysis" Codebehind="DriftAnalysis.aspx.cs" %>
    

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">

        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(showProgress);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(hideProgress);           
        });
        
        function showProgress(sender, args) {
            var c = '<%=modalExtender.ClientID %>';
            $find(c).show();            
        }

        function hideProgress(sender, args) {
            var c = '<%=modalExtender.ClientID %>';
            $find(c).hide();
        }

    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdateProgress id="DriftAnalysisUpdateProgress" runat="server" DynamicLayout="true" DisplayAfter="0" >
        <ProgressTemplate>
            <%--<div style="width:100%" align="center">
                <img src="../images/barloader.gif" />
            </div>--%>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <act:ModalPopupExtender ID="modalExtender" runat="server" TargetControlID="DriftAnalysisUpdateProgress"
        PopupControlID="Panel1" BackgroundCssClass="modalBackground" Enabled="true" >
    </act:ModalPopupExtender>

    <asp:Panel ID="Panel1" runat="server">
        <div style="width: 100%" align="center">
            <img src="../images/orangebarloader.gif" />
        </div>
    </asp:Panel>

    <div>                

		<asp:UpdatePanel ID="UpdatePanelDriftAnalysis" runat="server"  >			

			<ContentTemplate>               
				
                <div class="FormError" id="errors" runat="server" visible="false">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
                </div>
                <div class="FormMessage" id="messages" runat="server" visible="false"> 
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="submitMsg" runat ="server" >Ready to search.</span></p>
                </div> 

                <asp:Panel ID="pnlDevices" Visible="false" runat="server">
                
                    
                    <div class="OToolbar" id="DriftToolBar" runat ="server">
					    <ul>
						    <li>

                                <asp:LinkButton ID="btnPerformDriftAnalysis" runat="server"   
						            CssClass="Icon LightningGo" onclick="btnPerformDriftAnalysis_Click" >Perform Drift Analysis</asp:LinkButton>  
							    
                                <asp:LinkButton ID="btnResetGroup" runat="server"   
						            CssClass="Icon Refresh" onclick="btnResetGroup_Click" >Reset Drift Group</asp:LinkButton>  
                                
                                <asp:LinkButton ID="btnAccept" runat="server"   
						            CssClass="Icon Check" onclick="btnAccept_Click" >Accept</asp:LinkButton>  

                                <asp:LinkButton ID="btnDriftReport" runat="server"   
						            CssClass="Icon Report" onclick="btnDriftReport_Click" >View Drift Report</asp:LinkButton>  
                                
						    </li> 
                            
                            <li id="InventoryToolBar" runat ="server" >

                                <asp:LinkButton ID="btnCommitInventory" runat="server"   
						            CssClass="Icon Save" onclick="btnCommitInventory_Click" >Commit Inventory</asp:LinkButton> 
							    
                                <asp:LinkButton ID="btnInventoryReport" runat="server"   
						            CssClass="Icon Report" onclick="btnInventoryReport_Click" >View Inventory Report</asp:LinkButton>  
						    </li>                                                    
					    </ul>                    
				    </div>                     
        
                </asp:Panel>
                                                                               
                <div class="OToolbar JoinTable" id="SelectGroupToolBar" runat ="server">
					<ul>
						<li>
							<asp:Label ID="Label4" runat="server" Text="Select Group:"></asp:Label>   
                            <asp:DropDownList ID="ddlGroupName" runat="server" AutoPostBack="True" 
                                OnSelectedIndexChanged="ddlGroupName_SelectedIndexChanged" 
                                DataSourceID="ldsGroupNames" DataTextField="DeviceGroupName" 
                                DataValueField="DeviceGroupID">
                            </asp:DropDownList>
						</li>                                                   
					</ul>                    
				</div>                   
				               
                    	
				<asp:GridView ID="gvDevices" runat="server" AutoGenerateColumns="False" DataSourceID="ldsDevices"
                    PageSize="20" AllowPaging="True" CssClass="OTable" AllowSorting="true" >

                    <Columns>
                        <asp:BoundField DataField="DeviceID" HeaderText="Badge" ReadOnly="True" SortExpression="DeviceID"
                            HeaderStyle-Width="60" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" />
                                                
                        <asp:BoundField DataField="SerialNo" HeaderText="Serial#" ReadOnly="True" SortExpression="SerialNo"
                            HeaderStyle-Width="60" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd" />
                                                
                        <asp:BoundField DataField="SKU" HeaderText="SKU/Color" ReadOnly="True" SortExpression="SKU"
                            HeaderStyle-Width="80" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                                                
                        <asp:BoundField DataField="Status" HeaderText="Status" ReadOnly="True" SortExpression="Status"
                            HeaderStyle-Width="100" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                                                
                        <asp:BoundField DataField="CalibDate" DataFormatString="{0:MM/dd/yy h:mm tt}" HeaderText="Calibrated"
                            ReadOnly="True" SortExpression="CalibDate" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />

                        <asp:BoundField DataField="PreReadDate" DataFormatString="{0:MM/dd/yy h:mm tt}" HeaderText="Pre-read"
                            ReadOnly="True" SortExpression="PreReadDate" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />

                        <asp:BoundField DataField="PostReadDate" DataFormatString="{0:MM/dd/yy h:mm tt}"
                            HeaderText="Post-read" ReadOnly="True" SortExpression="PostReadDate" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />

                    </Columns>

                    <EmptyDataTemplate>
						<div class="NoData">
							There are no valid devices in this group!
						</div>
					</EmptyDataTemplate>  
                                                  
                    <AlternatingRowStyle CssClass="Alt" />
					<PagerStyle CssClass="Footer" />

                </asp:GridView>                

			</ContentTemplate>

		</asp:UpdatePanel>   
					
	</div>
            

    <asp:SqlDataSource ID="ldsGroupNames" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand=" 
                        Select 0 as DeviceGroupID, '' as  DeviceGroupName
                        Union
                        select DeviceGroupID, DeviceGroupName  
                        from DeviceGroups 
                        order by DeviceGroupName 
                      " >

    </asp:SqlDataSource>

    <asp:SqlDataSource ID="ldsDevices" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand=" 
                        select 
                        a.DeviceID
                        , a.SerialNo 
                        ,b.DeviceAnalysisName AS Status
                        ,c.ProductSKU AS SKU
                        ,r1.ExposureDate AS CalibDate
                        ,r2.ExposureDate AS PostReadDate
                        ,r3.ExposureDate AS PreReadDate
                        from DeviceInventory a
                        Join DeviceAnalysisStatus b On a.DeviceAnalysisStatusID = b.DeviceAnalysisStatusID 
                        Join Products c On a.ProductID = c.ProductID 
                        Left Join UserDeviceReads r1 On a.CalibrationReadID = r1.RID
                        Left Join UserDeviceReads r2 On a.PostReadID  = r2.RID
                        Left Join UserDeviceReads r3 On a.PreReadID  = r3.RID
                        WHERE a.CurrentGroupID = @CurrentGroupID
                        and a.FailedCalibration = 0
                        Order By a.SerialNo 
                      " >
                                    
        <SelectParameters>            
            <asp:ControlParameter ControlID="ddlGroupName" DefaultValue="0" 
                Name="CurrentGroupID" PropertyName="SelectedItem.Value" Type="String" />
        </SelectParameters>

    </asp:SqlDataSource> 
    
</asp:Content>
