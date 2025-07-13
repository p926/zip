<%@ Page Title="Order Detail" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master"
    AutoEventWireup="true"
    Inherits="InstaDose_InformationFinder_Details_ReviewOrder" Codebehind="ReviewOrder.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style type="text/css">
        /* CSS Definition/Override for HTML-rendered <TH> in GridView. */
        th.RightAlignHeaderText {
            text-align: right;
        }

        th.CenterAlignHeaderText {
            text-align: center;
        }
        .overlay {
            position: fixed;
            z-index: 98;
            top: 0px;
            left: 0px;
            right: 0px;
            bottom: 0px;
            background-color: #aaa;
            filter: alpha(opacity=50);
            opacity: 0.5;
        }
        .overlayContent
        {
          z-index: 99;
/*          margin: 250px auto;
*/        /*  width: 80px;
          height: 80px;*/
        }
        .overlayContent h2
        {
            font-size: 18px;
            font-weight: bold;
            color: #000;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <asp:UpdateProgress id="UpdateProgressDeleteOrder" runat="server" DynamicLayout="true" DisplayAfter="0" AssociatedUpdatePanelID="updtpnlMainContent" BackgroundPosition="Center">
        <ProgressTemplate>
            <div class="overlay"></div>
            <div align="center" class="overlayContent">
                <img src="../images/loading11.gif" alt=""/>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>    
    <div class="OToolbar">
        <ul>
            <li runat="server" ID="editOrder" visible="false">
                <asp:HyperLink ID="hlEditOrder" NavigateUrl="~/CustomerService/CreateOrder.aspx?OrderID={0}" 
                    CssClass="Icon Edit" Text="Edit Order" runat="server" />
            </li>
            <li>
                <asp:LinkButton ID="lnkbtnOrderAck" Text="Generate Acknowledgement" 
                    CssClass="Icon Print" OnClick="lnkbtnOrderAck_Click" runat="server" />
            </li>
        </ul>
    </div>
    <asp:UpdatePanel ID="updtpnlMainContent" runat="server">
            <ContentTemplate>
                <div id="divErrors" class="FormError" runat="server" visible="false">
		            <p><span class="MessageIcon"></span>
		            <strong>Messages:</strong>&nbsp;<span id="errorMsg" runat="server" >An error was encountered.</span></p>
                </div>
                <div id="divSuccesses" class="FormMessage" runat="server" visible="false">
		            <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong>&nbsp;<span id="successMsg" runat="server" >Submission of information has been successful.</span></p>
                </div>
    <asp:FormView ID="fvOrderHeader" runat="server" DataKeyNames="OrderNo" style="border-spacing:0; width:100%;"
        DataSourceID="sqlOrderHeader">
        <ItemTemplate>
            <table class="OTable">
                <tr>
                    <td style="width: 15%; font-weight: bold; text-align:right;">Order #:</td>
                    <td style="width: 35%">
                        <asp:Label ID="OrderNoLabel" runat="server" Text='<%# Eval("OrderNo") %>' />
                    </td>
                    <td style="width: 15%; text-align: right; font-weight: bold">Account Name:</td>
                    <td>
                        <asp:HyperLink ID="AccountNoLabel" runat="server" Text='<%# Eval("AccountName") %>'
                            NavigateUrl='<%# Eval("AccountNo", "~/InformationFinder/Details/Account.aspx?ID={0}#Order_tab") %>' />
                    </td>
                </tr>
                <tr>
                    <td style="font-weight: bold; text-align: right;">Status:</td>
                    <td>
                        <asp:Label ID="Label1" runat="server" Text='<%# Bind("Status") %>' /></td>
                    <td style="font-weight: bold; text-align: right;">Source:</td>
                    <td>
                        <asp:Label ID="SourceLabel" runat="server" Text='<%# Bind("Source") %>' /></td>
                </tr>
                <tr>
                    <%--<td style="font-weight: bold; text-align: right;">Rate Discount %:</td>
                    <td>
                        <asp:Label ID="StoreOrderIdLabel" runat="server" Text='<%# Bind("Discount", "{0:f}") %>' />
                    </td>--%>
                    <td style="font-weight: bold; text-align: right;">Created Date:</td>
                    <td>
                        <asp:Label ID="Label2" runat="server" Text='<%# Bind("CreatedDate", "{0:d}") %>' />
                    </td>
                    <td></td>
                    <td></td>
                </tr>
                <tr>
                    <td style="font-weight: bold; text-align: right;">Tracking #:</td>
                    <td>
                        <asp:HyperLink ID="hlTrackingNumber" runat="server" Text='<%# Bind("TrackingNumber") %>' />
                    </td>
                    <td style="font-weight: bold; text-align: right;">Payment Method:</td>
                    <td>
                        <asp:Label ID="PaymentMethodLabel" runat="server" Text='<%# Bind("PaymentMethod") %>' />
                    </td>
                </tr>
                <tr>
                    <td style="font-weight: bold; text-align: right;">Ship Date:</td>
                    <td>
                        <asp:Label ID="ShipDateLabel" runat="server" Text='<%# Bind("ShipDate", "{0:d}") %>' />
                    </td>
                    <td style="font-weight: bold; text-align: right;">PO #:</td>
                    <td>
                        <asp:Label ID="PONumberLabel" runat="server" Text='<%# Bind("PONumber") %>' />
                    </td>
                </tr>
                <tr>
                    <td style="font-weight: bold; text-align: right;">Coupon:</td>
                    <td>
                        <asp:Label ID="CouponLabel" runat="server" Text='<%# Bind("Coupon") %>' /></td>
                    <td style="font-weight: bold; text-align: right;">Shipping:</td>
                    <td>
                        <asp:Label ID="OrderShippingLabel" runat="server" Text='<%# Bind("OrderShipping", "{0:0.00}") %>' />
                    </td>
                </tr>
                <tr>
                    <td style="font-weight: bold; text-align: right;">Referral Code:</td>
                    <td>
                        <asp:Label ID="ReferralLabel" runat="server" Text='<%# Bind("Referral") %>' />
                    </td>
                    <td style="font-weight: bold; text-align: right;">Tax:</td>
                    <td>
                        <asp:Label ID="OrderTaxLabel" runat="server" Text='<%# Bind("OrderTax", "{0:0.00}") %>' />
                    </td>
                </tr>
                <tr>
                    <td style="font-weight: bold; text-align: right;">Released To Softrax:</td>
                    <td>
                        <asp:Label ID="Label3" runat="server" Text='<%# YesNo(Eval("SoftTraxIntegration")) %>' />
                    </td>
                    <td style="font-weight: bold; text-align: right;">Credits:</td>
                    <td>
                        <asp:Label ID="MiscCreditLabel" runat="server" Text='<%# Bind("MiscCredit", "{0:0.00}") %>' />
                    </td>
                </tr>
                <tr>
                    <td style="font-weight: bold; text-align: right;">Card Type:</td>
                    <td>
                        <asp:Label ID="Label4" runat="server" Text="" /></td>
                    <td style="font-weight: bold; text-align: right;">Subtotal:</td>
                    <td>
                        <asp:Label ID="OrderSubtotalLabel" runat="server" Text='<%# Bind("OrderSubtotal", "{0:0.00}") %>' />
                    </td>
                </tr>
                <tr>
                    <td style="font-weight: bold; text-align: right;">Special Instruction:</td>
                    <td>
                        <asp:Label ID="SpecialInstructionLabel" runat="server" Text='<%# Bind("SpecialInstructions") %>' />
                    </td>
                    <td style="font-weight: bold; text-align: right;">Total:</td>
                    <td>
                        <asp:Label ID="OrderTotalLabel" runat="server" Text='<%# Bind("OrderTotal", "{0:0.00}") %>' />
                    </td>
                </tr>
            </table>
        </ItemTemplate>
    </asp:FormView>

    <%--ORDER DETAILS GRIDVIEW--%>
    <%--<asp:GridView ID="gvOrderDetails" runat="server" CssClass="OTable" 
    AlternatingRowStyle-CssClass="Alt" AllowSorting="True"
    AutoGenerateColumns="False" AllowPaging="True" PageSize="20" Width="100%">
        <AlternatingRowStyle CssClass="Alt" />
        <Columns>
            <asp:BoundField DataField="ProductSKU" HeaderText="Product SKU" SortExpression="ProductSKU" />
            <asp:BoundField DataField="ProductName" HeaderText="Product Name" SortExpression="ProductName">
                <HeaderStyle CssClass="CenterAlignHeaderText" />
                <ItemStyle HorizontalAlign="Center" />
            </asp:BoundField>
            <asp:BoundField DataField="UnitPrice" HeaderText="Unit Price" SortExpression="UnitPrice">
                <HeaderStyle CssClass="RightAlignHeaderText" />
                <ItemStyle HorizontalAlign="Right" />
            </asp:BoundField>
            <asp:BoundField DataField="ItemQuantity" HeaderText="Qty." SortExpression="ItemQuantity">
                <HeaderStyle CssClass="CenterAlignHeaderText" />
                <ItemStyle HorizontalAlign="Center" />
            </asp:BoundField>
            <asp:BoundField DataField="SubTotal" HeaderText="Sub-Total" SortExpression="Subtotal">
                <HeaderStyle CssClass="RightAlignHeaderText" />
                <ItemStyle HorizontalAlign="Right" />
            </asp:BoundField>
        </Columns>
        <EmptyDataTemplate>
            <div>There are no records found!</div>
        </EmptyDataTemplate>
        <PagerSettings PageButtonCount="20" />
    </asp:GridView>--%>
    <%--END--%>

    <asp:GridView ID="gvOrderDetails" runat="server" AutoGenerateColumns="False" CssClass="OTable" OnSelectedIndexChanged="gvOrderDetails_SelectedIndexChanged">
        <Columns>
            <asp:BoundField DataField="SKU" HeaderText="SKU" ItemStyle-Width="175px" SortExpression="SKU"></asp:BoundField>
            <asp:BoundField DataField="ProductName" HeaderText="Product" ItemStyle-Width="175px" SortExpression="ProductName"></asp:BoundField>
            <asp:BoundField DataField="ProductVariant" HeaderText="Subscription" SortExpression="ProductVariant" ItemStyle-Width="175px"></asp:BoundField>
            <asp:BoundField DataField="Price" SortExpression="Price" DataFormatString="{0:c}" ItemStyle-Width="175px" HeaderStyle-CssClass="ralign" HeaderText="Unit Price">
                <HeaderStyle HorizontalAlign="Right" CssClass="RightAlignHeaderText"></HeaderStyle>
                <ItemStyle HorizontalAlign="Right"></ItemStyle>
            </asp:BoundField>
            <asp:BoundField ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-HorizontalAlign="Center" DataField="Quantity" HeaderText="Qty" SortExpression="Quantity" ItemStyle-Width="100px">
                <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Center"></HeaderStyle>
                <ItemStyle CssClass="mt-itm RightAlign"></ItemStyle>
            </asp:BoundField>
            <asp:BoundField DataField="LineTotal" HeaderText="Subtotal" SortExpression="LineTotal" DataFormatString="{0:c}">
                <HeaderStyle CssClass="RightAlignHeaderText"></HeaderStyle>
                <ItemStyle CssClass="mt-itm RightAlign" Width="175px" HorizontalAlign="Right"></ItemStyle>
            </asp:BoundField>
        </Columns>
        <AlternatingRowStyle CssClass="Alt" />
        <HeaderStyle HorizontalAlign="Right" />
        <PagerStyle CssClass="Footer" />
    </asp:GridView>
    <asp:GridView ID="gvPackages" CssClass="OTable" runat="server"
        AutoGenerateColumns="False" AllowPaging="true" PageSize="5" AllowSorting="true"
        DataSourceID="sqlGetAccountInfoPackage">
        <Columns>

            <asp:TemplateField HeaderText="Tracking#">
                <ItemTemplate>
                    <asp:HyperLink ID="Hyperlink3b" runat="server" Text='<%# Eval("TrackingNumber") %>'
                        NavigateUrl='<%# Eval("TrackingNumber", "http://fedex.com/Tracking?ascend_header=1&clienttype=dotcom&cntry_code=us&language=english&tracknumbers={0}") %>'
                        Target="_blank" />
                </ItemTemplate>
            </asp:TemplateField>

            <asp:BoundField HeaderStyle-CssClass="mt-hd"
                DataField="Shipdate" HeaderText="Shipped" SortExpression="Shipdate" DataFormatString="{0:d}" />

            <asp:TemplateField HeaderText="Shipping Address"
                HeaderStyle-Width="300" ItemStyle-Width="300" ItemStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:Label ID="txtPackShipAddress" runat="server"
                        Text='<%# GeneratePackShippingInfo(DataBinder.Eval(Container.DataItem,"Packageid","" )) %>'></asp:Label>

                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderStyle-CssClass="mt-hd"
                HeaderText="Shipping Devices" ItemStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:Label ID="txtSerialno" runat="server"
                        Text='<%# GeneratePackSerialNo(DataBinder.Eval(Container.DataItem,"Packageid","" )) %>'></asp:Label>

                </ItemTemplate>
            </asp:TemplateField>

        </Columns>
        <EmptyDataTemplate>
            <div class="NoData">
                There are no package associated with this order.
            </div>
        </EmptyDataTemplate>
        <AlternatingRowStyle CssClass="Alt" />
        <PagerStyle CssClass="Footer" />
    </asp:GridView>

    <div class="Buttons">
        <div class="ButtonHolder">
            <asp:Button ID="btnDeleteOrder" CssClass="OButton" runat="server" Text="Delete This Order"  OnClick="btnDeleteOrder_Click" OnClientClick="return confirm('Are you sure you want to delete this order?');" />
            <asp:Button ID="btnBack" CssClass="OButton" runat="server" Text="Back to Account"
                OnClick="btnBack_Click" />
        </div>
        <div class="Clear"></div>
    </div>



    <asp:SqlDataSource ID="sqlOrderDetails" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_if_GetOrderDetailsByOrderNo"
        SelectCommandType="StoredProcedure">

        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="OrderNo" QueryStringField="ID"
                Type="Int32" />
        </SelectParameters>

    </asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlOrderHeader" runat="server"
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="sp_if_GetOrderHeaderByNo" SelectCommandType="StoredProcedure">

        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="OrderNo" QueryStringField="ID"
                Type="Int32" />
        </SelectParameters>

    </asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlGetAccountInfoPackage" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="Select * 
		                From packages 	
		                Where OrderID  = @OrderNo ">
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="OrderNo" QueryStringField="ID"
                Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
                </ContentTemplate>
        </asp:UpdatePanel>

</asp:Content>

