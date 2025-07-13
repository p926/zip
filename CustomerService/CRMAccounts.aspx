<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_CRMAccounts" Codebehind="CRMAccounts.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        /* CSS for Instadose 1 & Instadose 2 Product Colors. */
        .productColor
        {
            background-color: #FFFFFF;
            color: #000000;
            padding: 7px 25px;
            text-align: center;
            text-shadow: 1px 1px 0px #333333;
            display: block;
            width: 100px;
        }

        .productColor.Blue
        {
            background-color: #357195;
            color: #FFFFFF;
        }

        .productColor.Green
        {
            background-color: #196445;
            color: #FFFFFF;
        }

        .productColor.Black
        {
            background-color: #000000;
            color: #FFFFFF;
        }

        .productColor.Pink
        {
            background-color: #dd82b2;
            text-shadow: 1px 1px 0px #FFFFFF;
            color: #000000;
        }

        .productColor.Silver
        {
            background-color: #C1C1C3;
            text-shadow: 1px 1px 0px #FFFFFF;
            color: #000000;
        }

        .productColor.Red
        {
            background-color: #C41230;
            text-shadow: 1px 1px 0px #FFFFFF;
            color: #FFFFFF;
        }

        .productColor.Orange
        {
            background-color: #F68933;
            text-shadow: 1px 1px 0px #FFFFFF;
            color: #FFFFFF;
        }

        .productColor.Gray
        {
            background-color: #3E3E3F;
            text-shadow: 1px 1px 0px #FFFFFF;
            color: #FFFFFF;
        }

        /* Override OTable Class */
        /* Orange border around RadTabContainer */
        #radTabContainer.OTable
        {
            margin: 0px 0px 0px 0px;
            padding: 5px 5px 5px 5px;
            width: auto;
        }

        /* Display CSS Class for Loading Animation. */
        #updateProgressBar
        {
            text-align: center; 
            font-family: Verdana; 
            font-size: 14px;
            line-height: 25px;
            vertical-align: top; 
            font-weight: bold; 
            color: #FF4E00; 
            height: 50px; 
            width: 100%;
        }

        /* CSS for Right Aligning "Clear Filter" link. */
        li.RightAlign
        {
            float: right;
            padding-right: 10px;
        }

        /* CSS Override for RadGrid_Default. */
        div.RadGrid_Default
        { 
            border: 1px solid #D6712D;
            color: #333333;
        }

        div.RadGrid_Default th.rgHeader 
        { 
            background: url('../css/dsd-default/images/o-toolbar.png') repeat-x #D6712D; 
            background-color: #D6712D;
            border-bottom: 1px solid #D6712D;
            border-right: 1px solid #D6712D;
            font-family: Arial, sans-serif;
            font-weight: bold;
            color: #FFFFFF; 
        }

        div.RadGrid_Default th.rgHeader a 
        { 
            font-style: normal;
            color: #FFFFFF;
        }

        div.RadGrid_Default th.rgHeader a:hover 
        { 
            text-decoration: underline;
        }

        div.RadGrid_Default .rgAltRow
        {
           background-color: #f9e4cb;
           color: #333333;
        }

        /* Fixes Background Color of Hover state for Alternating Rows. */
        div.RadGrid_Default tr.rgAltRow:hover,
        div.RadGrid_Default tr.rgAltRow:active
        {
            background-color: #C3C3C3 !important;
        }

        div.RadGrid_Default tr.rgSelectedRow
        {
            background-color: #808080 !important;
            color: #FFFFFF;
        }
    </style>
    <script type="text/javascript">
        function pageLoad(sender, args) {
            $(document).ready(function () {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
                JQueryControlsLoad();
            });
        }

        function JQueryControlsLoad() {
            // Accordion
            var activeIndex = parseInt($('#<%= hdnfldAccordionIndex.ClientID %>').val());

            // Accordion for Reviewing CRM Account Information Modal/Dialog.
            $("#accordion").accordion({
                header: "h3",
                autoHeight: false,
                active: activeIndex,
                change: function (event, ui) {
                    var index = $(this).accordion("option", "active");
                    $('#<%= hdnfldAccordionIndex.ClientID %>').val(index);
                }
            });

            // Modal/Dialog for Reviewing CRM Account Information.
            $('#divApproveDeclineCRMAccount').dialog({
                modal: true,
                autoOpen: false,
                width: 700,
                resizable: false,
                title: "Review CRM Account Information",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "Approve": function () {
                        $('#<%= btnApprove.ClientID %>').click();
                    },
                    "Decline": function () {
                        $('#<%= btnDecline.ClientID %>').click();
                    },
                    "Edit": function () {
                        $('#<%= btnEdit.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancel.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });
        }

        // Open jQuery Modal/Dialog.
        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        }

        // Close jQuery Modal/Dialog.
        function closeDialog(id) {
            $('#' + id).dialog("close");
        }
    </script>
    <%--RADSCRIPT/JAVASCRIPT that limits the type of FILTERS--%>
    <telerik:RadCodeBlock ID="RadCodeBlock0" runat="server">
        <script type="text/javascript">
            var column = null;
            function MenuShowing(sender, args) {
                if (column == null) return;
                var menu = sender; var items = menu.get_items();
                if (column.get_dataType() == "System.String") {
                    var i = 0;
                    while (i < items.get_count()) {
                        if (!(items.getItem(i).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            var item = items.getItem(i);
                            if (item != null)
                                item.set_visible(false);
                        }
                        else {
                            var item = items.getItem(i);
                            if (item != null)
                                item.set_visible(true);
                        } i++;
                    }
                }
                if (column.get_dataType() == "System.Int32") {
                    var j = 0; while (j < items.get_count()) {
                        if (!(items.getItem(j).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            var item = items.getItem(j); if (item != null)
                                item.set_visible(false);
                        }
                        else { var item = items.getItem(j); if (item != null) item.set_visible(true); } j++;
                    }
                }
                if (column.get_dataType() == "System.DateTime") {
                    var k = 0;
                    while (k < items.get_count()) {
                        if (!(items.getItem(k).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            var item = items.getItem(k);
                            if (item != null)
                                item.set_visible(false);
                        }
                        else {
                            var item = items.getItem(k);
                            if (item != null)
                                item.set_visible(true);
                        } k++;
                    }
                }

                column = null;
                menu.repaint();
            }

            function filterMenuShowing(sender, eventArgs) {
                column = eventArgs.get_column();
            }
        </script>
    </telerik:RadCodeBlock>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <telerik:RadScriptManager ID="rsManager" runat="server" />
    <telerik:RadStyleSheetManager ID="rssManager" runat="server"></telerik:RadStyleSheetManager>
    <asp:HiddenField ID="hdnfldAccordionIndex" runat="server" Value="0" />

    <%--FORM ERROR/SUCCESS MESSAGE(S)--%>
    <div class="FormError" id="divFormError" runat="server" visible="false">
	    <p><span class="MessageIcon"></span>
	    <strong>Messages:</strong>&nbsp;<span id="spnFormErrorMessage" runat="server" >Error</span></p>
    </div>
    <div class="FormMessage" id="divFormSuccess" runat="server" visible="false">
	    <p><span class="MessageIcon"></span>
	    <strong>Messages:</strong>&nbsp;<span id="spnFormSuccessMessage" runat="server" >Success</span></p>
    </div>
    <%--END--%>

    <%--APPROVE|DECLINE CRM ACCOUNT MODAL/DIALOG--%>
    <div id="divApproveDeclineCRMAccount">
        <asp:UpdatePanel ID="updtpnlApproveDeclineCRMAccount" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldCRMAccountID" runat="server" Value="0" />
                <div id="accordion" style="margin-top: 10px;">
                    <div id="divAccountInformation" runat="server">
			            <h3><a href="#">AccountInformation</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--ROW 1--%>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:Image ID="imgBrandLogo" style="width: 256px; height: 80px;" ImageUrl="~/images/logos/mirion.png" runat="server" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 2--%>
                            <div class="Row">
                                <div class="Label">Dealer Name:</div>
                                <div class="Control">
                                    <asp:Label ID="lblDealerName" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 3--%>
                            <div class="Row">
                                <div class="Label">Account Name:</div>
                                <div class="Control">
                                    <asp:Label ID="lblAccountName" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 4--%>
                            <div class="Row">
                                <div class="Label">Company Name:</div>
                                <div class="Control">
                                    <asp:Label ID="lblCompanyName" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 5--%>
                            <div class="Row">
                                <div class="Label">Referral:</div>
                                <div class="Control">
                                    <asp:Label ID="lblReferral" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 6--%>
                            <div class="Row">
                                <div class="Label">Industry Type:</div>
                                <div class="Control">
                                    <asp:Label ID="lblIndustryType" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 7--%>
                            <div class="Row">
                                <div class="Label">Customer Type:</div>
                                <div class="Control">
                                    <asp:Label ID="lblCustomerType" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 8--%>
                            <div class="Row">
                                <div class="Label">Unix Customer Type:</div>
                                <div class="Control">
                                    <asp:Label ID="lblUnixCustomerType" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 9--%>
                            <div class="Row">
                                <div class="Label">Contract Period:</div>
                                <div class="Control">
                                    <asp:Label ID="lblContractPeriod" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </div>   
                    <div id="divBillingMethodInformation" runat="server">
			            <h3><a href="#">Billing Method Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--ROW 1--%>
                            <div class="Row">
                                <div class="Label">Billing Frequency:</div>
                                <div class="Control">
                                    <asp:Label ID="lblBillingFrequency" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 2--%>
                            <div class="Row">
                                <div class="Label">Billing Method:</div>
                                <div class="Control">
                                    <asp:Label ID="lblBillingMethod" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 3--%>
                            <div id="divPONumber" runat="server" class="Row">
                                <div class="Label">PO Number:</div>
                                <div class="Control">
                                    <asp:Label ID="lblPONumber" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>     
                            <%--ROW 4--%>
                            <div id="divCreditCardInformation" runat="server">
                                <div class="Row">
                                    <div class="Label">Credit Card Type:</div>
                                    <div class="Control">
                                        <asp:Image ID="imgCreditCardType" runat="server" style="width: 30px;" ImageUrl="~/images/ccvisa.gif" />
                                    </div>
                                    <div class="Clear"></div>
                                </div>
                                <div class="Row">
                                    <div class="Label">Name On Card:</div>
                                    <div class="Control">
                                        <asp:Label ID="lblNameOnCard" runat="server" CssClass="LabelValue" />
                                    </div>
                                    <div class="Clear"></div>
                                </div>
                                <div class="Row">
                                    <div class="Label">Credit Card Number:</div>
                                    <div class="Control">
                                        <asp:Label ID="lblCreditCardNumber" runat="server" CssClass="LabelValue" />
                                    </div>
                                    <div class="Clear"></div>
                                </div>
                                <div class="Row">
                                    <div class="Label">Expiration Date:</div>
                                    <div class="Control">
                                        <asp:Label ID="lblExpirationDate" runat="server" CssClass="LabelValue" />
                                    </div>
                                    <div class="Clear"></div>
                                </div>
                                <div class="Row">
                                    <div class="Label">CVC:</div>
                                    <div class="Control">
                                        <asp:Label ID="lblCVC" runat="server" CssClass="LabelValue" />
                                    </div>
                                    <div class="Clear"></div>
                                </div>
                            </div>
                            <%--END--%>       
                        </div>
                    </div>
                    <div id="divAdministratorInformation" runat="server">
			            <h3><a href="#">Administrator Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <%--ROW 1--%>
                            <div class="Row">
                                <div class="Label">Username:</div>
                                <div class="Control">
                                    <asp:Label ID="lblUsername" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 2--%>
                            <div class="Row">
                                <div class="Label">Security Question:</div>
                                <div class="Control">
                                    <asp:Label ID="lblSecurityQuestion" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 3--%>
                            <div class="Row">
                                <div class="Label">Security Answer:</div>
                                <div class="Control">
                                    <asp:Label ID="lblSecurityAnswer" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 4--%>
                            <div class="Row">
                                <div class="Label">Contact Name:</div>
                                <div class="Control">
                                    <asp:Label ID="lblContactName" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 5--%>
                            <div class="Row">
                                <div class="Label">Gender:</div>
                                <div class="Control">
                                    <asp:Label ID="lblGender" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 6--%>
                            <div class="Row">
                                <div class="Label">E-mail:</div>
                                <div class="Control">
                                    <asp:Label ID="lblEmail" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 7--%>
                            <div class="Row">
                                <div class="Label">Telephone:</div>
                                <div class="Control">
                                    <asp:Label ID="lblTelephone" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                            <%--ROW 7--%>
                            <div class="Row">
                                <div class="Label">Fax:</div>
                                <div class="Control">
                                    <asp:Label ID="lblFax" runat="server" CssClass="LabelValue" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--END--%>
                        </div>
                    </div>
                    <div id="divBillingAndShippingInformation" runat="server">
			            <h3><a href="#">Billing & Shipping Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <div class="Row">
                                <asp:GridView ID="gvLocations" runat="server" CssClass="OTable" 
                                AlternatingRowStyle-CssClass="Alt" AllowSorting="False" AllowPaging="False"
                                AutoGenerateColumns="false" DataKeyNames="CRMLocationID" 
                                PageSize="10" Width="100%">
                                    <AlternatingRowStyle CssClass="Alt" />
                                    <Columns>
                                        <asp:BoundField DataField="CRMLocationID" Visible="false" />
                                        <asp:BoundField DataField="CRMAccountID" Visible="false" />
                                        <asp:TemplateField HeaderText="Billing Address">
                                            <ItemTemplate>
                                                <asp:Label ID="lblBillingCompanyName" runat="server" Text='<%# Eval("BillingCompanyName") %>'></asp:Label><br />                
                                                <asp:Label ID="lblBillingContactName" runat="server" Text='<%# Eval("BillingContactName") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingAddress" runat="server" Text='<%# Eval("BillingAddress") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingCityStatePostalCode" runat="server" Text='<%# Eval("BillingCityStatePostalCode") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingCountry" runat="server" Text='<%# Eval("BillingCountry") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingEmailAddress" runat="server" Text='<%# Eval("BillingEmailAddress") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingTelephone" runat="server" Text='<%# Eval("BillingTelephone") %>'></asp:Label><br />
                                                <asp:Label ID="lblBillingFax" runat="server" Text='<%# Eval("BillingFax") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Shipping Address">
                                            <ItemTemplate>
                                                <asp:Label ID="lblShippingCompanyName" runat="server" Text='<%# Eval("ShippingCompanyName") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingContactName" runat="server" Text='<%# Eval("ShippingContactName") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingAddress" runat="server" Text='<%# Eval("ShippingAddress") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingCityStatePostalCode" runat="server" Text='<%# Eval("ShippingCityStatePostalCode") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingCountry" runat="server" Text='<%# Eval("ShippingCountry") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingEmailAddress" runat="server" Text='<%# Eval("ShippingEmailAddress") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingTelephone" runat="server" Text='<%# Eval("ShippingTelephone") %>'></asp:Label><br />
                                                <asp:Label ID="lblShippingFax" runat="server" Text='<%# Eval("ShippingFax") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="Active" Visible="false" />
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div style="text-align: center;">There are no records found!</div>
                                    </EmptyDataTemplate>
                                    <PagerSettings PageButtonCount="10" />
                                </asp:GridView>
                            </div>
                        </div>
                    </div>
                    <div id="divOrderInformation" runat="server">
			            <h3><a href="#">Order Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <div class="Row">
                                <asp:GridView ID="gvOrders" runat="server" CssClass="OTable" 
                                AlternatingRowStyle-CssClass="Alt" AllowSorting="False" AllowPaging="False"
                                AutoGenerateColumns="false" DataKeyNames="CRMOrderID" 
                                PageSize="10" Width="100%">
                                    <AlternatingRowStyle CssClass="Alt" />
                                    <Columns>
                                        <asp:BoundField DataField="CRMOrderID" Visible="false" />
                                        <asp:BoundField DataField="CRMAccountID" Visible="false" />
                                        <asp:BoundField DataField="ProductGroupName" HeaderText="Group" />
                                        <asp:BoundField DataField="ProductName" HeaderText="Name" />
                                        <asp:TemplateField HeaderText="Color & SKU" HeaderStyle-HorizontalAlign="Left">
                                            <ItemTemplate>
                                                <asp:Label ID="lblProductColor" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"ProductSKU","" )%>'
                                                CssClass='<%#string.Format("productColor {0}", DataBinder.Eval(Container.DataItem,"Color","" ))%>' ToolTip='<%# string.Format("{0} {1}", Eval("Color"), Eval("ProductSKU")) %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="Quantity" HeaderText="Qty.">
                                            <ItemStyle HorizontalAlign="Center" CssClass="CenterAlign" />
                                            <HeaderStyle HorizontalAlign="Center" CssClass="CenterAlign" />
                                        </asp:BoundField>
                                        <asp:BoundField DataField="UnitPrice" HeaderText="Unit Price" DataFormatString="{0:c}">
                                            <ItemStyle HorizontalAlign="Right" CssClass="RightAlign" />
                                            <HeaderStyle HorizontalAlign="Right" CssClass="RightAlign" />
                                        </asp:BoundField>
                                        <asp:BoundField DataField="Active" Visible="false" />
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div style="text-align: center;">There are no records found!</div>
                                    </EmptyDataTemplate>
                                    <PagerSettings PageButtonCount="10" />
                                </asp:GridView>
                            </div>
                        </div>
                    </div>
                    <asp:Button ID="btnApprove" runat="server" Text="Approved" style="display:none;" OnClick="btnApprove_Click"></asp:Button>
                    <asp:Button ID="btnDecline" runat="server" Text="Declined" style="display:none;" OnClick="btnDecline_Click"></asp:Button>
                    <asp:Button ID="btnEdit" runat="server" Text="Edit" style="display:none;" OnClick="btnEdit_Click"></asp:Button>
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" style="display:none;" OnClick="btnCancel_Click"></asp:Button>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--RADAJAXLOADINGPANEL ANIMATION (Enclosed in a HTML Table to Center on MultiPage)--%>
    <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server" SkinID="Default" EnableSkinTransparency="true"
    Transparency="20" BackColor="#FFFFFF" ZIndex="100" BackgroundPosition="Center">
        <table style="width:100%; height:100%;">
            <tr style="height:100%">
                <td style="width: 100%; vertical-align: central; text-align: center;">
                    <asp:Label ID="lblLoading" runat="server" ForeColor="Black" Text="Loading..." Font-Bold="true" Font-Size="Medium" />
                    <br /><br />
                    <asp:Image ID="imgLoading" runat="server" Width="128px" Height="12px" ImageUrl="../../images/orangebarloader.gif" />
                </td>
            </tr>
        </table>
    </telerik:RadAjaxLoadingPanel>
    <%--END--%>

    <%--RAD AJAX MANAGER - Handles RAD CONTROLS/LOADING PANELS for Ajax Updating--%>
    <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server">  
        <AjaxSettings>
            <telerik:AjaxSetting AjaxControlID="rgCRMAccounts">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgCRMAccounts" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
        </AjaxSettings>
    </telerik:RadAjaxManager>
    <%--END--%>

    <div id="divMainContent">
        <div style="width: 100%;">
            <div class="OToolbar JoinTable" style="padding-top: 10px; padding-bottom: 10px;">
                <ul>
                    <li>
                        <asp:LinkButton ID="lnkbtnAddCRMAccount" runat="server" Font-Bold="true" ForeColor="#FFFFFF"  
			            CommandName="AddCRMAccount" CommandArgument='' ToolTip="Add CRM Account" 
			            CssClass="Icon Add" OnClick="lnkbtnAddCRMAccount_Click">Add CRM Account</asp:LinkButton>
                    </li>
                    <li class="RightAlign">
                        <asp:LinkButton ID="lnkbtnClearFilters" runat="server" OnClick="lnkbtnClearFilters_Click" 
                        Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                    </li>
                    <li class="Clear"></li>
                </ul>
            </div>
            <%--CRM ACCOUNTS RADGRID--%>
            <telerik:RadGrid ID="rgCRMAccounts" runat="server" 
            CssClass="OTable"
            SkinID="Default"
            AllowMultiRowSelection="false"
            AutoGenerateColumns="false"
            AllowPaging="true"
            AllowSorting="true"
            AllowFilteringByColumn="true"
            ShowStatusBar="true"
            EnableLinqExpressions="false" 
            OnNeedDataSource="rgCRMAccounts_NeedDataSource"
            OnItemCommand="rgCRMAccounts_ItemCommand" 
            OnItemDataBound="rgCRMAccounts_ItemDataBound"
            PageSize="20" Width="99.8%">
                <PagerStyle Mode="NumericPages" />
                <GroupingSettings CaseSensitive="false" />
                <ClientSettings EnableRowHoverStyle="true">
                    <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                    <Selecting AllowRowSelect="true" />
                    <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                </ClientSettings>
                <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
                <MasterTableView DataKeyNames="CRMAccountID" TableLayout="Fixed" AllowMultiColumnSorting="false" AutoGenerateColumns="false">
                    <Columns>
                        <telerik:GridBoundColumn DataField="CRMAccountID" UniqueName="CRMAccountID" Visible="false"></telerik:GridBoundColumn>
                        <telerik:GridDateTimeColumn DataField="CreatedDate" UniqueName="CreatedDate" HeaderText="Created On" SortExpression="CreatedDate" 
                        AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="150px" FilterControlWidth="115px" DataFormatString="{0:d}" />
                        <telerik:GridBoundColumn DataField="BrandName" UniqueName="BrandName" HeaderText="Brand" HeaderStyle-Width="110px" ItemStyle-Width="100px" AllowSorting="true" SortExpression="CreatedDate">
                            <FilterTemplate>
                                <telerik:RadComboBox ID="rcbBrandName" DataSourceID="sqlBrandName" DataTextField="BrandName"
                                    DataValueField="BrandName" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("BrandName").CurrentFilterValue %>'
                                    runat="server" OnClientSelectedIndexChanged="BrandNameIndexChanged" Width="100px">
                                    <Items>
                                        <telerik:RadComboBoxItem Text="All" />
                                    </Items>
                                </telerik:RadComboBox>
                                <telerik:RadScriptBlock ID="rsbBrandName" runat="server">
                                    <script type="text/javascript">
                                        function BrandNameIndexChanged(sender, args) {
                                            var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                            tableView.filter("BrandName", args.get_item().get_value(), "EqualTo");
                                        }
                                    </script>
                                </telerik:RadScriptBlock>
                            </FilterTemplate>
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn DataField="CompanyName" UniqueName="CompanyName" HeaderText="Company" AllowSorting="true" SortExpression="CompanyName" FilterControlWidth="85px">
                            <ItemStyle Width="125px" />
                            <HeaderStyle Width="125px" />
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn DataField="SRDCompanyName" UniqueName="SRDCompanyName" HeaderText="Referral" AllowSorting="true" SortExpression="SRDCompanyName">
                            <FilterTemplate>
                                <telerik:RadComboBox ID="rcbSRDCompanyName" DataSourceID="sqlSRDCompanyName" DataTextField="SRDCompanyName"
                                    DataValueField="SRDCompanyName" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("SRDCompanyName").CurrentFilterValue %>'
                                    runat="server" OnClientSelectedIndexChanged="SRDCompanyNameIndexChanged" Width="175px">
                                    <Items>
                                        <telerik:RadComboBoxItem Text="All" />
                                    </Items>
                                </telerik:RadComboBox>
                                <telerik:RadScriptBlock ID="rsbSRDCompanyName" runat="server">
                                    <script type="text/javascript">
                                        function SRDCompanyNameIndexChanged(sender, args) {
                                            var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                            tableView.filter("SRDCompanyName", args.get_item().get_value(), "EqualTo");
                                        }
                                    </script>
                                </telerik:RadScriptBlock>
                            </FilterTemplate>
                            <ItemStyle Width="175px" />
                            <HeaderStyle Width="175px" />
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn DataField="IndustryName" UniqueName="IndustryName" HeaderText="Industry" AllowSorting="true" SortExpression="IndustryName">
                            <FilterTemplate>
                                <telerik:RadComboBox ID="rcbIndustryName" DataSourceID="sqlIndustryName" DataTextField="IndustryName"
                                    DataValueField="IndustryName" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("IndustryName").CurrentFilterValue %>'
                                    runat="server" OnClientSelectedIndexChanged="IndustryNameIndexChanged" Width="100px">
                                    <Items>
                                        <telerik:RadComboBoxItem Text="All" />
                                    </Items>
                                </telerik:RadComboBox>
                                <telerik:RadScriptBlock ID="rsbIndustryName" runat="server">
                                    <script type="text/javascript">
                                        function IndustryNameIndexChanged(sender, args) {
                                            var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                            tableView.filter("IndustryName", args.get_item().get_value(), "EqualTo");
                                        }
                                    </script>
                                </telerik:RadScriptBlock>
                            </FilterTemplate>
                            <ItemStyle Width="100px" />
                            <HeaderStyle Width="110px" />
                        </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn DataField="CRMACCTFullName" UniqueName="CRMACCTFullName" HeaderText="Contact" AllowSorting="true" SortExpression="CRMACCTFullName">
                            <ItemStyle Width="100px" />
                            <HeaderStyle Width="110px" />
                        </telerik:GridBoundColumn>
                        <telerik:GridTemplateColumn DataField="CRMAccountID" AllowFiltering="false">
                            <ItemTemplate>
                                <asp:Label ID="lblStatus" runat="server" Text="" Font-Bold="true" Visible="false"></asp:Label>
                                <asp:Button ID="btnReview" runat="server" Text="Review" CommandName="ReviewCRMAccount" CommandArgument='<%# DataBinder.Eval(Container.DataItem,"CRMAccountID","") %>' Visible="false" />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" Width="75px" />
                            <HeaderStyle Width="75px" />
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn DataField="CRMACCTStatus" UniqueName="CRMACCTStatus" Visible="false"></telerik:GridBoundColumn>
                        <telerik:GridBoundColumn DataField="Active" UniqueName="Active" Visible="false"></telerik:GridBoundColumn>
                    </Columns>
                </MasterTableView>
            </telerik:RadGrid>						   
            <%--END--%>
        </div>
    </div>
    <%--BRANDNAME FILTER--%>
    <asp:SqlDataSource ID="sqlBrandName" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
    SelectCommand="SELECT DISTINCT BrandName FROM BrandSources WHERE BrandSourceID <> 1 ORDER BY BrandName ASC">
    </asp:SqlDataSource>
    <%--END--%>
    <%--SRDCOMPANYNAME FILTER--%>
    <asp:SqlDataSource ID="sqlSRDCompanyName" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
    SelectCommand="SELECT (SalesRepDistID + '-' + CompanyName) AS SRDCompanyName FROM SalesRepDistributors ORDER BY CompanyName ASC">
    </asp:SqlDataSource>
    <%--END--%>
    <%--INDUSTRYNAME FILTER--%>
    <asp:SqlDataSource ID="sqlIndustryName" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString  %>"
    SelectCommand="SELECT DISTINCT IndustryName FROM Industries ORDER BY IndustryName ASC">
    </asp:SqlDataSource>
    <%--END--%>
</asp:Content>

