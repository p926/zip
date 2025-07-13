<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="ReceivingBinManagement.aspx.cs" Inherits="Shipping_ReceivingBinManagement" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager>

    <div>

		<asp:UpdatePanel ID="UpdatePanelReturnReason" runat="server"  UpdateMode="Conditional">			
			<ContentTemplate>				

                <div class="FormError" id="error" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
				</div>
                <div class="FormMessage" id="success" runat="server" visible="false"> 
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="successMsg" runat ="server" >Commit successfully!.</span></p>
                </div> 

                <asp:GridView ID="grdViewReturnReason" CssClass="OTable"  runat="server" Visible ="true"
                    AllowPaging="true" PageSize = "20" AllowSorting="true"
                    AutoGenerateColumns="False" DataKeyNames="ReasonID"  
                    OnRowDataBound="grdViewReturnReason_RowDataBound"
                    DataSourceID="GetReturnReason" >
                    <Columns>   
                        
                            <asp:BoundField DataField="RMAType" HeaderText="RMA Type" SortExpression="RMAType" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="100px" />                           
                            <asp:BoundField DataField="Description" HeaderText="Return Reason" SortExpression="Description" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                                                        
                        
                            <asp:TemplateField HeaderText="Bin" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width ="100px" ItemStyle-Width="100px">
								<ItemTemplate>                                     
									<asp:Label ID="lblBinID" runat="server" Text='<%# Eval("BinID") %>' Visible="false" />
									<asp:DropDownList ID="ddlBin" runat="server" DataTextField="Bin" DataValueField="BinID" >                                                            
									</asp:DropDownList>
								</ItemTemplate>
							</asp:TemplateField> 

                    </Columns>

                    <EmptyDataTemplate>
						<div class="NoData">
							There are no Return Reasons records found!
						</div>
					</EmptyDataTemplate>                                
                    <AlternatingRowStyle CssClass="Alt" />
					<PagerStyle CssClass="Footer" />
                        
                </asp:GridView>	

                <div class="Buttons">        
                    <div class="ButtonHolder">                        
                        <asp:Button ID="btnSave" CssClass="OButton" runat="server" Text="Save" Font-Size="Large"
                            onclick="btnSave_Click" />
                    </div>
                    <div class="Clear"> </div>
                </div>
                								
			</ContentTemplate>
		</asp:UpdatePanel>   
					
	</div>    	

    <asp:SqlDataSource ID="GetReturnReason" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.AppConnectionString %>"
        SelectCommand="SELECT ReasonID, Description, BinID, 
                        case when DepartmentID = 3 then 'RMA Recall' else 'RMA Return' end as 'RMAType'                         
                        FROM rma_ref_ReturnReason where active = 1 Order By RMAType, Description;">
    </asp:SqlDataSource>

</asp:Content>
