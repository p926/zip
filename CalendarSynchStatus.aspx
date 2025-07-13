<%@ Page Title="Calendar Synch Status" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_CalendarSynchStatus" Codebehind="CalendarSynchStatus.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <style type="text/css">
        .synch
        {                        
            background-image:url('../images/Success.png');
            background-repeat:no-repeat; 
            background-position:left center;                                   
        }
        .notsynch
        {                       
            background-image:url('../images/Fail.png');
            background-repeat:no-repeat;  
            background-position:left center;                                     
        }
        .processing
        {                        
            background-image:url('../images/icons/clock_red.png');
            background-repeat:no-repeat;   
            background-position:left center;                                    
        }
        
    </style>
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

<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" runat="Server">
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

    <div >
        <table class="OTable" >                                                
              
            <tr>
                <td style="width: 10%" >
                    <asp:Label ID="Label1" runat = "server" Text="Device Group:" />
                </td>
                <td class="mt-itm-bold LeftAlign" style="width: 10%" >
                    <asp:Label ID="group" runat = "server"   />
                </td>
                <td style="width: 10%">
                    <asp:Label ID="Label2" runat = "server" Text="Schedule Name:"  />
                </td>
                <td class="mt-itm-bold leftalign " style="width: 30%">
                    <asp:Label ID="scheduleName" runat = "server"  />
                </td>
                <td style="width: 10%">
                    <asp:Label ID="Label3" runat = "server" Text="Start Date:"  />
                </td>
                <td class="mt-itm-bold LeftAlign" style="width: 10%">
                    <asp:Label ID="startDate" runat = "server" />
                </td>
                <td style="width: 10%">
                    <asp:Label ID="Label4" runat = "server" Text="Start Time:"  />
                </td>
                <td class="mt-itm-bold LeftAlign" >
                    <asp:Label ID="startTime" runat = "server"  />
                </td>
            </tr>                

        </table> 
    </div>  

    <div>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server"  UpdateMode="Conditional">
			<Triggers>
				<asp:AsyncPostBackTrigger controlid="btn_refresh" eventname="Click" />
			</Triggers>

			<ContentTemplate>
                <asp:GridView ID="grdStatisticResultView" CssClass="OTable"  runat="server" Width = "300px">                                

                    <EmptyDataTemplate>
				        <div class="NoData">
					        There are no available statistic data!
				        </div>
			        </EmptyDataTemplate>                                
                    <AlternatingRowStyle CssClass="Alt" />
			        <PagerStyle CssClass="Footer" />
                        
                </asp:GridView>	
            </ContentTemplate>
        </asp:UpdatePanel>         
    </div>

    <div class="Buttons">        
        <div class="ButtonHolder">            
            <asp:Button ID="btnBackTop" CssClass="OButton" runat="server" Text="Back"
                onclick="btnBack_Click" />  
        </div>
        <div class="Clear"> </div>
    </div> 

    <div>

		<asp:UpdatePanel ID="UpdatePanelScheduleReadGroup" runat="server"  UpdateMode="Conditional">			
			<ContentTemplate>

				<div class="OToolbar JoinTable" id="ScheduleReadGroupSearchToolBar" runat ="server">
					<ul>
                    
                        <li>
                                        
                            <asp:LinkButton ID="btn_refresh" runat="server"   
						            CssClass="Icon Refresh" onclick="btn_refresh_Click" >Refresh</asp:LinkButton>  
                               
                        </li>						                                                   
                        
					</ul>
				</div>

                <asp:GridView ID="grdScheduleReadGroupsView" CssClass="OTable"  runat="server" AutoGenerateColumns="False" OnRowDataBound="grdScheduleReadGroupsView_RowDataBound">                    
                    <Columns>                            
                                                        
                            <asp:BoundField DataField="SerialNo" HeaderText="SerialNo" SortExpression="SerialNo" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                           
                            <asp:BoundField DataField="ScheduleSyncDate" HeaderText="Schedule Synch Date" SortExpression="ScheduleSyncDate" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                            
                            <asp:BoundField DataField="ScheduleSyncStatus" HeaderText="Schedule Synch Status" SortExpression="ScheduleSyncStatus" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                                                        

                    </Columns>
                    <EmptyDataTemplate>
						<div class="NoData">
							There are no devices!
						</div>
					</EmptyDataTemplate>                                
                    <AlternatingRowStyle CssClass="Alt" />
					<PagerStyle CssClass="Footer" />
                        
                </asp:GridView>	
                								

			</ContentTemplate>
		</asp:UpdatePanel>   
					
	</div> 
    
    <div class="Buttons">        
        <div class="ButtonHolder">            
            <asp:Button ID="btnBackBottom" CssClass="OButton" runat="server" Text="Back"
                onclick="btnBack_Click" />              
        </div>
        <div class="Clear"> </div>
    </div>    
    
    <br />        

    <div style="width:300px">
        <table class="OTable" >                                    
            <tr>
                <th  class="mt-itm-bold leftalign" >
                    Legend
                </th>                                                                 
            </tr>
              
            <tr>
                <td>
                    <asp:Image ID="Image2" runat="server" ImageUrl="../images/Success.png" Visible="true" />
                    &nbsp;&nbsp; Synched.
                </td>
            </tr>
                <tr>
                <td>
                    <asp:Image ID="Image3" runat="server" ImageUrl="../images/Fail.png" Visible="true" />
                    &nbsp;&nbsp; Not Synched.
                </td>
            </tr>            

        </table> 
    </div>  

</asp:Content>