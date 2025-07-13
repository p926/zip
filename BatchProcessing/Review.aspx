<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_Admin_BatchProcessing_Review" Codebehind="Review.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">


    <style type="text/css">
        
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
  var PaymentsNotRequired = false;
  var SelectAllID = "#ctl00_primaryHolder_gvReviewOrders_ctl01_SelectAll";
  var cbPaymentsNotRequiredID = "#ctl00_cpBody_gvReviewOrders_ctl01_cbPaymentsNotRequired";
  var LogVisible = false;
  $(document).ready(function() {
    $('#ToggleLogDetails').click(function() {
      if (LogVisible) {
        $('#LogDetails').hide('normal');
        $('#ToggleLogDetails').text('Show Log Details');
        LogVisible = false;
      }
      else {
        $('#LogDetails').show('normal');
        $('#ToggleLogDetails').text('Hide Log Details');
        LogVisible = true;
      }
      $(this).attr("checked") = PaymentsNotRequired;
    });
    
    $(cbPaymentsNotRequiredID).click(function() {
      PaymentsNotRequired = $(this).attr("checked");
    });

    // selected serialno boxes.
    $(SelectAllID).click(function () {
        var ischecked = ($(SelectAllID).is(":checked"));
        // add attribute 
        $('#ctl00_primaryHolder_gvReviewOrders input:checkbox').attr("checked", function () {
            this.checked = ischecked;
        });
    });
    
  });
