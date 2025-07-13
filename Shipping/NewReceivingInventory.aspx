<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="NewReceivingInventory.aspx.cs" Inherits="Shipping_NewReceivingInventory" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">		
		.gridView
		{
			max-height: 350px; 
			overflow:auto;   
			margin-top: 10px;   
            width:710px;
		}

		/* Fixed gridview header*/
		.FixedHeader {
			position:absolute;
			margin:-10px 0px 0px 0px;
			/*z-index:99;*/           
		}	    
        
        td:has(> span.bin-extraction) {
            background-color: aqua;
        }

        .bin-extraction {
            color: black;
        }

        td:has(> span.bin-retired) {
            background-color: red;
        }

        .bin-retired {
            color: white;
        }


	</style>

    <script type="text/javascript">

        $(document).ready(function () {            
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
        });

        function JQueryControlsLoad() {
            
            $('#specialInstructionDialog').dialog({
				autoOpen: false,
                width: 400,
                resizable: false,
                modal: true,
                title: "Special Instruction",
                open: function (type, data) {
                    $(this).parent().appendTo("form");                    
                    $('.ui-dialog :input').focus();
                    $('.ui-dialog-titlebar-close').hide();
                },
                buttons: {
                    "OK": function () {
                       $('#<%= btnSpecialInstructionOK.ClientID %>').click();
                    },                   
                },
                close: function () {                    
                    $('.ui-overlay').fadeOut();
                }
            });	          
        }

        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        }

        function closeDialog(id) {
            $('#' + id).dialog("close");
        }       

    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>   
    
    <div id="specialInstructionDialog" >
        <asp:UpdatePanel ID="upnlSpecialInstructionDialog" runat="server">
            <ContentTemplate>                            
                <div class="OForm" > 
                    <div class="Row">
                        <%--<div class="Label Medium">label</div>--%>
                        <div class="Control">    
                            <asp:HiddenField ID="hdnSnowSysID" runat="server"/>
                            <asp:HiddenField ID="hdnTotalDays" runat="server"/>
                            <asp:HiddenField ID="hdnSnowTaskStatus" runat="server"/>
                            <span id="lblSpecialInstruction" runat="server" style="color:blue; font-size:x-large;"></span>
                            <span id="lblExtractionMessage" runat="server" style="color:black; font-size:medium; display: block;"></span>
                        </div>
                        <div class="Clear"></div>
                    </div>                                                
                </div>                   
                <asp:button text="OK" style="display:none;" id="btnSpecialInstructionOK" OnClick="btnSpecialInstructionOK_Click" runat="server" />                
            </ContentTemplate>

        </asp:UpdatePanel>			        
    </div>   
        
    <div>
        <asp:Panel ID="Panel1" runat="server" defaultbutton="btnSubmit" >
            <asp:UpdatePanel ID="UpdatePanelReturnPackage" runat="server"  UpdateMode="Conditional">
			    <Triggers>
				    <asp:AsyncPostBackTrigger controlid="btnSpecialInstructionOK" eventname="Click" />
			    </Triggers>
			    <ContentTemplate>	
                    <asp:HiddenField ID="packageID" runat="server" />   
                    <asp:HiddenField ID="hidCurrentActID" runat="server" /> 
                    <asp:HiddenField ID="hidPreviousPackageClick" runat="server" /> 

                    <div class="FormError" id="error" runat="server" visible="false">
		                <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
	                </div>

                    <table style="width:560px; border-spacing:20px;">
                        <tr >
                            <td style="width:150px; ">
                                <asp:Button ID="btnNewPackage"  runat="server" CssClass="OButton" Text= "New Package" Font-Size="Medium"
                                    Height="55" ToolTip="Click to start new package" OnClick="btnNewPackage_Click"  />
                            </td>                
                            <td style="width:150px; ">                
                                <asp:Button ID="btnPrevPackage"   runat="server" CssClass="OButton" Text="Prev Package" Font-Size="Medium"
                                    Height="55" ToolTip="Click to end the package" OnClick="btnPrevPackage_Click" />   
                            </td>
                            <td style="width:150px; "> 
                                <asp:Button ID="btnEndPackage"   runat="server" CssClass="OButton" Text="End Package" Font-Size="Medium"
                                    Height="55" ToolTip="Click to end the package" OnClick="btnEndPackage_Click" />        
                            </td>
                        </tr>
                        <tr>
                            <td style="width:150px; text-align:right";>Serial No<span class="Required">*</span>:</td>
                            <td style="width:150px" >
                                <asp:TextBox runat="server" ID="txtSerialNo" Width="150px"
                                    MaxLength="10" Font-Size="X-Large"/>
                            </td>
                            <td style="width:150px"> 
                                <asp:button text="Accept Badge" id="btnSubmit" CssClass="OButton" OnClick="btnSubmit_Click" runat="server"  Height="45" Font-Size="Medium" />
                            </td>
                        </tr>
                        <tr>
                            <td runat="server" id="tdCommitMsg1"></td>
                            <td colspan="2" runat="server" id="tdCommitMsg2">
                                <span id="lblCommitMsg" runat="server" style="color:blue; font-size:large;"></span>
                            </td>
                        </tr>
                    </table>
                
                    <%--insert here--%>

                    <div>Devices scanned today:</div>
                    <div id="scrollTopGridView" runat="server"  class="gridView">
                        <asp:GridView ID="grdViewReturnPackage" CssClass="OTable" Style="margin:0px 0" runat="server" AllowSorting="true" BorderWidth="0"                         
                            AutoGenerateColumns="False" DataKeyNames="ReturnPackageID"                     
                            DataSourceID="GetReturnPackage" Width="685px"
                            HeaderStyle-CssClass="FixedHeader" OnRowDataBound="grdViewReturnPackage_RowDataBound">
                            <Columns>                                                        
                                    <asp:BoundField DataField="PackageID" HeaderText="PackageID" SortExpression="PackageID"  HeaderStyle-Width ="70px" ItemStyle-Width="70px" />                           
                                    <asp:BoundField DataField="SerialNo" HeaderText="SerialNo" SortExpression="SerialNo"  HeaderStyle-Width ="80px" ItemStyle-Width="80px" />                                                       
                                    
                                        <asp:TemplateField HeaderText="Bucket"  HeaderStyle-Width="120px">
                                            <ItemTemplate>
                                                <asp:Label runat="server" class="<%# getBinCSS(((System.Data.DataRowView) Container.DataItem).Row) %>" >
                                                    <%# Eval("Category") %>
                                                </asp:Label>                                                
                                            </ItemTemplate>
                                            <ItemStyle HorizontalAlign="Center" />
                                        </asp:TemplateField>
                                
                                    <asp:BoundField DataField="Bin" HeaderText="Bin" SortExpression="Bin"  HeaderStyle-Width ="80px" ItemStyle-Width="80px" />                           
                                    <asp:BoundField DataField="CreatedDate" HeaderText="Scanned Date" SortExpression="CreatedDate"  HeaderStyle-Width ="160px" ItemStyle-Width="160px" />                            
                                    <asp:BoundField DataField="CreatedBy" HeaderText="Scanned By" SortExpression="CreatedBy"  HeaderStyle-Width ="120px" ItemStyle-Width="120px"/>      
                                    


                            </Columns>

                            <EmptyDataTemplate>
						        <div class="NoData">
							        No records scanned today!
						        </div>
					        </EmptyDataTemplate>                                
                            <AlternatingRowStyle CssClass="Alt" />
					        <PagerStyle CssClass="Footer" />
                        
                        </asp:GridView>	
                    </div>                    
                								
			    </ContentTemplate>
		    </asp:UpdatePanel> 
        </asp:Panel>
		  					
	</div>    	

    <asp:SqlDataSource ID="GetReturnPackage" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.AppConnectionString %>"
        SelectCommand="SELECT TOP 20 a.ReturnPackageID,a.PackageID, a.SerialNo,c.Bin ,a.CreatedDate,a.CreatedBy, s.SpecialInstruction, (CASE WHEN c.Bin = 'F' THEN 'Retired' WHEN s.SpecialInstruction <> '' THEN 'Extraction' ELSE 'Refurbish' END) as Category
                       FROM rma_ReturnPackage  a
                        Join rma_ref_ReturnReason b on a.ReturnReasonID = b.ReasonID
                        Left Join rma_ref_Bin c on b.BinID = c.BinID
                        Outer Apply (select top 1 SerialNo, SpecialInstruction from DeviceSpecialInstruction where SpecialInstruction like 'CST%' and SerialNo = a.SerialNo order by CreatedDate Desc) s
                        Where Cast(a.CreatedDate as date) = Cast(getdate() as date)
                        Order By a.CreatedDate desc, a.SerialNo;">
    </asp:SqlDataSource>
</asp:Content>

<script runat="server">
    public static string getBinCSS(System.Data.DataRow row)
    {
        string category = row["Category"].ToString().ToLower();
        return "bin-" + category;
    }
</script>
