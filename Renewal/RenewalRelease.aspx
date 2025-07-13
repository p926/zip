<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Finance_Renewal_RenewalRelease" EnableEventValidation ="false"  Codebehind="RenewalRelease.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
  
    <style type="text/css">  

      #overlay {
        background-color: black;
        position: fixed;
        top: 0; right: 0; bottom: 0; left: 0;
        opacity: 0.2; /* also -moz-opacity, etc. */
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
    </style>
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:ScriptManager runat="server" id="mainScriptManager"></asp:ScriptManager>

        <div class="FormError" id="plErrorMessage" runat="server" visible="false">
            <p><span class="MessageIcon"></span></p>
            <strong>Messages:</strong>
            <span id="lblRenewalRowMessage" runat="server" >An error was encountered.</span>
                
            <asp:BulletedList ID="blstErrors" runat="server" BulletStyle="Disc">
            </asp:BulletedList>
    
        </div>
        <div id="FormMsg" class="FormMessage" runat="server" visible="false">
		    <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong>&nbsp;<span id="MsgText" runat="server" >Submission of information has been successful.</span></p>
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
        EmptyDataText="There are no renewal orders for release." 
        PagerStyle-CssClass="mt-hd" Width="100%">
        <AlternatingRowStyle CssClass="Alt" />
        <Columns>
            <asp:TemplateField HeaderText="" ItemStyle-HorizontalAlign="Center">
                <HeaderTemplate>
                    <asp:CheckBox ID="SelectAll" runat="server" Checked="false" />
                </HeaderTemplate>
                <ItemTemplate>
                    <asp:CheckBox ID="cbRow" runat="server" Checked='<%# Bind("Process") %>' />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Order#" ItemStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    <asp:ImageButton ID="imgBtnDelete" runat="server" 
					    ImageUrl="~/css/dsd-default/images/icons/delete.png" 
						CommandName="DELETE" AlternateText="Delete Order"
						OnClick="imgBtnDelete_Click" CommandArgument='<%# Eval("OrderNo") %>' />

                    <asp:HyperLink ID="hlOrderLink" runat="server" 
                        NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"OrderNo","../../CustomerService/CreateOrder.aspx?OrderID={0}" ) %>' 
                        Target="_blank" Text='<%# Bind("OrderNo") %>'></asp:HyperLink>
                    <asp:HiddenField runat="server" ID="HidORDID"  Value='<%# Eval("OrderNo") %>' />
                    <act:ConfirmButtonExtender ID="ConfirmButtonExtender1" runat="server" 
                                 TargetControlID="imgBtnDelete"
                                 ConfirmText="Are you sure want to delete this order?">
                    </act:ConfirmButtonExtender> 
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Acc#" ItemStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    <asp:HyperLink ID="hlAccountLink" runat="server" 
                        NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"AccountNo","../../InformationFinder/Details/Account.aspx?ID={0}" ) %>' 
                        Target="_blank" Text='<%# Bind("AccountNo") %>'></asp:HyperLink>
                     <asp:HiddenField runat="server" ID="HidACCID"  Value='<%# Eval("AccountNo") %>' />
                     <asp:HiddenField runat="server" ID="hidLocationID"  Value='<%# Eval("LocationID") %>' />
   
                </ItemTemplate>  
            </asp:TemplateField>
            <asp:BoundField DataField="AccountName" 
                HeaderText="Account Name">
            </asp:BoundField>
            <asp:BoundField DataField="LocationName" 
                HeaderText="Location">
            </asp:BoundField>
             <asp:BoundField DataField="RenewalNo" 
                HeaderText="Renewal#">
            </asp:BoundField>
            <asp:BoundField DataField="BrandName" HeaderText="Brand" ItemStyle-HorizontalAlign="Left">
            <ItemStyle HorizontalAlign="Left" />
            </asp:BoundField>
            
            <asp:BoundField DataField="BillingTermDesc" HeaderText="Term">
            </asp:BoundField>
                       
            <asp:BoundField DataField="PaymentType" HeaderText="Type" ItemStyle-HorizontalAlign="Left">
             </asp:BoundField>
             <asp:TemplateField HeaderText="Amount"
				Visible="true">
				<ItemTemplate>
                    <div class="LabelValue;RightAlignItemText">
                    <asp:Label runat="server" ID="lblAmount" 
                        Width="175px" Text='<%# DataBinder.Eval(Container.DataItem,"RenewalAmount","" ) %>' /></div> 
							
				</ItemTemplate>
                <ControlStyle CssClass="RightAlignItemText"> </ControlStyle>
                <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left"></HeaderStyle>
				<ItemStyle CssClass="RightAlignItemText"></ItemStyle>
			</asp:TemplateField>
            	          
        </Columns>
        <PagerStyle />
    </asp:GridView>
              

    <div style="text-align: right;  padding: 10px;">
        <asp:Button ID="btnReleaseOrders" runat="server" Text="Release Orders" 
                onclick="btnReleaseOrders_Click" CssClass="OButton" />
    </div>

                </asp:Panel>
    
        <asp:Panel runat="server" ID="pnlReview" Visible="false">
            <div style="width: 100%">
            
            <asp:HiddenField runat="server" ID="hfRenewalNo" Value="0" />
            
            <div>
                <asp:Button Text="&lt;&lt; Back" runat="server" CssClass="OButton" OnClick="btnBack_Click" ID="btnBack" /> <br />
                <h3>Renewal Release Review</h3>
            </div>
                  
            <ec:GridViewEx ID="gvReview" runat="server" AutoGenerateColumns="false" CssClass="OTable"
                 AlternatingRowStyle-CssClass="Alt" PagerStyle-CssClass="mt-hd" 
                 EmptyDataText="There are no valid renewal hold orders released."  Width="100%">           
                 <Columns>
                    <asp:BoundField DataField="AccountID" HeaderText="Acc#" />
                    <asp:BoundField DataField="AccountName" HeaderText="Account Name" />
                    <asp:BoundField DataField="OrderStatusName" HeaderText="Status" />
                    
                    <asp:TemplateField HeaderText="Amount"
				        Visible="true">
				        <ItemTemplate>
                            <asp:HiddenField runat="server" ID="HidORDID"  Value='<%# Eval("OrderID") %>' />
                            <div class="LabelValue;RightAlignItemText">
                            <asp:Label runat="server" ID="lblAmount" 
                                Width="150px" Text='<%# DataBinder.Eval(Container.DataItem,"RenewalAmount","" ) %>' /></div> 
							
				        </ItemTemplate>
                        <ControlStyle CssClass="RightAlignItemText"> </ControlStyle>
                        <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left"></HeaderStyle>
				        <ItemStyle CssClass="RightAlignItemText"></ItemStyle>
			        </asp:TemplateField>
                    
                    <asp:BoundField DataField="BrandName" HeaderText="Brand" />
                    <asp:BoundField DataField="BillingTermDesc" HeaderText="Term" />
                    <asp:BoundField DataField="PaymentMethodName" HeaderText="Method" />
                    <asp:BoundField DataField="ErrorMessage" HeaderText="Error"></asp:BoundField>
                                             
                </Columns>
            </ec:GridViewEx>
       
        </div>
        </asp:Panel>
</asp:Content>