<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Sales_CouponCodes" Codebehind="CouponCodes.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <asp:HiddenField ID="SelectedCouponID" runat="server" Value="" />
    <table style="width: 100%;">
        <tr>
            <td style="text-align: left; white-space: nowrap">
                <div>
                    <table style="width: 850px;">
                        <tr>
                            <td width="320px">
                                <fieldset class="fixedheight">
                                    <legend>Search:</legend>
                                    <div>
                                        <asp:TextBox ID="txtSearchString" runat="server" Width="200px" AutoPostBack="True"></asp:TextBox>
                                        &nbsp;&nbsp;
                                        <asp:Button ID="btnSearch" runat="server" Text="Search" />
                                    </div>
                                </fieldset>
                            </td>
                            <td width="140px">
                                <fieldset class="fixedheight">
                                    <legend>Show:</legend>
                                    <div>
                                        <asp:RadioButtonList ID="radShow" runat="server" AutoPostBack="True" RepeatDirection="Horizontal">
                                            <asp:ListItem Selected="true" Text="Active" Value="Active" />
                                            <asp:ListItem Text="Inactive" Value="Inactive" />
                                        </asp:RadioButtonList>
                                    </div>
                                </fieldset>
                            </td>
                            <td width="380px">
                                <fieldset class="fixedheight">
                                    <legend>Sort:</legend>
                                    <div>
                                        <asp:Label ID="lblSortBy" runat="server" Text="By:"></asp:Label>&nbsp;
                                        <asp:DropDownList ID="ddlSortBy" runat="server" Width="150px" AutoPostBack="True">
                                            <asp:ListItem Text="Coupon Code" Value="CouponCode" />
                                            <asp:ListItem Text="Brand Name" Value="BrandName" />
                                        </asp:DropDownList>
                                        &nbsp;&nbsp;
                                        <asp:Label ID="lblOrderBy" runat="server" Text="Order:"></asp:Label>&nbsp;
                                        <asp:DropDownList ID="ddlSortOrder" runat="server" Width="100px" AutoPostBack="True">
                                            <asp:ListItem Text="Ascending" Value="Asc" />
                                            <asp:ListItem Text="Descending" Value="Desc" />
                                        </asp:DropDownList>
                                    </div>
                                </fieldset>
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div class="ui-widget-header-brand ui-corner-all ui-toolbar" style="width: 837px;">
                    <asp:LinkButton ID="btnAddRecord" runat="server" ToolTip="Add Coupon Code" OnClick="btnAddRecord_Click">
                        <img src="../css/dsd-default/images/icons/note_add.png" border="0" />Add Coupon Code
                    </asp:LinkButton>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div>
                    <asp:GridView ID="grdView" CssClass="OTable" AlternatingRowStyle-CssClass="Alt" runat="server"
                        AllowPaging="true" PageSize="20" AutoGenerateColumns="False" DataKeyNames="CouponID"
                        DataSourceID="sqlGetCouponCodes">
                        <Columns>
                            <asp:TemplateField ShowHeader="False" ItemStyle-Width="20px" ItemStyle-HorizontalAlign="left">
                                <ItemTemplate>
                                    <asp:ImageButton ID="btnGridEdit" runat="server" OnClick="btnGridEdit_Click" ToolTip='<%# GetEditToolTip() %>'
                                        CommandName="EditX" CommandArgument='<%# Eval("CouponID") %>' ImageUrl='<%# GetEditImageUrl() %>'>
                                    </asp:ImageButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="CouponID" HeaderText="CouponID" ReadOnly="True" Visible="false" />
                            <asp:BoundField DataField="CouponCode" HeaderText="Coupon Code" ReadOnly="True" HeaderStyle-Width="100px"
                                HeaderStyle-HorizontalAlign="Left" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="RateCode" HeaderText="Rate Code" ReadOnly="True" HeaderStyle-Width="70px"
                                HeaderStyle-HorizontalAlign="Left" ItemStyle-Width="70px" ItemStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="CouponDesc" HeaderText="Description" ReadOnly="True"
                                HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                            <asp:TemplateField HeaderText="Effective Period">
                                <ItemTemplate>
                                    <asp:Label Text='<%# String.Format("{0:d} - {1:d}", Eval("EffectiveDate"), Eval("ExpirationDate")) %>' runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:CheckBoxField DataField="FreeShipping" HeaderText="Free Ship" ReadOnly="True"
                                HeaderStyle-Width="60px" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="60px"
                                ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="BrandName" HeaderText="Brand" ReadOnly="True" HeaderStyle-Width="50px"
                                HeaderStyle-HorizontalAlign="Left" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Left" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div>
                                There are no records found!</div>
                        </EmptyDataTemplate>
                        <%--<RowStyle Font-Size="X-Small"  />
                                        <HeaderStyle Font-Bold="true" Font-Size="x-Small" />--%>
                        <PagerSettings PageButtonCount="20" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="sqlGetCouponCodes" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
                        SelectCommand="sp_GetAllCouponCodesBySearchSortOrderActiveStatus" SelectCommandType="StoredProcedure">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="txtSearchString" DefaultValue="" ConvertEmptyStringToNull="false"
                                Name="SearchString" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="ddlSortBy" DefaultValue="" Name="SortBy" PropertyName="SelectedItem.Value"
                                Type="String" />
                            <asp:ControlParameter ControlID="ddlSortOrder" DefaultValue="" Name="OrderBy" PropertyName="SelectedItem.Value"
                                Type="String" />
                            <asp:ControlParameter ControlID="radShow" DefaultValue="" Name="ActiveStatus" PropertyName="SelectedItem.Value"
                                Type="String" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </td>
        </tr>
    </table>

    <div>
        <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server">
        </act:ToolkitScriptManager>
        <act:ConfirmButtonExtender runat="server" ID="DeleteButtonConfirmExtender" TargetControlID="btnDelete"
            ConfirmText="Are you sure you want to deactivate it?" />
        <asp:Button ID="btnVirtual" Style="display: none" runat="server" Text="virtualButton" />
        <act:ModalPopupExtender ID="ModalPopup" runat="server" BehaviorID="MyModalPopup"
            TargetControlID="btnVirtual" PopupControlID="popUpPanel" CancelControlID="btnClose"
            BackgroundCssClass="modalBackground" PopupDragHandleControlID="PopupRateDetailsHeader">
        </act:ModalPopupExtender>
        <asp:Panel ID="popUpPanel" runat="server" CssClass="modalPopupAdd" Height="410px">

            <div class="PopupBody">
                        <div class="popupHeaderAdd" id="PopupAddHeader" runat="server">
                Adding Coupon Code</div>
                <table style="background-color:White; width:100%" >
                    <tr>
                        <td colspan="2">
                            &nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                        </td>
                        <td style="text-align: left; white-space: nowrap; width: 80%">
                            <asp:Label ID="lblValidationNote" runat="server" ForeColor="red" Width="320px"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                            <asp:Label ID="lblCouponCode" runat="server" Text="Coupon Code:"></asp:Label>&nbsp;
                        </td>
                        <td style="text-align: left; white-space: nowrap; width: 80%">
                            <asp:TextBox ID="txtCouponCode" runat="server" Width="200px"></asp:TextBox>&nbsp;<asp:Label
                                ID="lblCouponCodeValidate" runat="server" ForeColor="red" Text="*"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                            <asp:Label ID="lblCouponDesc" runat="server" Text="Coupon Description:"></asp:Label>&nbsp;
                        </td>
                        <td style="text-align: left; white-space: nowrap; width: 80%">
                            <asp:TextBox ID="txtCouponDesc" runat="server" Width="320px"></asp:TextBox>&nbsp;<asp:Label
                                ID="lblCouponDescValidate" runat="server" ForeColor="red" Text="*"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                            <asp:Label ID="lblEffectiveDate" runat="server" Text="Effective Date:"></asp:Label>&nbsp;
                        </td>
                        <td style="text-align: left; white-space: nowrap; width: 80%">
                            <asp:TextBox ID="txtEffectiveDate" runat="server" Width="100px" />&nbsp;<asp:Label
                                ID="lblEffectiveDateValidate" runat="server" ForeColor="red" Text="*"></asp:Label>
                        </td>
                        <act:CalendarExtender runat="server" ID="CalendarExtender1" Format="MM/dd/yyyy" TargetControlID="txtEffectiveDate"
                            PopupPosition="BottomLeft">
                        </act:CalendarExtender>
                    </tr>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                            <asp:Label ID="lblExpirationDate" runat="server" Text="Expiration Date:"></asp:Label>&nbsp;
                        </td>
                        <td style="text-align: left; white-space: nowrap; width: 80%">
                            <asp:TextBox ID="txtExpirationDate" runat="server" Width="100px" />&nbsp;<asp:Label
                                ID="lblExpirationDateValidate" runat="server" ForeColor="red" Text="*"></asp:Label>
                        </td>
                        <act:CalendarExtender runat="server" ID="CalendarExtender2" Format="MM/dd/yyyy" TargetControlID="txtExpirationDate"
                            PopupPosition="BottomLeft">
                        </act:CalendarExtender>
                    </tr>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                        </td>
                        <td style="text-align: left; white-space: nowrap; width: 80%">
                            <asp:CheckBox ID="chkFreeShipping" runat="server" Text="Free Shipping ?" TextAlign="Right" />&nbsp;<asp:Label
                                ID="lblFreeShippingValidate" runat="server" ForeColor="red" Text="*"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                            <asp:Label ID="lblProductVersion" runat="server" Text="Product Version:"></asp:Label>&nbsp;
                        </td>
                        <td style="text-align: left; white-space: nowrap; width: 80%">
                            <asp:TextBox ID="txtProductVersion" runat="server" Width="40px" />&nbsp;<asp:Label
                                ID="lblProductVersionValidate" runat="server" ForeColor="red" Text="*"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                            <asp:Label ID="lblRate" runat="server" Text="Rate Code:"></asp:Label>&nbsp;
                        </td>
                        <td style="text-align: left; white-space: nowrap; width: 80%">
                            <asp:DropDownList ID="ddlRate" runat="server" Width="320px" AutoPostBack="false" />
                            &nbsp;<asp:Label ID="lblRateValidate" runat="server" ForeColor="red" Text="*"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                            <asp:Label ID="lblBrandSource" runat="server" Text="Brand Source:"></asp:Label>&nbsp;
                        </td>
                        <td style="text-align: left; white-space: nowrap; width: 80%">
                            <asp:DropDownList ID="ddlBrandSource" runat="server" Width="120px" AutoPostBack="false" />
                            &nbsp;<asp:Label ID="lblBrandSourceValidate" runat="server" ForeColor="red" Text="*"></asp:Label>
                        </td>
                    </tr>
                    <%--//////////////////////////////////// Commint Message Row ////////////////////////////////////////--%>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                        </td>
                        <td style="text-align: left; white-space: normal;">
                            <asp:Label ID="lblCommitConfirm" runat="server" ForeColor="Blue" Width="100%"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            &nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; white-space: nowrap; width: 20%">
                        </td>
                        <td style="text-align: left; white-space: nowrap; width: 80%">
                            <asp:Button ID="btnClose" runat="server" Text="Close" />
                            &nbsp;&nbsp;<asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
                            &nbsp;&nbsp;<asp:Button ID="btnDelete" runat="server" Text="Deactivate" OnClick="btnDelete_Click" />
                        </td>
                    </tr>
                </table>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
