<%@ Page Title="Shipping Queue" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Manufacturing_ShippingQueue" Codebehind="ShippingQueue.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript">

        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(showProgress);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(hideProgress);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();

        });

        function JQueryControlsLoad() {

            $("#tabsContainer").tabs();

            $(".date-picker").datepicker({
                onClose: function () {
                    Page_ClientValidate();
                    if (!Page_IsValid)
                        $('#btnOrderSearch').prop('disabled', true);
                    else
                        $('#btnOrderSearch').prop('disabled', false);
                }
            });

            // UserBirthDate Datepicker
            $('#<%=txt_Startdate.ClientID %>').datepicker();

            // LastReminded Datepicker
            $('#<%=txt_Enddate.ClientID %>').datepicker();

            // selected pick sheet boxes.
            $('.chkbxHeaderPickSheet').click(function () {
                //ctl00_primaryHolder_TabContainer1_TabPanel1_gv_PickSheet_ctl01_chkbxHeaderPickSheet
                var ischecked = ($('#ctl00_primaryHolder_gv_PickSheet_ctl01_chkbxHeaderPickSheet').is(":checked"));
                // add attribute 

                $('#ctl00_primaryHolder_gv_PickSheet input:checkbox').attr("checked", function () {
                    this.checked = ischecked;
                });
            });

            // selected Pack List sheet boxes.
            $('.chkbxHeaderPackingList').click(function () {
                var ischecked = ($('#ctl00_primaryHolder_gv_PackingList_ctl01_chkbxHeaderPackingList').is(":checked"));
                // add attribute 

                $('#ctl00_primaryHolder_gv_PackingList input:checkbox').attr("checked", function () {
                    this.checked = ischecked;
                });
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

        function setButtonStatus(sender) {
            Page_ClientValidate();
            if (!Page_IsValid)
                $('#btnOrderSearch').prop('disabled', true);
            else
                $('#btnOrderSearch').prop('disabled', false);

        }

    </script>
</asp:Content>


<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>

   <asp:UpdateProgress id="ShippingQueueUpdateProgress" runat="server" DynamicLayout="true" DisplayAfter="0" >
        <ProgressTemplate>
            <%--<div style="width:100%" align="center">
                <img src="../images/barloader.gif" />
            </div>--%>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <act:ModalPopupExtender ID="modalExtender" runat="server" TargetControlID="ShippingQueueUpdateProgress"
        PopupControlID="Panel1" BackgroundCssClass="modalBackground" Enabled="true" >
    </act:ModalPopupExtender>

    <asp:Panel ID="Panel1" runat="server">
        <div style="width: 100%" align="center">
            <img src="../images/orangebarloader.gif" />
        </div>
    </asp:Panel>
   

   <div id="tabsContainer" >
        <ul>
			<li><a href="#PickingSheet_tab" id="PickingSheetTabHeader" runat ="server">Picking Sheets</a></li>
			<li><a href="#PackingList_tab" id="PackingListTabHeader" runat ="server">Packing Lists</a></li>
			<li><a href="#Instruction_tab" id="InstructionTabHeader" runat ="server">Shipping List & Instructions</a></li>
			<li><a href="#History_tab" id="HistoryTabHeader" runat ="server">Shipping History</a></li>			
		</ul>

        <div id="PickingSheet_tab">            
<%--VALIDATION SUMMARY (OF FORM ERRORS)--%>
    <div>
        <asp:ValidationSummary ID="ValidationSummary1" runat="server"
        DisplayMode ="BulletList" 
        EnableClientScript="true"
        ValidationGroup="PickingSheetValGroup"
        ShowSummary="true" CssClass="FormError" />          
    </div>
    <%--END--%>
			<asp:UpdatePanel ID="udpnPickingSheet" runat="server" UpdateMode="Conditional">
                <Triggers>
                    <asp:PostBackTrigger controlid="btn_ExportSelectedPickSheets" />
				    <asp:PostBackTrigger controlid="btn_ExportAllPickSheets"/>
				</Triggers>
				<ContentTemplate>                    
					<%--Search Bar--%>
			        <asp:Panel ID="pnOrderSearch" runat="server" CssClass="OToolbar" DefaultButton="btnOrderSearch">
				        <ul>
                            <li>
                                <label>Order #:</label>
                                <asp:TextBox ID="txtOrderFilterOrderNumber" runat="server" ClientIDMode="Static" CssClass="Size XSmall"></asp:TextBox>
                            </li>
					        <li>
						        <label>Account:</label>
						        <asp:TextBox ID="txtOrderFilterAccount" runat="server" ClientIDMode="Static" CssClass="Size XSmall"></asp:TextBox>
					        </li>
					        <li>
						        <label>Type:</label>
						        <asp:DropDownList ID="ddlOrderFilterType" runat="server" ClientIDMode="Static">
							        <asp:ListItem Value="" Text="ALL" />
							        <asp:ListItem Value="4" Text="New" />
							        <asp:ListItem Value="3" Text="Addon" />
							        <asp:ListItem Value="1" Text="Recall Replacement" />
							        <asp:ListItem Value="2" Text="Lost Replacement" />
						        </asp:DropDownList>
					        </li>
					        <li>
						        <label>Order Date:</label>
						        <asp:TextBox ID="txtOrderFilterDateRangeFrom" runat="server" ClientIDMode="Static" CssClass="date-picker Size XSmall"></asp:TextBox>
							        - 
						        <asp:TextBox ID="txtOrderFilterDateRangeTo" runat="server" ClientIDMode="Static" CssClass="date-picker Size XSmall" onkeyup="setButtonStatus(this);"></asp:TextBox>
                                <asp:CompareValidator ID="compvalOrderFilterDates" runat="server" ControlToCompare="txtOrderFilterDateRangeFrom" ControlToValidate="txtOrderFilterDateRangeTo" CssClass="InlineError" 
                                Type="Date" Operator="GreaterThanEqual" Display="None" ErrorMessage="Invalid Order Date Range" ></asp:CompareValidator>
					        </li>
                            <li>
						        <asp:Button ID="btnOrderSearch" runat="server" Text="Search" CssClass="btn btn-small btn-success" ClientIDMode="Static" OnClick="btnOrderSearch_Click" />
					        </li>
				        </ul>
                    </asp:Panel>
			        <%--End Search Bar--%>	
					<div class="OToolbar JoinTable" id="PickingSheetToolBar" runat ="server">
						<ul>
							<li>                                    
								<asp:LinkButton ID="btn_refreshPickSheet" runat="server"   
						            CssClass="Icon Refresh" onclick="btn_refreshPickSheet_Click" >Refresh Page</asp:LinkButton>  

                                <asp:LinkButton ID="btnPrintPickSheets" runat="server"   
						            CssClass="Icon Print" onclick="btnPrintPickSheets_Click" >Print Selected Picking Sheets</asp:LinkButton>  				                                   
                                                                
                                <span style="font-weight:bold; padding:0; float:right" >
                                    <asp:Label ID="lbl_PrintPickSheet" runat="server" Visible="false"></asp:Label>
                                    <asp:Label ID="lbl_OpenPickSheet" runat="server"></asp:Label>                                                                        
                                </span>

                                <asp:LinkButton ID="btn_ExportSelectedPickSheets" runat="server"   
						            CssClass="Icon Print" onclick="btn_ExportSelectedPickSheets_Click" >Export Selected Picking Sheets</asp:LinkButton>  				                                   

                                <asp:LinkButton ID="btn_ExportAllPickSheets" runat="server"   
						            CssClass="Icon Print" onclick="btn_ExportAllPickSheets_Click" >Export All Picking Sheets</asp:LinkButton>  				                                   
                               
							</li>							
						</ul>
					</div>

                    <asp:GridView ID="gv_PickSheet" runat="server" AllowSorting="True" 
                            AutoGenerateColumns="False" CssClass="OTable" DataSourceID="SqlOpenPickSheet" DataKeyNames="OrderID" AllowPaging="True"  PageSize="20" OnPageIndexChanging="gv_PickSheet_PageIndexChanging" OnSorting="gv_PickSheet_Sorting">

                        <Columns>
                            <asp:TemplateField ItemStyle-CssClass="CenterAlign" HeaderStyle-Width ="50px">
                                <%--<HeaderTemplate>
                                    <asp:CheckBox runat="server" ID="chkbxHeaderPickSheet" ToolTip="Select All Orders" class="chkbxHeaderPickSheet" />
                                </HeaderTemplate>  --%>                                                      
                                <ItemTemplate>
                                    <asp:CheckBox id="chkbxSelectPickSheet" runat="server" class="chkbxRow" />
                                    <asp:HiddenField runat="server" ID="HidOrderID"  Value='<%# Eval("OrderID") %>' />
                                </ItemTemplate>                                                         
                            </asp:TemplateField>
                                                    
                            <asp:BoundField ItemStyle-CssClass="LeftAlign" DataField="OrderId" HeaderText="Order#" SortExpression="orderid"  ItemStyle-Font-Bold="true" HeaderStyle-Width ="80px" />
                            <asp:BoundField ItemStyle-CssClass="LeftAlign" DataField="Acct ID" HeaderText="Acc#" SortExpression="acct ID" HeaderStyle-Width ="80px"  />
                            <asp:BoundField DataField="ShippingMethodMAS" HeaderText="Ship Via" SortExpression="ShippingMethodMAS" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" HeaderStyle-Width ="200px" />
                            <asp:BoundField DataField="Create Dt" HeaderText="Ordered" SortExpression="create dt" DataFormatString="{0:d}" HeaderStyle-Width ="90px" />
                            <asp:BoundField DataField="orderType" HeaderText="Type" SortExpression="orderType" HeaderStyle-Width ="120px"  />                                                         
                            
                            <asp:TemplateField ItemStyle-Wrap="false" >
                                <HeaderTemplate>
                                    Qty
                                </HeaderTemplate>                                                       
                                <ItemTemplate>
                                    <asp:label id="lblShipProduct" runat="server" 
                                        Text = '<%# Eval("ShippingProduct") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>                         
                
                        </Columns>
                        <EmptyDataTemplate>
							<div class="NoData">
								There are currently no picking sheets.
							</div>
						</EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />
                    </asp:GridView> 

                    <%--Picking Sheet Summary--%>
                    <div class="OToolbar JoinTable" style="width: 440px;">
                        <span>Picking Sheet By Search</span>
                    </div>
                    <asp:Repeater ID="rptOrderSummaries" runat="server">
                        <HeaderTemplate>
                            <table id="tblOrderSummary" class="OTable" style="width: 442px;">
                                <thead>
					                <tr>
						                <th style="width: 32%;">Type</th>
						                <th># of Orders</th>
						                <th>ID</th>
                                        <th>ID+</th>
                                        <th>ID2</th>
                                        <th>Instalink USB</th>
                                        <th>InstaLink</th>
                                        <th>ID3</th>
                                        <th>Vue Beta</th>
					                </tr>
				                </thead>
                                <tbody>
                        </HeaderTemplate>

                        <ItemTemplate>
                            <tr>
                                <td style="text-align: left;"><asp:Literal ID="ltOrderTypeName" runat="server" Text='<%# Eval("OrderTypeName") %>' /></td>
                                <td><asp:Literal ID="ltOrderCount" runat="server" Text='<%# Eval("OrderCount") %>' /></td>
                                <td><asp:Literal ID="ltIDCount" runat="server" Text='<%# Eval("IDCount") %>' /></td>
                                <td><asp:Literal ID="ltIDPlusCount" runat="server" Text='<%# Eval("IDPlusCount") %>' /></td>
                                <td><asp:Literal ID="ltID2Count" runat="server" Text='<%# Eval("ID2Count") %>' /></td>
                                <td><asp:Literal ID="ltInstaUSBCount" runat="server" Text='<%# Eval("InstaLinkUSBCount") %>' /></td>
                                <td><asp:Literal ID="ltInstaLinkCount" runat="server" Text='<%# Eval("InstaLinkCount") %>' /></td>
                                <td><asp:Literal ID="ltID3Count" runat="server" Text='<%# Eval("ID3Count") %>' /></td>
                                <td><asp:Literal ID="ltVueBetaCount" runat="server" Text='<%# Eval("VueBetaCount") %>' /></td>
                            </tr>
                        </ItemTemplate>

                        <AlternatingItemTemplate>
                            <tr class="Alt">
                                <td style="text-align: left;"><asp:Literal ID="ltOrderTypeName" runat="server" Text='<%# Eval("OrderTypeName") %>' /></td>
                                <td><asp:Literal ID="ltOrderCount" runat="server" Text='<%# Eval("OrderCount") %>' /></td>
                                <td><asp:Literal ID="ltIDCount" runat="server" Text='<%# Eval("IDCount") %>' /></td>
                                <td><asp:Literal ID="ltIDPlusCount" runat="server" Text='<%# Eval("IDPlusCount") %>' /></td>
                                <td><asp:Literal ID="ltID2Count" runat="server" Text='<%# Eval("ID2Count") %>' /></td>
                                <td><asp:Literal ID="ltInstaUSBCount" runat="server" Text='<%# Eval("InstaLinkUSBCount") %>' /></td>
                                <td><asp:Literal ID="ltInstaLinkCount" runat="server" Text='<%# Eval("InstaLinkCount") %>' /></td>
                                <td><asp:Literal ID="ltID3Count" runat="server" Text='<%# Eval("ID3Count") %>' /></td>
                                <td><asp:Literal ID="ltVueBetaCount" runat="server" Text='<%# Eval("VueBetaCount") %>' /></td>
                            </tr>
                        </AlternatingItemTemplate>

                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <%--Picking Sheet Summary--%>
									  
<%--					<asp:GridView ID="gv_PickSheet" runat="server" AllowSorting="True" 
                            AutoGenerateColumns="False" CssClass="OTable" DataSourceID="SqlOpenPickSheet" DataKeyNames="OrderID" >

                        <Columns>
                            <asp:TemplateField ItemStyle-CssClass="CenterAlign" HeaderStyle-Width ="50px">
                                <HeaderTemplate>
                                    <asp:CheckBox runat="server" ID="chkbxHeaderPickSheet" ToolTip="Select All Orders" class="chkbxHeaderPickSheet" />
                                </HeaderTemplate>                                                        
                                <ItemTemplate>
                                    <asp:CheckBox id="chkbxSelectPickSheet" runat="server" class="chkbxRow" />
                                    <asp:HiddenField runat="server" ID="HidOrderID"  Value='<%# Eval("OrderID") %>' />
                                </ItemTemplate>                                                         
                            </asp:TemplateField>
                                                    
                            <asp:BoundField ItemStyle-CssClass="LeftAlign" DataField="OrderId" HeaderText="Order#" SortExpression="orderid"  ItemStyle-Font-Bold="true" HeaderStyle-Width ="80px" />
                            <asp:BoundField ItemStyle-CssClass="LeftAlign" DataField="Acct ID" HeaderText="Acc#" SortExpression="acct ID" HeaderStyle-Width ="80px"  />
                            <asp:BoundField DataField="ShippingMethodMAS" HeaderText="Ship Via" SortExpression="ShippingMethodMAS" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" HeaderStyle-Width ="200px" />
                            <asp:BoundField DataField="Create Dt" HeaderText="Ordered" SortExpression="create dt" DataFormatString="{0:d}" HeaderStyle-Width ="90px" />
                            <asp:BoundField DataField="orderType" HeaderText="Type" SortExpression="orderType" HeaderStyle-Width ="120px"  />

                            <asp:BoundField DataField="Amount" HeaderText="Amount"  Visible="false" SortExpression="amount" />

                            <asp:TemplateField >
                                    <HeaderTemplate>
                                        Billing Info
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                    <asp:TextBox runat="server" ID="txtBillingInfo" TextMode="MultiLine" ReadOnly="true"
                                            Width="180" Text='<%# GenerateBillingInfo(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>'></asp:TextBox>
                                    </ItemTemplate>
                            </asp:TemplateField>
                        
                            <asp:TemplateField>
                                    <HeaderTemplate>
                                    Shipping Info
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                    <asp:TextBox runat="server" ID="txtShippingInfo" TextMode="MultiLine" ReadOnly="true"
                                            Width="180" BackColor="LightYellow" 
                                            Text='<%# GenerateShippingInfo(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>'></asp:TextBox>
                                    </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField ItemStyle-Wrap="false" >
                                <HeaderTemplate>
                                    Qty
                                </HeaderTemplate>                                                       
                                <ItemTemplate>
                                    <asp:label id="lblShipProduct" runat="server" 
                                        Text = '<%# GenerateShipProduct(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                
                        </Columns>
                        <EmptyDataTemplate>
							<div class="NoData">
								There are currently no picking sheets.
							</div>
						</EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />
                    </asp:GridView> --%>

				</ContentTemplate>
			</asp:UpdatePanel>
											   
		</div>
       
        <div id="PackingList_tab">

			<asp:UpdatePanel ID="udpnPackingList" runat="server" UpdateMode="Conditional">
                <Triggers>
					<asp:AsyncPostBackTrigger controlid="btnPrintPickSheets" eventname ="Click" />
				</Triggers>

				<ContentTemplate>
						
					<div class="OToolbar JoinTable" id="PackingListToolBar" runat ="server">
						<ul>
							<li>                                    
								<asp:LinkButton ID="btn_refreshPackingList" runat="server"   
						            CssClass="Icon Refresh" onclick="btn_refreshPackingList_Click" >Refresh Page</asp:LinkButton>  

                                <asp:LinkButton ID="btnPrintPackingList" runat="server"   
						            CssClass="Icon Print" onclick="btnPrintPackingList_Click" >Print Selected Packing Lists</asp:LinkButton>  

                                <asp:Label ID="Label5" runat="server" Text="Reprint Picking Sheets for Order#:" />
                                
                                <asp:TextBox runat="server" id="txtReprintPickingSheet" text="" Height="15px"></asp:TextBox>

                                <asp:LinkButton ID="btnReprintPickingSheet" runat="server"   
						            CssClass="Icon Print" onclick="btnReprintPickingSheet_Click" >Print</asp:LinkButton>                                                  
				                                    
                                <span style="font-weight:bold; padding:0; float:right" >
                                    <asp:Label ID="lbl_PrintPackingList" runat="server" Visible="false"></asp:Label>
                                </span>
							</li>
							
						</ul>
					</div>

                    <asp:GridView ID="gv_PackingList" runat="server" AllowSorting="True" AutoGenerateColumns="False" 
                        CssClass="OTable" DataSourceID="sqlOpenPackingList" DataKeyNames="OrderID">
                        <Columns>

                            <asp:TemplateField ItemStyle-CssClass="mt-itm centeralign">
                                <HeaderTemplate>                                    
                                    <asp:CheckBox runat="server" ID="chkbxHeaderPackingList" ToolTip="Select ALL Orders" class="chkbxHeaderPackingList" />
                                </HeaderTemplate>
                                                    
                                <ItemTemplate>
                                    <asp:CheckBox id="chkbxSelectPackingList" runat="server" class="chkbxRow" 
                                        Visible='<%# (Boolean.Parse(Eval("IsFulfilled").ToString())) %>' />
                                    <asp:HiddenField runat="server" ID="HidOrderID"  Value='<%# Eval("OrderID") %>' />
                                </ItemTemplate>                                                     
                            </asp:TemplateField>
                       
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="OrderId" HeaderText="Order#" SortExpression="orderid" ItemStyle-Font-Bold="true" />
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="Acct ID" HeaderText="Acc#" SortExpression="acct ID"  />
                            <asp:BoundField DataField="ShippingMethodMAS" HeaderText="Ship Via" SortExpression="ShippingMethodMAS" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" />
                            <asp:BoundField DataField="Create Dt" HeaderText="Ordered" SortExpression="create dt" DataFormatString="{0:d}" />
                            <asp:BoundField DataField="orderType" HeaderText="Type" SortExpression="orderType" ItemStyle-Wrap="false" />                           

                            <asp:TemplateField>
                                <HeaderTemplate>
                                    Billing Info
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox runat="server" ID="txtBillingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="180" Height="75" Text = '<%# ((string)Eval("BillingInfo") ?? string.Empty).Replace("\\r\\n", "\r\n") %>' ></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                        
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    Shipping Info
                                </HeaderTemplate>

                                <ItemTemplate>
                                    <asp:TextBox runat="server" ID="txtShippingInfo" TextMode="MultiLine" ReadOnly="true"
                                       Width="180" Height="75" BackColor="LightYellow" 
                                       Text = '<%# ((string)Eval("ShippingInfo") ?? string.Empty).Replace("\\r\\n", "\r\n") %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="70px" ItemStyle-Wrap="false">
                                <HeaderTemplate>
                                    Qty
                                </HeaderTemplate>
                                                           
                                <ItemTemplate>
                                    <asp:label id="lblShipProduct" runat="server" Text = '<%# Eval("ShippingProduct") %>' />
                                </ItemTemplate>

                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="100px">
                                <HeaderTemplate>
                                    Serial#
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtSerialno" TextMode="MultiLine" ReadOnly="true"
                                      Width="97" Height="75" BackColor="LightYellow" 
                                      Text = '<%# ((string)Eval("ScanedSerialNo") ?? string.Empty).Replace("\\r\\n", "\r\n") %>' ></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                
                        </Columns>
                        <EmptyDataTemplate>
							<div class="NoData">
								There are currently no packing lists.
							</div>
						</EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />
                    </asp:GridView> 
				
<%--					<asp:GridView ID="gv_PackingList" runat="server" AllowSorting="True" AutoGenerateColumns="False" 
                        CssClass="OTable" DataSourceID="sqlOpenPackingList" DataKeyNames="OrderID">
                        <Columns>

                            <asp:TemplateField ItemStyle-CssClass="mt-itm centeralign">
                                <HeaderTemplate>
                                    
                                    <asp:CheckBox runat="server" ID="chkbxHeaderPackingList" ToolTip="Select ALL Orders" class="chkbxHeaderPackingList" />
                                </HeaderTemplate>
                                                    
                                <ItemTemplate>
                                    <asp:CheckBox id="chkbxSelectPackingList" runat="server" class="chkbxRow" 
                                        Visible='<%#DisplayPackingListCheckBox(DataBinder.Eval(Container.DataItem,"OrderID","" ))%>' />
                                    <asp:HiddenField runat="server" ID="HidOrderID"  Value='<%# Eval("OrderID") %>' />
                                </ItemTemplate>                                                     
                            </asp:TemplateField>
                       
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="OrderId" HeaderText="Order#" SortExpression="orderid" ItemStyle-Font-Bold="true" />
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="Acct ID" HeaderText="Acc#" SortExpression="acct ID"  />
                            <asp:BoundField DataField="ShippingMethodMAS" HeaderText="Ship Via" SortExpression="ShippingMethodMAS" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" />
                            <asp:BoundField DataField="Create Dt" HeaderText="Ordered" SortExpression="create dt" DataFormatString="{0:d}" />
                            <asp:BoundField DataField="orderType" HeaderText="Type" SortExpression="orderType" ItemStyle-Wrap="false" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount"  Visible="false" SortExpression="amount" />

                            <asp:TemplateField>
                                <HeaderTemplate>
                                    Billing Info
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox runat="server" ID="txtBillingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="180" Height="75" Text='<%# GenerateBillingInfo(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                        
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    Shipping Info
                                </HeaderTemplate>

                                <ItemTemplate>
                                    <asp:TextBox runat="server" ID="txtShippingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="180" Height="75" BackColor="LightYellow" 
                                        Text='<%# GenerateShippingInfo(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="70px" ItemStyle-Wrap="false">
                                <HeaderTemplate>
                                    Qty
                                </HeaderTemplate>
                                                           
                                <ItemTemplate>
                                    <asp:label id="lblShipProduct" runat="server" 
                                    Text = '<%# GenerateShipProduct(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>' />
                                </ItemTemplate>

                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="100px">
                                <HeaderTemplate>
                                    Serial#
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtSerialno" TextMode="MultiLine" ReadOnly="true"
                                      Width="97" Height="75" BackColor="LightYellow" 
                                        Text='<%# GenerateScanedSerialNo(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                
                        </Columns>
                        <EmptyDataTemplate>
							<div class="NoData">
								There are currently no packing lists.
							</div>
						</EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />
                    </asp:GridView> 	--%>				

				</ContentTemplate>
			</asp:UpdatePanel>
											   
		</div>

        <div id="Instruction_tab">

			<asp:UpdatePanel ID="udpnInstruction" runat="server" UpdateMode="Conditional">
                <Triggers>
					<asp:AsyncPostBackTrigger controlid="btnPrintPackingList" eventname="Click" />
				</Triggers>

				<ContentTemplate>
						
					<div class="OToolbar JoinTable" id="InstructionToolBar" runat ="server">
						<ul>
							<li>                                    
								<asp:LinkButton ID="btn_refreshShippingInstruction" runat="server"   
						            CssClass="Icon Refresh" onclick="btn_refreshShippingInstruction_Click" >Refresh Page</asp:LinkButton>  
                                
                                <asp:Label ID="Label6" runat="server" Text="Reprint Packing List for Package#:" />
                                
                                <asp:TextBox runat="server" id="txtReprintPacking" text="" Height="15px"></asp:TextBox>

                                <asp:LinkButton ID="btnRePrintPackingList" runat="server"   
						            CssClass="Icon Print" onclick="btnRePrintPackingList_Click" >Print</asp:LinkButton>  
                                    
                                <span style="font-weight:bold; padding:0; float:right" >
                                    <asp:Label ID="lbl_RePrintPackingList" runat="server" Visible="false"></asp:Label>
                                </span>                                                
				                                    
							</li>
							
						</ul>
					</div>

                    <asp:GridView ID="gv_ShippingList" runat="server" AllowSorting="True" AutoGenerateColumns="False" 
                        CssClass="OTable" DataSourceID="sqlOpenShippingList" DataKeyNames="OrderID">
                        <Columns>
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="Packageid" HeaderText="Package#" SortExpression="packageid" ItemStyle-Font-Size="Large" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" />
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="OrderId" HeaderText="Order#" SortExpression="orderid" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" />
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="Acct ID" HeaderText="Acc#" SortExpression="acct ID"  />
                            <asp:BoundField DataField="ShippingMethodMAS" HeaderText="Ship Via" SortExpression="ShippingMethodMAS" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" />
                            <asp:BoundField DataField="Create Dt" HeaderText="Ordered" SortExpression="create dt"  DataFormatString="{0:d}" HeaderStyle-Wrap="false"/>
                            <asp:BoundField DataField="orderType" HeaderText="Type" SortExpression="orderType"  />
                            
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    Billing Info
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtBillingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="180" Height="75" Text = '<%# ((string)Eval("BillingInfo") ?? string.Empty).Replace("\\r\\n", "\r\n") %>' ></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                        
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    Shipping Info
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox runat="server" ID="txtShippingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="180" Height="75" BackColor="LightYellow" 
                                        Text = '<%# ((string)Eval("PackedShippingInfo") ?? string.Empty).Replace("\\r\\n", "\r\n") %>' ></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="70px" ItemStyle-Wrap="false" >
                                <HeaderTemplate>
                                    Qty
                                </HeaderTemplate>                                                                                            
                                <ItemTemplate>
                                    <asp:label id="lblShipProduct" runat="server" Text = '<%# Eval("ShippingProduct") %>' />
                                </ItemTemplate>

                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="100px" >
                                <HeaderTemplate>
                                    Serial#
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtSerialno" TextMode="MultiLine" ReadOnly="true"
                                        Width="97" Height="75" BackColor="LightYellow" 
                                        Text = '<%# ((string)Eval("PackedSerialNo") ?? string.Empty).Replace("\\r\\n", "\r\n") %>' ></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                
                        </Columns>
                        <EmptyDataTemplate>
							<div class="NoData">
								There are currently no shipping lists and instructions.
							</div>
						</EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />

                    </asp:GridView>
				
<%--					<asp:GridView ID="gv_ShippingList" runat="server" AllowSorting="True" AutoGenerateColumns="False" 
                        CssClass="OTable" DataSourceID="sqlOpenShippingList" DataKeyNames="OrderID">
                        <Columns>
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="Packageid" HeaderText="Package#" SortExpression="packageid" ItemStyle-Font-Size="Large" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" />
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="OrderId" HeaderText="Order#" SortExpression="orderid" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" />
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="Acct ID" HeaderText="Acc#" SortExpression="acct ID"  />
                            <asp:BoundField DataField="ShippingMethodMAS" HeaderText="Ship Via" SortExpression="ShippingMethodMAS" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" />
                            <asp:BoundField DataField="Create Dt" HeaderText="Ordered" SortExpression="create dt"  DataFormatString="{0:d}" HeaderStyle-Wrap="false"/>
                            <asp:BoundField DataField="orderType" HeaderText="Type" SortExpression="orderType"  />
                            <asp:BoundField DataField="Amount" HeaderText="Amount"  Visible="false" SortExpression="amount" />
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    Billing Info
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtBillingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="180" Height="75" Text='<%# GenerateBillingInfo(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                        
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    Shipping Info
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox runat="server" ID="txtShippingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="180" Height="75" BackColor="LightYellow" 
                                        Text='<%# GeneratePackShippingInfo(DataBinder.Eval(Container.DataItem,"Packageid","" )) %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="70px" ItemStyle-Wrap="false" >
                                <HeaderTemplate>
                                    Qty
                                </HeaderTemplate>                                                            
                                <ItemTemplate>
                                    <asp:label id="lblShipProduct" runat="server" 
                                        Text = '<%# GenerateShipProduct(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="100px" >
                                <HeaderTemplate>
                                    Serial#
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtSerialno" TextMode="MultiLine" ReadOnly="true"
                                        Width="97" Height="75" BackColor="LightYellow" 
                                        Text='<%# GeneratePackSerialNo(DataBinder.Eval(Container.DataItem,"Packageid","" )) %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                
                        </Columns>
                        <EmptyDataTemplate>
							<div class="NoData">
								There are currently no shipping lists and instructions.
							</div>
						</EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />

                    </asp:GridView>	--%>
                    
                    <asp:Label ID="lblShipEndUser" runat="server" Font-Bold="true" Font-Size="Large"
                         text="Individual User Shipments"></asp:Label>	
                         
                    <asp:GridView ID="gv_openshipment" runat="server" AllowSorting="True" 
                        AutoGenerateColumns="False" DataSourceID="SqlOpenShipment" CssClass ="OTable">
                        <Columns>
                            <asp:BoundField DataField="UserShipAssignID" HeaderText="Ref#" ItemStyle-CssClass="RightAlign" SortExpression="UserShipAssignID" /> 
                            <asp:BoundField DataField="PackageID" HeaderText="Package#" ItemStyle-Font-Size="Large" ItemStyle-Font-Bold="true" SortExpression="PackageID" ItemStyle-CssClass="RightAlign" />                                                                                                                            
                            <asp:BoundField DataField="Serialno" HeaderText="Serial#" ItemStyle-CssClass="RightAlign" SortExpression="serialno" ItemStyle-Font-Bold="true" />
                            <asp:BoundField DataField="accountid" HeaderText="Acc#" ItemStyle-CssClass="RightAlign" SortExpression="accountid" />
                            <asp:BoundField DataField="companyname" HeaderText="Company" SortExpression="companyname" /> 
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:BoundField DataField="Address1" HeaderText="Address1" SortExpression="Address1" />
                            <asp:BoundField DataField="Address2" HeaderText="Address2" SortExpression="Address2" />
                            <asp:BoundField DataField="City" HeaderText="City" SortExpression="City" />
                            <asp:BoundField DataField="State" HeaderText="State" SortExpression="State" />
                            <asp:BoundField DataField="PostalCode" HeaderText="Zip" ItemStyle-CssClass="RightAlign" SortExpression="PostalCode" />
                            <asp:BoundField DataField="Country" HeaderText="Country" SortExpression="Country" />
                        </Columns>

                        <EmptyDataTemplate>
							<div class="NoData">
								There are currently no end user shipments.
							</div>
						</EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />
                                              
                    </asp:GridView>			

				</ContentTemplate>
			</asp:UpdatePanel>
											   
		</div>

         <div id="History_tab">

			<asp:UpdatePanel ID="udpnHistory" runat="server" UpdateMode="Conditional">
				<ContentTemplate>
						
					<div class="OToolbar JoinTable" id="HistoryToolBar" runat ="server">
						<ul>
							<li>  
                                <asp:Label ID="Label3" runat="server" Text="Ship Date From:"></asp:Label>  
                                
                                <asp:TextBox ID="txt_Startdate" runat="server" Width="85px" Height="15px"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
                                    ControlToValidate="txt_Startdate" Display="Dynamic" 
                                    ErrorMessage="*Date Required " Font-Bold="True" ValidationGroup="Valid_form2"></asp:RequiredFieldValidator>                                

                                <asp:Label ID="Label4" runat="server" Text="To:"></asp:Label>

                                <asp:TextBox ID="txt_Enddate" runat="server" Width="85px" Height="15px"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" 
                                    ControlToValidate="txt_Enddate" Display="Dynamic" 
                                    ErrorMessage="*Date Required " Font-Bold="True" ValidationGroup="Valid_form2"></asp:RequiredFieldValidator>

								<asp:Button ID="btnFindRecordDate" runat="server" Text="Search" 
                                     ValidationGroup="Valid_form2" CssClass = "OButton" OnClick="btn_btnFindRecordDate_Click"/>
				                                    
                                <span style="font-weight:bold; padding:0; float:right" >
                                    <asp:Label ID="lblShippingHistoryResult" runat="server" Text=""></asp:Label>
                                </span>
							</li>
							
						</ul>
					</div>

                    <asp:GridView ID="gv_ShippingHistory" runat="server" AllowSorting="True" 
                        AutoGenerateColumns="False" CssClass="OTable" DataSourceID="sqlShippingHistory" DataKeyNames="OrderID" AllowPaging="true" PageSize="20">
                        <Columns>
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="OrderId" HeaderText="Order#" SortExpression="orderid" ItemStyle-Font-Size="Larger" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" />
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="Acct ID" HeaderText="Acc#" SortExpression="acct ID" ItemStyle-Font-Bold="true"  />
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="Packageid" HeaderText="Package#" SortExpression="packageid"  ItemStyle-Wrap="false" />
                            <asp:BoundField DataField="Create Dt" HeaderText="Order Date" SortExpression="create dt" HeaderStyle-Wrap="false" />
                            <asp:BoundField DataField="orderType" HeaderText="Type" SortExpression="orderType"  />
                            <asp:BoundField DataField="ShipDate" HeaderText="Ship Date" SortExpression="shipdate" ItemStyle-Font-Bold="true" HeaderStyle-Wrap="false" />
                            <asp:TemplateField HeaderText="ShipVia & Tracking#" SortExpression ="ShippingMethodMAS">
                                <ItemTemplate>
                                    <asp:Label ID="lblShipVia" runat="server" Text='<%# Eval("ShippingMethodMAS")%>'> </asp:Label>
                                    <asp:Hyperlink ID="Hyperlink3b"  runat="server" Text='<%# Eval("TrackingNumber") %>' 
                                    NavigateUrl='<%# Eval("TrackingNumber", "http://fedex.com/Tracking?ascend_header=1&clienttype=dotcom&cntry_code=us&language=english&tracknumbers={0}") %>' 
                                    Target="_blank"  />
                                </ItemTemplate>
                            </asp:TemplateField>                                                        

                            <asp:TemplateField>
                                <HeaderTemplate>
                                    Billing Address
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtBillingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="170" Height="75"  
                                        Text = '<%# ((string)Eval("BillingInfo") ?? string.Empty).Replace("\\r\\n", "\r\n") %>' ></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                        
                            <asp:TemplateField >
                                <HeaderTemplate>
                                    Shipping Address
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtShippingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="170" Height="75"   BackColor="LightYellow" 
                                        Text = '<%# ((string)Eval("PackedShippingInfo") ?? string.Empty).Replace("\\r\\n", "\r\n") %>' ></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="70px" ItemStyle-Wrap="false">
                                <HeaderTemplate>
                                    Qty
                                </HeaderTemplate>
                                                            
                                <ItemTemplate>
                                    <asp:label id="lblShipProduct" runat="server" Text = '<%# Eval("ShippingProduct") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="95px">
                                <HeaderTemplate>
                                    Serial#
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtSerialno" TextMode="MultiLine" ReadOnly="true"
                                        Width="92" Height="75"    BackColor="LightYellow" 
                                        Text = '<%# ((string)Eval("PackedSerialNo") ?? string.Empty).Replace("\\r\\n", "\r\n") %>' ></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                
                        </Columns>
                            <EmptyDataTemplate>
							<div class="NoData">
								No shipping records found.
							</div>
						</EmptyDataTemplate>                        
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />
                        <RowStyle Font-Size="Smaller"   />
                        <HeaderStyle Font-Bold="true" Font-Size="Small" />
                    </asp:GridView>
				
<%--					<asp:GridView ID="gv_ShippingHistory" runat="server" AllowSorting="True" 
                        AutoGenerateColumns="False" CssClass="OTable" DataSourceID="sqlShippingHistory" DataKeyNames="OrderID">
                        <Columns>
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="OrderId" HeaderText="Order#" SortExpression="orderid" ItemStyle-Font-Size="Larger" ItemStyle-Font-Bold="true" ItemStyle-Wrap="false" />
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="Acct ID" HeaderText="Acc#" SortExpression="acct ID" ItemStyle-Font-Bold="true"  />
                            <asp:BoundField ItemStyle-CssClass="RightAlign" DataField="Packageid" HeaderText="Package#" SortExpression="packageid"  ItemStyle-Wrap="false" />
                            <asp:BoundField DataField="Create Dt" HeaderText="Order Date" SortExpression="create dt" HeaderStyle-Wrap="false" />
                            <asp:BoundField DataField="orderType" HeaderText="Type" SortExpression="orderType"  />
                            <asp:BoundField DataField="ShipDate" HeaderText="Ship Date" SortExpression="shipdate" ItemStyle-Font-Bold="true" HeaderStyle-Wrap="false" />
                            <asp:TemplateField HeaderText="ShipVia & Tracking#" SortExpression ="ShippingMethodMAS">
                                <ItemTemplate>
                                    <asp:Label ID="lblShipVia" runat="server" Text='<%# Eval("ShippingMethodMAS")%>'> </asp:Label>
                                    <asp:Hyperlink ID="Hyperlink3b"  runat="server" Text='<%# Eval("TrackingNumber") %>' 
                                    NavigateUrl='<%# Eval("TrackingNumber", "http://fedex.com/Tracking?ascend_header=1&clienttype=dotcom&cntry_code=us&language=english&tracknumbers={0}") %>' 
                                    Target="_blank"  />
                                </ItemTemplate>
                            </asp:TemplateField>
                            
                            <asp:BoundField DataField="Amount" HeaderText="Amount"  Visible="false" SortExpression="amount" />

                            <asp:TemplateField>
                                <HeaderTemplate>
                                    Billing Address
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtBillingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="170" Height="75"  
                                        Text='<%# GenerateBillingInfo(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                        
                            <asp:TemplateField >
                                <HeaderTemplate>
                                    Shipping Address
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtShippingInfo" TextMode="MultiLine" ReadOnly="true"
                                        Width="170" Height="75"   BackColor="LightYellow" 
                                        Text='<%# GeneratePackShippingInfo(DataBinder.Eval(Container.DataItem,"Packageid","" )) %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="70px" ItemStyle-Wrap="false">
                                <HeaderTemplate>
                                    Qty
                                </HeaderTemplate>
                                                            
                                <ItemTemplate>
                                    <asp:label id="lblShipProduct" runat="server" 
                                        Text = '<%# GenerateShipProduct(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderStyle-Width="95px">
                                <HeaderTemplate>
                                    Serial#
                                </HeaderTemplate>
                                <ItemTemplate>
                                <asp:TextBox runat="server" ID="txtSerialno" TextMode="MultiLine" ReadOnly="true"
                                        Width="92" Height="75"    BackColor="LightYellow" 
                                        Text='<%# GeneratePackSerialNo(DataBinder.Eval(Container.DataItem,"Packageid","" )) %>'></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>

                
                        </Columns>
                            <EmptyDataTemplate>
							<div class="NoData">
								No shipping records found.
							</div>
						</EmptyDataTemplate>                        
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />
                        <RowStyle Font-Size="Smaller"   />
                        <HeaderStyle Font-Bold="true" Font-Size="Small" />
                    </asp:GridView>	--%>				

				</ContentTemplate>
			</asp:UpdatePanel>
											   
		</div>

   </div>


    <asp:SqlDataSource ID="SqlOpenPickSheet" runat="server" 
            ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>" 
            SelectCommand="sp_GetPickingSheet" 
            SelectCommandType="StoredProcedure" EnableCaching="true">
    </asp:SqlDataSource>
    <%--<asp:SqlDataSource ID="SqlOpenPickSheet" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="select ord.OrderStatusID , ord.OrderType, ord.OrderID, ISNULL(ord.POnumber,'')  as PoNumber,
        CONVERT (varchar(8), ord.OrderDate, 1) AS [Create Dt], ord.Accountid AS [Acct ID],
        Locations.BillingCompany, Locations.ShippingCompany,         
        ord.ReferralCode as referral,
        ShippingMethods.ShippingMethodMAS 
        from orders ord 
        inner join Accounts acct on ord.AccountID = acct.AccountID         
        inner join Locations on ord.locationID = Locations.LocationID
        inner join ShippingMethods  on ord.ShippingMethodID = ShippingMethods.ShippingMethodID 
        WHERE (ord.OrderStatusID =3 And ord.OrderType <> 'RENEWAL')  ORDER BY ord.OrderDate DESC" />--%>
<%--    <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="select ord.OrderStatusID , ord.OrderType, ord.OrderID, ISNULL(ord.POnumber,'')  as PoNumber,
        CONVERT (varchar(8), ord.OrderDate, 1) AS [Create Dt], ord.Accountid AS [Acct ID],
        Locations.BillingCompany, Locations.ShippingCompany, 
        case Payments.PaymentMethodID when 1 then 'CC' ELSE 'PO' END AS [Pmt Type], 
        dbo.fn_formatcurrency(ISNULL((SELECT SUM(price * quantity) AS Amount FROM OrderDetails 
            WHERE (Orderid = ord.OrderId)), 0)  + ISNULL(ord.ShippingCharge, 0), ord.currencycode) AS Amount, 
        ord.ReferralCode as referral,
        ShippingMethods.ShippingMethodMAS 
        from orders ord 
        inner join Accounts acct on ord.AccountID = acct.AccountID 
        left join Payments  on ord.orderid = payments.orderID
        inner join Locations on ord.locationID = Locations.LocationID
        inner join ShippingMethods  on ord.ShippingMethodID = ShippingMethods.ShippingMethodID 
        WHERE (ord.OrderStatusID =3 And ord.OrderType <> 'RENEWAL')  ORDER BY ord.OrderDate DESC" />--%>

    <asp:SqlDataSource ID="sqlOpenPackingList" runat="server" 
            ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>" 
            SelectCommand="sp_GetPackingList" 
            SelectCommandType="StoredProcedure">
        
    </asp:SqlDataSource>
    <%--<asp:SqlDataSource ID="sqlOpenPackingList" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="select ord.OrderStatusID , ord.OrderType, ord.OrderID, ISNULL(ord.POnumber,'')  as PoNumber,
        CONVERT (varchar(8), ord.OrderDate, 1) AS [Create Dt], ord.Accountid AS [Acct ID],
        Locations.BillingCompany, Locations.ShippingCompany,         
        ord.ReferralCode as referral,
        ShippingMethods.ShippingMethodMAS 
        from orders ord 
        inner join Accounts acct on ord.AccountID = acct.AccountID         
        inner join Locations on ord.locationID = Locations.LocationID
        inner join ShippingMethods  on ord.ShippingMethodID = ShippingMethods.ShippingMethodID 
        WHERE (ord.OrderStatusID =6 And ord.OrderType <> 'RENEWAL' )  ORDER BY ord.OrderDate DESC" />--%>
<%--    <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="select ord.OrderStatusID , ord.OrderType, ord.OrderID, ISNULL(ord.POnumber,'')  as PoNumber,
        CONVERT (varchar(8), ord.OrderDate, 1) AS [Create Dt], ord.Accountid AS [Acct ID],
        Locations.BillingCompany, Locations.ShippingCompany, 
        case Payments.PaymentMethodID when 1 then 'CC' ELSE 'PO' END AS [Pmt Type], 
        dbo.fn_formatcurrency(ISNULL((SELECT SUM(price * quantity) AS Amount FROM OrderDetails 
            WHERE (Orderid = ord.OrderId)), 0)  + ISNULL(ord.ShippingCharge, 0), ord.currencycode) AS Amount, 
        ord.ReferralCode as referral,
        ShippingMethods.ShippingMethodMAS 
        from orders ord 
        inner join Accounts acct on ord.AccountID = acct.AccountID 
        left join Payments  on ord.orderid = payments.orderID
        inner join Locations on ord.locationID = Locations.LocationID
        inner join ShippingMethods  on ord.ShippingMethodID = ShippingMethods.ShippingMethodID 
        WHERE (ord.OrderStatusID =6 And ord.OrderType <> 'RENEWAL' )  ORDER BY ord.OrderDate DESC" />--%>

    <asp:SqlDataSource ID="sqlOpenShippingList" runat="server" 
            ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>" 
            SelectCommand="sp_GetOpenShippingList" 
            SelectCommandType="StoredProcedure">
        
    </asp:SqlDataSource>
    <%--<asp:SqlDataSource ID="sqlOpenShippingList" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="select pack.PackageID, ord.OrderStatusID , ord.OrderType, ord.OrderID, ISNULL(ord.POnumber,'')  as PoNumber,
        CONVERT (varchar(8), ord.OrderDate, 1) AS [Create Dt], ord.Accountid AS [Acct ID],
        Locations.BillingCompany, Locations.ShippingCompany,         
        ord.ReferralCode as referral,
        ShippingMethods.ShippingMethodMAS 
        from packages pack 
        inner join orders ord  on pack.orderid = ord.orderid
        inner join Accounts acct on ord.AccountID = acct.AccountID         
        inner join Locations on ord.locationID = Locations.LocationID
        inner join ShippingMethods  on ord.ShippingMethodID = ShippingMethods.ShippingMethodID 
        WHERE (ord.OrderStatusID =7 And ord.OrderType <> 'RENEWAL' )  ORDER BY ord.OrderDate DESC" /> --%>
<%--    <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="select pack.PackageID, ord.OrderStatusID , ord.OrderType, ord.OrderID, ISNULL(ord.POnumber,'')  as PoNumber,
        CONVERT (varchar(8), ord.OrderDate, 1) AS [Create Dt], ord.Accountid AS [Acct ID],
        Locations.BillingCompany, Locations.ShippingCompany, 
        case Payments.PaymentMethodID when 1 then 'CC' ELSE 'PO' END AS [Pmt Type], 
        dbo.fn_formatcurrency(ISNULL((SELECT SUM(price * quantity) AS Amount FROM OrderDetails 
            WHERE (Orderid = ord.OrderId)), 0)  + ISNULL(ord.ShippingCharge, 0), ord.currencycode) AS Amount, 
        ord.ReferralCode as referral,
        ShippingMethods.ShippingMethodMAS 
        from packages pack 
        inner join orders ord  on pack.orderid = ord.orderid
        inner join Accounts acct on ord.AccountID = acct.AccountID 
        left join Payments  on ord.orderid = payments.orderID
        inner join Locations on ord.locationID = Locations.LocationID
        inner join ShippingMethods  on ord.ShippingMethodID = ShippingMethods.ShippingMethodID 
        WHERE (ord.OrderStatusID =7 And ord.OrderType <> 'RENEWAL' )  ORDER BY ord.OrderDate DESC" />--%> 

    <asp:SqlDataSource ID="SqlOpenShipment" runat="server" 
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


    <asp:SqlDataSource ID="sqlShippingHistory" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_GetShippingHistory" 
        SelectCommandType="StoredProcedure">
         
        <SelectParameters>
            <asp:ControlParameter ControlID="txt_Startdate" DefaultValue=" " 
                Name="FromDate" PropertyName="Text" Type="String" />

            <asp:ControlParameter ControlID="txt_Enddate" DefaultValue=" " 
                Name="EndDate" PropertyName="Text" Type="String" />            
        </SelectParameters>

    </asp:SqlDataSource>

<%--    <asp:SqlDataSource ID="sqlShippingHistory" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="select pack.PackageID, ord.OrderStatusID , ord.OrderType, ord.OrderID, ISNULL(ord.POnumber,'')  as PoNumber,
        CONVERT (varchar(8), ord.OrderDate, 1) AS [Create Dt], ord.Accountid AS [Acct ID],
        Locations.BillingCompany, Locations.ShippingCompany,         
        ord.ReferralCode as referral,
        ShippingMethods.ShippingMethodMAS ,
        pack.TrackingNumber, CONVERT (varchar(8), pack.ShipDate , 1) as ShipDate
        from packages pack 
        inner join orders ord  on pack.orderid = ord.orderid
        inner join Accounts acct on ord.AccountID = acct.AccountID         
        inner join Locations on ord.locationID = Locations.LocationID
        inner join ShippingMethods  on ord.ShippingMethodID = ShippingMethods.ShippingMethodID 
        WHERE pack.TrackingNumber <> '' 
        and pack.ShipDate between @FromDate and @EndDate
        ORDER BY ord.orderID ">
         
        <SelectParameters>
            <asp:ControlParameter ControlID="txt_Startdate" DefaultValue=" " 
                Name="FromDate" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="txt_Enddate" DefaultValue=" " 
                Name="EndDate" PropertyName="Text" Type="String" />
            <asp:Parameter DefaultValue="1" Name="reportType" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>--%>
<%--    <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="select pack.PackageID, ord.OrderStatusID , ord.OrderType, ord.OrderID, ISNULL(ord.POnumber,'')  as PoNumber,
        CONVERT (varchar(8), ord.OrderDate, 1) AS [Create Dt], ord.Accountid AS [Acct ID],
        Locations.BillingCompany, Locations.ShippingCompany, 
        case Payments.PaymentMethodID when 1 then 'CC' ELSE 'PO' END AS [Pmt Type], 
        dbo.fn_formatcurrency(ISNULL((SELECT SUM(price * quantity) AS Amount FROM OrderDetails 
            WHERE (Orderid = ord.OrderId)), 0)  + ISNULL(ord.ShippingCharge, 0), ord.currencycode) AS Amount, 
        ord.ReferralCode as referral,
        ShippingMethods.ShippingMethodMAS ,
        pack.TrackingNumber, CONVERT (varchar(8), pack.ShipDate , 1) as ShipDate
        from packages pack 
        inner join orders ord  on pack.orderid = ord.orderid
        inner join Accounts acct on ord.AccountID = acct.AccountID 
        left join Payments  on ord.orderid = payments.orderID
        inner join Locations on ord.locationID = Locations.LocationID
        inner join ShippingMethods  on ord.ShippingMethodID = ShippingMethods.ShippingMethodID 
        WHERE pack.TrackingNumber <> '' 
        and pack.ShipDate between @FromDate and @EndDate
        ORDER BY ord.orderID ">
         
        <SelectParameters>
            <asp:ControlParameter ControlID="txt_Startdate" DefaultValue=" " 
                Name="FromDate" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="txt_Enddate" DefaultValue=" " 
                Name="EndDate" PropertyName="Text" Type="String" />
                <asp:Parameter DefaultValue="1" Name="reportType" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>--%>
  
  
</asp:Content>

