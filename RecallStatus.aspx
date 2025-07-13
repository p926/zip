<%@ Page Title="Recall Status" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_RecallStatus" Codebehind="RecallStatus.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">

    <asp:ScriptManager ID="ScriptManager1" runat="server"> </asp:ScriptManager>

    <div>
    
        <div class="OToolbar JoinTable" id="myToolBar" runat ="server"  >							
		    <ul>								
			    <li>   
                    <asp:LinkButton ID="btnSaveExcel" runat="server"   
						CssClass="Icon Export" onclick="btnSaveExcel_Click" >Export to Excel</asp:LinkButton>  

                    <asp:LinkButton ID="btnShowAllRecall" runat="server"   
						CssClass="Icon Refresh" onclick="btnShowAllRecall_Click" >Paging On/Off</asp:LinkButton>  
				    
                    <span style="font-weight:bold; float:right" ><asp:Label runat="server" ID="lblTotalRecall"  Font-Bold="true"></asp:Label></span>
			    </li>               
		    </ul>							
	    </div> 
        
        <asp:GridView ID="gvRecalls" runat="server" AutoGenerateColumns="False" 
            DataSourceID="sqlRecalls" CssClass="OTable" AllowSorting="true"
            DataKeyNames="ReturnID" AllowPaging="false"  >
            <Columns>
            
                <asp:TemplateField HeaderText="RMA#" SortExpression="ReturnID"
                    ItemStyle-CssClass="mt-itm rightalign"  HeaderStyle-CssClass="mt-hd" ItemStyle-VerticalAlign="Top">
                    <ItemTemplate>										
						<a href='<%# String.Format("../InformationFinder/Details/Return.aspx?ID={0}", Eval("ReturnID")) %>' ><%# Eval("ReturnID")%></a>
					</ItemTemplate>                        
                </asp:TemplateField> 
                        
                <%--<asp:TemplateField HeaderStyle-Width="35px" ItemStyle-CssClass="mt-itm"  HeaderStyle-CssClass="mt-hd" >                
                    <ItemTemplate>
                        <asp:HyperLink ID="HyperLink2" NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"ReturnID","../InformationFinder/Details/Return.aspx?ID={0}" ) %>'
                        runat="server" ToolTip="Click to View Recall" Text="View"></asp:HyperLink>                        
                    </ItemTemplate>                    
                </asp:TemplateField>--%>
                <%--<asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm" DataField="ReturnID" HeaderText="RMA#" SortExpression="ReturnID"    />--%>
                <asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm rightalign" DataField="AccountID" HeaderText="Acc#" SortExpression="accountID" ItemStyle-VerticalAlign="Top"    />
                <asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm rightalign" DataField="serialno" HeaderText="Serial#"  SortExpression="serialno" ItemStyle-VerticalAlign="Top" />
                <asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm" DataField="sku" HeaderText="SKU"  SortExpression="sku" ItemStyle-VerticalAlign="Top" />
                <asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm" DataField="sku status" HeaderText="Badge Status"  SortExpression="sku status" HeaderStyle-Wrap="false" ItemStyle-VerticalAlign="Top" />
                <asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm" DataField="company" HeaderText="Account Name" SortExpression="company" HeaderStyle-Width ="100" ItemStyle-VerticalAlign="Top" />
                <asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm" DataField="createdDate" HeaderText="Recalled"  SortExpression="createdDate"  DataFormatString="{0:d}" ItemStyle-VerticalAlign="Top" />
                <%--<asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm" DataField="Createdby" HeaderText="Created By" SortExpression="Createdby"  HeaderStyle-Width ="70" />--%>
                <asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm" DataField="reason" HeaderText="Reason" SortExpression="reason" HeaderStyle-Width ="100" ItemStyle-VerticalAlign="Top" />
                <asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm" DataField="Status" HeaderText="Status" SortExpression="status"  HeaderStyle-Width ="120" ItemStyle-VerticalAlign="Top"  />
                <asp:BoundField HeaderStyle-CssClass="mt-hd" ItemStyle-CssClass="mt-itm" DataField="Notes" HeaderText="Notes" SortExpression="Notes"  ItemStyle-VerticalAlign="Top"  />
            </Columns>
            <EmptyDataTemplate>
			    <div class="NoData">
				    There are no RMAs!
			    </div>
		    </EmptyDataTemplate>  
                                                  
            <AlternatingRowStyle CssClass="Alt" />
            <PagerSettings Position="Bottom" />    
		    <PagerStyle CssClass="Footer" HorizontalAlign="Right" />
                                
        </asp:GridView>                

    </div> 

    <asp:SqlDataSource ID="sqlRecalls" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
     SelectCommand="select a.ReturnID, a.accountid, b.[description] as Reason, c.[status] as [status],
            a.CreatedDate, a.CreatedBy, a.Notes, a.orderID, d.CompanyName as company, 
            e.serialno, h.ProductSKU as sku, 
            g.DeviceInvStatusName  as [SKU Status]
            from rma_returns a 
            inner join  rma_ref_returnreason b on a.return_reasonid = b.reasonid
            inner join rma_ref_returnStatus c on a.[status] = c.returnStatusid
            inner join accounts d on a.accountid = d.accountid
            inner join rma_returndevices e on a.returnid = e.returnid
            inner join deviceinventory f on e.serialno = f.serialno
            inner join Products h on f.ProductID=h.ProductID 
            left join DeviceInventoryStatus g on f.DeviceInvStatusID = g.DeviceInvStatusID 
            where a.returntypeid =3
            order by a.returnid desc" />

</asp:Content>


