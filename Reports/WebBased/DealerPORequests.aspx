<%@ Page Language="C#" AutoEventWireup="true" Codebehind="DealerPORequests.aspx.cs" MasterPageFile="~/Reports/WebBased/Print.master" Inherits="Reports_WebBased_DealerPORequests" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContent">
    <asp:Repeater runat="server" ID="rptOrders" OnItemDataBound="rptOrders_ItemDataBound">
        <ItemTemplate>
    <div class="page-break iccare">
        <div class="page-header">
            <div style="margin-right: -15px; margin-left: -15px;">
                <div class="company-address" style="float: left; position: relative; min-height: 1px; padding-right: 0px; padding-left: 15px; width: 30.33333333%;">
                    <div><img src="data:image/png;base64, iVBORw0KGgoAAAANSUhEUgAAAQAAAABQCAYAAAD2i6slAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAA2hpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDpGNzdGMTE3NDA3MjA2ODExODIyQUFDMEI3QzYyREMyMSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDpDMEFGNzhEMTI2RDIxMUUyOEEwMUI2NEZDMzE0QTJGQiIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDpDMEFGNzhEMDI2RDIxMUUyOEEwMUI2NEZDMzE0QTJGQiIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ1M2IChNYWNpbnRvc2gpIj4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6Rjc3RjExNzQwNzIwNjgxMTgyMkFBQzBCN0M2MkRDMjEiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6Rjc3RjExNzQwNzIwNjgxMTgyMkFBQzBCN0M2MkRDMjEiLz4gPC9yZGY6RGVzY3Jp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" style="width:190px;" /></div>
                    <div>IC Care</div>
                    <div>104 Union Valley Rd</div>
                    <div>Oak Ridge, TN 37830</div>
                    <div>Toll Free US/Canada &nbsp;&nbsp;Phone: (877) 477-5486</div>
                    <div>info@iccare.net &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Fax: (877) 477-5186</div>
                </div>
                <div style="position: relative; min-height: 1px; padding-right: 0px; padding-left: 15px; width: 38.66666667%; float: left">
                    <h2 class="align-center">Purchase Order Request</h2>
                    <div class="align-center"><%# Eval("DealerName") %></div>
                    <div class="align-center"><%# Eval("DealerAddress1") %></div>
                    <div class="align-center"><%# Eval("DealerAddress2") %></div>
                    <div class="align-center"><%# Eval("DealerAddress3") %></div>
                    <div class="align-center"><%# Eval("DealerCity") %>, <%# Eval("DealerState") %> <%# Eval("DealerPostalCode") %></div>
                </div>
                <div style="width: 25%; float: left; position: relative; min-height: 1px; padding-right: 15px; padding-left: 15px; ">
                    <table class="small-table borderless" style="margin-top: 2em">
                        <tr>
                            <td class="align-right"><strong>Date:</strong></td>
                            <td><%# Eval("OrderDate", "{0:M/d/yyyy}") %></td>
                        </tr>
                        <tr>
                            <td class="align-right"><strong>Dealer:</strong></td>
                            <td><%# Eval("DealerNative") %></td>
                        </tr>
                        <tr>
                            <td class="align-right"><strong>Telephone:</strong></td>
                            <td><%# Eval("DealerTelephone") %></td>
                        </tr>
                        <tr>
                            <td class="align-right"><strong>Fax:</strong></td>
                            <td><%# Eval("DealerFax") %></td>
                        </tr>
                    </table>
                </div>
            </div>
            <div class="clear"></div>
        </div>
        <div style="margin-right: -15px; margin-left: -15px;">
            <div style="font-size:14pt; width: 49%; position: relative; min-height: 1px; padding-right: 0px; padding-left: 15px; float: left; margin-left: 8.33333333%;">
                <div><%# Eval("ShippingCompany") %></div>
                <div><%# Eval("ShippingAddress1") %></div>
                <div><%# Eval("ShippingAddress2") %></div>
                <div><%# Eval("ShippingAddress3") %></div>
                <div><%# Eval("ShippingCity") %>, <%# Eval("ShippingState") %> <%# Eval("ShippingPostalCode") %></div>
            </div>
            <div style="position: relative; min-height: 1px; padding-right: 15px; padding-left: 0px; width: 35.66666667%; float: left">
                <table class="medium-table">
                    <tr>
                        <td class="align-right"><strong>Account #:</strong></td>
                        <td colspan="3"><%# Eval("AccountID") %></td>
                    </tr>
                    <tr>
                        <td class="align-right"><strong>Location:</strong></td>
                        <td colspan="3"><%# Eval("LocationName") %></td>
                    </tr>
                    <tr>
                        <td class="align-right"><strong>Phone:</strong></td>
                        <td colspan="3"><%# Eval("ShippingTelephone") %></td>
                    </tr>
                    <tr>
                        <td class="align-right"><strong>Fax:</strong></td>
                        <td colspan="3"><%# Eval("ShippingFax") %></td>
                    </tr>
                    <tr>
                        <td class="align-right"><strong>Last P.O. #:</strong></td>
                        <td colspan="3"><%# Eval("LastPONumber") %></td>
                    </tr>
                    <tr>
                        <td class="align-right"><strong>Request #:</strong></td>
                        <td><%# Eval("OrderID") %></td>
                        <td class="align-right"><strong>Total Qty:</strong></td>
                        <td><%# Eval("TotalQuantity") %></td>
                    </tr>
                </table>
            </div>
        </div>
        <div class="clear"></div>
        <asp:Repeater runat="server" ID="rptOrderDetails" OnItemDataBound="rptOrderDetails_ItemDataBound">
            <HeaderTemplate>
        <table style="border-collapse: collapse;">
            <tr>
                <th class="align-center">Item #</th>
                <th>Description</th>
                <th class="align-center">Wear Period</th>
                <th class="align-center">Sub. Service<br />Period</th>
                <th class="align-center">Qty</th>
                <th class="align-right" style="width: 10%;">Wholesale<br /> Price</th>
                <th class="align-right">Total</th>
            </tr>
            </HeaderTemplate>
            <ItemTemplate>
            <tr>
                <td class="align-center"><%# Eval("DealerSKU") == null ? "N/A" : Eval("DealerSKU") %></td>
                <td><%# Eval("Description") %></td>
                <td class="align-center"><%# Eval("WearStartDate", "{0:M/d/yyyy}") %> - <%# Eval("WearEndDate", "{0:M/d/yyyy}") %></td>
                <td class="align-center"><%# Eval("SubscriptionPeriod") %></td>
                <td class="align-center"><%# Eval("Quantity") %></td>
                <td class="align-right"><%# Eval("Price") %></td>
                <td class="align-right"><%# Eval("Total") %></td>
            </tr>
            </ItemTemplate>
            <FooterTemplate>
            <tr>
                <td colspan="5" rowspan="2"><div class="fill-in-box-label">New PO# Issued by Company:</div><div class="fill-in-box"></div></td>
                <td class="align-right"><strong>Freight:</strong></td>
                <td class="align-right"><asp:Label Text="$0.00" ID="lblShippingCharge" runat="server" /></td>
            </tr>
            <tr>
                <td class="align-right"><strong>Total:</strong></td>
                <td class="align-right"><asp:Label Text="$0.00" ID="lblOrderTotal" runat="server" /></td>
            </tr>
        </table>
            </FooterTemplate>
        </asp:Repeater>
        <div class="notice">
            <h4>ATTENTION:</h4>
            <ul>
                <li>Please write your PO# in the space provided of each listed request.</li>
                <li>Once complete please email to: iccarepo@iccare.net or fax to: 877-477-5186</li>
                <li>The following PO Request may include annual renewal of customer accounts. In order to prevent a disruption of dosimetry service, and unless otherwise instructed by the customer, ICCare will automatically initiate renewal of subscription based dosimetry services in advance of expiry.</li>
                <li>All fees, charges and sales are final. Once dosimeter(s) have been produced, orders are non-refundable and subscriptions will automatically renew until canceled. Upon service cancellation all badges must be returned to ICCare within 30 days. If the badges are not returned a charge of  $25.00 per badge non-returned fee will be incurred.</li>
                <li>If you have questions please call (877) 477-5486 or email: info@iccare.net.</li>
            </ul>
        </div>
    </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>