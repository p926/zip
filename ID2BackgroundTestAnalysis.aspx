<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="ID2BackgroundTestAnalysis.aspx.cs" Inherits="portal_instadose_com_v3.TechOps.ID2BackgroundTestAnalysis" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        .pass
        {                        
            background-image:url('../images/Success.png');
            background-repeat:no-repeat; 
            background-position:left center;                                   
        }
        .fail
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
        .mismatchdate
        {                        
            background-image:url('../images/icons/date_next.png');
            background-repeat:no-repeat;   
            background-position:left center;                                    
        }
        
        .yes
        {             
            color:Red ;              
        }
        
        .no
        {           
            color:Black ;
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

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager> 

    <asp:UpdateProgress id="LoadingUpdateProgress" runat="server" DynamicLayout="true" DisplayAfter="0" >
        <ProgressTemplate>
            
        </ProgressTemplate>
    </asp:UpdateProgress>

    <act:ModalPopupExtender ID="modalExtender" runat="server" TargetControlID="LoadingUpdateProgress"
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
                <th colspan = "9" runat="server" id="Header" >Background Test Pass/Fail Criteria</th>
            </tr>  
            <tr>
                <td style="width: 16%" >
                    <asp:Label ID="Label1" runat = "server" Text="BKGD 1st Pass Low Range:" />
                </td>
                <td class="mt-itm-bold LeftAlign" style="width: 5%" >
                    <asp:TextBox ID="txtBkgd_1stP_LR" runat = "server" Width ="30px" Enabled = "false" />
                </td>
                <td style="width: 16%">
                    <asp:Label ID="Label2" runat = "server" Text="BKGD 1st Pass High Range:"  />
                </td>
                <td class="mt-itm-bold leftalign " style="width: 5%">
                    <asp:TextBox ID="txtBkgd_1stP_HR" runat = "server"  Width ="30px" Enabled = "false" />
                </td>
                <td style="width: 16%" >
                    <asp:Label ID="Label3" runat = "server" Text="BKGD 2nd Pass Low Range:" />
                </td>
                <td class="mt-itm-bold LeftAlign" style="width: 5%" >
                    <asp:TextBox ID="txtBkgd_2ndP_LR" runat = "server"  Width ="30px" Enabled = "false" />
                </td>
                <td style="width: 16%">
                    <asp:Label ID="Label4" runat = "server" Text="BKGD 2nd Pass High Range:"  />
                </td>
                <td class="mt-itm-bold leftalign " >
                    <asp:TextBox ID="txtBkgd_2ndP_HR" runat = "server" Width ="30px" Enabled = "false" />
                </td>
                
            </tr>                

        </table> 
    </div>  

    <div>
        <asp:UpdatePanel ID="UpdatePanelStatistic" runat="server"  UpdateMode="Conditional">
			<%--<Triggers>
				<asp:AsyncPostBackTrigger controlid="btnOK" eventname="Click" />
			</Triggers>--%>

			<ContentTemplate>
                <asp:GridView ID="grdStatisticResultView" CssClass="OTable"  runat="server" Width = "500px">                                

                    <EmptyDataTemplate>
				        <div class="NoData">
					        There are no available statistic data!
				        </div>
			        </EmptyDataTemplate>                                
                    <AlternatingRowStyle CssClass="Alt" />
                    <RowStyle HorizontalAlign="Center" /> 
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

		<asp:UpdatePanel ID="UpdatePanelBackgroundTestAnalysis" runat="server"  UpdateMode="Conditional">			
			<ContentTemplate>				

                <asp:GridView ID="grdBackgroundTestAnalysisView" CssClass="OTable"  runat="server" OnRowDataBound="grdBackgroundTestAnalysisView_RowDataBound">                    

                    <EmptyDataTemplate>
						<div class="NoData">
							There are no data or a group has bad read data.
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
                    &nbsp;&nbsp; Pass.
                </td>
            </tr>
                <tr>
                <td>
                    <asp:Image ID="Image3" runat="server" ImageUrl="../images/Fail.png" Visible="true" />
                    &nbsp;&nbsp; Fail.
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Image ID="Image4" runat="server" ImageUrl="../images/icons/clock_red.png" Visible="true" />
                    &nbsp;&nbsp; Not Read.
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Image ID="Image1" runat="server" ImageUrl="../images/icons/date_next.png" Visible="true" />
                    &nbsp;&nbsp; Incorrect Read Date.
                </td>
            </tr>

        </table> 
    </div> 
     
</asp:Content>
