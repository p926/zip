<%@ Page Title="PO Receipts" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Shipping_POReceipt" Codebehind="POReceipt.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">

        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(showProgress);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(hideProgress);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();

        });

        function JQueryControlsLoad() {

            // Accordion
            $("#accordion").accordion({
                header: "h3",
                autoHeight: false
            });

            $('#<%=txtReceiptDate.ClientID %>').datepicker(
                {
                    showOn: "button"
                }
             );

            $('#poReceiptDialog').dialog({
                autoOpen: false,
                width: 500,
                resizable: false,
                title: "Manage PO Receipt",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('#<%= btnLoadPOReceipt.ClientID %>').click();
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Ok": function () {
                        $('#<%= btnAddPOReceipt.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancelPOReceipt.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });

            // UserBirthDate Datepicker
            $('#<%=txtFrom.ClientID %>').datepicker();
            $('#<%=txtTo.ClientID %>').datepicker();



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

        function executePrint(pReceiptID, pNCopies) {
            if (window.ActiveXObject) {
                try {
                    //alert("i am here.");
                    var oShell = new ActiveXObject("Shell.Application");
                    var prog = "C:\\Program Files\\POReceipts\\POReceiptBoxLabel.exe";
                    var pStr = pReceiptID + " " + pNCopies
                    oShell.ShellExecute(prog, pStr, "", "open", "1");
                }
                catch (e) {
                    alert(e.message);
                }
            }
            else {
                alert("Your browser does not support the local label printing. Try IE browser.");
            }
        }

    </script>
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager> 

    <asp:UpdateProgress id="POReceiptUpdateProgress" runat="server" DynamicLayout="true" DisplayAfter="0" >
        <ProgressTemplate>
            
        </ProgressTemplate>
    </asp:UpdateProgress>

    <act:ModalPopupExtender ID="modalExtender" runat="server" TargetControlID="POReceiptUpdateProgress"
        PopupControlID="Panel1" BackgroundCssClass="modalBackground" Enabled="true" >
    </act:ModalPopupExtender>

    <asp:Panel ID="Panel1" runat="server">
        <div style="width: 100%" align="center" >
            <img src="../images/orangebarloader.gif" />
        </div>
    </asp:Panel>

    <div id="poReceiptDialog" >
        <asp:UpdatePanel ID="upnl" runat="server" UpdateMode="Conditional"    >
            <ContentTemplate>
            
                <div class="FormError" id="poReceiptDialogError" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="poReceiptDialogErrorMsg" runat="server" >An error was encountered.</span></p>
				</div>
                <div class="FormMessage" id="messages" runat="server" visible="false"> 
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="submitMsg" runat ="server" >Ready to search.</span></p>
                </div> 

                <div id="accordion" style="margin-top:10px; width:99.9%;">

                    <div >
			            <h3><a href="#">Receipt Header</a></h3>
                        <div id="Div1"  class="OForm AccordionPadding" runat ="server">                                        
                            
                            <div class="Row" id= "receiptID" runat = "server" >
                                <div class="Label Medium">Receipt ID<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtReceiptID" 
                                        MaxLength="6" CssClass="Size XSmall" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label Medium">PO#<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlPONumber" runat="server" AutoPostBack="true" 
                                        onselectedindexchanged="ddlPONumber_SelectedIndexChanged" />                                    
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label Medium">Vendor#<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtVendor" 
                                        MaxLength="7" CssClass="Size XSmall" ValidationGroup="form" />                                                                                    
                                </div>
                                <div class="Clear"></div>
                            </div> 
                            
                            <div class="Row">
                                <div class="Label Medium">Vendor Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtVendorName" 
                                        CssClass="Size Medium2" ValidationGroup="form" />                                                                                    
                                </div>
                                <div class="Clear"></div>
                            </div>                                                     

                            <div class="Row">
                                <div class="Label Medium">Shipment ID<span class="Required">*</span>:<br /> <font size="-2">(Invoice#-Note#)</font> </div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtShipmentID" 
                                        MaxLength="30" CssClass="Size Medium2" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>                                        
                            
                            <div class="Row">
                                <div class="Label Medium">Box Count<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtCount" AutoPostBack ="true"
                                        MaxLength="4" CssClass="Size XXSmall" ValidationGroup="form"
                                        ontextchanged="txtCount_TextChanged" /> 
                                    
                                </div>
                                <div class="Clear"></div>
                            </div> 

                            <div class="Row">
                                <div class="Label Medium">Receipt Date<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtReceiptDate" 
                                        MaxLength="10" CssClass="Size Small" ValidationGroup="form" />                                    
                                </div>
                                <div class="Clear"></div>
                            </div> 
                            
                            <div class="Row">
                                <div class="Label Medium">Qty Total<span class="Required">*</span>:<br /> <font size="-2">(Per PackingSlip)</font></div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtPackingSlipTotal" 
                                        MaxLength="7" CssClass="Size XXSmall" ValidationGroup="form" />                                    
                                </div>
                                <div class="Clear"></div>
                            </div>                                                                                                                     
                                                                                                                                                                                                                                                                                                                                                                            
                        </div> 
                    </div>

                    <div id="receiptDetail" runat ="server">
			            <h3><a href="#">Receipt Details</a></h3>
                        <div class="OForm AccordionPadding">
                            
                             <asp:UpdatePanel ID="upnlReceiptDetail" runat="server" UpdateMode="Conditional" >

                                <Triggers>
				                    <asp:AsyncPostBackTrigger controlid="txtCount" eventname="TextChanged" />
			                    </Triggers>                                                                                              

                                <ContentTemplate>  
                                     
                                    <div id = "EditDetail" runat="server" class="Row" >
                                        <asp:GridView ID="grdViewEditDetail" CssClass="OTable"  runat="server" AutoGenerateColumns="False" 
                                            OnRowDataBound="grdViewEditDetail_RowDataBound" >
                                            <Columns>                            
                                                                                                                                                                                               
                                                <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  HeaderText="Item Number" >
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblReceiptDetailID" runat="server" Text='<%# Eval("ReceiptDetailID") %>' Visible="false" />
                                                        <asp:Label ID="lblRDItemNumber" runat="server" Text='<%# Eval("ItemNumber") %>' Visible="false" />
                                                        <asp:DropDownList ID="ddlRDItemNumber" runat="server" DataTextField="ItemNumber" DataValueField="ItemNumber" >                                                            
                                                        </asp:DropDownList>
                                                    </ItemTemplate>
                                                </asp:TemplateField> 
                                                
                                                <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  HeaderText="Qty Per PackingSlip">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtRDQtyPerPackingSlip"  runat = "Server" Width="60px" Text='<%# Eval("QtyPerPackingSlip") %>'  />
                                                    </ItemTemplate>
                                                </asp:TemplateField>                                         

                                                <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  HeaderText="Actual Qty Received">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtRDQtyRecd"  runat = "Server"  Width="60px" Text='<%# Eval("QtyRecd") %>'  />
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
                                        <asp:LinkButton id="btnMoreDetail" runat="server" text ="More Item Number ..." onclick="btnMoreDetail_Click" /> 
                                    </div>                                                                         

                                </ContentTemplate>

                            </asp:UpdatePanel> 

                        </div>                                                   
                    </div>                    

                    <div >
			            <h3><a href="#">Printing Options</a></h3>
                        <div id="Div2"  class="OForm AccordionPadding" runat ="server">   
                          
                             <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" >
                                <ContentTemplate>
                                    <div class="Row" id= "Div3" runat = "server" >
                                        <div class="Label Small"></div>
                                        <div class="Control">
                                            <asp:CheckBox ID="chkReceiveDoc" runat="server" Text="Print Receiving Document" />    
                                        </div>
                                        <div class="Clear"></div>
                                    </div>

                                    <div class="Row">
                                        <div class="Label Small"></div>
                                        <div class="Control">
                                            <asp:CheckBox ID="chkBoxLabel" runat="server" Text="Print Box Label" />                                   
                                        </div>
                                        <div class="Clear"></div>
                                    </div> 
                                </ContentTemplate>
                             </asp:UpdatePanel>                                   
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                        </div> 
                    </div>

                </div>                                                                                            
                             
                <asp:button text="Save" style="display:none;" id="btnAddPOReceipt" OnClick="btnAddPOReceipt_Click" runat="server" />
                <asp:button text="Close" style="display:none;" id="btnCancelPOReceipt" OnClick="btnCancelPOReceipt_Click" runat="server" />
                <asp:button text="Load" style="display:none;" id="btnLoadPOReceipt" OnClick="btnLoadPOReceipt_Click" runat="server" />

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>


    <div>

		<asp:UpdatePanel ID="UpdatePanelPOReceipt" runat="server"  UpdateMode="Conditional">
			<Triggers>
				<asp:AsyncPostBackTrigger controlid="btnAddPOReceipt" eventname="Click" />
			</Triggers>

			<ContentTemplate>

				<div class="OToolbar JoinTable" id="POReceiptSearchToolBar" runat ="server">
					<ul>
                    
                        <li>
                                        
                            <asp:LinkButton ID="btnNewPOReceipt" runat="server"  
                                CommandName="NewPOReceipt" CommandArgument="" 
                                CssClass="Icon Add" onclick="btnNewPOReceipt_Click" >Create PO Receipt</asp:LinkButton>
                               
                        </li>						
                            
                        <li>

                            <asp:Label ID="Label3" runat="server" Text="Vendor:"></asp:Label>                                   
                            <asp:DropDownList ID="ddlVendorSearch" runat="server" AutoPostBack="true" >                                                              
                            </asp:DropDownList> &nbsp;
                               
                            <asp:Label ID="Label2" runat="server" Text="From:"></asp:Label>                                   
                            <asp:TextBox ID="txtFrom" runat="server" Width="100px" AutoPostBack="False"></asp:TextBox>
                            <asp:Label ID="Label1" runat="server" Text="To:"></asp:Label>                                   
                            <asp:TextBox ID="txtTo" runat="server" Width="100px" AutoPostBack="False"></asp:TextBox> &nbsp;

                            <asp:Label ID="lblSearch" runat="server" Text="Search:"></asp:Label>                                  
                            <asp:TextBox ID="txtSearch" runat="server" Width="200px" AutoPostBack="False"></asp:TextBox> &nbsp;
                                                        
                            <asp:Button ID="btnGo" runat="server" Text="Go" CssClass="OButton" />
                              
                        </li>
                        
					</ul>
				</div>

                <asp:GridView ID="grdPOReceiptsView" CssClass="OTable"  runat="server" Visible ="true"
                    AllowPaging="true" PageSize = "20" AllowSorting="true"
                    AutoGenerateColumns="False" DataKeyNames="ReceiptID" 
                    OnRowDataBound="grdPOReceiptsView_RowDataBound"
                    DataSourceID="GetPOReceiptsBySearchCriteria" >
                    <Columns>                            
                            
                            <asp:TemplateField ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-CssClass="mt-hd" HeaderText="Receipt#" SortExpression="ReceiptID" HeaderStyle-Width="100px">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEditPOReceipt" runat="server"  
                                        CommandName="EditPOReceipt" CommandArgument='<%# Eval("ReceiptID")%>' 
                                        onclick="btnEditPOReceipt_Click" ><%# Eval("ReceiptID")%></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>                            
                             
                            <asp:BoundField DataField="PONumber" HeaderText="PO#" SortExpression="PONumber" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="100px" />                           
                            <asp:BoundField DataField="Vendor" HeaderText="Vendor#" SortExpression="Vendor" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="100px" />                            
                            <asp:BoundField DataField="ShipmentID" HeaderText="ShipmentID" SortExpression="ShipmentID" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                            
                            <asp:BoundField DataField="BoxesRecd" HeaderText="Box Count" SortExpression="BoxesRecd" ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="100px" />
                            <asp:BoundField DataField="TotalRecd" HeaderText="Total Recd" SortExpression="TotalRecd" ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="100px" />
                            <%--<asp:BoundField DataField="FDA10BLU" HeaderText="FDA10BLU" SortExpression="FDA10BLU" ItemStyle-CssClass="mt-itm RightAlign " HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="70px" />
                            <asp:BoundField DataField="FDA10BLK" HeaderText="FDA10BLK" SortExpression="FDA10BLK" ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="70px" />
                            <asp:BoundField DataField="FDA10SLV" HeaderText="FDA10SLV" SortExpression="FDA10SLV" ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="70px" />
                            <asp:BoundField DataField="FDA10GRN" HeaderText="FDA10GRN" SortExpression="FDA10GRN" ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="70px" />
                            <asp:BoundField DataField="FDA10PNK" HeaderText="FDA10PNK" SortExpression="FDA10PNK" ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="70px" />--%>
                            <asp:BoundField DataField="ReceiptDate" HeaderText="Date" SortExpression="ReceiptDate" DataFormatString="{0:d}" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="100px" />                            
                            <asp:CheckBoxField DataField="MASUploaded" HeaderText="MAS Uploaded" SortExpression="MASUploaded"  HeaderStyle-Width="100px" ItemStyle-CssClass="mt-itm CenterAlign" HeaderStyle-CssClass="mt-hd" HeaderStyle-Wrap="false"/>                                              

                            <asp:TemplateField ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-CssClass="mt-hd" HeaderText="Release To WIP" HeaderStyle-Width="100px">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnReleaseToWIP" runat="server"  
                                        CommandName="ReleaseToWip" CommandArgument='<%# Eval("ReceiptID")%>' 
                                        onclick="btnReleaseToWIP_Click" ></asp:LinkButton>                                                                       

                                </ItemTemplate>
                            </asp:TemplateField>                                                      

                    </Columns>

                    <EmptyDataTemplate>
						<div class="NoData">
							There are no PO Receipt records found!
						</div>
					</EmptyDataTemplate>                                
                    <AlternatingRowStyle CssClass="Alt" />
					<PagerStyle CssClass="Footer" />
                        
                </asp:GridView>	
                								

			</ContentTemplate>
		</asp:UpdatePanel>   
					
	</div>

    <asp:SqlDataSource ID="GetPOReceiptsBySearchCriteria" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_GetAllPOReceiptsBySearch" SelectCommandType="StoredProcedure" >
        <SelectParameters>
            <asp:ControlParameter ControlID="txtSearch" DefaultValue=" "  Name="SearchString" PropertyName="Text" Type="String" /> 
            <asp:ControlParameter ControlID="txtFrom" DefaultValue=" "  Name="FromDate" PropertyName="Text" Type="String" /> 
            <asp:ControlParameter ControlID="txtTo" DefaultValue=" "  Name="ToDate" PropertyName="Text" Type="String" /> 
            <asp:ControlParameter ControlID="ddlVendorSearch" DefaultValue=" "  Name="Vendor" PropertyName="SelectedItem.Value" Type="String" />            
        </SelectParameters>
    </asp:SqlDataSource>	    

</asp:Content>

