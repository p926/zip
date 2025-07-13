<%@ Page Title="Manage Hardware Updates" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_HardwareUpdates" Codebehind="HardwareUpdates.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript">

        $(function () {
            $('#uploadUpdateModal').dialog({
                autoOpen: false,
                resizable: false,
                width: 670,
                title: 'Upload a Hardware Update',
                buttons: {
                    "Save": function () {
                        if (isNaN($("#<%= txtUploadSequence.ClientID %>").val()))
                            alert("Sequence must be a number.");
                        else
                            $('#<%= btnUploadVersion.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            }).parent().appendTo($("form"));

            $('#editUpdateModal').dialog({
                autoOpen: false,
                resizable: false,
                width: 670,
                title: 'Edit a Hardware Update',
                buttons: {
                    "Update": function () {
                        if (isNaN($("#<%= txtSequence.ClientID %>").val()))
                            alert("Sequence must be a number.");
                        else
                            $('#<%= btnEditVersion.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            }).parent().appendTo($("form"));

            $('#openUploadDialog').click(function () {
                $('#uploadUpdateModal').dialog("open");
                $('.ui-overlay').fadeIn();
            });
            $('.datePicker').datepicker();

        });
        function openEditDialog() {
            $('#editUpdateModal').dialog("open");
            $('.ui-overlay').fadeIn();
        }

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:ScriptManager runat="server" />
    
    <asp:Button Text="Upload" ID="btnUploadVersion" OnClick="btnUploadVersion_Click" style="display:none;" runat="server"  />
    <asp:Button Text="Edit" ID="btnEditVersion" OnClick="btnEditVersion_Click" style="display:none;" runat="server"  />

    <div id="uploadUpdateModal" style="display:none;">
        <div class="FormError" id="uploadError" runat="server" visible="false">
            <p>
                <span class="MessageIcon"></span>
                <strong>Messages:</strong>
                <asp:Label Text="" ID="lblUploadError" runat="server" />
            </p>
        </div>
        <div class="OForm">
            
            <div class="Row">
                <div class="Label">Hardware<span class="Required">*</span>:</div>
                <div class="Control">
                    <asp:DropDownList runat="server" ID="ddlHardwareList">
                    </asp:DropDownList>
                </div>
                <div class="Clear"></div>
            </div>

            <div class="Row">
                <div class="Label">Version<span class="Required">*</span>:</div>
                <div class="Control">
                    <asp:TextBox runat="server" ID="txtVersionMajor" MaxLength="2" CssClass="Size XXSmall CenterAlign" placeholder="1" /> .
                    <asp:TextBox runat="server" ID="txtVersionMinor" MaxLength="2" CssClass="Size XXSmall CenterAlign" placeholder="0" /> .
                    <asp:TextBox runat="server" ID="txtVersionRevision" MaxLength="2" CssClass="Size XXSmall CenterAlign" placeholder="0" />
                </div>
                <div class="Clear"></div>
            </div>

            <div class="Row">
                <div class="Label">Publish Date:</div>
                <div class="Control">
                    <asp:TextBox runat="server" ID="txtPublishDate" CssClass="Size Small datePicker" placeholder="mm/dd/yyyy" />
                </div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Update Type:</div>
                <div class="Control">
                    <asp:CheckBoxList runat="server" ID="chkUploadUpdateType" RepeatDirection="Horizontal">
                        <asp:ListItem Text="A" Value="A"></asp:ListItem>
                        <asp:ListItem Text="B" Value="B"></asp:ListItem>
                        <asp:ListItem Text="E" Value="E"></asp:ListItem>
                        <asp:ListItem Text="S" Value="S"></asp:ListItem>
                    </asp:CheckBoxList>
                </div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Sequence:</div>
                <div class="Control">
                    <asp:TextBox runat="server" ID="txtUploadSequence" Text="1" CssClass="Size XXSmall CenterAlign" ></asp:TextBox>
                </div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Upload Build<span class="Required">*</span>:</div>
                <div class="Control">
                    <asp:FileUpload runat="server" ID="fuBuildFile" />
                </div>
                <div class="Clear"></div>
            </div>
            
            <div class="Row">
                <div class="Label">Release Notes<span class="Required">*</span>:</div>
                <div class="Control">
                    <telerik:RadEditor Width="460" runat="server" ID="reReleaseNotes" Height="300"
                        NewLineMode="P" ContentAreaMode="Div" EditModes="All" ToolsFile="~/App_Data/RadEditorTools.xml">
                    </telerik:RadEditor>
                </div>
                <div class="Clear"></div>
            </div>

            <div class="Footer">
                <span class="Required">*</span> indicates a required field.
            </div>
        </div>
    </div>

    <div id="editUpdateModal" style="display:none;">
        <asp:HiddenField runat="server" ID="hfUpdateID" Value="0" />
        <div class="FormError" id="editError" runat="server" visible="false">
            <p>
                <span class="MessageIcon"></span>
                <strong>Messages:</strong>
                <asp:Label Text="" ID="lblEditError" runat="server" />
            </p>
        </div>
        <div class="OForm">
            
            <div class="Row">
                <div class="Label">Delete:</div>
                <div class="Control">
                    <asp:CheckBox Text="Delete this update." ID="cbEditDelete" runat="server" />
                </div>
                <div class="Clear"></div>
            </div>

            <div class="Row">
                <div class="Label">Hardware<span class="Required">*</span>:</div>
                <div class="Control">
                    <asp:DropDownList runat="server" ID="ddlEditHardwareList">
                    </asp:DropDownList>
                </div>
                <div class="Clear"></div>
            </div>

            <div class="Row">
                <div class="Label">Version:</div>
                <div class="Control">
                    <asp:TextBox runat="server" ID="txtEditVersionMajor" MaxLength="2" CssClass="Size XXSmall CenterAlign" placeholder="1" /> .
                    <asp:TextBox runat="server" ID="txtEditVersionMinor" MaxLength="2" CssClass="Size XXSmall CenterAlign" placeholder="0" /> .
                    <asp:TextBox runat="server" ID="txtEditVersionRevision" MaxLength="2" CssClass="Size XXSmall CenterAlign" placeholder="0" />
                </div>
                <div class="Clear"></div>
            </div>

            <div class="Row">
                <div class="Label">Publish Date:</div>
                <div class="Control">
                    <asp:TextBox runat="server" ID="txtEditPublishDate" CssClass="Size Small datePicker" placeholder="mm/dd/yyyy" />
                </div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Update Type:</div>
                <div class="Control">
                    <asp:CheckBoxList runat="server" ID="chkUpdateType" RepeatDirection="Horizontal">
                        <asp:ListItem Text="A" Value="A"></asp:ListItem>
                        <asp:ListItem Text="B" Value="B"></asp:ListItem>
                        <asp:ListItem Text="E" Value="E"></asp:ListItem>
                        <asp:ListItem Text="S" Value="S"></asp:ListItem>
                    </asp:CheckBoxList>
                </div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Sequence:</div>
                <div class="Control">
                    <asp:TextBox runat="server" ID="txtSequence" Text="" MaxLength="2" CssClass="Size XXSmall CenterAlign"></asp:TextBox>
                </div>
                <div class="Clear"></div>
            </div>
            <div class="Row">
                <div class="Label">Uploaded Build:</div>
                <div class="LabelValue">
                    <asp:Label ID="lblEditUploadedBuild" Text="My File Name" runat="server" />
                </div>
                <div class="Clear"></div>
            </div>
            
            <div class="Row">
                <div class="Label">Release Notes<span class="Required">*</span>:</div>
                <div class="Control">
                    <telerik:RadEditor Width="460" runat="server" ID="reEditReleaseNotes" Height="200"
                        NewLineMode="P" ContentAreaMode="Div" EditModes="All" ToolsFile="~/App_Data/RadEditorTools.xml">
                    </telerik:RadEditor>
                </div>
                <div class="Clear"></div>
            </div>

            <div class="Footer">
                <span class="Required">*</span> indicates a required field.
            </div>
        </div>
    </div>

    <div class="FormMessage" id="uploadSuccess" runat="server" visible="false">
        <p>
            <span class="MessageIcon"></span>
            <strong>Messages:</strong>
            <span>The update has been uploaded successfully.</span>
        </p>
    </div>
    
    <div class="FormMessage" id="editSuccess" runat="server" visible="false">
        <p>
            <span class="MessageIcon"></span>
            <strong>Messages:</strong>
            <span>The update has been modified.</span>
        </p>
    </div>

    <div class="OToolbar JoinTable">
        <ul>
            <li><a href="#" class="Icon Add" id="openUploadDialog">Upload Hardware Update</a></li>
        </ul>
    </div>

    <ec:GridViewEx id="gvHardwareUpdates" runat="server" AutoGenerateColumns="False" CssClass="OTable"
        AlternatingRowStyle-CssClass="Alt" EmptyDataText="There no hardware updates." 
        SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif"
        SortOrderDescendingImagePath="~/images/icon_sort_descending.gif">
            <Columns>
                <asp:TemplateField HeaderText="Edit" HeaderStyle-Width="40">
                    <ItemTemplate>
                        <asp:LinkButton ToolTip="Edit the Update" CssClass="Icon Edit" ID="lbtnEdit" OnClick="lbtnEdit_Click" CommandArgument='<%# Eval("UpdateID") %>' runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="HardwareDescription" HeaderText="Hardware" />
                <asp:TemplateField HeaderText="Version">
                    <ItemTemplate>
                        <asp:Label ID="Label1" Text='<%# string.Format("{0}.{1}.{2}", Eval("UpdateMajor"), Eval("UpdateMinor"), Eval("UpdateRevision")) %>' runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Download Update">
                    <ItemTemplate>
                        <asp:HyperLink ID="HyperLink1" NavigateUrl='<%# string.Format("HardwareUpdates.aspx?Guid={0}", Eval("UpdateGuid")) %>' Text='<%# Eval("FileName") %>' runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="UpdatePaths" HeaderText="Update Type" />
                <asp:BoundField DataField="Sequence" HeaderText="Sequence" />
                <asp:BoundField DataField="PublishedDate" HeaderText="Published" DataFormatString="{0:MM/dd/yyyy}" />
                <asp:BoundField DataField="UploadedDate" HeaderText="Uploaded" DataFormatString="{0:MM/dd/yyyy}" />
                        
                <asp:TemplateField HeaderText="Notes" HeaderStyle-Width="40">
                    <ItemTemplate>
                        <asp:Label ID="lblReleaseNotes" Text="" CssClass="Icon Comment" runat="server" />
                                                            
                        <telerik:RadToolTip ID="RadToolTip1" runat="server" TargetControlID="lblReleaseNotes" 
                            RelativeTo="Element" Position="BottomCenter" AutoCloseDelay="0" Width="400" Height="200"  RenderInPageRoot="true">
                            <div><strong>Release Notes:</strong></div>
                            <%# DataBinder.Eval(Container, "DataItem.ReleaseNotes")%>
                        </telerik:RadToolTip>

                    </ItemTemplate>
                </asp:TemplateField>
                
                <asp:BoundField DataField="MD5Hash" HeaderText="MD5 Checksum" />

            </Columns>
    </ec:GridViewEx>

</asp:Content>

