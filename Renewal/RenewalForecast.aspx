<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Finance_Renewal_RenewalForecast"
     EnableEventValidation ="false"  Debug="true" Codebehind="RenewalForecast.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>



<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
	<script type="text/javascript">
	    $(function () {
	        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
	        EndRequestHandler(null, null);
	    });

	    function EndRequestHandler(sender, args) {
	        $("#<%= txtPeriodFrom.ClientID %>").datepicker();
	        $("#<%= txtPeriodTo.ClientID %>").datepicker();
	    }
    </script>
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
				<li>Period From:</li>
			    <li>
					<asp:TextBox ID="txtPeriodFrom" runat="server" AutoPostBack="true" 
							  OnTextChanged="txtPeriodFrom_TextChange" style="width: 100px" />
			    </li>
			    <li>To:</li>
                <li>
			    	<asp:TextBox ID="txtPeriodTo" runat="server" AutoPostBack="true" 
							  OnTextChanged="txtPeriodTo_TextChange" style="width: 100px" />
			    </li>
			    <li>
                    <asp:Button Text="Search" runat="server" CssClass="OButton" OnClick="btnSearch_Click" ID="btnSearch" /> 
                </li>
            </ul>
	        
		 </div>
			<ec:GridViewEx ID="gvForecastRenewals" runat="server" CssClass="OTable" 
					AlternatingRowStyle-CssClass="Alt"  AutoGenerateColumns="False" 
					AllowPaging="True" meta:resourcekey="Grid" AllowSorting="True" PageSize="20" 
					DataKeyNames="AccountID" CurrentSortedColumn="AccountID" CurrentSortDirection="Ascending" 
					PagerStyle-CssClass="mt-hd" PagerSettings-Mode="NextPreviousFirstLast"
					OnSorting="gvForecastRenewals_Sorting" OnPageIndexChanging="gvForecastRenewals_PageIndexChanging"
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

						<asp:BoundField DataField="AccountName" HeaderStyle-CssClass="mt-hd" 
							HeaderText="Account" ItemStyle-CssClass="mt-itm" 
							SortExpression="AccountName">
							<HeaderStyle CssClass="mt-hd" />
							<ItemStyle CssClass="mt-itm" />
						</asp:BoundField>

						<asp:BoundField DataField="BrandName" HeaderStyle-CssClass="mt-hd" 
							HeaderText="Brand" ItemStyle-CssClass="mt-itm" 
							SortExpression="Brand">
							<HeaderStyle CssClass="mt-hd" />
							<ItemStyle CssClass="mt-itm" />
						</asp:BoundField>

						<asp:BoundField DataField="RenewalMonth" HeaderStyle-CssClass="mt-hd" 
							HeaderText="Month" ItemStyle-CssClass="mt-itm" ReadOnly="True" 
							SortExpression="RenewalMonth">
							<HeaderStyle CssClass="mt-hd" />
							<ItemStyle CssClass="mt-itm" />
						</asp:BoundField>

						<asp:BoundField DataField="RenewalYear" 
							HeaderText="RenewalYear"  
							SortExpression="Year" HeaderStyle-HorizontalAlign="Left">
						
							<HeaderStyle />
							<ItemStyle HorizontalAlign="Left" />
						</asp:BoundField>

                      
						<asp:BoundField DataField="RenewalTotal"  
							HeaderText="RenewalTotal" SortExpression="RevAmt" 
							DataFormatString="{0:C}">
							
							<HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Right" />
							<ItemStyle CssClass="mt-itm" HorizontalAlign="Right" />
						</asp:BoundField>
						
                        <%--
                        <asp:BoundField DataField="CurrencyCode" HeaderStyle-CssClass="mt-hd" 
							HeaderText="Currency Code" ItemStyle-CssClass="mt-itm" 
							SortExpression="CurrencyCode">
							<HeaderStyle CssClass="mt-hd" />
							<ItemStyle CssClass="mt-itm" />
						</asp:BoundField>
                        --%>
					</Columns>
					<PagerSettings Mode="NextPreviousFirstLast" />
					<PagerStyle CssClass="mt-hd" />
					<RowStyle CssClass="Row" />
					<SelectedRowStyle CssClass="Row Selected" />
			</ec:GridViewEx>
		
		</asp:Panel>
	   
	</div>

</asp:Content>