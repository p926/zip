<%@ Page Title="Manage Dose" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_ManageDose" Codebehind="ManageDose.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css" media="all">
		/* CSS Definition/Override for HTML-rendered <TH> in GridView. */
		th.RightAlignHeaderText
		{
			text-align: right;
			vertical-align: middle;
		}

		.block-display {display: block !important;}
	</style>
	<link href="/css/jquery.timepicker.min.css" rel="stylesheet" type="text/css" />
	<script type="text/javascript" src="/scripts/jquery.timepicker.min.js"></script>
	<script type="text/javascript" charset="utf-8">
		 $(document).ready(function () {

			 Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
			 JQueryControlsLoad();
			 
		 });

		 function JQueryControlsLoad() {

			 $('#<%=txtExposureDate.ClientID %>').datepicker(
				{
					showOn: "button"
				}
             );

             $('#<%=txtExposureTime.ClientID %>').timepicker(
                 {
                     'timeFormat': 'H:i:s',
                     showOn: null
                 }
             );

			 $('#<%=txtExposureTime.ClientID %>').click(function(){
				 $('#<%=txtExposureTime.ClientID %>').timepicker('show');
				 
			 });
 
			 $('#ReadDetailDialog').dialog({

				 autoOpen: false,
				 width: 460,
				 resizable: false,
				 title: "Add/Edit Reading",
				 open: function (type, data) {
					 $(this).parent().appendTo("form");
					 $('#<%= btnLoadRead.ClientID %>').click();
					 $('#<%= btnAddRead.ClientID %>').attr("enabled","enabled");
					 $('.ui-dialog :input').focus();
				 },
				 buttons: {
					 "Ok": function () {
						 $('#<%= btnAddRead.ClientID %>').click();
						 $('#<%= btnAddRead.ClientID %>').attr("disabled","disabled");
					 },
					 "Cancel": function () {
						 $(this).dialog("close");
					 }
				 },
				 close: function () {
					 $('#<%= btnCancelRead.ClientID %>').click();
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

		function setSerialNoSearch(serialNo) {
            $('#<%= txtSerialNoSearch.ClientID %>').val(serialNo);
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">  
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>

	<div class="OToolbar JoinTable" id="ManageDoseToolBar" runat ="server">

				<asp:Label ID="Label4" runat="server" Text="Read Filter:"></asp:Label>   
				<asp:DropDownList id="ddlReadFilter" runat="server">
					<asp:ListItem Text="User Reads" Value="UserRead" />
					<asp:ListItem Text="Factory Reads" Value="FactoryRead" />
				</asp:DropDownList>

				<asp:Label ID="Label5" runat="server" Text="Display:"></asp:Label>    
				<asp:DropDownList id="ddlDisplay" runat="server">
					<asp:ListItem Text="Doses" Value="Doses" />
					<asp:ListItem Text="Audits" Value="Audits" />
				</asp:DropDownList>

				<asp:Label ID="LblSoftRead" runat="server" Text="Show Soft Read:"></asp:Label>
				<asp:CheckBox id="CbSoftRead" checked="true" runat="server" AutoPostBack="True"/>

				<asp:Label ID="Label1" runat="server" Text="Account#:"></asp:Label>                                  
				<asp:TextBox ID="txtAccountIDSearch" runat="server" Width="70px" AutoPostBack="False"></asp:TextBox>
				
				<asp:Label ID="Label2" runat="server" Text="Serial#:"></asp:Label>                                   
				<asp:TextBox ID="txtSerialNoSearch" runat="server" Width="70px" AutoPostBack="False"></asp:TextBox>
				<asp:Label ID="Label3" runat="server" Text="Read ID:"></asp:Label>                                 
				<asp:TextBox ID="txtRIDSearch" runat="server" Width="70px" AutoPostBack="False"></asp:TextBox>&nbsp;
				<asp:Button ID="btnGo" runat="server" Text="Go" CssClass="OButton" onclick="btnGo_Click" CausesValidation="False" />
	</div>

	<div id="ReadDetailDialog">
		<asp:UpdatePanel ID="upnlRead" runat="server" UpdateMode="Conditional">
			<ContentTemplate>
				<div class="FormError" id="readDialogErrors" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
					<strong>Messages:</strong> <span id="readDialogErrorMsg" runat="server" >An error was encountered.</span></p>
				</div>
				<div class="OForm" >
					<div class="Row">
						<div class="Label Medium2 ">RID:</div>
						<div class="LabelValue" style="width:100px"><asp:Label runat="server" ID="txtRID" CssClass="StaticLabel" /></div>
						<div class="Clear"></div>
					</div>

					<div class="Row">
						<div class="Label Medium2">Account#:</div>
						<div class="LabelValue" style="width:100px"><asp:Label runat="server" ID="txtAccountID" CssClass="StaticLabel" /></div>
						<div class="Clear"></div>
					</div>
					<div class="Row">
						<div class="Label Medium2">Serial#:</div>
						<div class="Control LabelValue">
							<asp:Label runat="server" ID="txtSerialNo" CssClass="StaticLabel" />
							<asp:TextBox runat="server" ID="txtSerialNo2" CssClass="Size Small" Visible="false" />
							<button type="button" id="btnLoadSerialNo" runat="server" onserverclick="btnLoadSerialNo_Click">Find</button>
							<span class="InlineError block-display" id="lblSerialNoError" runat="server" visible="false"></span>
						</div>
						<div class="Clear"></div>
					</div>

					<div class="Row">
						<div class="Label Medium2">User#:</div>
						<div class="LabelValue" style="width:100px"><asp:Label runat="server" ID="txtUserID" CssClass="StaticLabel" /></div>
						<div class="Clear"></div>
					</div>
					<div class="Row">
						<div class="Label Medium2">Name:</div>
						<div class="LabelValue" style="width:250px"><asp:Label runat="server" ID="txtName" CssClass="StaticLabel" /></div>
						<div class="Clear"></div>
					</div>

					<div class="Row"> 
						<div class="Label Medium2">Read Type:</div>
						<div class="Control"> 
							<asp:DropDownList ID="ddlReadType" runat="server" OnSelectedIndexChanged="ddlReadType_SelectedIndexChanged" AutoPostBack="True"  />                                      
						</div>
						<div class="Clear"></div>
					</div> 
					<div class="Row">
						<div class="Label Medium2">Exposure Date<span class="Required">*</span>:</div>
						<div class="Control">
							<asp:TextBox ID="txtExposureDate" runat="server" CssClass="Size Small" ValidationGroup="form" OnTextChanged="txtExposureDate_TextChanged" AutoPostBack="true" />
							<span class="InlineError" id="lblExposureDateValidate" runat="server" visible="false"></span>
						</div>
						<div class="Clear"></div>
					</div>
					 <div class="Row">
						<div class="Label Medium2">Exposure Time<span class="Required">*</span>:</div>
						<div class="Control">
							<asp:TextBox ID="txtExposureTime" runat="server" CssClass="Size Small" ValidationGroup="form" OnTextChanged="txtExposureTime_TextChanged" AutoPostBack="true" />
							<span class="InlineError" id="lblExposureTimeValidate" runat="server" visible="false"></span>
						</div>
						<div class="Clear"></div>
					</div>
					<div class="Row">
						<div class="Label Medium2">Deep<span class="Required">*</span>:</div>
						<div class="Control">
							<asp:TextBox runat="server" ID="txtDeep" CssClass="Size Small" ValidationGroup="form" />
							<span class="InlineError" id="lblDeepValidate" runat="server" visible="false"></span>
						</div>
						<div class="Clear"></div>
					</div>
					<div class="Row">
						<div class="Label Medium2">Eye<span class="Required">*</span>:</div>
						<div class="Control">
							<asp:TextBox runat="server" ID="txtEye" CssClass="Size Small" ValidationGroup="form" />
							<span class="InlineError" id="lblEyeValidate" runat="server" visible="false"></span>
						</div>
						<div class="Clear"></div>
					</div>

					<div class="Row">
						<div class="Label Medium2">Shallow<span class="Required">*</span>:</div>
						<div class="Control">
							<asp:TextBox runat="server" ID="txtShallow" CssClass="Size Small" ValidationGroup="form" />
							<span class="InlineError" id="lblShallowValidate" runat="server" visible="false"></span>
						</div>
						<div class="Clear"></div>
					</div>                    
					<div class="Row"> 
						<div class="Label Medium2">Region:</div>
						<div class="Control"> 
							<asp:DropDownList ID="ddlRegion" runat="server" OnSelectedIndexChanged="ddlRegion_SelectedIndexChanged" AutoPostBack="true" />                                      
						</div>
						<div class="Clear"></div>
					</div> 
                    <div class="Row" >
						<div class="Label Medium2">EDE:</div>
						<div class="Control">
							<asp:TextBox runat="server" ID="txtEDE" CssClass="Size Small" ValidationGroup="form" OnTextChanged="txtEDE_TextChanged" AutoPostBack="true" />
                            <asp:button text="Calculate EDE" ID="btnCalculateEDE" OnClick="btnCalculateEDE_Click" runat="server" />
							<span class="InlineError" id="lblEDEValidate" runat="server" visible="false"></span>
						</div>
						<div class="Clear"></div>
					</div>
                    <div class="Row" id="edeErrorNote" runat="server" visible="false">
						<div class="Label Medium2"></div>
						<div class="Control">
                            <asp:Label runat="server" ID="lblEDEErrorNote" CssClass="Size Small" ForeColor="Red"  />
						</div>
						<div class="Clear"></div>
					</div>
                    <div class="Row">
						<div class="Label Medium2">EDE Type:</div>
						<div class="Control">
                            <asp:TextBox runat="server" ID="txtEDEType" CssClass="Size Small" ValidationGroup="form" Enabled="false"  />
						</div>
						<div class="Clear"></div>
					</div>
                    <div class="Row">
						<div class="Label Medium2">EDE Status:</div>
						<div class="Control" >                            
                            <asp:TextBox runat="server" ID="txtEDEStatus" CssClass="Size Small" ValidationGroup="form" Enabled ="false"  />
						</div>
						<div class="Clear"></div>
					</div>
					<div class="Row">
						<div class="Label Medium2"></div>
						<div class="Control">
							<asp:CheckBox ID="chkAnomaly" runat="server" Text="Anomaly"  />
						</div>
						<div class="Clear"></div>
					</div>                        
				</div>               
				<asp:button text="Save" style="display:none;" id="btnAddRead" OnClick="btnAddRead_Click" runat="server" ValidationGroup="form" />
				<asp:button text="Close" style="display:none;" id="btnCancelRead" OnClick="btnCancelRead_Click" runat="server" ValidationGroup="form" />
				<asp:button text="Load" style="display:none;" id="btnLoadRead" OnClick="btnLoadRead_Click" runat="server" ValidationGroup="form" />
			</ContentTemplate>
		</asp:UpdatePanel>
	</div>
   
	<div>
		<asp:UpdatePanel ID="UpdatePanelReading" runat="server" UpdateMode="Conditional" Visible="True" >
			<Triggers>
				<asp:AsyncPostBackTrigger controlid="btnAddRead" eventname="Click" />
			</Triggers>
			<ContentTemplate>
				<asp:GridView ID="grdReadingView" CssClass="OTable"  runat="server" Visible ="False"
					AllowPaging="true" PageSize = "20" AllowSorting="true"
					AutoGenerateColumns="False" DataKeyNames="RID" DataSourceID="sqlGetReads" >
					<Columns>
							<asp:TemplateField ShowHeader="False" HeaderStyle-Width="40px" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd">
								<ItemTemplate>
									<asp:ImageButton ID="btnGridAdd" runat="server" onclick="btnGridAdd_Click" ToolTip="Add reading"
									CommandName="AddX" CommandArgument='<%# Eval("RID") %>' 
									ImageUrl="~/css/dsd-default/images/icons/note_add.png" 
									Visible = '<%# YesNo() %>'>
									</asp:ImageButton>
									<asp:ImageButton ID="btnGridEdit" runat="server" onclick="btnGridEdit_Click" ToolTip="Edit reading"
									CommandName="EditX" CommandArgument='<%# Eval("RID") %>' 
									ImageUrl="~/css/dsd-default/images/icons/note_edit.png">
									</asp:ImageButton>
								</ItemTemplate>
							</asp:TemplateField>                            
							<asp:BoundField DataField="RID" HeaderText="Read ID" SortExpression="RID" />
							<asp:BoundField DataField="AccountID" HeaderText="Acc#" SortExpression="AccountID" />
							<asp:BoundField DataField="SerialNo" HeaderText="Serial#" SortExpression="SerialNo" />
							<asp:BoundField DataField="Names" HeaderText="Name" SortExpression="Names" />
							<asp:BoundField DataField="ExposureDate" HeaderText="Read Date" DataFormatString="{0:d}" SortExpression="ExposureDate" />
							<asp:BoundField DataField="ExposureTime" HeaderText="Read Time" DataFormatString="{0:t}" />
							<asp:BoundField DataField="ReadTypeName" HeaderText="Type" SortExpression="ReadTypeName">
								<ItemStyle Wrap="False" />
							</asp:BoundField>
							<asp:BoundField DataField="DeepLowDose" HeaderText="Deep" HeaderStyle-Width="50px">
								<HeaderStyle CssClass="RightAlignHeaderText" />
								<ItemStyle HorizontalAlign="Right" />
							</asp:BoundField>
							<asp:BoundField DataField="EyeDose" HeaderText="Eye" HeaderStyle-Width="50px">
								<HeaderStyle CssClass="RightAlignHeaderText" />
								<ItemStyle HorizontalAlign="Right" />
							</asp:BoundField>
							<asp:BoundField DataField="ShallowLowDose" HeaderText="Shallow" HeaderStyle-Width="50px">
								<HeaderStyle CssClass="RightAlignHeaderText" />
								<ItemStyle HorizontalAlign="Right" />
							</asp:BoundField>
                        	<asp:BoundField DataField="EDEDose" HeaderText="EDE" HeaderStyle-Width="30px" >
								<HeaderStyle CssClass="RightAlignHeaderText" />
								<ItemStyle HorizontalAlign="Right" />
							</asp:BoundField>
							<asp:BoundField DataField="BodyRegionName" HeaderText="Region" SortExpression="BodyRegionName">
								<HeaderStyle CssClass="RightAlignHeaderText" />
								<ItemStyle HorizontalAlign="Right" />
							</asp:BoundField>
							<asp:CheckBoxField DataField="HasAnomaly" HeaderText="Anomaly" SortExpression="HasAnomaly" />                                              
					</Columns>
					<EmptyDataTemplate>
						<div class="NoData">
							There are no dose records found!
							<br />
							<button type="button" id="btnShowAddReadDialog" runat="server" onserverclick="btnShowAddReadDialog_Click"  class="btn btn-primary">Add Read</button>
						</div>
					</EmptyDataTemplate>                                
					<AlternatingRowStyle CssClass="Alt" />
					<PagerStyle CssClass="Footer" />
				</asp:GridView>	
			</ContentTemplate>
		</asp:UpdatePanel>   
					
	</div>

	<div>
		<asp:UpdatePanel ID="UpdatePanelAuditReading" runat="server" UpdateMode="Conditional" Visible="True">
			<Triggers>
				<asp:AsyncPostBackTrigger controlid="btnAddRead" eventname="Click" />
			</Triggers>
			<ContentTemplate>
				<asp:GridView ID="grdAuditReadingView" CssClass="OTable" runat="server" Visible ="false"
				AllowPaging="true" PageSize = "20" AllowSorting="true"
				AutoGenerateColumns="False" DataKeyNames="UDRHistoryID" DataSourceID="sqlGetAuditReads">
					<Columns>                                               
							<asp:BoundField DataField="RID" HeaderText="Read ID" SortExpression="RID" HeaderStyle-Wrap="false" />
							<asp:BoundField DataField="AccountID" HeaderText="Acc#" SortExpression="AccountID" />
							<asp:BoundField DataField="SerialNo" HeaderText="Serial#" SortExpression="SerialNo" />
							<asp:BoundField DataField="Names" HeaderText="Name" SortExpression="Names" HeaderStyle-Wrap="false" />
							<asp:BoundField DataField="ExposureDate" HeaderText="Read Date" DataFormatString="{0:d}" SortExpression="ExposureDate" HeaderStyle-Wrap="false" />
							<asp:BoundField DataField="ExposureTime" HeaderText="Read Time" DataFormatString="{0:T}" />    
							<asp:BoundField DataField="ReadTypeName" HeaderText="Type" SortExpression="ReadTypeName" />
							<asp:BoundField DataField="DeepLowDose" HeaderText="Deep" HeaderStyle-Wrap="false" HeaderStyle-Width="50px">
								<HeaderStyle CssClass="RightAlignHeaderText" />
								<ItemStyle HorizontalAlign="Right" />
							</asp:BoundField>
							<asp:BoundField DataField="EyeDose" HeaderText="Eye" HeaderStyle-Wrap="false" HeaderStyle-Width="50px">
								<HeaderStyle CssClass="RightAlignHeaderText" />
								<ItemStyle HorizontalAlign="Right" />
							</asp:BoundField>
							<asp:BoundField DataField="ShallowLowDose" HeaderText="Shallow" HeaderStyle-Wrap="false" HeaderStyle-Width="50px">
								<HeaderStyle CssClass="RightAlignHeaderText" />
								<ItemStyle HorizontalAlign="Right" />
							</asp:BoundField>
                            <asp:BoundField DataField="EDEDose" HeaderText="EDE" HeaderStyle-Wrap="false" HeaderStyle-Width="30px">
								<HeaderStyle CssClass="RightAlignHeaderText" />
								<ItemStyle HorizontalAlign="Right" />
							</asp:BoundField>
							<asp:BoundField DataField="BodyRegionName" HeaderText="Region"  SortExpression="BodyRegionName">
								<HeaderStyle CssClass="RightAlignHeaderText" />
								<ItemStyle HorizontalAlign="Right" />
							</asp:BoundField>
							<asp:CheckBoxField DataField="HasAnomaly" HeaderText="Anomaly" SortExpression="HasAnomaly" />                                              
							<asp:BoundField DataField="ModifiedDate" HeaderText="Modified" DataFormatString="{0:d}" SortExpression="ModifiedDate" />
							<asp:BoundField DataField="ModifiedBy" HeaderText="Modified By"  SortExpression="ModifiedBy" />
					</Columns>
					<EmptyDataTemplate>
						<div class="NoData">
							There are no audit records found!
						</div>
					</EmptyDataTemplate>                                
					<AlternatingRowStyle CssClass="Alt" />
					<PagerStyle CssClass="Footer" />       
				</asp:GridView>
			</ContentTemplate>
		</asp:UpdatePanel>   
					
	</div>



	<asp:SqlDataSource ID="sqlGetReads" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
		SelectCommand="sp_GetManageDoseReading" SelectCommandType="StoredProcedure">
									
		<SelectParameters>
			<asp:ControlParameter ControlID="txtAccountIDSearch" DefaultValue=" " Name="AccountIDSearch"
			PropertyName="Text" Type="String" />
			<asp:ControlParameter ControlID="txtSerialNoSearch" DefaultValue=" " Name="SerialNoSearch"
			PropertyName="Text" Type="String" />
			<asp:ControlParameter ControlID="txtRIDSearch" DefaultValue=" " Name="RIDSearch" 
			PropertyName="Text" Type="String" />
			<asp:ControlParameter ControlID="ddlReadFilter" DefaultValue=" " 
				Name="ReadFilter" PropertyName="SelectedItem.Value" Type="String" />
			<asp:ControlParameter ControlID="CbSoftRead" DefaultValue=" " Name="SoftReadFilter" PropertyName="Checked" Type="String" />
		</SelectParameters>

	</asp:SqlDataSource> 

	<asp:SqlDataSource ID="sqlGetAuditReads" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
		SelectCommand="sp_GetManageDoseAuditReading" SelectCommandType="StoredProcedure">
									
		<SelectParameters>
			<asp:ControlParameter ControlID="txtAccountIDSearch" DefaultValue=" " Name="AccountIDSearch"
			PropertyName="Text" Type="String" />
			<asp:ControlParameter ControlID="txtSerialNoSearch" DefaultValue=" " Name="SerialNoSearch"
			PropertyName="Text" Type="String" />
			<asp:ControlParameter ControlID="txtRIDSearch" DefaultValue=" " Name="RIDSearch" 
			PropertyName="Text" Type="String" />
			<asp:ControlParameter ControlID="ddlReadFilter" DefaultValue=" " 
				Name="ReadFilter" PropertyName="SelectedItem.Value" Type="String" />
		</SelectParameters>

	</asp:SqlDataSource>
		   
		
</asp:Content>

