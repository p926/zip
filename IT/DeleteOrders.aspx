<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_DeleteOrders" Codebehind="DeleteOrders.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        /* CSS Definition/Override for HTML-rendered <TH> in GridView. */
        th.RightAlignHeaderText {
            text-align: right;
        }

        th.CenterAlignHeaderText {
            text-align: center;
        }
    </style>
    <script type="text/javascript">
        $(document).ready(function () {
            // Confirm Order Deletion.
            $('#confirmOrderDeletion').dialog({
                autoOpen: false,
                width: 385,
                height: 225,
                resizable: false,
                title: "Confirm Deletion of Order",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "Delete": function () {
                        $('#<%= btnConfirmOrderDeletion.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $('#<%= btnModalCancel.ClientID %>').click();
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
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <asp:HiddenField ID="hdnfldOrderID" runat="server" Value="0" />
    <%--CONFIRM ORDER DELETION--%>
    <div id="confirmOrderDeletion">
        <asp:UpdatePanel ID="updtpnlConfirmOrderDeletion" runat="server">
            <ContentTemplate>
                <div id="divModalError" class="FormError" runat="server" visible="false">
		            <p><span class="MessageIcon"></span>
		            <strong>Messages:</strong>&nbsp;<span id="modalErrorMessage" runat="server" >An error was encountered.</span></p>
                </div>
                <div class="OForm">
                    <%--ORDER ID LABEL--%>
                    <div class="Row">                
				        <div class="Label Small">Order #:</div>
				        <div class="Control">
                            <asp:Label ID="lblModalOrderID" runat="server" CssClass="LabelValue" Text=""></asp:Label>    
				        </div>
				        <div class="Clear"></div>
			        </div>
                    <%--END--%>
                    <%--ORDER DELETION MESSAGE--%>
                    <div class="Row">                
				        <div class="Control">
                            <asp:Label ID="lblOrderDeletionMessage" runat="server" Font-Bold="true" ForeColor="Red">Are you sure you would like to delete this order?</asp:Label>    
				        </div>
				        <div class="Clear"></div>
			        </div>
                    <%--END--%>
		        </div>
                <asp:Button ID="btnConfirmOrderDeletion" runat="server" Text="Delete" OnClick="btnConfirmOrderDeletion_Click" style="display: none;" />
                <asp:Button ID="btnModalCancel" runat="server" Text="Cancel" OnClick="btnModalCancel_Click" style="display: none;" />
                <%--END--%>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--MAIN PAGE CONTENT--%>
    <div style="width: 100%;">
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

        <div id="orderNumberSearchToolbar" class="OToolbar">
            <%--ORDER NUMBER SEARCH TOOLBAR--%>
            <ul>
                <li> 
                    Enter Order #:&nbsp;<asp:TextBox ID="txtEnterOrderNumber" runat="server" CssClass="Size Small" Text=""></asp:TextBox> 
                </li> 
                <li>
                    <asp:Button ID="btnSearchOrderNumber" runat="server" Text="Search" OnClick="btnSearchOrderNumber_Click" CssClass="OButton" />
                </li>
            </ul>
            <%--END--%>
        </div>

        <%--ORDER INFORMATION--%>
        <div>            
                
            <asp:FormView ID="fvOrderInformation" runat="server" Width="100%">
                <ItemTemplate>
                    <table class="OTable">
                        <tr>
                            <td style="width: 15%; font-weight: bold; text-align:right;">Order #:</td>
                            <td style="width: 35%">
                                <asp:Label ID="lblOrderNo" runat="server" Text='<%# Bind("OrderNo") %>' />
                            </td>
                            <td style="width: 15%; text-align: right; font-weight: bold">Account Name:</td>
                            <td>
                                <asp:HyperLink ID="hyprlnkAccountName" runat="server" Text='<%# Bind("AccountName") %>' NavigateUrl='<%# Eval("AccountNo", "~/InformationFinder/Details/Account.aspx?ID={0}#Order_tab") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; text-align: right;">Status:</td>
                            <td>
                                <asp:Label ID="lblStatus" runat="server" Text='<%# Bind("Status") %>' />
                            </td>
                            <td style="font-weight: bold; text-align: right;">Source:</td>
                            <td>
                                <asp:Label ID="lblSource" runat="server" Text='<%# Bind("Source") %>' /></td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; text-align: right;">Created Date:</td>
                            <td>
                                <asp:Label ID="lblCreatedDate" runat="server" Text='<%# string.Format("{0:d}", Eval("CreatedDate")) %>' />
                            </td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; text-align: right;">Tracking #:</td>
                            <td>
                                <asp:HyperLink ID="hyprlnkTrackingNumber" runat="server" Text='<%# Bind("TrackingNumber") %>' />
                            </td>
                            <td style="font-weight: bold; text-align: right;">Payment Method:</td>
                            <td>
                                <asp:Label ID="lblPaymentMethod" runat="server" Text='<%# Bind("PaymentMethod") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; text-align: right;">Ship Date:</td>
                            <td>
                                <asp:Label ID="lblShipDate" runat="server" Text='<%# string.Format("{0:d}", Eval("ShipDate")) %>' />
                            </td>
                            <td style="font-weight: bold; text-align: right;">PO #:</td>
                            <td>
                                <asp:Label ID="lblPONumber" runat="server" Text='<%# Bind("PONumber") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; text-align: right;">Coupon:</td>
                            <td>
                                <asp:Label ID="lblCoupon" runat="server" Text='<%# Bind("Coupon") %>' />
                            </td>
                            <td style="font-weight: bold; text-align: right;">Shipping:</td>
                            <td>
                                <asp:Label ID="lblOrderShipping" runat="server" Text='<%# Bind("FormattedShipping") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; text-align: right;">Referral Code:</td>
                            <td>
                                <asp:Label ID="lblReferralCode" runat="server" Text='<%# Bind("Referral") %>' />
                            </td>
                            <td style="font-weight: bold; text-align: right;">Tax:</td>
                            <td>
                                <asp:Label ID="lblOrderTax" runat="server" Text='<%# Bind("FormattedTax") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; text-align: right;">Released To Softrax:</td>
                            <td>
                                <asp:Label ID="lblReleasedToSoftrax" runat="server" Text='<%# YesNo(Eval("SoftTraxIntegration")) %>' />
                            </td>
                            <td style="font-weight: bold; text-align: right;">Credits:</td>
                            <td>
                                <asp:Label ID="lblMiscCredit" runat="server" Text='<%# Bind("FormattedCredit") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; text-align: right;">Card Type:</td>
                            <td>
                                <asp:Label ID="lblCardType" runat="server" Text="" />
                            </td>
                            <td style="font-weight: bold; text-align: right;">Subtotal:</td>
                            <td>
                                <asp:Label ID="lblOrderSubtotal" runat="server" Text='<%# Bind("FormattedSubtotal") %>' />
                            </td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold; text-align: right;">Special Instruction:</td>
                            <td>
                                <asp:Label ID="lblSpecialInstructions" runat="server" Text='<%# Bind("SpecialInstructions") %>' />
                            </td>
                            <td style="font-weight: bold; text-align: right;">Total:</td>
                            <td>
                                <asp:Label ID="lblOrderTotal" runat="server" Text='<%# Bind("FormattedTotal") %>' />
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>
                <EmptyDataTemplate>
                    <div class="NoData">
                        The Order # you entered has either been deleted or does not exist.
                    </div>
                </EmptyDataTemplate>
            </asp:FormView>

            <%--ORDER DETAILS GRIDVIEW--%>
            <asp:GridView ID="gvOrderDetails" runat="server" 
            AutoGenerateColumns="False" CssClass="OTable">
                <Columns>
                    <asp:BoundField DataField="SKU" HeaderText="SKU" SortExpression="SKU" />
                    <asp:BoundField DataField="ProductName" HeaderText="Product" SortExpression="ProductName" />
                    <asp:BoundField DataField="ProductVariant" HeaderText="Subscription" SortExpression="ProductVariant" />
                    <asp:BoundField DataField="Price" HeaderText="Unit Price" SortExpression="Price" DataFormatString="{0:c}" HeaderStyle-CssClass="ralign">
                        <HeaderStyle HorizontalAlign="Right" CssClass="RightAlignHeaderText"></HeaderStyle>
                        <ItemStyle HorizontalAlign="Right"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="Quantity" HeaderText="Qty." SortExpression="Quantity" HeaderStyle-CssClass="CenterAlignHeaderText">
                        <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center"></HeaderStyle>
                        <ItemStyle CssClass="CenterAlign" HorizontalAlign="Center"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="LineTotal" HeaderText="Subtotal" SortExpression="LineTotal" DataFormatString="{0:c}" HeaderStyle-CssClass="RightAlignHeaderText">
                        <HeaderStyle CssClass="RightAlignHeaderText"></HeaderStyle>
                        <ItemStyle CssClass="rightalign" Width="175px" HorizontalAlign="Right"></ItemStyle>
                    </asp:BoundField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="NoData">
                        There are no details related to this Order #.
                    </div>
                </EmptyDataTemplate>
                <AlternatingRowStyle CssClass="Alt" />
                <PagerStyle CssClass="Footer" />
            </asp:GridView>
            <%--END--%>

            <%--PACKAGE INFORMATION GRIDVIEW--%>
            <asp:GridView ID="gvPackageDetails" runat="server" 
                AutoGenerateColumns="False" CssClass="OTable" 
                AllowPaging="true" PageSize="10" AllowSorting="true">
                <Columns>
                    <asp:TemplateField HeaderText="Tracking #" ItemStyle-VerticalAlign="Top" HeaderStyle-Width="200px" ItemStyle-Width="200px">
                        <ItemTemplate>
                            <asp:HyperLink ID="hyprlnkTrackingNumber" runat="server" Text='<%# Eval("TrackingNumber") %>' Target="_blank"
                            NavigateUrl='<%# Eval("TrackingNumber", "http://fedex.com/Tracking?ascend_header=1&clienttype=dotcom&cntry_code=us&language=english&tracknumbers={0}") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="ShipDate" HeaderText="Shipped" SortExpression="ShipDate" DataFormatString="{0:d}" ItemStyle-VerticalAlign="Top" HeaderStyle-Width="200px" ItemStyle-Width="200px" />
                    <asp:TemplateField HeaderText="Shipping Address" ItemStyle-VerticalAlign="Top" HeaderStyle-Width="300px" ItemStyle-Width="300px">
                        <ItemTemplate>
                            <asp:Label ID="txtPackShipAddress" runat="server"
                            Text='<%# GeneratePackShippingInfo(DataBinder.Eval(Container.DataItem,"PackageID","" )) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Shipping Devices" ItemStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="lblSerialno" runat="server"
                            Text='<%# GeneratePackSerialNo(DataBinder.Eval(Container.DataItem,"PackageID","" )) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="NoData">
                        There are no packages associated with this Order #.
                    </div>
                </EmptyDataTemplate>
                <AlternatingRowStyle CssClass="Alt" />
                <PagerStyle CssClass="Footer" />
            </asp:GridView>
            <%--END--%>
                    
        </div>
        <%--END--%>

        <%--DELETE BUTTON (OPENS MODAL/DIALOG)--%>
        <div class="Buttons">
            <div class="ButtonHolder">
                <asp:Button ID="btnDeleteOrder" CssClass="OButton" runat="server" Text="Delete This Order" OnClientClick="openDialog('confirmOrderDeletion'); return false;" />
            </div>
            <div class="Clear"></div>
        </div>
        <%--END--%>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>
</asp:Content>

