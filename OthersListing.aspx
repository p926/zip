<%@ Page Title="Others Listing" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_TechOps_OthersListing" Codebehind="OthersListing.aspx.cs" %>
<asp:Content ID="Content2" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript" charset="utf-8">
        $(document).ready(function () {

            // Declare JQuery Datepicker
            $('#<%=txtStartdate.ClientID %>').datepicker();

        });        

    </script>  
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" Runat="Server">

    <act:ToolkitScriptManager id="ToolkitScriptManager1" runat="server" />   

     <div  >
        
        <div class="OToolbar JoinTable" id="myToolBar" runat ="server"  >							
			<ul>								
				<li>     
					Start Date: 
                    <asp:TextBox ID="txtStartdate" runat="server" Width="100px" ></asp:TextBox>
					<asp:Button ID="btnGetListing" runat="server" Text="Go" CssClass="OButton" />
				</li>
			</ul>							
		</div>        

        <asp:GridView ID="gvOthersListing" runat="server" 
            AutoGenerateColumns="False" DataSourceID="lOthersListing"
            PageSize="20" AllowPaging="True" CssClass="OTable" AllowSorting="true" >

             <Columns>
                <asp:TemplateField HeaderText="Acc#" SortExpression="AccountID"
                    HeaderStyle-Width="60px" ItemStyle-CssClass="mt-itm rightalign"  HeaderStyle-CssClass="mt-hd">
                    <ItemTemplate>										
						<a href='<%# String.Format("../InformationFinder/Details/Account.aspx?ID={0}", Eval("AccountID")) %>' ><%# Eval("AccountID")%></a>
					</ItemTemplate>                        
                </asp:TemplateField> 
                <asp:TemplateField HeaderText="Account Name" SortExpression="AccountName"
                        ItemStyle-CssClass="mt-itm "  HeaderStyle-CssClass="mt-hd">
                        <ItemTemplate>
                            <asp:Label ID="lblCompany" runat="server" Text='<%# Bind("AccountName") %>' ></asp:Label>
                        </ItemTemplate>
                </asp:TemplateField> 
                <asp:TemplateField HeaderText="Created" SortExpression="CreatedDate"
                        HeaderStyle-Width="100px" ItemStyle-CssClass="mt-itm "  HeaderStyle-CssClass="mt-hd">
                        <ItemTemplate>
                            <asp:Label ID="lblCreatedDate" runat="server" Text='<%# Bind("CreatedDate","{0:d}") %>' ></asp:Label>
                        </ItemTemplate>
                </asp:TemplateField> 
                <asp:TemplateField HeaderText="Industry" SortExpression="IndustryName"
                        HeaderStyle-Width="100px" ItemStyle-CssClass="mt-itm "  HeaderStyle-CssClass="mt-hd">
                        <ItemTemplate>
                            <asp:Label ID="lblIndustry" runat="server" Text='<%# Bind("IndustryName") %>' ></asp:Label>
                        </ItemTemplate>
                </asp:TemplateField> 
                <asp:TemplateField HeaderText="Customer Type" SortExpression="CustomerTypeDesc"
                        HeaderStyle-Width="220px" ItemStyle-CssClass="mt-itm "  HeaderStyle-CssClass="mt-hd">
                        <ItemTemplate>
                            <asp:Label ID="lblCustomerDesc" runat="server" Text='<%# Bind("CustomerTypeDesc") %>' ></asp:Label>
                        </ItemTemplate>
                </asp:TemplateField> 
                </Columns> 

            <EmptyDataTemplate>
				<div class="NoData">
					There are no Others Listing by selected Start Date!
				</div>
			</EmptyDataTemplate>  
                                                  
            <AlternatingRowStyle CssClass="Alt" />
			<PagerStyle CssClass="Footer" />
                  
        </asp:GridView>

       
     </div>

     <asp:SqlDataSource ID="lOthersListing" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand=" 
                        select a.AccountID, a.AccountName, a.CreatedDate, i.IndustryName, c.CustomerTypeDesc
                        from dbo.Accounts	a
                        Join Industries i On a.IndustryID = i.IndustryID 
                        Join CustomerTypes c On a.CustomerTypeID = c.CustomerTypeID 
                        where i.IndustryName = 'Other'
                        and a.CreatedDate >= @StartDate
                        Order By a.CreatedDate, a.AccountID
                      " >
                                    
        <SelectParameters>            
            <asp:ControlParameter ControlID="txtStartdate" DefaultValue="" 
                Name="StartDate" PropertyName="Text" Type="String" />
        </SelectParameters>

    </asp:SqlDataSource>
 
</asp:Content>
         
              