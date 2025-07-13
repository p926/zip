<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_CreateOntimeItem" Codebehind="CreateOntimeItem.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
     <script type="text/javascript">

         $(document).ready(function () {
             Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(showProgress);
             Sys.WebForms.PageRequestManager.getInstance().add_endRequest(hideProgress);
             Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
             JQueryControlsLoad();

         });

         function JQueryControlsLoad() {

             var activeTabIndex = parseInt($('#<%= hdnfldTabIndex.ClientID %>').val());

             //$("#tabsContainer").tabs();

             $("#tabsContainer").tabs({
                 selected: activeTabIndex,
                 show: function () {
                     var selectedTab = $('#tabsContainer').tabs('option', 'selected');
                     $("#<%= hdnfldTabIndex.ClientID %>").val(selectedTab);
                }
              });
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

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="server" >
    <act:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server">
    </act:toolkitscriptmanager> 

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

     <div class="FormError" id="dialogErrors" runat="server" visible="false">
			    <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong> <span id="dialogErrorMsg" runat="server" >An error was encountered.</span></p>
	 </div>

     <div class="FormMessage" id="dialogSuccess" runat="server" visible="false" style="margin:10px" > 
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong> <span id="dialogSuccessMsg" runat ="server" >Ready to search.</span></p>
     </div> 

     <div id = "backButton" runat = "server" class="Right" style="width: auto;">
                <asp:Button ID="btnRedirect" CssClass="OButton" runat="server" Text="Back" 
                    onclick="btnBack_Click" />                                       
     </div>

     <%--Start TabsContainer Section--%>
    <div id="tabsContainer">
    
        <asp:HiddenField ID="hdnfldTabIndex" runat="server" Value="0" />

         <ul>				                        
            <li><a href="#TabPanel1" id="TabPanel1Header" runat ="server">Create Item</a></li>						               
	        <li><a href="#TabPanel2" id="TabPanel2Header" runat ="server">View Items</a></li>	
         </ul> 
        
        <!-- View Items  -->   
	    <div id="TabPanel2">        
            
            <div class="OToolbar JoinTable">
                    <ul>
                        <%--<li>Project:</li>
                        <li>
                            <asp:DropDownList ID="ddlViewProject" runat="server" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlProject_SelectedIndexChanged">
                            </asp:DropDownList>
                        </li>--%>
                        <li>Type:</li>
                        <li>
                            <asp:DropDownList ID="ddlBackLogType" runat="server" AutoPostBack="true" 
                                 OnSelectedIndexChanged="ddlBackLogType_SelectedIndexChanged">
                                <asp:ListItem>ALL</asp:ListItem>
                                <asp:ListItem>defects</asp:ListItem>
                                <asp:ListItem>incidents</asp:ListItem>
                                <asp:ListItem>features</asp:ListItem>
                            </asp:DropDownList>
                        </li>
                       <%-- <li>Worksflow Step:</li>
                            <li>
                            <asp:DropDownList ID="ddlWorkflow" runat="server" AutoPostBack="true" 
                                 OnSelectedIndexChanged="ddlWorkflow_SelectedIndexChanged">
                                <asp:ListItem>ALL</asp:ListItem>
                                <asp:ListItem>Defect</asp:ListItem>
                                <asp:ListItem>Incident</asp:ListItem>
                            </asp:DropDownList>
                        </li>
                         <li>Assigned To:</li>
                        <li>
                            <asp:DropDownList ID="ddlAssignedTo" runat="server" AutoPostBack="true" 
                                 OnSelectedIndexChanged="ddlAssignedTo_SelectedIndexChanged">
                                <asp:ListItem>ALL</asp:ListItem>
                                <asp:ListItem>Defect</asp:ListItem>
                                <asp:ListItem>Incident</asp:ListItem>
                            </asp:DropDownList>
                        </li>--%>
                        <li>Status:</li>
                        <li>
                            <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" 
                                 OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                                <asp:ListItem>ALL</asp:ListItem>
                            </asp:DropDownList>
                        </li>
                        <li class="RightAlign">
                            <asp:LinkButton ID="lnkbtnClearFilters" runat="server" 
                                    OnClick="lnkbtnClearFilters_Click" 
                                    Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" 
                                    CssClass="Icon Remove" ToolTip="Clear Filters" />
                        </li>
                        <li class="RightAlign">
                            <asp:LinkButton ID="lnkReturn" runat="server" 
                                    OnClick="lnkbtnReturn_Click" Visible="false"
                                    Text="Return" Font-Bold="true" ForeColor="#FFFFFF" 
                                    CssClass="Icon LightningGo"  ToolTip="Clear Filters" />
                        </li>
                    </ul>
            </div>
            <%--OnPreRender="grdItemsView_PreRender" 
            <asp:LinqDataSource runat="server" ID="ldsgrdItemsView" OnSelecting="grdItemsView_Selecting"></asp:LinqDataSource>
            --%>
            <ec:GridViewEx ID="grdItemsView" CssClass="OTable"  runat="server" AutoGenerateColumns="False"
                CurrentSortDirection="Descending" OnSorting="grdItemsView_Sorting"
                 currentSortedColumn="ItemID" AllowSorting="true">                    
                        
                <Columns>                            
                             
                    <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="ID" HeaderStyle-Width="50px">
                        <ItemTemplate>
                            <asp:HyperLink ID="hyprlnkItemID" runat="server" 
                                NavigateUrl='<%# String.Format("OntimeItemDetail.aspx?ItemID={0}&Type={1}", Eval("ItemID"), Eval("BacklogType")) %>' Text='<%# Eval("ItemID") %>'  >
                            </asp:HyperLink>
                        </ItemTemplate>
                    </asp:TemplateField> 
                                                                                                                                                                                                                       
                    <asp:BoundField DataField="OnTimeProject" HeaderText="Project" SortExpression="OnTimeProject" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="100px" />                            
                    <asp:BoundField DataField="BacklogType" HeaderText="Type" SortExpression="BacklogType" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="70px" />  
                    
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="200px" />                                                          
                    <asp:BoundField DataField="WorkflowStep" HeaderText="Workflow Step" SortExpression="WorkflowStep" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="100px" />                            
                    <asp:BoundField DataField="AssignTo" HeaderText="Assign To" SortExpression="AssignTo" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="100px" />                                    
                    <asp:BoundField DataField="ReportedDate" HeaderText="Date Found" SortExpression="ReportedDate" DataFormatString="{0:d}" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="80px" />                                                          
                    <asp:BoundField DataField="CompleteDate" HeaderText="Completed" SortExpression="CompleteDate" DataFormatString="{0:d}" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="80px" />                            
                    <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="80px" />                                                                      
                                                                                                                                                                                                                           
                </Columns>

                <EmptyDataTemplate>
					<div class="NoData">
						There are no items!
					</div>
				</EmptyDataTemplate>                                
                <AlternatingRowStyle CssClass="Alt" />
				<PagerStyle CssClass="Footer" />
                        
            </ec:GridViewEx>	
        </div>
        <!-- View Items  -->

        <!-- Create Item  -->   
        <div id="TabPanel1">

          <%--  <div class="FormError" id="dialogErrors" runat="server" visible="false">
			    <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong> <span id="dialogErrorMsg" runat="server" >An error was encountered.</span></p>
		    </div>

            <div class="FormMessage" id="dialogSuccess" runat="server" visible="false" style="margin:10px" > 
	            <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong> <span id="dialogSuccessMsg" runat ="server" >Ready to search.</span></p>
            </div> 

            <div id = "backButton" runat = "server" class="Right" style="width: auto;">
                <asp:Button ID="btnRedirect" CssClass="OButton" runat="server" Text="Back" 
                    onclick="btnBack_Click" />                                       
            </div>--%>
        
            <div class="OForm" id = "mainForm" runat = "server">                                          
             
                <div class="Row">
                    <div class="Label Medium2">Backlog Type:</div>
                    <div class="Control">
                        <asp:RadioButtonList id="radBacklogType" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" OnSelectedIndexChanged="radBacklogType_SelectedIndexChanged">                        
                            <asp:ListItem selected="true">Help Desk</asp:ListItem>
                            <asp:ListItem Enabled="false" >Defect</asp:ListItem>
                            <asp:ListItem Enabled ="false" >Feature</asp:ListItem>
                        </asp:RadioButtonList>                                        
                    </div>
                    <div class="Clear"></div>
                </div> 
                         
                <div class="Row">
                    <div class="Label Medium2">Project<span class="Required">*</span>:</div>
                    <div class="Control">
                        <asp:DropDownList ID="ddlProjects" runat="server"  />                                    
                    </div>
                    <div class="Clear"></div>
                </div> 

                <div class="Row">
                    <div class="Label Medium2">Severity:</div>
                    <div class="Control">
                        <asp:DropDownList ID="ddlSeverity" runat="server">
                            <asp:ListItem Text="" Value="0" Selected="True" />
                            <asp:ListItem Text="No Impact" Value="5"  />
                            <asp:ListItem Text="Low Impact" Value="3"  /> 
                            <asp:ListItem Text="Medium Impact" Value="4" />
                            <asp:ListItem Text="High Impact" Value="2" />
                            <asp:ListItem Text="Critical" Value="1" /> 
                        </asp:DropDownList>                                    
                    </div>
                    <div class="Clear"></div>
                </div> 

                <div class="Row">
                    <div class="Label Medium2">Title<span class="Required">*</span>:</div>
                    <div class="Control"><asp:TextBox runat="server" ID="txtTitle" 
                            CssClass="Size Large " ValidationGroup="form" /></div>
                    <div class="Clear"></div>
                </div>

                <div class="Row">
                    <div class="Label Medium2">Customer Contact:</div>
                    <div class="Control">
                        <asp:DropDownList ID="ddlContacts" runat="server" Enabled = "false" />                                    
                    </div>
                    <div class="Clear"></div>
                </div> 

                <div class="Row">
                    <div class="Label Medium2">Description<span class="Required">*</span>:</div>
                    <div class="Control"><asp:TextBox runat="server" ID="txtDescription" TextMode="MultiLine"
                            CssClass="Size Large " ValidationGroup="form" Height="150px" 
                            Width="800px" /></div>
                    <div class="Clear"></div>
                </div>                    

                <div class="Row">
                    <div class="Label Medium2">Repro Steps:</div>
                    <div class="Control"><asp:TextBox runat="server" ID="txtReproStep" TextMode="MultiLine"
                            CssClass="Size Large " ValidationGroup="form" Height="150px" 
                            Width="800px"/></div>
                    <div class="Clear"></div>
                </div>

                 <div class="Row">
                    <div class="Label Medium2">Notes:</div>
                    <div class="Control"><asp:TextBox runat="server" ID="txtNotes" TextMode="MultiLine"
                            CssClass="Size Large " ValidationGroup="form" Height="150px" 
                            Width="800px"/></div>
                    <div class="Clear"></div>
                </div>

                <div class="Row">
                    <div class="Label Medium2 ">Attachment 1:</div>
                    <div class="Control">
                        <asp:FileUpload ID="attachment1" runat="server" Width="400px" />                            
                    </div>
                    <div class="Clear"></div>
                </div>                                              

                <div class="Row">
                    <div class="Label Medium2">Attachment 2:</div>
                    <div class="Control">
                        <asp:FileUpload ID="attachment2" runat="server" Width="400px" />                            
                    </div>
                    <div class="Clear"></div>
                </div>                                   

                <div class="Row" id = "deleteSection" runat="server" visible="true">
                    <div class="Label Medium2">Delete Item#:</div>
                    <div class="Control"><asp:TextBox runat="server" ID="txtItemNo" 
                            CssClass="Size Small" ValidationGroup="form" /></div>
                    <div class="Clear"></div>
                </div>
                    
                <div class="Row">
                    <div class="Label Medium2">&nbsp;</div>
                    <div class="Control">
                        <asp:Button Text="Add" ID="btnAdd" runat="server" 
                                ValidationGroup="form" cssClass="OButton" 
                            onclick="btnAdd_Click" /> &nbsp;&nbsp;&nbsp;
                        <asp:Button Text="Reset" ID="btnReset" runat="server" 
                                ValidationGroup="form" cssClass="OButton" 
                            onclick="btnReset_Click" /> &nbsp;&nbsp;&nbsp;
                        <asp:Button Text="Back" ID="btnBack" runat="server" 
                                ValidationGroup="form" cssClass="OButton" 
                            onclick="btnBack_Click" /> &nbsp;&nbsp;&nbsp;
                        <asp:Button Text="Delete" ID="btnDelete" runat="server" visible="true"
                                ValidationGroup="form" cssClass="OButton" onclick="btnDelete_Click" 
                                /> 
                    </div>
                    <div class="Clear"></div>
                </div>                                        

            </div>  
                                                                   
        </div>
        <!-- Create Item  -->       
    
    </div>
    
          
    

</asp:Content>

