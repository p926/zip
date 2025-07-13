<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" 
    Inherits="InstaDose_Finance_Renewal_ScheduledBillings" Codebehind="ScheduledBillings.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">

 <style type="text/css">  

  #overlay {
    background-color: black;
    position: fixed;
    top: 0; right: 0; bottom: 0; left: 0;
    opacity: 0.2; /* also -moz-opacity, etc. */
    /*opacity: 0.5; */
    z-index: 10;
    }


.updateProgress
{
	border-width: 1px;
	border-style: solid;
	background-color: #FFFFFF;
	position: absolute;
	width: 50em;  /*180px;*/
	height: 100px /*65px;*/
}

    /*class="ControlsCenter" will center all controls*/
    .ControlsCenter
    {
        margin-left: auto;
        margin-right: auto;
        /*width: 50em;*/
        width: 500em;
        text-align:center;
    }
      
</style>

<script type="text/javascript">
    var SelectAllID = "#ctl00_primaryHolder_gvUpcomingBillings_ctl01_SelectAll";
 
    $(document).ready(function () {

        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(lockControls);
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
        JQueryControlsLoad();
    });

     function lockControls() {
         $('#<%= btnGenerate.ClientID %>').attr("disabled", "disabled").val('Processing...');
     }

     function JQueryControlsLoad(sender, args) {
         $('#<%= btnGenerate.ClientID %>').removeAttr("disabled").val('Generate Selected Billings');

         // Set the date picker objects
         $("#<%= txtPeriodFrom.ClientID %>").datepicker();
         $("#<%= txtPeriodTo.ClientID %>").datepicker();

 
         // selected Account No boxes.
         $(SelectAllID).click(function SelectAllAccounts() {
             var ischecked = ($(SelectAllID).is(":checked"));
             // add attribute 
             $('#ctl00_primaryHolder_gvUpcomingBillings input:checkbox').attr("checked", function () {
                 this.checked = ischecked;
             });
         });

     }

