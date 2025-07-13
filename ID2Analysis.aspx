<%@ Page Title="ID2 Analysis" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="ID2Analysis.aspx.cs" Inherits="portal_instadose_com_v3.TechOps.ID2Analysis" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager> 

    <asp:UpdateProgress id="ScheduleReadGroupUpdateProgress" runat="server" DynamicLayout="true" DisplayAfter="0" >
        <ProgressTemplate>
            
        </ProgressTemplate>
    </asp:UpdateProgress>

    <act:ModalPopupExtender ID="modalExtender" runat="server" TargetControlID="ScheduleReadGroupUpdateProgress"
        PopupControlID="Panel1" BackgroundCssClass="modalBackground" Enabled="true" >
    </act:ModalPopupExtender>

    <asp:Panel ID="Panel1" runat="server">
        <div style="width: 100%" align="center" >
            <img src="../images/orangebarloader.gif" />
        </div>
    </asp:Panel>

    <div>

		<asp:UpdatePanel ID="UpdatePanelScheduleReadGroup" runat="server"  UpdateMode="Conditional">			
			<ContentTemplate>

				<div class="OToolbar JoinTable" id="ScheduleReadGroupSearchToolBar" runat ="server">
					<ul>                                            					                                                   
                        <li>Group Search:</li>
                        <li><asp:TextBox ID="txtGroup" runat="server" class="Size Small" Text=""></asp:TextBox></li>
                        <li><asp:Button ID="btnGroupSearch" runat="server" Text="Go" CssClass="OButtonFormDetail" /></li>
					</ul>
				</div>

                <asp:GridView ID="grdScheduleReadGroupsView" CssClass="OTable"  runat="server" Visible ="true"
                    AllowPaging="true" PageSize = "20" AllowSorting="true"
                    AutoGenerateColumns="False" DataKeyNames="ScheduleReadGroupID" DataSourceID="GetScheduleReadGroups" >
                    <Columns>                                                                                                                                       
                            <asp:BoundField DataField="DeviceGroupName" HeaderText="Group"  SortExpression="DeviceGroupName" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                           
                            <asp:BoundField DataField="ScheduleName" HeaderText="Schedule Name" SortExpression="ScheduleName" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                                                          
                            <asp:BoundField DataField="StartDate" HeaderText="Start Date" SortExpression="StartDate" DataFormatString="{0:d}" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                                                                                    

                             <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="" HeaderStyle-Width="120px">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hyprlnkCalibrationAnalysis" runat="server" NavigateUrl='<%# String.Format("ID2CalibrationVerificationAnalysis.aspx?AnalysisType={0}&DeviceGroupID={1}", "Calibration", Eval("DeviceGroupID")) %>' Text='Calibration Anls' ></asp:HyperLink>
                                    <asp:Image ID="imgCalibrationAnalysis" runat="server" ImageUrl="../images/tick.png" Visible='<%# Convert.ToBoolean (Eval("CalAnls")) %>' /> 
                                </ItemTemplate>
                            </asp:TemplateField>  
                            
                            <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="" HeaderStyle-Width="120px">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hyprlnkPostDriftAnalysis" runat="server" NavigateUrl='<%# String.Format("ID2PostDriftAnalysis.aspx?AnalysisType={0}&DeviceGroupID={1}", "PostDrift", Eval("DeviceGroupID")) %>' Text='Post Drift Anls' ></asp:HyperLink>
                                    <asp:Image ID="imgPostDriftAnalysis" runat="server" ImageUrl="../images/tick.png" Visible='<%# Convert.ToBoolean(Eval("DriftAnls")) %>' /> 
                                </ItemTemplate>
                            </asp:TemplateField>  

                            <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="" HeaderStyle-Width="120px">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hyprlnkBackgroundTestAnalysis" runat="server" NavigateUrl='<%# String.Format("ID2BackgroundTestAnalysis.aspx?AnalysisType={0}&DeviceGroupID={1}", "BackgroundTest", Eval("DeviceGroupID")) %>' Text='Background Anls' ></asp:HyperLink>
                                    <asp:Image ID="imgBackgroundTestAnalysis" runat="server" ImageUrl="../images/tick.png" Visible='<%# Convert.ToBoolean(Eval("BackgroundAnls")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>   
                            
                            <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="" HeaderStyle-Width="100px">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hyprlnkOverallAnalysis" runat="server" NavigateUrl='<%# String.Format("ID2OverallAnalysis.aspx?DeviceGroupID={0}", Eval("DeviceGroupID")) %>' Text='Overall Anls' ></asp:HyperLink>
                                    <asp:Image ID="imgOverallAnalysis" runat="server" ImageUrl="../images/tick.png" Visible='<%# Convert.ToBoolean(Eval("OverallAnls")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>                          
                           
                    </Columns>

                    <EmptyDataTemplate>
						<div class="NoData">
							There are no groups scheduled to read!
						</div>
					</EmptyDataTemplate>                                
                    <AlternatingRowStyle CssClass="Alt" />
					<PagerStyle CssClass="Footer" />
                        
                </asp:GridView>	
                								

			</ContentTemplate>
		</asp:UpdatePanel>   
					
	</div>

    <asp:SqlDataSource ID="GetScheduleReadGroups" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_GetAllScheduleReadGroups" SelectCommandType="StoredProcedure" > 
        <SelectParameters>
            <asp:ControlParameter ControlID="txtGroup" DefaultValue="" ConvertEmptyStringToNull="false" Name="SearchGroup" PropertyName="Text" Type="String" />             
        </SelectParameters>       
    </asp:SqlDataSource>	

</asp:Content>
