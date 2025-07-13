<%@ Page Title="Device Warehouse Transfer" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="DeviceWarehouseTransfer.aspx.cs" Inherits="TechOps_DeviceWarehouseTransfer" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">		
		.gridView
		{
			max-height: 350px; 
			overflow:auto;   
			margin-top: 10px;   
            width:450px;
		}

		/* Fixed gridview header*/
		.FixedHeader {
			position:absolute;
			margin:-10px 0px 0px 0px;
			/*z-index:99;*/           
		}	      		
	</style>

    <script type="text/javascript" >
       
        $(document).ready(function () {               
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();            
        });

        function JQueryControlsLoad() {

            $('#mainErrorDialog').dialog({
				autoOpen: false,
                width: 450,
                resizable: false,
                modal: true,
                title: "Error",
                open: function (type, data) {
                    $(this).parent().appendTo("form");                    
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "OK": function () {
                       $('#<%= btnMainErrorOK.ClientID %>').click();
                    },                   
                },
                close: function () {                    
                    $('.ui-overlay').fadeOut();
                }
            });	

            $('#passwordDialog').dialog({
				autoOpen: false,
                width: 400,
                resizable: false,
                modal: true,
                title: "Enter Password",
                open: function (type, data) {
                    $(this).parent().appendTo("form");                    
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "OK": function () {
                       $('#<%= btnPasswordOK.ClientID %>').click();
                    }, 
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {                    
                    $('.ui-overlay').fadeOut();
                }
            });	

            $('#scanErrorDialog').dialog({
				autoOpen: false,
                width: 450,
                resizable: false,
                modal: true,
                title: "Scan Error",
                open: function (type, data) {
                    $(this).parent().appendTo("form");                    
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "OK": function () {
                       $('#<%= btnScanErrorOK.ClientID %>').click();
                    },                   
                },
                close: function () {                    
                    $('.ui-overlay').fadeOut();
                }
            });	  

            $('#warehouseMovementSettingDialog').dialog({
				autoOpen: false,
                width: 450,
                resizable: false,
                modal: true,
                title: "Warehouse/Status Setting",
                open: function (type, data) {
                    $(this).parent().appendTo("form");                    
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Confirm": function () {
                       $('#<%= btnWarehouseMovementSetting.ClientID %>').click();
                    }, 
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {                    
                    $('.ui-overlay').fadeOut();
                }
            });	                       

            $('#<%= txtFrom.ClientID %>').datepicker({
                constrainInput: true,
                changeMonth: true,
                changeYear: true,
                dateFormat: 'mm/dd/yy',
                gotoCurrent: true,
                hideIfNoPrevNext: true,
                minDate: '-5y',
                maxDate: '+5y'
            });
            $('#<%= txtTo.ClientID %>').datepicker({
                constrainInput: true,
                changeMonth: true,
                changeYear: true,
                dateFormat: 'mm/dd/yy',
                gotoCurrent: true,
                hideIfNoPrevNext: true,
                minDate: '-5y',
                maxDate: '+5y'
            });
            $('#ui-datepicker-div').css("z-index",
                        $(this).parents(".ui-dialog").css("z-index") + 1);
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
    
    <div id="mainErrorDialog" >
        <asp:UpdatePanel ID="upnlMainErrorDialog" runat="server">
            <ContentTemplate>                                            
                <div class="FormError" id="error" runat="server" visible="false">
		            <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
	            </div>
                
                <asp:button text="OK" style="display:none;" id="btnMainErrorOK" OnClick="btnMainErrorOK_Click" runat="server" />                
            </ContentTemplate>

        </asp:UpdatePanel>		        
    </div>

    <div id="passwordDialog" >
        <asp:Panel ID="PanelPassword" runat="server" defaultbutton="btnPasswordOK">
            <asp:UpdatePanel ID="upnlPasswordDialog" runat="server">
                <ContentTemplate>                            
                    <div > 
                        <div class="FormError" id="passwordError" runat="server" visible="false">
		                    <p><span class="MessageIcon"></span>
	                        <strong>Messages:</strong> <span id="passwordErrorMsg" runat="server" >An error was encountered.</span></p>
	                    </div>

                        <table class="OTable" style="border:0;">
                            <tr>
                                <td style="text-align:center;"><span style="font-size:x-large;">Enter Password</span></td>                            
                            </tr>
                            <tr>
                                <td style="text-align:center;">
                                    <asp:TextBox runat="server" ID="txtPassword" Width="300px" Height="30px" Font-Size="Large"  MaxLength="30" TextMode="Password" />
                                </td>                            
                            </tr>
                        </table>                                         
                    </div>                   
                    <asp:button text="OK" style="display:none;" id="btnPasswordOK" OnClick="btnPasswordOK_Click" runat="server" />                
                </ContentTemplate>
            </asp:UpdatePanel>	
        </asp:Panel>            		        
    </div>   

    <div id="scanErrorDialog" >
        <asp:UpdatePanel ID="upnlScanErrorDialog" runat="server">
            <ContentTemplate>                            
                <div > 
                    <table class="OTable" style="border:0;">
                        <tr>
                            <td style="text-align:center;"><span style="color:red; font-size:x-large;">SCAN ERROR</span></td>
                        </tr>
                    </table> 
                    
                    <table class="OTable">
                        <tr>
                            <td style="width:100px; text-align:right; ">SERIAL NUMBER: </td>
                            <td ><span id="lblSerialNo" runat="server" style="font-size:medium;"></span></td>
                        </tr>
                        <tr>
                            <td style="width:100px; text-align:right; ">REASON: </td>
                            <td ><span id="lblDeviceWarehouseStatus" runat="server" style="font-size:medium;"></span></td>
                        </tr>
                        <tr>
                            <td style="width:100px; text-align:right; ">ACTION: </td>
                            <td ><span id="lblNote" runat="server" style="font-size:medium;"></span></td>
                        </tr>
                    </table> 
                </div>                   
                <asp:button text="OK" style="display:none;" id="btnScanErrorOK" OnClick="btnScanErrorOK_Click" runat="server" />                
            </ContentTemplate>

        </asp:UpdatePanel>			        
    </div>   

    <div id="warehouseMovementSettingDialog" >
        <asp:UpdatePanel ID="upnlWarehouseMovementSettingDialog" runat="server">
            <ContentTemplate>                            
                <div > 
                    <table class="OTable" style="border:0;">
                        <tr>
                            <td style="text-align:center;"><span style="color:blue; font-size:x-large  ; ">Warehouse & Status Settings</span></td>
                        </tr>
                    </table> 
                    
                    <div class="FormError" id="settingError" runat="server" visible="false">
		                <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="settingErrorMsg" runat="server" >An error was encountered.</span></p>
	                </div>

                    <table cellspacing="5" cellpadding="5" style="width:420px; ">
                        <tr>
                            <td style="width:200px;  border:0; border-right:3px dashed grey;"><span style="font-size:medium; font-weight:900">FROM</span></td>                            
                            <td><span style="font-size:medium; font-weight:900">TO</span></td>
                        </tr>
                        <tr>
                            <td style="width:200px; border:0; border-right:3px dashed grey;">
                                <asp:DropDownList ID="ddlFromWarehouse" runat="server"  Enabled = "true"
                                                DataValueField="Warehouse" DataTextField="Warehouse" AutoPostBack = "true" 
                                                onselectedindexchanged="ddlFromWarehouse_SelectedIndexChanged" />	 
                            </td>                            
                            <td>
                                <asp:DropDownList ID="ddlToWarehouse" runat="server"  Enabled = "true"
                                                DataValueField="Warehouse" DataTextField="Warehouse" AutoPostBack = "true" 
                                                onselectedindexchanged="ddlToWarehouse_SelectedIndexChanged" />	 
                            </td>
                        </tr>
                        <tr>
                            <td style="width:200px; border:0; border-right:3px dashed grey;">
                                <asp:DropDownList ID="ddlFromStaus" runat="server"  Enabled = "true"
                                                DataValueField="DeviceAnalysisStatusID" DataTextField="DeviceAnalysisName" AutoPostBack = "true"
                                                onselectedindexchanged="ddlFromStaus_SelectedIndexChanged" />	 
                            </td>                            
                            <td>
                                <asp:DropDownList ID="ddlToStatus" runat="server"  Enabled = "true"
                                                DataValueField="DeviceAnalysisStatusID" DataTextField="DeviceAnalysisName" AutoPostBack = "true"
                                                onselectedindexchanged="ddlToStatus_SelectedIndexChanged" />	 
                            </td>
                        </tr>
                    </table> 
                </div>                   
                <asp:button text="Confirm" style="display:none;" id="btnWarehouseMovementSetting" OnClick="btnWarehouseMovementSetting_Click" runat="server" Enabled =" false"/>                
            </ContentTemplate>

        </asp:UpdatePanel>			        
    </div>   
     
    <div class="OToolbar">  
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <ul>
			        <li>Scan From:</li>
                    <li><asp:TextBox ID="txtFrom" runat="server" CssClass="Size Small" ></asp:TextBox></li>
			        <li>To:</li>
                    <li><asp:TextBox ID="txtTo" runat="server" CssClass="Size Small" ></asp:TextBox></li>				            
                    <li><asp:Button ID="btnExport" runat="server" CssClass="OButton" OnClick="btnExport_Click" Text="Export to Excel" /></li>
		        </ul> 	 
            </ContentTemplate>   
        </asp:UpdatePanel>         
	</div>

    <div>
        <asp:UpdatePanel ID="UpdatePanelWarehouseMovementSetting" runat="server">
            <ContentTemplate>                                 
                <table style="width:660px; ">
                    <tr >
                        <td style="width:450px; "> 
                            <table class="OTable" >
                                <tr>                                    
                                    <td style="width:110px; ">From WareHouse:</td>
                                    <td style="width:40px; "><span id="lblFromWarehouse" runat="server" style="color:blue; font-size:medium;"></span></td>
                                    <td style="width:50px; ">Status:</td>
                                    <td><span id="lblFromStatus" runat="server" style="color:blue; font-size:medium;"></span></td>
                                </tr>
                            </table>
                        </td>  
                        <td style="width:20px; "></td>
                        <td > 
                            <asp:Button ID="btnAdminSetting"   runat="server" CssClass="OButton" Text="Admin Settings" Font-Size="Medium"
                                Height="40" ToolTip="Setting device status movement" OnClick="btnAdminSetting_Click" />        
                        </td>
                    </tr>
                    <tr>                            
                        <td style="width:450px; "> 
                            <table class="OTable">
                                <tr>                                    
                                    <td style="width:110px; ">To WareHouse:</td>
                                    <td style="width:40px; "><span id="lblToWarehouse" runat="server" style="color:blue; font-size:medium;"></span></td>
                                    <td style="width:50px; ">Status:</td>
                                    <td><span id="lblToStatus" runat="server" style="color:blue; font-size:medium;"></span></td>
                                </tr>
                            </table>
                        </td>
                        <td style="width:20px; "></td>
                        <td></td>
                    </tr>
                        
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
        
    <div>
        <asp:Panel ID="PanelMain" runat="server" defaultbutton="btnAccept" >
            <asp:UpdatePanel ID="UpdatePanelDeviceMovementHistory" runat="server" >			    
			    <ContentTemplate>	
                    
                    <%--<div class="FormError" id="error" runat="server" visible="false">
		                <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
	                </div>--%>                    
                    
                    <table class="OTable" style="width:450px; border:0; ">
                        <tr>                            
                            <td  style="width:300px;">
                                <asp:TextBox runat="server" ID="txtSerialNo" style="text-align:center;" Width="300px" Height="50px"
                                    MaxLength="9" Font-Size="XX-Large" />
                            </td>
                            <td>
                                <asp:Button ID="btnAccept"   runat="server" CssClass="OButton" Text="Accept" Font-Size="Medium"
                                    Height="40" ToolTip="Update device status" OnClick="btnAccept_Click" />    
                            </td>
                        </tr>
                    </table>
                                    
                    <%--insert here--%>
                    <br />
                    <h4>Scan History List</h4>
                    
                    <div id="scrollTopGridView" runat="server"  class="gridView">
                        <asp:GridView ID="grdViewDeviceMovementHistory" CssClass="OTable" Style="margin:0px 0" runat="server" AllowSorting="true" BorderWidth="0"                         
                            AutoGenerateColumns="False" DataKeyNames="ID"                     
                            DataSourceID="GetDeviceMovementHistory" Width="450px"
                            HeaderStyle-CssClass="FixedHeader" OnRowDataBound="grdViewDeviceMovementHistory_RowDataBound">
                            <Columns>                                                        
                                    <asp:BoundField DataField="ID" HeaderText="No." SortExpression="ID"  HeaderStyle-Width ="70px" ItemStyle-Width="70px" />                           
                                    <asp:BoundField DataField="SerialNo" HeaderText="SerialNo" SortExpression="SerialNo"  HeaderStyle-Width ="80px" ItemStyle-Width="80px" />                                                                                                                
                                    <asp:BoundField DataField="CreatedDate" HeaderText="Date/Time" SortExpression="CreatedDate"  HeaderStyle-Width ="250px" ItemStyle-Width="250px" />                                                                                                                                                                          
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
    
    

    <asp:SqlDataSource ID="GetDeviceMovementHistory" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand=" SELECT a.ID, b.SerialNo, a.CreatedDate 
                        FROM DeviceMovementHistory a
                        Join DeviceInventory b on a.DeviceID = b.DeviceID
                        Where Cast(a.CreatedDate as date) = Cast(getdate() as date)                       
                        Order By a.CreatedDate desc;">
    </asp:SqlDataSource>
</asp:Content>
