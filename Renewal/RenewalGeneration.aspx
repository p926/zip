<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true"
    Inherits="InstaDose_Finance_Renewal_RenewalGeneration" Codebehind="RenewalGeneration.aspx.cs" %>

<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Src="~/Controls/ErrorLabel.ascx" TagName="ErrorLabel" TagPrefix="id" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

    <script type="text/javascript">
        $(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequestHandler);
            endRequestHandler(null, null);
        });

        function endRequestHandler(sender, args) {
            // Set the date picker objects
            $(".date-picker").datepicker();
            // Select all items in the gridview
            $(".select-all input").change(function () {
                // Select or unselect all checkbox items in the closest table where they are not disabled.
                $(this).closest("table").find('td :checkbox:not(:disabled)').prop("checked", $(this).is(":checked"));
            });
        }
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>

    <div class="FormError" id="divErrorMessage" runat="server" visible="false">
        <p>
            <span class="MessageIcon"></span>
            <strong>Messages:</strong> <span id="errorMessage" runat="server">An error was encountered.</span>
        </p>
    </div>


    <asp:Panel runat="server" ID="pnlRenewalsTable">

        <div class="OToolbar">
            <ul>
                <li>Billing Term:</li>
                <li>
                    <asp:DropDownList ID="ddlBillingTerm" runat="server"
                        DataValueField="BillingTermID" DataTextField="BillingTermDesc">
                    </asp:DropDownList>

                </li>
                <li>Brand:</li>
                <li>
                    <asp:DropDownList ID="ddlBrand" runat="server"
                        DataValueField="BrandName" DataTextField="BrandName">
                    </asp:DropDownList>

                </li>
                <li>Customer Type:</li>
                <li>
                    <asp:DropDownList ID="ddlCustomerType" runat="server"
                        DataValueField="CustomerTypeID" DataTextField="CustomerTypeName">
                    </asp:DropDownList>

                </li>
                <li>Billing Method:</li>
                <li>
                    <asp:DropDownList ID="ddlBillingMethod" runat="server"
                        DataValueField="PaymentMethodID" DataTextField="PaymentMethodName">
                    </asp:DropDownList>

                </li>
            </ul>
        </div>

        <div class="OToolbar JoinTable">
            <ul>
                <li>Period From:</li>
                <li>
                    <asp:TextBox runat="server" ID="txtPeriodFrom" CssClass="date-picker"
                        AutoPostBack="true" Style="width: 100px" />
                </li>
                <li>To:</li>
                <li>
                    <asp:TextBox runat="server" ID="txtPeriodTo" CssClass="date-picker"
                        AutoPostBack="true" Style="width: 100px" />
                </li>
                <li>
                    <asp:LinkButton ID="lnkbtnDisplay" runat="server" ToolTip="Search"
                        CommandName="DisplayAccounts" CssClass="btn btn-success" OnClick="lnkbtnDisplay_Click">Apply Filter</asp:LinkButton>
                </li>
                <li>
                    <asp:LinkButton ID="lnkClear" runat="server" ToolTip="Clear"
                        CommandName="ClearFilters" CssClass="btn btn-danger" OnClick="lnkbtnClear_Click">Clear Filters</asp:LinkButton>
                </li>
            </ul>
        </div>

        <ec:GridViewEx ID="gvUpcomingRenewals" runat="server" AutoGenerateColumns="False"
            AllowPaging="True" AllowSorting="True" PageSize="25"
            DataKeyNames="AccountID" CurrentSortedColumn="RenewalDate" CurrentSortDirection="Ascending"
            CssClass="OTable" AlternatingRowStyle-CssClass="Alt"
            PagerSettings-Mode="NextPreviousFirstLast"
            OnSorting="gvUpcomingRenewals_Sorting" OnPageIndexChanging="gvUpcomingRenewals_PageIndexChanging"
            EmptyDataText="There are no renewals that meet the criteria you specified."
            SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
            SortOrderDescendingImagePath="~/images/icon_sort_descending.gif"
            OnSelectedIndexChanging="gvUpcomingRenewals_SelectedIndexChanging">

            <AlternatingRowStyle CssClass="Alt" />

            <Columns>
                <asp:TemplateField HeaderText="" ItemStyle-HorizontalAlign="Center">
                    <HeaderTemplate>
                        <asp:CheckBox CssClass="select-all" runat="server" Checked="false" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox runat="server" ID="cbProcess" Checked='<%# Bind("Process") %>' />
                        <asp:HiddenField runat="server" ID="hfAccountID" Value='<%# Eval("AccountID") %>' />
                    </ItemTemplate>

                    <ItemStyle HorizontalAlign="Left" />
                </asp:TemplateField>

                <asp:TemplateField HeaderText="" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>

                        <asp:Image ID="imgUnProcess" runat="server"
                            Visible="false" />
                        <asp:Image ID="imgError" runat="server" ImageUrl="/images/icons/exclamation.png"
                            Visible="false" />
                        <asp:Image ID="imgDone" runat="server" ImageUrl="/images/Success.png"
                            Visible="false" />
                        <asp:Image ID="imgReprocess" runat="server" ImageUrl="/images/Warning.png"
                            Visible="false" />
                    </ItemTemplate>

                    <ItemStyle HorizontalAlign="Left" />
                </asp:TemplateField>

                <asp:BoundField
                    DataField="AccountID" HeaderText="Account#" ReadOnly="True"
                    SortExpression="AccountID">
                    <HeaderStyle />
                    <ItemStyle />
                </asp:BoundField>

                <asp:BoundField
                    DataField="AccountName" HeaderText="Account Name"
                    SortExpression="AccountName">

                    <HeaderStyle />
                    <ItemStyle />
                </asp:BoundField>

                <asp:BoundField
                    DataField="BillingMethod" HeaderText="Method"
                    ReadOnly="True" SortExpression="BillingMethod">
                    <HeaderStyle />
                    <ItemStyle />
                </asp:BoundField>

                <asp:BoundField
                    DataField="RenewalDate" HeaderText="Renewal Date"
                    SortExpression="RenewalDate" DataFormatString="{0:d}">
                    <HeaderStyle />
                    <ItemStyle />
                </asp:BoundField>

                <asp:BoundField
                    DataField="ContractEndDate" HeaderText="Contract Ends"
                    SortExpression="ContractEndDate" DataFormatString="{0:d}">
                    <HeaderStyle />
                    <ItemStyle />
                </asp:BoundField>
                <asp:BoundField
                    DataField="CustomerType" HeaderText="Type"
                    SortExpression="CustomerType">
                    <HeaderStyle />
                    <ItemStyle />
                </asp:BoundField>
                <asp:BoundField
                    DataField="LastBilled" HeaderText="Last Billed"
                    SortExpression="LastBilled" DataFormatString="{0:d}">
                    <HeaderStyle />
                    <ItemStyle />
                </asp:BoundField>
                <asp:BoundField DataField="RenewalCode" Visible="true" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:HiddenField runat="server" ID="HidRenewalCode" Value='<%# Eval("RenewalCode") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerSettings Mode="NextPreviousFirstLast" />
            <PagerStyle />
            <RowStyle CssClass="Row" />
            <SelectedRowStyle CssClass="Row Selected" />
        </ec:GridViewEx>

        <div style="text-align: right; padding: 10px;">
            <asp:Button ID="btnGenerate2" runat="server" Text="Generate Renewals" OnClick="btnGenerate_Click" CssClass="OButton" />
        </div>

        <table class="OTable" style="width: 450px;">
            <tr>
                <th>Legend</th>
            </tr>
            <tr>
                <td>
                    <asp:Image ID="Image1" runat="server" Visible="false" />
                    &nbsp;&nbsp; No icon - Renewal has not been generated. (Not processed).
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Image ID="Image2" runat="server" ImageUrl="/images/icons/exclamation.png" Visible="true" />
                    &nbsp;&nbsp; Error in current Renewal processing.
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Image ID="Image3" runat="server" ImageUrl="/images/Warning.png" Visible="true" />
                    &nbsp;&nbsp; Re-processed as PO renewal order.
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Image ID="Image4" runat="server" ImageUrl="/images/Success.png" Visible="true" />
                    &nbsp;&nbsp; Renewal has been successfully generated. (Done).
                </td>
            </tr>
        </table>

    </asp:Panel>

    <asp:Panel runat="server" ID="pnlReview" Visible="false">
        <asp:HiddenField runat="server" ID="hfRenewalNo" Value="0" />

        <div class="title">
            <h2>Renewal#: <asp:Label ID="lblRenewalNo" runat="server" /></h2>
        </div>

        <asp:Button Text="&lt;&lt; Back" runat="server" CssClass="OButton" OnClick="btnBack_Click" ID="btnBack" />
        <h3>Renewal Review</h3>

        <ec:GridViewEx ID="gvReview" runat="server" AutoGenerateColumns="false" CssClass="OTable"
            AlternatingRowStyle-CssClass="Alt" PagerStyle-CssClass="mt-hd"
            EmptyDataText="There are no valid renewal hold orders processed." Width="100%">
            <Columns>
                <asp:BoundField DataField="AccountID" HeaderText="Acc#" />
                <asp:BoundField DataField="OrderID" HeaderText="Order#" />
                <asp:BoundField DataField="OrderStatusName" HeaderText="Status" ItemStyle-Wrap="false" />
                <asp:BoundField ItemStyle-HorizontalAlign="Right" DataField="RenewalAmount" HeaderText="Amount" DataFormatString="{0:C}" />
                <asp:BoundField DataField="BrandName" HeaderText="Brand" ItemStyle-Wrap="false" />
                <asp:BoundField DataField="BillingTermDesc" HeaderText="Term" ItemStyle-Wrap="false" />
                <asp:BoundField DataField="PaymentMethodName" HeaderText="Method" ItemStyle-Wrap="false" />
                <asp:BoundField DataField="ErrorMessage" HeaderText="Status" />
            </Columns>
        </ec:GridViewEx>
    </asp:Panel>

    <asp:Panel runat="server" ID="pnlCompleted" Visible="false">
        <div style="padding: 10px;">
            <asp:Button Text="&lt;&lt; Back" runat="server" OnClick="btnBack_Click" ID="btnBack2" />
            <br />
            <span style="font-weight: bold; font-size: 13pt;">Renewal Status</span>
        </div>

        <asp:Label ID="lblProcessingErrors" Text="" Visible="false" runat="server" CssClass="errorBox" Style="padding: 10px;" />

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

</asp:Content>

