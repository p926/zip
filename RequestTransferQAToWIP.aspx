<%@ Page Title="Request a Transfer to WIP" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_RequestTransferQAToWIP" Codebehind="RequestTransferQAToWIP.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">

    <script type="text/javascript">

        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
        });

        function JQueryControlsLoad() {
            $('#<%=txtReceiptDate.ClientID %>').datepicker();
        }
        
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager> 

    <div>

		<%--<asp:UpdatePanel ID="udpn" runat="server"  UpdateMode="Conditional">
			
			<ContentTemplate>

                
                								

			</ContentTemplate>
		</asp:UpdatePanel>   --%>

        <div class="FormError" id="errors" runat="server" visible="false" style="margin:10px" >
	        <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
        </div>
        <div class="FormMessage" id="messages" runat="server" visible="false" style="margin:10px" > 
	        <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="submitMsg" runat ="server" >Ready to search.</span></p>
        </div>    

		<div class="OToolbar JoinTable" id="POSearchToolBar" runat ="server">
			<ul>
                                            
                <li>
                    <asp:LinkButton ID="btn_refreshPickSheet" runat="server"   
						            CssClass="Icon Refresh" onclick="btnFind_Click" >Refresh Page</asp:LinkButton> &nbsp;
                    <asp:Label ID="lblReceiptID" runat="server" Text="Receipt#:"></asp:Label>                                  
                    <asp:TextBox ID="txtReceiptID" runat="server" Width="100px" AutoPostBack="False"></asp:TextBox> &nbsp;
                    <asp:Label ID="lblReceiptDate" runat="server" Text="Receipt Date:"></asp:Label>                                   
                    <asp:TextBox ID="txtReceiptDate" runat="server" Width="100px" AutoPostBack="False"></asp:TextBox> &nbsp;
                            
                    <asp:Button ID="btnFind" runat="server" Text="Find Available in QA" CssClass="OButton" 
                        onclick="btnFind_Click"/>
                              
                </li>
                        
			</ul>
		</div>

        <asp:GridView ID="gvQADevice" CssClass="OTable"  runat="server" Visible ="true"
            AllowSorting="false" AutoGenerateColumns="False" DataKeyNames="ReceiptID" >

            <Columns>                                                                                                              
                            
                    <asp:BoundField DataField="ReceiptID" HeaderText="Receipt#" SortExpression="ReceiptID" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                           
                    <asp:BoundField DataField="ReceiptDate" HeaderText="Receipt Date" SortExpression="ReceiptDate" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataFormatString="{0:d}" />   
                    <asp:BoundField DataField="ShipmentID" HeaderText="Shipment#" SortExpression="ShipmentID" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />   
                    <asp:BoundField DataField="Vendor" HeaderText="Vendor" SortExpression="Vendor" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />   
                    <asp:BoundField DataField="PONumber" HeaderText="PO#" SortExpression="PONumber" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />   
                    <asp:BoundField DataField="ItemNumber" HeaderText="Item#" SortExpression="ItemNumber" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  /> 
                    <asp:BoundField DataField="Pending Release" HeaderText="Pending Release" SortExpression="Pending Release" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />  
                    <asp:BoundField DataField="Remaining" HeaderText="Avail in QA" SortExpression="Remaining" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                                               
                    <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  HeaderText="Request Total">
                        <ItemTemplate>
                            <asp:TextBox ID="txtRequestTotal"  runat = "Server" Text='' Enabled="true"  />
                        </ItemTemplate>
                    </asp:TemplateField>

            </Columns>

            <EmptyDataTemplate>
				<div class="NoData">
					No Instadose available in QA to make a transfering request!
				</div>
			</EmptyDataTemplate>                                
            <AlternatingRowStyle CssClass="Alt" />
			<PagerStyle CssClass="Footer" />
                        
        </asp:GridView>	

        <div class="Buttons">
            <%--<div class="RequiredIndicator"><span class="Required">*</span> Indicates a required field.</div>--%>
            <div class="ButtonHolder">                    
                <asp:Button ID="btnSubmit" CssClass="OButton" runat="server" Text="Submit" onclick="btnSubmit_Click" />                                             
            </div>
            <div class="Clear"> </div>
        </div> 

        <br />
        <br />        

        <div style="width:300px">
            <table class="OTable" >                                    
                <tr>
                    <th  class="mt-itm-bold leftalign" >
                        Availability in QA Summary
                    </th>                                                                 
                </tr>
              
                <tr>
                    <td class="leftalign">
                        <span id="Summary" runat="server" >N/A</span>
                    </td>                              
                </tr>
            </table> 
        </div>
        
					
	</div>

</asp:Content>

