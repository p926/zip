<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Admin_Renewal_ReviewRenewals" EnableEventValidation ="false"  Codebehind="ReviewRenewals.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">
    $(function () {
        $('#ctl00_primaryHolder_gvReviewOrders_ctl01_SelectAll').click(function SelectAllAccounts() {
            $('#ctl00_primaryHolder_gvReviewOrders input:checkbox').attr("checked", $(this).is(":checked"));
        });
    });
</script>
</asp:Content>

 


<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" runat="Server">

<asp:ScriptManager runat="server" id="mainScriptManager"></asp:ScriptManager>
     <div class="FormError" id="plErrorMessage" runat="server" visible="false">
		<p><span class="MessageIcon"></span>
	        <strong>Messages:</strong>
            <span id="lblRenewalRowMessage" runat="server" >An error was encountered.</span>
                
            <asp:BulletedList ID="blstErrors" runat="server" BulletStyle="Disc">
            </asp:BulletedList>
        </p>
    </div>

    <asp:Panel ID="pnlRelease" runat="server">
          <div class="OToolbar JoinTable">      
             <ul>
                <li>Billing Term:</li>
                <li>
                    <asp:DropDownList ID="ddlBillingTerm" runat="server" AutoPostBack="true" 
                        OnSelectedIndexChanged="gridviewBinder">
                    </asp:DropDownList>
                </li>
                <li>Brand:</li>
                <li>        
                    <asp:DropDownList ID="ddlBrand" runat="server" AutoPostBack="true" 
                        OnSelectedIndexChanged="gridviewBinder">
                    </asp:DropDownList>
                </li>
                <li>Billing Method:</li>
				<li>
                    <asp:DropDownList ID="ddlBillingMethod" runat="server" AutoPostBack="true" 
                        OnSelectedIndexChanged="gridviewBinder">
                    </asp:DropDownList>
                </li>
                <li>Renewal#:</li>
				<li>
                    <asp:DropDownList ID="ddlRenewalLog" runat="server" AutoPostBack="true" 
                        OnSelectedIndexChanged="gridviewBinder">
                    </asp:DropDownList>
                </li>
            </ul>
   	
		 </div>
         <asp:GridView ID="gvReviewOrders" CssClass="OTable" AlternatingRowStyle-CssClass="Alt" 
        AutoGenerateColumns="False" runat="server" 
        OnSorting="gvReviewOrders_Sorting" OnPageIndexChanging="gvReviewOrders_PageIndexChanging"
        DataKeyNames="AccountNo" CurrentSortedColumn="AccountNo" CurrentSortDirection="Ascending" 
        EmptyDataText="There are no renewal hold orders processed." OnRowCommand="gvReviewOrders_RowCommand">
        <AlternatingRowStyle CssClass="Alt" />
        <Columns>
            <asp:TemplateField HeaderText="" ItemStyle-HorizontalAlign="Center">
                <ItemTemplate>
                    <asp:HiddenField runat="server" ID="HidRenewalLogID"  Value='<%# Eval("RenewalLogID") %>' />
                     <asp:LinkButton ID="lnkHide" runat="server"  CommandName="Hide" CommandArgument='<%# Eval("RenewalLogID") %>' Text="Hide" />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Order#" ItemStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    <asp:HiddenField runat="server" ID="hfOrderID"  Value='<%# Eval("OrderID") %>' />
                    <asp:HyperLink ID="hlOrderLink" runat="server" 
                        NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"OrderNo","~/InformationFinder/Details/Order.aspx?ID={0}" ) %>' 
                        Target="_blank" Text='<%# Bind("OrderNo") %>'></asp:HyperLink>
                    <asp:HiddenField runat="server" ID="HidORDID"  Value='<%# Eval("OrderNo") %>' />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Acc#" ItemStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    <asp:HyperLink ID="hlAccountLink" runat="server" 
                        NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"AccountNo","~/InformationFinder/Details/Account.aspx?ID={0}" ) %>' 
                        Target="_blank" Text='<%# Bind("AccountNo") %>'></asp:HyperLink>
                     <asp:HiddenField runat="server" ID="HidACCID"  Value='<%# Eval("AccountNo") %>' />
                </ItemTemplate>  
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>
               <asp:BoundField DataField="AccountName" HeaderText="Account Name">
            </asp:BoundField>
             <asp:BoundField DataField="RenewalNo" HeaderText="Renewal#">
            </asp:BoundField>
             <asp:BoundField DataField="OrderStatus" HeaderText="Status">
            </asp:BoundField>
            <asp:BoundField DataField="BrandName"  HeaderText="Brand" ItemStyle-HorizontalAlign="Left">
            <HeaderStyle />
            <ItemStyle HorizontalAlign="Left" />
            </asp:BoundField>
            
            <asp:BoundField DataField="BillingTermDesc"  HeaderText="Term">
            </asp:BoundField>
                       
            <asp:BoundField DataField="PaymentType"  
                HeaderText="Type" ItemStyle-HorizontalAlign="Left">
            <ItemStyle HorizontalAlign="Left" />
            
            </asp:BoundField>
            <asp:BoundField DataField="RenewalAmount" DataFormatString="{0:C}" 
                HeaderText="Amount" ItemStyle-HorizontalAlign="Right">
            <ItemStyle HorizontalAlign="Right" />
           </asp:BoundField>

        </Columns>
    </asp:GridView>
    </asp:Panel>
    
    <asp:Panel runat="server" ID="pnlReview" Visible="false">
        <asp:HiddenField runat="server" ID="hfRenewalNo" Value="0" />
            
        <div>
            <asp:Button Text="&lt;&lt; Back" runat="server" CssClass="OButton" OnClick="btnBack_Click" ID="btnBack" /> <br />
            <h3>Renewal Release Review</h3>
        </div>
                  
        <ec:GridViewEx ID="gvReview" runat="server" AutoGenerateColumns="false" CssClass="OTable"
                AlternatingRowStyle-CssClass="Alt" EmptyDataText="There are no valid renewal hold orders released.">           
                <Columns>
                <asp:BoundField DataField="AccountID" HeaderText="Acc#" />
                <asp:BoundField DataField="AccountName" HeaderText="Account Name" />
                <asp:BoundField DataField="OrderStatusName" HeaderText="Status" />
                <asp:BoundField ItemStyle-HorizontalAlign="Right" DataField="RenewalAmount" HeaderText="Amount" DataFormatString="{0:C}" />
                <asp:BoundField DataField="BrandName" HeaderText="Brand" />
                <asp:BoundField DataField="BillingTermDesc" HeaderText="Term" />
                <asp:BoundField DataField="PaymentMethodName" HeaderText="Method" />
                                             
            </Columns>
        </ec:GridViewEx>
    </asp:Panel>    
   

</asp:Content>