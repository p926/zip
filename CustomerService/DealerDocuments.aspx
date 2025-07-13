<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="DealerDocuments.aspx.cs" Inherits="portal_instadose_com_v3.CustomerService.DealerDocuments" %>

<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="../css/rad-controls/RadGrid.css" />
    <style type="text/css">
        /* CSS for Right Aligning "Clear Filter" link. */
        li.RightAlign
        {
            float: right;
            padding-right: 10px;
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
    </style>
    <script type="text/javascript">
        $(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(aJaxFunctions);
            aJaxFunctions();

            $('#uploadDocumentModal').dialog({
                autoOpen: false,
                resizable: false,
                width: 670,
                modal: true,
                title: 'Upload Dealer Document',
                open: function (type, data) {
                    $(this).parent().appendTo("form");

                    // Populate ddlUploadDealer DropDownList if QueryString value is present.
                    var queryStringDealerID = getQueryStrings();

                    if (queryStringDealerID === null) {
                        $('#<%= ddlUploadDealer.ClientID %>').val('0');
                    }
                    else {
                        var dealerID = queryStringDealerID['DealerID'];
                        $('#<%= ddlUploadDealer.ClientID %>').val(dealerID);
                    }
                },
                buttons: {
                    "Save": function () {
                        $('#<%= btnUploadDocument.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            $('#editDocumentModal').dialog({
                autoOpen: false,
                resizable: false,
                width: 670,
                modal: true,
                title: 'Edit a Document',
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "Update": function () {
                        $('#<%= btnEditDocument.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            // 08/15/2014 (ANANDI) - Taken out because Success/Error Message not visible behind Modal/Dialog which displays each time the page Posts Back.
            // Upon entering Page (and on PostBack) the Upload Document Modal/Dialog displays.
            //if (getQueryString("Upload") === "True") {
            //    $('#uploadDocumentModal').dialog("open");
            //}
        });

        function aJaxFunctions() {
            $(".datePicker").datepicker();
        };

        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        }

        function closeDialog(id) {
            $('#' + id).dialog("close");
        }

        function getQueryString(name) {
            name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
            var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
                results = regex.exec(location.search);
            return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
        }

        // Get QueryString value for DealerID.
        function getQueryStrings() {
            // Holds key:value pairs.
            var queryStringCollection = null;

            // Get QueryString from URL.
            var requestUrl = window.location.search.toString();

            if (requestUrl !== '') {
                // window.location.search returns the part of the URL that follows the ? symbol, including the ? symbol.
                requestUrl = requestUrl.substring(1);

                queryStringCollection = new Array();

                // Get key:value pairs from QueryString.
                var kvPairs = requestUrl.split('&');

                for (var i = 0; i < kvPairs.length; i++) {
                    var kvPair = kvPairs[i].split('=');
                    queryStringCollection[kvPair[0]] = kvPair[1];
                }
            }

            return queryStringCollection;
        }
    </script>
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
                    var h = 0;
                    while (h < items.get_count()) {
                        if (!(items.getItem(h).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'LessThan': '', 'IsNull': '' })) {
                            var item = items.getItem(h);
                            if (item != null)
                                item.set_visible(false);
                        }
                        else {
                            var item = items.getItem(h);
                            if (item != null)
                                item.set_visible(true);
                        } h++;
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
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="server">
    <telerik:RadScriptManager ID="rsManager" runat="server" />
    <telerik:RadStyleSheetManager ID="rssManager" runat="server"></telerik:RadStyleSheetManager>

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
            <telerik:AjaxSetting AjaxControlID="rgDealerDocuments">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgDealerDocuments" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="lnkbtnUploadDealerDocument">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgDealerDocuments" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
        </AjaxSettings> 
    </telerik:RadAjaxManager>
    <%--END--%>

    <%--UPLOAD DOCUMENT SUCCESS MESSAGE--%>
    <div class="FormMessage" id="uploadSuccess" runat="server" visible="false" style="text-align: left;">
        <p>
            <span class="MessageIcon"></span><strong>Messages:</strong>
            <span>The document has been uploaded successfully.</span>
        </p>
    </div>
    <%--END--%>

    <%--UPLOAD DOCUMENT ERROR MESSAGE--%>
    <div class="FormError" id="uploadError" runat="server" visible="false" style="text-align: left;">
	    <p>
            <span class="MessageIcon"></span><strong>Messages:</strong>
            <span id="uploadErrorMessage" runat="server">An error was encountered.</span></p>
    </div>
    <%--END--%>

    <%--EDIT/UPDATE DOCUMENT SUCCESS MESSAGE--%>
    <div class="FormMessage" id="editSuccess" runat="server" visible="false" style="text-align: left;">
        <p>
            <span class="MessageIcon"></span><strong>Messages:</strong>
            <span>The document has been updated successfully.</span>
        </p>
    </div>
    <%--END--%>

    <%--EDIT/UPDATE DOCUMENT ERROR MESSAGE--%>
    <div class="FormError" id="editError" runat="server" visible="false" style="text-align: left;">
	    <p>
            <span class="MessageIcon"></span><strong>Messages:</strong>
            <span id="editErrorMessage" runat="server">An error was encountered.</span></p>
    </div>
    <%--END--%>

    <%--MODAL/DIALOG FUNCTION BUTTONS--%>
    <asp:Button ID="btnUploadDocument" runat="server" OnClick="btnUploadDocument_Click" Text="Upload" UseSubmitBehavior="false" ValidationGroup="UPLOAD" style="display: none;" TabIndex="7" />
    <asp:Button ID="btnUploadDocumentCancel" runat="server" OnClick="btnUploadDocumentCancel_Click" Text="Cancel" style="display: none;" TabIndex="8" />
    <asp:Button ID="btnEditDocument" runat="server" OnClick="btnEditDocument_Click" Text="Update" UseSubmitBehavior="false" ValidationGroup="EDIT" style="display: none;" TabIndex="16" />
    <asp:Button ID="btnEditDocumentCancel" runat="server" Text="Cancel" OnClick="btnEditDocumentCancel_Click" style="display: none;" TabIndex="17" />
    <%--END--%>

    <%--UPLOAD DOCUMENT FORM--%>
    <div id="uploadDocumentModal">
        <asp:UpdatePanel ID="updtpnlUploadDocument" runat="server">
            <ContentTemplate>
                <div class="OForm">
                    <div class="Row">
                        <div class="Label">Dealer<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlUploadDealer" runat="server" AppendDataBoundItems="true" TabIndex="1">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalUploadDealer" runat="server" CssClass="InlineError" Display="Dynamic" InitialValue="0" 
                            ErrorMessage="Dealer required." ControlToValidate="ddlUploadDealer" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Category<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlUploadCategory" runat="server" AppendDataBoundItems="true" AutoPostBack="true" 
                            OnSelectedIndexChanged="ddlUploadCategory_SelectedIndexChanged" TabIndex="2">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalUploadCategory" runat="server" CssClass="InlineError" Display="Dynamic" InitialValue="0" 
                            ErrorMessage="Category required." ControlToValidate="ddlUploadCategory" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">PO #<span id="spnUploadPONumberRequired" runat="server" class="Required" visible="false">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtUploadPONumber" runat="server" MaxLength="50" TabIndex="3" />
                            <asp:RequiredFieldValidator ID="reqfldvalUploadPONumber" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="PO # required." ControlToValidate="txtUploadPONumber" ValidationGroup="UPLOAD" Enabled="false"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Document<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:FileUpload ID="fuUploadDocument" runat="server" TabIndex="4" />
                            <asp:RequiredFieldValidator ID="reqfldvalUploadDocument" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Document required." ControlToValidate="fuUploadDocument" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Description<span class="Required">*</span>:</div>
                        <div class="Control">
                            <telerik:RadEditor ID="reUploadDescription" runat="server" Height="200" Width="460" TabIndex="5" 
                            NewLineMode="P" ContentAreaMode="Div" EditModes="All" ToolsFile="~/App_Data/RadEditorTools.xml" MaxTextLength="255">
                            </telerik:RadEditor>
                            <asp:RequiredFieldValidator ID="reqfldvalUploadDescription" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Description required." ControlToValidate="reUploadDescription" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Received Date<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtUploadReceivedDate" runat="server" CssClass="Size Small datePicker" TabIndex="6" />
                            <asp:RequiredFieldValidator ID="reqfldvalUploadPublishDate" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Received date required." ControlToValidate="txtUploadReceivedDate" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Footer">
                        <span class="Required">*</span> indicates a required field.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>

    <%--EDIT DOCUMENT FORM--%>
    <div id="editDocumentModal">
        <asp:UpdatePanel ID="updtpnlEditDocument" runat="server">
            <ContentTemplate>
                <asp:HiddenField runat="server" ID="hfDocumentID" Value="0" />
                <div class="OForm">
                    <div class="Row">
                        <div class="Label">Delete:</div>
                        <div class="Control">
                            <asp:CheckBox ID="cbEditDelete" runat="server" Text="Delete this document." TabIndex="9" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Dealer<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlEditDealer" runat="server" AppendDataBoundItems="true" TabIndex="10">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalEditDealer" runat="server" CssClass="InlineError" Display="Dynamic" InitialValue="0" 
                            ErrorMessage="Dealer required." ControlToValidate="ddlEditDealer" ValidationGroup="EDIT"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Category<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlEditCategory" runat="server" AppendDataBoundItems="true" AutoPostBack="true" 
                            OnSelectedIndexChanged="ddlEditCategory_SelectedIndexChanged" TabIndex="11">
                                <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="reqfldvalEditCategory" runat="server" CssClass="InlineError" Display="Dynamic" InitialValue="0" 
                            ErrorMessage="Category required." ControlToValidate="ddlEditCategory" ValidationGroup="EDIT"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">PO #<span id="spnEditPONumberRequired" runat="server" class="Required" visible="false">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEditPONumber" runat="server" placeholder="12345" TabIndex="12" />
                            <asp:RequiredFieldValidator ID="reqfldvalEditPONumber" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="PO # required." ControlToValidate="txtEditPONumber" ValidationGroup="EDIT"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Old Document:</div>
                        <div class="Control">
                            <asp:Label ID="lblOldDocument" runat="server" CssClass="LabelValue"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">New Document:</div>
                        <div class="Control">
                            <asp:FileUpload ID="fuEditDocument" runat="server" TabIndex="13" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Description<span class="Required">*</span>:</div>
                        <div class="Control">
                            <telerik:RadEditor ID="reEditDescription" runat="server" Height="200" Width="460" TabIndex="14"
                            NewLineMode="P" ContentAreaMode="Div" EditModes="All" ToolsFile="~/App_Data/RadEditorTools.xml">
                            </telerik:RadEditor>
                            <asp:RequiredFieldValidator ID="reqfldvalEditDescription" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Description required." ControlToValidate="reEditDescription" ValidationGroup="EDIT"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Received Date<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEditReceivedDate" runat="server" CssClass="Size Small datePicker" TabIndex="15" />
                            <asp:RequiredFieldValidator ID="reqfldvalEditReceivedDate" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Received date required." ControlToValidate="txtEditReceivedDate" ValidationGroup="EDIT"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Footer">
                        <span class="Required">*</span> indicates a required field.
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%--END--%>
    
    <%--MAIN PAGE AREA W/ RADGRID--%>
    <div style="width: 100%;">
        <asp:UpdatePanel ID="updtpnlDealerDocuments" runat="server" UpdateMode="Conditional">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="lnkbtnOpenUploadDocumentModal" EventName="Click" />
				<asp:AsyncPostBackTrigger ControlID="lnkbtnClearFilters" EventName="Click" />
			</Triggers>
            <ContentTemplate>
                <div class="OToolbar JoinTable">
                    <ul>
                        <li>
                            <asp:LinkButton ID="lnkbtnOpenUploadDocumentModal" runat="server" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Add" 
			                CommandName="UploadDocument" ToolTip="Upload Dealer Document" OnClientClick="openDialog('uploadDocumentModal'); return false;">Upload Document</asp:LinkButton>
                        </li>
                        <li class="RightAlign">
                            <asp:LinkButton ID="lnkbtnClearFilters" runat="server" OnClick="lnkbtnClearFilters_Click" 
                            Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters">Clear Filters</asp:LinkButton>
                        </li>
                    </ul>
                </div>
                <asp:Panel ID="pnlRadGrid" runat="server" SkinID="Default">
                    <telerik:RadGrid ID="rgDealerDocuments" runat="server" 
                    CssClass="OTable"
                    SkinID="Default"
                    AllowMultiRowSelection="false"
                    AutoGenerateColumns="false"
                    AllowPaging="true"
                    AllowSorting="true"
                    AllowFilteringByColumn="true"
                    ShowStatusBar="true"
                    EnableLinqExpressions="false" 
                    OnNeedDataSource="rgDealerDocuments_NeedDataSource"
                    OnItemCommand="rgDealerDocuments_ItemCommand"  
                    PageSize="20" Width="99.9%">
                        <PagerStyle Mode="NumericPages" />
                        <GroupingSettings CaseSensitive="false" />
                        <ClientSettings EnableRowHoverStyle="true">
                            <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                            <Selecting AllowRowSelect="true" />
                            <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                        </ClientSettings>
                        <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
                        <MasterTableView DataKeyNames="DealerID,DocumentID" TableLayout="Fixed" AllowMultiColumnSorting="false" AutoGenerateColumns="false">
                            <Columns>
                                <telerik:GridTemplateColumn HeaderStyle-Width="30px" ItemStyle-Width="30px" AllowFiltering="false">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkbtnEdit" runat="server" ToolTip="Edit the Document" OnClick="lnkbtnEdit_Click" CommandArgument='<%# Eval("DocumentID") %>'>
                                            <asp:Image ID="imgEdit" runat="server" CssClass="Icon16" ImageUrl="~/images/icons/pencil.png" AlternateText='<%# string.Format("Edit {0}", Eval("Category")) %>' />
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                    <HeaderStyle HorizontalAlign="Center" CssClass="centeralign" />
                                    <ItemStyle HorizontalAlign="Center" CssClass="centeralign" />
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn DataField="DealerID" UniqueName="DealerID" HeaderText="Dealer #" SortExpression="DealerID" AllowFiltering="true" AllowSorting="true" 
                                ItemStyle-Width="100px" HeaderStyle-Width="100px" FilterControlWidth="75px" AutoPostBackOnFilter="true" CurrentFilterFunction="EqualTo" />
                                <telerik:GridBoundColumn DataField="DealerName" UniqueName="DealerName" HeaderText="Dealer Name" AllowFiltering="true" AllowSorting="true" SortExpression="DealerName" ItemStyle-Width="125px" HeaderStyle-Width="125px">
                                    <FilterTemplate>
                                        <telerik:RadComboBox ID="rcbDealerName" runat="server" DataTextField="DealerName" DataSourceID="SQLDSDealerNames"
                                        DataValueField="DealerName" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("DealerName").CurrentFilterValue %>'
                                        OnClientSelectedIndexChanged="DealerNameIndexChanged" Width="125px">
                                            <Items>
                                                <telerik:RadComboBoxItem Text="-Select-" Value="" />
                                            </Items>
                                        </telerik:RadComboBox>
                                        <telerik:RadScriptBlock ID="RadScriptBlock2" runat="server">
                                            <script type="text/javascript">
                                                function DealerNameIndexChanged(sender, args) {
                                                    var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                    tableView.filter("DealerName", args.get_item().get_value(), "EqualTo");
                                                }
                                            </script>
                                        </telerik:RadScriptBlock>
                                    </FilterTemplate>
                                </telerik:GridBoundColumn>
                                <telerik:GridTemplateColumn DataField="FileName" UniqueName="FileName" HeaderText="Document" AllowFiltering="true" AutoPostBackOnFilter="true" 
                                ItemStyle-Wrap="true" HeaderStyle-Width="225px" ItemStyle-Width="225px" FilterControlWidth="200px" SortExpression="FileName" CurrentFilterFunction="Contains">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="hyprlnkFileName" runat="server" NavigateUrl='<%# string.Format("../ViewDocuments.aspx?Guid={0}", Eval("DocumentGUID")) %>' Text='<%# Eval("FileName") %>'></asp:HyperLink>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn DataField="Category" UniqueName="Category" HeaderText="Category" AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="100px" ItemStyle-Width="100px" SortExpression="Category">
                                    <FilterTemplate>
                                        <telerik:RadComboBox ID="rcbCategory" runat="server" DataTextField="Category" DataSourceID="SQLDSCategory"
                                        DataValueField="Category" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("Category").CurrentFilterValue %>'
                                        OnClientSelectedIndexChanged="CategoryIndexChanged" Width="100px">
                                            <Items>
                                                <telerik:RadComboBoxItem Text="-Select-" Value="" />
                                            </Items>
                                        </telerik:RadComboBox>
                                        <telerik:RadScriptBlock ID="RadScriptBlock3" runat="server">
                                            <script type="text/javascript">
                                                function CategoryIndexChanged(sender, args) {
                                                    var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                    tableView.filter("Category", args.get_item().get_value(), "EqualTo");
                                                }
                                            </script>
                                        </telerik:RadScriptBlock>
                                    </FilterTemplate>
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="PONumber" UniqueName="PONumber" HeaderText="PO #" SortExpression="PONumber" CurrentFilterFunction="Contains"
                                AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="75px" ItemStyle-Width="75px" FilterControlWidth="50px" AutoPostBackOnFilter="true" />
                                <telerik:GridDateTimeColumn DataField="ReceivedDate" UniqueName="ReceivedDate" HeaderText="Received" SortExpression="ReceivedDate" AutoPostBackOnFilter="true"
                                AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="135px" FilterControlWidth="100px" DataFormatString="{0:MM/dd/yyyy}" CurrentFilterFunction="GreaterThanOrEqualTo" />
                            </Columns>
                        </MasterTableView>
                    </telerik:RadGrid>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div class="Buttons">
            <div class="ButtonHolder">
                <div>
                    <asp:Button ID="btnReturnTo" runat="server" CssClass="OButton" Text="Return to ICCare Dealer Maintenance" OnClick="btnReturnTo_Click"></asp:Button>    
                </div>
                <div class="Clear"></div>
            </div>
        </div>
    </div>
    <%--END--%>

    <%--CATEGORY RADCOMBOBOX FILTER--%>
    <asp:SqlDataSource ID="SQLDSCategory" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="SELECT Category FROM DealerDocumentCategories;">
    </asp:SqlDataSource>
    <%--END--%>

    <%--DEALER NAMES RADCOMBOBOX FILTER--%>
    <asp:SqlDataSource ID="SQLDSDealerNames" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="SELECT DISTINCT DealerName FROM Dealers ORDER BY DealerName ASC;">
    </asp:SqlDataSource>
    <%--END--%>
</asp:Content>
