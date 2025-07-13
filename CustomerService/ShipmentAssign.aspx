<%@ Page Title="Enterprise User Assignment" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_ShipmentAssign" Codebehind="ShipmentAssign.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
<script type="text/javascript">

    $(document).ready(function() {

        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(showProgress);
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(hideProgress);
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
        JQueryControlsLoad();        

    });
    
    function JQueryControlsLoad() {

        $("#TabContainer1").tabs();

        // UserBirthDate Datepicker
        $('#<%=txt_Startdate.ClientID %>').datepicker();
        $('#<%=txt_Enddate.ClientID %>').datepicker();

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

<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" />
    
    <asp:UpdateProgress id="EnterpriseUserUpdateProgress" runat="server" DynamicLayout="true" DisplayAfter="0" >
        <ProgressTemplate>
            <%--<div style="width:100%" align="center">
                <img src="../images/barloader.gif" />
            </div>--%>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <act:ModalPopupExtender ID="modalExtender" runat="server" TargetControlID="EnterpriseUserUpdateProgress"
        PopupControlID="Panel1" BackgroundCssClass="modalBackground" Enabled="true" >
    </act:ModalPopupExtender>

    <asp:Panel ID="Panel1" runat="server">
        <div style="width: 100%" align="center">
            <img src="../images/orangebarloader.gif" />
        </div>
    </asp:Panel>

    <%--Start TabsContainer Section--%>
    <div id="TabContainer1" >

	    <ul>				
            <li><a href="#TabPanel1" id="TabPanel1Header" runat ="server">Open Queue</a></li>	
            <li><a href="#TabPanel2" id="TabPanel2Header" runat ="server">Submit Request</a></li>	
            <li><a href="#TabPanel3" id="TabPanel3Header" runat ="server">Order & Ship History</a></li>						               
	    </ul>

        <!-- Open Queue  -->   
        <div id="TabPanel1">

            <asp:UpdatePanel ID="UpdateTabPanel1" runat="server" UpdateMode="Conditional">

                <Triggers>
					<asp:AsyncPostBackTrigger controlid="btn_assignShipTo" eventname="Click" />
				</Triggers>

                <ContentTemplate>

                    <%--<asp:UpdateProgress id="UpdateProgressMerge" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>  --%>                                      
                    
                    <div class="OToolbar JoinTable" id="CreateDevice" runat ="server">
						<ul>
							<li style="margin-left:10px;">                                                                        								
                                <asp:LinkButton ID="btn_deleteUser" runat="server"
						            CssClass="Icon Remove" onclick="btn_deleteUser_Click" >Delete Selected User</asp:LinkButton>                                      
                                <asp:LinkButton ID="btn_refreshQueue" runat="server"
						            CssClass="Icon Refresh" onclick="btn_refreshQueue_Click" >Refresh</asp:LinkButton> 
                                    
                                <span style="padding:0; float:right" >
                                    <asp:Label ID="lbl_openshipment" runat="server"></asp:Label>
                                </span>      
								
							</li>
						</ul>
					</div> 

                    <asp:GridView ID="gv_openshipment" runat="server" CssClass="OTable" AllowSorting="True" 
                        AutoGenerateColumns="False" DataSourceID="SqlDataSource1" >
                        <Columns>
                            <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="Select">
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbxSelectDelete" runat="server" Checked="false" />
                                </ItemTemplate>
                                <HeaderStyle CssClass="mt-hd" />
                                <ItemStyle CssClass="mt-itm" Width="30px" />
                            </asp:TemplateField>

                            <asp:BoundField DataField="userid" HeaderText="User#" 
                                SortExpression="userid" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                            <asp:BoundField DataField="accountid" HeaderText="Acc#" 
                                SortExpression="accountid" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                            <asp:BoundField DataField="Serialno" HeaderText="Serial#" 
                                SortExpression="serialno" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" 
                                ItemStyle-Font-Bold="true" ItemStyle-Font-Size="Medium"  />                                                                      
                            <asp:BoundField DataField="PackageID" HeaderText="Package#" 
                                SortExpression="PackageID" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" /> 
                            <asp:BoundField DataField="companyname" HeaderText="Company" 
                                SortExpression="companyname" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" /> 
                            <asp:BoundField DataField="Name" HeaderText="Name" ReadOnly="True" 
                                SortExpression="Name" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                            <asp:BoundField DataField="Address" HeaderText="Address" 
                                SortExpression="Address" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                            <%--<asp:BoundField DataField="Address2" HeaderText="Address2" 
                                SortExpression="Address2" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />--%>
                            <asp:BoundField DataField="City" HeaderText="City" SortExpression="City" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                            <asp:BoundField DataField="State" HeaderText="State" SortExpression="State" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                            <%--<asp:BoundField DataField="PostalCode" HeaderText="Zip" 
                                SortExpression="PostalCode" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                            <asp:BoundField DataField="Country" HeaderText="Country" 
                                SortExpression="Country" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />--%>
                            <%--<asp:BoundField DataField="AssignedBy" HeaderText="By" 
                                SortExpression="AssignedBy" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                            <asp:BoundField DataField="AssignedDate" HeaderText="Date" 
                                SortExpression="AssignedDate" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />   --%>                       
                            <asp:BoundField DataField="UserShipAssignID" HeaderText="Ref#" 
                                SortExpression="UserShipAssignID" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" /> 
                            <asp:BoundField DataField="locationid" HeaderText="Loc#" 
                                SortExpression="loactionid" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />       
                        </Columns>
                        <EmptyDataTemplate>
                            <div class = "NoData">
                                There are no open users shipment!
                            </div>
                        </EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
                        <PagerStyle CssClass="Footer" />

                    </asp:GridView>

                    <asp:SqlDataSource ID="SqlDataSource1" runat="server" 
                            ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>" 
                            SelectCommand="sp_if_GetUserShipmentByAccountNo" 
                            SelectCommandType="StoredProcedure">
                        <SelectParameters>
                            <asp:Parameter  DefaultValue=" " Name="FromDate" Type="String" />
                            <asp:Parameter  DefaultValue=" " Name="endDate" Type="String" />
                            <asp:Parameter DefaultValue="0" Name="accountid" Type="Int32" />  
                            <asp:Parameter DefaultValue="1" Name="reportType" Type="Int32" />
                        </SelectParameters>
                    </asp:SqlDataSource>  
                                                           
                </ContentTemplate>
            </asp:UpdatePanel>
                                                                   
        </div>
        <!-- Open Queue  -->   

        <!-- Submit Request  -->   
	    <div id="TabPanel2">

            <asp:UpdatePanel ID="UpdateTabPanel2" runat="server"  UpdateMode="Conditional">
                
                <ContentTemplate>

                    <%--<asp:UpdateProgress id="UpdateProgressInsert" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress> --%>
                    
                    <div class="OToolbar JoinTable" id="Div1" runat ="server">
						<ul>
							<li style="margin-left:10px;"> 
                                <asp:LinkButton ID="btn_refreshRequest" runat="server"
						            CssClass="Icon Refresh" onclick="btn_refreshRequest_Click" >Refresh</asp:LinkButton>
                                <asp:LinkButton ID="btn_assignShipTo" runat="server"
						            CssClass="Icon LightningGo " onclick="btn_assignShipTo_Click" >Ship to USER</asp:LinkButton> &nbsp; &nbsp;&nbsp;                                                                                                                                         
                                                                                                                                                                       
								Enter Acc#: <asp:TextBox ID="txt_Accountid" runat="server" Width="80px"></asp:TextBox>
								<asp:Button ID="btn_findAccountID" runat="server" Text="Go" 
                                    onclick="btn_findAccountID_Click" CssClass = "OButtonFormDetail" /> 
                                    
                                <span style="padding:0; float:right" >
                                    <asp:Label ID="lbl_shipUser" runat="server"></asp:Label>
                                </span>                                  
                                
							</li>                            
						</ul>
					</div>                                                                                                                                           

                    <!-- Display all users by account ID-->            
                    <asp:GridView ID="gv_shipUser" runat="server" CssClass="OTable" AllowSorting="True" 
                        AutoGenerateColumns="False" DataSourceID="SqlDataSource2" >
                            <Columns>
                                <asp:TemplateField HeaderText="Select" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="cbxSelect" runat="server" Checked="false" Visible = '<%# DisplaySelectCheckBox(Eval("deviceactive")) %>' />
                                    </ItemTemplate>
                                    <HeaderStyle CssClass="mt-hd" />
                                    <ItemStyle CssClass="mt-itm" Width="30px" />
                                </asp:TemplateField>
                                <asp:BoundField DataField="UserID" HeaderText="User#" 
                                    SortExpression="UserID" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                                <asp:BoundField DataField="serialno" HeaderText="Serial#" 
                                    SortExpression="serialno" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                                <asp:BoundField DataField="deviceactive" HeaderText="Active" 
                                    SortExpression="deviceactive" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                                <asp:BoundField DataField="FirstName" HeaderText="First Name" 
                                    SortExpression="FirstName" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                                <asp:BoundField DataField="LastName" HeaderText="Last Name" 
                                    SortExpression="LastName" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                                <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                                <asp:BoundField DataField="ShipmentRecord" HeaderText="Shipping Records" ItemStyle-Wrap="true"
                                    SortExpression="ShipmentRecord" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" Visible="false" />
                                                                    
                                <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  FooterStyle-HorizontalAlign="center"
                                        ItemStyle-HorizontalAlign="left" ItemStyle-Wrap="false" HeaderText="Shipping Address">
                                        <ItemTemplate>
                                            <div><asp:Label ID="lbluseraddress1" runat="server" Text='<%# Bind("Address1") %>' /></div>
                                            <div><asp:Label ID="lbluseraddress2" runat="server" Text='<%# Bind("Address2") %>' /></div>
                                            <div><asp:Label ID="Label3" runat="server" Text='<%# Bind("city") %>' /></div>
                                            <div><asp:Label ID="Label4" runat="server" Text='<%# Bind("state") %>' />
                                            <asp:Label ID="Label5" runat="server" Text='<%# Bind("postalcode") %>' />
                                            <asp:Label ID="Label6" runat="server" Text='<%# Bind("country") %>' /></div>
                            
                                        </ItemTemplate>
                                </asp:TemplateField>
                                                
                            </Columns>

                            <EmptyDataTemplate>
                                <div class = "NoData">
                                    No User Found!
                                </div>
                            </EmptyDataTemplate>
                            <AlternatingRowStyle CssClass="Alt" />
                            <PagerStyle CssClass="Footer" />
                            
                    </asp:GridView>                    
            
                    <asp:SqlDataSource ID="SqlDataSource2" runat="server" 
                                ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>" 
                                SelectCommand="sp_if_GetUserShipmentByAccountNo" 
                                SelectCommandType="StoredProcedure">
                        <SelectParameters>
                            <asp:Parameter  DefaultValue=" " Name="FromDate" Type="String" />
                            <asp:Parameter  DefaultValue=" " Name="endDate" Type="String" />
                            <asp:ControlParameter ControlID="txt_Accountid" DefaultValue="0" 
                                Name="accountid" PropertyName="Text" Type="Int32" />  
                            <asp:Parameter DefaultValue="2" Name="reportType" Type="Int32" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                     
                </ContentTemplate>
            </asp:UpdatePanel>   
                    
        </div>
        <!-- Submit Request  -->

        <!-- Order & Ship History  -->   
	    <div id="TabPanel3">

            <asp:UpdatePanel ID="UpdatePanel1" runat="server"  UpdateMode="Conditional">
                
                <ContentTemplate>

                    <%--<asp:UpdateProgress id="UpdateProgress1" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>  --%>                                    

                    <div class="OToolbar JoinTable" id="Div2" runat ="server">
						<ul>
							<li style="margin-left:10px;">   
                                <asp:LinkButton ID="btn_refreshHistory" runat="server"
						            CssClass="Icon Refresh" onclick="btn_refreshHistory_Click" >Refresh</asp:LinkButton> &nbsp; &nbsp;&nbsp;
                                                                                                 
								Assign Date From: 
                                <asp:TextBox ID="txt_Startdate" runat="server" Width= "80px"></asp:TextBox>
                                     <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
                                         ControlToValidate="txt_Startdate" Display="Dynamic" 
                                         ErrorMessage="*Date Required " Font-Bold="True" ValidationGroup="Valid_form2">
                                     </asp:RequiredFieldValidator>
                                To: 
                                <asp:TextBox ID="txt_Enddate" runat="server" Width= "80px"></asp:TextBox>
                                     <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" 
                                        ControlToValidate="txt_Enddate" Display="Dynamic" 
                                        ErrorMessage="*Date Required " Font-Bold="True" ValidationGroup="Valid_form2">
                                     </asp:RequiredFieldValidator>
								<asp:Button ID="btnFindRecordDate" runat="server" Text="Go" 
                                     ValidationGroup="Valid_form2" CssClass = "OButtonFormDetail" />
                                
                                <span style="padding:0; float:right" >
                                    <asp:Label ID="lbl_orderShipHistory" runat="server"></asp:Label>
                                </span>  
                                
							</li>
                            
						</ul>
					</div>                                        

                    <asp:GridView ID="gv_orderShipHistory" runat="server" CssClass="OTable" AllowSorting="True" 
                        AutoGenerateColumns="False" DataSourceID="SqlDataSource3" >
                         <Columns>
                             <asp:BoundField DataField="assignedBy" HeaderText="By" 
                                 SortExpression="assignedBy" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                             <asp:BoundField DataField="AssignedDate" HeaderText="Assigned Date" 
                                 SortExpression="AssignedDate" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                             <asp:BoundField DataField="AccountID" HeaderText="Acc#" 
                                 SortExpression="AccountID" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                             <asp:BoundField DataField="Userid" HeaderText="User#" 
                                 SortExpression="Userid" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                             <asp:HyperlinkField DataNavigateUrlFields="Serial#" 
                                 DataNavigateUrlFormatString="http://portal.instadose.com/Instadose/InformationFinder/Details/Device.aspx?ID={0}" 
                                 DataTextField="SerialNo" HeaderText="SerialNo" SortExpression="SerialNo" 
                                 Target="_blank">
                                 <HeaderStyle CssClass="mt-hd" />
                                 <ItemStyle HorizontalAlign="Center" CssClass = "mt-itm" />
                             </asp:HyperlinkField>
                             <asp:HyperlinkField DataNavigateUrlFields="Tracking#" 
                                 DataNavigateUrlFormatString="http://fedex.com/Tracking?ascend_header=1&amp;clienttype=dotcom&amp;cntry_code=us&amp;language=english&amp;tracknumbers={0}" 
                                 DataTextField="TrackingNO" HeaderText="Tracking#" SortExpression="TrackingNO" 
                                 Target="_blank">
                                 <HeaderStyle CssClass="mt-hd" />
                                 <ItemStyle HorizontalAlign="Center" CssClass = "mt-itm" />
                             </asp:HyperlinkField>
                             <asp:BoundField DataField="ShipDate" HeaderText="Ship Date" 
                                 SortExpression="ShipDate" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                             <asp:BoundField DataField="company" HeaderText="Company" ReadOnly="True" 
                                 SortExpression="company" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                             <asp:BoundField DataField="name" HeaderText="User Name" ReadOnly="True" 
                                 SortExpression="name" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />
                         </Columns>

                         <EmptyDataTemplate>
                            <div class = "NoData">
                                No User Shipment Record Found!
                            </div>
                        </EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
                        <PagerStyle CssClass="Footer" />
                         
                    </asp:GridView>

                    <asp:SqlDataSource ID="SqlDataSource3" runat="server" 
                            ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>" 
                            SelectCommand="sp_if_GetUserShipmentByAccountNo" 
                            SelectCommandType="StoredProcedure">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="txt_Startdate" DefaultValue=" " 
                            Name="FromDate" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="txt_Enddate" DefaultValue=" " 
                            Name="endDate" PropertyName="Text" Type="String" />
                            <asp:Parameter DefaultValue="0" Name="accountid" Type="Int32" />  
                            <asp:Parameter DefaultValue="3" Name="reportType" Type="Int32" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                     
                </ContentTemplate>
            </asp:UpdatePanel>   
                    
        </div>
        <!-- Order & Ship History  -->

    </div>
    <%--End TabsContainer Section--%>
            
</asp:Content>

