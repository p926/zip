<%@ Page Title="Home" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master"
    AutoEventWireup="true" Inherits="InstaDose_InformationFinder_Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
<style type="text/css">
    .FieldIcon {
        border:0;
        width: 16px;
        height: 16px;
    }
    .FieldName {
        padding: 0px 0 0px 6px;
        display: inline-block;
        text-decoration: underline;
    }
</style>
<script type="text/javascript">
     $(document).ready(function () {           
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();                                
    });

    function JQueryControlsLoad() {
         $('#resultsDialog').dialog({
            autoOpen: false,
            width: 700,            
            resizable: false,
            modal: true,
            title: "Search Results",
            open: function (type, data) {
                $(this).parent().appendTo("form");
                $('.ui-dialog :input').focus();
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

        $('#shipmentTrackingNumberSearchResultsDialog').dialog({
            autoOpen: false,
            width: 700,            
            resizable: false,
            modal: true,
            title: "Search Tracking Number Results",
            open: function (type, data) {
                $(this).parent().appendTo("form");
                $('.ui-dialog :input').focus();
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
	}
    
    function openDialog(id) {
        $('.ui-overlay').fadeIn();
        $('#' + id).dialog("open");
    }

    function closeDialog(id) {
        $('#' + id).dialog("close");
    }
</script>
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="primaryHolder" runat="Server">
    
    <telerik:RadScriptManager ID="rsManager" runat="server" AsyncPostBackTimeout="1800" />
    <%--RADAJAXLOADINGPANEL ANIMATION (Enclosed in a HTML Table to Center on MultiPage)--%>
    <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server" SkinID="Default" EnableSkinTransparency="true"
    Transparency="20" BackColor="#FFFFFF" ZIndex="100" BackgroundPosition="Center">
        <table style="width:100%; height:100%;">
            <tr style="height:100%">
                <td style="width: 100%; vertical-align: central; text-align: center;">
                    <asp:Label ID="lblSearching" runat="server" ForeColor="Black" Text="Searching..." Font-Bold="true" Font-Size="Medium" />
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
            <%--QUICK SEARCH--%>
            <telerik:AjaxSetting AjaxControlID="btnSearch">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rapQuickSearchArea" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>

            <%--Tracking# SEARCH--%>
            <telerik:AjaxSetting AjaxControlID="btnFind">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rapQuickSearchArea" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>

            <%--Tracking# SEARCH--%>
            <telerik:AjaxSetting AjaxControlID="btnDetails">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rapQuickSearchArea" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
            <%--END--%>
        </AjaxSettings> 
    </telerik:RadAjaxManager>
    <%--END--%>

    <%--QUICK SEARCH RESULTS MODAL/DIALOG--%>
    <%--02/23/2015 by Anuradha Nandi - Added the "Type" attribute to display so "Active" flag is more clear to CS User.--%>
    <div id="resultsDialog">
        <asp:UpdatePanel ID="upnlResultsDialog" runat="server" >
            <ContentTemplate>
                <div style="padding-bottom: 5px;">
                    <asp:Label ID="lblResults" runat="server" Text="Results found: 0"></asp:Label>
                </div>
                <div style="overflow-x: none; overflow-y: scroll; width: 100%; height: 350px;">
                    <asp:GridView runat="server" ID="gvSearchResults" AlternatingRowStyle-CssClass="Alt" CssClass="OTable" AutoGenerateColumns="false">
                        <Columns>
                            <asp:TemplateField HeaderText="Name">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hlFieldLink" runat="server" NavigateUrl='<%#Eval("Hyperlink")%>'>
                                        <asp:Image ImageUrl='<%# Eval("IconPath")%>' ID="imgFieldIcon" CssClass="FieldIcon"  runat="server" />
                                        <asp:Label Text='<%#Eval("Name")%>' runat="server" ID="lblFieldName" CssClass="FieldName" />
                                    </asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Acc#" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="40">
                                <ItemTemplate>
                                    <asp:HyperLink ID="HyperLink1" NavigateUrl='<%# string.Format("Details/Account.aspx?ID={0}", Eval("AccountID"))%>' Text='<%#Eval("AccountID")%>' runat="server"></asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="BrandName" HeaderText="Brand" HeaderStyle-Width="75">
                                <ItemStyle CssClass="CenterAlign" />
                                <HeaderStyle CssClass="CenterAlign" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Type" HeaderText="Type">
                                <ItemStyle CssClass="CenterAlign" />
                                <HeaderStyle CssClass="CenterAlign" />
                            </asp:BoundField>
                            <asp:BoundField HeaderText="Active" DataField="ActiveText" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="40" />
                        </Columns>
                    </asp:GridView>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--TRACKING# SEARCH RESULTS MODAL/DIALOG--%>
    <div id="shipmentTrackingNumberSearchResultsDialog">
        <asp:UpdatePanel ID="upnlShipmentTrackingNumberSearchResultsDialog" runat="server" >
            <ContentTemplate>
                <div style="padding-bottom: 5px;">
                    <asp:Label ID="lblShipmentTrackingSearchResults" runat="server" Text="Results found: 0"></asp:Label>
                </div>
                <div style="overflow-x: none; overflow-y: scroll; width: 100%; height: 350px;">
                    <asp:GridView runat="server" ID="gvTrackingSearchResults" AlternatingRowStyle-CssClass="Alt" CssClass="OTable" AutoGenerateColumns="false">
                        <Columns>
                            <asp:TemplateField HeaderText="Order#" HeaderStyle-Width="40">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hlOrderDetailLink" runat="server" NavigateUrl='<%# string.Format("../CustomerService/ReviewOrder.aspx?ID={0}", Eval("OrderID"))%>' Text='<%#Eval("OrderID")%>' >                                                                                
                                    </asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>                            
                            <asp:BoundField DataField="AccountID" HeaderText="Acc#" HeaderStyle-Width="40" />                        
                            <asp:BoundField DataField="TrackingNumber"  HeaderText="Tracking#" HeaderStyle-Width="200" />
                        </Columns>
                    </asp:GridView>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--MAIN PAGE CONTENT--%>
    <div id="divMainPageContent" style="margin: 10px auto; width: 650px;">
        
        <telerik:RadAjaxPanel ID="rapQuickSearchArea" runat="server">

            <%--QUICK SEARCH TABLE/AREA w/ TELERIK LOADING PANEL--%>
            <table class="OTable">
                <tr><th>Quick Search:</th></tr>
                <tr>
                    <td style="text-align: center;">
                        <asp:Panel ID="pnlQuickSearch" runat="server" DefaultButton="btnSearch">
                            <asp:TextBox ID="txtName" runat="server" style="width: 515px"></asp:TextBox>
                            <asp:Button ID="btnSearch" runat="server" OnClick="btnSearch_Click" CssClass="OButton" Text="Search" />
                        </asp:Panel>
                    </td>
                </tr>                
            </table>            
            <%--END--%>

            <%--FIND AN ACCOUNT BY TABLE/AREA--%>
            <asp:Panel ID="pnlFindAnAccountBy" runat="server" DefaultButton="btnFind">
                <table class="OTable" style="width: 320px; float:left;">
                    <tr><th colspan="2">Find an Account By:</th></tr>
                    <tr>
                        <td class="Label" style="width: 110px;">Account #:</td>
                        <td>
                            <asp:TextBox ID="txtAccountNo" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">C/S Request #:</td>
                        <td>
                            <asp:TextBox ID="txtCSRequestNo" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">Invoice #:</td>
                        <td>
                            <asp:TextBox ID="txtInvoiceNo" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">Order #:</td>
                        <td>
                            <asp:TextBox ID="txtOrderNo" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">PO #:</td>
                        <td>
                            <asp:TextBox ID="txtPONo" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">Serial #:</td>
                        <td>
                            <asp:TextBox ID="txtDeviceNo" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">User Name:</td>
                        <td>
                            <asp:TextBox ID="txtUserName" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" style="text-align: right;">
                            <asp:Label ID="lblFindAccountError" runat="server" CssClass="error"></asp:Label>
                            <asp:Button ID="btnFind" runat="server" CssClass="OButton" OnClick="btnFind_Click" Text="Find" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <%--END--%>

            <%--GET THE DETAILS--%>
            <asp:Panel ID="pnlGetTheDetails" runat="server" DefaultButton="btnDetails">
                <table class="OTable" style="width: 320px; float:left; margin-left: 10px;"> 
                    <tr><th colspan="2">Get the Details:</th></tr>
                    <tr>
                        <td class="Label">C/S Request #:</td>
                        <td>
                            <asp:TextBox ID="txtCSRequestNoDetails" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">Order #:</td>
                        <td>
                            <asp:TextBox ID="txtOrderNoDetails" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">RMA #:</td>
                        <td>
                            <asp:TextBox ID="txtReturnRequestNoDetails" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label" style="width: 110px;">Serial #:</td>
                        <td>
                            <asp:TextBox ID="txtDeviceNoDetails" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label" style="width: 110px;">Tracking #:</td>
                        <td>
                            <asp:TextBox ID="txtTrackingNumber" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="Label">User Name:</td>
                        <td>
                            <asp:TextBox ID="txtUserNameDetails" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" style="text-align: right;">
                            <asp:Label ID="lblFindDetailsError" runat="server" CssClass="error"></asp:Label>
                            <asp:Button ID="btnDetails" runat="server" CssClass="OButton" OnClick="btnDetails_Click" Text="Details" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <%--END--%>

            <div class="Clear"></div>

        </telerik:RadAjaxPanel>
    </div>
    <%--END--%>
</asp:Content>
