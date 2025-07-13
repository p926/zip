<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Finance_CreditMemo" Codebehind="CreditMemo.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        /* CSS Definition for Label Values in Modal/Dialog. */
        .StaticLabel 
        {
            color: #000000; 
            text-align: left; 
            font-weight: normal;
            display: block;
            padding: 3px;
            margin: 2px 0;
        }
    </style>
    <script type="text/javascript">

          //on page load
          $(document).ready(function () {
              Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(showProgress);
              Sys.WebForms.PageRequestManager.getInstance().add_endRequest(hideProgress);
              Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ajaxLoad);
              ajaxLoad();

          });

          function showProgress(sender, args) {
              var c = '<%=modalExtender.ClientID %>';
              $find(c).show();
          }

          function hideProgress(sender, args) {
              var c = '<%=modalExtender.ClientID %>';
              $find(c).hide();
          }

          function ajaxLoad() {

              $("#tabsContainer").tabs();

              // ServiceStartDate Datepicker
              $('#<%=txtStartDate.ClientID %>').datepicker({
                  changeMonth: true,
                  changeYear: true
              });

              // ServiceEndDate Datepicker
              $('#<%=txtEndDate.ClientID %>').datepicker({
                  changeMonth: true,
                  changeYear: true
              });

              // ServiceStartDate Datepicker in GridView
              $("[id$=txtGVStartDate]").datepicker({
                  changeMonth: true,
                  changeYear: true
              });

              // ServiceEndDate Datepicker in GridView
              $("[id$=txtGVEndDate]").datepicker({
                  changeMonth: true,
                  changeYear: true
              });

              // Basic Credit Calculation. 
              $('.txtCredit').blur(function () {
                  var cost = 0;
                  var CreditItem = $('#<%=txtCreditItem.ClientID%>').val();
                  var CreditMisc = 0;
                  var CreditShipping = $('#<%=txtCreditShipping.ClientID%>').val();

                  if (!isNaN(CreditItem) && CreditItem != "")
                      cost += parseFloat(CreditItem);

                    if (!isNaN(CreditMisc) && CreditMisc != "")
                        cost += parseFloat(CreditMisc);

                  if (!isNaN(CreditShipping) && CreditShipping != "")
                      cost += parseFloat(CreditShipping);

                  $('#<%=txtTotalCredit.ClientID%>').val(cost.toFixed(2));

              });


              //Service Daet
              $('#<%=chkboxAllDate.ClientID%>').click(function () {
                  if ($('#<%=chkboxAllDate.ClientID%>').is(":checked")) {
                      var StartDate = $('#<%=txtStartDate.ClientID%>').val();
                      $('.classtxtStartDate').val(StartDate);
                      var EndDate = $('#<%=txtEndDate.ClientID%>').val();
                      $('.classtxtEndDate').val(EndDate);

                  };
              });

              // selected serialno boxes.
              $('.chkbxHeadercc').click(function () {
                  var ischecked = ($('#ctl00_primaryHolder_gvOrderInvoiceDetails_ctl01_chkbxHeader').is(":checked"));
                  // add attribute 
                  $('#ctl00_primaryHolder_gvOrderInvoiceDetails input:checkbox').attr("checked", function () {
                      this.checked = ischecked;
                  });
              });


          }

    </script> 
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <%--PROGRESS BAR--%>
    <asp:UpdateProgress id="LoadingProgress" runat="server" DynamicLayout="true" DisplayAfter="0" >
        <ProgressTemplate>            
        </ProgressTemplate>
    </asp:UpdateProgress>

    <act:ModalPopupExtender ID="modalExtender" runat="server" TargetControlID="LoadingProgress"
        PopupControlID="Panel1" BackgroundCssClass="modalBackground" Enabled="true" >
    </act:ModalPopupExtender>

    <asp:Panel ID="Panel1" runat="server">
        <div style="width: 100%" align="center">
            <img src="../images/orangebarloader.gif" />
        </div>
    </asp:Panel>
    <%--END PROGRESS BAR--%>

    <%--BASIC CREDIT MEMO DETAILS--%>
    <div class="ui-widget ui-widget-content">   
        <asp:FormView Width="100%" ID="fvAccountDetails" runat="server" DataKeyNames="AccountID" DataSourceID="sqlAccount">
            <ItemTemplate>
                <table class="OTable" style="width: 100%; border: 0; margin: 0;">
                    <tr>
                        <td style="width: 120px" class="Label">Company:</td>
                        <td>
                            <asp:Hyperlink ID="AccountNoLabel" runat="server" Text='<%# Bind("Companyname") %>' NavigateUrl='<%# Eval("AccountID", "~/InformationFinder/Details/Account.aspx?ID={0}#Invoice_tab") %>' />
                        </td>
                        <td style="width: 120px" class="Label">Quarterly Price:</td>
                        <td>
                            <asp:Label ID="lblQuarterlyPrice" runat="server" Text='<%# Eval("PriceQuarterly", "{0:n}") %>' />&nbsp;USD
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 120px" class="Label">Name:</td>
                        <td>
                            <asp:Label ID="lblAccountName" runat="server" Text='<%# Bind("AccountName") %>' />
                        </td>
                        <td style="width: 120px" class="Label">Yearly Price:</td>
                        <td>
                            <asp:Label ID="lblYearlyPrice" runat="server" Text='<%# Eval("PriceYearly", "{0:n}") %>' />&nbsp;USD
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 120px" class="Label">Account #:</td>
                        <td>
                            <asp:Label ID="lblAccountID" runat="server" Text='<%# Eval("AccountID") %>' />
                        </td>
                        <td style="width: 120px" class="Label">Cancellation Date:</td>
                        <td>
                            <asp:Label ID="lblCancelledDate" runat="server" Text='<%# Eval("StopServiceDate") %>' />
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 120px" class="Label">Active:</td>
                        <td>
                            <asp:Label ID="lblActive" runat="server" Text='<%# YesNo(Eval("Active")) %>' CssClass='<%# Eval("Active", "lblActive{0}") %>' />
                        </td>
                        <td style="width: 120px" class="Label">Contact Name:</td>
                        <td>
                            <asp:Label ID="lblContactName" runat="server" Text='<%# Bind("ContactName") %>' />
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 120px" class="Label">Created:</td>
                        <td>
                            <asp:Label ID="lblCreated" runat="server" Text="" />
                        </td>
                        <td style="width: 120px" class="Label">Cut-Off Date:</td>
                        <td>
                            <asp:Label ID="lblCutoffDate" runat="server" Text="" />
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 120px" class="Label">Referral Code:</td>
                        <td>
                            <asp:Label ID="lblReferralCode" runat="server" Text='<%# Bind("ReferralCode") %>' />
                        </td>
                        <td style="width: 120px" class="Label">Test Account:</td>
                        <td>
                            <asp:Label ID="lblTestAccount" runat="server" Text="" /> 
                        </td>
                    </tr>
                    <tr>
                        <td style="width: 120px" class="Label">Customer Type:</td>
                        <td>
                            <asp:Label ID="lblCustomerType" runat="server" Text="" />
                        </td>
                        <td style="width: 120px" class="Label">Industry:</td>
                        <td>
                            <asp:Label ID="lblIndustry" runat="server" Text="" />
                        </td>
                    </tr>
                </table>   
            </ItemTemplate>
            <EmptyDataTemplate>
                <div class = "NoData">
                    There are no records found! Please enter a valid Account #.
                </div>
            </EmptyDataTemplate>
        </asp:FormView>          
    </div>
    <%--END - BASIC CREDIT MEMO DETAILS--%>

    <div>&nbsp;</div>    
    
    <%--TAB CONTROL--%>
    <div id="tabsContainer">
        <%--TABS--%>
        <ul>				
            <li><a href="#Invoice_tab" id="InvoiceTabHeader" runat ="server">Invoices</a></li>	
            <li><a href="#ProcessCreditMemo_tab" id="ProcessCreditMemoTabHeader" runat ="server">Credit Memo Process</a></li>						            
        </ul>
        <%--END - TABS--%>

        

        <%--INVOICE TABS AREA--%>
        <div id="Invoice_tab">    
            <asp:UpdatePanel ID="upnlInvoice" runat="server"  UpdateMode="Conditional">               
                <ContentTemplate>
                    <asp:GridView ID="gvInvoices" runat="server" CssClass="OTable"
                    AutoGenerateColumns="False"  >
                        <Columns>
                            <asp:TemplateField HeaderText="Order#">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hyprlnkOrderNumber" NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"OrderId","~/CustomerService/CreateOrder.aspx?OrderID={0}" ) %>' Text='<%# DataBinder.Eval(Container.DataItem,"OrderId","{0}" ) %>' runat="server"></asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Qty#">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblOrderCount" Text='<%#DisplayQtyCountText(Eval("OrderId")) %>' ></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="InvoiceNo" HeaderText="Invoice#"  />
                            <asp:BoundField DataField="InvoiceDate" DataFormatString="{0:d}" HeaderText="Invoice Date"  />
                            <asp:TemplateField ShowHeader="true" HeaderText="Amount" >
                                <ItemTemplate>
                                    <asp:Label ID="lblAmountAndCurrency" runat="server" Text='<%# Eval("Amount", "{0:n}") %>'>
                                    </asp:Label><%--&nbsp;USD --%>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="invoiceType" HeaderText="Invoice Type" SortExpression="invoiceType"  />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class = "NoData">
                                There are no Invoices associated with this Account #.
                            </div>
                        </EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
                        <PagerStyle CssClass="Footer" />
                    </asp:GridView>  
                </ContentTemplate> 
            </asp:UpdatePanel>                                                                                                                                          
        </div>
        <%--END - INVOICE TAB AREA--%>

        <%--CREDIT MEMO PROCESS TAB AREA--%>
        <div id="ProcessCreditMemo_tab">
            <asp:UpdatePanel ID="upnlCreditMemo" runat="server"  UpdateMode="Conditional">               
                <ContentTemplate>

                    <%--ERROR MESSAGE AREA--%>
                    <div id="divErrors" class="FormError" runat="server" visible="true">
		                <p><span class="MessageIcon"></span>
		                <strong>Messages:</strong>&nbsp;<span id="errorMsg" runat="server" >An error was encountered.</span></p>
                    </div>
                    <%--END - ERROR MESSAGE AREA--%>

                    <div id="divCreditMemoDetail" runat="server" visible="true">
                        <%--CREDIT MEMO PROCESS - SEARCH TOOLBAR--%>
                        <div class="OToolbar JoinTable" id="CreateNote" runat ="server">
                            <ul>
						        <li>
                                    <asp:RadioButton ID="rbtnCreditMemo" Text="Credit Memo" 
                                    runat="server"  GroupName="rbtCreditMemoType" Checked="true" 
                                    ToolTip ="This is a credit memo for badges that has been returned and received. Credit data submit to MAS and Softrax"
                                    oncheckedchanged="rbtnCreditMemo_CheckedChanged" AutoPostBack="true" />
                                </li>

                                <li>
                                    <asp:RadioButton ID="rbtnCreditAndRebill" Text="Credit and Rebill" 
                                    runat="server"  GroupName="rbtCreditMemoType" 
                                    ToolTip ="This is a credit memo in full and the badges are back in INV. Credit data will submit to MAS and Softrax"
                                    oncheckedchanged="rbtnCreditAndRebill_CheckedChanged" AutoPostBack="true" />
                                </li>

                                <li>    
                                    <asp:RadioButton ID="rbtnPriceMatch" Text="Price Match" 
                                    runat="server"  GroupName="rbtCreditMemoType" 
                                    ToolTip="This is a credit memo for Price Adjustments. Credit data submit to MAS and Softrax"
                                    oncheckedchanged="rbtnPriceMatch_CheckedChanged" AutoPostBack="true" />
                                </li>

                                <li>    
                                    <asp:RadioButton ID="rbtnCreditWithBadges" Text="Credit Non-Returned Badges" 
                                    runat="server"  GroupName="rbtCreditMemoType" 
                                    ToolTip="This is a credit memo for Non-Returned badges. Credit data submit to MAS"
                                    oncheckedchanged="rbtnCreditWithBadges_CheckedChanged" AutoPostBack="true" />
                                </li>

                                <li>   
                                    <asp:RadioButton ID="rbtnBadDebt" Text="Bad Debt" runat="server"  
                                    GroupName="rbtCreditMemoType" 
                                    ToolTip="This is a credit for bad debt/write off and the badges are not returned. Credit data submit to MAS"
                                    oncheckedchanged="rbtnBadDebt_CheckedChanged"  AutoPostBack="true"   />
                                </li>

                                <li>   
                                    <asp:RadioButton ID="rbtnFreight" Text="Freight" runat="server"  
                                    GroupName="rbtCreditMemoType"
                                    ToolTip="This is a credit/write off for shipping charge only. Credit data submit to MAS"
                                    oncheckedchanged="rbtnFreight_CheckedChanged"  AutoPostBack="true"   />
                                </li>                                                                
                                
                            </ul>
                        </div>
                        <%--END - SEARCH TOOLBAR--%>
                        <%--GENERAL CREDIT MEMO INFORMATION--%>
                        <table class="OTable JoinTable" runat="server" cellpadding="0" cellspacing="0">
                            <tr>
                                <td colspan="3">
                                    <span id="creditTypeDesc" runat="server" style="margin-left:22px; font-style:italic; font-size:smaller;"></span>
                                </td>
                            </tr>
                            <tr>
                                <td >
                                    <div class="OForm">
                                        <div class="Row">   
                                            <div class="Label Large">Select Invoice to Credit:</div>
                                            <div class="Control">
                                                <asp:DropDownList ID="ddlInvoice" runat="server"  Enabled = "true"
                                                DataValueField="InvoiceNo" DataTextField="Invoice" AutoPostBack = "true" 
                                                onselectedindexchanged="ddlInvoice_SelectedIndexChanged" />	                                                                
                                            </div>
                                            <div class="Clear"></div>
                                        </div>
                                        <div class="Row">   
                                            <div class="Label Large">Service Start Date:</div>
                                            <div class="Control">
                                                <asp:TextBox  ID="txtStartDate" runat="server" CssClass="Size Small" ></asp:TextBox>
                                                <%--<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*Date Required " 
                                                Display="Dynamic" ValidationGroup="ValidForm"  
                                                ControlToValidate="txtStartDate"></asp:RequiredFieldValidator>      --%>                                     
                                            </div>
                                            <div class="Clear"></div>
                                        </div>
                                        <div class="Row">   
                                            <div class="Label Large">Service End Date:</div> 
                                            <div class="Control">
                                                <asp:TextBox  ID="txtEndDate" runat="server" CssClass="Size Small" ></asp:TextBox>
                                                <%--<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*Date Required " 
                                                Display="Dynamic" ValidationGroup="ValidForm" 
                                                ControlToValidate="txtEndDate"></asp:RequiredFieldValidator>      --%>                                      
                                            </div>
                                            <div class="Clear"></div>
                                        </div>                                        
                                        <div class="Row">   
                                            <div class="Label Large" >Apply Date to All Devices:</div>
                                            <div class="Control">
                                                <asp:CheckBox ID="chkboxAllDate" runat="server" />
                                            </div>                                                                                                           
                                            <div class="Clear"></div>
                                        </div>
                                        <div class="Row">   
                                            <div class="Label Large">Comment:</div>
                                            <div class="Control">
                                                <asp:TextBox  ID="txtComment" runat="server" CssClass="Size Large2 " ></asp:TextBox>                                                                                
                                            </div>
                                            <div class="Clear"></div>
                                        </div>
                                    </div>
                                </td>
                                <td width="10">&nbsp;</td>
                                <td >
                                    <div class="OForm">                                    
                                        <div class="Row">
                                            <div class="Label">Item Credit:</div>
                                            <div class="Control" runat="server">
                                                <asp:TextBox  ID="txtCreditItem" runat="server" 
                                                    style="text-align:right" Text="0.00"  
                                                    Width="100" class="txtCredit" />                                                                
                                            </div>
                                            <div class="Control">
                                                <asp:Label ID="lblItemCreditUSD" runat="server" class="StaticLabel" Text="USD"></asp:Label>
                                            </div>
                                            <div class="Clear"></div>                          
                                        </div>
                                        <%--<div class="Row" >  
                                            <div class="Label ">Miscellaneous Credit:</div>
                                            <div class="Control">
                                                <asp:TextBox  ID="txtCreditMisc" runat="server" 
                                                    style="text-align:right" Text="0.00" 
                                                    Width="100" class="txtCredit" />
                                            </div>
                                            <div class="Control">
                                                <asp:Label ID="lblMiscellaneousCreditUSD" runat="server" class="StaticLabel" Text="USD"></asp:Label>
                                            </div>
                                            <div class="Clear"></div>                                         
                                        </div>--%>
                                        <div class="Row">  
                                            <div class="Label ">Shipping Credit:</div>
                                            <div class="Control">
                                                <asp:TextBox  ID="txtCreditShipping" runat="server" 
                                                    style="text-align:right" Text="0.00" 
                                                   Width="100" class="txtCredit"  />
                                            </div>
                                            <div class="Control">
                                                <asp:Label ID="lblShippingCreditUSD" runat="server" class="StaticLabel" Text="USD"></asp:Label>
                                            </div>
                                            <div class="Clear"></div>                                          
                                        </div>
                                        <div class="Row"> 
                                            <div class="Label ">Total Credit:</div>
                                            <div class="Control">
                                                <asp:TextBox ID="txtTotalCredit" ReadOnly="true" runat="server"  
                                                    CssClass="Size Small" style="text-align:right" Text="0.00" />
                                            </div>
                                            <div class="Control">
                                                <asp:Label ID="lblTotalCreditUSD" runat="server" class="StaticLabel" Text="USD"></asp:Label>
                                            </div>
                                            <div class="Clear"></div>                                           
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <%--END - GENERAL CREDIT MEMO INFORMATION--%>
                        <%--CREDIT MEMO PROCESS GRIDVIEW--%>
                        <table width="100%" border="0" cellpadding="0" cellspacing="0" style="margin-top: -21px;">
                            <tr >
                                <td colspan="3">                                
                                    <asp:GridView ID="gvOrderInvoiceDetails" runat="server" AutoGenerateColumns="False" 
                                    DataSourceID="sqlOrderInvoiceDetails" CssClass="OTable"  DataKeyNames="LotSerialNo"
                                    AllowSorting="true" OnRowDataBound="gvOrderInvoiceDetails_RowDataBound">
                                        <Columns>
                                            <asp:TemplateField >
                                                <HeaderTemplate>
                                                    <asp:CheckBox runat="server" ID="chkbxHeader" ToolTip="Select ALL Serial#" class="chkbxHeadercc" />
                                                </HeaderTemplate>
                                                <HeaderStyle Width="20px" Wrap="false" />
                                                <ItemTemplate>
                                                <asp:CheckBox id="chkbxSelect" runat="server" class="chkbxRow" />
                                                <asp:HiddenField runat="server" ID="HidProductID"  Value='<%# Eval("ProductID") %>' />
                                                <asp:HiddenField runat="server" ID="HidDeviceID"  Value='<%# Eval("DeviceID") %>' />
                                                <asp:HiddenField runat="server" ID="HidCreditRate" Value='<%#calculateRate(DataBinder.Eval(Container.DataItem,"ServiceStart","{0}" ),DataBinder.Eval(Container.DataItem,"ServiceEnd","{0}" ), DataBinder.Eval(Container.DataItem,"Price","{0}" )) %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="InvoiceNo" HeaderText="Invoice#" SortExpression="InvoiceNo" Visible="false" />
                                            <asp:BoundField DataField="InvoiceDD" HeaderText="Invoice Date" SortExpression="InvoiceDate" Visible="false"  DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="LotSerialNo" HeaderText="Serial#" SortExpression="LotSerialNo" ItemStyle-Font-Bold="true" />
                                            <asp:BoundField DataField="SKU" HeaderText="SKU" SortExpression="SKU" />

                                            <asp:TemplateField Visible="true" SortExpression="StartDate">
                                                <HeaderTemplate >Service Start</HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:TextBox ID ="txtGVStartDate" runat="server" Width="85px"  Text ='<%# DataBinder.Eval(Container.DataItem,"ServiceStart","{0:d}" ) %>' class="classtxtStartDate" />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <asp:TemplateField Visible="true" SortExpression="EndDate">
                                                <HeaderTemplate >Service End</HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:TextBox ID ="txtGVEndDate" runat="server" Width="85px" Text ='<%# DataBinder.Eval(Container.DataItem,"ServiceEnd","{0:d}" ) %>' class="classtxtEndDate" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                                                                        
                                            <asp:BoundField DataField="inRMA" HeaderText="Device Returned?" SortExpression="inRMA" />
                                            
                                            <asp:TemplateField ShowHeader="true" HeaderText="Assigning Warehouse" SortExpression="">
                                                <ItemTemplate>
                                                    <asp:RadioButtonList ID="rbtnlstWHS" runat="server" RepeatColumns="3" Enabled ="false" 
                                                    Visible='true' ToolTip ="Assigning Warehouse" CellPadding="0" CellSpacing="0">
                                                    <asp:ListItem id="INV" Value="INV" runat="server" />
                                                    <asp:ListItem id="SCP" Value="SCP" runat="server" />
                                                    <asp:ListItem id="NRB" Value="NRB" runat="server" />
                                                    </asp:RadioButtonList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <div class = "NoData">
                                                There are no devices associated with this invoice.
                                            </div>
                                        </EmptyDataTemplate>
                                        <AlternatingRowStyle CssClass="Alt" />
                                        <PagerStyle CssClass="Footer" />
                                    </asp:GridView>                                  
                                </td>
                            </tr>
                        </table>
                    </div>
                    <%--END - CREDIT MEMO PROCESS GRIDVIEW--%>

                    <%--CONFIRM CREDIT & BACK TO ACCOUNT SEARCH BUTTONS--%>
                    <div class="Buttons" style="padding-right: 0px;">
                        <div class="ButtonHolder">
                            <asp:Button ID="btnConfirm" runat="server" Text="Confirm Credit" CssClass="OButton" onclick="btnConfirm_Click"  />
                            <asp:Button ID="btnCancel" runat="server" Text="Back to Account Information" 
                                CssClass="OButton" ToolTip="Back to Account Details" onclick="btnCancel_Click" />       
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END - CONFIRM CREDIT & BACK TO ACCOUNT SEARCH BUTTONS--%>

                    <div  id="divCreditMemoResult" runat="server" visible="false">

                        <table  class="OTable" width="100%" border="1" >

                            <tr>
                                <td colspan="3" align="right"  >
                                    <asp:Button ID="btnBack" runat="server" Text="Back to Credit Memo Process" CssClass="OButton " 
                                        onclick="btnBack_Click" />
                                    <asp:Button ID="btnCreditMemoSearch" runat="server" Text="Search Credit Memo Search" 
                                        CssClass="OButton " onclick="btnCreditMemoSearch_Click" /> 
                                </td>
                            </tr>
                            <tr >
                                <td width="275" valign="top"  >
                                    <b><u>Credit Memo Order </u></b><br />
                                    <br />
                                    Account#: <asp:Label ID="lbl_CM_accountid" runat="server" Text="" Font-Bold="true" /><br /> 
                                    Credit Memo Order#: <asp:Label ID="lbl_CM_orderid" runat="server" Text="" Font-Bold="true" />
                                    <hr />
                                    Item Credit: <asp:Label ID="lbl_CM_itemCredit" runat="server" Text=""  Font-Bold="true" /><br />
                                    Miscellaneous Credit: <asp:Label ID="lbl_CM_misCredit" runat="server" Text=""  Font-Bold="true" /><br />
                                    Shipping Credit:  <asp:Label ID="lbl_CM_ShippingCredit" runat="server" Text="" Font-Bold="true" /><br />
                                    Total Credit:  <asp:Label ID="lbl_CM_totalCredit" runat="server" Text="" Font-Bold="true" />
                                    <hr />
                                    Softrax process: <asp:Label ID="lbl_CM_softwaxProcess" runat="server" Text="" Font-Bold="true" />
                                </td>
                                <td width="100" valign="top"  />
                                <td align="center" valign="top">
                                    <b><u>Credit Memo Order Item Detail</u></b><br />
                                    <asp:Label ID="lbl_CM_OrderDetail" runat="server" Text="" />
                                </td>
                            </tr>
              
                        </table>

                    </div>
                        

                    </ContentTemplate>
                </asp:UpdatePanel>                                               
            </div>
	   
        </div>
    <%--END TAB CONTROL--%>
  
    <%--GET SQL DATA SOURCE--%>
    <asp:SqlDataSource ID="sqlAccount" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_if_GetAccountByNo" SelectCommandType="StoredProcedure">
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="AccountNo" QueryStringField="ID"
                Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlInvoices" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.MASConnectionString %>"
        SelectCommand="sp_if_GetAllInvoiceHistoryByAccountNo" SelectCommandType="StoredProcedure">
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="AccountNo" QueryStringField="ID"
                Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    
    <%--<asp:SqlDataSource ID="sqlOrderInvoiceDetails" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_if_GetInvoiceLotSerialByAccountID" 
        SelectCommandType="StoredProcedure">
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="ID" QueryStringField="ID" 
                Type="Int32" /> 
            <asp:ControlParameter ControlID="ddlInvoice" DefaultValue="" 
                        Name="InvoiceNo" PropertyName="SelectedItem.Value" Type="String" />           
        </SelectParameters>
    </asp:SqlDataSource>--%>

    <asp:SqlDataSource ID="sqlOrderInvoiceDetails" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_if_GetOrderInvoiceRmaByOrderNo" 
        SelectCommandType="StoredProcedure">
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="ID" QueryStringField="ID" 
                Type="Int32" />
            <asp:ControlParameter ControlID="ddlInvoice" DefaultValue="" 
                        Name="InvoiceNo" PropertyName="SelectedItem.Value" Type="String" /> 
        </SelectParameters>
    </asp:SqlDataSource>

    <%--END GET SQL DATA SOURCE--%>                  
    
</asp:Content>

