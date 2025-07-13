<%@ Page Title="Read Analysis" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="ReadAnalysis.aspx.cs" Inherits="TechOps_ReadAnalysis" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <link rel="stylesheet" type="text/css" href="../css/rad-controls/RadGrid.css" />
    <style type="text/css">
        /* CSS for Progress Bar area. */
         .ui-progressbar {
            position: relative;
        }

        .progress-label {
            position: absolute;
            text-align: center;
            left: 50%;
            top: 4px;
            font-weight: bold;
            text-shadow: 1px 1px 0 #fff;
        }

        /*.ui-dialog-titlebar-close {
            visibility: hidden;
        }*/
    </style>
    <%--JQUERY HERE--%>
    <script type="text/javascript">
        // Declare [Global] Variables.
        var logID = 0;
        var workerStatus = '';
        var totalReads = 0;
        var readsCompleted = 0;
        var readErrors = 0;
        var messageDesc = '';

        var selectedTab;

        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
        });

        function JQueryControlsLoad() {

            $(".datePicker").datepicker();

            // Perform Read Analysis Modal/Dialog.
            $('#divReadAnalyisDialog').dialog({
                autoOpen: false,
                width: 475,
                modal: true,
                resizable: false,
                title: "Configure Read Analysis",
                open: function (type, data) {
                    var $this = $(this);
                    $this.parent().appendTo("form");
                    $('.ui-dialog :input').focus();

                    changeProductType();

                    $('#<%= ddlProductType.ClientID %>').change(function () {
                        changeProductType();
                    })
                },
                buttons: {
                    "Run Read Analysis": function () {
                        $('#<%= btnRunReadAnalysis.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $('#<%= btnCancel_RunReadAnalysis.ClientID %>').click();
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancel_RunReadAnalysis.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });
        };

        function changeProductType() {
            var productTypeId = $('#<%= ddlProductType.ClientID %>').val();
            var cummulativeDose = 8000;
            if (productTypeId == "9") {
                $("#divReadAnalyisDialog .NonID1").hide();
            } else if (productTypeId == "18") {
                $("#divReadAnalyisDialog .NonID1").show();
                cummulativeDose = 50000;
            } else {
                $("#divReadAnalyisDialog .NonID1").show();
            }

            $("#<%=txtCumulativeDoseLimit.ClientID%>").val(cummulativeDose);
        }

        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        };

        function closeDialog(id) {
            $('#' + id).dialog("close");
        };

        function pageLoad(sender, args) {
            
        };

        // Simple jQuery that sets Recall Limit (when changed) to Watchlist Low Limit.
        function SetWatchlistLowLimit() {
            $('#<%= txtWatchLowLimit.ClientID %>').val($('#<%= txtRecallLimit.ClientID %>').val());
        }
    </script>
    <%--END--%>
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
                        if (!(items.getItem(i).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'IsNull': '' })) {
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
                if (column.get_dataType() == "System.DateTime") {
                    var h = 0; while (h < items.get_count()) {
                        if (!(items.getItem(h).get_value() in { 'NoFilter': '', 'Contains': '', 'EqualTo': '', 'GreaterThan': '', 'GreaterThanOrEqualTo': '', 'LessThan': '', 'LessThanOrEqualTo': '', 'IsNull': '' })) {
                            var item = items.getItem(h); if (item != null)
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
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <telerik:RadScriptManager ID="RadScriptManager1" runat="server" />
    <telerik:RadStyleSheetManager ID="RadStyleSheetManager1" runat="server"></telerik:RadStyleSheetManager>
    <%--CONFIGURE/RUN/STATUS READ ANALYSIS MODAL/DIALOG--%>
    <div id="divReadAnalyisDialog">
        <asp:UpdatePanel ID="updtpnlModalDialog" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:UpdateProgress id="updtprgssModalDialog" runat="server" DynamicLayout="true" DisplayAfter="0" >
                    <ProgressTemplate>
                        <div style="width: 100%">
                            <center><img src="../images/orangebarloader.gif" width="128" height="15" alt="Processing, please wait..." /></center>
                        </div>
                    </ProgressTemplate>
                </asp:UpdateProgress>
                <asp:Panel runat="server" ID="pnlPerformForm">
                    <p>Provide the parameters to perform a read analysis.</p>
                    <div class="OForm">
                        <div class="Row">
                            <div class="Label Size XLarge2" style="width: 250px;">Badge Type<span class="Required">*</span>:</div>
                            <div class="Control">
                               <asp:DropDownList runat="server" ID="ddlProductType" CssClass="Size Small" TabIndex="1">
                                  <asp:ListItem Selected="True" Value="9"> Instadose 1 </asp:ListItem>
                                  <asp:ListItem Value="11"> Instadose 2 </asp:ListItem>
                                  <asp:ListItem Value="10"> Instadose Plus </asp:ListItem>
                                  <asp:ListItem Value="18"> Instadose VUE </asp:ListItem>
                               </asp:DropDownList>
                               <span class="InlineError">
                                    <asp:RequiredFieldValidator ID="reqDdlProductType" runat="server" Text="Required." 
                                    ValidationGroup="RUNANALYSIS" ControlToValidate="ddlProductType" Display="Dynamic"></asp:RequiredFieldValidator>
                                </span>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row">
                            <div class="Label Size XLarge2" style="width: 250px;">Number to Analyze<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox runat="server" ID="txtNumberToAnalyze" Text="10000" CssClass="Size XXSmall RightAlign" MaxLength="5" TabIndex="2" />
                                <span class="InlineError">
                                    <asp:RequiredFieldValidator ID="reqfldvalNumberToAnalyze" runat="server" Text="Required." 
                                    ValidationGroup="RUNANALYSIS" ControlToValidate="txtNumberToAnalyze" Display="Dynamic"></asp:RequiredFieldValidator>
                                </span>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row">
                            <div class="Label Size XLarge2" style="width: 250px;">DLDoseCalc Recall Limit<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox runat="server" ID="txtRecallLimit" Text="-40" CssClass="Size XXSmall RightAlign" MaxLength="4" onchange="javascript: SetWatchlistLowLimit(); return false;" TabIndex="3" />
                                <span class="InlineError">
                                    <asp:RequiredFieldValidator ID="reqfldvalRecallLimit" runat="server" Text="Required." 
                                    ValidationGroup="RUNANALYSIS" ControlToValidate="txtRecallLimit" Display="Dynamic"></asp:RequiredFieldValidator>
                                </span>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row" style="display: none;">
                            <div class="Label Size XLarge2" style="width: 250px;">Watchlist Low Limit<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox runat="server" ID="txtWatchLowLimit" Text="-40" CssClass="Size XXSmall RightAlign" MaxLength="4" />
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row">
                            <div class="Label Size XLarge2" style="width: 250px;">DLDoseCalc Watchlist High Limit<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox runat="server" ID="txtWatchHighLimit" Text="-15" CssClass="Size XXSmall RightAlign" MaxLength="4" TabIndex="4" />
                                <span class="InlineError">
                                    <asp:RequiredFieldValidator ID="reqfldvalWatchHighLimit" runat="server" Text="Required." 
                                    ValidationGroup="RUNANALYSIS" ControlToValidate="txtWatchHighLimit" Display="Dynamic"></asp:RequiredFieldValidator>
                                    <asp:CompareValidator ID="compvalWatchHighLimit" runat="server" Text="High Limit > Recall Limit" ValidationGroup="RUNANALYSIS"
                                    Operator="GreaterThan" ControlToValidate="txtWatchHighLimit" Display="Dynamic" Type="Integer" ControlToCompare="txtRecallLimit"></asp:CompareValidator>
                                </span>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row">
                            <div class="Label Size XLarge2" style="width: 250px;">Cumulative Dose Limit<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox runat="server" ID="txtCumulativeDoseLimit" Text="8000" CssClass="Size XXSmall RightAlign" MaxLength="5" TabIndex="5" />
                                <span class="InlineError">
                                    <asp:RequiredFieldValidator ID="reqfldvalCumulativeDoseLimit" runat="server" Text="Required." 
                                    ValidationGroup="RUNANALYSIS" ControlToValidate="txtCumulativeDoseLimit" Display="Dynamic"></asp:RequiredFieldValidator>
                                </span>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row">
                            <div class="Label Size XLarge2" style="width: 250px;">Expiration Years Limit<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox runat="server" ID="txtExpirationYearsLimit" Text="9" CssClass="Size XXSmall RightAlign" MaxLength="2" TabIndex="6" />
                                <span class="InlineError">
                                    <asp:RequiredFieldValidator ID="reqfldvalExpirationYearsLimit" runat="server" Text="Required." 
                                    ValidationGroup="RUNANALYSIS" ControlToValidate="txtExpirationYearsLimit" Display="Dynamic"></asp:RequiredFieldValidator>
                                </span>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row NonID1" style="display:none;">
                            <div class="Label Size XLarge2" style="width: 250px;">Battery % Limit<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox runat="server" ID="txtBatteryPercentLimit" Text="25" CssClass="Size XXSmall RightAlign" MaxLength="5" TabIndex="7" />
                                <span class="InlineError">
                                    <asp:RequiredFieldValidator ID="reqBatteryPercentLimit" runat="server" Text="Required." 
                                    ValidationGroup="RUNANALYSIS" ControlToValidate="txtBatteryPercentLimit" Display="Dynamic"></asp:RequiredFieldValidator>
                                </span>
                            </div>
                            <div class="Clear"></div>
                        </div>                       
                    </div>
                </asp:Panel>
                <asp:Button ID="btnRunReadAnalysis" runat="server" Text="Run Read Analysis" CssClass="OButton" style="display: none;" OnClick="btnRunReadAnalysis_RunReadAnalysis_Click" ValidationGroup="RUNANALYSIS" TabIndex="6" />
                <asp:Button ID="btnCancel_RunReadAnalysis" runat="server" Text="Cancel" CssClass="OButton" style="display: none;" OnClick="btnCancel_RunReadAnalysis_Click" TabIndex="7" />
            </ContentTemplate>
        </asp:UpdatePanel>
	</div>
    <%--END--%>
    <%------------------------------------------ BEGIN MAIN PAGE CONTENT ------------------------------------------%>
    <div id="divMainContent">
        <asp:UpdatePanel ID="updtpnlMainContent" runat="server">
            <ContentTemplate>
                <div class="FormError" id="divErrorMessage" runat="server" visible="false" style="text-align: left;">
	                <p><span class="MessageIcon"></span>
	                <strong>Message:</strong>&nbsp;<span id="spnErrorMessage" runat="server">An error as occurred.</span></p>
                </div>
                <%--TAB CONTAINERS SECTION - HEADER--%>
                <div id="divTabsContainerMain">
                    <div id="tabsContainer" class="ui-tabs ui-widget ui-widget-content ui-corner-all">
                        <ul class="ui-tabs-nav ui-helper-reset ui-helper-clearfix ui-widget-header ui-corner-all">
                            <li class="ui-state-default ui-corner-top ui-tabs-selected ui-state-active"><a href="#PerformReadAnalysis_tab" id="Tab1" tabindex="0">Perform Read Analysis</a></li>
                            <li class="ui-state-default ui-corner-top"><a href="ReadAnalysisRecallList.aspx" tabindex="1">Recall List</a></li>
                        </ul>
                        <%--PERFORM READ ANALYSIS TAB--%>
                        <div id="PerformReadAnalysis_tab">
                            <asp:UpdatePanel ID="updtpnlPerformReadAnalysisTab" runat="server">
                                <ContentTemplate>
                                    <div class="OToolbar JoinTable">
                                        <ul>
                                            <li>
                                                <a href="#" class="btn btn-default" id="performAnalysis" onclick="javascript: openDialog('divReadAnalyisDialog'); return false;">
                                                    <i class="Icon Edit"></i>
                                                    Perform Read Analysis
                                                </a>
                                            </li>
                                            <li>
                                                <asp:LinkButton ID="lnkbtnRefreshData_PerformReadAnalysis" runat="server" OnClick="lnkbtnRefreshData_PerformReadAnalysis_Click" 
                                                Text="Refresh Data" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Refresh" ToolTip="Refresh Data" />
                                            </li>
                                            <li>
                                                <asp:LinkButton ID="lnkbtnClearFilters_PerformReadAnalysis" runat="server" OnClick="lnkbtnClearFilters_PerformReadAnalysis_Click" 
                                                Text="Clear Filters" Font-Bold="true" ForeColor="#FFFFFF" CssClass="Icon Remove" ToolTip="Clear Filters" />
                                            </li>
                                        </ul>
                                    </div>
                                    <asp:Panel ID="pnlReadAnalysisLogDetails" runat="server" SkinID="Default">
                                        <telerik:RadGrid ID="rgReadAnalysisLogDetails" runat="server"
                                        CssClass="OTable"  
                                        AllowMultiRowSelection="false" 
                                        AutoGenerateColumns="false"
                                        AllowFilteringByColumn="true"
                                        EnableLinqExpressions="false"
                                        EnableViewState="false"
                                        OnNeedDataSource="rgReadAnalysisLogDetails_NeedDataSource"
                                        OnItemCommand="rgReadAnalysisLogDetails_ItemCommand" 
                                        AllowPaging="true" AllowSorting="true" 
                                        Style="border: 1px solid #D6712D;"
                                        Width="99.8%" PageSize="20"
                                        GridLines="None" SkinID="Default">
                                            <PagerStyle Mode="NumericPages" />
                                            <GroupingSettings CaseSensitive="false" />
                                            <ClientSettings EnableRowHoverStyle="true">
                                                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
                                                <Selecting AllowRowSelect="false" />
                                                <ClientEvents OnFilterMenuShowing="filterMenuShowing" />
                                            </ClientSettings>
                                            <FilterMenu OnClientShown="MenuShowing" EnableImageSprites="False" />
                                            <MasterTableView TableLayout="Fixed" AllowMultiColumnSorting="true">
                                                <NoRecordsTemplate>No records found.</NoRecordsTemplate>
                                                <Columns>
                                                    <telerik:GridDateTimeColumn DataField="RunDate" UniqueName="RunDate" HeaderText="Run Date" DataFormatString="{0:d}" 
                                                    AllowFiltering="true" AllowSorting="true" HeaderStyle-Width="125px" ItemStyle-Width="100px" FilterControlWidth="125px" 
                                                    SortExpression="RunDate" CurrentFilterFunction="EqualTo" AutoPostBackOnFilter="true" />
                                                    <telerik:GridDateTimeColumn DataField="RunTime" UniqueName="RunTime" HeaderText="Time" DataFormatString="{0:hh:mm:ss tt}" 
                                                    AllowFiltering="false" HeaderStyle-Width="75px" ItemStyle-Width="75px" AllowSorting="false" />
                                                    <telerik:GridBoundColumn UniqueName="RunBy" DataField="RunBy" HeaderText="Run By" AutoPostBackOnFilter="true"
                                                    AllowFiltering="true" HeaderStyle-Width="125px" FilterControlWidth="100px" CurrentFilterFunction="Contains" />
                                                    <telerik:GridBoundColumn UniqueName="ProductType" DataField="ProductType" HeaderText="Badge Type" AutoPostBackOnFilter="true"
                                                    AllowFiltering="true" HeaderStyle-Width="85px" ItemStyle-Width="85px" AllowSorting="false" />
                                                    <telerik:GridBoundColumn UniqueName="RunQuantity" DataField="RunQuantity" HeaderText="Run Quantity" AllowFiltering="false" 
                                                    HeaderStyle-CssClass="CenterAlign" ItemStyle-CssClass="CenterAlign" HeaderStyle-Width="100px" ItemStyle-Width="100px" />
                                                    <telerik:GridBoundColumn UniqueName="NumberAnalyzed" DataField="NumberAnalyzed" HeaderText="Analyzed" AllowFiltering="false" 
                                                    HeaderStyle-CssClass="CenterAlign" ItemStyle-CssClass="CenterAlign" HeaderStyle-Width="100px" ItemStyle-Width="100px" />
                                                    <telerik:GridBoundColumn UniqueName="NumberRemaining" DataField="NumberRemaining" HeaderText="Remaining" AllowFiltering="false" 
                                                    HeaderStyle-CssClass="CenterAlign" ItemStyle-CssClass="CenterAlign" HeaderStyle-Width="100px" ItemStyle-Width="100px" />
                                                    <telerik:GridCalculatedColumn HeaderText="% Completed" UniqueName="PercentageCompleted" DataType="System.Int32"
                                                    DataFields="NumberAnalyzed, RunQuantity" Expression="(({0}/{1}) * 100)" AllowFiltering="false" AllowSorting="true" DataFormatString="{0:#0.0}%"
                                                    HeaderStyle-Width="100px" ItemStyle-Width="100px" HeaderStyle-CssClass="CenterAlign" ItemStyle-CssClass="CenterAlign" />
                                                    <telerik:GridTemplateColumn DataField="ReadAnalysisLogDetailID" UniqueName="ReadAnalysisLogDetailID" AllowFiltering="false"
                                                    HeaderStyle-Width="100px" ItemStyle-Width="100px" HeaderStyle-CssClass="RightAlign" ItemStyle-CssClass="RightAlign" HeaderText="Status">
                                                        <ItemTemplate>
                                                            <asp:HyperLink ID="hyprlnkReviewStatus" runat="server" NavigateUrl='<%# string.Format("ReadAnalysisReview.aspx?LogID={0}", Eval("ReadAnalysisLogDetailID")) %>' Text="Continue" Target="_top"></asp:HyperLink>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                </Columns>
                                            </MasterTableView>
                                        </telerik:RadGrid>
                                    </asp:Panel>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <%--END--%>
                    </div>
                </div>
                <%--END--%>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <%---------------------------------- END MAIN PAGE CONTENT ------------------------------------------------------%>   
</asp:Content>

