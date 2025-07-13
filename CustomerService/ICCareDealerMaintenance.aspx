<%@ Page Title="IC Care Dealers" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_CustomerService_ICCareDealerMaintenance" Codebehind="ICCareDealerMaintenance.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="Server">
    <style type="text/css">
        .noRecordFound {
            font-size: 14px;
            font-weight: bold;
            color: #FF0000;
            text-align: center;
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
            // Maintains Accordion (Index) after PostBack.
            var activeIndex = parseInt($('#<%= hdnfldAccordionIndex.ClientID %>').val());
            
            var reportFormatType = $("#<%= ddlReportFormatType.ClientID %>");
            var poDeliveryMethod = $("#<%= ddlPODeliveryMethod.ClientID %>");
            poDeliveryMethod.on("change", function () {
                if ($(this).val() == 0)
                    reportFormatType.val("P");
                else
                    reportFormatType.val("S");
            });
            //if onload it is not correct update it
            if (poDeliveryMethod.val() == 0)
                reportFormatType.val("P");
            else
                reportFormatType.val("S");
            // Accordion
            $("#accordion").accordion({
                header: "h3",
                autoHeight: false,
                active: activeIndex,
                change: function (event, ui) {
                    var index = $(this).accordion("option", "active");
                    $('#<%= hdnfldAccordionIndex.ClientID %>').val(index);
                }
            });

            $(function () {
                $('#<%= txtDealerName.ClientID %>').change(function () {
                    $('#<%= txtDealerNative.ClientID %>').val($(this).val());
                });
            })

            $('#dealerDialog').dialog({
                autoOpen: false,
                width: 700,
                resizable: false,
                modal: true,
                title: "Manage IC Care Dealers",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('#<%= txtDealerName.ClientID %>').focus();
                },
                buttons: {
                    "Save": function () {
                        $('#<%= btnAddDealer.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('.ui-overlay').fadeOut();
                }
            });

            $('#dealerNotFoundDialog').dialog({
                autoOpen: false,
                width: 500,
                height: 125,
                resizable: false,
                modal: true,
                title: "Alert Message",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                },
                buttons: {
                    "Close": function () {
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
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div id="dealerNotFoundDialog">
        <asp:UpdatePanel ID="updtpnlDealerNotFound" runat="server">
            <ContentTemplate>
                <div class="noRecordFound">
                    Dealer record could not be found in UNIX.
                </div>
                <asp:Button ID="btnClose" runat="server" text="Close" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div id="dealerDialog">
        <asp:UpdatePanel ID="updtpnlDealerDialog" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldDealerID" runat="server" Value="0" />
                <asp:HiddenField ID="hdnfldAccordionIndex" runat="server" Value="0" />
                <%--08/14/2014 (ANANDI) - DEFAULTS ADDED PER CUSTOMER SERVICE REQUEST.--%>
                <asp:HiddenField ID="hdnfldTypeDefault" runat="server" Value="None Available" />
                <asp:HiddenField ID="hdnfldBrandSourceIDDefault" runat="server" Value="3" />
                <asp:HiddenField ID="hdnfldDealerGroupIDDefault" runat="server" Value="1" />
                <asp:HiddenField ID="hdnfldUNIXRateCodeDefault" runat="server" Value="500003" />
                <%--END--%>
                <div class="FormError" id="dealerDialogError" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="dealerDialogErrorMsg" runat="server" >An error was encountered.</span></p>
				</div>
                <div id="accordion" style="margin-top: 10px;">
		            <div id="divBasicDetails">
                        <%--BASIC DETAILS--%>
			            <h3><a href="#">Basic Details</a></h3>
                        <div class="OForm AccordionPadding">
                            <div class="Row">
                                <div class="Label Small">Dealer #<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtDealerID" runat="server" CssClass="Size Small" TabIndex="1" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Small">Dealer Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtDealerName" runat="server" MaxLength="50" CssClass="Size Large" TabIndex="2" />                                    
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Small">Dealer Native<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtDealerNative" runat="server" MaxLength="50" CssClass="Size Large" TabIndex="3" />                                    
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Small">Type:</div>
                                <div class="Control">
                                    <asp:Label ID="lblType" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Small">Brand:</div>
                                <div class="Control">
                                    <asp:Label ID="lblBrand" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Small">Group:</div>
                                <div class="Control">
                                    <asp:Label ID="lblGroup" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>          
                            <div class="Row">
                                <div class="Label Small">Open AR:</div>
                                <div class="Control">
                                    <asp:Label ID="lblOpenAR" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>          
                            <div class="Row">
                                <div class="Label Small">Active:</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkBoxActive" runat="server" Text="" TabIndex="4"  />
                                </div>
                                <div class="Clear"></div>
                            </div>                                                                                                                                                                                       
                        </div>
                    </div>
                    <%--END--%>
                    <%--CONTACT INFORMATION--%>
                    <div id="divContactInformation">
			            <h3><a href="#">Contact Information</a></h3>
                        <div class="OForm AccordionPadding">           
                            <div class="Row">
                                <div class="Label Small">Contact Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtFirstName" runat="server" MaxLength="40" CssClass="Size Medium2" TabIndex="5" placeholder="John" />
                                    <asp:TextBox ID="txtLastName" runat="server" MaxLength="40" CssClass="Size Medium2" TabIndex="6" placeholder="Smith" />                                            
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Small">Attention Name:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtAttentionName" runat="server" MaxLength="30" CssClass="Size Large2" TabIndex="7" />                                           
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Small">Phone<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtTelephone" runat="server" CssClass="Size Medium2" MaxLength="24" placeholder="(888)555-1212" TabIndex="8" />                                            
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label Small">Fax:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtFax" runat="server" CssClass="Size Medium2" MaxLength="24" placeholder="(888)555-1212" TabIndex="9" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Small">E-Mail 1<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="Size Large2" MaxLength="60" placeholder="jsmith@companyname.com" TabIndex="10" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Small">E-Mail 2:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtEmail2" runat="server" CssClass="Size Large2" MaxLength="100" placeholder="jsmith@companyname.com" TabIndex="11" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Small">Country<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlCountryID" runat="server" DataValueField="CountryID" 
                                    DataTextField="CountryName" DataSourceID="SQLCountries" AutoPostBack="True"
                                    OnSelectedIndexChanged="ddlCountryID_SelectedIndexChanged" TabIndex="12">
                                    </asp:DropDownList>                                                                    
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <%--DOMESTIC ADDRESS--%>
                            <div id="divDomesticAddress" runat="server">
                                <div class="Row">
                                    <div class="Label Small">Address<span class="Required">*</span>:</div>
                                    <div class="Control">
                                        <asp:TextBox ID="txtAddressLine1_Domestic" runat="server" CssClass="Size Large" MaxLength="255" TabIndex="13" />                                            
                                    </div>   
                                    <div class="Clear"></div>                
                                </div>
                                <div class="Row">
                                    <div class="Label Small">&nbsp;</div>
                                    <div class="Control">
                                        <asp:TextBox ID="txtAddressLine2_Domestic" runat="server" CssClass="Size Large" MaxLength="255" TabIndex="14" />                                            
                                    </div>   
                                    <div class="Clear"></div>                
                                </div>
                                <div class="Row">
                                    <div class="Label Small">City<span class="Required">*</span>:</div>
                                    <div class="Control">
                                        <asp:TextBox ID="txtCity_Domestic" runat="server" CssClass="Size Large" MaxLength="255" TabIndex="15" />                                            
                                    </div>   
                                    <div class="Clear"></div>                
                                </div>
                                <div class="Row">
                                    <div class="Label Small">State<span class="Required">*</span>:</div>
                                    <div class="Control">
                                        <asp:DropDownList ID="ddlStateID_Domestic" runat="server" DataSourceID="SQLGetStatesByCountry_Domestic" DataValueField="StateID" DataTextField="StateAbbrev" TabIndex="16"> 
                                        </asp:DropDownList>                                           
                                    </div>
                                    <div class="Clear"></div>
                                </div>
                                <div class="Row">
                                    <div class="Label Small">Postal Code<span class="Required">*</span>:</div>
                                    <div class="Control">
                                        <asp:TextBox ID="txtPostalCode_Domestic" runat="server" CssClass="Size Medium2" MaxLength="15" TabIndex="17" />                                            
                                    </div>
                                    <div class="Clear"></div>
                                </div> 
                            </div>
                            <%--END--%>
                            <%--FOREIGN ADDRESS--%>
                            <div id="divForeignAddress" runat="server" visible="false">
                                <div class="Row">
                                    <div class="Label Small">Address<span class="Required">*</span>:</div>
                                    <div class="Control">
                                        <asp:TextBox ID="txtAddressLine1_Foreign" runat="server" CssClass="Size Large" MaxLength="255" TabIndex="18" />                                            
                                    </div>   
                                    <div class="Clear"></div>                
                                </div>
                                <div class="Row">
                                    <div class="Label Small">&nbsp;</div>
                                    <div class="Control">
                                        <asp:TextBox ID="txtAddressLine2_Foreign" runat="server" CssClass="Size Large" MaxLength="255" TabIndex="19" />                                            
                                    </div>   
                                    <div class="Clear"></div>                
                                </div>
                                <div class="Row">
                                    <div class="Label Small">&nbsp;</div>
                                    <div class="Control">
                                        <asp:TextBox ID="txtCity_Foreign" runat="server" CssClass="Size Large" MaxLength="255" TabIndex="20" />                                            
                                    </div>   
                                    <div class="Clear"></div>                
                                </div>
                                <div class="Row">
                                    <div class="Label Small">&nbsp;</div>
                                    <div class="Control">
                                        <asp:TextBox ID="txtAddressLine4_Foreign" runat="server" CssClass="Size Large" MaxLength="255" TabIndex="21" />                                            
                                    </div>   
                                    <div class="Clear"></div>                
                                </div>
                                <div class="Row">
                                    <div class="Label Small">Country Code<span class="Required">*</span>:</div>
                                    <div class="Control">
                                        <asp:DropDownList ID="ddlStateID_Foreign" runat="server" DataSourceID="SQLGetStatesByCountry_Foreign" DataValueField="StateID" DataTextField="StateAbbrev" TabIndex="22"> 
                                        </asp:DropDownList>                                           
                                    </div>
                                    <div class="Clear"></div>
                                </div>
                                <div class="Row">
                                    <div class="Label Small">Postal Code<span class="Required">*</span>:</div>
                                    <div class="Control">
                                        <asp:TextBox ID="txtPostalCode_Foreign" runat="server" CssClass="Size Medium2" MaxLength="15" TabIndex="23" />                                            
                                    </div>
                                    <div class="Clear"></div>
                                </div> 
                            </div>
                            <%--END--%>                
                        </div>
                    </div>
                    <%--END--%>
                    <%--ADDITIONAL INFORMATION--%>
                    <div id="divAdditionalInformation">
			            <h3><a href="#">Additional Information</a></h3>
                        <div class="OForm AccordionPadding">
                            <div class="Row">
                                <div class="Label Large">Client ID:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlClientID" runat="server" DataSourceID="SQLEDIClients" DataValueField="ClientID" 
                                    DataTextField="ClientName" AppendDataBoundItems="true" AutoPostBack="true" TabIndex="24" OnSelectedIndexChanged="ddlClientID_SelectedIndexChanged">
                                        <asp:ListItem Text="-Select-" Value="0"></asp:ListItem>
                                    </asp:DropDownList> 
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Large">EDI Invoice Required:</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxEDIInvoiceRequired" runat="server" OnCheckedChanged="chkbxEDIInvoiceRequired_CheckedChanged" AutoPostBack="true" TabIndex="25" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Large">Dealer Notes:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtDealerNotes" runat="server" TextMode="MultiLine" CssClass="Size Large" Height="75px" MaxLength="60" TabIndex="26"></asp:TextBox>
                                    <br />
                                    <asp:RegularExpressionValidator ID="regexpvalDealerNotes" runat="server" ControlToValidate="txtDealerNotes" SetFocusOnError="true" Display="Dynamic"  
                                    ValidationExpression="^[\s\S]{0,60}$" ErrorMessage="Maximum of 60 characters has been exceeded." ValidationGroup="SUBMIT"></asp:RegularExpressionValidator>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Large">Bill for Shipping:</div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkbxBillForShipping" runat="server" TabIndex="27" Checked="true" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Large">Renewal Type:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlRenewalType" runat="server" AutoPostBack="false" TabIndex="28">
                                        <asp:ListItem Text="Send PO" Value="S" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="Auto Renew" Value="A"></asp:ListItem>
                                        <asp:ListItem Text="Never" Value="N"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Large">POR Delivery Method:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlPODeliveryMethod" runat="server" AutoPostBack="false" TabIndex="29">
                                        <asp:ListItem Text="Fax" Value="0" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="Email 1" Value="1"></asp:ListItem>
                                        <asp:ListItem Text="Email 2" Value="2"></asp:ListItem>
                                        <asp:ListItem Text="Email 1 + various" Value="3"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Large">UNIX Rate Code:</div>
                                <div class="Control">
                                    <asp:Label ID="lblUNIXRateCode" runat="server" CssClass="LabelValue"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label Large">Rate Field Type:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlRateFieldType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlRateFieldType_SelectedIndexChanged" TabIndex="30">
                                        <asp:ListItem Text="Discount" Value="2"></asp:ListItem>
                                        <asp:ListItem Text="Custom" Value="3" Selected="True"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div id="divDiscountPercentage" class="Control" runat="server" visible="false">
                                    <div class="Label XXSmall">Discount %:</div>
                                    <div class="Control">
                                        <asp:TextBox ID="txtDiscountField" runat="server" CssClass="Size XXSmall" Text="" placeholder="0" MaxLength="2" TabIndex="31"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row" style="display:none;">
                                <div class="Label Large">POR Report Format:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlReportFormatType" runat="server" AutoPostBack="true" TabIndex="32">
                                        <asp:ListItem Text="Print" Value="P" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="SpreadSheet" Value="S"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>
                        </div>
                    </div>
                    <%--END--%>
                </div>         
                <asp:Button ID="btnAddDealer" runat="server" OnClick="btnAddDealer_Click" text="Save" style="display: none;" ValidationGroup="SUBMIT" />
                <asp:Button ID="btnCancelDealer" runat="server" text="Cancel" style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <%--MAIN PAGE CONTENT--%>
    <div id="divMainPageContent" style="padding: 10px;">
        <asp:Panel ID="pnlMainPageContent" runat="server" DefaultButton="btnFind">
            <div style="width: 100%;">
                <asp:UpdatePanel ID="updtpnlICCareDealerMaintenance" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div id="DealerSearchToolBar" runat ="server" class="OToolbar JoinTable">
					        <ul>
                                <li>           
                                    <asp:LinkButton ID="lnkbtnNewDealer" runat="server"  
                                    OnClick="lnkbtnNewDealer_Click" 
                                    CssClass="Icon Add">Create Dealer</asp:LinkButton>
                                </li>
                                <li>           
                                    <asp:LinkButton ID="lnkbtnUploadDealerDocuments" runat="server"  
                                    CommandName="UploadDealerDocuments" CommandArgument="" 
                                    CssClass="Icon Add" OnClick="lnkbtnUploadDealerDocuments_Click">Upload Documents</asp:LinkButton>
                                </li>						
                                <li>
                                    <asp:Label ID="lblDealerID" runat="server" Text="Dealer#:" />                                  
                                    <asp:TextBox ID="txtSearchDealerID" runat="server" Width="100px" Text="" />
                                    &nbsp;
                                    <asp:Label ID="lblDealerName" runat="server" Text="Name:" />
                                    <asp:TextBox ID="txtSearchDealerName" runat="server" Width="100px" Text="" />
                                    &nbsp;
                                    <asp:Button ID="btnFind" runat="server" Text="Find" CssClass="OButton" />
                                    &nbsp;
                                    <asp:Label ID="lblDealerGroupName" runat="server" Text="Group:" />                              
                                    <asp:DropDownList ID="ddlSearchDealerGroups" runat="server" AutoPostBack="true" AppendDataBoundItems="true">
                                        <asp:ListItem Value="" Text="-All-" Selected="True"></asp:ListItem>
                                    </asp:DropDownList>
                                    &nbsp;
                                    <%--08/15/2014 (ANANDI) - Added Active/Inactive Filter for search criteria (SP has been updated).--%>
                                    <asp:Label ID="lblActive" runat="server" Text="Active:" />                              
                                    <asp:DropDownList ID="ddlSearchDealerStatus" runat="server" AutoPostBack="true">
                                        <asp:ListItem Text="-All-" Value="" Selected="True" />
                                        <asp:ListItem Text="Yes" Value="1"  />
                                        <asp:ListItem Text="No" Value="0" />
                                    </asp:DropDownList>
                                    <%--END--%>
                                </li>
					        </ul>
				        </div>
                        <div id="divGridViewDealerInformation">
                            <asp:UpdatePanel ID="updtpnlGridViewDealerInformation" runat="server">
                                <ContentTemplate>
                                    <%--DEALER INFORMATION GRIDVIEW--%>
                                    <asp:GridView ID="gvDealersView" CssClass="OTable"  runat="server"
                                    AllowPaging="true" PageSize="20" AllowSorting="true" DataKeyNames="DealerID" 
                                    AutoGenerateColumns="false" DataSourceID="SQLGetDealersBySearchCriteria">
                                        <Columns>                            
                                            <asp:TemplateField HeaderText="Dealer#" SortExpression="DealerID">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="lnkbtnEditDealer" runat="server" CommandName="EditDealer"  
                                                        CommandArgument='<%# Eval("DealerID")%>' OnClick="lnkbtnEditDealer_Click"><%# Eval("DealerID")%></asp:LinkButton>
                                                </ItemTemplate>
                                            </asp:TemplateField>                            
                                            <asp:TemplateField SortExpression="DealerID">
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="imgbtnGotoDealerDocuments" runat="server" CommandName="GotoDealerDocuments"
                                                    CommandArgument='<%# Eval("DealerID")%>' OnClick="imgbtnGotoDealerDocuments_Click" ImageUrl="~/images/icons/page.png" /> 
                                                </ItemTemplate>
                                            </asp:TemplateField>  
                                            <asp:BoundField DataField="DealerName" HeaderText="Dealer Name" SortExpression="DealerName" />                            
                                            <asp:BoundField DataField="Address" HeaderText="Address"  />
                                            <asp:BoundField DataField="DealerCity" HeaderText="City" SortExpression="DealerCity" />
                                            <asp:BoundField DataField="DealerState" HeaderText="State" SortExpression="DealerState" />
                                            <asp:BoundField DataField="DealerPostalCode" HeaderText="Zip"  />
                                            <asp:BoundField DataField="DealerCountry" HeaderText="Country" SortExpression="DealerCountry" />
                                            <%--08/15/2014 (ANANDI) - Added Active/Inactive Filter for search criteria (SP has been updated).--%>
                                            <asp:TemplateField HeaderText="Active" SortExpression="DealerStatus" HeaderStyle-Width="60px" ItemStyle-CssClass="CenterAlign">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblIsActive" runat="server" CssClass='<%# Eval("DealerStatus", "lblActive{0}") %>' Text='<%# ActiveInactive(Eval("DealerStatus")) %>' Font-Bold="false" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <%--END--%>
                                            <%--<asp:CheckBoxField DataField="DealerStatus" HeaderText="Active" SortExpression="DealerStatus" HeaderStyle-Width="60px" ItemStyle-CssClass="CenterAlign" />--%>                                              
                                        </Columns>
                                        <EmptyDataTemplate>
						                    <div class="NoData">There are no dealer records found!</div>
					                    </EmptyDataTemplate>                                
                                        <AlternatingRowStyle CssClass="Alt" />
                                        <PagerSettings Mode="Numeric" />
					                    <PagerStyle CssClass="Footer" />
                                    </asp:GridView>
                                    <%--END--%>
                                    <%--SEARCH FOR DEALER(S)--%>
                                    <asp:SqlDataSource ID="SQLGetDealersBySearchCriteria" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
                                    SelectCommand="sp_GetAllICCareDealersBySearch" SelectCommandType="StoredProcedure">
                                        <SelectParameters>
                                            <asp:ControlParameter ControlID="txtSearchDealerID" DefaultValue="" ConvertEmptyStringToNull="false" Name="SearchDealerID" PropertyName="Text" Type="String" /> 
                                            <asp:ControlParameter ControlID="txtSearchDealerName" DefaultValue="" ConvertEmptyStringToNull="false" Name="SearchDealerName" PropertyName="Text" Type="String" />
                                            <asp:ControlParameter ControlID="ddlSearchDealerGroups" DefaultValue="" ConvertEmptyStringToNull="false" Name="SearchDealerGroupName" PropertyName="SelectedValue" Type="String" />
                                            <%--08/15/2014 (ANANDI) - Added Active/Inactive Filter for search criteria (SP has been updated).--%>
                                            <asp:ControlParameter ControlID="ddlSearchDealerStatus" DefaultValue="" ConvertEmptyStringToNull="false" Name="SearchDealerStatus" PropertyName="SelectedItem.Value" Type="String" />
                                            <%--END--%>            
                                        </SelectParameters>
                                    </asp:SqlDataSource>	
                                    <%--END--%>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </asp:Panel>
    </div>

    <%--BRAND SOURCES (Just displays Mirion and ICCare Brands)--%>
    <asp:SqlDataSource ID="SQLBrandSources" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="SELECT [BrandSourceID], [BrandName] 
                   FROM [dbo].[BrandSources]
                   WHERE [BrandSourceID] = 3
                   ORDER BY [BrandName] ASC">
    </asp:SqlDataSource>
    <%--END--%>

    <%--DEALER GROUPS--%>
    <asp:SqlDataSource ID="SQLDealerGroups" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="SELECT [DealerGroupID], [DealerGroupName] 
                   FROM [dbo].[DealerGroups] 
                   ORDER BY [DealerGroupName]">
    </asp:SqlDataSource>
    <%--END--%>

    <%--COUNTRIES--%>
    <asp:SqlDataSource ID="SQLCountries" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="SELECT [CountryID], [CountryName] 
                   FROM [dbo].[Countries] 
                   WHERE [Active] = 1 
                   ORDER BY [CountryName] ASC">
    </asp:SqlDataSource>
    <%--END--%>

    <%--STATES BY COUNTRY--%>
    <asp:SqlDataSource ID="SQLGetStatesByCountry_Domestic" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand=" SELECT [StateID], [StateAbbrev] 
                    FROM [dbo].[States] 
                    WHERE [CountryID] = @CountryID 
                    ORDER BY [StateAbbrev] ASC">
        <SelectParameters>
            <asp:ControlParameter ControlID="ddlCountryID" DefaultValue="" Name="CountryID" PropertyName="SelectedItem.Value" Type="String" /> 
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="SQLGetStatesByCountry_Foreign" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand=" SELECT [StateID], [StateAbbrev] 
                    FROM [dbo].[States] 
                    WHERE [CountryID] = @CountryID 
                    ORDER BY [StateAbbrev] ASC">
        <SelectParameters>
            <asp:ControlParameter ControlID="ddlCountryID" DefaultValue="" Name="CountryID" PropertyName="SelectedItem.Value" Type="String" /> 
        </SelectParameters>
    </asp:SqlDataSource>
    <%--END--%>

    <%--EDI CLIENTS--%>
    <asp:SqlDataSource ID="SQLEDIClients" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="SELECT [ClientID], [ClientName] 
                   FROM [edi].[Clients] 
                   WHERE [Active] = 1 
                   ORDER BY [ClientName] ASC">
    </asp:SqlDataSource>
    <%--END--%>
</asp:Content>
