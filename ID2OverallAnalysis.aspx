<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="ID2OverallAnalysis.aspx.cs" Inherits="portal_instadose_com_v3.TechOps.ID2OverallAnalysis" %>

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
        .outlier
        {       
            background-image:url('../images/icons/error.png');
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
                <th runat="server" id="Header" >Overall Pass/Fail Analysis</th>
            </tr>                            
        </table> 
    </div>  

    <div>
        <asp:UpdatePanel ID="UpdatePanelStatistic" runat="server"  UpdateMode="Conditional">
			<%--<Triggers>
				<asp:AsyncPostBackTrigger controlid="btnOK" eventname="Click" />
			</Triggers>--%>

			<ContentTemplate>
                <asp:GridView ID="grdStatisticResultView" CssClass="OTable"  runat="server" Width = "360px">                                

                    <EmptyDataTemplate>
				        <div class="NoData">
					        There is no available statistic data!
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

		<asp:UpdatePanel ID="UpdatePanelAnalysis" runat="server"  UpdateMode="Conditional">			
			<ContentTemplate>				

                <asp:GridView ID="grdOverallAnalysisView" CssClass="OTable"  runat="server" OnRowDataBound="grdOverallAnalysisView_RowDataBound">                    

                    <EmptyDataTemplate>
						<div class="NoData">
							There is no data!
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
                    &nbsp;&nbsp; Pass
                </td>
            </tr>
                <tr>
                <td>
                    <asp:Image ID="Image3" runat="server" ImageUrl="../images/Fail.png" Visible="true" />
                    &nbsp;&nbsp; Fail
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Image ID="Image4" runat="server" ImageUrl="../images/icons/error.png" Visible="true" />
                    &nbsp;&nbsp; Outlier(Not read, incorrect read date)
                </td>
            </tr>

        </table> 
    </div> 
     
</asp:Content>
