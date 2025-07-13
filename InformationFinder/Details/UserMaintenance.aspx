<%@ Page Title="Add/Edit User" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InformationFinder_Details_UserMaintenance" Codebehind="UserMaintenance.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">  
    
    <script type="text/javascript" charset="utf-8">
        $(document).ready(function () {

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ajaxLoad);
            ajaxLoad();

        });

    	function ajaxLoad() {
    	    // Accordion
    	    $("#accordion").accordion({ header: "h3", 
                                        autoHeight: false
                                        });

    	    // UserBirthDate Datepicker
    	    $('#<%=txtUserBirthDate.ClientID %>').datepicker();

    	    // LastReminded Datepicker
    	    $('#<%=txtUserLastReminded.ClientID %>').datepicker();
    	}

    	function openWindow(link) {
    	    window.open(link, '_blank', '');
    	}

    </script>  
    <style type="text/css">
        .InlineError {
            max-width: 650px;
        }
        div.Control input {
            vertical-align: top;
        }
    </style>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">

    <act:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server">
    </act:toolkitscriptmanager>

    <div style="100%">
        
        
        <div class="FormError" id="error" runat="server">
		    <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
	    </div>

        <div id="accordion" style="margin-top:10px;">

		    <div >
			    <h3><a href="#">Personal Information</a></h3>
			    <div class="OForm AccordionPadding">
                    
                            <div class="Row">                
                                <div class="Label">Account #:</div>
                                <div class="LabelValue" ><asp:Label runat="server" ID="lblAccountID"  /></div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">User #:</div>
                                <div class="LabelValue"><asp:Label runat="server" ID="lblUserID" CssClass="StaticLabel" /></div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row" id= "LogIn" runat = "server">
                                <div class="Label">Log In:</div>
                                <div class="Control"><asp:Button runat="server" ID="btnAuthenticate" OnClick="btnAuthenticate_Click" Text="Access Instadose" CssClass="OButton" /></div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Prefix:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlUserPrefix" runat="server" >
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
                                    <asp:TextBox runat="server" ID="txtUserFirstName" 
                                        MaxLength="40" CssClass="Size Medium" ValidationGroup="form" />
                                    <span class="InlineError" id="lblUserFirstNameValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Middle Name:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserMiddleName" 
                                        MaxLength="40" CssClass="Size Medium" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Last Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserLastName" MaxLength="40" 
                                        CssClass="Size Medium" ValidationGroup="form"/>
                                    <span class="InlineError" id="lblUserLastNameValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Gender:</div>
                                <div class="Control">
                                    <asp:RadioButton ID="radGenderMale" GroupName="Gender" Text="Male" runat="server" />
                                    <asp:RadioButton ID="radGenderFemale" GroupName="Gender" Text="Female" runat="server" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Identification Type:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlUserEmployeeType" runat="server">
                                        <asp:ListItem Text="SSN" Value="SSN" />
                                        <asp:ListItem Text="Employee ID" Value="Employee ID" />
                                    </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Identification #:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserEmployeeID" 
                                        MaxLength="16" CssClass="Size Medium2" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Date of Birth:</div>
                                <div class="Control">
                                    <asp:TextBox ID="txtUserBirthDate" runat="server"
                                        CssClass="Size Small" />
                                    <span class="InlineError" id="lblUserBirthDateValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Email<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserEmail" 
                                        CssClass="Size Large2" ValidationGroup="form" MaxLength="60" />   
                                    <span class="InlineError" id="lblUserEmailValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Phone Number:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserTelephone" 
                                        CssClass="Size Medium2" ValidationGroup="form" MaxLength="24" />               
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Fax Number:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserFax" CssClass="Size Medium2 " 
                                        ValidationGroup="form" MaxLength="24" />
                                </div>
                                <div class="Clear"></div>
                            </div>                            
               
                </div>
		    </div>

		    <div >
			    <h3><a href="#">User Information:</a></h3>
			    <div class="OForm AccordionPadding">
                    <asp:UpdatePanel ID="upnlUserInformation" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>

                            <div class="Row">
                                <div class="Label">Username<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserName" MaxLength="50" 
                                        CssClass="Size Medium " ValidationGroup="form" />
                                    <span class="InlineError" id="lblUserNameValidate" runat="server" visible="false"></span>  
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Password<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserPassword" MaxLength="100" 
                                        CssClass="Size Medium" ValidationGroup="form"  /> 
                                    <asp:Button ID="btnUserChangePassword" runat= "server" Text="Change Password" 
                                        onclick="btnUserChangePassword_Click"  />
                                    <span class="InlineError" id="lblUserPasswordValidate" runat="server" visible="false"></span>               
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label"></div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkBoxUserMustChangePassword" runat="server" Text="Must Change Password" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Security Question:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlUserSecurityQuestion" runat="server" DataSourceID= "GetSecurityQuestions" AutoPostBack="false" 
                                            DataTextField="SecurityQuestionText" DataValueField="SecurityQuestionID" ValidationGroup="form" />
                                    <span class="InlineError" id="lblUserSecurityQuestionValidate" runat="server" visible="false"></span>
                                </div>
                                    <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Security Answer:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserSecurityQuestionAnswer" 
                                        MaxLength="30" CssClass="Size Medium" ValidationGroup="form" />
                                    <span class="InlineError" id="lblUserSecurityQuestionAnswerValidate" runat="server" visible="false"></span>
                                </div>
                                    <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Role:</div>
                                <div class="Control"> 
                                    <asp:DropDownList ID="ddlUserRole" runat="server" DataSourceID= "GetRoles" AutoPostBack="true" 
                                                    DataTextField="UserRoleName" DataValueField="UserRoleID" 
                                        onselectedindexchanged="ddlUserRole_SelectedIndexChanged" />
                                    <asp:CheckBox ID="chkBoxIsPrimaryUser" runat="server" Text="Is Primary Admin User?" />
                                </div>
                                    <div class="Clear"></div>
                            </div>
        
                            <div class="Row">
                                <div class="Label">Location:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlUserLocation" runat="server" 
                                        DataSourceID= "GetLocations" AutoPostBack="true" DataTextField="LocationName"
                                        DataValueField="LocationID" 
                                        onselectedindexchanged="ddlUserLocation_SelectedIndexChanged" />
                                </div>
                                    <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Group:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlUserGroup" runat="server" DataSourceID= "GetGroupsByLocation" AutoPostBack="false" 
                                            DataTextField="GroupName" DataValueField="GroupID"  />
                                    <span class="InlineError" id="lblUserGroupValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Moveable User:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlUserMovable" runat="server">
                                                    <asp:ListItem Text="Not Moveable" Value="1" />
                                                    <asp:ListItem Text="Able to Move Group" Value="2" />
                                                    <asp:ListItem Text="Able to Move Group and Location" Value="3" />
                                                </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Last Reminded:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserLastReminded" 
                                        CssClass="Size Small " ValidationGroup="form" />
                                    <span class="InlineError" id="lblUserLastRemindedValidate" runat="server" visible="false"></span>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label"></div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkBoxUserActive" runat="server" Text="Active" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label"></div>
                                <div class="Control">
                                    <asp:CheckBox ID="chkBoxUserAutoDeviceAssign" runat="server" Text="Auto Assign Device" />
                                    <asp:CheckBox ID="chkBoxUserOkToEmail" runat="server" Text="Ok to Email" />
                                    <asp:CheckBox ID="chkBoxUserOkToEmailHDN" runat="server" Text="Ok to Email HDN" />
                                </div>
                                <div class="Clear"></div>
                            </div>                            

                        </ContentTemplate>
                    </asp:UpdatePanel>                                  
                </div>
		    </div>

		    <div >
			    <h3><a href="#">Shipping Address Information:</a></h3>
			    <div class="OForm AccordionPadding">
                    <asp:UpdatePanel ID="upnlShippingAddress" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>

                            <div class="Row">
                                <div class="Label">Address:</div>
                                <div class="Control"><asp:TextBox runat="server" ID="txtUserAddress1" 
                                        MaxLength="255" CssClass="Size XLarge " ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label"></div>
                                <div class="Control"><asp:TextBox runat="server" ID="txtUserAddress2" 
                                        MaxLength="255" CssClass="Size XLarge" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label"></div>
                                <div class="Control"><asp:TextBox runat="server" ID="txtUserAddress3" 
                                        MaxLength="255" CssClass="Size XLarge" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Country:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlUserCountryID" runat="server"  DataValueField="CountryID"
                                                    DataTextField="CountryName" DataSourceID="GetCountries" 
                                        AutoPostBack="True" 
                                        onselectedindexchanged="ddlUserCountryID_SelectedIndexChanged" >
                                                </asp:DropDownList>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">City:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserCity" MaxLength="100" 
                                        CssClass="Size Large2" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">State:</div>
                                <div class="Control">
                                    <asp:DropDownList ID="ddlUserStateID" runat="server" DataSourceID="GetStatesByCountry" 
                                            DataValueField="StateID" DataTextField="StateAbbrev" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label">Zip Code:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtUserPostalCode" 
                                        MaxLength="15" CssClass="Size Medium2" ValidationGroup="form" />
                                </div>
                                <div class="Clear"></div>
                            </div>                            

                        </ContentTemplate>                    
                    </asp:UpdatePanel> 
                </div>
		    </div>

            <div id="DosimeterAssignment" runat ="server" >
			    <h3><a href="#">Dosimeter Assignment History:</a></h3>
                <div>
                    <asp:GridView ID="gvUserDevice" CssClass="OTable" runat="server"
                        AutoGenerateColumns="False"  DataSourceID="sqlUserDevice" 
                        allowpaging="True" AllowSorting="True" DataKeyNames="userdeviceid"
                        >
               
                        <Columns>

                            <asp:TemplateField ItemStyle-CssClass="mt-itm RightAlign" HeaderStyle-CssClass="mt-hd" HeaderText="Serial#" SortExpression="SerialNo">
                                <ItemTemplate>
                                    <asp:HyperLink ID="HyperLink2" 
                                        NavigateUrl='<%# String.Format("Device.aspx?ID={0}&AccountID={1}", Eval("SerialNo"), Request.QueryString["accountID"]) %>'
                                        runat="server"><%# Eval("Serialno") %></asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="BodyRegionName" HeaderText="Body Region" 
                                SortExpression="BodyRegionName" />

                            <asp:BoundField DataField="AssignmentDate" HeaderText="Assigned On" 
                                SortExpression="AssignmentDate" />

                            <asp:BoundField DataField="DeactivateDate" HeaderText="Deactivated On" 
                                SortExpression="DeactivateDate" />

                            <asp:TemplateField ItemStyle-CssClass="mt-itm CenterAlign" HeaderStyle-CssClass="mt-hd" HeaderText="Active" SortExpression="Active" >
                                <ItemTemplate>
                                    <asp:Label ID="lblDeviceActive" runat="server" CssClass='<%# Eval("Active", "lblActive{0}") %>' Text='<%# YesNo(Eval("Active")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                                                      
                        </Columns>

                        <EmptyDataTemplate>
                            <div>
                                There are no badges assigned to this user.
                            </div>
                        </EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
                        <PagerStyle CssClass="Footer" />
                    </asp:GridView>
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
    


    
 
    <asp:SqlDataSource ID="sqlUserDevice" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="select a.userdeviceid, a.UserID, a.AssignmentDate, a.DeactivateDate,
        b.BodyRegionName, c.SerialNo,
        Case when a.Active = 1 then 'Yes' else 'No' End as myActive, a.active
            from UserDevices  a 
            left join  BodyRegions b on a.BodyRegionID=b.BodyRegionID 
            left join DeviceInventory c on a.DeviceID = c.DeviceID 
            WHERE (a.UserID = @UserID)
        ">
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="UserID" QueryStringField="userID" 
                Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetCountries" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [CountryID], [CountryName] FROM [Countries] Where [Active] = 1 Order By CountryName">
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetStatesByCountry" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [StateID], [StateAbbrev] FROM [States] Where [CountryID] = @CountryID Order By StateAbbrev">
        <SelectParameters>
            <asp:ControlParameter ControlID="ddlUserCountryID" DefaultValue="" 
                 Name="CountryID" PropertyName="SelectedItem.Value" Type="String" /> 
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetLocations" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [LocationID], [LocationName] FROM [Locations] Where [AccountID] = @AccountID Order By LocationName">
        <SelectParameters>
            <asp:QueryStringParameter DefaultValue="0" Name="AccountID" QueryStringField="accountID" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetGroupsByLocation" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [GroupID], [GroupName] FROM [LocationGroups] Where [LocationID] = @LocationID And  [Active] = 1 Order By GroupName">
        <SelectParameters>
            <asp:ControlParameter ControlID="ddlUserLocation" DefaultValue="" 
                 Name="LocationID" PropertyName="SelectedItem.Value" Type="String" /> 
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetRoles" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [UserRoleID], [UserRoleName] FROM [UserRoles] Where [UserRoleName] <> 'God' Order By UserRoleName">
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="GetSecurityQuestions" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
        SelectCommand="SELECT [SecurityQuestionID], [SecurityQuestionText] FROM [SecurityQuestions] Where [Active] = 1 Order By SecurityQuestionText">
    </asp:SqlDataSource>

</asp:Content>

