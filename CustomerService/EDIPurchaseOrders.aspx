<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_EDIPurchaseOrders" Codebehind="EDIPurchaseOrders.aspx.cs" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        #pof-details td {
            vertical-align:top;
        }
        .modal-body {
            max-height: 500px;
        }
    </style>
    <script type="text/javascript">
        var pos_to_reviews = [];
        var current_id = 0;
        $(function () {
            $("#tabs").tabs();
            $("#po_status").click(function () {
                $('.po_status input').prop('checked', $(this).is(':checked'));
            });
            $("#po_review").click(function () {
                clearPOForm();
                pos_to_reviews = [];
                $(".po_status input[type='checkbox']:checked").each(function() {
                    pos_to_reviews.push($(this).parent().data("id"));
                });
                // exit if no POs were selected.
                if (pos_to_reviews.length == 0) return;

                loadNextPO();
                $("#po_review_modal").modal('show');
            });
            $("#po_review_save").click(function () {
                // save and get next.
                savePO();

                if (pos_to_reviews.length > 0) {
                    loadNextPO();
                } else {
                    $("#po_review_modal").modal('hide');
                    clearPOForm();
                }
            });
            $("#invoice_status").click(function () {
                $('.invoice_status input').prop('checked', $(this).is(':checked'));
            });
            $(".invoice_confirm").click(function () {
                var action = $(this).data("action");
                var count = $(".invoice_status input[type='checkbox']:checked").length;
                if (count == 0) return;
                var text = "Are you sure you want to " + action + " this invoice?";
                if (count > 1) text = "Are you sure you want to " + action + " " + count + " invoices?";
                return confirm(text);
            });
            $("#req_lookup").click(function () {
                $("#req_lookup_modal").modal('show');
            });
        });
        function formatDate(dt) {
            var d = new Date(parseInt(dt.replace("/Date(", "").replace(")/", ""), 10))
            var day = d.getDate();
            var month = d.getMonth() + 1;
            var year = d.getFullYear();
            return month + '/' + day + '/' + year;
        }

        function savePO() {
            $.ajax({
                url: 'EDIPurchaseOrders.aspx',
                dataType: 'json',
                data: {
                    action: "po-save",
                    id: current_id,
                    reviewStatus: $("input[@name=pof-status]:checked").val(),
                    applicationID: $("input[@name=pof-application]:checked").val(),
                    notes: $("#pof-notes").val(),
                    account: $("#pof-account").val(),
                    poRequestNumber: $("#pof-po-request-number").val()
                }
            }).done(function (obj) {
                if (obj === 0) return;
                if ("error" in obj) {
                    alert("An error occurred while saving the " +
                        "purchase order. Error: " + obj.error);
                    return;
                }


            });
        }
        function clearPOForm() {
            current_id = 0;
            $("#pof-error").hide();
            $("#pof-application-unix").attr("checked", "checked");
            $("#pof-status-nonreviewed").attr("checked", "checked");
            $("#pof-customer-name").html("");
            $("#pof-customer-po").html("");
            $("#pof-requested-date").html("");
            $("#pof-account").val("");
            $("#pof-po-request-number").val("");
            $("#pof-address1").html("");
            $("#pof-address2").html("");
            $("#pof-address3").html("");
            $("#pof-notes").val("");
            $("#pof-details tbody").html("");
        }

        function loadNextPO()
        {
            if (pos_to_reviews.length == 0) {
                return;
            } else if (pos_to_reviews.length == 1) {
                $("#po_review_save").html("Save &amp; Close");
            } else {
                $("#po_review_save").html("Save &amp; Next");
            }

            // get the current PO id.
            var id = pos_to_reviews.pop();

            $.ajax({
                url: 'EDIPurchaseOrders.aspx',
                dataType: 'json',
                data: { action: "po-get", id: id }
            }).done(function (obj) {
                if ("error" in obj) {
                    alert("An error occurred while loading the " +
                        "purchase order. Error: " + obj.error);
                    return;
                }

                // handle the rest.
                if (obj.HasIssues) $("#pof-error").show();
                else $("#pof-error").hide();

                current_id = id;
                
                if (obj.ReviewStatusID == 2) {
                    $("#pof-status-container").hide();
                    $("#pof-status-message").html("Accepted");
                    $("#pof-status-accept").attr("checked", "checked");
                } else if (obj.ReviewStatusID == 3) {
                    $("#pof-status-container").hide();
                    $("#pof-status-message").html("Rejected");
                    $("#pof-status-reject").attr("checked", "checked");
                } else {
                    $("#pof-status-message").html("");
                    $("#pof-status-container").show();
                    $("#pof-status-nonreviewed").attr("checked", "checked");
                }

                if (obj.ApplicationID == 1)
                    $("#pof-application-unix").attr("checked", "checked");
                else
                    $("#pof-application-instadose").attr("checked", "checked");

                $("#pof-customer-name").html(obj.CustomerName);
                $("#pof-customer-po").html(obj.PONumber);
                $("#pof-requested-date").html(formatDate(obj.PORequestDate));
                $("#pof-account").val(obj.Account);
                $("#pof-po-request-number").val(obj.PORequestNumber);
                $("#pof-address1").html(obj.Address1);
                $("#pof-address2").html(obj.Address2);
                $("#pof-address3").html(obj.City + ", " + obj.StateProvinceCode + " " + obj.PostalCode);
                $("#pof-notes").val(obj.Notes);
                $("#pof-details tbody").html("");

                $.each(obj.details, function (idx, detail) {
                    var alt = (idx % 2 == 1) ? " class=\"Alt\"" : "";
                    var row = "<tr" + alt + "><td>" + detail.PODetailLineNum + "</td><td>" + detail.Quantity +
                        "</td><td>$" + detail.UnitPrice + "</td><td>" + detail.Product.SKU +
                        "</td><td>" + detail.ItemDescription + "</td></tr>";
                    $("#pof-details tbody").append(row);
                });

            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <telerik:RadScriptManager runat="server"></telerik:RadScriptManager>
    <asp:HiddenField runat="server" ID="hfActiveGrid" Value="PO" />
    
    <div id="po_review_modal" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="po_review_modal_label" aria-hidden="true">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
            <h3 id="po_review_modal_label">Review Purchase Order #: <span id="pof-customer-po"></span> (<span id="pof-requested-date"></span>)</h3>
        </div>
        <div class="modal-body">
            <div class="FormError" id="pof-error" style="display:none;">
                <p>
                    <span class="MessageIcon"></span>
                    <strong>Messages:</strong>
                    <span id="errors">This purchase order was flagged with issues.  Please review the notes below.</span>
                </p>
            </div>
            <div class="OForm">
                <div class="Row">
                    <div class="Label">Customer Name:</div>
                    <div class="Control"><span id="pof-customer-name" class="LabelValue"></span></div>
                    <div class="Clear"></div>
                </div>
                <div class="Row">
                    <div class="Label">Address:</div>
                    <div class="Control">
                        <span id="pof-address1" class="LabelValue"></span><br />
                        <span id="pof-address2" class="LabelValue"></span><br />
                        <span id="pof-address3" class="LabelValue"></span>
                    </div>
                    <div class="Clear"></div>
                </div>
               
                <div class="Row">
                    <div class="Label">System:</div>
                    <div class="Control">
                        <input id="pof-application-unix"  type="radio" name="pof-application" value="1" />
                        <label for="pof-application-unix">Unix</label>
                        <input id="pof-application-instadose" type="radio" name="pof-application" value="2" />
                        <label for="pof-application-instadose">Instadose</label>
                    </div>
                    <div class="Clear"></div>
                </div>
                <div class="Row">
                    <div class="Label">Account #<span class="Required">*</span>:</div>
                    <div class="Control"><input type="text" value="" id="pof-account" maxlength="10" class="" /></div>
                    <div class="Clear"></div>
                </div>
                <div class="Row">
                    <div class="Label">PO Request #<span class="Required">*</span>:</div>
                    <div class="Control"><input type="text" value="" id="pof-po-request-number" maxlength="20" class="" /> <button type="button" class="btn btn-mini" id="req_lookup"><img src="/images/icons/magnifier.png" style="width:16px; height: 16px; border:0;  width: 16px; margin: 3px -2px -3px -2px;" alt="Lookup" /></button></div>
                    <div class="Clear"></div>
                </div>
                <div class="Row">
                    <div class="Label">Status:</div>
                    <div class="Control">
                        <div id="pof-status-container">
                            <input id="pof-status-accept"  type="radio" name="pof-status" value="2" />
                            <label for="pof-status-accept">Accept</label>
                            <input id="pof-status-reject" type="radio" name="pof-status" value="3" />
                            <label for="pof-status-reject">Reject</label>
                            <input id="pof-status-nonreviewed" type="radio" checked="checked" name="pof-status" value="1" />
                            <label for="pof-status-nonreviewed">Not Reviewed</label>
                        </div>
                        <span class="LabelValue" id="pof-status-message"></span>
                    </div>
                    <div class="Clear"></div>
                </div>
                <div class="Row">
                    <div class="Label">Notes:</div>
                    <div class="Control">
                        <textarea id="pof-notes" class="Size Large2" style="height:70px"></textarea>
                    </div>
                    <div class="Clear"></div>
                </div>
                 <div class="Row">
                    <div class="Label"></div>
                    <div class="Control">
                    <table class="OTable" id="pof-details" style="width:480px">
                        <thead>
                            <tr>
                                <th>#</th>
                                <th>Qty</th>
                                <th>Unit</th>
                                <th>Badge</th>
                                <th>Description</th>
                            </tr>
                        </thead>
                        <tbody>
                        </tbody>
                    </table>
                    </div>
                </div>
            </div>
        </div>
        <div class="modal-footer">
            <div class="Footer" style="text-align:left;">
                    <span class="Required">*</span>indicates a required field.
                </div>
			<button class="btn" data-dismiss="modal" aria-hidden="true">Cancel</button>
			<button type="button" class="btn btn-primary" id="po_review_save">Select</button>
        </div>
    </div>
    
    <div id="req_lookup_modal" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="req_lookup_modal_label" aria-hidden="true">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
            <h3 id="req_lookup_modal_label">Lookup</h3>
        </div>
        <div class="modal-body">
            content here.
        </div>
        <div class="modal-footer">
			<button class="btn" data-dismiss="modal" aria-hidden="true">Close</button>
			<button type="button" class="btn btn-primary" id="req_select">Select</button>
        </div>
    </div>

    <ul class="OToolbar">
        <li>
            <asp:Button Text="Validate" runat="server" CssClass="btn active" ID="btnValidatePO"
                OnClick="btnValidatePO_Click" CommandArgument="PO" />
        </li>
    </ul>

    <asp:Panel runat="server" ID="pnlPurchaseOrders">
        <!-- Purchase Orders -->
        <asp:HiddenField runat="server" ID="hfPOType" Value="GDS" />
        <asp:HiddenField runat="server" ID="hfPOFilterStatus" Value="1" />
        <ul class="OToolbar JoinTable">
            <li>

                <div class="btn-group">
                    <asp:Button Text="GDS" runat="server" CssClass="btn active" ID="btnPOTypeGDS"
                        OnClick="btnPOType_Click" CommandArgument="GDS" />
                    <asp:Button Text="Instadose" runat="server" CssClass="btn" ID="btnPOTypeInstadose"
                        OnClick="btnPOType_Click" CommandArgument="Instadose" />
                </div>
            </li>
            <li>
                <asp:TextBox runat="server" ID="txtPOSearch" Style="width: 300px;" placeholder="Search purchase orders..." />
                <div class="btn-group">
                    <asp:Button Text="Search" CssClass="btn btn-primary" runat="server" ID="btnPOSearch"
                        OnClick="btnPOSearch_Click" />
                    <asp:Button Text="Clear" CssClass="btn btn-danger" runat="server" ID="btnPOClear"
                        OnClick="btnPOClear_Click" />
                </div>
            </li>
            <li style="float: right;">

                <div class="btn-group">
                    <asp:Button Text="Non-Reviewed" runat="server" CssClass="btn active" ID="btnPOFilterNonReviewed"
                        OnClick="btnPOFilter_Click" CommandArgument="1" />
                    <asp:Button Text="Approved" runat="server" CssClass="btn btn-success" ID="btnPOFilterApproved"
                        OnClick="btnPOFilter_Click" CommandArgument="2" />
                    <asp:Button Text="Rejected" runat="server" CssClass="btn btn-info" ID="btnPOFilterRejected"
                        OnClick="btnPOFilter_Click" CommandArgument="3" />
                    <asp:Button Text="Errors" runat="server" CssClass="btn btn-danger" ID="btnPOFilterErrors"
                        OnClick="btnPOFilter_Click" CommandArgument="4" />
                </div>
            </li>
        </ul>
        <ec:GridViewEx ID="gvPurchaseOrders" runat="server" AutoGenerateColumns="False" CssClass="OTable"
            AlternatingRowStyle-CssClass="Alt" EmptyDataText="There are no purchase orders." AllowPaging="true"
            PageSize="20" OnSorting="gvPurchaseOrders_Sorting" GridLines="None"
            CurrentSortDirection="Ascending" CurrentSortedColumn="PONumber" 
            OnPageIndexChanging="gvPurchaseOrders_PageIndexChanging"
            SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
            SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">
            <Columns>
                <asp:TemplateField HeaderStyle-Width="20" HeaderStyle-CssClass="CenterAlign" ItemStyle-CssClass="CenterAlign">
                    <HeaderTemplate>
                        <input type="checkbox" id="po_status" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox Text="" ID="cbStatus" Checked="false" CssClass="po_status" runat="server" data-id='<%# Eval("PurchaseOrderID") %>' />
                        <asp:HiddenField runat="server" ID="hfPurchaseOrderID" Value='<%# Eval("PurchaseOrderID") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Account" HeaderText="Account" />
                <asp:BoundField DataField="PORequestNumber" HeaderText="PO Request" />
                <asp:BoundField DataField="PONumber" HeaderText="PO#" />
                <asp:BoundField DataField="PORequestDate" HeaderText="Requested" DataFormatString="{0:M/d/yyyy}" />
                <asp:BoundField DataField="CustomerName" HeaderText="Company" />
                <asp:BoundField DataField="Address1" HeaderText="Address" />
                <asp:BoundField DataField="City" HeaderText="City" />
                <asp:BoundField DataField="StateProvinceCode" HeaderText="State" />
                <asp:BoundField DataField="PostalCode" HeaderText="Zip Code" />

            </Columns>
        </ec:GridViewEx>
        <ul class="OToolbar JoinTableBottom">
            <li>
                <div class="btn-group">
                    <button type="button" data-toggle="modal" class="btn btn-success" id="po_review">Review</button>
                </div>
            </li>
        </ul>

    </asp:Panel>

    <asp:Panel runat="server" ID="pnlInvoices" Visible="false">
        <!-- Invoices -->
        <asp:HiddenField runat="server" ID="hfInvoiceType" Value="GDS" />
        <asp:HiddenField runat="server" ID="hfInvoiceFilterStatus" Value="1" />
        <ul class="OToolbar JoinTable">
            <li>

                <div class="btn-group">
                    <asp:Button Text="GDS" runat="server" CssClass="btn active" ID="btnInvoiceTypeGDS"
                        OnClick="btnInvoiceType_Click" CommandArgument="GDS" />
                    <asp:Button Text="Instadose" runat="server" CssClass="btn" ID="btnInvoiceTypeInstadose"
                        OnClick="btnInvoiceType_Click" CommandArgument="Instadose" />
                </div>
            </li>
            <li>
                <asp:TextBox runat="server" ID="txtInvoiceSearch" Style="width: 300px;" placeholder="Search invoices..." />
                <div class="btn-group">
                    <asp:Button Text="Search" CssClass="btn btn-primary" runat="server" ID="btnInvoiceSearch"
                        OnClick="btnInvoiceSearch_Click" />
                    <asp:Button Text="Clear" CssClass="btn btn-danger" runat="server" ID="btnInvoiceClear"
                        OnClick="btnInvoiceClear_Click" />
                </div>
            </li>
            <li style="float: right;">

                <div class="btn-group">
                    <asp:Button Text="Non-Reviewed" runat="server" CssClass="btn active" ID="btnInvoiceFilterNonReviewed"
                        OnClick="btnInvoiceFilter_Click" CommandArgument="1" />
                    <asp:Button Text="Approved" runat="server" CssClass="btn btn-success" ID="btnInvoiceFilterApproved"
                        OnClick="btnInvoiceFilter_Click" CommandArgument="2" />
                    <asp:Button Text="Rejected" runat="server" CssClass="btn btn-info" ID="btnInvoiceFilterRejected"
                        OnClick="btnInvoiceFilter_Click" CommandArgument="3" />
                    <asp:Button Text="Errors" runat="server" CssClass="btn btn-danger" ID="btnInvoiceFilterErrors"
                        OnClick="btnInvoiceFilter_Click" CommandArgument="4" />
                </div>
            </li>
        </ul>

        <ec:GridViewEx ID="gvInvoices" runat="server" AutoGenerateColumns="False" CssClass="OTable"
            AlternatingRowStyle-CssClass="Alt" EmptyDataText="There are no invoices." AllowPaging="true"
            PageSize="20" OnSorting="gvInvoices_Sorting" GridLines="None" AllowSorting="true"
            CurrentSortDirection="Descending" CurrentSortedColumn="InvoiceDate" 
            OnPageIndexChanging="gvInvoices_PageIndexChanging">
            <Columns>
                <asp:TemplateField HeaderStyle-Width="20" HeaderStyle-CssClass="CenterAlign" ItemStyle-CssClass="CenterAlign">
                    <HeaderTemplate>
                        <input type="checkbox" id="invoice_status" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox Text="" ID="cbStatus" Checked="false" CssClass="invoice_status" runat="server" />
                        <asp:HiddenField runat="server" ID="hfInvoiceID" Value='<%# Eval("InvoiceID") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="InvoiceNumber" HeaderText="Invoice#" SortExpression="InvoiceNumber" />
                <asp:BoundField DataField="OrderID" HeaderText="Order#" SortExpression="OrderID" />
                <asp:BoundField DataField="InvoiceDate" HeaderText="Invoiced" SortExpression="InvoiceDate" DataFormatString="{0:M/d/yyyy}" />
                <asp:BoundField DataField="PONumber" HeaderText="PO#" SortExpression="PONumber" />
                <asp:BoundField DataField="BillingCompanyName" HeaderText="Billing Company" />
                <asp:BoundField DataField="ShippingCompanyName" HeaderText="Shipping Company" />
                <asp:BoundField DataField="TotalInvoiceAmount" HeaderText="Invoice Total" HeaderStyle-CssClass="RightAlign"
                    ItemStyle-CssClass="RightAlign" DataFormatString="{0:$#,###,##0.00}" />

                <asp:BoundField DataField="ReviewedBy" HeaderText="Reviewed By" SortExpression="ReviewedBy" />
                <asp:BoundField DataField="ReviewedDate" HeaderText="Reviewed Date" SortExpression="ReviewedDate" DataFormatString="{0:M/d/yyyy}" />
                <asp:TemplateField HeaderText="Error" HeaderStyle-Width="40">
                    <ItemTemplate>
                        <asp:Label ID="lblReleaseNotes" Text="" CssClass="Icon Comment" runat="server" />

                        <telerik:RadToolTip ID="RadToolTip1" runat="server" TargetControlID="lblReleaseNotes"
                            RelativeTo="Element" Position="BottomCenter" AutoCloseDelay="0" Width="400" RenderInPageRoot="true">
                            <div><strong>Error Message:</strong></div>
                            <%# DataBinder.Eval(Container, "DataItem.TransferErrorMessage")%>
                        </telerik:RadToolTip>

                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </ec:GridViewEx>
        <ul class="OToolbar JoinTableBottom">
            <li>
                <div class="btn-group">
                    <asp:Button Text="Approve" runat="server" CssClass="btn btn-success invoice_confirm"
                        data-action="approve" ID="btnInvoiceReviewApprove"
                        OnClick="btnInvoiceReview_Click" CommandArgument="2" />
                    <asp:Button Text="Reject" runat="server" CssClass="btn btn-info invoice_confirm"
                        data-action="reject" ID="btnInvoiceReviewReject"
                        OnClick="btnInvoiceReview_Click" CommandArgument="3" />
                </div>
            </li>
        </ul>
    </asp:Panel>
    
    <asp:Panel runat="server" ID="pnlAcknowledgements" Visible="false">
        <!-- Acknowledgements -->

    </asp:Panel>


</asp:Content>