</script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>

  <asp:UpdatePanel ID="uppnlBillings" runat="server">
    <ContentTemplate>
 
    <div style="padding:10px">
       
    <div class="FormError" id="plErrorMessage" runat="server" visible="false">
        <p><span class="MessageIcon"></span> </p>
            <strong>Messages:</strong>
            <span id="lblBillingErrorMessage" runat="server" >One or more errors have occurred:</span>
                
            <asp:BulletedList ID="blstErrors" runat="server" BulletStyle="Disc">
            </asp:BulletedList>                            
    </div>
 
                          
    <asp:Panel runat="server" ID="pnlBillings">
        
        <div style="width: 100%">
          
            <div class="OToolbar JoinTable">
               <ul>
                   
                    <li>Brand:</li>
                    <li>
                        <asp:DropDownList ID="ddlBrand" runat="server" AutoPostBack="true" 
                              OnSelectedIndexChanged="gridviewBinder">
                        </asp:DropDownList>
                    </li>
                  
                    <li>Billing Method:</li>
                    <li>
                          <asp:DropDownList ID="ddlBillingMethod" runat="server" AutoPostBack="True"
                              OnSelectedIndexChanged="gridviewBinder">
                          </asp:DropDownList>
                    </li>
                    <li>Period From:</li>
                    <li>
                         <asp:TextBox runat="server" ID="txtPeriodFrom" 
                            OnTextChanged="txtPeriodFrom_TextChange" 
                            AutoPostBack="true" style="width: 100px" />
                    </li>
                    <li>To:</li>
                    <li>
                        <asp:TextBox runat="server" ID="txtPeriodTo" 
                            OnTextChanged="txtPeriodTo_TextChange" 
                            AutoPostBack="true" style="width: 100px" />
                    </li>
  
                </ul>
            </div>

            <ec:GridViewEx ID="gvUpcomingBillings" runat="server" AutoGenerateColumns="False" 
                AllowPaging="True" AllowSorting="True" PageSize="20" 
             DataKeyNames="AccountID" CurrentSortedColumn="WhenToBill" CurrentSortDirection="Ascending" 
             CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
             PagerSettings-Mode="NextPreviousFirstLast"
             OnSorting="gvUpcomingBillings_Sorting"  OnPageIndexChanging="gvUpcomingBillings_PageIndexChanging"
             EmptyDataText="There are no billings that meet the criteria you specified." 
                Width="100%" 
                SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
                SortOrderDescendingImagePath="~/images/icon_sort_descending.gif" 
                onselectedindexchanging="gvUpcomingBillings_SelectedIndexChanging">
                
             <AlternatingRowStyle CssClass="Alt" />
                
             <Columns>
                   <asp:TemplateField HeaderText="" ItemStyle-HorizontalAlign="Center">
                        <HeaderTemplate>
                            <asp:CheckBox ID="SelectAll" runat="server" Checked="false"/>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="cbRow" runat="server" Checked='<%# Bind("Process") %>'  />                            
                            <asp:HiddenField runat="server" ID="HidACCID"  Value='<%# Eval("AccountID") %>' />
                       </ItemTemplate>
                    
                        <ItemStyle HorizontalAlign="Left" />
                    </asp:TemplateField>
                                        
                    <asp:BoundField DataField="AccountID" HeaderText="Acc#" ReadOnly="True" 
                        SortExpression="AccountID" />
                    
                    <asp:BoundField DataField="AccountName" HeaderText="Account Name" 
                        SortExpression="AccountName" />
                    <asp:BoundField DataField="WhenToBill" HeaderText="Renew On" 
                        ReadOnly="True" SortExpression="WhenToBill" DataFormatString="{0:d}" />
                    
                    <asp:BoundField DataField="BillingMethod" HeaderText="Method" 
                        ReadOnly="True" SortExpression="BillingMethod" />
                    
                    <asp:BoundField DataField="ContractEndDate" HeaderText="Contract Ends" 
                        SortExpression="ContractEndDate" DataFormatString="{0:d}" />
                    
                    <asp:BoundField DataField="CustomerType" HeaderText="Type" 
                        SortExpression="CustomerType" />
                    
                    <asp:BoundField DataField="LastBilled" HeaderText="Last Billed" 
                        SortExpression="LastBilled" DataFormatString="{0:d}" />

            </Columns>
             <PagerSettings Mode="NextPreviousFirstLast" />
             <PagerStyle />
             <RowStyle CssClass="Row" />
             <SelectedRowStyle CssClass="Row Selected" />
            </ec:GridViewEx>

            <div style="text-align: right; padding: 10px;">
                    <asp:Button ID="btnGenerate" runat="server" Text="Generate Selected Billings" 
                            onclick="btnGenerate_Click" CssClass="OButton" />
            </div>                  
      
      </div>
        
    </asp:Panel>

    <asp:Panel runat="server" ID="pnlReview" Visible="false">
        
        <%--<div class="title">
            <h2>Renewal#: <asp:Label ID="lblRenewalNo" runat="server" /></h2>
        </div>--%>

        <div style="width: 100%">
            
            <asp:HiddenField runat="server" ID="hfRenewalNo" Value="0" />
            
            <div>
                <asp:Button Text="&lt;&lt; Back" runat="server" CssClass="OButton" OnClick="btnBack_Click" ID="btnBack" /> 
                <h3>Scheduled Billings Review</h3>
            </div>
                  
            <ec:GridViewEx ID="gvReview" runat="server" AutoGenerateColumns="false" CssClass="OTable"
                 AlternatingRowStyle-CssClass="Alt" PagerStyle-CssClass="mt-hd" 
                 EmptyDataText="There are no valid billings orders processed."  Width="100%">
                  <Columns>
                    <asp:BoundField DataField="AccountID" HeaderText="Acc#" ReadOnly="True" SortExpression="AccountID" />
                    <asp:BoundField DataField="AccountName" HeaderText="Account Name" SortExpression="AccountName" />
                    <asp:BoundField DataField="WhenToBill" HeaderText="Renew On" ReadOnly="True" SortExpression="WhenToBill" DataFormatString="{0:d}" />
                    <asp:BoundField DataField="BillingMethod" HeaderText="Method" ReadOnly="True" SortExpression="BillingMethod" />
                    <asp:BoundField DataField="ContractEndDate" HeaderText="Contract Ends" SortExpression="ContractEndDate" DataFormatString="{0:d}" />
                    <asp:BoundField DataField="CustomerType" HeaderText="Type" SortExpression="CustomerType" />
                    <asp:BoundField DataField="LastBilled" HeaderText="Last Billed" SortExpression="LastBilled" DataFormatString="{0:d}" />
                    <asp:BoundField DataField="Result" HeaderText="Status" />                                  
                </Columns>
            </ec:GridViewEx>      
        </div>
    </asp:Panel>

    <asp:Panel runat="server" ID="pnlCompleted" Visible="false">
        <div style="padding:10px;">
            <asp:Button Text="&lt;&lt; Back" runat="server" OnClick="btnBack_Click" ID="btnBack2" /> <br />
            <span style="font-weight:bold; font-size: 13pt;">Renewal Status</span>
        </div>
        
        <asp:Label ID="lblProcessingErrors" Text="" Visible="false" runat="server" CssClass="errorBox" style="padding:10px;" />
        
        <div class="review">
          <div class="review_container">
            <asp:Panel ID="respMessage" CssClass="message1" runat="server">Results</asp:Panel>
            <asp:Panel ID="respMessage2" CssClass="message2" runat="server">Order was not generated.</asp:Panel>
            <asp:Panel ID="respOrder" CssClass="response_error" runat="server">The order was not created.</asp:Panel>
            <asp:Panel ID="respPORequest" CssClass="response_error" runat="server">The PO request was not created.</asp:Panel>
            <asp:Panel ID="respPayment" CssClass="response_error" runat="server">The payment has not been processed.</asp:Panel>
            <asp:Panel ID="respUpdateAccount" CssClass="response_error" runat="server">The account contract and billings have not been created.</asp:Panel>
            <asp:Panel ID="respMAS" CssClass="response_error" runat="server">The order was not sent to MAS.</asp:Panel>
            <asp:Panel ID="respSoftrax" CssClass="response_error" runat="server">The order was not sent to Softrax.</asp:Panel>
          </div>
        </div>
    </asp:Panel>
    
    </div>
  </ContentTemplate>
</asp:UpdatePanel>

</asp:Content>

