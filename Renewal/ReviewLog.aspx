<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Finance_Renewal_ReviewLog" EnableEventValidation ="false"  Codebehind="ReviewLog.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>



<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">
        var SelectAllID = "#ctl00_primaryHolder_gvReviewOrders_ctl01_SelectAll";
 
    $(document).ready(function () {
        $('#resultsDialog').dialog({
            autoOpen: false,
            width: 700,
            resizable: false,
            title: "Search Results",
            open: function (type, data) {
                $(this).parent().appendTo("form");
            },
            buttons: {
                "Cancel": function () {
                    $(this).dialog("close");
                }
            },
            close: function () {
                $('.ui-overlay').fadeOut();
            }
   
        });
      
        // selected Account No boxes.
        $(SelectAllID).click(function SelectAllAccounts() {
            var ischecked = ($(SelectAllID).is(":checked"));
            // add attribute 
            $('#ctl00_primaryHolder_gvReviewOrders input:checkbox').attr("checked", function () {
                this.checked = ischecked;
            });
        });

    });

    function openDialog(id) {
        $('.ui-overlay').fadeIn();
        $('#' + id).dialog("open");
    }

    function closeDialog(id) {
        $('#' + id).dialog("close");
    }

</script>
    <style type="text/css">
        .style1
        {
            text-align: right;
            font-weight: bold;
            text-shadow: 1px 1px white;
            width: 92px;
        }
        .style2
        {
            text-align: right;
            font-weight: bold;
            text-shadow: 1px 1px white;
            width: 65px;
        }
        .style3
        {
            text-align: right;
            font-weight: bold;
            text-shadow: 1px 1px white;
            width: 103px;
        }
        .style4
        {
            width: 103px;
        }
        .style5
        {
            height: 30px;
        }
        .Control
        {}
        .style6
        {
            width: 106px;
        }
        .style7
        {
            width: 112px;
        }
        
        th.RightAlignHeaderText
        {
            text-align: right;
        }
        
        th.CenterAlignHeaderText
        {
            text-align: center;
        }
        
        td.RightAlignItemText
        {
            text-align: right;
        }
		
		td.CenterItemText
        {
            text-align: center;
        }
        
        .CenterTextbox
        {
            text-align: center;
        }
    </style>
</asp:Content>

 


<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" runat="Server">

<asp:ScriptManager runat="server" id="mainScriptManager"></asp:ScriptManager>
  
     <div class="FormError" id="plErrorMessage" runat="server" visible="false">
		<p><span class="MessageIcon"></span>
	        <strong>Messages:</strong>
            <span id="lblRenewalRowMessage" runat="server" >An error was encountered.</span>
                
            <asp:BulletedList ID="blstErrors" runat="server" BulletStyle="Disc">
            </asp:BulletedList>
                
    </div>

    <asp:Panel ID="pnlRelease" runat="server">
          <div class="OToolbar JoinTable">      
             <ul>
                <li>
                    <asp:LinkButton ID="btnExport" runat="server"  
                        CommandName="Convert_Click"  
                        CssClass="Icon Export" onclick="btnExport_Click" >Export to Excel</asp:LinkButton>
                </li>
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
        DataKeyNames="AccountID" CurrentSortedColumn="AccountID" CurrentSortDirection="Ascending" 
        EmptyDataText="There are no renewal hold orders processed." 
        PagerStyle-CssClass="mt-hd" Width="100%"  OnRowCommand="gvReviewOrders_RowCommand"
              onselectedindexchanged="gvReviewOrders_SelectedIndexChanged">
        <AlternatingRowStyle CssClass="Alt" />
        <Columns>
            <asp:TemplateField HeaderText="" ItemStyle-HorizontalAlign="Center">
               
                <ItemTemplate>
                    <asp:HiddenField runat="server" ID="HidRenewalLogID"  Value='<%# Eval("RenewalLogID") %>' />
          
                     <asp:LinkButton ID="lnkHide" runat="server" 
                           CommandName="Hide"
                           CommandArgument='<%# Eval("RenewalLogID") %>'>
                           Hide
                     </asp:LinkButton> 
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Order#" ItemStyle-HorizontalAlign="Left">
                <ItemTemplate>

                   	<asp:Hyperlink ID="hlOrderLink"  runat="server" Text='<%# Eval("OrderID") %>'  
		    					NavigateUrl='<%# Eval("OrderID", "../../CustomerService/ReviewOrder.aspx?ID={0}") %>' 
			    				 />
                    <asp:HiddenField runat="server" ID="HidORDID"  Value='<%# Eval("OrderID") %>' />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Acc#" ItemStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    <asp:HyperLink ID="hlAccountLink" runat="server" 
                        NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"AccountID","../../InformationFinder/Details/Account.aspx?ID={0}" ) %>' 
                        Target="_blank" Text='<%# Bind("AccountID") %>'></asp:HyperLink>
                     <asp:HiddenField runat="server" ID="HidACCID"  Value='<%# Eval("AccountID") %>' />
 
                </ItemTemplate>  
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>
               <asp:BoundField DataField="AccountName"  
                HeaderText="Account Name" ItemStyle-Width="175px" HeaderStyle-Width="175px">
            <HeaderStyle />
            <ItemStyle />
            </asp:BoundField>
             <asp:BoundField DataField="RenewalNo"  
                HeaderText="Renewal#" >
            <HeaderStyle />
            <ItemStyle />
            </asp:BoundField>
             <asp:BoundField DataField="OrderStatus" 
                HeaderText="Status" >
            <HeaderStyle />
            <ItemStyle />
            </asp:BoundField>
            <asp:BoundField DataField="BrandName"  
                HeaderText="Brand"  
                ItemStyle-HorizontalAlign="Left" HeaderStyle-Width="75px" ItemStyle-Width="75px">
            <HeaderStyle />
            <ItemStyle HorizontalAlign="Left" />
            </asp:BoundField>
            
            <asp:BoundField DataField="BillingTermDesc"  
                HeaderText="Term" >
            <HeaderStyle />
            <ItemStyle />
            </asp:BoundField>
                       
            <asp:BoundField DataField="PaymentType"  
                HeaderText="Type" ControlStyle-Width="100px" 
                ItemStyle-HorizontalAlign="Left" HeaderStyle-Width="100px" ItemStyle-Width="100px">
            <HeaderStyle />
            
            <ItemStyle HorizontalAlign="Left" />
            
            </asp:BoundField>
            <asp:BoundField DataField="RenewalAmount" DataFormatString="{0:C}" 
                HeaderStyle-CssClass="RightAlignHeaderText" HeaderText="Amount"  
                ItemStyle-HorizontalAlign="Right" HeaderStyle-Width="175px" ItemStyle-Width="175px">
            <HeaderStyle />
            
            <ItemStyle HorizontalAlign="Right" />
            
           </asp:BoundField>
           <%-- <asp:BoundField DataField="ErrorMessage" DataFormatString="{0:C}" 
                HeaderText="Status" 
                ItemStyle-HorizontalAlign="Left">
            <HeaderStyle />
            
            </asp:BoundField>--%>
        </Columns>
        <PagerStyle />
    </asp:GridView>
    

    </asp:Panel>
   
</asp:Content>