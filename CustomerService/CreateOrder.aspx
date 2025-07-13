<%@ Page Title="CreateOrder" Language="C#" EnableEventValidation="false" MasterPageFile="~/Masterpages/MirionDSD.master"
    AutoEventWireup="True" CodeBehind="CreateOrder.aspx.cs" Inherits="CustomerService_CreateOrder" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="act" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style type="text/css">
        th.RightAlignHeaderText {
            text-align: right;
        }

        th.CenterAlignHeaderText {
            text-align: center;
        }

        td.RightAlignItemText {
            text-align: right;
        }

        .CenterTextbox {
            text-align: center;
        }

        .productColor {
            padding: 7px 25px;
            text-align: center;
            text-shadow: 1px 1px 0px #333; /* #333 - black(ish) shadow */
            display: block;
            width: 100px;
        }

        .productColor.Blue {
            background-color: #357195;
            color: white;
        }

        .productColor.Green {
            background-color: #196445;
            color: white;
        }

        .productColor.Black {
            background-color: #000000;
            color: white;
        }

        .productColor.Pink {
            background-color: #dd82b2;
            text-shadow: 1px 1px 0px #fff;
            color: black;
        }

        .productColor.Silver {
            background-color: #c1c1c3;
            text-shadow: 1px 1px 0px #fff;
            color: black;
        }

        .productColor.Blue2 {
            background-color: #005581;
            color: white;
            padding: 7px 25px;
            text-shadow: 1px 1px 0px #333;
        }

        .productColor.Green2 {
            background-color: #00703c;
            color: white;
            text-shadow: 1px 1px 0px #333;
        }

        .productColor.Pink2 {
            background-color: #dc83a6;
            text-shadow: 1px 1px 0px #fff;
            color: black;
        }

        .productColor.Red2 {
            background-color: #c41230;
            color: white;
            text-shadow: 1px 1px 0px #333;
        }
        .productColor.Orange2 {
            background-color: #f68933;
            color: white;
            text-shadow: 1px 1px 0px #333;
        }
        .productColor.Grey2 {
            background-color: #3e3e3f;
            color: White;
            text-shadow: 1px 1px 0px #333;
        }

        .toUpperCase {
            text-transform: uppercase;
        }
    </style>
    <script type="text/javascript">
        $(document).ready(function () {
            // Accordion
            $("#accordion").accordion({
                header: "h3",
                autoHeight: false
            });

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ajaxLoad);
            ajaxLoad();

        });

        function ajaxLoad() {
            $('#<%= txtTransitionServiceStart.ClientID %>').datepicker();

            var activeTabIndex = parseInt($('#<%= hdnfldTabIndex.ClientID %>').val());


            $(".changeLocation").click(function () {
                $('#locationSelectionDialog').dialog('open');
            });

            $('#locationSelectionDialog').dialog({
                autoOpen: false,
                width: 500,
                resizable: false,
                modal: true,
                title: "Select a Location",
                open: function (type, data) {
                    orderAckSent = false;
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Ok": function () {
                        $('#<%= btnChangeLocation.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                        var selLocation = $(this).find('select');
                        selLocation.val(selLocation.find("option:first").val());
                    },
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            $('#assignedWearerDialog').dialog({
                autoOpen: false,
                width: 550,
                resizable: false,
                modal: true,
                title: "Assign Wearers",
                open: function (type, data) {                    
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Assign": function () {
                        $('#<%= btnAssignedWearer.ClientID %>').click();
                    },
                    "Close": function () {
                        $(this).dialog("close");
                    },
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            $('#assignedWearerDialog_Edit').dialog({
                autoOpen: false,
                width: 550,
                resizable: false,
                modal: true,
                title: "Edit Assigned Wearers",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Edit": function () {
                        $('#<%= btnAssignedWearer_Edit.ClientID %>').click();
                    },
                    "Close": function () {
                        $(this).dialog("close");
                    },
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            $("#tabsContainer").tabs({
                selected: activeTabIndex,
                show: function () {
                    var selectedTab = $('#tabsContainer').tabs('option', 'selected');
                    $("#<%= hdnfldTabIndex.ClientID %>").val(selectedTab);
                }
            });

            // selected check box for assignning users.
            $('.chkbxHeaderWearerList').click(function () {
                var ischecked = ($('#ctl00_primaryHolder_gv_WearerList_ctl01_chkbxHeaderWearerList').is(":checked"));
                // add attribute 

                $('#ctl00_primaryHolder_gv_WearerList input:checkbox').attr("checked", function () {
                    this.checked = ischecked;
                });
            });

            // selected check box for un_assignning users.
            $('.chkbxHeaderWearerList_Edit').click(function () {
                var ischecked = ($('#ctl00_primaryHolder_gv_WearerList_Edit_ctl01_chkbxHeaderWearerList_Edit').is(":checked"));
                // add attribute 

                $('#ctl00_primaryHolder_gv_WearerList_Edit input:checkbox').attr("checked", function () {
                    this.checked = ischecked;
                });
            });


        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server">
    </act:ToolkitScriptManager>

    <div id="locationSelectionDialog">
        <asp:UpdatePanel ID="upnlSelectLocation" runat="server">
            <ContentTemplate>
                <div class="OForm">
                    <div class="Row">
                        <div class="Label Small">
                            Account Name:
                        </div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label runat="server" ID="lblAccountName2" Style="display: inline;" />
                                -
                                <asp:Label runat="server" ID="lblAccountID2" Style="display: inline;" />
                            </div>
                        </div>
                        <div class="Clear">
                        </div> 
                    </div>
                    <div class="Row">
                        <div class="Label Small">
                            Location:
                        </div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlSelectLocation" runat="server" DataValueField="LocationID"
                                DataTextField="LocationName">
                            </asp:DropDownList>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div id="SelectedProductName" runat ="server" class="Row">
                        <div class="Label Small">
                            Product Name:
                        </div>
                        <div class="Control">                            
                            <asp:RadioButtonList ID="radProductName" runat="server" RepeatDirection="Horizontal" RepeatColumns="2"
                                 AutoPostBack="False" >
                                <asp:ListItem Value="Instadose 1" Enabled="true" Selected="True">Instadose 1</asp:ListItem>
                                <asp:ListItem Value="Instadose Plus" Enabled="true">Instadose Plus and Instadose 2</asp:ListItem>     
                                <asp:ListItem Value="Instadose 3" Enabled="true">Instadose 3</asp:ListItem>     
                                <%--<asp:ListItem Value="Instadose Elite Demo" Enabled="true">Instadose 2 Elite Demo</asp:ListItem>--%>
                            </asp:RadioButtonList>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div id="SelectedProratePeriod" runat ="server" class="Row" visible="false">
                        <div class="Label Small">
                            ProRate Period:
                        </div>
                        <div class="Control">                            
                            <asp:RadioButtonList ID="radProratePeriod" runat="server" RepeatDirection="Horizontal" RepeatColumns="2"
                                 AutoPostBack="true" OnSelectedIndexChanged="radProratePeriod_SelectedIndexChanged" >                                
                            </asp:RadioButtonList>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                </div>
                <asp:Button Text="Change" Style="display: none;" ID="btnChangeLocation"
                    OnClick="btnChangeLocation_Click" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <div id="assignedWearerDialog" >
        <asp:UpdatePanel ID="upnlAssignedWearer" runat="server">
            <ContentTemplate>
                <div class="FormError" id="assignedWearerDialogError" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="assignedWearerDialogErrorMsg" runat="server" >An error was encountered.</span></p>
				</div>

                <div class="OForm">
                    
                    <div class="Row">
                        <div class="Label Small">Product:</div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblProduct" runat="server" ></asp:Label>
                            </div>                            
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Location:</div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblAssignLocation" runat="server" ></asp:Label>
                            </div>                            
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Color:</div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblColor" runat="server" ></asp:Label>
                            </div>                            
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label Small">Total Quantity:</div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblTotalQty" runat="server" ></asp:Label>
                            </div>                            
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label Small">Total Assigned:</div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblTotalAssignedWearer" runat="server" ></asp:Label>
                            </div>                            
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Search By:</div>
                        <div class="Control">
                            <asp:RadioButtonList ID="rbtnSearchBy" runat="server" RepeatDirection="Horizontal"
                                 AutoPostBack="true" OnSelectedIndexChanged="rbtnSearchBy_SelectedIndexChanged" >
                                <%--<asp:ListItem Value="wearerID" Enabled="true" Selected ="True" >Wearer #</asp:ListItem>
                                <asp:ListItem Value="rangeWearerID" Enabled="true">Range of Wearers</asp:ListItem>  --%>
                                <asp:ListItem Value="all" Enabled="true" Selected ="True" >All</asp:ListItem>
                                <asp:ListItem Value="lastName" Enabled="true">Last Name</asp:ListItem>  
                                <asp:ListItem Value="assigned" Enabled="true">Assigned</asp:ListItem>                                  
                                <asp:ListItem Value="un-assigned" Enabled="true">Un-assigned</asp:ListItem>                                                   
                            </asp:RadioButtonList>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row" id="searchFilter" runat="server" visible="false">
                        <div class="Label Small">Filter:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtOrderAssignSearch" runat="server" CssClass="Size Small" ></asp:TextBox>
                            <%--<asp:TextBox ID="txtFromWearerID" runat="server" CssClass="Size Small" ></asp:TextBox>
                            <asp:Label ID="lblDash" runat="server" CssClass="StaticLabel" Text =" - " Visible ="false"></asp:Label>
                            <asp:TextBox ID="txtToWearerID" runat="server" CssClass="Size Small" Visible ="false" ></asp:TextBox>--%>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    
                    <div class="Row">
                        <div class="Label Small"></div>
                        <div class="Control">
                            <asp:Button ID="btnWearerSearch" runat="server" CssClass="OButton" Text="Search" OnClick="btnWearerSearch_Click"></asp:Button>
                        </div>
                        <div class="Clear"></div>
                    </div>                    
                    
                </div>

                <div class="OToolbar JoinTable" id="WearerListToolBar" runat ="server">
					<ul>
						<li>                                    
							<asp:Label ID="lblSetBodyRegion" runat="server" Text="Set grid to BodyRegion:"></asp:Label>                                   
                            <asp:DropDownList ID="ddlSetBodyRegion" runat="server" AutoPostBack="true" 
                                DataValueField="BodyRegionID" DataTextField="BodyRegionName" OnSelectedIndexChanged="ddlSetBodyRegion_OnSelectedIndexChange" />                                                           
						</li>
							
					</ul>
				</div>

                <div style="overflow-x:no-display; overflow-y: scroll; height: 350px;">
                    <asp:GridView ID="gv_WearerList" runat="server" AutoGenerateColumns="False" CssClass="OTable Scroll" DataKeyNames="UserID"                         
                        OnRowDataBound="gv_WearerList_RowDataBound" >

                        <%--OnPageIndexChanging="gv_WearerList_PageIndexChanging" 
                        OnSorting="gv_WearerList_Sorting"
                        AllowPaging="true" PageSize="10" AllowSorting="true"
                        CurrentSortedColumn="Name" CurrentSortDirection="Ascending"
                        SortOrderAscendingImagePath="~/images/icon_sort_ascending.gif" 
                        SortOrderDescendingImagePath="~/images/icon_sort_descending.gif" > --%>                       
                        <Columns>

                            <asp:TemplateField HeaderStyle-Width ="25px" ItemStyle-HorizontalAlign="Center">
                                <HeaderTemplate>                                    
                                    <asp:CheckBox runat="server" ID="chkbxHeaderWearerList" ToolTip="Select all wearers" class="chkbxHeaderWearerList" />
                                </HeaderTemplate>
                                                    
                                <ItemTemplate>
                                    <asp:CheckBox id="chkbxSelectWearerList" runat="server" class="chkbxRow" />
                                    <asp:HiddenField runat="server" ID="HidUserID"  Value='<%# Eval("UserID") %>' />
                                </ItemTemplate>                                                     
                            </asp:TemplateField>
                       
                            <asp:BoundField DataField="UserID" HeaderText="Wearer #" SortExpression="UserID" ItemStyle-Width="70px" />
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name"  />
                            
                            <asp:TemplateField HeaderText="BodyRegion" ItemStyle-Width="100px">
                                <ItemTemplate>                                    
                                    <asp:Label ID="lblBodyRegionID" runat="server" Text='<%# Eval("BodyRegionID") %>' Visible="false" />
                                    <asp:DropDownList ID="ddlBodyRegion" runat="server" DataTextField="BodyRegionName" DataValueField="BodyRegionID" >                                                            
                                    </asp:DropDownList>
                                </ItemTemplate>
                            </asp:TemplateField>                              
                
                        </Columns>
                        <EmptyDataTemplate>
							<div class="NoData">
								No wearer found.
							</div>
						</EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />
                    </asp:GridView>                  
                </div>

                <asp:Button Text="Assign" Style="display: none;" ID="btnAssignedWearer"
                    OnClick="btnAssignedWearer_Click" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

     <div id="assignedWearerDialog_Edit" >
        <asp:UpdatePanel ID="upnlAssignedWearer_Edit" runat="server">
            <ContentTemplate>
                <div class="FormError" id="assignedWearerDialogError_Edit" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="assignedWearerDialogErrorMsg_Edit" runat="server" >An error was encountered.</span></p>
				</div>

                <div class="OForm">

                    <div class="Row">
                        <div class="Label Small">Product:</div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblProduct_Edit" runat="server" ></asp:Label>
                            </div>                            
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label Small">Color:</div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblColor_Edit" runat="server" ></asp:Label>
                            </div>                            
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label Small">Total Quantity:</div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblTotalQty_Edit" runat="server" ></asp:Label>
                            </div>                            
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label Small">Total Assigned:</div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblTotalAssignedWearer_Edit" runat="server" ></asp:Label>
                            </div>                            
                        </div>
                        <div class="Clear"></div>
                    </div>                                   
                    
                </div>

                <div class="OToolbar JoinTable" id="AssignedWearerToolBar_Edit" runat ="server">
					<ul>
						<li>                                    
							<asp:Label ID="lblUnassignNotice" runat="server" Text="* By unchecking the checkbox you will remove assigned users from the order"></asp:Label>                                                                                                                       
						</li>							
					</ul>
				</div>
                <div style="overflow-x:no-display; overflow-y: scroll; height: 350px;">
                
                    <asp:GridView ID="gv_WearerList_Edit" runat="server" AutoGenerateColumns="False" CssClass="OTable Scroll" DataKeyNames="UserID"                         
                        OnRowDataBound="gv_WearerList_Edit_RowDataBound" >
                        <Columns>

                            <asp:TemplateField HeaderStyle-Width ="25px" ItemStyle-HorizontalAlign="Center">
                                <HeaderTemplate>                                    
                                    <asp:CheckBox runat="server" ID="chkbxHeaderWearerList_Edit" ToolTip="Select all wearers" class="chkbxHeaderWearerList_Edit" />
                                </HeaderTemplate>
                                                    
                                <ItemTemplate>
                                    <asp:CheckBox id="chkbxSelectWearerList_Edit" runat="server" class="chkbxRow" />
                                    <asp:HiddenField runat="server" ID="HidUserID_Edit"  Value='<%# Eval("UserID") %>' />
                                </ItemTemplate>                                                     
                            </asp:TemplateField>
                       
                            <asp:BoundField DataField="UserID" HeaderText="Wearer #" SortExpression="UserID" ItemStyle-Width="70px" />
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name"  />
                            
                            <asp:TemplateField HeaderText="BodyRegion" ItemStyle-Width="100px">
                                <ItemTemplate>                                    
                                    <asp:Label ID="lblBodyRegionID_Edit" runat="server" Text='<%# Eval("BodyRegionID") %>' Visible="false" />
                                    <asp:DropDownList ID="ddlBodyRegion_Edit" runat="server" DataTextField="BodyRegionName" DataValueField="BodyRegionID" >                                                            
                                    </asp:DropDownList>
                                </ItemTemplate>
                            </asp:TemplateField>                              
                
                        </Columns>
                        <EmptyDataTemplate>
							<div class="NoData">
								No wearer found.
							</div>
						</EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
						<PagerStyle CssClass="Footer" />
                    </asp:GridView>
                </div>

                <asp:Button Text="Assign" Style="display: none;" ID="btnAssignedWearer_Edit"
                    OnClick="btnAssignedWearer_Edit_Click" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    
    <asp:UpdatePanel runat="server" ID="upnlPageErrorMsg" >
        <%--<Triggers>
            <asp:AsyncPostBackTrigger controlid="btnChangeLocation" eventname="Click" />
        </Triggers>--%>
        <ContentTemplate>

            <div id="MaxErrorForm" class="FormError" visible="false" runat="server">
                <p>
                    <span class="MessageIcon"></span><strong>Messages:</strong>
                    <asp:Label Text="" ID="lblMaxErrorMessage" runat="server" />
                </p>
            </div>

            <div id="errorForm" class="FormError" visible="false" runat="server">
                <p>
                    <span class="MessageIcon"></span><strong>Messages:</strong>
                    <asp:Label Text="" ID="lblErrorMessage" runat="server" />
                </p>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    
    <asp:ValidationSummary ID ="ValidationSummary1" runat="server" CssClass="FormError" ForeColor="#B94A48" DisplayMode="BulletList"
        ValidationGroup="CreateOrder" EnableClientScript="true" HeaderText="<p><span class='MessageIcon'></span><strong>Messages:</strong> Please verify the following fields are correct:</p>" />

    <asp:UpdatePanel runat="server" ID="upnlToolbar">
        <ContentTemplate>
            <div id="Toolbar" class="OToolbar">
                <ul>
                    <li><a href="#" class="changeLocation Icon Edit">Change Location</a></li>
                    <li style="float:right; margin-right:10px;">Active Badges: <asp:Label ID="lblActiveBadge" runat="server" Text="0" Style="padding: 0;" /></li>
                </ul>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <div id="accordion" style="margin-top: 10px;">
        <h3><a href="#">Billing Address</a></h3>
        <div id="billing_fields" class="OForm AccordionPadding">
            <asp:UpdatePanel ID="upnlBillingInformation" runat="server">
                <ContentTemplate>
                    <asp:HiddenField runat="server" ID="hfAccountID" Value="0" />
                    <asp:HiddenField runat="server" ID="hfLocationID" Value="0" />
                    <asp:HiddenField runat="server" ID="hfOrderID" Value="0" />
                    <asp:HiddenField runat="server" ID="hfOrderType" Value="New" />
                    <asp:HiddenField runat="server" ID="hfSelectedProductName" Value="Instadose 1" />
                    <div class="Row">
                        <div class="Label">
                            Account#:
                        </div>
                        <div class="Control">
                            <div class="Control">
                                <asp:TextBox ID="txtAccountID" runat="server" ReadOnly="true"></asp:TextBox>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Account Name:
                        </div>
                        <div class="Control">
                            <div class="Control">
                                <asp:TextBox ID="txtAccountName" CssClass="Size Large2" MaxLength="100" runat="server"
                                    ReadOnly="true"></asp:TextBox>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Location:
                        </div>
                        <div class="Control">
                            <div class="Control">
                                <asp:TextBox ID="txtLocation" runat="server" ReadOnly="true"></asp:TextBox>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Company<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtBillingCompany" runat="server" MaxLength="100" CssClass="Size Large2"
                                ReadOnly="true"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" ControlToValidate="txtBillingCompany"
                                ValidationGroup="CreateOrder" Display="Dynamic" ErrorMessage="Billing Company"
                                Text="Company is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            First Name<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtBillingFirstName" runat="server" CssClass="Size Medium " MaxLength="50"
                                ReadOnly="true"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidatorFirstNameB" runat="server"
                                ControlToValidate="txtBillingFirstName" ValidationGroup="CreateOrder" Display="Dynamic"
                                ErrorMessage="Billing First Name" Enabled="false" Text="First Name is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Last Name<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtBillingLastName" runat="server" CssClass="Size Medium" MaxLength="50"
                                ReadOnly="true"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidatorLastNameB" runat="server" ControlToValidate="txtBillingLastName"
                                ValidationGroup="CreateOrder" Enabled="false" Display="Dynamic" ErrorMessage="Billing Last Name"
                                Text="Last Name is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Address<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtBillingAddress1" runat="server" CssClass="Size XLarge" MaxLength="255"
                                ReadOnly="true"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="txtBillingAddress1"
                                ValidationGroup="CreateOrder" Enabled="false" Display="Dynamic" ErrorMessage="Billing Address"
                                Text="Address is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtBillingAddress2" Enabled="false" runat="server" CssClass="Size XLarge" MaxLength="255"
                                ReadOnly="true"></asp:TextBox>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtBillingAddress3" Enabled="false" runat="server" CssClass="Size XLarge" MaxLength="255"
                                ReadOnly="true"></asp:TextBox>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Country<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlBillingCountry" runat="server" DataValueField="CountryID"
                                Enabled="false" DataTextField="CountryName" AutoPostBack="true" OnSelectedIndexChanged="ddlCountry_OnSelectedIndexChange" />
                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator23" ControlToValidate="ddlBillingCountry"
                                ErrorMessage="Billing Country" Enabled="false" Text="Country is required" Display="Dynamic" InitialValue="0"
                                ValidationGroup="CreateOrder">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            City<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtBillingCity" runat="server" CssClass="Size Large2" MaxLength="100"
                                ReadOnly="true"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="txtBillingCity"
                                ValidationGroup="CreateOrder" Enabled="false" Display="Dynamic" ErrorMessage="Billing City" Text="City is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            State/Postal<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlBillingState" Enabled="false" runat="server" DataValueField="StateID" DataTextField="StateAbbrev" />
                            <asp:TextBox ID="txtBillingPostalCode" Enabled="false" runat="server" CssClass="Size Small" MaxLength="15"></asp:TextBox>
                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator9" ControlToValidate="ddlBillingState"
                                ErrorMessage="Billing State" Text="State is required" Display="Dynamic" InitialValue="0"
                                ValidationGroup="CreateOrder">
                            </asp:RequiredFieldValidator>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" ControlToValidate="txtBillingPostalCode"
                                ValidationGroup="CreateOrder" Display="Dynamic" ErrorMessage="Billing Postal Code"
                                Text="Postal Code is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <h3><a href="#">Shipping Address</a></h3>
        <div id="shipping_fields" class="OForm AccordionPadding">
            <asp:UpdatePanel ID="upnlShippingInformation" runat="server">
                <ContentTemplate>
                    <div class="Row">
                        <div class="Label">
                            Company<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtShippingCompany" runat="server" MaxLength="100" CssClass="Size Large2"
                                ReadOnly="true"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator19" runat="server" ControlToValidate="txtShippingCompany"
                                ValidationGroup="CreateOrder" Display="Dynamic" ErrorMessage="Shipping Company"
                                Text="Company is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            First Name<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox CssClass="Size Medium " MaxLength="50" ID="txtShippingFirstName" ReadOnly="true"
                                runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator15" runat="server" ControlToValidate="txtShippingFirstName"
                                ValidationGroup="CreateOrder" Display="Dynamic" ErrorMessage="Shipping First Name"
                                Text="First Name is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Last Name<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtShippingLastName" runat="server" CssClass="Size Medium" ReadOnly="true"
                                MaxLength="50"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator16" runat="server" ControlToValidate="txtShippingLastName"
                                ValidationGroup="CreateOrder" Display="Dynamic" ErrorMessage="Shipping Last Name"
                                Text="Last Name is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Address<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtShippingAddress1" runat="server" CssClass="Size XLarge" ReadOnly="true"
                                MaxLength="255"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator17" runat="server" ControlToValidate="txtShippingAddress1"
                                ValidationGroup="CreateOrder" Display="Dynamic" ErrorMessage="Shipping Address"
                                Text="Address is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtShippingAddress2" runat="server" CssClass="Size XLarge" ReadOnly="true"
                                MaxLength="255"></asp:TextBox>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtShippingAddress3" runat="server" CssClass="Size XLarge" ReadOnly="true"
                                MaxLength="255"></asp:TextBox>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Country<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlShippingCountry" runat="server" DataValueField="CountryID"
                                DataTextField="CountryName" AutoPostBack="true" Enabled="false" />
                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator24" ControlToValidate="ddlShippingCountry"
                                ErrorMessage="Shipping Country" Text="Country is required" Display="Dynamic"
                                InitialValue="0" ValidationGroup="CreateOrder">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            City<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtShippingCity" runat="server" CssClass="Size Large2" ReadOnly="true"
                                MaxLength="100"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator18" runat="server" ControlToValidate="txtShippingCity"
                                ValidationGroup="CreateOrder" Display="Dynamic" ErrorMessage="Shipping City"
                                Text="City is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            State/Postal<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlShippingState" runat="server" DataValueField="StateID" DataTextField="StateAbbrev"
                                Enabled="false" />
                            <asp:TextBox ID="txtShippingPostalCode" runat="server" CssClass="Size Small" MaxLength="15"
                                ReadOnly="true"></asp:TextBox>
                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator27" ControlToValidate="ddlShippingState"
                                ErrorMessage="Shipping State" Text="State is required" Display="Dynamic" InitialValue="0"
                                ValidationGroup="CreateOrder">
                            </asp:RequiredFieldValidator>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator26" runat="server" ControlToValidate="txtShippingPostalCode"
                                ValidationGroup="CreateOrder" Display="Dynamic" ErrorMessage="Shipping Postal Code"
                                Text="Postal Code is required">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <h3><a href="#">Billing & Pricing</a></h3>
        <div class="OForm AccordionPadding">
            <asp:UpdatePanel ID="upnlPricingInformation" runat="server">
                <ContentTemplate>
                    <div class="Row">
                        <div class="Label">
                            Currency<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlCurrency" runat="server" AutoPostBack="false" DataValueField="CurrencyCode"
                                DataTextField="CurrencyCode" Enabled="False">
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator34" ControlToValidate="ddlCurrency"
                                ErrorMessage="Currency" Text="Currency is required" Display="Dynamic" InitialValue="0"
                                ValidationGroup="CreateOrder">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Referral<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlReferral" runat="server" AutoPostBack="false" DataValueField="SalesRepDistID"
                                DataTextField="SalesCompanyName" Enabled="False">
                            </asp:DropDownList>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Rate Code:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtRateCode" runat="server" AutoPostBack="true" Enabled="false"
                                CssClass="Size Small"></asp:TextBox>
                            <asp:Label ID="lblInvalidRate" ForeColor="Red" runat="server"></asp:Label>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Billing Term<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:Label ID="lblBillingTerm" runat="server" Text="Yearly" CssClass="LabelValue" />
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row" style="display: none;">
                        <div class="Label">
                            Coupon<span class="Required">*</span>:
                        </div>
                        <div class="Row" style="display: none;">
                            <asp:DropDownList ID="ddlCoupon" runat="server" CssClass="tb" DataValueField="CouponID"
                                DataTextField="CouponCode" AutoPostBack="true">
                            </asp:DropDownList>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <div class="Row">
                        <div class="Label">
                            Service Period:
                        </div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label ID="lblServiceStartDate" runat="server" Text="" Style="display: inline" />
                                -
                                <asp:Label ID="lblServiceEndDate" runat="server" Text="" Style="display: inline" />
                                (<asp:Label ID="lblServiceDayLeft" runat="server" Text="0" Style="display: inline" />)

                                <asp:CustomValidator ErrorMessage="The contract service period has expired." OnServerValidate="valServicePeriod_ServerValidate"
                                     ID="valServicePeriod" ValidationGroup="CreateOrder" EnableClientScript="false" runat="server" />

                            </div>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <asp:Panel runat="server" ID="pnlServiceStartDate" Visible="false" CssClass="Row">
                        <div class="Label">
                            Order Start:
                        </div>
                        <div class="Control">
                            <asp:DropDownList runat="server" ID="ddlServiceStartDate">
                            </asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlProratePeriod" Visible="false" CssClass="Row">
                        <div class="Label">
                            Prorate Period:
                        </div>
                        <div class="Control">
                            <div class="LabelValue">
                                <asp:Label Text="" ID="lblProratePeriod" Style="display: inline" runat="server" />
                            </div>                            
                        </div>
                        <div class="Clear"></div>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlTransitionServicePeriod" Visible="false" CssClass="Row">
                        <div class="Label">
                            Transition Period<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtTransitionServiceStart" runat="server" CssClass="Size Small"
                                AutoPostBack="true" OnTextChanged="txtTransitionServiceStart_TextChanged"></asp:TextBox>
                            -
                            <asp:Label Text="MM/dd/yyyy" ID="lblTransitionServiceEnd" Style="display: inline"
                                runat="server" />
                        </div>
                        <div class="Clear">
                        </div>
                    </asp:Panel>
                    <div class="Row">
                        <div class="Label">
                            Account Discounts:
                        </div>
                        <div class="Control">
                            <asp:GridView ID="gvDiscounts" CssClass="OTable"
                                AlternatingRowStyle-CssClass="Alt" runat="server" AutoGenerateColumns="False" 
                                DataKeyNames="ProductGroupID" OnRowDataBound="gvDiscounts_RowDataBound" Width="450px">
                                <AlternatingRowStyle CssClass="Alt" />
                                <Columns>
                                    <asp:BoundField DataField="ProductGroupName" HeaderText="Product Group" SortExpression="ProductGroupName" />
                                    <asp:BoundField DataField="Discount" HeaderText="Discount" SortExpression="Discount %" />
                                    <asp:BoundField HeaderText="Target Price" SortExpression="Discount" />
                                    <asp:TemplateField Visible="false" ItemStyle-HorizontalAlign="center">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hfProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
                                            <asp:HiddenField runat="server" ID="hfDiscount" Value='<%# Eval("Discount") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    <div>This account has no discounts.</div>
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
        </div>
        <h3><a id="tabheadProd" runat="server" href="#">Products</a></h3>
        <div id="divProducts" class="OForm AccordionPadding">            
            <asp:UpdatePanel ID="updtpnlAccountAndLocationInformation" runat="server" >
                <ContentTemplate>
                    <asp:HiddenField ID="hdnfldTabIndex" runat="server" Value="0" />

                    <div id="divTabs" runat="server">
                        <div class="OToolbar" id="orderTypeForm" visible="false" runat="server">
                            <ul>
                                <li style="margin: 5px">Order Type:
                                </li>
                                <li>
                                    <asp:RadioButton Text="Broken Replacement" ID="rbtnReplacementBrokenClip" AutoPostBack="true"
                                        GroupName="OrderType" runat="server" OnCheckedChanged="rbtnReplacementBrokenClip_CheckedChanged" />
                                    <asp:RadioButton Text="Lost Replacement" ID="rbtnReplacementLostBadge" AutoPostBack="true"
                                        GroupName="OrderType" runat="server" OnCheckedChanged="rbtnReplacementLostBadge_CheckedChanged" />
                                    <asp:RadioButton Text="Lost and No Replacement" ID="rbtnNoReplacement" AutoPostBack="true"
                                        GroupName="OrderType" runat="server" OnCheckedChanged="rbtnNoReplacement_CheckedChanged" />
                                </li>
                            </ul>
                        </div>                                                  

                        <%--PRODUCTS GRIDVIEW--%>
                        <div id="tabsContainer">
                            <div id="divTabs2" runat="server">
                                <ul>
                                    <li><a href="#tabInstadose1" id="tabInsta1" runat="server">Instadose 1</a></li>
                                    <li><a href="#tabInstadosePlus" id="tabInstaPlus" runat="server">Instadose Plus</a></li>
                                    <li><a href="#tabInstadosePlusAccessories" id="tabInstaPlusAccessories" runat="server" visible="false">Instadose Plus Accessories</a></li>
                                    <li><a href="#tabInstadose2" id="tabInsta2" runat="server" visible="false">Instadose 2</a></li>
                                    <li><a href="#tabInstadoseEliteDemo" id="tabInstaEliteDemo" runat="server" visible="false">Instadose 2 Elite Demo</a></li>
                                    <li><a href="#tabInstadose3" id="tabInsta3" runat="server" visible="false">Instadose 3</a></li>
                                </ul>
                                <div id="tabInstadose1">
                                    <asp:UpdatePanel ID="upnlInstadose1" runat="server">
                                        <ContentTemplate>
                                            <div id="divInstadose1" style="margin: 0 auto; width: 850px;">                                                

                                                <asp:GridView ID="gvProduct" runat="server" AutoGenerateColumns="False"
                                                    DataKeyNames="ProductID,ProductName,Color,ProductSKU"
                                                    CellPadding="0" AlternatingRowStyle-CssClass="Alt" GridLines="None"
                                                    EmptyDataText="No products on this order."
                                                    CssClass="OTable" Visible="true">
                                                    <RowStyle HorizontalAlign="Right" />
                                                    <AlternatingRowStyle CssClass="Alt" />
                                                    <Columns>
                                                        <asp:BoundField DataField="ProductName" HeaderText="Product" ItemStyle-Width="250px" HeaderStyle-Wrap="False"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Color" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                                <asp:Label runat="server" ID="lblColor" Text='<%# DataBinder.Eval(Container.DataItem,"Color","" )%>'
                                                                CssClass='<%#string.Format("productColor {0}", DataBinder.Eval(Container.DataItem,"Color","" ))%>' />
                                                                <asp:HiddenField runat="server" ID="hfProductID" Value='<%# Eval("ProductID") %>' />
                                                                <asp:HiddenField runat="server" ID="hfProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="ProductSKU" HeaderText="SKU" ItemStyle-Width="300px"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Qty" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtQty" runat="server" Text='' OnTextChanged="txtQty_OnTextChanged"
                                                                    TabIndex="0" CssClass="CenterTextbox" AutoPostBack="true" MaxLength="4" />
                                                            </ItemTemplate>
                                                            <ControlStyle Width="100px" />
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="100px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="Price">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblPrice" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle CssClass="RightAlignItemText" Width="125px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle CssClass="RightAlignItemText" HorizontalAlign="Right" Wrap="False" Width="75px" />
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="Total ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblExtended" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle Width="150px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle HorizontalAlign="Right" Wrap="False" Width="175px" />
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <HeaderStyle CssClass="Alt" Font-Bold="True" ForeColor="Black" />
                                                </asp:GridView>
                                                

                                                <%--//********************************************************************************
                                                // gridView gvRenewal
                                                //*********************************************************************************--%>
                                                <asp:GridView ID="gvRenewal" runat="server" AutoGenerateColumns="False" visible="false" DataKeyNames="ProductID,ProductName,BillingTermDesc,BillingTermID"
                                                    CellPadding="0" AlternatingRowStyle-CssClass="Alt" GridLines="None" EmptyDataText="No products on this order."
                                                    CssClass="OTable" >
                                                    <RowStyle HorizontalAlign="Right" />
                                                    <AlternatingRowStyle CssClass="Alt" />
                                                    <Columns>
                                                        <asp:BoundField DataField="ProductName" HeaderText="Product"
                                                            ItemStyle-Width="150" ItemStyle-HorizontalAlign="NotSet"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="" HeaderStyle-HorizontalAlign="Left" Visible="false">
                                                            <ItemTemplate>
                                                                <asp:HiddenField runat="server" ID="hfProductID" Value='<%# Eval("ProductID") %>' />
                                                                <asp:HiddenField runat="server" ID="hfProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                           
                                                        <asp:TemplateField HeaderText="Qty" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtQty" runat="server" Text='<%# Eval("Quantity") %>' OnTextChanged="txtQty_Renewal_OnTextChanged"
                                                                    TabIndex="0" CssClass="CenterTextbox" AutoPostBack="true" MaxLength="4" />
                                                            </ItemTemplate>
                                                            <ControlStyle Width="75px" />
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="75px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="Price ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblPrice" text='<%#  Eval("Price") %>' />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle CssClass="RightAlignItemText" Width="125px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle CssClass="RightAlignItemText" HorizontalAlign="Right" Wrap="False" Width="75px" />
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="Total ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblTotal" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle Width="150px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle HorizontalAlign="Right" Wrap="False" Width="175px" />
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <HeaderStyle CssClass="Alt" Font-Bold="True" ForeColor="Black" />
                                                </asp:GridView>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>

                                <div id="tabInstadosePlus">
                                    <asp:UpdatePanel ID="upnlInstadosePlus" runat="server">
                                        <%--<Triggers>
				                            <asp:AsyncPostBackTrigger controlid="btnAssignedWearer" eventname="Click" />
			                            </Triggers>--%>
                                        <ContentTemplate>
                                            <div id="divInstadosePlus" style="margin: 0 auto; width: 850px">

                                                <%--//********************************************************************************
                                                // Instadose Plus gridView
                                                //*********************************************************************************--%>
                                                <asp:GridView ID="gvInstadosePlus" runat="server" AutoGenerateColumns="False" DataKeyNames="ProductID,ProductName,Color,ProductSKU,BillingTermDesc,BillingTermID"
                                                    CellPadding="0" AlternatingRowStyle-CssClass="Alt" GridLines="None" EmptyDataText="No products on this order."
                                                    CssClass="OTable" Visible="true"
                                                    OnRowDataBound="gvInstadosePlus_RowDataBound">
                                                    <RowStyle HorizontalAlign="Right" />
                                                    <AlternatingRowStyle CssClass="Alt" />
                                                    <Columns>
                                                        <asp:BoundField DataField="ProductName" HeaderText="Product"
                                                            ItemStyle-Width="150px" ItemStyle-HorizontalAlign="NotSet"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Color" ItemStyle-Width="120px" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                                <asp:Label runat="server" CssClass='<%# DataBinder.Eval(Container.DataItem,"Color","productColor {0}2" )%>' Text='<%# DataBinder.Eval(Container.DataItem,"Color" )%>' ID="lblColor" />
                                                                <asp:HiddenField runat="server" ID="hfProductID" Value='<%# Eval("ProductID") %>' />
                                                                <asp:HiddenField runat="server" ID="hfProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="ProductSKU" HeaderText="SKU" ItemStyle-Width="200px"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Qty" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtQty" runat="server" Text='' OnTextChanged="txtQtyInstadosePlus_OnTextChanged"
                                                                    TabIndex="0" CssClass="CenterTextbox" AutoPostBack="true" MaxLength="4" />
                                                            </ItemTemplate>
                                                            <ControlStyle Width="50px" />
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="50px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Assigned Wearer" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:LinkButton ID="btnAssignedWearerByProduct" runat="server"  
                                                                    CommandName="AssignedWearerByProduct" CommandArgument='<%# Eval("ProductID")%>' 
                                                                    onclick="btnAssignedWearerByProduct_Click" ></asp:LinkButton>                                                                

                                                                <asp:ImageButton ID="imgAssignedWearerByProduct" runat="server" ImageUrl="~/images/icons/user_add.png" Width="16px" Height="16px" 
                                                                    style="position:relative;top:4px"
                                                                    AlternateText="" ToolTip="Assign wearer to order"
                                                                    onClick="imgAssignedWearerByProduct_Click"
                                                                    CommandName="AssignedWearerByProduct" CommandArgument='<%# Eval("ProductID") %>' />
                                                            </ItemTemplate>
                                                            
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="75px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Price ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblPrice" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle CssClass="RightAlignItemText" Width="100px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle CssClass="RightAlignItemText" HorizontalAlign="Right" Wrap="False" Width="75px" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Total ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblExtended" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle Width="150px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle HorizontalAlign="Right" Wrap="False" Width="150px" />
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <HeaderStyle CssClass="Alt" Font-Bold="True" ForeColor="Black" />
                                                </asp:GridView>

                                                <%--//********************************************************************************
                                                // Instadose Plus Caps gridView
                                                //*********************************************************************************--%>
                                                <asp:GridView ID="gvInstadosePlusCaps" runat="server" AutoGenerateColumns="False" DataKeyNames="ProductID,ProductName,Color,ProductSKU,BillingTermDesc,BillingTermID"
                                                    CellPadding="0" AlternatingRowStyle-CssClass="Alt" GridLines="None" EmptyDataText="No products on this order."
                                                    CssClass="OTable" Visible="false"
                                                    >
                                                    <RowStyle HorizontalAlign="Right" />
                                                    <AlternatingRowStyle CssClass="Alt" />
                                                    <Columns>
                                                        <asp:BoundField DataField="ProductSKU" HeaderText="Product"
                                                            ItemStyle-Width="150px" ItemStyle-HorizontalAlign="NotSet" ItemStyle-CssClass="toUpperCase"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Color" ItemStyle-Width="120px" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                                <asp:Label runat="server" CssClass='<%# DataBinder.Eval(Container.DataItem,"Color","productColor {0}2" )%>' Text='<%# DataBinder.Eval(Container.DataItem,"Color" )%>' ID="lblColor" />
                                                                <asp:HiddenField runat="server" ID="hfProductID" Value='<%# Eval("ProductID") %>' />
                                                                <asp:HiddenField runat="server" ID="hfProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="ProductSKU" HeaderText="SKU" ItemStyle-Width="200px"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Qty" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtQty_Caps" runat="server" Text='' OnTextChanged="txtQty_Caps_OnTextChanged"
                                                                    TabIndex="0" CssClass="CenterTextbox" AutoPostBack="true" MaxLength="4" />
                                                            </ItemTemplate>
                                                            <ControlStyle Width="50px" />
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="50px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Price ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblPrice" text='<%# Eval("Price") %>' />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle CssClass="RightAlignItemText" Width="100px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle CssClass="RightAlignItemText" HorizontalAlign="Right" Wrap="False" Width="75px" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Total ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblTotal" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle Width="150px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle HorizontalAlign="Right" Wrap="False" Width="150px" />
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <HeaderStyle CssClass="Alt" Font-Bold="True" ForeColor="Black" />
                                                </asp:GridView>


                                                <div id="div1" style="margin: 0 auto; width: 850px">
                                                <%--//********************************************************************************
                                                // Instadose accessories gridView
                                                //*********************************************************************************--%>
                                                    <asp:GridView ID="gvAccessories" runat="server" AutoGenerateColumns="False" DataKeyNames="ProductID,ProductName,Color,ProductSKU,BillingTermDesc,BillingTermID"
                                                        CellPadding="0" AlternatingRowStyle-CssClass="Alt" GridLines="None" EmptyDataText="No products on this order."
                                                        CssClass="OTable" Visible="true">
                                                        <RowStyle HorizontalAlign="Right" />
                                                        <AlternatingRowStyle CssClass="Alt" />
                                                        <Columns>
                                                            <asp:BoundField DataField="ProductName" HeaderText="Product"
                                                                ItemStyle-Width="150" ItemStyle-HorizontalAlign="NotSet"></asp:BoundField>
                                                            <asp:TemplateField HeaderText="" HeaderStyle-HorizontalAlign="Left">
                                                                <ItemTemplate>
                                                                    <asp:HiddenField runat="server" ID="hfProductID" Value='<%# Eval("ProductID") %>' />
                                                                    <asp:HiddenField runat="server" ID="hfProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:BoundField DataField="ProductSKU" HeaderText="SKU" ItemStyle-Width="150"></asp:BoundField>
                                                            <asp:TemplateField HeaderText="Qty" ItemStyle-HorizontalAlign="Center">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="txtQty" runat="server" Text='' OnTextChanged="txtQty_Accessories_OnTextChanged"
                                                                        TabIndex="0" CssClass="CenterTextbox" AutoPostBack="true" MaxLength="4" />
                                                                </ItemTemplate>
                                                                <ControlStyle Width="75px" />
                                                                <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                                <ItemStyle CssClass="CenterAlignItemText" Width="75px" Wrap="False" HorizontalAlign="Center" />
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Price ">
                                                                <ItemTemplate>
                                                                    <div class="LabelValue;RightAlignItemText">
                                                                        <asp:Label runat="server" ID="lblPrice" />
                                                                    </div>
                                                                </ItemTemplate>
                                                                <ControlStyle CssClass="RightAlignItemText" Width="125px" />
                                                                <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                                <ItemStyle CssClass="RightAlignItemText" HorizontalAlign="Right" Wrap="False" Width="75px" />
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Total ">
                                                                <ItemTemplate>
                                                                    <div class="LabelValue;RightAlignItemText">
                                                                        <asp:Label runat="server" ID="lblExtended" />
                                                                    </div>
                                                                </ItemTemplate>
                                                                <ControlStyle Width="150px" />
                                                                <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                                <ItemStyle HorizontalAlign="Right" Wrap="False" Width="175px" />
                                                            </asp:TemplateField>
                                                        </Columns>
                                                        <HeaderStyle CssClass="Alt" Font-Bold="True" ForeColor="Black" />
                                                    </asp:GridView>
                                                </div>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>  <%-- tabInstadosePlus tab end--%>

                                <div id="tabInstadoseEliteDemo">
                                    <asp:UpdatePanel ID="upnlInstadoseEliteDemo" runat="server">
                                        <ContentTemplate>
                                            <div id="divInstadoseEliteDemo" style="margin: 0 auto; width: 850px">

                                                <%--//********************************************************************************
                                                // Instadose Plus gridView
                                                //*********************************************************************************--%>
                                                <asp:GridView ID="gvInstadoseEliteDemo" runat="server" AutoGenerateColumns="False"      
                                                    DataKeyNames="ProductID,ProductName,Color,ProductSKU,BillingTermDesc,BillingTermID"
                                                    CellPadding="0" AlternatingRowStyle-CssClass="Alt" GridLines="None" EmptyDataText="No products on this order."
                                                    CssClass="OTable" Visible="true"
                                                    OnRowDataBound="gvInstadoseEliteDemo_RowDataBound">
                                                    <RowStyle HorizontalAlign="Right" />
                                                    <AlternatingRowStyle CssClass="Alt" />
                                                    <Columns>
                                                        <asp:BoundField DataField="ProductName" HeaderText="Product" ItemStyle-Width="150px" ItemStyle-HorizontalAlign="NotSet"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Color" ItemStyle-Width="120px" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                                <asp:Label runat="server" CssClass='<%# DataBinder.Eval(Container.DataItem,"Color","productColor {0}2" )%>' Text='<%# DataBinder.Eval(Container.DataItem,"Color" )%>' ID="lblColor" />
                                                                <asp:HiddenField runat="server" ID="hfProductID" Value='<%# Eval("ProductID") %>' />
                                                                <asp:HiddenField runat="server" ID="hfProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="ProductSKU" HeaderText="SKU" ItemStyle-Width="200px"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Qty" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtQty" runat="server" Text='' OnTextChanged="txtQty_OnTextChanged"
                                                                    TabIndex="0" CssClass="CenterTextbox" AutoPostBack="true" MaxLength="4" />
                                                            </ItemTemplate>
                                                            <ControlStyle Width="50px" />
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="50px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>

                                                        <%--<asp:TemplateField HeaderText="Assigned Wearer" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:LinkButton ID="btnAssignedWearerByProduct" runat="server"  
                                                                    CommandName="AssignedWearerByProduct" CommandArgument='<%# Eval("ProductID")%>' 
                                                                    onclick="btnAssignedWearerByProduct_Click" ></asp:LinkButton>                                                                

                                                                <asp:ImageButton ID="imgAssignedWearerByProduct" runat="server" ImageUrl="~/images/icons/user_add.png" Width="16px" Height="16px" 
                                                                    style="position:relative;top:4px"
                                                                    AlternateText="" ToolTip="Assign wearer to order"
                                                                    onClick="imgAssignedWearerByProduct_Click"
                                                                    CommandName="AssignedWearerByProduct" CommandArgument='<%# Eval("ProductID") %>' />
                                                            </ItemTemplate>
                                                            
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="75px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>--%>

                                                        <asp:TemplateField HeaderText="Price ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblPrice" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle CssClass="RightAlignItemText" Width="100px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle CssClass="RightAlignItemText" HorizontalAlign="Right" Wrap="False" Width="75px" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Total ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblExtended" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle Width="150px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle HorizontalAlign="Right" Wrap="False" Width="150px" />
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <HeaderStyle CssClass="Alt" Font-Bold="True" ForeColor="Black" />
                                                </asp:GridView>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>

                                <%-- begining of tabInstadosePlusAccessories Accessories tab 
                               	<div id="tabInstadosePlusAccessories">
		                             <asp:UpdatePanel ID="upnlInstadosePlusAccessories" runat="server">
		                                  <ContentTemplate>
                                            	<div id="divInstadosePlusAccessories" style="margin: 0 auto; width: 850px;">
                                            	                                            		  
                                            		
							                    </div>
						                 </ContentTemplate>
					                </asp:UpdatePanel>
					            </div>   tabInstadosePlusAccessories Accessories tab end--%>      

                                <%--start of id 2 tab--%>
                                <div id="tabInstadose2">
                                    <asp:UpdatePanel ID="upnlInstadose2" runat="server">
                                        <ContentTemplate>
                                            <div id="divInstadose2" style="margin: 0 auto; width: 850px">
                                                <%--//********************************************************************************
                                                // Instadose Plus gridView
                                                //*********************************************************************************--%>
                                                <asp:GridView ID="gvInstadose2" runat="server" AutoGenerateColumns="False" DataKeyNames="ProductID,ProductName,Color,ProductSKU,BillingTermDesc,BillingTermID"
                                                    CellPadding="0" AlternatingRowStyle-CssClass="Alt" GridLines="None" EmptyDataText="No products on this order."
                                                    CssClass="OTable" Visible="true"
                                                    OnRowDataBound="gvInstadose2_RowDataBound">
                                                    <RowStyle HorizontalAlign="Right" />
                                                    <AlternatingRowStyle CssClass="Alt" />
                                                    <Columns>
                                                        <asp:BoundField DataField="ProductName" HeaderText="Product"
                                                            ItemStyle-Width="150px" ItemStyle-HorizontalAlign="NotSet"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Color" ItemStyle-Width="120px" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                                <asp:Label runat="server" CssClass='<%# DataBinder.Eval(Container.DataItem,"Color","productColor {0}2" )%>' Text='<%# DataBinder.Eval(Container.DataItem,"Color" )%>' ID="lblColor" />
                                                                <asp:HiddenField runat="server" ID="hfProductID" Value='<%# Eval("ProductID") %>' />
                                                                <asp:HiddenField runat="server" ID="hfProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="ProductSKU" HeaderText="SKU" ItemStyle-Width="200px"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Qty" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtQty" runat="server" Text='' OnTextChanged="txtQtyInstadose2_OnTextChanged"
                                                                    TabIndex="0" CssClass="CenterTextbox" AutoPostBack="true" MaxLength="4" />
                                                            </ItemTemplate>
                                                            <ControlStyle Width="50px" />
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="50px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Assigned Wearer" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:LinkButton ID="btnAssignedWearerByProduct" runat="server"  
                                                                    CommandName="AssignedWearerByProduct" CommandArgument='<%# Eval("ProductID")%>' 
                                                                    onclick="btnAssignedWearerByProduct_Click" ></asp:LinkButton>                                                                

                                                                <asp:ImageButton ID="imgAssignedWearerByProduct" runat="server" ImageUrl="~/images/icons/user_add.png" Width="16px" Height="16px" 
                                                                    style="position:relative;top:4px"
                                                                    AlternateText="" ToolTip="Assign wearer to order"
                                                                    onClick="imgAssignedWearerByProduct_Click"
                                                                    CommandName="AssignedWearerByProduct" CommandArgument='<%# Eval("ProductID") %>' />
                                                            </ItemTemplate>
                                                            
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="75px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Price ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblPrice" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle CssClass="RightAlignItemText" Width="100px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle CssClass="RightAlignItemText" HorizontalAlign="Right" Wrap="False" Width="75px" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Total ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblExtended" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle Width="150px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle HorizontalAlign="Right" Wrap="False" Width="150px" />
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <HeaderStyle CssClass="Alt" Font-Bold="True" ForeColor="Black" />
                                                </asp:GridView>


                                                <%--//********************************************************************************
										       	// Instadose 2 gridView for consignment
										       	//*********************************************************************************--%>
										       	<asp:GridView ID="gvInstadose2Con" runat="server" AutoGenerateColumns="False" DataKeyNames="ProductID,ProductName,Color,ProductSKU,BillingTermDesc,BillingTermID"
										       		CellPadding="0" AlternatingRowStyle-CssClass="Alt" GridLines="None" EmptyDataText="No products on this order."
										       		CssClass="OTable" Visible="false"
										       		OnRowDataBound="gvInstadose2Con_RowDataBound">
										       		<RowStyle HorizontalAlign="Right" />
										       		<AlternatingRowStyle CssClass="Alt" />
										       		<Columns>
										       			<asp:BoundField DataField="ProductName" HeaderText="Product"
										       				ItemStyle-Width="250px" ItemStyle-HorizontalAlign="NotSet" ItemStyle-CssClass="toUpperCase"></asp:BoundField>
										       			<asp:TemplateField HeaderText="Color" ItemStyle-Width="120px" HeaderStyle-HorizontalAlign="Left">
										       				<ItemTemplate>
										       					<asp:Label runat="server" CssClass='<%# DataBinder.Eval(Container.DataItem,"Color","productColor {0}2" )%>' Text='<%# DataBinder.Eval(Container.DataItem,"Color" )%>' ID="lblColor" />
										       					<asp:HiddenField runat="server" ID="hfProductID" Value='<%# Eval("ProductID") %>' />
										       					<asp:HiddenField runat="server" ID="hfProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
										       				</ItemTemplate>
										       			</asp:TemplateField>
										       			<asp:BoundField DataField="ProductSKU" HeaderText="SKU" ItemStyle-Width="200px"></asp:BoundField>
										       			<asp:TemplateField HeaderText="Qty" ItemStyle-HorizontalAlign="Center">
										       				<ItemTemplate>
										       					<asp:TextBox ID="txtID2ConQty" runat="server" Text='' OnTextChanged="txtID2ConQty_OnTextChanged"
										       					    TabIndex="0" CssClass="CenterTextbox" AutoPostBack="true" MaxLength="4" />
										       				</ItemTemplate>
										       				<ControlStyle Width="50px" />
										       				<HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
										       				<ItemStyle CssClass="CenterAlignItemText" Width="50px" Wrap="False" HorizontalAlign="Center" />
										       			</asp:TemplateField>
										       					    
										       									       					    
										       			<asp:TemplateField HeaderText="Price ">
										       				<ItemTemplate>
										       					<div class="LabelValue;RightAlignItemText">
										       					    <asp:Label runat="server" ID="lblPrice" />
										       					</div>
										       				</ItemTemplate>
										       				<ControlStyle CssClass="RightAlignItemText" Width="100px" />
										       				<HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
										       				<ItemStyle CssClass="RightAlignItemText" HorizontalAlign="Right" Wrap="False" Width="75px" />
										       			</asp:TemplateField>
										       					    
										       			<asp:TemplateField HeaderText="Total ">
										       				<ItemTemplate>
										       					<div class="LabelValue;RightAlignItemText">
										       					    <asp:Label runat="server" ID="lblExtended" />
										       					</div>
										       				</ItemTemplate>
										       				<ControlStyle Width="150px" />
										       				<HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
										       				<ItemStyle HorizontalAlign="Right" Wrap="False" Width="150px" />
										       			</asp:TemplateField>
										       		</Columns>
										       		<HeaderStyle CssClass="Alt" Font-Bold="True" ForeColor="Black" />
				                                </asp:GridView>  <%--end id 2 con grid--%>

                                          
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div> 
                                <%--end of id 2 tab--%>

                                <%--start of id 3 tab--%>
                                <div id="tabInstadose3">
                                    <asp:UpdatePanel ID="upnlInstadose3" runat="server">
                                        <ContentTemplate>
                                            <div id="divInstadose3" style="margin: 0 auto; width: 850px">
                                                <%--//********************************************************************************
                                                // Instadose 3 gridView
                                                //*********************************************************************************--%>
                                                <asp:GridView ID="gvInstadose3" runat="server" AutoGenerateColumns="False" DataKeyNames="ProductID,ProductName,Color,ProductSKU,BillingTermDesc,BillingTermID"
                                                    CellPadding="0" AlternatingRowStyle-CssClass="Alt" GridLines="None" EmptyDataText="No products on this order."
                                                    CssClass="OTable" Visible="true"
                                                    OnRowDataBound="gvInstadose3_RowDataBound">
                                                    <RowStyle HorizontalAlign="Right" />
                                                    <AlternatingRowStyle CssClass="Alt" />
                                                    <Columns>
                                                        <asp:BoundField DataField="ProductName" HeaderText="Product"
                                                            ItemStyle-Width="150px" ItemStyle-HorizontalAlign="NotSet"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Color" ItemStyle-Width="120px" HeaderStyle-HorizontalAlign="Left">
                                                            <ItemTemplate>
                                                                <asp:Label runat="server" CssClass='<%# DataBinder.Eval(Container.DataItem,"Color","productColor {0}2" )%>' Text='<%# DataBinder.Eval(Container.DataItem,"Color" )%>' ID="lblColor" />
                                                                <asp:HiddenField runat="server" ID="hfProductID" Value='<%# Eval("ProductID") %>' />
                                                                <asp:HiddenField runat="server" ID="hfProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="ProductSKU" HeaderText="SKU" ItemStyle-Width="200px"></asp:BoundField>
                                                        <asp:TemplateField HeaderText="Qty" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:TextBox ID="txtQty" runat="server" Text='' OnTextChanged="txtQtyInstadose3_OnTextChanged"
                                                                    TabIndex="0" CssClass="CenterTextbox" AutoPostBack="true" MaxLength="4" />
                                                            </ItemTemplate>
                                                            <ControlStyle Width="50px" />
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="50px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Assigned Wearer" ItemStyle-HorizontalAlign="Center">
                                                            <ItemTemplate>
                                                                <asp:LinkButton ID="btnAssignedWearerByProduct" runat="server"  
                                                                    CommandName="AssignedWearerByProduct" CommandArgument='<%# Eval("ProductID")%>' 
                                                                    onclick="btnAssignedWearerByProduct_Click" ></asp:LinkButton>                                                                

                                                                <asp:ImageButton ID="imgAssignedWearerByProduct" runat="server" ImageUrl="~/images/icons/user_add.png" Width="16px" Height="16px" 
                                                                    style="position:relative;top:4px"
                                                                    AlternateText="" ToolTip="Assign wearer to order"
                                                                    onClick="imgAssignedWearerByProduct_Click"
                                                                    CommandName="AssignedWearerByProduct" CommandArgument='<%# Eval("ProductID") %>' />
                                                            </ItemTemplate>
                                                            
                                                            <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center" />
                                                            <ItemStyle CssClass="CenterAlignItemText" Width="75px" Wrap="False" HorizontalAlign="Center" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Price ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblPrice" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle CssClass="RightAlignItemText" Width="100px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle CssClass="RightAlignItemText" HorizontalAlign="Right" Wrap="False" Width="75px" />
                                                        </asp:TemplateField>

                                                        <asp:TemplateField HeaderText="Total ">
                                                            <ItemTemplate>
                                                                <div class="LabelValue;RightAlignItemText">
                                                                    <asp:Label runat="server" ID="lblExtended" />
                                                                </div>
                                                            </ItemTemplate>
                                                            <ControlStyle Width="150px" />
                                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                                            <ItemStyle HorizontalAlign="Right" Wrap="False" Width="150px" />
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <HeaderStyle CssClass="Alt" Font-Bold="True" ForeColor="Black" />
                                                </asp:GridView>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div> 
                                <%--end of id 3 tab--%>

                            </div>
                        </div>  <%--end product tab--%>                        
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <br />            
            <div id="serialNoSection" visible="false" runat="server">
                <fieldset style="border-color:orange; border-width:thin;">
                    <legend>Select Lost/Broken SerialNo:</legend>
                    <div>
                        <asp:CheckBoxList ID="ckblSerialNumber" 
                            CellPadding="1"
                            CellSpacing="0"
                            RepeatColumns="10"
                            RepeatDirection="Horizontal"
                            RepeatLayout="Table"
                            TextAlign="Right"
                            ToolTip ="Check Serial#"
                            runat="server"  ForeColor="Black" 
                                >
                        </asp:CheckBoxList>
                    </div>
                </fieldset>
                <%--<div><span style='font-style:italic; font-weight:600;'>Select Lost/Broken SerialNo</span>:</div>--%>                                                    
            </div>
        </div> 


        <h3><a href="#">Payment Method</a></h3>
        <div class="OForm AccordionPadding">
            <asp:UpdatePanel ID="upnlPaymentMethod" runat="server">
                <ContentTemplate>
                    <%--<div class="Row">
                        <div class="Label">
                            Billing Method:
                        </div>
                        <div class="Control">
                            <asp:RadioButtonList ID="rblstPayMethod" runat="server" RepeatDirection="Horizontal"
                                 AutoPostBack="True" OnSelectedIndexChanged="rblstPayMethod_SelectedIndexChanged">
                                <asp:ListItem Value="Credit Card" Enabled="true">Credit Card</asp:ListItem>
                                <asp:ListItem Value="Purchase Order" Enabled="true">Purchase Order</asp:ListItem>
                            </asp:RadioButtonList>
                            <asp:Label ID="lblUpdateCCInformation" runat="server" Font-Bold="true" ForeColor="Red" Visible="false"></asp:Label>
                        </div>
                        <div class="Clear">
                        </div>
                    </div>--%>
                    <%--<div class="Row" runat="server" id="divPaymentPO" visible="false">--%>
                    <div class="Row" runat="server" id="divPaymentPO">
                        <div class="Label">
                            PO Number<span class="Required">*</span>:
                        </div>
                        <div class="Control">
                            <asp:TextBox ID="txtPOno" runat="server" Text="" MaxLength="15"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidatorPONumber" runat="server" ControlToValidate="txtPOno"
                                ValidationGroup="CreateOrder" Display="Dynamic" Text="A PO number is required."
                                ErrorMessage="PO Number" />
                            <asp:RegularExpressionValidator ID="RegularExpressionValidator1" ControlToValidate="txtPOno"
                                ValidationExpression="[0-9a-zA-Z?\s~!\-@#$%^&amp;*/]{1,15}" Display="Dynamic"
                                Text="PO Number is max 15 characters or numerics" runat="server" />
                        </div>
                        <div class="Clear">
                        </div>
                    </div>
                    <%--<div runat="server" id="divPaymentCC" visible="false">
                        <div class="Row">
                            <div class="Label">
                                Card Type<span class="Required">*</span>:
                            </div>
                            <div class="Control">
                                <asp:RadioButtonList ID="rbtCardType" runat="server" RepeatColumns="4">
                                    <asp:ListItem Value="1" Text="Visa" Selected="True"><img src='../images/ccvisa.gif' alt='Visa Card' width='30'></asp:ListItem>
                                    <asp:ListItem Value="2" Text="MasterCard"><img src='../images/ccmastercard.gif' alt='MasterCard' width='30'></asp:ListItem>
                                    <asp:ListItem Value="3" Text="Discover"><img src='../images/ccdiscover.gif' alt='Discover' width='30'></asp:ListItem>
                                    <asp:ListItem Value="4" Text="Amex"><img src='../images/ccamex.gif' alt='American Express' width='30'></asp:ListItem>
                                </asp:RadioButtonList>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row">
                            <div class="Label">
                                Credit Card Number<span class="Required">*</span>:
                            </div>
                            <div class="Control">
                                <asp:TextBox ID="txtCCno" runat="server" CssClass="Size Medium2"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator29" runat="server" ControlToValidate="txtCCno"
                                ValidationGroup="CreateOrder" Display="Dynamic" Text="The credit card number is required."
                                ErrorMessage="Credit Card Number"></asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Row">
                            <div class="Label">
                                Name On Card<span class="Required">*</span>:
                            </div>
                            <div class="Control">
                                <asp:TextBox ID="txtCCName" runat="server" CssClass="Size Medium"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator30" runat="server" ControlToValidate="txtCCName"
                                    ValidationGroup="CreateOrder" Display="Dynamic" Text="The name on the card is required."
                                    ErrorMessage="Name on Credit Card"></asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Row">
                            <div class="Label">
                                Expiration Month/Year<span class="Required">*</span>:
                            </div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlCCMonth" runat="server">
                                    <asp:ListItem Value="0" Text="Month" />
                                    <asp:ListItem Value="1" Text="Jan" />
                                    <asp:ListItem Value="2" Text="Feb" />
                                    <asp:ListItem Value="3" Text="Mar" />
                                    <asp:ListItem Value="4" Text="Apr" />
                                    <asp:ListItem Value="5" Text="May" />
                                    <asp:ListItem Value="6" Text="Jun" />
                                    <asp:ListItem Value="7" Text="Jul" />
                                    <asp:ListItem Value="8" Text="Aug" />
                                    <asp:ListItem Value="9" Text="Sep" />
                                    <asp:ListItem Value="10" Text="Oct" />
                                    <asp:ListItem Value="11" Text="Nov" />
                                    <asp:ListItem Value="12" Text="Dec" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator31" ControlToValidate="ddlCCMonth"
                                    Text="Month is required." Display="Dynamic" InitialValue="0" ValidationGroup="CreateOrder"
                                    ErrorMessage="Credit Card Expiration Month"></asp:RequiredFieldValidator>
                                <asp:DropDownList ID="ddlCCYear" runat="server">
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator32" ControlToValidate="ddlCCYear"
                                    Text="Year is required." Display="Dynamic" InitialValue="0" ValidationGroup="CreateOrder"
                                    ErrorMessage="Credit Card Expiration Year"></asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Row">
                            <div class="Label">
                                Security Code (CVC)<span class="Required">*</span>:
                            </div>
                            <div class="Control">
                                <asp:TextBox CssClass="Size XXSmall" ID="txtCCcvc" runat="server"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator28" runat="server" ControlToValidate="txtCCcvc"
                                    ValidationGroup="CreateOrder" Display="Dynamic" Text="Card security code is required."
                                    ErrorMessage="Credit Card Security Code"></asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                    </div>--%>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <h3><a href="#">Shipping & Order Total</a></h3>
        <div>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="OForm">
                        <div class="Row">
                            <div class="Label">
                                Package Type:
                            </div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlPackageType" runat="server"
                                    AutoPostBack="true"
                                    DataValueField="PackageTypeID"
                                    DataTextField="PackageDesc"
                                    OnSelectedIndexChanged="ddlPackageType_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Row">
                            <div class="Label">
                                Shipping Option:
                            </div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlShippingOption" runat="server"
                                    AutoPostBack="true"
                                    DataValueField="ShippingOptionID"
                                    DataTextField="ShippingOptionDesc"
                                    OnSelectedIndexChanged="ddlShippingOption_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        
                        <div class="Row" id="divFedex" runat="server">
                            <div class="Label">
                                Shipping Method<span class="Required">*</span>:
                            </div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlShippingMethod" runat="server" AutoPostBack="true" DataValueField="ShippingMethodID"
                                    DataTextField="ShippingMethodDesc" OnSelectedIndexChanged="ddlShippingMethod_SelectedIndexChanged" />
                                <asp:RequiredFieldValidator runat="server" ID="rfvShippingMethod" ControlToValidate="ddlShippingMethod"
                                    Display="Dynamic" ErrorMessage="Shipping Method" InitialValue="0" ValidationGroup="CreateOrder"></asp:RequiredFieldValidator>
                                <asp:Label Text="" CssClass="InlineError" Style="display: inline;" ID="lblShippingChargeError" runat="server" />
                            </div>
                            <div class="Clear">
                            </div>
                        </div>

                        <div class="Row" id="divMalvernCarrier" runat="server">
                            <div class="Label">
                                Shipping Carrier:
                            </div>
                            <div class="Control">
                                <asp:DropDownList ID="ddShippingCarrier" runat="server" AutoPostBack="false"
                                    DataValueField="ShippingCarrier" DataTextField="ShippingCarrier" />                               
                            </div>
                            <div class="Clear">
                            </div>
                        </div>

                        <div class="Row" id="divMalvernShipMethod" runat="server">
                            <div class="Label">
                                Shipping Method<span class="Required">*</span>:
                            </div>
                            <div class="Control">                                                                
                                <asp:DropDownList ID="ddlShippingMethodMalvern" runat="server" AutoPostBack="true" DataValueField="ShippingMethodID"
                                    DataTextField="ShippingMethodDesc" OnSelectedIndexChanged="ddlShippingMethod_SelectedIndexChanged" />
                                <asp:RequiredFieldValidator runat="server" ID="rfMalvernShippingMethod" ControlToValidate="ddlShippingMethodMalvern"
                                    Display="Dynamic" ErrorMessage="Shipping Method" InitialValue="0" ValidationGroup="CreateOrder"></asp:RequiredFieldValidator>
                                <asp:Label Text="" CssClass="InlineError" Style="display: inline;" ID="lblMalvernShippingChargeError" runat="server" />
                            </div>
                            <div class="Clear">
                            </div>
                        </div>

                        <%-- Comment by Tdo, 08/13/2014. Controls are not inline
                        <div class="Row" id="divMalvern" runat="server">
                           <div class="Control">
                            <table style="width:100%" >
                                <tr >
                                    <td style="text-align:right; width:150px">
                                   <asp:Label ID="lblShippingCarrier" runat="server" Text="Shipping Carrier:" Font-Bold="true" ></asp:Label>
                                    </td>
                                    <td>
                                    <asp:DropDownList ID="ddShippingCarrier" runat="server" 
                                      AutoPostBack="false"
                                    DataValueField="ShippingCarrier"
                                    DataTextField="ShippingCarrier"></asp:DropDownList>
                                    </td>
                                    <td> 
                                        <asp:Label ID="Label1" runat="server" Text="Shipping Method:" Font-Bold="true" ></asp:Label><span class="Required">*</span>
                                        <asp:DropDownList ID="ddlShippingMethodMalvern" runat="server" AutoPostBack="true" DataValueField="ShippingMethodID"
                                            DataTextField="ShippingMethodDesc" OnSelectedIndexChanged="ddlShippingMethod_SelectedIndexChanged" />
                                        <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator1" ControlToValidate="ddlShippingMethodMalvern"
                                            Display="Dynamic" ErrorMessage="Shipping Method" InitialValue="0" ValidationGroup="CreateOrder"></asp:RequiredFieldValidator>
                                        <asp:Label Text="" CssClass="InlineError" Style="display: inline;" ID="Label2"
                                            runat="server" />
                                    </td>
                                </tr>
                             </table>                            
                            </div>
                            <div class="Clear">
                            </div>
                         </div>--%>
                        
                        <div class="Row">
                            <div class="Label">
                                Special Instructions:
                            </div>
                            <div class="Control">
                                <asp:TextBox ID="txtSpecialInstruction"
                                    AutoPostBack="true"
                                    TextMode="MultiLine" Height="50" MaxLength="200"
                                    runat="server" OnTextChanged="txtSpecialInstruction_TextChanged"></asp:TextBox>
                                <asp:RegularExpressionValidator ID="RegularExpressionSpecialInstructionValidator"
                                    ControlToValidate="txtSpecialInstruction" ValidationExpression="^.{0,200}$" ValidationGroup="CreateOrder"
                                    Display="Dynamic" ErrorMessage="Special Instructions is max 200 characters."
                                    Text="Special Instructions is max 200 characters" runat="server" />
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Row">
                            <div class="Label">
                                Subtotal:
                            </div>
                            <div class="Control">
                                <div class="LabelValue">
                                    <asp:Label ID="lblSubTotal" runat="server" Text="0.00 USD"></asp:Label>
                                </div>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Row" style="display: none;">
                            <div class="Label">
                                Coupon:
                            </div>
                            <div class="Control">
                                <div class="LabelValue">
                                    <asp:Label ID="lblCouponDiscountAmount" runat="server" Text="0.00 USD"></asp:Label>
                                </div>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Row">
                            <div class="Label">
                                Shipping:
                            </div>
                            <div class="Control">
                                <div class="LabelValue">
                                    <asp:Label ID="lblShippingCharge" runat="server" Text="0.00 USD"></asp:Label>
                                </div>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                        <div class="Row">
                            <div class="Label">
                                Total:
                            </div>
                            <div class="Control">
                                <div class="LabelValue">
                                    <asp:Label ID="lblOrderTotal" runat="server" Text="0.00 USD"></asp:Label>
                                </div>
                            </div>
                            <div class="Clear">
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <h3><a href="#">Order Summary</a></h3>
        <div>
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <table class="OTable">
                        <tr>
                            <td class="Label" style="width: 250px">Order #:
                            </td>
                            <td style="width: 180px">
                                <asp:Label ID="lblOrderID" Font-Bold="true" runat="server" Text="N/A" />
                            </td>
                            <td class="Label">Account #:
                            </td>
                            <td style="width: 180px">
                                <asp:Label ID="lblAccountID" Font-Bold="true" runat="server" Text="" />

                            </td>
                        </tr>
                        <tr>
                            <td class="Label">Company:
                            </td>
                            <td>
                                <asp:Label ID="lblOrderCompany" runat="server" Text="" />
                            </td>
                            <td class="Label">Account Name:
                            </td>
                            <td>
                                <asp:Label ID="lblAccountName" runat="server" Text='<%# Bind("AccountName") %>' />
                            </td>

                        </tr>
                        <tr>
                            <td style="font-weight: bold;" class="Label">Billing Address:
                            </td>
                            <td></td>
                            <td class="Label" style="font-weight: bold; width: 300px;">Shipping Address:
                            </td>
                            <td></td>
                        </tr>
                        <tr>
                            <td class="Label"></td>

                            <td>
                                <div id="divBillingCompany" runat="server">
                                    <asp:Label ID="lblBillingCompany" runat="server"></asp:Label>
                                </div>
                                <div id="divNoBillingCompany" runat="server" visible="false"></div>
                                <div>
                                    <asp:Label ID="lblBillingName" runat="server" Text="" />
                                </div>
                                <div>
                                    <asp:Label ID="lblBillingAddress1" runat="server" Text="" />
                                </div>
                                <div>
                                    <asp:Label ID="lblBillingAddress2" runat="server" Text="" />
                                </div>
                                <div>
                                    <asp:Label ID="lblBillingAddress3" runat="server" Text="" />
                                </div>
                                <div>
                                    <asp:Label ID="lblBillingCity" runat="server" Text="" />,
                                    <asp:Label ID="lblBillingState" runat="server" Text="" />
                                    <asp:Label ID="lblBillingPostalCode" runat="server" Text="" />
                                </div>
                            </td>
                            <td class="Label"></td>
                            <td>
                                <div>
                                    <asp:Label ID="lblShippingCompany" runat="server"></asp:Label>
                                </div>
                                <div>
                                    <asp:Label ID="lblShippingName" runat="server" Text="" />
                                </div>
                                <div>
                                    <asp:Label ID="lblShippingAddress1" runat="server" Text="" />
                                </div>
                                <div>
                                    <asp:Label ID="lblShippingAddress2" runat="server" Text="" />
                                </div>
                                <div>
                                    <asp:Label ID="lblShippingAddress3" runat="server" Text="" />
                                </div>
                                <div>
                                    <asp:Label ID="lblShippingCity" runat="server" Text="" />,
                                    <asp:Label ID="lblShippingState" runat="server" />
                                    <asp:Label ID="lblShippingPostalCode" runat="server" Text="" />
                                </div>
 
                            </td>
                        </tr>

                        <tr>
                            <td class="Label">Billing Day Prior:</td>
                            <td>
                                <asp:Label ID="lblBillingDayPrior" runat="server" Text="" />
                            </td>

                            <td class="Label">Biling Term:
                            </td>
                            <td>
                                <asp:Label ID="lblOrderBillingTerm" runat="server" Text="" />
                            </td>
                        </tr>

                        <tr>
                            <td class="Label">Rate Code:
                            </td>
                            <td>
                                <asp:Label ID="lblRateCode" runat="server" Text="" />
                            </td>
                            <td class="Label">Service Period:
                            </td>
                            <td>
                                <asp:Label ID="lblOrderServicePeriod" runat="server" Width="383px" />
                            </td>
                        </tr>
                        <tr>
                            <td class="Label">Referral Code:
                            </td>
                            <td>
                                <asp:Label ID="lblReferralCode" runat="server"
                                    Text='<%# Bind("ReferralCode") %>' />
                            </td>
                            <td class="Label">Prorate Period:
                            </td>
                            <td>
                                <asp:Label ID="lblOrderProratePeriod" runat="server" Width="370px" />
                            </td>
                        </tr>
                        <tr>
                            <td class="Label">Payment Method:</td>
                            <td>
                                <asp:Label ID="lblPaymentMethod" runat="server" Text="" />
                            </td>
                            <td class="Label">Card Type:
                            </td>
                            <td>
                                <asp:Label ID="lblCreditCard" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td class="Label">Created Date:</td>
                            <td>
                                <asp:Label ID="lblCreatedDate" runat="server" Text="" />
                            </td>
                            <td class="Label" style="font-weight: bold; text-align: right;">Shipping Method:
                            </td>
                            <td>
                                <asp:Label ID="lblShippingMethod" runat="server" Text="" />
                            </td>
                        </tr>
                        <tr>
                            <td class="Label">Package Type:</td>
                            <td>
                                <asp:Label ID="lblPackageType" runat="server" Width="250px" />
                            </td>
                            <td class="Label" style="font-weight: bold; text-align: right;">SubTotal:
                            </td>
                            <td>
                                <asp:Label ID="lblOrderSubtotal" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td class="Label">Shipping Option:</td>
                            <td>
                                <asp:Label ID="lblShippingOption" runat="server" Width="266px" />
                            </td>
                            <td class="Label">Shipping:
                            </td>
                            <td>
                                <asp:Label ID="lblOrderShipping" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td class="Label">Special Instructions:</td>
                            <td>
                                <asp:Label ID="lblSpecialInstructions" runat="server" Width="250px" />
                            </td>
                            <td class="Label" style="font-weight: bold; text-align: right;">Total:
                            </td>
                            <td>
                                <asp:Label ID="lblOrderSummaryTotal" runat="server" />
                            </td>
                        </tr>
                                                    
                    </table>

                    <div class="OToolbar JoinTable">
                        <ul>
                            <li>
                                <asp:Label ID="lblProdOrdered" runat="server"></asp:Label>
                            </li>
                        </ul>
                        <div style="float: right; padding-right: 15px;">
                            <asp:Label ID="lblAccessTotal" runat="server" Text="0" Style="padding: 2px;" />
                        </div>

                        <div style="float: right; padding-right: 15px;">
                            <asp:Label ID="lblInstaPlusTotal" runat="server" Text="0" Style="padding: 2px;" />
                        </div>
                        <div style="float: right; padding-right: 15px;">
                            <asp:Label ID="lblInstaTotal" runat="server" Text="0" Style="padding: 2px;" /> 
                        </div>
                        <div style="float: right; padding-right: 15px;">
                            <asp:Label ID="lblInsta2Total" runat="server" Text="0" Style="padding: 2px;" /> 
                        </div>
                    </div>
                    <asp:GridView ID="gvOrderSummary" runat="server" AutoGenerateColumns="False" CssClass="OTable"
                        OnSelectedIndexChanged="gvOrderDetails_SelectedIndexChanged" Visible="true"
                        EmptyDataText="No products have been selected for this order" Width="100%">
                        <Columns>
                            <asp:BoundField DataField="SKU" HeaderText="SKU" ItemStyle-Width="175px" SortExpression="SKU"></asp:BoundField>
                            <asp:BoundField DataField="ProductName" HeaderText="Product" ItemStyle-Width="175px"
                                SortExpression="ProductName"></asp:BoundField>
                            <asp:BoundField DataField="Color" HeaderText="Color" SortExpression="Color" ItemStyle-Width="175px">
                                <HeaderStyle></HeaderStyle>
                                <ItemStyle></ItemStyle>
                            </asp:BoundField>
                            <asp:BoundField DataField="Price" SortExpression="Price" 
                                ItemStyle-Width="175px" HeaderStyle-CssClass="ralign" HeaderText="Unit Price">
                                <HeaderStyle HorizontalAlign="Right" CssClass="RightAlignHeaderText"></HeaderStyle>
                                <ItemStyle HorizontalAlign="Right"></ItemStyle>
                            </asp:BoundField>
                            <asp:BoundField ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-HorizontalAlign="Center"
                                DataField="Quantity" HeaderText="Qty" SortExpression="Quantity" ItemStyle-Width="100px">
                                <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Center"></HeaderStyle>
                                <ItemStyle CssClass="mt-itm RightAlign"></ItemStyle>
                            </asp:BoundField>
                            <asp:BoundField DataField="SubTotal" HeaderText="Subtotal" SortExpression="SubTotal">
                                <HeaderStyle CssClass="RightAlignHeaderText"></HeaderStyle>
                                <ItemStyle CssClass="mt-itm RightAlign" Width="175px" HorizontalAlign="Right"></ItemStyle>
                            </asp:BoundField>
                        </Columns>
                        <AlternatingRowStyle CssClass="Alt" />
                        <HeaderStyle HorizontalAlign="Right" />
                        <PagerStyle CssClass="Footer" />
                        <RowStyle></RowStyle>
                    </asp:GridView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <asp:UpdatePanel runat="server" ID="upnlCreateOrderButton" >
        <%--<Triggers>
            <asp:AsyncPostBackTrigger controlid="btnChangeLocation" eventname="Click" />
        </Triggers>--%>

        <ContentTemplate>
            <div class="Buttons">
                <div class="RequiredIndicator">
                    <span class="Required">*</span> Indicates a required field.
                </div>
                <div class="ButtonHolder">
                    <asp:Button ID="btnCreate" runat="server" CausesValidation="true" Text="Create Order" OnClick="btnCreate_Click"
                        ValidationGroup="CreateOrder" CssClass="OButton" />
                    <asp:Button ID="btnCancel" runat="server" Text="Back to Account" OnClick="btnCancel_Click"
                        CssClass="OButton" OnClientClick="return confirm('Are you sure you want to cancel this order?');" />
                </div>
                <div class="Clear">
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    

    
</asp:Content>
