<%@ Page Title="Manage Dosimetry Documents" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_DosimetryDocuments" Codebehind="DosimetryDocuments.aspx.cs" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
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
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(aJaxFunctions);
                aJaxFunctions();
            });
        }

        function aJaxFunctions() {
            $('#uploadDocumentModal').dialog({
                autoOpen: false,
                resizable: false,
                width: 670,
                title: 'Upload a Document',
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "Save": function () {
                        $('#<%= btnUploadDocument.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $('#<%= btnUploadDocumentCancel.ClientID %>').click();
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
                title: 'Edit a Document',
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "Update": function () {
                        $('#<%= btnEditDocument.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $('#<%= btnEditDocumentCancel.ClientID %>').click();
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            $('.datePicker').datepicker();
        };

        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        }

        function closeDialog(id) {
            $('#' + id).dialog("close");
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
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
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
            <telerik:AjaxSetting AjaxControlID="rgDosimetryDocuments">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgDosimetryDocuments" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="lnkbtnOpenUploadDocumentModal">  
                <UpdatedControls>  
                    <telerik:AjaxUpdatedControl ControlID="rgDosimetryDocuments" LoadingPanelID="RadAjaxLoadingPanel1" /> 
                </UpdatedControls> 
            </telerik:AjaxSetting>
        </AjaxSettings> 
    </telerik:RadAjaxManager>
    <%--END--%>

    <%--UPLOAD DOCUMENT SUCCESS MESSAGE--%>
    <div class="FormMessage" id="uploadSuccess" runat="server" visible="false">
        <p>
            <span class="MessageIcon"></span>
            <strong>Messages:</strong>
            <span>The document has been uploaded successfully.</span>
        </p>
    </div>
    <%--END--%>

    <%--EDIT UPLOAD DOCUMENT SUCCESS MESSAGE--%>
    <div class="FormMessage" id="editSuccess" runat="server" visible="false">
        <p>
            <span class="MessageIcon"></span>
            <strong>Messages:</strong>
            <span>The document has been updated successfully.</span>
        </p>
    </div>
    <%--END--%>

    <%--MODAL/DIALOG FUNCTION BUTTONS--%>
    <asp:Button ID="btnUploadDocument" runat="server" OnClick="btnUploadDocument_Click" UseSubmitBehavior="false" Text="Upload" TabIndex="9" style="display: none;" ValidationGroup="UPLOAD" />
    <asp:Button ID="btnUploadDocumentCancel" runat="server" OnClick="btnUploadDocumentCancel_Click" Text="Cancel" TabIndex="10" style="display: none;" />
    <asp:Button ID="btnEditDocument" runat="server" OnClick="btnEditDocument_Click" UseSubmitBehavior="false" Text="Update" TabIndex="19" style="display: none;" ValidationGroup="UPDATE" />
    <asp:Button ID="btnEditDocumentCancel" runat="server" OnClick="btnEditDocumentCancel_Click" Text="Cancel" TabIndex="20" style="display: none;" />
    <%--END--%>

    <%--UPLOAD DOCUMENT FORM--%>
    <div id="uploadDocumentModal">
        <asp:UpdatePanel ID="updtpnlUploadDocument" runat="server">
            <ContentTemplate>
                <div class="FormError" id="uploadError" runat="server" visible="false" style="text-align: left;">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong>&nbsp;<span id="uploadErrorMessage" runat="server">An error was encountered.</span></p>
                </div>
                <div class="OForm">
                    <div class="Row">
                        <div class="Label">Application:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlUploadApplication" runat="server" TabIndex="1"></asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Account #<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtUploadAccount" runat="server" placeholder="12345" TabIndex="2" />
                            <asp:RequiredFieldValidator ID="reqfldvalUploadApplication" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Account # required." ControlToValidate="txtUploadAccount" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Title<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtUploadDocumentTitle" runat="server" CssClass="Size Large" placeholder="My Document Title" TabIndex="3" />
                            <asp:RequiredFieldValidator ID="reqfldvalUploadDocumentTitle" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Title required." ControlToValidate="txtUploadDocumentTitle" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Category:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlUploadCategory" runat="server" TabIndex="4"></asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Select Document<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:FileUpload ID="fuUploadBuildFile" runat="server" TabIndex="5" />
                            <asp:RequiredFieldValidator ID="reqfldvalUploadBuildFile" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Document required." ControlToValidate="fuUploadBuildFile" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Description<span class="Required">*</span>:</div>
                        <div class="Control">
                            <telerik:RadEditor ID="reUploadDescription" runat="server" Height="200" Width="460" TabIndex="6" 
                            NewLineMode="P" ContentAreaMode="Div" EditModes="All" ToolsFile="~/App_Data/RadEditorTools.xml">
                            </telerik:RadEditor>
                            <asp:RequiredFieldValidator ID="reqfldvalUploadDescription" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Description required." ControlToValidate="reUploadDescription" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Internal Notes:</div>
                        <div class="Control">
                            <telerik:RadEditor ID="reUploadInternalNotes" runat="server" Height="200" Width="460" TabIndex="7" 
                            NewLineMode="P" ContentAreaMode="Div" EditModes="All" ToolsFile="~/App_Data/RadEditorTools.xml">
                            </telerik:RadEditor>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Publish Date<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtUploadPublishDate" runat="server" CssClass="Size Small datePicker" TabIndex="8" />
                            <asp:RequiredFieldValidator ID="reqfldvalUploadPublishDate" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Publish date required." ControlToValidate="txtUploadPublishDate" ValidationGroup="UPLOAD"></asp:RequiredFieldValidator>
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
                <div class="FormError" id="editError" runat="server" visible="false" style="text-align: left;">
	                <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong>&nbsp;<span id="editErrorMessage" runat="server">An error was encountered.</span></p>
                </div>
                <div class="OForm">
                    <div class="Row">
                        <div class="Label">Delete:</div>
                        <div class="Control">
                            <asp:CheckBox ID="cbEditDelete" runat="server" Text="Delete this document." TabIndex="11" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Application:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlEditApplication" runat="server" TabIndex="12"></asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Account #<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEditAccount" runat="server" placeholder="12345" TabIndex="13" />
                            <asp:RequiredFieldValidator ID="reqfldvalEditAccount" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Account # required." ControlToValidate="txtEditAccount" ValidationGroup="UPDATE"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Title<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEditDocumentTitle" runat="server" CssClass="Size Large" placeholder="My Document Title" TabIndex="14" />
                            <asp:RequiredFieldValidator ID="reqfldvalEditDocumentTitle" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Title required." ControlToValidate="txtEditDocumentTitle" ValidationGroup="UPDATE"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Category:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlEditCategory" runat="server" TabIndex="15"></asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Document:</div>
                        <div class="LabelValue">
                            <asp:Label ID="lblEditUploadedDocument" runat="server" Text="My File Name" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Description<span class="Required">*</span>:</div>
                        <div class="Control">
                            <telerik:RadEditor ID="reEditDescription" runat="server" Height="200" Width="460" TabIndex="16"
                            NewLineMode="P" ContentAreaMode="Div" EditModes="All" ToolsFile="~/App_Data/RadEditorTools.xml">
                            </telerik:RadEditor>
                            <asp:RequiredFieldValidator ID="reqfldvalEditDescription" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Description required." ControlToValidate="reEditDescription" ValidationGroup="UPDATE"></asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Internal Notes:</div>
                        <div class="Control">
                            <telerik:RadEditor ID="reEditInternalNotes" runat="server" Width="460" Height="200" TabIndex="17"
                            NewLineMode="P" ContentAreaMode="Div" EditModes="All" ToolsFile="~/App_Data/RadEditorTools.xml">
                            </telerik:RadEditor>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Publish Date<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEditPublishDate" runat="server" CssClass="Size Small datePicker" TabIndex="18" />
                            <asp:RequiredFieldValidator ID="reqfldvalEditPublishDate" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Publish date required." ControlToValidate="txtEditPublishDate" ValidationGroup="UPDATE"></asp:RequiredFieldValidator>
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
        <%--<asp:UpdatePanel ID="mainPageAreaWithRadGrid" runat="server">
            <ContentTemplate>--%>
                <div class="OToolbar JoinTable">
                    <ul>
                        <li>
                            <asp:LinkButton ID="lnkbtnOpenUploadDocumentModal" runat="server" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Add" 
			                CommandName="UploadDocument" ToolTip="Create Note" OnClientClick="openDialog('uploadDocumentModal'); return false;">Upload Document</asp:LinkButton>
                        </li>
                        <li class="RightAlign">
                            <asp:LinkButton ID="lnkbtnClearFilters" runat="server" OnClick="lnkbtnClearFilters_Click" 
                            Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters">Clear Filters</asp:LinkButton>
                        </li>
                    </ul>
                </div>
                <asp:Panel ID="pnlRadGrid" runat="server" SkinID="Default">
                    <telerik:RadGrid ID="rgDosimetryDocuments" runat="server" 
                    CssClass="OTable"
                    SkinID="Default"
                    AllowMultiRowSelection="false"
                    AutoGenerateColumns="false"
                    AllowPaging="true"
                    AllowSorting="true"
                    AllowFilteringByColumn="true"
                    ShowStatusBar="true"
                    EnableLinqExpressions="false" 
                    OnNeedDataSource="rgDosimetryDocuments_NeedDataSource"
                    OnItemCommand="rgDosimetryDocuments_ItemCommand"  
                    PageSize="20" Width="99.8%">
                        <PagerStyle Mode="NumericPages" />
                        <GroupingSettings CaseSensitive="false" />
                        <ClientSettings EnableRowHoverStyle="true">
                            <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                            <Selecting AllowRowSelect="true" />
                            <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                        </ClientSettings>
                        <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
                        <MasterTableView DataKeyNames="DocumentID" TableLayout="Fixed" AllowMultiColumnSorting="false" AutoGenerateColumns="false">
                            <Columns>
                                <telerik:GridTemplateColumn HeaderStyle-Width="30px" ItemStyle-Width="30px" AllowFiltering="false">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkbtnEdit" runat="server" ToolTip="Edit the Document" OnClick="lnkbtnEdit_Click" CommandArgument='<%# Eval("DocumentID") %>'>
                                            <asp:Image ID="imgEdit" runat="server" CssClass="Icon16" ImageUrl="~/css/dsd-default/images/icons/pencil.png" AlternateText='<%# string.Format("Edit {0}", Eval("DocumentTitle")) %>' />
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                    <HeaderStyle HorizontalAlign="Center" CssClass="centeralign" />
                                    <ItemStyle HorizontalAlign="Center" CssClass="centeralign" />
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn DataField="Account" UniqueName="Account" HeaderText="Account" AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="75px" ItemStyle-Width="75px" 
                                FilterControlWidth="40px" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" SortExpression="Account" />
                                <telerik:GridBoundColumn DataField="ApplicationName" UniqueName="ApplicationName" HeaderText="App." AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="100px" 
                                ItemStyle-Width="100px" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" SortExpression="ApplicationName">
                                    <FilterTemplate>
                                        <telerik:RadComboBox ID="rcbApplicationName" runat="server" DataTextField="ApplicationName" DataSourceID="SQLDSApplicationName"
                                        DataValueField="ApplicationName" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("ApplicationName").CurrentFilterValue %>'
                                        OnClientSelectedIndexChanged="ApplicationNameIndexChanged" Width="85px">
                                            <Items>
                                                <telerik:RadComboBoxItem Text="-Select-" Value="" />
                                            </Items>
                                        </telerik:RadComboBox>
                                        <telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
                                            <script type="text/javascript">
                                                function ApplicationNameIndexChanged(sender, args) {
                                                    var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                    tableView.filter("ApplicationName", args.get_item().get_value(), "EqualTo");
                                                }
                                            </script>
                                        </telerik:RadScriptBlock>
                                    </FilterTemplate>
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="DocumentTitle" UniqueName="DocumentTitle" HeaderText="Title" AllowFiltering="true" AllowSorting="true" SortExpression="DocumentTitle"
                                ItemStyle-Wrap="true" HeaderStyle-Width="150px" ItemStyle-Width="150px" FilterControlWidth="100px" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains" />
                                <telerik:GridTemplateColumn DataField="FileName" UniqueName="FileName" HeaderText="Document" AllowFiltering="true" AutoPostBackOnFilter="true" CurrentFilterFunction="Contains"
                                ItemStyle-Wrap="true" HeaderStyle-Width="150px" ItemStyle-Width="150px" FilterControlWidth="100px" SortExpression="FileName">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="hyprlnkFileName" runat="server" NavigateUrl='<%# string.Format("DosimetryDocuments.aspx?Guid={0}", Eval("DocumentGuid")) %>' Text='<%# FormatDocumentTitle(Eval("FileName").ToString()) %>'></asp:HyperLink>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn DataField="Category" UniqueName="Category" HeaderText="Category" AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="100px" ItemStyle-Width="100px" SortExpression="Category">
                                    <FilterTemplate>
                                        <telerik:RadComboBox ID="rcbCategory" runat="server" DataTextField="Category" DataSourceID="SQLDSCategory"
                                        DataValueField="Category" AppendDataBoundItems="true" SelectedValue='<%# ((GridItem)Container).OwnerTableView.GetColumn("Category").CurrentFilterValue %>'
                                        OnClientSelectedIndexChanged="CategoryIndexChanged" Width="85px">
                                            <Items>
                                                <telerik:RadComboBoxItem Text="-Select-" Value="" />
                                            </Items>
                                        </telerik:RadComboBox>
                                        <telerik:RadScriptBlock ID="RadScriptBlock2" runat="server">
                                            <script type="text/javascript">
                                                function CategoryIndexChanged(sender, args) {
                                                    var tableView = $find("<%# ((GridItem)Container).OwnerTableView.ClientID %>");
                                                    tableView.filter("Category", args.get_item().get_value(), "EqualTo");
                                                }
                                            </script>
                                        </telerik:RadScriptBlock>
                                    </FilterTemplate>
                                </telerik:GridBoundColumn>
                                <telerik:GridDateTimeColumn DataField="PublishedDate" UniqueName="PublishedDate" HeaderText="Published" SortExpression="PublishedDate"
                                AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="135px" FilterControlWidth="100px" DataFormatString="{0:MM/dd/yyyy}" />
                                <telerik:GridDateTimeColumn DataField="CreatedDate" UniqueName="CreatedDate" HeaderText="Created" SortExpression="CreatedDate"
                                AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="135px" FilterControlWidth="100px" DataFormatString="{0:MM/dd/yyyy}" />
                                <telerik:GridDateTimeColumn DataField="ExpirationDate" UniqueName="ExpirationDate" HeaderText="Expires" SortExpression="ExpirationDate"
                                AllowFiltering="false" AllowSorting="true" HeaderStyle-Width="75px" ItemStyle-Width="75px" DataFormatString="{0:MM/dd/yyyy}" />
                                <telerik:GridTemplateColumn DataField="InternalNotes" UniqueName="InternalNotes" HeaderText="Notes" ItemStyle-Wrap="true"
                                HeaderStyle-Width="125px" ItemStyle-Width="125px" FilterControlWidth="85px" SortExpression="InternalNotes" AutoPostBackOnFilter="true" 
                                CurrentFilterFunction="Contains">
                                    <ItemTemplate>
                                        <asp:Image ID="imgNotes" runat="server" CssClass="Icon16" ImageUrl="~/css/dsd-default/images/icons/comment.png" AlternateText="Notes" />
                                        <telerik:RadToolTip ID="rttNotes" runat="server" TargetControlID="imgNotes" RelativeTo="Element" Position="BottomCenter"
                                        AutoCloseDelay="0" Width="400" RenderInPageRoot="true" Text='<%# DataBinder.Eval(Container, "DataItem.InternalNotes") %>'>
                                        </telerik:RadToolTip>
                                    </ItemTemplate>
                                    <HeaderStyle HorizontalAlign="Center" CssClass="centeralign" />
                                    <ItemStyle HorizontalAlign="Center" CssClass="centeralign" />
                                </telerik:GridTemplateColumn>
                            </Columns>
                        </MasterTableView>
                    </telerik:RadGrid>
                </asp:Panel>
            <%--</ContentTemplate>
        </asp:UpdatePanel>--%>
    </div>
    <%--END--%>

    <%--APPLICATION NAME RADCOMBOBOX FILTER--%>
    <asp:SqlDataSource ID="SQLDSApplicationName" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Mirion.DSD.Docs.DocumentsConnectionString %>"
    SelectCommand="SELECT ApplicationName FROM Applications ORDER BY ApplicationName ASC;">
    </asp:SqlDataSource>
    <%--END--%>

    <%--CATEGORY RADCOMBOBOX FILTER--%>
    <asp:SqlDataSource ID="SQLDSCategory" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Mirion.DSD.Docs.DocumentsConnectionString %>"
    SelectCommand="SELECT DISTINCT Category FROM Documents ORDER BY Category ASC;">
    </asp:SqlDataSource>
    <%--END--%>
</asp:Content>