</script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
  <div class="title">
    <h2>Batch #: <asp:Label ID="lblBatchNo" runat="server" /></h2>
  </div>

  <div style="width: 100%">

    <div class="FormError" id="plErrorMessage" runat="server" visible="false">
			<p><span class="MessageIcon"></span>
	            <strong>Messages:</strong>
                <span id="lblErrorMessage" runat="server" >An error was encountered.</span>
            </p>
    </div>

  <asp:Panel ID="plReviewOrders" runat="server">

    <div>
       Process Payments: 
        
        <asp:RadioButton ID="rbPaymentsRequired" GroupName="PaymentsRequired" 
            Checked="true" Text="Yes" runat="server" 
            oncheckedchanged="rbPaymentsRequired_CheckedChanged" />

        <asp:RadioButton ID="rbPaymentsNotRequired" GroupName="PaymentsRequired" Text="No" 
            runat="server" oncheckedchanged="rbPaymentsNotRequired_CheckedChanged" />
    </div> 

    <asp:GridView ID="gvReviewOrders" CssClass="OTable" AlternatingRowStyle-CssClass="Alt" 
        AutoGenerateColumns="False" runat="server" 
        PagerStyle-CssClass="mt-hd" Width="100%">

        <AlternatingRowStyle CssClass="Alt" />
        <Columns>
            <asp:TemplateField HeaderText="" ItemStyle-HorizontalAlign="Center">
                <HeaderTemplate>
                    <asp:CheckBox ID="SelectAll" runat="server" Checked="true" />
                </HeaderTemplate>
                <ItemTemplate>
                    <asp:CheckBox ID="cbRow" runat="server" Checked='<%# Bind("Process") %>' />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Order#" ItemStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    <asp:HyperLink ID="hlOrderLink" runat="server" 
                        NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"OrderNo","~/CustomerService/ReviewOrder.aspx?ID={0}" ) %>' 
                        Target="_blank" Text='<%# Bind("OrderNo") %>'></asp:HyperLink>
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>
            <asp:BoundField DataField="InvoiceNo" 
                HeaderText="Invoice#"  
                ItemStyle-HorizontalAlign="Left">
            <HeaderStyle />
            <ItemStyle HorizontalAlign="Left" />
            </asp:BoundField>
            <asp:TemplateField HeaderText="Account#" ItemStyle-HorizontalAlign="Left">
                <ItemTemplate>
                    <asp:HyperLink ID="hlAccountLink" runat="server" 
                        NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"AccountNo","~/InformationFinder/Details/Account.aspx?ID={0}" ) %>' 
                        Target="_blank" Text='<%# Bind("AccountNo") %>'></asp:HyperLink>
                </ItemTemplate>
               
            </asp:TemplateField>
            <asp:BoundField DataField="BillToName"  
                HeaderText="Billing Name" >
            <HeaderStyle />
            <ItemStyle  />
            </asp:BoundField>
            <asp:BoundField DataField="Type" HeaderText="Type" 
                 ItemStyle-HorizontalAlign="Left">
            <HeaderStyle CssClass="mt-hd" />
            <ItemStyle  HorizontalAlign="Left" />
            </asp:BoundField>
            <asp:BoundField DataField="Total" DataFormatString="{0:C}" 
                HeaderText="Total" 
                ItemStyle-HorizontalAlign="Right">
            <HeaderStyle CssClass="RightAlignHeaderText" />
            
            </asp:BoundField>
            <asp:BoundField DataField="Payments" DataFormatString="{0:C}" 
                HeaderText="Payments"  
                ItemStyle-HorizontalAlign="Right">
            <HeaderStyle CssClass="RightAlignHeaderText" />
            
            </asp:BoundField>
            <asp:BoundField DataField="Balance" DataFormatString="{0:C}" 
                HeaderText="Balance Due" 
                ItemStyle-HorizontalAlign="Right">
            <HeaderStyle CssClass="RightAlignHeaderText" />
            
            </asp:BoundField>
        </Columns>
        <PagerStyle CssClass="mt-hd" />
    </asp:GridView>

    <div style="text-align: right; padding: 5px 0 0 0;">
      <asp:Button ID="btnProcessOrders" runat="server" Text="Process Orders" 
        onclick="btnProcessOrders_Click" CssClass="OButton" />
    </div>
  
  </asp:Panel>
  
  <asp:Panel ID="plResultsTable" Visible="false" runat="server">
  
    <asp:GridView ID="gvResultsTable" CssClass="OTable" AlternatingRowStyle-CssClass="Alt" 
        AutoGenerateColumns="False" EnableViewState="true" runat="server" Width="100%">
        <Columns>
          <asp:BoundField HeaderText="Order#" DataField="OrderNo" ItemStyle-HorizontalAlign="Left" />
          <asp:BoundField HeaderText="Type" DataField="Type" ItemStyle-HorizontalAlign="Left" />
          <asp:BoundField HeaderText="Total" DataField="Total" DataFormatString="{0:C}" ItemStyle-HorizontalAlign="Right" />
         
          <asp:TemplateField HeaderText="Payment" ItemStyle-HorizontalAlign="Center">
            <ItemTemplate>
              <asp:Image ID="Image1" runat="server"
                ImageUrl='<%# DataBinder.Eval(Container.DataItem, "Payment","~/images/{0}.png" ) %>' />
            </ItemTemplate>
          </asp:TemplateField>
          
          <asp:TemplateField HeaderText="Tracking" ItemStyle-HorizontalAlign="Center">
            <ItemTemplate>
              <asp:Image ID="Image2" runat="server"
                ImageUrl='<%# DataBinder.Eval(Container.DataItem, "Tracking","~/images/{0}.png" ) %>' />
            </ItemTemplate>
          </asp:TemplateField>                    
          
          <asp:TemplateField HeaderText="Set Renewal Date" ItemStyle-HorizontalAlign="Center">
            <ItemTemplate>
              <asp:Image ID="Image3" runat="server"
                ImageUrl='<%# DataBinder.Eval(Container.DataItem, "SetRenewalDates","~/images/{0}.png" ) %>' />
            </ItemTemplate>
          </asp:TemplateField>
          
          <asp:TemplateField HeaderText="Sent to Softrax" ItemStyle-HorizontalAlign="Center">
            <ItemTemplate>
              <asp:Image ID="Image3" runat="server"
                ImageUrl='<%# DataBinder.Eval(Container.DataItem, "Softrax","~/images/{0}.png" ) %>' />
            </ItemTemplate>
          </asp:TemplateField>
          
          <asp:TemplateField HeaderText="Process Complete" ItemStyle-HorizontalAlign="Center">
            <ItemTemplate>
              <asp:Image ID="Image3" runat="server"
                ImageUrl='<%# DataBinder.Eval(Container.DataItem, "Process","~/images/{0}.png" ) %>' />
            </ItemTemplate>
          </asp:TemplateField>

       </Columns>
    </asp:GridView>
  </asp:Panel>

  <div>&nbsp;</div>
  
  <asp:Panel ID="plLogDetails" Visible="false" CssClass="ui-widget ui-widget-content" runat="server">
    <div class="ToggleLogDetails"><a id="ToggleLogDetails" href="#">Show Log Details</a></div>
    <div class="LogDetails" id="LogDetails" style="display:none;">
        <asp:Label runat="server" ID="lblMessageLog" Text="" />
    </div>
  </asp:Panel>
  
</div>
</asp:Content>

