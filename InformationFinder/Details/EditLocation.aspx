<%@ Page Title="Add/Edit Location" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InformationFinder_Details_EditLocation" Codebehind="EditLocation.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script src="/scripts/jquery.validate.min.js" type="text/javascript"></script>
	
     <script type="text/javascript">

         $(document).ready(function () {

             Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
             JQueryControlsLoad();
             
         });

         function JQueryControlsLoad() {

             // Service start date Datepicker
             $('#<%=txtServiceStartDate.ClientID %>').datepicker();

             // Service end date Datepicker
             $('#<%=txtServiceEndDate.ClientID %>').datepicker();

             // Accordion
             $("#accordion").accordion({ header: "h3",
                                        autoHeight: false                         
                                       });

             $('#groupDialog').dialog({
                 autoOpen: false,
                 width: 400,
                 resizable: false,
                 title: "Manage Group",
                 open: function (type, data) {
                     $(this).parent().appendTo("form");
                     $('#<%= btnLoadGroup.ClientID %>').click();
                     $('.ui-dialog :input').focus();
                 },
                 buttons: {
                     "Ok": function () {
                         $('#<%= btnAddGroup.ClientID %>').click();
                     },
                     "Cancel": function () {
                         $(this).dialog("close");
                     }
                 },
                 close: function () {
                     $('#<%= btnCancelGroup.ClientID %>').click();
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
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">

    <act:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server">
    </act:toolkitscriptmanager>
   

    <div id="groupDialog">
        <asp:UpdatePanel ID="upnlAddNotes" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
            
                <div class="FormError" id="groupDialogErrors" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="groupDialogErrorMsg" runat="server" >An error was encountered.</span></p>
				</div>

                <div class="OForm" > 
                             
                    <div class="Row">
                        <div class="Label Small">Location #:</div>
                        <div class="LabelValue"><asp:Label runat="server" ID="lblGroupLocationID" CssClass="StaticLabel" /></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Location Name:</div>
                        <div class="LabelValue"><asp:Label runat="server" ID="lblGroupLocationName" CssClass="StaticLabel" /></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Group #:</div>
                        <div class="LabelValue"><asp:Label runat="server" ID="lblGroupID" CssClass="StaticLabel" /></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Active:</div>
                        <div class="Control">
                            <asp:CheckBox ID="chkBoxGroupActive" runat="server" Text=""  />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Group Name<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtGroupName" 
                                MaxLength="40" CssClass="Size Medium" ValidationGroup="form" />
                            <span class="InlineError" id="lblGroupNameValidate" runat="server" visible="false"></span>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row"> 
                        <div class="Label Small">Schedule:</div>
                        <div class="Control"> 
                            <asp:DropDownList ID="ddlGroupSchedule" runat="server" DataValueField="ScheduleID"
                                        DataTextField="ScheduleName" DataSourceID="GetSchedules"></asp:DropDownList>                                        
                        </div>
                        <div class="Clear"></div>
                    </div>       

                </div>               
                <asp:button text="Save" style="display:none;" id="btnAddGroup" OnClick="btnAddGroup_Click" runat="server" />
                <asp:button text="Close" style="display:none;" id="btnCancelGroup" OnClick="btnCancelGroup_Click" runat="server" />
                <asp:button text="Load" style="display:none;" id="btnLoadGroup" OnClick="btnLoadGroup_Click" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

 
     <div style="width:100%" >

        <div class="FormError" id="error" runat="server">
		    <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
	    </div>

        <div id="accordion" style="margin-top:10px;">

		    <div >
			    <h3><a href="#">Base Details</a></h3>
                <div class="OForm AccordionPadding">
                           
                    <div class="Row">
                        <div class="Label">Account #:</div>
                        <div class="LabelValue"><asp:Label runat="server" ID="lblAccountID" CssClass="StaticLabel" /></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Location #:</div>
                        <div class="LabelValue"><asp:Label runat="server" ID="lblLocationID" CssClass="StaticLabel" /></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Active:</div>
                        <div class="Control">
                            <asp:CheckBox ID="chkBoxActive" runat="server" Text=""  />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Location Name<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtLocationName" 
                                MaxLength="50" CssClass="Size Medium" ValidationGroup="form" />
                            <span class="InlineError" id="lblLocationNameValidate" runat="server" visible="false"></span>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label"></div>
                        <div class="Control">
                            <asp:CheckBox ID="chkBoxIsDefaultLocation" runat="server" Text="Default Location" />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Location Code:</div>
                        <div class="Control">
                            <asp:TextBox runat="server" ID="txtLocationCode" 
                                MaxLength="50" CssClass="Size Medium" ValidationGroup="form" />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Reporting UOM:</div>
                        <%--<div class="LabelValue"><asp:Label runat="server" ID="lblLocationUOM" CssClass="StaticLabel" /></div>--%>
                        <div class="Control">
                            <asp:DropDownList ID="ddlLocationUOM" runat="server" >                            
                                <asp:ListItem Text="mrem" Value="mrem" />
                                <asp:ListItem Text="mSv" Value="mSv" />                            
                            </asp:DropDownList>              
                        </div>
                        <div class="Clear"></div>
                    </div>                    

                    <div class="Row">
                        <div class="Label">Time Zone<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlTimeZone" runat="server"  DataValueField="TimeZoneID"
                                          DataTextField="TimeZone" DataSourceID="GetTimeZones" AutoPostBack="false" /> 
                            <span class="InlineError" id="lblTimeZoneValidate" runat="server" visible="false"></span>                          
                        </div>
                        <div class="Clear"></div>
                    </div>
                <div id="divLocationServiceDates" runat="server" class="Row">
                    <div class="Label">Service Start & End<span class="Required">*</span>:</div>
                    <div  class="Control">
                        <asp:TextBox ID="txtServiceStartDate"  Enabled="false" runat="server" 
                            AutoPostBack ="true" CssClass="Size Small" 
                            OnTextChanged="txtServiceStartDate_TextChanged"></asp:TextBox>                                        
                        TO
                        <asp:TextBox ID="txtServiceEndDate" runat="server" Enabled="false" CssClass="Size Small" readonly="true" OnTextChanged="txtServiceEndDate_TextChanged"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator_txtServiceStartDate" runat="server" 
                                    ControlToValidate="txtServiceStartDate" Display="Dynamic" 
                                    ErrorMessage="Service Start & End Date" Text="Service Start & End Date is required for this account location."
                                    ValidationGroup="form" />
                
                    </div>
                    <div class="Clear"></div>
                    </div>          
                </div>
            </div>

            <div >
			    <h3><a href="#">Billing Address</a></h3>
                <div class="OForm AccordionPadding">

                    <asp:UpdatePanel ID="upnlBillingAddress" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                                                 
                            <div class="Row">
                                <div class="Label"></div>
                                <div class="Control"><asp:CheckBox ID="chkCopyAccountBilling" runat="server" 
                                        Text="Copy from Account Billing Address" Font-Bold="False" Font-Italic="True" AutoPostBack= "True"
                                        oncheckedchanged="chkCopyAccountBilling_CheckedChanged" /></div>
                                <div class="Clear"></div>

                            </div>

                            <div class="Row">
                                <div class="Label">Company<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtBCompany" CssClass="Size XLarge" 
                                        ValidationGroup="form" MaxLength="100" />
                                     <span class="InlineError" id="txtBCompanyValidate" runat="server" visible="false"></span>
 
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Prefix:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlBPrefix" runat="server" >
                                        <asp:ListItem Text="" Value="" />
                                        <asp:ListItem Text="Dr." Value="Dr." />
                                        <asp:ListItem Text="Mr." Value="Mr." />
                                        <asp:ListItem Text="Mrs." Value="Mrs." />
                                        <asp:ListItem Text="Ms." Value="Ms." />
                                    </asp:DropDownList>              
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">First Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtBFirstName" 
                                        MaxLength="50" CssClass="Size Large" ValidationGroup="form" />
                                    <span class="InlineError" id="lblBFirstNameValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Last Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtBLastName" MaxLength="50" 
                                        CssClass="Size Large" ValidationGroup="form"/>
                                    <span class="InlineError" id="lblBLastNameValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Phone Number<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtBTelephone" 
                                        CssClass="Size Medium2" ValidationGroup="form" MaxLength="24" />
                                    <span class="InlineError" id="lblBTelephoneValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Fax Number:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtBFax" CssClass="Size Medium2" 
                                        ValidationGroup="form" MaxLength="24" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">E-Mail Address:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtBEmailAddress" CssClass="Size Large" 
                                        ValidationGroup="form" MaxLength="60" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Country<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlBCountry" runat="server"  DataValueField="CountryID"
                                                    DataTextField="CountryName" DataSourceID="GetCountries" 
                                                    AutoPostBack="True" onselectedindexchanged="ddlBCountry_SelectedIndexChanged" />
                           
                                    <span class="InlineError" id="lblBCountryValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtBAddress1" 
                                        MaxLength="255" CssClass="Size XLarge" ValidationGroup="form" />
                                    <span class="InlineError" id="lblBAddress1Validate" runat="server" visible="false"></span>
                                </div>   
                                <div class="Clear"></div>                
                            </div>

                            <div class="Row">
                                <div class="Label"></div>
                                <div class="Control"><asp:TextBox runat="server" ID="txtBAddress2" 
                                        MaxLength="255" CssClass="Size XLarge" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label"></div>
                                <div class="Control"><asp:TextBox runat="server" ID="txtBAddress3" 
                                        MaxLength="255" CssClass="Size XLarge" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">City<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtBCity" MaxLength="100" 
                                        CssClass="Size Large2" ValidationGroup="form" />
                                    <span class="InlineError" id="lblBCityValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">State<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlBState" runat="server" DataSourceID="GetBStatesByCountry" 
                                            DataValueField="StateID" DataTextField="StateAbbrev" />
                                    <span class="InlineError" id="lblBStateValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Zip Code<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtBPostalCode" 
                                        MaxLength="15" CssClass="Size Medium2" ValidationGroup="form" />
                                    <span class="InlineError" id="lblBPostalCodeValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>                            
                    
                        </ContentTemplate>  
                    </asp:UpdatePanel>    
                                
                </div>
            </div>

            <div >
			    <h3><a href="#">Shipping Address</a></h3>
                <div class="OForm AccordionPadding">
                    
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                                              
                            <div class="Row">
                                <div class="Label">
                                </div>
                                <div class="Control"><asp:CheckBox ID="chkBoxSameAsBilling" runat="server" 
                                        Text="Same as billing address" Font-Bold="False" Font-Italic="True" AutoPostBack= "True"
                                        oncheckedchanged="chkBoxSameAsBilling_CheckedChanged" /></div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Company<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSCompany" CssClass="Size XLarge" 
                                        ValidationGroup="form" MaxLength="100" />
                                     <span class="InlineError" id="txtSCompanyValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Prefix:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlSPrefix" runat="server" >
                                        <asp:ListItem Text="" Value="" />
                                        <asp:ListItem Text="Dr." Value="Dr." />
                                        <asp:ListItem Text="Mr." Value="Mr." />
                                        <asp:ListItem Text="Mrs." Value="Mrs." />
                                        <asp:ListItem Text="Ms." Value="Ms." />
                                    </asp:DropDownList>              
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">First Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSFirstName" 
                                        MaxLength="50" CssClass="Size Large" ValidationGroup="form" />
                                    <span class="InlineError" id="lblSFirstNameValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Last Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSLastName" MaxLength="50" 
                                        CssClass="Size Large" ValidationGroup="form"/>
                                    <span class="InlineError" id="lblSLastNameValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Phone Number<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSTelephone" 
                                        CssClass="Size Medium" ValidationGroup="form" MaxLength="24" />
                                    <span class="InlineError" id="lblSTelephoneValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Fax Number:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSFax" CssClass="Size Medium " 
                                        ValidationGroup="form" MaxLength="24" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">E-Mail Address:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSEmailAddress" CssClass="Size Large" 
                                        ValidationGroup="form" MaxLength="60" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Country<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlSCountry" runat="server"  DataValueField="CountryID"
                                                    DataTextField="CountryName" DataSourceID="GetCountries" 
                                                    AutoPostBack="True" onselectedindexchanged="ddlSCountry_SelectedIndexChanged" />
                         
                                    <span class="InlineError" id="lblSCountryValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSAddress1" 
                                        MaxLength="255" CssClass="Size XLarge" ValidationGroup="form" />
                                    <span class="InlineError" id="lblSAddress1Validate" runat="server" visible="false"></span>
                                </div> 
                                <div class="Clear"></div>                  
                            </div>

                            <div class="Row">
                                <div class="Label"></div>
                                <div class="Control"><asp:TextBox runat="server" ID="txtSAddress2" 
                                        MaxLength="255" CssClass="Size XLarge" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label"></div>
                                <div class="Control"><asp:TextBox runat="server" ID="txtSAddress3" 
                                        MaxLength="255" CssClass="Size XLarge" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">City<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSCity" MaxLength="100" 
                                        CssClass="Size Large2" ValidationGroup="form" />
                                    <span class="InlineError" id="lblSCityValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">State<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlSState" runat="server" DataSourceID="GetSStatesByCountry" 
                                            DataValueField="StateID" DataTextField="StateAbbrev" />
                                    <span class="InlineError" id="lblSStateValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Zip Code<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtSPostalCode" 
                                        MaxLength="15" CssClass="Size Medium2 " ValidationGroup="form" />
                                    <span class="InlineError" id="lblSPostalCodeValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>                            

                        </ContentTemplate>
                    </asp:UpdatePanel> 

                </div>
            </div>

            <div >
			    <h3><a href="#">Default Setup</a></h3>
                <div class="OForm AccordionPadding">
             
                 <asp:UpdatePanel ID="upnlDefaultSetup" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>                                
                        
                        <div class="Row"> 
                            <div class="Label">Primary Product<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:DropDownList ID="ddlProductGroup" runat="server"  DataSourceID="GetProductGroups"
                                    DataValueField="ProductGroupID" DataTextField="ProductGroupName" AutoPostBack="true" ></asp:DropDownList>
                                <span class="InlineError" id="lblProductGroupValidate" runat="server" visible="false"></span>
                            </div>
                            <div class="Clear"></div>
                        </div>
                        
                        <div class="Row"> 
                            <div class="Label">Label<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:DropDownList ID="ddlLabel" runat="server"  DataSourceID="GetLabels"
                                    DataValueField="LabelID" DataTextField="LabelDesc" ></asp:DropDownList>
                                <span class="InlineError" id="ddlLabelValidate" runat="server" visible="false"></span>
                            </div>
                            <div class="Clear"></div>
                        </div>                                             
                     
                         <div class="Row"> 
                            <div class="Label">Schedule:</div>
                            <div class="Control"> 
                                <asp:DropDownList ID="ddlSchedule" runat="server" DataValueField="ScheduleID"
                                    DataTextField="ScheduleName" DataSourceID="GetSchedules"></asp:DropDownList>                                        
                            </div>
                            <div class="Clear"></div>
                        </div>                        

                    </ContentTemplate>   
                 </asp:UpdatePanel>
                </div>
            </div>

            <div id="HDNSection" runat ="server" >
			    <h3><a href="#">High Dose Notifications</a></h3>
                <div >
                   
                        <p >High Dose Notification will alert administrators when a wearer reads their dosimeter and the dose is above the limits specified below. Limits can be disabled by setting them to <b>0</b>.</p>
                        <table  style="margin: 0 60px;" >
                            <tr>
                                <td>&nbsp;</td>
                                <td class="Label" style="text-align:left" >Deep</td>
                                <td class="Label" style="text-align:left">Shallow</td>
                                <td class="Label" style="text-align:left">Eye</td>
                            </tr>
                            <tr>
                                <td class="Label">Current:</td>
                                <td><asp:TextBox ID="txtDLCurrent" runat="server" MaxLength="6" Text="0" /></td>
                                <td><asp:TextBox ID="txtSLCurrent" runat="server" MaxLength="6" Text="0" /></td>
                                <td><asp:TextBox ID="txtEyeCurrent" runat="server" MaxLength="6" Text="0" /></td>
                            </tr>
                            <tr>
                                <td class="Label">Quarter To Date:</td>
                                <td><asp:TextBox ID="txtDLQuarter" runat="server" MaxLength="6" Text="0" /></td>
                                <td><asp:TextBox ID="txtSLQuarter" runat="server" MaxLength="6" Text="0" /></td>
                                <td><asp:TextBox ID="txtEyeQuarter" runat="server" MaxLength="6" Text="0" /></td>
                            </tr>
                            <tr>
                                <td class="Label">Year To Date:</td>
                                <td><asp:TextBox ID="txtDLYear" runat="server" MaxLength="6" Text="0" /></td>
                                <td><asp:TextBox ID="txtSLYear" runat="server" MaxLength="6" Text="0" /></td>
                                <td><asp:TextBox ID="txtEyeYear" runat="server" MaxLength="6" Text="0" /></td>
                            </tr>
                            <tr>
                                <td ></td>
                                <td colspan = "3"><span class="InlineError" id="lblHDNValidate" runat="server" visible="false"></span></td>
                            </tr>
                        </table>
                                        
                </div>
            </div>

            <div id="GroupSection" runat ="server" >
			    <h3><a href="#">Groups</a></h3>
                <div >

                    <asp:UpdatePanel ID="upnlGroupSection" runat="server"  UpdateMode="Conditional">
                        <Triggers>
                            <asp:AsyncPostBackTrigger controlid="btnAddGroup" eventname="Click" />
                        </Triggers>

                        <ContentTemplate>
                    
                            <div class="OToolbar JoinTable" id="CreateGroup" runat ="server">
					            <ul>
						            <li>
                                        
                                        <asp:LinkButton ID="btnNewGroup" runat="server"  
                                            CommandName="NewGroup" CommandArgument='<%= Request.QueryString["locationID"] %>' 
                                            CssClass="Icon Add" onclick="btnNewGroup_Click" >Create Group</asp:LinkButton>
                               
                                    </li>
					            </ul>
				            </div>

                            <asp:GridView ID="gvLocationGroup" CssClass="OTable" runat="server"
                                AutoGenerateColumns="False"  DataSourceID="sqlLocationGroup" 
                                allowpaging="True" AllowSorting="True" PageSize="10" DataKeyNames="GroupID">
                      
                                <Columns>

                                    <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" HeaderText="Group Name" SortExpression="GroupName">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnEditGroup" runat="server"  
                                                CommandName="EditGroup" CommandArgument='<%# Eval("GroupID")%>' 
                                                onclick="btnEditGroup_Click" ><%# Eval("GroupName")%></asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField ItemStyle-CssClass="mt-itm centeralign" HeaderStyle-CssClass="mt-hd" HeaderStyle-Width="60" HeaderText="Active" SortExpression="Active">
                                        <ItemTemplate>
                                            <asp:Label ID="lblDeviceActive" runat="server" CssClass='<%# Eval("Active", "lblActive{0}") %>' Text='<%# YesNo(Eval("Active")) %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                       
                                </Columns>

                                <EmptyDataTemplate>
                                    <div>
                                        There are no groups assigned to this location.
                                    </div>
                                </EmptyDataTemplate>
                                <AlternatingRowStyle CssClass="Alt" />
                                <PagerStyle CssClass="Footer" />
                       
                            </asp:GridView>

                        </ContentTemplate>
                    </asp:UpdatePanel> 
                    
                </div>
            </div>

            <div id="BillingGroupSection" runat ="server">
			    <h3><a href="#">Invoice Delivery Method</a></h3>                       
                <div id="billing_fields" class="OForm AccordionPadding">
                    <asp:UpdatePanel ID="upnlBillingInformation" runat="server" UpdateMode="Always">                    

                        <ContentTemplate>
                             
                            <div class="Row"> 
                                <div class="Label">Billing Group:</div>
                                <div class="Control" >                                    
                                    <asp:DropDownList ID="ddlBillingGroup" runat="server"  
                                        DataValueField="BillingGroupID" DataTextField="BillingGroupDetail" AutoPostBack="true"
                                        OnSelectedIndexChanged="ddlBillingGroup_SelectedIndexChanged" />
                                </div>
                                <div class="Clear"></div>
                            </div>                           

                            <hr />

                            <div class="Row"> 
                                <div class="Label">PO#:</div>
                                <div class="Control">
                                    <asp:TextBox  ID="txtPOno"   runat="server" CssClass ="Size Small" MaxLength="60" Enabled="false" ></asp:TextBox>                                                   
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label">Comapny Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox  ID="txtCompanyName"   runat="server" CssClass ="Size Medium" MaxLength="100" Enabled="false" ></asp:TextBox>    
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidatorCompanyName" runat="server" 
                                        ControlToValidate= "txtCompanyName" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                        ErrorMessage="Billing Company Name" Text="Company Name is required" >
                                    </asp:RequiredFieldValidator>            
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label">Contact Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox  ID="txtContactName"   runat="server" CssClass ="Size Medium" MaxLength="60" Enabled="false"></asp:TextBox>    
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidatorContactName" runat="server" 
                                        ControlToValidate= "txtContactName" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                        ErrorMessage="Billing Contact Name" Text="Contact Name is required" >
                                    </asp:RequiredFieldValidator>              
                                </div>
                                <div class="Clear"></div>
                            </div>                            

                            <div class="Row"> 
                                <div class="Label">Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox  ID="txtAddress1B"   runat="server" CssClass ="Size XLarge" MaxLength="255" Enabled="false"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddress1B" runat="server" 
                                        ControlToValidate= "txtAddress1B" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                        ErrorMessage="Billing Address" Text="Address is required">
                                    </asp:RequiredFieldValidator>                   
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label"></div>
                                <div class="Control"> <asp:TextBox  ID="txtAddress2B" runat="server" CssClass ="Size XLarge" MaxLength="255" Enabled="false"></asp:TextBox></div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label"></div>
                                <div class="Control"> <asp:TextBox  ID="txtAddress3B"  runat="server" CssClass ="Size XLarge" MaxLength="255" Enabled="false"></asp:TextBox></div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label">Country<span class="Required">*</span>:</div>
                                <div class="Control"> 
                                    <asp:DropDownList ID="ddlCountryB" runat="server"  DataSourceID="GetCountries" Enabled="false"
                                    DataValueField="CountryID" DataTextField="CountryName" AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlCountryB_SelectedIndexChanged" /> 
                                    <asp:RequiredFieldValidator runat="server" ID="reqfldvalCountryB" ControlToValidate="ddlCountryB"
                                        ErrorMessage="Billing Country" Text="Country is required" Display="Dynamic" InitialValue="0" ValidationGroup="CSRegisterForm">
                                    </asp:RequiredFieldValidator>                                                                          
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label">City<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox  ID="txtCityB" runat="server" CssClass ="Size Large2" MaxLength="100" Enabled="false"></asp:TextBox> 
                                    <asp:RequiredFieldValidator ID="reqfldvalCity" runat="server" 
                                        ControlToValidate= "txtCityB" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                        ErrorMessage="Billing City" Text="City is required" >
                                    </asp:RequiredFieldValidator>                   
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label">State/Postal<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlStateB" runat="server" Enabled="false"
                                        DataValueField="StateID" DataTextField="StateAbbrev" /> 
                                
                                    <asp:TextBox  ID="txtPostalB"   runat="server" CssClass ="Size Small" MaxLength="15" Enabled="false"></asp:TextBox>   

                                    <asp:RequiredFieldValidator runat="server" ID="reqfldvalStateB" ControlToValidate="ddlStateB"
                                        ErrorMessage="Billing State" Text="State is required" Display="Dynamic" InitialValue="0" ValidationGroup="CSRegisterForm">
                                    </asp:RequiredFieldValidator>

                                    <asp:RequiredFieldValidator ID="reqfldvalPostalB" runat="server" 
                                        ControlToValidate= "txtPostalB" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                        ErrorMessage="Billing Postal Code" Text="Postal Code is required">
                                    </asp:RequiredFieldValidator>                                            
                                </div>
                                <div class="Clear"></div>
                            </div> 
                            
                            <hr />

                            <table style="width:100%;border:0;" cellpadding="0" cellspacing="0" class="OTable">
                                <tr id ="rowPrintMail" runat="server">
                                    <td style="width:15%;" class="Label">
                                        Print & Mail:
                                    </td>
                                    <td style="width:3%;">
                                        <asp:CheckBox ID="chkBoxInvDeliveryPrintMail" runat="server" Text="" Enabled="false" />
                                    </td>
                                    <td style="width:11%;">                                
                                    </td>
                                    <td>                                
                                    </td>                            
                                </tr>
                                <tr>
                                    <td class="Label">
                                        Email:
                                    </td>
                                    <td>
                                        <asp:CheckBox ID="chkBoxInvDeliveryEmail" runat="server" Text="" Enabled="false" />
                                    </td>
                                    <td >
                                        Primary Email:                                
                                    </td>
                                    <td >   
                                        <asp:TextBox  ID="txtInvDeliveryPrimaryEmail" runat="server" MaxLength="60" width="250" Enabled="false"></asp:TextBox>   
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidatorInvDeliveryPrimaryEmail" runat="server" 
                                            ControlToValidate= "txtInvDeliveryPrimaryEmail" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                            ErrorMessage="Invoice Delivery Primary E-mail Address" Text="E-Mail Address is required" enabled="false">
                                        </asp:RequiredFieldValidator>
                                        <asp:RegularExpressionValidator ID="RegularExpressionValidatorInvDeliveryPrimaryEmail" runat="server" 
                                            ErrorMessage="Invoice Delivery Primary Email" ControlToValidate="txtInvDeliveryPrimaryEmail" Display="Dynamic"
                                            ValidationGroup="CSRegisterForm" Text="Invalid e-mail format" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" enabled="false">
                                        </asp:RegularExpressionValidator>
                                    </td>                            
                                </tr>
                                 <tr >
                                    <td>                                
                                    </td>
                                    <td>                                
                                    </td>                            
                                    <td >  
                                        Secondary Email:
                                    </td>
                                    <td >  
                                        <asp:TextBox  ID="txtInvDeliverySecondaryEmail" runat="server" MaxLength="60" width="250" Enabled="false"></asp:TextBox>
                                        <asp:RegularExpressionValidator ID="RegularExpressionValidatorInvDeliverySecondaryEmail" runat="server" 
                                            ErrorMessage="Invoice Delivery Secondary Email" ControlToValidate="txtInvDeliverySecondaryEmail" Display="Dynamic"
                                            ValidationGroup="CSRegisterForm" Text="Invalid e-mail format" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" enabled="false">
                                        </asp:RegularExpressionValidator>
                                    </td>
                                </tr>
                                <tr id ="rowFax" style="display:none">
                                    <td class="Label">
                                        Fax:
                                    </td>
                                    <td>
                                        <asp:CheckBox ID="chkBoxInvDeliveryFax" runat="server" Text="" Enabled="false" />
                                    </td>
                                    <td>
                                        Primary Fax:
                                    </td>
                                    <td>  
                                        <asp:TextBox  ID="txtInvDeliveryPrimaryFax" runat="server" CssClass ="Size Medium2" MaxLength="24" Enabled="false"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidatorInvDeliveryPrimaryFax" runat="server" 
                                            ControlToValidate= "txtInvDeliveryPrimaryFax" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                            ErrorMessage="Invoice Delivery Fax" Text="Fax# is required" enabled="false">
                                        </asp:RequiredFieldValidator>
                                    </td>                            
                                </tr>
                                 <tr id ="rowEDI" runat="server">
                                    <td class="Label">
                                        EDI:
                                    </td>
                                    <td>
                                        <asp:CheckBox ID="chkBoxInvDeliveryEDI" runat="server" Text="" Enabled="false"/>
                                    </td>
                                    <td>  
                                        Client:
                                    </td>
                                    <td>   
                                        <asp:TextBox  ID="txtInvDeliveryEDIClientID" runat="server" CssClass ="Size Medium2" MaxLength="24" Enabled="false"></asp:TextBox>
                                    </td>                            
                                </tr>
                                 <tr >
                                    <td class="Label">
                                        Upload:
                                    </td>                             
                                    <td>
                                        <asp:CheckBox ID="chkBoxInvDeliveryUpload" runat="server" Text="" Enabled="false" />
                                    </td>
                                    <td>  
                                        Instruction File:
                                    </td>
                                    <td>   
                                        <asp:FileUpload ID="fileUploadInvDeliveryUpload" runat="server" Enabled="false" />
                                    </td>                            
                                </tr>
                                <tr >
                                    <td>                                 
                                    </td>
                                    <td>                                 
                                    </td>
                                    <td>  
                                        Instruction Note:
                                    </td>
                                    <td>  
                                        <asp:TextBox  ID="txtInvDeliveryUploadInstruction" TextMode="MultiLine" Height="50" Width="400" MaxLength="1000" runat="server" Enabled="false"></asp:TextBox>
                                        <asp:RegularExpressionValidator id="RegularExpressionValidatorInvDeliveryUploadInstruction" 
                                                            ControlToValidate="txtInvDeliveryUploadInstruction"
                                                            ValidationExpression="^.{0,1000}$"  
                                                            ValidationGroup="CSRegisterForm"
                                                            Display="Dynamic"
                                                            ErrorMessage="Upload Instructions is max 1000 characters."
                                                            Text="Upload Instructions is max 1000 characters"
                                                            runat="server"
                                                            enabled="false" />                                
                                    </td>                            
                                </tr>
                                <tr >
                                    <td class="Label">
                                        Do Not Send:
                                    </td>
                                    <td>
                                        <asp:CheckBox ID="chkBoxInvDeliveryDoNotSend" runat="server" Text="" Enabled="false" />
                                    </td>
                                    <td>                                
                                    </td>
                                    <td>                                
                                    </td>                            
                                </tr>                                 
                            </table>
                
                        </ContentTemplate>  
                    </asp:UpdatePanel>         
                </div>                                               
            </div>            

        </div>

        <div class="Buttons">
            <div class="RequiredIndicator"><span class="Required">*</span> Indicates a required field.</div>
            <div class="ButtonHolder">
                <asp:Button Text="Save" ID="btnSave" runat="server" OnClick="btnSave_Click" 
                    ValidationGroup="form" CssClass = "OButton" />
                <asp:Button ID="btnBack" CssClass="OButton" runat="server" Text="Back to Account"
                    onclick="btnBack_Click" /> 
            </div>
            <div class="Clear"> </div>
        </div>

     </div>
    
    


    <asp:SqlDataSource ID="GetTimeZones" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [TimeZoneID], [TimeZoneDesc] + ' ' + [TimeZoneName] As TimeZone FROM [TimeZones] ">
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetCountries" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [CountryID], [CountryName] FROM [Countries] Where [Active] = 1 Order By CountryName">
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetBStatesByCountry" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [StateID], [StateAbbrev] FROM [States] Where [CountryID] = @CountryID Order By StateAbbrev">
        <SelectParameters>
            <asp:ControlParameter ControlID="ddlBCountry" DefaultValue="" 
                 Name="CountryID" PropertyName="SelectedItem.Value" Type="String" /> 
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetSStatesByCountry" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [StateID], [StateAbbrev] FROM [States] Where [CountryID] = @CountryID Order By StateAbbrev">
        <SelectParameters>
            <asp:ControlParameter ControlID="ddlSCountry" DefaultValue="" 
                 Name="CountryID" PropertyName="SelectedItem.Value" Type="String" /> 
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlLocationGroup" runat="server" 
        ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="select GroupID, GroupName, Active
                        from LocationGroups
                        WHERE (LocationID = @LocationID) "
                       >
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="LocationID" QueryStringField="locationID" 
                Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetSchedules" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [ScheduleID], [ScheduleName] FROM [Schedules] Where [Active] = 1 Order By ScheduleName ">
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetProductGroups" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand=" SELECT [ProductGroupID], [ProductGroupName] FROM [ProductGroups] 
                        Where [ProductGroupName] != 'Instadose 1'
                        Order By ProductGroupName ">
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetLabels" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand=" select '0' As LabelID, '  -- Select Label --' As LabelDesc
                        Union
                        select a.LabelID, 
                        Case When b.LabelTypeName = 'No Label' Then a.LabelDesc Else a.LabelDesc + ' -- ' + b.LabelTypeName End As LabelDesc                    
                        from DeviceLabels a
                        Join LabelTypes b On a.LabelTypeID = b.LabelTypeID 
                        Where b.ProductGroupID = @ProductGroupID OR a.LabelID = 10
                        Order By LabelDesc" >
        <SelectParameters>
            <asp:ControlParameter ControlID="ddlProductGroup" DefaultValue="" 
                 Name="ProductGroupID" PropertyName="SelectedItem.Value" Type="String" /> 
        </SelectParameters>
    </asp:SqlDataSource>

</asp:Content>

