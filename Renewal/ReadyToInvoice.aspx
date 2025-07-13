<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Finance_Renewal_ReadyToInvoice" EnableEventValidation ="false"  Codebehind="ReadyToInvoice.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>



<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">
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
    </style>
</asp:Content>

 


<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" runat="Server">

<asp:ScriptManager runat="server" id="mainScriptManager"></asp:ScriptManager>
  
    <div style="margin: 10px auto; width: 100%;">
            
          <asp:Panel ID="Panel1" runat="server">
          <div class="OToolbar JoinTable">      
             <ul>
                <li>
                    <asp:LinkButton ID="btnConvert" runat="server"  
                        CommandName="Convert_Click"  
                        CssClass="Icon Export" onclick="btnConvert_Click" >Export to Excel</asp:LinkButton>
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
            </ul>
   	
		 </div>
         <ec:GridViewEx ID="gvUpcomingRenewals" runat="server" CssClass="OTable" 
                    AlternatingRowStyle-CssClass="Alt"  AutoGenerateColumns="False" 
                    AllowPaging="True" meta:resourcekey="Grid" AllowSorting="True" PageSize="20" 
                    DataKeyNames="AccountID" CurrentSortedColumn="AccountID" CurrentSortDirection="Ascending" 
                    PagerStyle-CssClass="mt-hd" PagerSettings-Mode="NextPreviousFirstLast"
                    OnSorting="gvUpcomingRenewals_Sorting" OnPageIndexChanging="gvUpcomingRenewals_PageIndexChanging"
                    EmptyDataText="There are no renewals that meet the criteria you specified." 
                    SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
                    SortOrderDescendingImagePath="~/images/icon_sort_descending.gif" 
                    Width="100%">
                    <AlternatingRowStyle CssClass="Alt" />
                    <Columns>
                        <asp:BoundField DataField="AccountID" HeaderStyle-CssClass="mt-hd" 
                            HeaderText="Account#" meta:resourcekey="AccountID" 
                            ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                            SortExpression="AccountID">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                        </asp:BoundField>
                        <asp:BoundField DataField="CompanyName" HeaderStyle-CssClass="mt-hd" 
                            HeaderText="Company Name" ItemStyle-CssClass="mt-itm" 
                            SortExpression="CompanyName">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                        </asp:BoundField>
                        <asp:BoundField DataField="OrderID" DataFormatString="{0:d}" 
                            HeaderStyle-CssClass="mt-hd" HeaderText="Order#" ItemStyle-CssClass="mt-itm" 
                            ReadOnly="True" SortExpression="OrderID">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                        </asp:BoundField>
                        <asp:BoundField DataField="InvoiceNo" HeaderStyle-CssClass="mt-hd" 
                            HeaderText="Invoice#" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
                            SortExpression="InvoiceNo">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                        </asp:BoundField>
                        <asp:BoundField DataField="InvoiceAmt" HeaderStyle-CssClass="mt-hd" 
                            HeaderText="Amount" ItemStyle-CssClass="mt-hd" 
                            SortExpression="InvcAmt" DataFormatString="{0:C}" HeaderStyle-HorizontalAlign="Right">
                        
                        <HeaderStyle CssClass="mt-itm" />
                        
                        <ItemStyle CssClass="mt-itm" HorizontalAlign="Right" />
                        </asp:BoundField>
                        <asp:BoundField DataField="RevAmt" HeaderStyle-CssClass="mt-hd" 
                            HeaderText="Rev.Amt" ItemStyle-CssClass="mt-itm" SortExpression="RevAmt" 
                            DataFormatString="{0:C}">
                        <HeaderStyle CssClass="mt-hd" HorizontalAlign="Right" />
                        <ItemStyle CssClass="mt-itm" HorizontalAlign="Right" />
                        </asp:BoundField>
                        <asp:BoundField DataField="OrderDate" DataFormatString="{0:d}" 
                            HeaderStyle-CssClass="mt-hd" HeaderText="Ordered" ItemStyle-CssClass="mt-itm" 
                            SortExpression="OrderDate">
                        <HeaderStyle CssClass="mt-hd" />
                        <ItemStyle CssClass="mt-itm" />
                        </asp:BoundField>
                    
                        <asp:BoundField DataField="BrandName" HeaderText="Brand" />
                    
                    </Columns>
                    <PagerSettings Mode="NextPreviousFirstLast" />
                    <PagerStyle CssClass="mt-hd" />
                    <RowStyle CssClass="Row" />
                    <SelectedRowStyle CssClass="Row Selected" />
            </ec:GridViewEx>
        </asp:Panel>
       
    </div>

</asp:Content>