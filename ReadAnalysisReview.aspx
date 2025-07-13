<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_ReadAnalysisReview" EnableEventValidation="false" Codebehind="ReadAnalysisReview.aspx.cs" %>
<%@ Register Assembly="ExtendedControls" Namespace="ExtendedControls" TagPrefix="ec" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <%--Additional "Toolbar" area for Search TextBox--%>
    <style type="text/css">
        .SearchBar
        {
            background-image: url('../css/dsd-default/images/o-toolbar.png');
	        background-repeat: repeat-x;
            margin-bottom: -10px;
            border-top: 0px none;
            border-bottom: 0px none;
            color: #FFFFFF;
            font-weight: bold;
        }

        #hyprlnkReview, #btnMarkAsReviewed, #hyprlnkRecall {
            padding: 5px 3px;
            font-size: 10px;
        }
    </style>
    <%--End--%>
    <script src="../scripts/jquery.quicksearch.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $('<%= this.lnkbtnClearFilters.ClientID %>').click(function () {
                $(this).closest('form').find("input#<%=inptSearch.ClientID %>, textarea").val("");
                $(this).closest('form').find("select#<%=ddlFailedTestFilter.ClientID %>").val("");
            });
        });

        // JAVASCRIPT QUICKSEARCH FOR INPUT CONTROL.
        $(function () {
            $('input#<%=inptSearch.ClientID %>').quicksearch('table#gvReadAnalysisReview tbody tr');
        });
        // END

        // JAVASCRIPT THAT TOGGLES GRIDVIEW CHECKBOXES BASED ON HEADER CHECKBOX STATE.
        var TotalChkBx;
        var Counter;

        window.onload = function () {
            //Get Total No. of CheckBoxes inside the GridView (gvReadAnalysisReview).
            TotalChkBx = parseInt('<%= this.gvReadAnalysisReview.Rows.Count %>');

            //Get Total No. of checked CheckBoxes inside the GridView  (gvReadAnalysisReview).
            Counter = 0;
        }

        function HeaderClick(CheckBox) {
            // Get HeaderTemplate and ItemTemplate controls.
            // "headerChkBx" and "chkbxCompleteReview" respectively.
            var TargetBaseControl = document.getElementById('<%= this.gvReadAnalysisReview.ClientID %>');
            var TargetChildControl = "chkbxCompleteReview";

            //Get all the controls of the type INPUT (which is rendered at runtime) in the HeaderTemplate control.
            var Inputs = TargetBaseControl.getElementsByTagName("input");

            //Check/Uncheck ALL the CheckBoxes inside the GridView (gvReadAnalysisReview).
            for (var n = 0; n < Inputs.length; ++n)
                if (Inputs[n].type == 'checkbox' &&
                    Inputs[n].id.indexOf(TargetChildControl, 0) >= 0)
                    Inputs[n].checked = CheckBox.checked;

            //Reset Counter.
            Counter = CheckBox.checked ? TotalChkBx : 0;
        }

        function ChildClick(CheckBox, HCheckBox) {
            // Get HeaderTemplate CheckBox control.
            var HeaderCheckBox = document.getElementById(HCheckBox);

            //Modifiy Counter; 
            if (CheckBox.checked && Counter < TotalChkBx)
                Counter++;
            else if (Counter > 0)
                Counter--;

            //Change state of the HeaderTemplate CheckBox.
            if (Counter < TotalChkBx)
                HeaderCheckBox.checked = false;
            else if (Counter == TotalChkBx)
                HeaderCheckBox.checked = true;
        }
        // END

        // Open Link into New Window.
        function openNewWindow(pageUrl) {
            window.open(pageUrl, '_blank');
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout="1800"></asp:ScriptManager>
    <asp:HiddenField ID="hdnfldAccountIDs" runat="server" Value="" />
    <asp:HiddenField ID="hdnfldCompanyName" runat="server" Value="" />
    <asp:HiddenField ID="hdnFailedTest" runat="server" Value="" />
    <%--TOOLBAR AREA--%>
    <div id="divToolbar" class="OToolbar JoinTable">
        <ul>
            <li>
                <asp:LinkButton ID="lnkbtnAll" runat="server"
                    CssClass="Icon Lightning"
                    Text="All"
                    OnClick="lnkbtnAll_Click"
                    Tooltip="All"
                    TabIndex="1">
                </asp:LinkButton>
            </li>
            <li>
                <asp:LinkButton ID="lnkBtnSpecialAccounts" runat="server"
                    CssClass="Icon Account"
                    Text='Special Accounts'
                    OnClick="lnkBtnSpecialAccounts_Click"
                    Tooltip='Special Accounts'
                    TabIndex="2">
                </asp:LinkButton>
            </li>
            <li>
                <asp:LinkButton ID="lnkbtnRefreshRadGrid" runat="server" 
                    CssClass="Icon Refresh"
                    Text="Refresh Data" 
                    OnClick="lnkbtnRefreshGridView_Click" 
                    ToolTip="Refresh Data"
                    TabIndex="3">
                </asp:LinkButton>
            </li>
            <li>
                <asp:LinkButton ID="lnkbtnClearFilters" runat="server" 
                    CssClass="Icon Remove"
                    Text="Clear Filters"  
                    OnClick="lnkbtnClearFilters_Click" 
                    ToolTip="Clear Filters"
                    TabIndex="4">
                </asp:LinkButton>
            </li>
            <li>
                <asp:LinkButton ID="lnkbtnReturnToReadAnalysisPage" runat="server" 
                    CssClass="Icon PageGo"
                    Text="Return to Read Analysis"  
                    ToolTip="Return to Read Analysis"
                    OnClick="lnkbtnReturnToReadAnalysisPage_Click" 
                    TabIndex="5">
                </asp:LinkButton>
            </li>
        </ul>
    </div>
    <%--END--%>
    <div id="divMainContent" class="JoinTable">
        <table class="OTable SearchBar">
            <tr>
                <td style="width: 150px;">
                    <strong>Search:&nbsp;</strong><input id="inptSearch" type="text" runat="server" placeholder="Search" />
                </td>
                <td>
                    <asp:Label AssociatedControlID="ddlFailedTestFilter" Text="Failed Tests:" runat="server"></asp:Label>
                    <asp:DropDownList ID="ddlFailedTestFilter" runat="server" OnSelectedIndexChanged="ddlFailedTestFilter_SelectedIndexChanged" AppendDataBoundItems="true" AutoPostBack="true" EnableViewState="true">
                        <asp:ListItem Value="">All</asp:ListItem>
                    </asp:DropdownList>
                </td>
            </tr>
        </table>
        <asp:UpdatePanel ID="updtpnlReadAnalysisReview" runat="server">
            <ContentTemplate>
                <ec:GridViewEx ID="gvReadAnalysisReview" runat="server"
                CssClass="OTable"
                AllowViewState="false"
                AutoGenerateColumns="false" 
                AllowPaging="true" PageSize="1000"
                AllowSorting="true"
                DataKeyNames="RID" 
                OnPageIndexChanging="gvReadAnalysisReview_PageIndexChanging"
                OnRowDataBound="gvReadAnalysisReview_RowDataBound" 
                OnRowCreated="gvReadAnalysisReview_RowCreated"
                OnPreRender="gvReadAnalysisReview_PreRender" 
                OnSorting="gvReadAnalysisReview_OnSorting"
                AlternatingRowStyle-CssClass="Alt" Width="100%"  
                ClientIDMode="Static">
                <AlternatingRowStyle CssClass="Alt" />
                    <Columns>
                        <asp:TemplateField HeaderText="" HeaderStyle-Width="25px">
                            <HeaderTemplate>
                                <asp:CheckBox ID="headerChkBx" runat="server" onclick="javascript:HeaderClick(this);"></asp:CheckBox>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="chkbxCompleteReview" runat="server"></asp:CheckBox>
                            </ItemTemplate>        
                        </asp:TemplateField>            
                        <asp:BoundField DataField="DeviceID" Visible="false" ItemStyle-Wrap="false" />
                        <asp:TemplateField HeaderText="Serial #" HeaderStyle-Wrap="false" ItemStyle-Wrap="false" SortExpression="SerialNumber" >
                            <ItemTemplate>
                                <asp:HyperLink ID="hyprlnkSerialNumber" runat="server" 
                                NavigateUrl='<%# string.Format("~/InformationFinder/Details/Device.aspx?ID={0}&AccountID={1}", Eval("SerialNumber"), Eval("AccountID")) %>' 
                                Target="_blank" Text='<%# Eval("SerialNumber")%>' ToolTip='<%# string.Format("Serial #{0}", Eval("SerialNumber")) %>'>
                                </asp:HyperLink>  
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Account #" HeaderStyle-Wrap="false" ItemStyle-Wrap="false" SortExpression="AccountID" >
                            <ItemTemplate>
                                <asp:HyperLink ID="hyprlnkGotoFactorsPage" runat="server" CssClass="Icon Factors" 
                                NavigateUrl='<%# string.Format("Factors.aspx?AccountID={0}", Eval("AccountID")) %>' 
                                Target="_blank" Text="" ToolTip='<%# string.Format("Factors for Account #{0}", Eval("AccountID")) %>'>
                                </asp:HyperLink>
                                <asp:HyperLink ID="hyprlnkAccountID" runat="server" 
                                NavigateUrl='<%# string.Format("../InformationFinder/Details/Account.aspx?ID={0}", Eval("AccountID")) %>' 
                                Target="_blank" Text='<%# (Eval("GDSAccount") == null ? Eval("AccountID") : Eval("GDSAccount")) %>' ToolTip='<%# string.Format("{0}", Eval("CompanyName")) %>'>
                                </asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="UserID" Visible="false" />
                        <asp:TemplateField HeaderText="Assigned To" HeaderStyle-Wrap="true" ItemStyle-Wrap="true" SortExpression="AssignedTo">
                            <ItemTemplate>
                                <asp:HyperLink ID="hyprlnkUserName" runat="server" 
                                NavigateUrl='<%# string.Format("../InformationFinder/Details/UserMaintenance.aspx?AccountID={0}&UserID={1}", Eval("AccountID"), Eval("UserID")) %>' 
                                Target="_blank" Text='<%# Eval("AssignedTo")%>' ToolTip='<%# string.Format("{0}", Eval("AssignedTo")) %>'>
                                </asp:HyperLink>  
                            </ItemTemplate>
                            <HeaderStyle Wrap="true" />
                            <ItemStyle Wrap="true" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="CreatedDate" HeaderText="Created Date" HeaderStyle-Wrap="false" ItemStyle-Wrap="true" DataFormatString="{0:MM/dd/yyyy HH:mm}" SortExpression="CreatedDate">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>

                        <asp:BoundField DataField="ModifiedDate" HeaderText="Modified Date" HeaderStyle-Wrap="false" ItemStyle-Wrap="true" DataFormatString="{0:MM/dd/yyyy HH:mm}" SortExpression="ModifiedDate">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Badge Type" HeaderStyle-Wrap="true" ItemStyle-Wrap="true" SortExpression="ProductGroupID">
                            <ItemTemplate>                                
                                 <%# GetProductTypeCodeByID(Convert.ToInt32(Eval("ProductGroupID"))) %>
                            </ItemTemplate>
                            <HeaderStyle Wrap="true" />
                            <ItemStyle Wrap="true" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="ReadTypeName" HeaderText="Read Type" HeaderStyle-Wrap="false" ItemStyle-Wrap="true" SortExpression="ReadTypeName">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Init / Anom" HeaderStyle-Wrap="false" ItemStyle-Wrap="true" SortExpression="InitialRead">
                            <ItemTemplate>
                                <asp:Label ID="lblInitialReadHasAnomaly" runat="server" Text='<%# string.Format("{0}/{1}", Eval("InitialRead"), Eval("HasAnomaly")) %>' />
                            </ItemTemplate>
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="DLDCalc" HeaderText="DLD Calc" SortExpression="DLDCalc" DataFormatString="{0:0.0}">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>
                        <asp:BoundField DataField="ActualDeepLowDose" HeaderText="Small Incr" SortExpression="ActualDeepLowDose" DataFormatString="{0:0}">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>

                         <asp:TemplateField HeaderText="BKGD" HeaderStyle-Wrap="false" ItemStyle-Wrap="false" SortExpression="BackgroundExposure" >
                            <ItemTemplate>
                                <asp:Label ID="lblBackground" runat="server" Text='<%# String.Format("{0:0.0}", (Convert.ToInt32(Eval("ProductGroupID")) == 2 || Convert.ToInt32(Eval("ProductGroupID")) == 11 ? Eval("BackgroundExposureDL") : Eval("BackgroundExposure"))) %>' ToolTip='<%# string.Format("{0}", Eval("CompanyName")) %>'>
                                </asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="CumulDose" HeaderText="Cumul Dose" SortExpression="CumulDose" DataFormatString="{0:0}">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>                        
                        <asp:BoundField DataField="DeepLowCumulativeDose" HeaderText="Cumul Dose" SortExpression="DeepLowCumulativeDose" DataFormatString="{0:0}">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>

                        <asp:BoundField DataField="Deep" HeaderText="Deep" NullDisplayText="*" SortExpression="Deep">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>
                        <asp:BoundField DataField="Shallow" HeaderText="Shallow" NullDisplayText="*" SortExpression="Shallow">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>
                        <asp:BoundField DataField="Eye" HeaderText="Eye" NullDisplayText="*" SortExpression="Eye">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>
                        <asp:BoundField DataField="BatteryPercent" HeaderText="Battery Percent" NullDisplayText="*" SortExpression="BatteryPercent">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>
                         <asp:BoundField DataField="FailedTests" HeaderText="Failed Tests" NullDisplayText="*" SortExpression="FailedTests">
                            <HeaderStyle CssClass="CenterAlign" Wrap="true" />
                            <ItemStyle CssClass="CenterAlign" Wrap="true" />
                        </asp:BoundField>

                        <asp:TemplateField HeaderText="" HeaderStyle-Wrap="false" ItemStyle-CssClass="RightAlign" ItemStyle-Wrap="false">
                            <ItemTemplate>
                                <asp:HyperLink ID="hyprlnkRecall" CssClass="btn" runat="server" 
                                    NavigateUrl='<%# string.Format("ReturnAddNewDeviceRMA.aspx?SerialNo={0}&Reason=Fade", Eval("SerialNumber")) %>' 
                                    Target="_blank" Text="Recall" ToolTip='<%# string.Format("Recall #{0}", Eval("SerialNumber")) %>'
                                  >
                                    
                                </asp:HyperLink>  

                                <asp:HyperLink ID="hyprlnkReview" CssClass="btn" runat="server" 
                                NavigateUrl='<%# string.Format("BadgeReview.aspx?SerialNo={0}&RID={1}", Eval("SerialNumber"), Eval("RID")) %>' 
                                Target="_blank" Text="Review" ToolTip='<%# string.Format("Review Badge #{0}", Eval("SerialNumber")) %>'>
                                </asp:HyperLink>  
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div>There are no records available.</div>
                    </EmptyDataTemplate>	
                    <RowStyle CssClass="Row" />
                    <SelectedRowStyle CssClass="Row Selected" />
                </ec:GridViewEx>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div class="Buttons">
            <div class="ButtonHolder">
                <asp:Button ID="btnMarkAsReviewed" runat="server" CssClass="OButton" Text="Mark As Reviewed" ToolTip="Mark As Reviewed" OnClick="btnMarkAsReviewed_Click" />
            </div>
        </div>
    </div>
</asp:Content>

