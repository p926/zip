<%@ Page Title="Convert Product SKU" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="ConvertProductSKU.aspx.cs" Inherits="portal_instadose_com_v3.Manufacturing.ConvertProductSKU" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        /* CSS Definition/Override for HTML-rendered <TH> in GridView. */
        th.RightAlignHeaderText {
            text-align: right;
        }

        th.CenterAlignHeaderText {
            text-align: center;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div style="width: 100%">
        <asp:UpdatePanel ID="updtpnlOrderDetails" runat="server" DefaultButton="btnFindOrder">
            <ContentTemplate>
                <asp:UpdateProgress id="updtpbOrderDetails" runat="server" DynamicLayout="true" DisplayAfter="0" >
                    <ProgressTemplate>
                        <div style="width: 100%;">
                            <center><img src="../images/loading11.gif" alt="Please wait..."/></center>
                        </div>
                    </ProgressTemplate>
                </asp:UpdateProgress>
                <div id="divFormError" runat="server" class="FormError" visible="false" style="margin: 10px;" >
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="spnFormErrorMessage" runat="server" >An error has occurred</span></p>
                </div>
                <div id="divFormSuccess" runat="server" class="FormMessage" visible="false" style="margin: 10px;" > 
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="spnFormSuccessMessage" runat ="server" >A success has occurred.</span></p>
                </div> 
                <div id="divMainContent" style="width: 100%;">
                    <%--ENTER ORDER NUMBER TOOLBAR--%>
                    <div id="divSearchToolbar" class="OToolbar">
                        <ul>
                            <li>Enter Order #:</li>
                            <li>
                                <asp:TextBox ID="txtOrderNumber" runat="server" placeholder="12345" TabIndex="1"></asp:TextBox>
                            </li>
                            <li>
                                <asp:Button ID="btnFindOrder" runat="server" OnClick="btnFindOrder_Click" Text="Find Order" TabIndex="2" />
                            </li>
                        </ul>
                    </div>
                    <%--END--%>
                    <div id="divOrderDetails" runat="server" style="width: 100%;" visible="false">
                        <%--ORDER HEADER DETAILS--%>
                        <table class="OTable">
                            <tr>
                                <th style="width: 20%; text-align: center;">Order #</th>
                                <th style="width: 20%; text-align: center;">Account Name</th>
                                <th style="width: 20%; text-align: center;">Order Date</th>
                                <th style="width: 20%; text-align: center;">Status</th>
                                <th style="width: 20%; text-align: center;">Currency Code</th>          
                            </tr>
                            <tr>
                                <td class="CenterAlign">
                                    <asp:Label ID="lblOrderID" runat="server" Text="" />
                                </td>
                                <td class="CenterAlign">
                                    <asp:HyperLink ID="hyprlnkAccountName" runat="server" Text="" />
                                </td>
                                <td class="CenterAlign">
                                    <asp:Label ID="lblOrderDate" runat="server" Text="" />
                                </td>
                                <td class="CenterAlign">
                                    <asp:Label ID="lblStatus" runat="server" Text="" />
                                </td>
                                <td class="CenterAlign">
                                    <asp:Label ID="lblCurrencyCode" runat="server" Text="" />
                                </td>
                            </tr>
                        </table>
                        <%--END--%>
                        <%--ORDER DETAILS GRIDVIEW--%>
                        <asp:GridView ID="gvOrderDetails" runat="server" CssClass="OTable" 
                        AlternatingRowStyle-CssClass="Alt" AllowSorting="True" DataKeyNames="OrderDetailID"
                        AutoGenerateColumns="False" AllowPaging="True" PageSize="20" Width="100%">
                            <AlternatingRowStyle CssClass="Alt" />
                            <Columns>
                                <asp:BoundField DataField="ProductSKU" HeaderText="Product SKU" SortExpression="ProductSKU" />
                                <asp:BoundField DataField="ProductName" HeaderText="Product Name" SortExpression="ProductName">
                                    <HeaderStyle CssClass="CenterAlignHeaderText" />
                                    <ItemStyle HorizontalAlign="Center" />
                                </asp:BoundField>
                                <asp:BoundField DataField="UnitPrice" HeaderText="Unit Price" SortExpression="UnitPrice" DataFormatString="{0:F2}">
                                    <HeaderStyle CssClass="RightAlignHeaderText" />
                                    <ItemStyle HorizontalAlign="Right" />
                                </asp:BoundField>
                                <asp:BoundField DataField="ItemQuantity" HeaderText="Qty." SortExpression="ItemQuantity">
                                    <HeaderStyle CssClass="CenterAlignHeaderText" />
                                    <ItemStyle HorizontalAlign="Center" />
                                </asp:BoundField>
                                <asp:BoundField DataField="SubTotal" HeaderText="Sub-Total" SortExpression="Subtotal" DataFormatString="{0:F2}">
                                    <HeaderStyle CssClass="RightAlignHeaderText" />
                                    <ItemStyle HorizontalAlign="Right" />
                                </asp:BoundField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div>There are no records found!</div>
                            </EmptyDataTemplate>
                            <PagerSettings PageButtonCount="20" />
                        </asp:GridView>
                        <%--END--%>
                        <%--TOTAL--%>
                        <table class="OTable">
		                    <tr>
			                    <th style="text-align: right !important; padding-right: 5px;">Total:</th>
			                    <th style="text-align: right !important; width: 200px"><asp:Label ID="lblTotal" runat="server"></asp:Label></th>
		                    </tr>
	                    </table>
                        <%--END --%>
                        <%--CONVERT BUTTON--%>
                        <div style="text-align: right; padding-top: 0px;">
                            <asp:Button ID="btnConvertProductSKUs" runat="server" Text="Convert Product SKU's" CssClass="OButton" OnClick="btnConvertProductSKUs_Click" Enabled="true" />
                        </div>
                        <%--END--%>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>  
</asp:Content>
