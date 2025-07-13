<%@ Page Title="Master/Basline Files Upload" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_MasterBaselineFileUpload" Codebehind="MasterBaselineFileUpload.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">

</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></act:ToolkitScriptManager> 

    <div>

         <asp:GridView ID="gvPOUpload" CssClass="OTable"  runat="server" Visible ="true"
            AllowPaging="true" PageSize = "20" AllowSorting="true"
            AutoGenerateColumns="False" DataKeyNames="ReceiptID" DataSourceID="GetUnloadedPOReceipts" >
            <Columns>                            
                            
                    <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="" >
						<ItemTemplate>
							                            
                            <asp:Hyperlink ID="Hyperlink3e"  runat="server" Text='Upload' 
											NavigateUrl='<%# GetUrl(DataBinder.Eval(Container.DataItem,"ReceiptID","" )) %>' 											
                                            Visible='<%# DisplayUploadButton(DataBinder.Eval(Container.DataItem,"UploadInMAS","" )) %>' />
                                                                   
						</ItemTemplate>
					</asp:TemplateField>                                           
                            
                    <asp:BoundField DataField="ReceiptID" HeaderText="Receipt#" SortExpression="ReceiptID" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                            
                    <asp:BoundField DataField="PONumber" HeaderText="PO#" SortExpression="PONumber" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                           
                    <asp:BoundField DataField="Vendor" HeaderText="Vendor#" SortExpression="Vendor" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" />                            
                    <asp:BoundField DataField="ShipmentID" HeaderText="ShipmentID" SortExpression="ShipmentID" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                                                        
                    <asp:BoundField DataField="ReceiptDate" HeaderText="Date" SortExpression="ReceiptDate" DataFormatString="{0:d}" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  /> 
                    <asp:BoundField DataField="TotalRecd" HeaderText="Total Recd" SortExpression="TotalRecd" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  />                                                        
                    <asp:BoundField DataField="UploadInMAS" HeaderText="Uploaded" SortExpression="UploadInMAS" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" Visible = "false"  />                                                        
            </Columns>

            <EmptyDataTemplate>
				<div class="NoData">
					There are no PO Receipt to upload!
				</div>
			</EmptyDataTemplate>                                
            <AlternatingRowStyle CssClass="Alt" />
			<PagerStyle CssClass="Footer" />
                        
        </asp:GridView>		
					
	</div>

    <asp:SqlDataSource ID="GetUnloadedPOReceipts" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand=" 
                        Select	a.ReceiptID, a.ShipmentID, a.Vendor , a.PONumber, a.ReceiptDate, a.UploadInMAS
                        ,TotalRecd = (select SUM(QtyRecd) from dbo.POReceiptDetails where ReceiptID = a.ReceiptID)
                        From dbo.POReceipts a                        	
                        Order By a.ReceiptDate Desc
                      " >
    </asp:SqlDataSource>    

</asp:Content>

