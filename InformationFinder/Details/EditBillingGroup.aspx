<%@ Page Title="Invoice Delivery Method" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" CodeBehind="EditBillingGroup.aspx.cs" Inherits="InformationFinder_Details_EditBillingGroup" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script src="/scripts/jquery.validate.min.js" type="text/javascript"></script>
	
     <script type="text/javascript">

         $(document).ready(function () {
             DisableEnableOtherCheckBoxByDoNotSendCheckBox();

             Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
             JQueryControlsLoad();
                              
         });

         function DisableEnableOtherCheckBoxByDoNotSendCheckBox() {
             if ($('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop("checked") == true) {  

                    <%-- $('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').val('');
                     $('#<%=txtInvDeliverySecondaryEmail.ClientID%>').val('');
                     $('#<%=txtInvDeliveryPrimaryFax.ClientID%>').val('');
                     $('#<%=txtInvDeliveryUploadInstruction.ClientID%>').val('');
                     $('#<%=fileUploadInvDeliveryUpload.ClientID%>').val('');--%>

                     <%--$('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').attr('disabled', true);
                     $('#<%=txtInvDeliverySecondaryEmail.ClientID%>').attr('disabled', true); 
                     $('#<%=txtInvDeliveryPrimaryFax.ClientID%>').attr('disabled', true);
                     $('#<%=txtInvDeliveryUploadInstruction.ClientID%>').attr('disabled', true); 
                     $('#<%=fileUploadInvDeliveryUpload.ClientID%>').attr('disabled', true);--%>

                     $('#<%=chkBoxInvDeliveryPrintMail.ClientID%>').attr('checked', false);
                     $('#<%=chkBoxInvDeliveryEmail.ClientID%>').attr('checked', false); 
                     $('#<%=chkBoxInvDeliveryFax.ClientID%>').attr('checked', false);
                     $('#<%=chkBoxInvDeliveryUpload.ClientID%>').attr('checked', false);                      

                     $('#<%=chkBoxInvDeliveryPrintMail.ClientID%>').attr('disabled', true);
                     $('#<%=chkBoxInvDeliveryEmail.ClientID%>').attr('disabled', true); 
                     $('#<%=chkBoxInvDeliveryFax.ClientID%>').attr('disabled', true);
                     $('#<%=chkBoxInvDeliveryUpload.ClientID%>').attr('disabled', true);                       

                     ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'), false);
                     ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>'), false);
                     ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>'), false);
                     ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>'), false); 
                     ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryUploadInstruction.ClientID%>'), false);
            }
            else {  
                    $('#<%=chkBoxInvDeliveryPrintMail.ClientID%>').attr('disabled', false);
                    $('#<%=chkBoxInvDeliveryEmail.ClientID%>').attr('disabled', false); 
                    $('#<%=chkBoxInvDeliveryFax.ClientID%>').attr('disabled', false);
                    $('#<%=chkBoxInvDeliveryUpload.ClientID%>').attr('disabled', false);                     
            }    
         }

         function JQueryControlsLoad() {
             // Accordion
             $("#accordion").accordion({ header: "h3",
                autoHeight: false
             });

             $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').change(function () {
                 DisableEnableOtherCheckBoxByDoNotSendCheckBox();           
             });     

             $('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').change(function () {
                 $('#<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>').attr('enableClientScript', true);                
                    ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'), true);

                    $('#<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>').attr('enableClientScript', true); 
                   ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>'), true);           
             }); 

             $('#<%=txtInvDeliverySecondaryEmail.ClientID%>').change(function () {
                 $('#<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>').attr('enableClientScript', true); 
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>'), true);           
             }); 

             $('#<%=txtInvDeliveryPrimaryFax.ClientID%>').change(function () {
                 $('#<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>').attr('enableClientScript', true);                
                    ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>'), true);             
             });

             $('#<%=txtInvDeliveryUploadInstruction.ClientID%>').change(function () {
                 $('#<%=RegularExpressionValidatorInvDeliveryUploadInstruction.ClientID%>').attr('enableClientScript', true);                   
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryUploadInstruction.ClientID%>'), true);            
             });

             $('#<%=chkBoxInvDeliveryEmail.ClientID%>').click(function () {
                 if ($(this).prop("checked") == true) {  

                    <%--$('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').attr('disabled', false);
                    $('#<%=txtInvDeliverySecondaryEmail.ClientID%>').attr('disabled', false);--%>                                                      

                   $('#<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>').attr('enableClientScript', true);                
                    ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'), true);

                    $('#<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>').attr('enableClientScript', true); 
                   ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>'), true);

                    $('#<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>').attr('enableClientScript', true); 
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>'), true);                                        
                }
                else {  
                    <%--$('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').val('');
                    $('#<%=txtInvDeliverySecondaryEmail.ClientID%>').val('');--%>

                    <%--$('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').attr('disabled', true);
                    $('#<%=txtInvDeliverySecondaryEmail.ClientID%>').attr('disabled', true);  --%>
                
                    ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'), false);
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>'), false);
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>'), false);
                }                
            });

            $('#<%=chkBoxInvDeliveryFax.ClientID%>').click(function () {
                if ($(this).prop("checked") == true) {   

                    <%--$('#<%=txtInvDeliveryPrimaryFax.ClientID%>').attr('disabled', false);--%>                                                   

                    $('#<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>').attr('enableClientScript', true);                
                    ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>'), true);                                                  
                }
                else {    
                    <%--$('#<%=txtInvDeliveryPrimaryFax.ClientID%>').val('');--%>  

                    <%--$('#<%=txtInvDeliveryPrimaryFax.ClientID%>').attr('disabled', true); --%>
            
                    ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>'), false);                    
                }                
            });

            $('#<%=chkBoxInvDeliveryUpload.ClientID%>').click(function () {
                if ($(this).prop("checked") == true) {  

                    <%--$('#<%=fileUploadInvDeliveryUpload.ClientID%>').attr('disabled', false);
                    $('#<%=txtInvDeliveryUploadInstruction.ClientID%>').attr('disabled', false);  --%> 
                
                    $('#<%=RegularExpressionValidatorInvDeliveryUploadInstruction.ClientID%>').attr('enableClientScript', true);                   
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryUploadInstruction.ClientID%>'), true);                    
                }
                else {  
                    <%--$('#<%=fileUploadInvDeliveryUpload.ClientID%>').val('');
                    $('#<%=txtInvDeliveryUploadInstruction.ClientID%>').val('');  --%>

                    <%--$('#<%=fileUploadInvDeliveryUpload.ClientID%>').attr('disabled', true);
                    $('#<%=txtInvDeliveryUploadInstruction.ClientID%>').attr('disabled', true);  --%>
                   
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryUploadInstruction.ClientID%>'), false);                    
                }                
             });

             $('#assignedAddressDialog').dialog({
				autoOpen: false,
                width: 900,
                resizable: false,
                modal: true,
                title: "Billing Address",
                open: function (type, data) {
                    $(this).parent().appendTo("form");                    
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Ok": function () {
                       $('#<%= btnAssignedAddress.ClientID %>').click();
                    },
                    "Cancel": function () {
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

         function RadioSelectAddressCheck(rb) {
			var gv = document.getElementById("<%=gv_AddressList.ClientID%>");
			var rbs = gv.getElementsByTagName("input");

			var row = rb.parentNode.parentNode;
			for (var i = 0; i < rbs.length; i++) {
				if (rbs[i].type == "radio") {
					if (rbs[i].checked && rbs[i] != rb) {
						rbs[i].checked = false;
						break;
					}
				}
			}
		}

    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
  
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <%--ASSIGN LOCATION MODAL/DIALOG--%>
    <div id="assignedAddressDialog" style="display: none;">
        <asp:UpdatePanel ID="upnlAssignedAddress" runat="server">
		    <ContentTemplate>
                <div style="overflow-x:no-display; overflow-y: scroll; height: 350px;">
                        <asp:GridView ID="gv_AddressList" runat="server" AutoGenerateColumns="False" CssClass="OTable Scroll" DataKeyNames="AddressID" >                                             
				        <Columns>                            

					        <asp:TemplateField HeaderStyle-Width ="25px" ItemStyle-HorizontalAlign="Center">                                
						        <ItemTemplate>
							        <asp:RadioButton ID="radSelectAddress" runat="server" onclick = "RadioSelectAddressCheck(this);" />							                                        
                                    <asp:Label ID="lblAddressID" runat="server" Text='<%# Eval("AddressID") %>' Visible="False" />
						        </ItemTemplate>                                                     
					        </asp:TemplateField>                                                         
					                           
					        <asp:BoundField DataField="Address1" HeaderText="Address1" SortExpression="Address1"  />
					        <asp:BoundField DataField="Address2" HeaderText="Address2" SortExpression="Address2"  />  
                            <asp:BoundField DataField="Address3" HeaderText="Address3" SortExpression="Address3" />
					        <asp:BoundField DataField="City" HeaderText="City" SortExpression="City"  /> 
                            <asp:BoundField DataField="StateAbbName" HeaderText="State" SortExpression="StateAbbName"  />
                            <asp:BoundField DataField="CountryName" HeaderText="CountryName" SortExpression="CountryName" />
					        <asp:BoundField DataField="PostalCode" HeaderText="PostalCode" SortExpression="PostalCode"   />                              
				        </Columns>
				        <EmptyDataTemplate>
					        <div class="NoData">
						        No address found.
					        </div>
				        </EmptyDataTemplate>
				        <AlternatingRowStyle CssClass="Alt" />
				        <PagerStyle CssClass="Footer" />
			        </asp:GridView> 
                </div>
                                    
                <asp:Button Text="Assign" Style="display: none;" ID="btnAssignedAddress"
			        OnClick="btnAssignedAddress_Click" runat="server" />

		    </ContentTemplate>
        </asp:UpdatePanel>       	    
    </div>
    <%--ASSIGN WEARER MODAL/DIALOG--%>

    <div style="width:100%" >
    
        <div class="FormError" id="errors" runat="server" visible="false">
		    <p><span class="MessageIcon"></span>
	            <strong>Messages:</strong>
                <span id="errorMsg" runat="server" > An error was encountered.</span>
            </p>
	    </div>

        <div>
            <asp:ValidationSummary ID="ValidationSummary1"
                    HeaderText="<span class='MessageIcon'></span><strong>You must enter a valid/required value in the following fields:</strong><br/>"
                    DisplayMode ="BulletList" 
                    EnableClientScript="true"
                    runat="server"
                    ValidationGroup="CSRegisterForm"
                    ShowSummary="true" CssClass="FormError" ForeColor="#B94A48"
                    />          
        </div>

        <div id="accordion" style="margin-top:10px;">
    
            <div>
			    <h3><a href="#">Billing Information</a></h3>                       
                <div id="billing_fields" class="OForm AccordionPadding">
                    <asp:UpdatePanel ID="upnlBillingInformation" runat="server" UpdateMode="Always">                    
                        <ContentTemplate>
                               
                            <div class="Row"> 
                                <div class="Label">BillingGroup#:</div>
                                <div class="Control" >
                                    <asp:Label ID="lblBillingGroupID" runat="server" style="padding:3px;"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label">Account#:</div>
                                <div class="Control" >
                                    <asp:Label ID="lblAccountNo" runat="server" style="padding:3px;"></asp:Label>
                                </div>
                                <div class="Clear"></div>
                            </div>

                           <div class="Row"> 
                                <div class="Label">Comapny Name<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox  ID="txtCompanyName"   runat="server" CssClass ="Size Medium" MaxLength="100"></asp:TextBox>    
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
                                    <asp:TextBox  ID="txtContactName"   runat="server" CssClass ="Size Medium" MaxLength="60"></asp:TextBox>    
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidatorContactName" runat="server" 
                                        ControlToValidate= "txtContactName" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                        ErrorMessage="Billing Contact Name" Text="Contact Name is required" >
                                    </asp:RequiredFieldValidator>              
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label">PO#:</div>
                                <div class="Control">
                                    <asp:TextBox  ID="txtPOno"   runat="server" CssClass ="Size Small" MaxLength="50"></asp:TextBox>                                                     
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label"></div>
                                <div class="Control">
                                    <asp:Button ID="btnExistingAddress" CssClass="OButton" runat="server" Text="Select Existing Address" OnClick="btnExistingAddress_Click" />              
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label">Address<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox  ID="txtAddress1B"   runat="server" CssClass ="Size XLarge" MaxLength="255" ></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="reqfldvalAddress1B" runat="server" 
                                        ControlToValidate= "txtAddress1B" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                        ErrorMessage="Billing Address" Text="Address is required">
                                    </asp:RequiredFieldValidator>                   
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label"></div>
                                <div class="Control"> <asp:TextBox  ID="txtAddress2B" runat="server" CssClass ="Size XLarge" MaxLength="255" ></asp:TextBox></div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label"></div>
                                <div class="Control"> <asp:TextBox  ID="txtAddress3B"  runat="server" CssClass ="Size XLarge" MaxLength="255" ></asp:TextBox></div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label">Country<span class="Required">*</span>:</div>
                                <div class="Control"> 
                                    <asp:DropDownList ID="ddlCountryB" runat="server"  
                                    DataValueField="CountryID" DataTextField="CountryName" AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlCountryOnSelectedIndexChange" /> 
                                    <asp:RequiredFieldValidator runat="server" ID="reqfldvalCountryB" ControlToValidate="ddlCountryB"
                                        ErrorMessage="Billing Country" Text="Country is required" Display="Dynamic" InitialValue="0" ValidationGroup="CSRegisterForm">
                                    </asp:RequiredFieldValidator>                        
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row"> 
                                <div class="Label">City<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox  ID="txtCityB" runat="server" CssClass ="Size Large2" MaxLength="100"></asp:TextBox> 
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
                                    <asp:DropDownList ID="ddlStateB" runat="server" 
                                        DataValueField="StateID" DataTextField="StateAbbName" /> 
                                
                                    <asp:TextBox  ID="txtPostalB"   runat="server" CssClass ="Size Small" MaxLength="15"></asp:TextBox>   

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
                
                        </ContentTemplate>  
                    </asp:UpdatePanel>         
                </div>                                               
            </div>

            <div >
			    <h3><a href="#">Invoice Delivery Method</a></h3>
                <div class="OForm AccordionPadding">             
                     <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>                                

                         <table style="width:100%;border:0;" cellpadding="0" cellspacing="0" class="OTable">
                            <tr id ="rowPrintMail" runat="server">
                                <td style="width:15%;" class="Label">
                                    Print & Mail:
                                </td>
                                <td style="width:3%;">
                                    <asp:CheckBox ID="chkBoxInvDeliveryPrintMail" runat="server" Text="" />
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
                                    <asp:CheckBox ID="chkBoxInvDeliveryEmail" runat="server" Text="" />
                                </td>
                                <td >
                                    Primary Email:                                
                                </td>
                                <td >   
                                    <asp:TextBox  ID="txtInvDeliveryPrimaryEmail" runat="server" MaxLength="60" width="250" Enabled="true"></asp:TextBox>   
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
                                    <asp:TextBox  ID="txtInvDeliverySecondaryEmail" runat="server" MaxLength="60" width="250" Enabled="true"></asp:TextBox>
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
                                    <asp:CheckBox ID="chkBoxInvDeliveryFax" runat="server" Text="" />
                                </td>
                                <td>
                                    Primary Fax:
                                </td>
                                <td>  
                                    <asp:TextBox  ID="txtInvDeliveryPrimaryFax" runat="server" CssClass ="Size Medium2" MaxLength="24" Enabled="true"></asp:TextBox>
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
                                    <asp:CheckBox ID="chkBoxInvDeliveryUpload" runat="server" Text="" />
                                </td>
                                <td>  
                                    Instruction File:
                                </td>
                                <td>   
                                    <asp:FileUpload ID="fileUploadInvDeliveryUpload" runat="server" Enabled="true" />
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
                                    <asp:TextBox  ID="txtInvDeliveryUploadInstruction" TextMode="MultiLine" Height="50" Width="400" MaxLength="1000" runat="server" Enabled="true"></asp:TextBox>
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
                                    <asp:CheckBox ID="chkBoxInvDeliveryDoNotSend" runat="server" Text="" />
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
                <asp:Button ID="btnSave" CssClass="OButton" runat="server" Text="Save" onclick="btnSave_Click" ValidationGroup="CSRegisterForm" />
                <asp:Button ID="btnCancel" CssClass="OButton" runat="server" Text="Back to Account" onclick="btnCancel_Click" />
            </div>
            <div class="Clear"> </div>
        </div>
                     
      </div>
</asp:Content>
