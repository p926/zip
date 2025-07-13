<%@ Page Title="Device Recall" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_ReturnAddNewDeviceRMA" Codebehind="ReturnAddNewDeviceRMA.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">    
    <style type="text/css">
        .gridView
		{
			max-height: 200px; 
			/*overflow:auto;*/   
			margin-top: 10px;  
            overflow-x:scroll;
            overflow-y:auto;
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

            $("#<%=btnContinue.ClientID %>").click(function () {
                $("#page-loader").show();
            })
            <%--var ischecked = $('#<%= rdobtnBySN.ClientID %>').is(":checked");
            if (ischecked == false) {
                $(scrollTopGridView).scrollTop(1);
            } --%>                           
        });
			
		function JQueryControlsLoad() {
			
			$('#assignedLocationDialog').dialog({
				autoOpen: false,
                width: 1100,
                resizable: false,
                modal: true,
                title: "Shipping Location",
                open: function (type, data) {
                    $(this).parent().appendTo("form");                    
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Ok": function () {
                       $('#<%= btnAssignedLocation.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {                    
                    $('.ui-overlay').fadeOut();
                }
            });		

            $('#addressCorrectionDialog').dialog({
				autoOpen: false,
                width: 550,
                resizable: false,
                modal: true,
                title: "Select Address",
                open: function (type, data) {
                    $(this).parent().appendTo("form");                    
                    $('.ui-dialog :input').focus();                    
                },
                buttons: {
                    "Select": function () {
                       $('#<%= btnPreferAddressSelect.ClientID %>').click();                        
                    },
                    "Close": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {                    
                    $('.ui-overlay').fadeOut();
                }
            });	

            //// selected header check box.
            //$('.chkbxHeaderDeviceList').click(function () {
            //    var ischecked = ($('#ctl00_primaryHolder_gv_DeviceList_ctl01_chkbxHeaderDeviceList').is(":checked"));
            //    // add attribute 
            //    $('#ctl00_primaryHolder_gv_DeviceList input:checkbox[id$=chkbxSelectDeviceList]').attr("checked", ischecked);                
            //});   
		}
				
		function openDialog(id) {
			$('.ui-overlay').fadeIn();
			$('#' + id).dialog("open");
		}

		function closeDialog(id) {
			$('#' + id).dialog("close");
        }	        

        function RadioSelectUserCheck(rb) {
			var gv = document.getElementById("<%=gv_LocationList.ClientID%>");
			var rbs = gv.getElementsByTagName("input");

			var row = rb.parentNode.parentNode;
			for (var i = 0; i < rbs.length; i++) {
				if (rbs[i].type == "radio") {
					if (rbs[i].checked && rbs[i] != rb) {
						rbs[i].checked = false;
						break;
					}
				}
			}
        }


    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>

    <div id="page-loader" class="page-overlay">
        <div class="overlay-spinner"></div>
    </div>

    <asp:UpdateProgress id="UpdateProgress1" runat="server" DynamicLayout="true" DisplayAfter="0" >
        <ProgressTemplate>
            <div class="page-overlay" style="display: block">
                <div class="overlay-spinner"></div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <%--ASSIGN LOCATION MODAL/DIALOG--%>
	<div id="assignedLocationDialog" style="display: none;">
        <asp:UpdatePanel ID="upnlAssignedWearer" runat="server">
			<ContentTemplate>
                <div style="overflow-x:no-display; overflow-y: scroll; height: 350px;">
                     <asp:GridView ID="gv_LocationList" runat="server" AutoGenerateColumns="False" CssClass="OTable Scroll" DataKeyNames="LocationID" OnRowDataBound="gv_LocationList_RowDataBound">                                             
				        <Columns>                            

					        <asp:TemplateField HeaderStyle-Width ="25px" ItemStyle-HorizontalAlign="Center">                                
						        <ItemTemplate>
							        <asp:RadioButton ID="radSelectLocation" runat="server" onclick = "RadioSelectUserCheck(this);" />							    
                                    <asp:HiddenField runat="server" ID="hidLocationID"  Value='<%# Eval("LocationID") %>'  />
                                    <asp:HiddenField runat="server" ID="hidStateID"  Value='<%# Eval("StateID") %>'  />
                                    <asp:HiddenField runat="server" ID="hidCountryID"  Value="11" />
                                    <asp:Label ID="lblLocationID" runat="server" Text='<%# Eval("LocationID") %>' Visible="False" />
                                    <asp:Label ID="lblStateID" runat="server" Text='<%# Eval("StateID") %>' Visible="False" />
                                    <asp:Label ID="lblCountryID" runat="server" Text='<%# Eval("CountryID") %>' Visible="False" />
						        </ItemTemplate>                                                     
					        </asp:TemplateField>                        
					                           
					        <asp:BoundField DataField="LocationName" HeaderText="Location/Ptr" SortExpression="LocationName"  />
					        <asp:BoundField DataField="Contact" HeaderText="Contact" SortExpression="Contact"  />  
                            <asp:BoundField DataField="Company" HeaderText="Company" SortExpression="Company" />
					        <asp:BoundField DataField="Address1" HeaderText="Address1" SortExpression="Address1"  /> 
                            <asp:BoundField DataField="Address2" HeaderText="Address2" SortExpression="Address2"  />
                            <asp:BoundField DataField="City" HeaderText="City" SortExpression="City" />
					        <asp:BoundField DataField="State" HeaderText="State" SortExpression="State"   />  
                            <asp:BoundField DataField="ZipCode" HeaderText="ZipCode" SortExpression="ZipCode"  />
                            <asp:BoundField DataField="Country" HeaderText="Country" SortExpression="Country"  />                        
						    <%--<asp:BoundField DataField="Telephone" HeaderText="Telephone" SortExpression="Telephone" Visible ="false" />--%>	
                            <asp:TemplateField HeaderStyle-Width ="0px" ItemStyle-HorizontalAlign="Center">                                
						        <ItemTemplate>							        
                                    <asp:Label ID="lblTelephone" runat="server" Text='<%# Eval("Telephone") %>' Visible="False" />                                    
						        </ItemTemplate>                                                     
					        </asp:TemplateField>  
				        </Columns>
				        <EmptyDataTemplate>
					        <div class="NoData">
						        No location found.
					        </div>
				        </EmptyDataTemplate>
				        <AlternatingRowStyle CssClass="Alt" />
				        <PagerStyle CssClass="Footer" />
			        </asp:GridView> 
                </div>
                                    
                <asp:Button Text="Assign" Style="display: none;" ID="btnAssignedLocation"
			        OnClick="btnAssignedLocation_Click" runat="server" />

			</ContentTemplate>
        </asp:UpdatePanel>
        
		<%--<div style="overflow-x:no-display; overflow-y: scroll; height: 350px;">
			
		</div>--%>
	</div>
	<%--ASSIGN WEARER MODAL/DIALOG--%>

    <div id="addressCorrectionDialog" >
        <asp:UpdatePanel ID="upnlAddressCorrection" runat="server">
            <ContentTemplate>
                <div >
                    <span>Use entered address:</span><br></br>                  
                    <asp:RadioButton ID="rdoEnteredAddress" runat="server" AutoPostBack ="true" OnCheckedChanged="RdoEnteredAddress_CheckedChanged1" />                                    
                </div>
                <br>
                </br> 
                <div>
                    <span>Use verified address (preferred):</span>
                    <div style="height: 230px; overflow-y: auto;">
                        <asp:RadioButtonList ID="rdoVerifiedAddressList" runat="server" AutoPostBack ="true" OnSelectedIndexChanged="RdoVerifiedAddressList_SelectedIndexChanged1">
                        </asp:RadioButtonList>
                    </div>
                </div>
                <%--<div style="overflow-x:no-display; overflow-y: scroll; height: 350px;">                    
                </div>--%>
                <asp:Button ID="btnPreferAddressSelect" runat="server" OnClick="btnPreferAddressSelect_Click" Style="display: none;" Text="Select" />                              
			</ContentTemplate>
        </asp:UpdatePanel>			        
    </div>

    <div >   
        <asp:UpdatePanel ID="UpdatePanel2" runat="server">
            <ContentTemplate>
                <div class="FormError" id="errors" runat="server" visible="false">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
              </div> 
              <div class="FormMessage" id="success" runat="server" visible="false"> 
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong> <span id="successMsg" runat ="server" >Commit successfully!.</span></p>
            </div> 
            </ContentTemplate>
        </asp:UpdatePanel>  
        
        <asp:Panel ID="Panel1" DefaultButton="btnGo" runat="server">
            <div class="OToolbar JoinTable " id="SerialSearch" runat ="server">
			    <ul>												                                            
				    <li><asp:RadioButton ID="rdobtnBySN" runat="server" Text="Lookup By S/N"  GroupName="LookupGroup" OnCheckedChanged="rdobtnBySN_CheckedChanged" AutoPostBack="true"   /></li>
                    <li><asp:RadioButton ID="rdobtnByInstadoseAccount" runat="server" Text="By Instadose Account#" GroupName="LookupGroup" OnCheckedChanged="rdobtnByInstadoseAccount_CheckedChanged" AutoPostBack="true"  /></li>
                    <li><asp:RadioButton ID="rdobtnByGDSAccount" runat="server" Text="By GDS Account#" GroupName="LookupGroup" OnCheckedChanged="rdobtnByGDSAccount_CheckedChanged" AutoPostBack="true"  /></li>
                    <li><asp:TextBox ID="txtSerialNoInput" runat="server"></asp:TextBox></li>
                    <li><asp:TextBox ID="txtAccountInput" runat="server" Visible ="false"></asp:TextBox></li>
                    <li><asp:Button ID="btnGo" OnClick="btnGo_Click" Text="Go" runat="server" CssClass = "OButton" Width="80px" Height="25px"/></li>				    
			    </ul>
		    </div>     
        </asp:Panel> 
                  
    </div>    
       

    <asp:UpdatePanel ID="upnlAccountInfo" runat="server" Visible ="false" >           
        <ContentTemplate>                        

            <table class="OTable">
                <tr>
                    <td style="width:160px"class="mt-itm-bold rightalign">Account #:</td>
                
                    <td valign="top">                
                        <table >                        
                            <tr>
                                <td valign="center" >
                                    <asp:Label ID="lblAccountNo" runat="server" ></asp:Label><asp:Label ID="lblGDSAccount" runat="server" ></asp:Label> (<asp:Label ID="lblAccountName" runat="server" ></asp:Label>)
                                </td>
                                    
                                <td valign="center" >
                                    <asp:Image ID="imgBrandLogo" style="width: 75px; height: 25px;" ImageUrl="~/images/logos/mirion.png" runat="server" />
                                </td>                                                                
                            </tr>
                        </table>   
                    </td>

                </tr>

                <tr>
                    <td class="mt-itm-bold rightalign">Product:</td>
                    <td class="mt-itm">
                        <asp:DropDownList ID="ddlProduct" runat="server" OnSelectedIndexChanged="ddlProduct_SelectedIndexChanged" AutoPostBack ="true"></asp:DropDownList>
                    </td>
                </tr>
                <%--<tr id="LocationLabelRow" runat="server" visible="false">
                    <td class="mt-itm-bold rightalign">Location:</td>
                    <td class="mt-itm">
                        <asp:Label ID="lblLocation" runat="server" ></asp:Label>
                    </td>
                </tr>--%>
                <tr>
                    <td class="mt-itm-bold rightalign">Device Location:</td>
                    <td class="mt-itm">
                        <asp:DropDownList ID="ddlLocation" runat="server" OnSelectedIndexChanged="ddlLocation_SelectedIndexChanged" AutoPostBack ="true"></asp:DropDownList>
                    </td>
                </tr>

                <tr id="ProRatePeriod" runat="server" visible="false">
                    <td class="mt-itm-bold rightalign">ProRate Period:</td>
                    <td class="mt-itm">
                        <asp:RadioButtonList ID="radProratePeriod" runat="server" RepeatDirection="Horizontal" RepeatColumns="2">                                
                        </asp:RadioButtonList>
                    </td>
                </tr>
                
                <tr>
                    <td class="mt-itm-bold rightalign">Return Type:</td>
                    <td class="mt-itm">
                    <asp:Label ID="lblReturnType" runat="server" ></asp:Label>
                    </td>
                </tr>

                <tr>
                    <td class="mt-itm-bold rightalign">Return Reason<span class="Required">*</span>:</td>
                    <td class="mt-itm">
                        <asp:DropDownList ID="ddlReturnReason" runat="server"  OnSelectedIndexChanged="ddlReturnReason_SelectedIndexChanged" AutoPostBack ="true" ></asp:DropDownList>
                    </td>
                </tr>

                <tr>
                    <td class="mt-itm-bold rightalign" valign="top">Notes:</td>
                    <td class="mt-itm">
                    <asp:TextBox ID="txtNotes" TextMode="MultiLine" Height="80px" CssClass="Notes" runat="server" Width="700px"></asp:TextBox>
                    </td>
                </tr>

                <tr>
                    <td class="mt-itm-bold rightalign" valign="top">Special Instruction:</td>
                    <td class="mt-itm">
                    <asp:TextBox ID="txtSpecialInstruction" TextMode="MultiLine" CssClass="Notes" 
                            runat="server" Height="50px" MaxLength="1024" Width="700px" ></asp:TextBox>
                    </td>
                </tr>  

                <tr id="SingleFlowRecall" runat="server">
                    <td colspan="2">
                        <table>
                            <tr>
                                <td class="mt-itm-bold rightalign">Serial #:</td>
                                <td class="mt-itm">
                                <asp:Label ID="lblSerialNo" runat="server" ></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="mt-itm-bold rightalign" valign="top">Current Assigned To:</td>
                                <td valign="top">
                                    <asp:Label ID="lblAssignedUser" runat="server" ></asp:Label>
                                </td>
                            </tr>
                            <tr id="CaseCoverRow" runat="server">
                                <td class="mt-itm-bold rightalign" valign="top">Color<span class="Required">*</span>:</td>
                                <td class="mt-itm">
                                    <asp:HiddenField runat="server" ID="HidCaseCover" />
                                    <asp:DropDownList ID="ddlCaseCover" runat="server" OnSelectedIndexChanged ="ddlCaseCover_SelectedIndexChanged" AutoPostBack="true">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr id="AssingnUserRow" runat="server">
                                <td class="mt-itm-bold rightalign" valign="top">Ship W/O Assigned User:</td>
                                <td valign="top">
                                    <asp:CheckBox ID="chkBoxWOAssingnUser" runat="server" ></asp:CheckBox>
                                </td>
                            </tr>
                            <tr>
                                <td class="mt-itm-bold rightalign" valign="top">Services Period:</td>
                                <td valign="top">
                                    <asp:Label ID="lalServiceDate" runat="server" ></asp:Label>
                                </td>
                            </tr>
                                <tr>
                                <td class="mt-itm-bold rightalign" valign="top">Status/History:</td>
                                <td class="mt-itm" valign="top">
                                <asp:Label ID="lalDeviceStatus" runat="server" ></asp:Label>
                                </td>
                            </tr>  
                        </table>
                    </td>
                </tr>
                
                <tr id="MultipleFlowRecall" runat="server" >
                    <td class="mt-itm-bold rightalign" style="vertical-align:top; padding-top:10px;">Serial #:</td>
                    <td >
                        <asp:UpdatePanel ID="upnlDeviceGrid" runat="server">
                            <ContentTemplate>

                                <div class="OToolbar JoinTable" id="DeviceListToolBar" runat ="server">
							        <ul>
								        <li>                                    
									        <asp:Label ID="lblSearch" runat="server" Text="Search:"></asp:Label>                                   
									        <asp:TextBox ID="txtDeviceSearch" runat="server" width="150px" ontextchanged="txtDeviceSearch_TextChanged" AutoPostBack ="true"></asp:TextBox>                                                           
								        </li>							
							        </ul>
						        </div>

                                <div id="scrollTopGridView" class="gridView" >
							        <asp:GridView ID="gv_DeviceList" runat="server" CssClass="OTable"  BorderWidth="1px" Style="margin:-1px 0" DataKeyNames="DeviceID" AutoGenerateColumns="False"                                                                                       
								        OnRowDataBound="gv_DeviceList_RowDataBound" 
								        HeaderStyle-CssClass="FixedHeader" >
										   
								        <Columns>

									        <asp:TemplateField HeaderStyle-Width ="50px" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Center">
										        <HeaderTemplate>  
											        <img src="/images/icons/package.png" style="border: 0px none;width: 16px; height: 16px;" />                                  
											        <asp:CheckBox runat="server" ID="chkbxHeaderDeviceList" ToolTip="Select all recall devices" OnCheckedChanged="chkbxHeaderDeviceList_CheckedChanged" AutoPostBack="true" class="chkbxHeaderDeviceList"  />
										        </HeaderTemplate>
													
										        <ItemTemplate>                                                        
											        <asp:CheckBox id="chkbxSelectDeviceList" runat="server" OnCheckedChanged="chkbxSelectDeviceList_CheckedChanged" AutoPostBack="true" />
											        <asp:HiddenField runat="server" ID="HidDeviceID"  Value='<%# Eval("DeviceID") %>' />
											        <asp:HiddenField runat="server" ID="HidAssignedUserID"  Value='<%# Eval("UserID") %>' />
                                                    <asp:HiddenField runat="server" ID="HidAssignedWearerID"  Value='<%# Eval("GDSWearerID") %>' />
										        </ItemTemplate>                                                     
									        </asp:TemplateField>									
					   									
                                            <asp:TemplateField HeaderText="Color" HeaderStyle-Width ="135px" ItemStyle-Width="135px">
										        <ItemTemplate>                                    
											        <asp:Label ID="lblColorProductID" runat="server" Text='<%# Eval("ColorProductID") %>' Visible="false" />
											        <asp:DropDownList ID="ddlColor" runat="server" DataTextField="Color" DataValueField="ProductID" OnSelectedIndexChanged ="ddlColor_SelectedIndexChanged" AutoPostBack ="true"></asp:DropDownList>
										        </ItemTemplate>                                        
									        </asp:TemplateField> 

									        <asp:BoundField DataField="BodyRegionName" HeaderText="Body Region" SortExpression="BodyRegionName" HeaderStyle-Width ="80px" ItemStyle-Width="80px" />			
									        <asp:BoundField DataField="SerialNo" HeaderText="Serial No" SortExpression="SerialNo" HeaderStyle-Width ="60px" ItemStyle-Width="60px" />                                       
				
									        <asp:TemplateField HeaderText="Ship W/O Assigned" HeaderStyle-Width ="70px" ItemStyle-Width="70px">
										        <ItemTemplate>                                                        
											        <asp:CheckBox runat="server" ID="chkbxShipWOAssigned" ToolTip="Check to ship w/o assign to user" OnCheckedChanged="chkbxShipWOAssigned_CheckedChanged" AutoPostBack="true" />											
										        </ItemTemplate>  
                                                <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Left" />
										        <ItemStyle Wrap="False" HorizontalAlign="Center" />
									        </asp:TemplateField>                                                                                 
				                            <asp:BoundField DataField="FullName" HeaderText="Wearer Name" SortExpression="FullName" HeaderStyle-Width ="420px" ItemStyle-Width="420px" />
								        </Columns>
								        <EmptyDataTemplate>
									        <div class="NoData">
										        No device found.
									        </div>
								        </EmptyDataTemplate>
								        <AlternatingRowStyle CssClass="Alt" />
								        <PagerStyle CssClass="Footer" />
							        </asp:GridView>                  
						        </div>

                            </ContentTemplate>
                        </asp:UpdatePanel>                        
                    </td>
                </tr>

                
                <tr id="WearDateRow" runat="server">
                    <td class="mt-itm-bold rightalign">Wear Date(s)<span class="Required">*</span>:</td>
                    <td class="mt-itm">
                        <asp:DropDownList ID="ddlWearDate" runat="server" ></asp:DropDownList>
                    </td>
                </tr>

                <tr id="ShippingMethodRow" runat="server">
                    <td class="mt-itm-bold rightalign">Shipping Method<span class="Required">*</span>:</td>
                    <td class="mt-itm">
                    <asp:DropDownList ID="ddlShippingMethod" runat="server" Enabled="false">
                        <asp:ListItem Text="Express (2 Day)" Value="1" />
                        <asp:ListItem Text="Ground" Value="0" Selected="True" />
                    </asp:DropDownList>
                    </td>
                </tr>

                <tr id="ShippingCarrierRow" runat="server">
                    <td class="mt-itm-bold rightalign">Shipping Carrier<span class="Required">*</span>:</td>
                    <td class="mt-itm">
                    <asp:DropDownList ID="ddlShippingCarrier" runat="server" >
                        <asp:ListItem Text="Default shipping" Value="" />
                        <asp:ListItem Text="FEDEX" Value="1" Selected="True" />
                        <asp:ListItem Text="USPS" Value="2" />
                        <asp:ListItem Text="GBM (Global DHL)" Value="3" />
                        <asp:ListItem Text="DHL" Value="4" />
                        <asp:ListItem Text="UPS" Value="5" />
                    </asp:DropDownList>
                    </td>
                </tr>

                <tr>
                    <td class="mt-itm-bold rightalign" valign="top">Ship Replacement To:</td>           
                    <td valign="top">
                
                        <table >
                            <tr>
                                <td valign="top">
                                    <asp:RadioButton ID="rbtnUserAddress" Text="User"   
                                    runat="server"  GroupName="rbtShippingAddress" AutoPostBack = "true"
                                        oncheckedchanged="rbtnUserAddress_CheckedChanged"  /><br />
                                    <asp:Label ID="lblAddressUser" runat="server"></asp:Label>
                                </td>
                                    
                                <td valign="top">
                                    <asp:RadioButton ID="rbtnLocationAddress" Text="Location" 
                                    runat="server"  GroupName="rbtShippingAddress" AutoPostBack = "true"
                                        oncheckedchanged="rbtnLocationAddress_CheckedChanged"  /><br />
                                    <asp:Label ID="lblAddressLocation" runat="server"></asp:Label>
                                </td>
                                    
                                <td valign="top">
                                    <asp:RadioButton ID="rbtnMainAddress" Text="Main Office" 
                                    runat="server"  GroupName="rbtShippingAddress" AutoPostBack = "true"
                                        oncheckedchanged="rbtnMainAddress_CheckedChanged" /><br />
                                    <asp:Label ID="lblAddressMain" runat="server"></asp:Label>
                                </td>
                            </tr>
                                                
                            <tr>
                                <td valign="top">
                                    <asp:RadioButton ID="rbtnOtherLocation" Text="Other Location" 
                                    runat="server"  GroupName="rbtShippingAddress" AutoPostBack = "true"
                                        oncheckedchanged="rbtnOtherLocation_CheckedChanged"  />
                                </td>
                                    
                                <td valign="top" >     
                                    <asp:Button Text="Existing Locations"  ID="btnExistingLocations"
			                            OnClick="btnExistingLocations_Click" runat="server" Visible = "false" />
                                    <asp:Button Text="New Location"  ID="btnNewLocation"
			                            OnClick="btnNewLocations_Click" runat="server" Visible = "false" />                                                                        
                                </td>
                                    
                                <td valign="top" >
                                    <asp:Label ID="lblOtherLocation" runat="server" Visible = "false"></asp:Label>
                                </td>
                            </tr>
                        </table>   
                    </td>
                </tr>

                <tr id="AddAddress" runat="server">
                    <td class="mt-itm-bold rightalign" valign="top"></td>           
                    <td valign="top">                
                        <table >
                            <tr style ="display:none;">
                                <td class="mt-itm-bold leftalign">LocationID:</td>
                                <td class="mt-itm">
                                    <asp:TextBox ID="txtLocationID" runat="server" Width="50px" ></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td class="mt-itm-bold leftalign">Contact Name<span class="Required">*</span>:</td>
                                <td class="mt-itm">
                                    <asp:TextBox ID="txtAddrContactName" runat="server" Width="350px" ></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td class="mt-itm-bold leftalign">Country<span class="Required">*</span>:</td>
                                <td class="mt-itm">
                                    <asp:DropDownList ID="ddlAddrCountry" runat="server"  
                                    DataValueField="CountryID" DataTextField="CountryName" AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlAddrCountryOnSelectedIndexChange" />
                                </td>
                            </tr>
                            <tr>
                                <td class="mt-itm-bold leftalign">Address<span class="Required">*</span>:</td>
                                <td class="mt-itm">
                                    <asp:TextBox ID="txtAddrAddress1" runat="server" Width="350px"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td class="mt-itm-bold leftalign"></td>
                                <td class="mt-itm">
                                    <asp:TextBox ID="txtAddrAddress2" runat="server" Width="350px"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td class="mt-itm-bold leftalign">City<span class="Required">*</span>:</td>
                                <td class="mt-itm">
                                    <asp:TextBox ID="txtAddrCity" runat="server" Width="350px"></asp:TextBox>
                                </td>
                            </tr>
                            <tr id="StateRow" runat="server">
                                <td class="mt-itm-bold leftalign">State<span class="Required">*</span>:</td>
                                <td class="mt-itm">
                                    <asp:DropDownList ID="ddlAddrState" runat="server" 
                                        DataValueField="StateID" DataTextField="StateAbbName" />
                                </td>
                            </tr>
                            <tr>
                                <td class="mt-itm-bold leftalign">Zip Code<span class="Required">*</span>:</td>
                                <td class="mt-itm">
                                    <asp:TextBox ID="txtAddrZipCode" runat="server" ></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td class="mt-itm-bold leftalign">Phone<span class="Required">*</span>:</td>
                                <td class="mt-itm">
                                    <asp:TextBox ID="txtAddrPhone" runat="server" ></asp:TextBox>
                                </td>
                            </tr>
                            <tr style ="display:none;">
                                <td class="mt-itm-bold leftalign">Disable Addr Verify:</td>
                                <td class="mt-itm">
                                    <asp:CheckBox ID="chkBoxAddrDisableVerification" runat="server" ></asp:CheckBox>
                                </td>
                            </tr>
                            <tr id="AddressVerifyRow" runat="server">
                                <td class="mt-itm-bold leftalign"></td>
                                <td class="mt-itm">
                                    <asp:Button ID="btnAddressVerify" CssClass="OButton" runat="server" Text="Address Verify" onclick="btnAddressVerify_Click" />
                                </td>
                            </tr>
                        </table>   
                    </td>
                </tr>
                
            </table>

            <div  runat="server" id="div_rmaEmailHistory" visible="false">
                <table class="OTable" >
                    <tr>
                        <th class="mt-hd">Email History</th> 
                    </tr>
                    <tr>
                        <td class="mt-itm">
                            <asp:GridView ID="gv_EmailHistory" runat="server" CssClass="OTable" AutoGenerateColumns="false">
                                <Columns>
                                   <asp:TemplateField HeaderText="Email Datetime" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd">
                                      <ItemTemplate>
                                        <asp:Label ID="lblEmailDate" runat="server" 
                                        Text='<%#GetPDTtime(DataBinder.Eval(Container.DataItem,"createdDate","" ))%>' > </asp:Label>
                                      </ItemTemplate>
                                  </asp:TemplateField>
                                  <asp:BoundField DataField="CreatedBy" HeaderText="Send By" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                                  <asp:BoundField DataField="Notes" HeaderText="Notes/Comment" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                                </Columns>
                                <EmptyDataTemplate>
						        <div class="NoData">
							        No email history found!
						        </div>
					            </EmptyDataTemplate>                                
                                <AlternatingRowStyle CssClass="Alt" />
					            <PagerStyle CssClass="Footer" />
                            </asp:GridView>
                        </td> 
                    </tr>
                </table> 
            </div>
            
		</ContentTemplate>
     </asp:UpdatePanel>

    <div id="div_InitiateRequestButtonRow" runat="server" visible="false">
        <div class="Left">
            <strong>User Name: </strong>
            <asp:Label ID="lblCreatedBy" runat="server" ></asp:Label>
        </div>
            
        <div class="Right"> 
            <asp:Button ID="btnContinue" CssClass="OButton" runat="server" Text="Initiate Request" onclick="btnContinue_Click" />
            <asp:Button ID="btnCancel" CssClass="OButton" runat="server" Text="Cancel" onclick="btnCancel_Click" />                 
        </div>  
            
        <div class="Clear"></div> 
    </div>
         
        
</asp:Content>

