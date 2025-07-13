<%@ Page Title="Calendar Setting" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_CalendarSetting" Codebehind="CalendarSetting.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    
    <script type="text/javascript">

        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(showProgress);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(hideProgress);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();

        });

        function JQueryControlsLoad() {

            // ServiceStartDate Datepicker in GridView
            $("[id$=txtStartDate]").datepicker({
                changeMonth: true,
                changeYear: true
            });

            $('#ScheduleReadGroupDialog').dialog({
                autoOpen: false,
                width: 600,
                resizable: false,
                title: "Manage Calendar Setting",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('#<%= btnLoadScheduleReadGroup.ClientID %>').click();
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Ok": function () {
                        $('#<%= btnAddScheduleReadGroup.ClientID %>').click();
                    },

                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancelScheduleReadGroup.ClientID %>').click();
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

    <div id="ScheduleReadGroupDialog" >
        <asp:UpdatePanel ID="upnl" runat="server" UpdateMode="Conditional"    >
            <ContentTemplate>
            
                <div class="FormError" id="DialogError" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="DialogErrorMsg" runat="server" >An error was encountered.</span></p>
				</div>
                <div class="FormMessage" id="messages" runat="server" visible="false"> 
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="submitMsg" runat ="server" >Ready to search.</span></p>
                </div> 

                <div id="Div1"  class="OForm" runat ="server">                                                                                        
                    
                    <div class="Row">
                        <div class="Label Medium"></div>
                        <div class="Control">
                            <asp:CheckBox ID="chkBoxDeActive" runat="server" Text="De-activate Calendar Setting" />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Medium">Group<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlDeviceGroup" runat="server"  />                                    
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Medium">Schedule Name<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtScheduleName" 
                                MaxLength="40" CssClass="Size Large" ValidationGroup="form" />                                                                                    
                        </div>
                        <div class="Clear"></div>
                    </div> 

                    <div class="Row">
                        <div class="Label Medium">Schedule Desc:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtScheduleDesc" 
                                MaxLength="100" CssClass="Size Large" ValidationGroup="form" />                                                                                    
                        </div>
                        <div class="Clear"></div>
                    </div>                     
                            
                    <div class="Row">
                        <div id = "EditDetail" runat="server" class="Row" >
                            <asp:GridView ID="grdViewEditDetail" CssClass="OTable"  runat="server" AutoGenerateColumns="False" 
                                OnRowDataBound="grdViewEditDetail_RowDataBound" >
                                <Columns>                            
                                                                                                                                                                                               
                                    <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  HeaderText="Read Type" >
                                        <ItemTemplate>  
                                            <asp:Label ID="lblReadTypeID" runat="server" Text='<%# Eval("ReadTypeID") %>' Visible="false" />                                          
                                            <asp:DropDownList ID="ddlReadType" runat="server" DataTextField="ReadTypeName" DataValueField="ReadTypeID" >                                                            
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField> 
                                                
                                    <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  HeaderText="Start Date" >
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtStartDate"  runat = "Server" Width="90px" Text='<%# Eval("StartDate") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>                                         

                                    <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  HeaderText="Start Time">
                                        <ItemTemplate>
                                            <%--<asp:TextBox ID="txtStartTime"  runat = "Server"  Width="60px" Text='<%# Eval("StartTime") %>' />--%>
                                            <asp:Label ID="lblStartTime" runat="server" Text='<%# Eval("StartTime") %>' Visible="false" />   
                                            <asp:DropDownList ID="ddlStartTime" runat="server" >                                                
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  HeaderText="% Read">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtReadPercentage"  runat = "Server"  Width="40px" Text='<%# Eval("ReadPercentage") %>'  />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                                                                                                                                                                                                           
                                </Columns>

                                <EmptyDataTemplate>
						            <div class="NoData">
							            
						            </div>
					            </EmptyDataTemplate>                                
                                <AlternatingRowStyle CssClass="Alt" />
					            <PagerStyle CssClass="Footer" />
                        
                            </asp:GridView>                             
                            <asp:LinkButton id="btnAnotherRead" runat="server" text ="Another Read ..." onclick="btnAnotherRead_Click" /> 
                        </div> 
                    </div>                                                                                                                 
                                                                                                                                                                                                                                                                                                                                                                            
                </div>                                                                                          
                                   
                <asp:button text="Save" style="display:none;" id="btnAddScheduleReadGroup" OnClick="btnAddScheduleReadGroup_Click" runat="server" />
                <asp:button text="Close" style="display:none;" id="btnCancelScheduleReadGroup" OnClick="btnCancelScheduleReadGroup_Click" runat="server" />
                <asp:button text="Load" style="display:none;" id="btnLoadScheduleReadGroup" OnClick="btnLoadScheduleReadGroup_Click" runat="server" />

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <div>

		<asp:UpdatePanel ID="UpdatePanelScheduleReadGroup" runat="server"  UpdateMode="Conditional">
			<Triggers>
				<asp:AsyncPostBackTrigger controlid="btnAddScheduleReadGroup" eventname="Click" />                
			</Triggers>

			<ContentTemplate>

				<div class="OToolbar JoinTable" id="ScheduleReadGroupSearchToolBar" runat ="server">
					<ul>
                    
                        <li>                                        
                            <asp:LinkButton ID="btnNewScheduleReadGroup" runat="server"  
                                CommandName="NewScheduleReadGroup" CommandArgument="" 
                                CssClass="Icon Add" onclick="btnNewScheduleReadGroup_Click" >Create Calendar Setting</asp:LinkButton>                               
                        </li>						                                                   
                        <li>Group Search:</li>
                        <li><asp:TextBox ID="txtGroup" runat="server" class="Size Small" Text=""></asp:TextBox></li>
                        <li><asp:Button ID="btnGroupSearch" runat="server" Text="Go" CssClass="OButtonFormDetail" /></li>
					</ul>
				</div>

                <asp:GridView ID="grdScheduleReadGroupsView" CssClass="OTable"  runat="server" Visible ="true"
                    AllowPaging="true" PageSize = "20" AllowSorting="true"
                    AutoGenerateColumns="False" DataKeyNames="ScheduleReadGroupID" DataSourceID="GetScheduleReadGroups" >
                    <Columns>                            
                            
                            <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="Device Group" SortExpression="DeviceGroupName" >
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEditScheduleReadGroup" runat="server"  
                                        CommandName="EditScheduleReadGroup" CommandArgument='<%# Eval("ScheduleReadGroupID")%>' 
                                        onclick="btnEditScheduleReadGroup_Click" ><%# Eval("DeviceGroupName")%></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>                            
                             
                            <%--<asp:BoundField DataField="DeviceGroupName" HeaderText="Group"  SortExpression="DeviceGroupName" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="100px" />                           --%>
                            <asp:BoundField DataField="ScheduleName" HeaderText="Schedule Name" SortExpression="ScheduleName" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                                                          
                            <asp:BoundField DataField="StartDate" HeaderText="Start Date" SortExpression="StartDate" DataFormatString="{0:d}" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="80px" />                            
                            <asp:BoundField DataField="StartTime" HeaderText="Start Time" SortExpression="StartTime" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="80px" />                            

                            <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="" HeaderStyle-Width="90px">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hyprlnkResultStatus" runat="server" NavigateUrl='<%# String.Format("CalendarSettingReport.aspx?ScheduleReadGroupID={0}", Eval("ScheduleReadGroupID")) %>' Text='Result Status' ></asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>  
                            
                            <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="" HeaderStyle-Width="90px">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hyprlnkSynchStatus" runat="server" NavigateUrl='<%# String.Format("CalendarSynchStatus.aspx?ScheduleReadGroupID={0}", Eval("ScheduleReadGroupID")) %>' Text='Synch Status' ></asp:HyperLink>
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


