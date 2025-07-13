<%@ Page Title="Add/Edit Account" Language="C#" EnableEventValidation="false" 
    MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" 
    Inherits="CustomerService_CreateAccount" Codebehind="CreateAccount.aspx.cs" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="act" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        .OForm .Row .Label {
            width:230px;
        }
        #divModalDiscount .OForm .Row .Label {
           width: 120px;
        }
    </style>
    <script type="text/javascript" >

        $(document).ready(function () {                        
            DisableEnableOtherCheckBoxByDoNotSendCheckBox();
            //accountTypeChanged();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ajaxLoad);
            ajaxLoad();

            doNotSendInvoiceDeliveryConfirmation();
        });

       function doNotSendInvoiceDeliveryConfirmation() {
            $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').on("click", function () {

                var customerGroupDDL = $('#<%=ddlCustomerGroup.ClientID%>');
                var customerTypeDDL = $('#<%=ddlCustomerType.ClientID%>');
                
                if ($('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop("checked") == true && ((customerTypeDDL.val() != "3" && customerTypeDDL.val() != "4") && (customerGroupDDL.val() != "3" && customerGroupDDL.val() != "4"))) {
                   
                  var message = "<div><p>Are you sure you want to set \"Do Not Send\" invoice ON for this account?</p></div>";

                 $("#donotsend-message").html(message);
                 $("#confirm-donotsend-dialog").dialog({
                    width: 300,
                    resizable: false,
                    title: "Do Not Send invoice",
                    modal: true,
                    open: function (type, data) {

                    },
                    close: function () {
                        $('.ui-overlay').fadeOut();
                    },
                    buttons: {
                        "OK, Continue": function () {        
                            $(this).dialog("close");
                        },
                        "Cancel": function () {
                             $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop("checked", false);
                             $(this).dialog("close");
                         }
                     }
                 });

                }
            });

        }

        
        function DisableEnableOtherCheckBoxByDoNotSendCheckBox() {            

            if ($('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop("checked") == true) {  
                     
                <%--$('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').prop('disabled', true);
                $('#<%=txtInvDeliverySecondaryEmail.ClientID%>').prop('disabled', true); 
                $('#<%=txtInvDeliveryPrimaryFax.ClientID%>').prop('disabled', true);
                $('#<%=txtInvDeliveryUploadInstruction.ClientID%>').prop('disabled', true); 
                $('#<%=fileUploadInvDeliveryUpload.ClientID%>').prop('disabled', true);--%>

                $('#<%=chkBoxInvDeliveryPrintMail.ClientID%>').prop('checked', false);
                $('#<%=chkBoxInvDeliveryEmail.ClientID%>').prop('checked', false); 
                $('#<%=chkBoxInvDeliveryFax.ClientID%>').prop('checked', false);
                $('#<%=chkBoxInvDeliveryUpload.ClientID%>').prop('checked', false);                       

                $('#<%=chkBoxInvDeliveryPrintMail.ClientID%>').prop('disabled', true);
                $('#<%=chkBoxInvDeliveryEmail.ClientID%>').prop('disabled', true); 
                $('#<%=chkBoxInvDeliveryFax.ClientID%>').prop('disabled', true);
                $('#<%=chkBoxInvDeliveryUpload.ClientID%>').prop('disabled', true);                                     

                ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'), false);
                ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>'), false);
                ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>'), false);
                ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>'), false); 
                ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryUploadInstruction.ClientID%>'), false);  
            } else {  
                $('#<%=chkBoxInvDeliveryPrintMail.ClientID%>').prop('disabled', false);
                $('#<%=chkBoxInvDeliveryEmail.ClientID%>').prop('disabled', false); 
                $('#<%=chkBoxInvDeliveryFax.ClientID%>').prop('disabled', false);
                $('#<%=chkBoxInvDeliveryUpload.ClientID%>').prop('disabled', false);      

                // Resolve the issue of chkBoxInvDeliveryDoNotSend is returned disable automatically when hitting save button with all Invoice Delivery Method checkboxes are unchecked.
                var customerGroupDDL = $('#<%=ddlCustomerGroup.ClientID%>'); 
                var customerTypeDDL = $('#<%=ddlCustomerType.ClientID%>');                 

               var isDoNotSendRestricted = <%= IsDoNotSendRestricted.ToString().ToLower() %>;

                if (isDoNotSendRestricted) {
                    $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop('disabled', false);
                }
                else {
                    $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop('disabled', true);
                }    
            }    
        }

        function ajaxLoad() {
            // Accordion
            $("#accordion").accordion({ header: "h3",
                autoHeight: false
            });

            // UserBirthDate Datepicker
            $('#<%=txt_ServiceStartDate.ClientID %>').datepicker();

            // LastReminded Datepicker
            $('#<%=txt_ServiceEndDate.ClientID %>').datepicker();

            // PO Start Date Datepicker
            $('#<%=txtPOBeginDate.ClientID %>').datepicker();

            // PO End Date Datepicker
            $('#<%=txtPOEndDate.ClientID %>').datepicker();

            //when the checkbox is checked or unchecked
            //            $('#copyaddress').click(function() {
            

            if($('#<%=ddlBrand.ClientID%>').val() == 3) {
                $('#rowEDI').show();
            }
            else {
                $('#rowEDI').hide();
            }  

           <%-- $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').change(function () {
                 DisableEnableOtherCheckBoxByDoNotSendCheckBox();           
            });--%>

            $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').click(function () {
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

            $('#<%=ddlCustomerType.ClientID%>').change(function () {   
                var customerGroupDDL = $('#<%=ddlCustomerGroup.ClientID%>');                
                if (this.value == "3" || this.value == "4" || customerGroupDDL.val() == "3" || customerGroupDDL.val() == "4") {                          
                    $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop('checked',true);                                   
                    $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop('disabled', false);
                    
                    DisableEnableOtherCheckBoxByDoNotSendCheckBox();
                } 
                else {                                  
                    $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop('checked',false);                    
                    $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop('disabled', true);
                    
                    DisableEnableOtherCheckBoxByDoNotSendCheckBox();
                    $('#<%=chkBoxInvDeliveryPrintMail.ClientID%>').prop('checked',true);
                }
            });   

            $('#<%=ddlCustomerGroup.ClientID%>').change(function () {   
                var customerTypeDDL = $('#<%=ddlCustomerType.ClientID%>');                     
                if (this.value == "3" || this.value == "4" || customerTypeDDL.val() == "3" || customerTypeDDL.val() == "4") {                          
                    $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop('checked',true);                                   
                    $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop('disabled', false);
                    
                    DisableEnableOtherCheckBoxByDoNotSendCheckBox();                    
                } 
                else {                                  
                    $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop('checked',false);                    
                    $('#<%=chkBoxInvDeliveryDoNotSend.ClientID%>').prop('disabled', true);
                    
                    DisableEnableOtherCheckBoxByDoNotSendCheckBox();                    
                    $('#<%=chkBoxInvDeliveryPrintMail.ClientID%>').prop('checked',true);
                }
            });   

            $('#<%=chkBoxInvDeliveryEmail.ClientID%>').click(function () {
                if ($(this).prop("checked") == true) {  
                    setInvoiceDeliveryMethodEmail();
                    <%--$('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').val($('#<%=txtEmailAddressB.ClientID%>').val())--%>       

                   <%--$('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').prop('disabled', false);
                    $('#<%=txtInvDeliverySecondaryEmail.ClientID%>').prop('disabled', false);--%>                     

                   <%--$('#<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>').attr('enableClientScript', true);                
                    ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'), true);

                    $('#<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>').attr('enableClientScript', true); 
                   ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>'), true);

                    $('#<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>').attr('enableClientScript', true); 
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>'), true);--%>      

                    <%-- ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'), true);
                    document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>').isvalid = true;
                   ValidatorUpdateDisplay(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'));--%>
                }
                else {                      
                    <%--$('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').prop('disabled', true);
                    $('#<%=txtInvDeliverySecondaryEmail.ClientID%>').prop('disabled', true); --%>                  
                
                    ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'), false);
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>'), false);
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>'), false);
                }                
            });

            $('#<%=txtEmailAddressB.ClientID%>').on('change', function () {
                if ($('#<%=chkBoxInvDeliveryEmail.ClientID%>').prop('checked')) {
                    setInvoiceDeliveryMethodEmail();
                }
            });

            $('#<%=chkBoxInvDeliveryFax.ClientID%>').click(function () {
                if ($(this).prop("checked") == true) {  
                    $('#<%=txtInvDeliveryPrimaryFax.ClientID%>').val($('#<%=txtFaxB.ClientID%>').val()) 
                    
                    <%--$('#<%=txtInvDeliveryPrimaryFax.ClientID%>').attr('disabled', false);   --%>                                             

                    $('#<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>').attr('enableClientScript', true);                
                    ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>'), true);   

                    <%-- ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>'), true);
                    document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>').isvalid = true;
                    ValidatorUpdateDisplay(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>'));--%>
                }
                else {                        
                   <%-- $('#<%=txtInvDeliveryPrimaryFax.ClientID%>').attr('disabled', true); --%>
            
                    ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryFax.ClientID%>'), false);                    
                }                
            });

            $('#<%=chkBoxInvDeliveryUpload.ClientID%>').click(function () {
               if($(this).prop("checked") == true){  
                    <%--$('#<%=fileUploadInvDeliveryUpload.ClientID%>').attr('disabled', false);
                    $('#<%=txtInvDeliveryUploadInstruction.ClientID%>').attr('disabled', false); --%>  
                
                    $('#<%=RegularExpressionValidatorInvDeliveryUploadInstruction.ClientID%>').attr('enableClientScript', true);                   
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryUploadInstruction.ClientID%>'), true);                    
                }
                else {                      
                    <%--$('#<%=fileUploadInvDeliveryUpload.ClientID%>').attr('disabled', true);
                    $('#<%=txtInvDeliveryUploadInstruction.ClientID%>').attr('disabled', true);--%>  
                   
                    ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryUploadInstruction.ClientID%>'), false);                    
                }                
            });

            $('#<%=ckbxCopyCompany.ClientID%>').click(function () {
                $('#<%=txtCompanyName.ClientID%>').val($('#<%=txtAccountName.ClientID%>').val())
                $('#<%=txtCompanyNameB.ClientID%>').val($('#<%=txtAccountName.ClientID%>').val())
                $('#<%=txtCompanyNameS.ClientID%>').val($('#<%=txtAccountName.ClientID%>').val())
            });

            // Modal/Dialog for Adding Rate Code.
            $('#divModalDiscount').dialog({
                modal: true,
                autoOpen: false,
                width: 400,
                height: 300,
                resizable: false,
                title: "Account Discount Details",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                    $('#<%= ddlDiscountProductGroup.ClientID %>').focus();
                },
                buttons: {
                    "Calculate": function () {
                        $('#<%= btnCalculateDiscount.ClientID %>').click();
                     },
                    "Save": function () {
                        $('#<%= btnDiscountSave.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnDiscountCancel.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });

           

            // Modal/Dialog for Editing or Deactivating Rate Code.
            $('#divModalEditDiscount').dialog({
                autoOpen: false,
                width: 500,
                height: 350,
                resizable: false,
                title: "Edit Rate Details",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Delete": function () {
                        $('#<%= btnDeactivate.ClientID %>').click();
                    },
                    "Save": function () {
                        $('#<%= btnEdit.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<= btnCancel_Edit.ClientID >').click();
                    $('.ui-overlay').fadeOut();
                }
            });

            //disable RegularExpressionValidator1 when Activate/Reactivate account SCTASK0024093
            $('#ctl00_primaryHolder_btnDeactivate').click(function () {
                ValidatorEnable(document.getElementById('ctl00_primaryHolder_RegularExpressionValidator1'), false);
            });
        }

       
        function openDialog(id) {
            $('.ui-overlay').fadeIn();
            $('#' + id).dialog("open");
        }

        function closeDialog(id) {
            $('#' + id).dialog("close");
        }

        function accountTypeChanged() {
            var accountTypeId = $('#ddlAccountType').val();
            var customerTypeDDL = $('#<%=ddlCustomerType.ClientID%>');

            // if account type is Distributor (ID - 2), change customer type to DIST and disable the customer type select box
            if (accountTypeId == 2) {
                disableConsigmentCustomerTypeOption(true);
                customerTypeDDL.val("8").prop('disabled', true);
            } else if (accountTypeId == 3) {
                disableConsigmentCustomerTypeOption(false);
                // if account type is Consignment (ID - 3), change customer type to CON and disable the customer type select box 
                customerTypeDDL.val("47").prop('disabled', true);
            } else {
                customerTypeDDL.prop('disabled', false);
                var customerTypeDDLValue = customerTypeDDL.val();
                disableConsigmentCustomerTypeOption(true);
                //if (customerTypeDDLValue === "8" || customerTypeDDLValue === "47") {
                //    customerTypeDDL.val("0");
                //}
            }

            function disableConsigmentCustomerTypeOption(disable) {
               customerTypeDDL.children("[value='47']").prop("disabled", disable);
            }
            
            customerTypeChanged();
        }

        function enableValidator(id, enable) {
            var validator = document.getElementById(id);

            if (validator) {
                ValidatorEnable(validator, enable);
            }
        }

        // if consignment account, make the following changes for consignment
        function customerTypeChanged() {            

            // consignment customer type
            if ($('#<%=ddlCustomerType.ClientID%>').val() == '47')
            {
                $('#<%=ddlBrand.ClientID%>').val('2');   // mirion
                $('#<%=ddlReferral.ClientID%>').val('0000');   // house account

                $('#<%=divMaxIDPlusInventoryLimit.ClientID%>').show();
                $('#<%=lMaxIDPlusInventoryLimit.ClientID%>').show();
                $('#<%=txtMaxIDPlusInventoryLimit.ClientID%>').show();
                $('#<%=RequiredFieldValidatorMaxIDPlusInventory.ClientID%>').attr('enableClientScript', true);
                $('#<%=RegularExpressionValidatorMaxIDPlusInventory.ClientID%>').attr('enableClientScript', true);
                enableValidator('<%=RequiredFieldValidatorMaxIDPlusInventory.ClientID%>', true);
                enableValidator('<%=RegularExpressionValidatorMaxIDPlusInventory.ClientID%>', true);

                $('#<%=divMaxIDLinkInventoryLimit.ClientID%>').show();
                $('#<%=lMaxIDLinkInventoryLimit.ClientID%>').show();
                $('#<%=txtMaxIDLinkInventoryLimit.ClientID%>').show();
                $('#<%=RequiredFieldValidatorMaxIDLinkInventory.ClientID%>').attr('enableClientScript', true);
                $('#<%=RegularExpressionValidatorMaxIDLinkInventory.ClientID%>').attr('enableClientScript', true);
                enableValidator('<%=RequiredFieldValidatorMaxIDLinkInventory.ClientID%>', true);
                enableValidator('<%=RegularExpressionValidatorMaxIDLinkInventory.ClientID%>', true);
                
                $('#<%=divMaxIDLinkUSBInventoryLimit.ClientID%>').show();
                $('#<%=lMaxIDLinkUSBInventoryLimit.ClientID%>').show();
                $('#<%=txtMaxIDLinkUSBInventoryLimit.ClientID%>').show();
                $('#<%=RequiredFieldValidatorMaxIDLinkUSBInventory.ClientID%>').attr('enableClientScript', true);
                $('#<%=RegularExpressionValidatorMaxIDLinkUSBInventory.ClientID%>').attr('enableClientScript', true);
                enableValidator('<%=RequiredFieldValidatorMaxIDLinkUSBInventory.ClientID%>', true);
                enableValidator('<%=RegularExpressionValidatorMaxIDLinkUSBInventory.ClientID%>', true);

                $('#<%=divMaxID1InventoryLimit.ClientID%>').show();
                $('#<%=lMaxID1InventoryLimit.ClientID%>').show();
                $('#<%=txtMaxID1InventoryLimit.ClientID%>').show();
                $('#<%=RequiredFieldValidatorMaxID1Inventory.ClientID%>').attr('enableClientScript', true);
                $('#<%=RegularExpressionValidatorMaxID1Inventory.ClientID%>').attr('enableClientScript', true);
                enableValidator('<%=RequiredFieldValidatorMaxID1Inventory.ClientID%>', true);
                enableValidator('<%=RegularExpressionValidatorMaxID1Inventory.ClientID%>', true);

                $('#<%=lMaxID2InventoryLimit.ClientID%>').show();
                $('#<%=txtMaxID2InventoryLimit.ClientID%>').show();
                $('#<%=RequiredFieldValidatorMaxID2Inventory.ClientID%>').attr('enableClientScript', true);
                $('#<%=RegularExpressionValidatorMaxID2Inventory.ClientID%>').attr('enableClientScript', true);
                enableValidator('<%=RequiredFieldValidatorMaxID2Inventory.ClientID%>', true);
                enableValidator('<%=RegularExpressionValidatorMaxID2Inventory.ClientID%>', true);

                // remove discount
                $('#<%=btnAddDiscount.ClientID%>').hide();
                <%--$('#<%=udtpnlRateDetailsGridView.ClientID%>tl00_primaryHolder_').hide();--%>
                $('#<%=udtpnlRateDetailsGridView.ClientID%>').hide();
                $('#<%=divAddAndSearchToolbar.ClientID%>').hide();
                $('#<%=ddlReferral.ClientID%>').attr('disabled', true);                               
                $('#<%=ddlBrand.ClientID%>').attr('disabled', true);
            }
            else
            {        
                $('#<%=udtpnlRateDetailsGridView.ClientID%>').show();
                $('#<%=divAddAndSearchToolbar.ClientID%>').show();
                $('#<%=btnAddDiscount.ClientID%>').show();
                $('#<%=ddlReferral.ClientID%>').attr('disabled', false);   
                
                // for update account, do not enable ddlBrand
                var urlQueryParams = (new URL(document.location)).searchParams;
                if (!urlQueryParams.has('ID'))
                    $('#<%=ddlBrand.ClientID%>').attr('disabled', false);

                $('#<%=divMaxIDPlusInventoryLimit.ClientID%>').hide();
                $('#<%=lMaxIDPlusInventoryLimit.ClientID%>').hide();
                $('#<%=txtMaxIDPlusInventoryLimit.ClientID%>').hide();
                enableValidator('<%=RequiredFieldValidatorMaxIDPlusInventory.ClientID%>', false);
                enableValidator('<%=RegularExpressionValidatorMaxIDPlusInventory.ClientID%>', false);

                $('#<%=divMaxIDLinkInventoryLimit.ClientID%>').hide();
                $('#<%=lMaxIDLinkInventoryLimit.ClientID%>').hide();
                $('#<%=txtMaxIDLinkInventoryLimit.ClientID%>').hide();
                enableValidator('<%=RequiredFieldValidatorMaxIDLinkInventory.ClientID%>', false);
                enableValidator('<%=RegularExpressionValidatorMaxIDLinkInventory.ClientID%>', false);

                $('#<%=divMaxIDLinkUSBInventoryLimit.ClientID%>').hide();
                $('#<%=lMaxIDLinkUSBInventoryLimit.ClientID%>').hide();
                $('#<%=txtMaxIDLinkUSBInventoryLimit.ClientID%>').hide();
                enableValidator('<%=RequiredFieldValidatorMaxIDLinkUSBInventory.ClientID%>', false);
                enableValidator('<%=RegularExpressionValidatorMaxIDLinkUSBInventory.ClientID%>', false);

                $('#<%=divMaxID1InventoryLimit.ClientID%>').hide();
                $('#<%=lMaxID1InventoryLimit.ClientID%>').hide();
                $('#<%=txtMaxID1InventoryLimit.ClientID%>').hide();
                enableValidator('<%=RequiredFieldValidatorMaxID1Inventory.ClientID%>', false);
                enableValidator('<%=RegularExpressionValidatorMaxID1Inventory.ClientID%>', false);

                $('#<%=lMaxID2InventoryLimit.ClientID%>').hide();
                $('#<%=txtMaxID2InventoryLimit.ClientID%>').hide();
                enableValidator('<%=RequiredFieldValidatorMaxID2Inventory.ClientID%>', false);
                enableValidator('<%=RegularExpressionValidatorMaxID2Inventory.ClientID%>', false);
            }            
        };

        function setInvoiceDeliveryMethodEmail() {
            $('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').val($('#<%=txtEmailAddressB.ClientID%>').val())

            <%--$('#<%=txtInvDeliveryPrimaryEmail.ClientID%>').prop('disabled', false);
            $('#<%=txtInvDeliverySecondaryEmail.ClientID%>').prop('disabled', false);--%>                     

            $('#<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>').attr('enableClientScript', true);                
            ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'), true);

            $('#<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>').attr('enableClientScript', true); 
            ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliveryPrimaryEmail.ClientID%>'), true);

            $('#<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>').attr('enableClientScript', true); 
            ValidatorEnable(document.getElementById('<%=RegularExpressionValidatorInvDeliverySecondaryEmail.ClientID%>'), true);      

            <%-- ValidatorEnable(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'), true);
            document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>').isvalid = true;
            ValidatorUpdateDisplay(document.getElementById('<%=RequiredFieldValidatorInvDeliveryPrimaryEmail.ClientID%>'));--%>
        }

    </script>  

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:toolkitscriptmanager ID="ToolkitScriptManager1" runat="server">
    </act:toolkitscriptmanager>
 
     <%--//////////////////////////////////// Modal/Dialog - Add Rate Code ////////////////////////////////////////--%>
    <div id="divModalDiscount">
        <asp:UpdatePanel ID="udtpnlDiscount" runat="server">
            <ContentTemplate>
                <div class="FormError" id="discountDialogErrors" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
					<strong>Messages:</strong><span id="discountDialogErrorMsg" runat="server">An error was encountered.</span></p>
				</div>
                <div class="OForm">
                    
                    <div class="Row">
                        <div class="Label">Rate:</div>
                        <div class="Control">
                             <div class="LabelValue">
                                <asp:Label ID="lblRateCode" runat="server"></asp:Label> 
                            </div>
                              
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Product Group:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlDiscountProductGroup" runat="server">
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator runat="server" ID="reqfldValidProdGroup" 
                                ControlToValidate="ddlDiscountProductGroup"
                                ErrorMessage="product Group" Text="Product Group is required" Display="Dynamic"
                                InitialValue="0" ValidationGroup="CSRegisterForm">
                            </asp:RequiredFieldValidator>
                            <asp:HiddenField ID="hidProductGroupID" runat="server" />
                            <asp:HiddenField ID="hidMode" runat="server" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Initial. Qty.<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtInitialQty" runat="server" CssClass="Size Small" TabIndex="1" ValidationGroup="ADD"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqfldvalAddMinimumQuantity" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="txtInitialQty" ValidationGroup="ADD"></asp:RequiredFieldValidator>
                            <asp:CompareValidator ID="cmpvalAddMinimumQuantity" runat="server" Operator="DataTypeCheck" Type="Integer" CssClass="InlineError" 
                            ControlToValidate="txtInitialQty" ErrorMessage="Invalid input." Display="Dynamic" ValidationGroup="ADD"/>
                        </div>
                        <div class="Clear"></div>
                    </div>
                       <div class="Row">
                        <div class="Label">Price<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtTargetPrice" runat="server" CssClass="Size Small" TabIndex="4" ValidationGroup="ADD"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqfldvalAddPrice" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="txtTargetPrice" ValidationGroup="ADD"></asp:RequiredFieldValidator>
                            <asp:RangeValidator ID="rngvalAddPrice" runat="server" ErrorMessage="Invalid input." Display="Dynamic" CssClass="InlineError" 
                            MinimumValue="0.01" MaximumValue="999999999999.00" ControlToValidate="txtTargetPrice" ValidationGroup="ADD" Type="Currency"></asp:RangeValidator>
                        </div>
                        <div class="Control">
                            <asp:Label ID="lblSpecificCurrencyCode_Add" runat="server" CssClass="StaticLabel"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">                       
                    <div class="Label">Discount %:</div>
                    <div class="Control"> 
                        <asp:TextBox  ID="txtDiscount" Text="0.00" 
                            runat="server" MaxLength="5" CssClass="Size XXSmall" OnTextChanged="txtDiscount_TextChanged" ></asp:TextBox> 
                        <asp:RegularExpressionValidator id="RegularExpressionValidator2" 
                             MinimumValue="0.01" MaximumValue="100.00"
                                    ControlToValidate="txtDiscount"
                                    ValidationExpression="^[-+]?\d*\.?\d*$"
                                    Display="Dynamic"
                                    ErrorMessage="Discount is numerics only." Text="Numerics only"
                                    runat="server"/> 
                    </div>
                    <div class="Clear"></div>
                </div>
                     <asp:Button ID="btnCalculateDiscount" runat="server" Text="Calculate Discount" CommandName="Add" 
                                ValidationGroup="ADD" style="display:none;" TabIndex="5"
                                 OnClick="btnCalculateDiscount_Click"></asp:Button>
                    <asp:Button ID="btnDiscountSave" runat="server" Text="Save" CommandName="Add" 
                                ValidationGroup="ADD" style="display:none;" TabIndex="5"
                                 OnClick="btnDiscountSave_Click"></asp:Button>
                    <asp:Button ID="btnDiscountCancel" runat="server" Text="Cancel" style="display:none;" TabIndex="6" 
                                OnClick="btnDiscountCancel_Click"></asp:Button>
                </div>

                <asp:Label ID="lblErrorMessageModal" runat="server" CssClass="InlineError" ForeColor="#FF0000" Font-Bold="true"></asp:Label>
             
		    </ContentTemplate>
        </asp:UpdatePanel>
    </div>
     <div id="divModalEditDiscount">
        <asp:UpdatePanel ID="udtpnlEditDeactivateRateDetails" runat="server">
            <ContentTemplate>
                <asp:HiddenField ID="hdnfldRateDetailID_Edit" runat="server" />
                <div class="OForm">
                    <div class="Row">
                        <div class="Label">Brand:</div>
                        <div class="Control">
                            <asp:Label ID="lblEditBrandSource" runat="server" CssClass="StaticLabel"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Code:</div>
                        <div class="Control">
                            <asp:Label ID="lblEditRateCode" runat="server" CssClass="StaticLabel"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Product:</div>
                        <div class="Control">
                            <asp:Label ID="lblEditProductGroup" runat="server" CssClass="StaticLabel"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <div class="Row">
                        <div class="Label">Price<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox ID="txtEditPrice" runat="server" CssClass="Size Small" TabIndex="10" ValidationGroup="EDIT"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="reqfldvalEditPrice" runat="server" CssClass="InlineError" Display="Dynamic" 
                            ErrorMessage="Required" ControlToValidate="txtEditPrice" ValidationGroup="EDIT"></asp:RequiredFieldValidator>
                            <asp:RangeValidator ID="rngvalEditPrice" runat="server" ErrorMessage="Invalid input." Display="Dynamic" CssClass="InlineError" 
                            MinimumValue="0.01" MaximumValue="999999999999.00" ControlToValidate="txtEditPrice" ValidationGroup="EDIT" Type="Double"></asp:RangeValidator>
                        </div>
                        <div class="Control">
                            <asp:Label ID="lblSpecificCurrencyCode_Edit" runat="server" CssClass="StaticLabel"></asp:Label>
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <asp:Button ID="Button2" runat="server" Text="Delete" CommandName="Delete" style="display:none;" TabIndex="11" OnClick="btnDeactivate_Click"></asp:Button>
                    <asp:Button ID="btnEdit" runat="server" Text="Save" CommandName="Update" ValidationGroup="EDIT" style="display:none;"
                                 TabIndex="12" OnClick="btnEdit_Click"></asp:Button>
                    <asp:Button ID="btnCancel_Edit" runat="server" Text="Cancel" style="display:none;" TabIndex="13"></asp:Button>
                </div>
               
		    </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <div id="confirm-donotsend-dialog" style="display: none;">
        <div id="donotsend-message">
        </div>
     </div>
    
<div runat="server" id="divMainForm" style="100%">
    
    <div class="FormError" id="error" runat="server">
		<p><span class="MessageIcon"></span>
	        <strong>Messages:</strong>
            <span id="errorMsg" runat="server" >               
            </span>
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

    <div id="AccountToolBar" class="OToolbar" runat="server">
		<ul>			
            <li>
                <asp:LinkButton Text="Deactivate" ID="btnDeactivate" onclick="btnDeactivate_Click" CssClass="Icon Remove"  runat="server" />
            </li>
		</ul>
	</div>        
    
    <div id="accordion" style="margin-top:10px;">
    
        <div >
			<h3><a href="#">Account Information</a></h3>
            <div class="OForm AccordionPadding">
                <asp:UpdatePanel ID="upnlAccountInformation" runat="server" UpdateMode="Conditional">
                
                    <ContentTemplate> 

                    <div class="Row">
                        <div class="Label">Brand Source:</div>
                        <div class="Control">
                            <asp:DropDownList runat="server" ID="ddlBrand" AutoPostBack="true"
                                onselectedindexchanged="ddlBrand_SelectedIndexChanged">
                                <asp:ListItem Text="IC Care" Value="3" />
                                <asp:ListItem Text="Mirion" Value="2"  Selected="True" />
                            </asp:DropDownList>                    
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Account Type<span class="Required">*</span></div>
                        <div class="Control">
                            <asp:DropDownList runat="server" ID="ddlAccountType" ClientIDMode="Static" onchange="javascript:accountTypeChanged()"></asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Customer Group<span class="Required">*</span></div>
                        <div class="Control">
                            <asp:DropDownList runat="server" ID="ddlCustomerGroup" ></asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Customer Type<span class="Required">*</span>:</div>
                        <div class="Control"> 
                            <asp:DropDownList ID="ddlCustomerType" runat="server"  
                                DataValueField="CustomerTypeID" DataTextField="CustomerTypeCode" onchange="javascript:customerTypeChanged()" > 
	                        </asp:DropDownList> 
                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator25" ControlToValidate="ddlCustomerType"
                                ErrorMessage="Customer Type" Text="Customer Type is required" Display="Dynamic" InitialValue="0" ValidationGroup="CSRegisterForm">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row" id="divICCareSalesRepCommission" runat="server" visible="false">
                        <div class="Label"></div>
                        <div class="Control">
                            <asp:CheckBox ID="chkICCareSalesRepCommission" runat="server" Text="Eligible for Independent Sales Rep Commission" />
                        </div>
                         <div class="Clear"></div>
                    </div>

                    <div class="Row" id="divICCareDemoAccount" runat="server" visible="false">
                        <div class="Label"></div>
                        <div class="Control">
                            <asp:CheckBox ID="chkICCareDemoAccount" runat="server" Text="IC Care Demo Account" />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div id="dealerRow" runat="server" class="Row" visible="false" >
                        <div class="Label"><asp:Label ID="lblDealer" runat="server"  Text= "Dealer"/>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlDealer" runat="server"   
                                DataValueField="DealerID" DataTextField="DealerIDName" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlDealerOnSelectedIndexChange" />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Account Name<span class="Required">*</span>:</div>
                        <div class="Control"> 
                            <asp:TextBox  ID="txtAccountName" runat="server" MaxLength="60" CssClass="Size Large"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
                                ControlToValidate= "txtAccountName" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                ErrorMessage="Account Name" Text="Account Name is required">
                            </asp:RequiredFieldValidator>                            
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label"></div>
                        <div class="Control">
                            <asp:CheckBox ID="ckbxCopyCompany" runat="server"  Text="Fill all Company fields" />         
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Company<span class="Required">*</span>:</div>
                        <div class="Control"> 
                            <asp:TextBox  ID="txtCompanyName" runat="server" MaxLength="60" CssClass="Size Large" ></asp:TextBox> 
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" 
                                ControlToValidate= "txtCompanyName" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                ErrorMessage="Company" Text="Company is required">
                            </asp:RequiredFieldValidator>  
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Referral<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlReferral" runat="server"  AutoPostBack="true" OnSelectedIndexChanged="ddlReferral_SelectedIndexChanged"
                                DataValueField="SalesRepDistID" DataTextField="SalesCompanyName">
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator22" ControlToValidate="ddlReferral"
                                ErrorMessage="Referral" Text="Referral is required" Display="Dynamic" InitialValue="0" ValidationGroup="CSRegisterForm">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Sales Rep:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlSalesRep" runat="server"></asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Market<span class="Required">*</span>:
                        </div>
                        <div class="Control" > 
                           <%-- <asp:DropDownList ID="ddlUnixCustomerType" runat="server"  
                                DataValueField="UnixCustomerTypeID" DataTextField="UnixCustomerDescription" >
	                        </asp:DropDownList> --%>

                            <asp:DropDownList ID="ddlIndustryType" runat="server"  
                                DataValueField="IndustryID" DataTextField="IndustryName"  >
	                        </asp:DropDownList>

                            <asp:RequiredFieldValidator ID="RequiredFieldValidator34" runat="server" ErrorMessage="Account Type" ControlToValidate="ddlIndustryType" InitialValue="0" ValidationGroup="CSRegisterForm">Account Type is required</asp:RequiredFieldValidator>

                            <asp:CheckBox ID="chkUseLocationBilling" runat="server" Enabled="false" Visible="false"
                               Text="Use Location Billing" />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Service Start & End<span class="Required">*</span>:</div>
                        <div  class="Control">
                            <asp:TextBox ID="txt_ServiceStartDate" runat="server" 
                                AutoPostBack ="true" CssClass="Size Small" 
                                OnTextChanged="txt_ServiceStartDate_TextChanged"></asp:TextBox>                                        
                            TO
                            <asp:TextBox ID="txt_ServiceEndDate" runat="server" ReadOnly="true" CssClass="Size Small" Enabled="false"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator_txt_ServiceStartDate" runat="server" 
                                        ControlToValidate="txt_ServiceStartDate" Display="Dynamic" 
                                        ErrorMessage="Service Start & End Date" Text="Service Start & End Date is required" ValidationGroup="CSRegisterForm" />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Payment Term<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlPaymentTerm" runat="server">
                                <asp:ListItem Text="30 Days" Value="30" />
                                <asp:ListItem Text="45 Days" Value="45" />
                                <asp:ListItem Text="60 Days" Value="60" />
                                <asp:ListItem Text="90 Days" Value="90" />
                            </asp:DropDownList>
                        </div>
                        <asp:RequiredFieldValidator ID="rfvPaymentTerm" runat="server" ErrorMessage="Payment Term" ControlToValidate="ddlPaymentTerm" ValidationGroup="CSRegisterForm">
                            Payment Term is required
                        </asp:RequiredFieldValidator>
                    </div>

                    </ContentTemplate>  
                </asp:UpdatePanel>                 
            </div>
        </div>

        <div>
            <h3><a href="#">Discounts</a></h3>
            <div id="divProducts" runat="server" class="OForm AccordionPadding">
                <asp:UpdatePanel ID="updtpnlAccountAndLocationInformation" runat="server">
                    <ContentTemplate>
                       <div class="Row">
                            <div class="Label">Rate<span class="Required">*</span>:</div>
                           <div class="Control">
                      
                                   <asp:DropDownList ID="ddlRateCode" runat="server"  
                                            AutoPostBack="true"
                                            DataValueField="rateID" DataTextField="RateDesc" 
                                            OnSelectedIndexChanged="ddlRateCode_SelectedIndexChanged" >
	                               </asp:DropDownList> 
                                   <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator3" ControlToValidate="ddlRateCode"
                                            ErrorMessage="Rate" Text="Rate is required" Display="Dynamic"
                                            InitialValue="0" ValidationGroup="CSRegisterForm">
                                   </asp:RequiredFieldValidator>
                                   <asp:Label ID="lblInvalidRate" ForeColor="Red" runat="server"></asp:Label>
 
                                  <%--10/2013 WK - To be used for next Instadose build (after 10/15/13) --%>                       
                                  <asp:CheckBox ID="chkCustomRate" Text="Use Custom Rate" runat="server"
                                      OnCheckedChanged="chkCustomRate_CheckedChanged" AutoPostBack="True" />
                           
                            </div>
                            <div class="Clear"></div>
                        </div>
                        <div style="float: left; margin: 4px 5px 0 116px; width: 550px">
                        <div id="divAddAndSearchToolbar" runat="server" class="OToolbar JoinTable">
                            <ul>
                                <li>
                                    <asp:LinkButton ID="btnAddDiscount" CssClass="Add Icon" runat="server" 
                                         Visible="false" OnClick="btnAddDiscount_Click">Add Discount</asp:LinkButton>
                                </li>
                            </ul>
                        </div>
               
                        <asp:UpdatePanel ID="udtpnlRateDetailsGridView" runat="server">
                            <ContentTemplate>
                                <asp:GridView ID="gvDiscounts" CssClass="OTable" 
                                AlternatingRowStyle-CssClass="Alt" runat="server" AllowSorting="True"
                                AutoGenerateColumns="False" DataKeyNames="ProductGroupID" 
                                AllowPaging="True" PageSize="20" Width="550px">
                                    <AlternatingRowStyle CssClass="Alt" />
                                    <Columns>
                                        <asp:BoundField DataField="ProductGroupName" HeaderText="Product Group" SortExpression="ProductGroupName" />
                                        <asp:TemplateField HeaderText="Discount">
                                            <ItemTemplate>
                                                <div class="LabelValue;RightAlignItemText">
                                                    <asp:Label runat="server" ID="lblDiscount" Value='<%# Eval("Discount") %>' />
                                                </div>
                                                <asp:HiddenField runat="server" ID="hidProductGroupID" Value='<%# Eval("ProductGroupID") %>' />
                                                <asp:HiddenField runat="server" ID="hidDiscount" Value='<%# Eval("Discount") %>' />
                                            </ItemTemplate>
                                            <ControlStyle CssClass="RightAlignItemText" Width="100px" />
                                            <HeaderStyle CssClass="RightAlignHeaderText" HorizontalAlign="Left" />
                                            <ItemStyle CssClass="RightAlignItemText" HorizontalAlign="Right" Wrap="False" Width="75px" />
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="CreatedBy" HeaderText="Created By"
                                                SortExpression="ProductGroupName" />
                                        <asp:BoundField DataField="CreatedDate" HeaderText="Created On" 
                                            DataFormatString="{0:d}" SortExpression="ProductGroupName" />
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div>There are no records found!</div>
                                    </EmptyDataTemplate>
                                    <PagerSettings PageButtonCount="20" />
                                </asp:GridView>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        
                      </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>                  

        <div>
			<h3><a href="#">Billing Method Information</a></h3>
            <div class="OForm AccordionPadding">
                <asp:UpdatePanel ID="upnlBillingMethod" runat="server" UpdateMode="Conditional">
                    <Triggers>
                        <asp:AsyncPostBackTrigger controlid="ddlBrand" eventname="SelectedIndexChanged" />
                    </Triggers>
                    <ContentTemplate> 
                        <div class="Row">   
                            <div class="Label">Billing Frequency<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlBillingTerm" runat="server" >
                                    <asp:ListItem value="0" Text=" -- Select Billing Term --" />
                                    <asp:ListItem value="1" Text="Quarterly" />
	                                <asp:ListItem value="2" Text="Yearly" />
                                </asp:DropDownList> 
                                <asp:RequiredFieldValidator runat="server" ID="reqddlBillingTerm" ControlToValidate="ddlBillingTerm"
                                    ErrorMessage="Billing Frequency" Text="Billing Frequency is required" Display="Dynamic" InitialValue="0" ValidationGroup="CSRegisterForm">
                                </asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row">   
                            <div class="Label">Billing Days Prior:</div>
                            <div class="Control"><asp:TextBox ID="txtBillPriorDay" CssClass="Size XXSmall" runat="server" Text="14"/></div>
                            <div class="Clear"></div>
                        </div>
   
                        <div class="Row">
                            <div class="Label">PO Number<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:TextBox  ID="txtPOno" runat="server" Text="" MaxLength="15" ></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidatorPONumber" runat="server" 
                                    ControlToValidate= "txtPOno" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                    ErrorMessage="PO Number"  Text="PO Number is required" />
                                <asp:RegularExpressionValidator id="RegularExpressionValidator1" 
                                    ControlToValidate="txtPOno"
                                    ValidationExpression="[0-9a-zA-Z?\s~!\-@#$%^&amp;*/]{1,15}"
                                    Display="Dynamic"
                                    ErrorMessage="PO Number is max 15 characters or numerics." Text="PO Number is max 15 characters or numerics"
                                    runat="server"/>
                            </div>
                            <div class="Clear"></div>
                        </div>  

                        <div class="Row">   
                            <div class="Label">Begin & End PO On:</div>
                            <div class="Control">
                                <asp:TextBox ID="txtPOBeginDate" runat="server" CssClass="Size XSmall has-date-picker" />
                                    -
                                <asp:TextBox ID="txtPOEndDate" runat="server" CssClass="Size XSmall has-date-picker" />
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"  runat="server" id="divMaxID1InventoryLimit" visible="true">           
		                    <div class="Label" id="lMaxID1InventoryLimit" visible="true" runat="server">Max ID 1 Inventory Limit<span class="Required">*</span>:</div>
		                    <div class="Control"> 
		                        <asp:TextBox visible="true" ID="txtMaxID1InventoryLimit" runat="server" Text="" MaxLength="15" ></asp:TextBox>
		                        <asp:RequiredFieldValidator ID="RequiredFieldValidatorMaxID1Inventory" runat="server" 
		                            ControlToValidate= "txtMaxID1InventoryLimit" ValidationGroup="CSRegisterForm" Display="Dynamic"  
		                            ErrorMessage="Max ID 1 Inventory Limit"  Text="Max ID 1 Inventory Limit is required" enabled="false"/>                       
		                        <asp:RegularExpressionValidator id="RegularExpressionValidatorMaxID1Inventory" 
		                            ControlToValidate="txtMaxID1InventoryLimit" Enabled="false"                            
                                    ValidationExpression="^([0-9]|[1-9][0-9]|[1-9][0-9][0-9]|[1-9][0-9][0-9][0-9]|[1-9][0-9][0-9][0-9][0-5])$"
		                            Display="Dynamic" ValidationGroup="CSRegisterForm"
		                            ErrorMessage="Max ID 1 Inventory Limit has to be >=0 !" Text="Max ID 1 Inventory Limit has to be >= 0!"
		                            runat="server"/>
		                    </div>
		                    <div class="Clear"></div>
		                </div>  
		
		 
                        <div class="Row" runat="server" id="divMaxIDPlusInventoryLimit" visible="true">           
                            <div class="Label" id="lMaxIDPlusInventoryLimit" visible="true" runat="server">Max ID Plus Inventory Limit<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:TextBox visible="true" ID="txtMaxIDPlusInventoryLimit" runat="server" Text="" MaxLength="15" ></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidatorMaxIDPlusInventory" runat="server" 
                                    ControlToValidate= "txtMaxIDPlusInventoryLimit" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                    ErrorMessage="Max ID Plus Inventory Limit"  Text="Max ID Plus Inventory Limit is required" enabled="false"/>                       
                                <asp:RegularExpressionValidator id="RegularExpressionValidatorMaxIDPlusInventory" 
                                    ControlToValidate="txtMaxIDPlusInventoryLimit" Enabled="false"                            
                                    ValidationExpression="^([0-9]|[1-9][0-9]|[1-9][0-9][0-9]|[1-9][0-9][0-9][0-9]|[1-9][0-9][0-9][0-9][0-5])$"
                                    Display="Dynamic" ValidationGroup="CSRegisterForm"
                                    ErrorMessage="Max ID Plus Inventory Limit has to be >=0 !" Text="Max ID Plus Inventory Limit has to be >=0 !"
                                    runat="server"/>
                            </div>
                            <div class="Clear"></div>
                        </div>  

                        <div class="Row" runat="server" id="divMaxID2InventoryLimit" visible="true">           
		                    <div class="Label" id="lMaxID2InventoryLimit" visible="true" runat="server">Max ID 2 Inventory Limit<span class="Required">*</span>:</div>
		                    <div class="Control"> 
		                        <asp:TextBox visible="true" ID="txtMaxID2InventoryLimit" runat="server" Text="" MaxLength="15" ></asp:TextBox>
		                        <asp:RequiredFieldValidator ID="RequiredFieldValidatorMaxID2Inventory" runat="server" 
		                            ControlToValidate= "txtMaxID2InventoryLimit" ValidationGroup="CSRegisterForm" Display="Dynamic"  
		                            ErrorMessage="Max ID 2 Inventory Limit"  Text="Max ID 2 Inventory Limit is required" enabled="false"/>                       
		                        <asp:RegularExpressionValidator id="RegularExpressionValidatorMaxID2Inventory" 
		                            ControlToValidate="txtMaxID2InventoryLimit" Enabled="false"                            
                                    ValidationExpression="^([0-9]|[1-9][0-9]|[1-9][0-9][0-9]|[1-9][0-9][0-9][0-9]|[1-9][0-9][0-9][0-9][0-5])$"
		                            Display="Dynamic" ValidationGroup="CSRegisterForm"
		                            ErrorMessage="Max ID 2 Inventory Limit has to be >= 0!" Text="Max ID 2 Inventory Limit has to be >=0 !"
		                            runat="server"/>
		                    </div>
		                    <div class="Clear"></div>
                        </div> 

                        <div class="Row" runat="server" id="divMaxIDLinkInventoryLimit" visible="true">           
                            <div class="Label" id="lMaxIDLinkInventoryLimit" visible="true" runat="server">Max ID Link Inventory Limit<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:TextBox visible="true" ID="txtMaxIDLinkInventoryLimit" runat="server" Text="" MaxLength="15" ></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidatorMaxIDLinkInventory" runat="server" 
                                    ControlToValidate= "txtMaxIDLinkInventoryLimit" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                    ErrorMessage="Max ID Link Inventory Limit"  Text="Max ID Link Inventory Limit is required" enabled="false"/>                       
                                <asp:RegularExpressionValidator id="RegularExpressionValidatorMaxIDLinkInventory" 
                                    ControlToValidate="txtMaxIDLinkInventoryLimit" Enabled="false"                            
                                    ValidationExpression="^([0-9]|[1-9][0-9]|[1-9][0-9][0-9]|[1-9][0-9][0-9][0-9]|[1-9][0-9][0-9][0-9][0-5])$"
                                    Display="Dynamic" ValidationGroup="CSRegisterForm"
                                    ErrorMessage="Max ID Link Inventory Limit has to be >=0 !" Text="Max ID Link Inventory Limit has to be >=0 !"
                                    runat="server"/>
                            </div>
                            <div class="Clear"></div>
                        </div>  

                        <div class="Row" runat="server" id="divMaxIDLinkUSBInventoryLimit" visible="true">           
                            <div class="Label" id="lMaxIDLinkUSBInventoryLimit" visible="true" runat="server">Max ID Link USB Inventory Limit<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:TextBox visible="true" ID="txtMaxIDLinkUSBInventoryLimit" runat="server" Text="" MaxLength="15" ></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidatorMaxIDLinkUSBInventory" runat="server" 
                                    ControlToValidate= "txtMaxIDLinkUSBInventoryLimit" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                    ErrorMessage="Max ID Link USB Inventory Limit"  Text="Max ID Link USB Inventory Limit is required" enabled="false"/>                       
                                <asp:RegularExpressionValidator id="RegularExpressionValidatorMaxIDLinkUSBInventory" 
                                    ControlToValidate="txtMaxIDLinkUSBInventoryLimit" Enabled="false"                            
                                    ValidationExpression="^([0-9]|[1-9][0-9]|[1-9][0-9][0-9]|[1-9][0-9][0-9][0-9]|[1-9][0-9][0-9][0-9][0-5])$"
                                    Display="Dynamic" ValidationGroup="CSRegisterForm"
                                    ErrorMessage="Max ID Link USB Inventory Limit has to be >=0 !" Text="Max ID Link USB Inventory Limit has to be >=0 !"
                                    runat="server"/>
                            </div>
                            <div class="Clear"></div>
                        </div>  
                    </ContentTemplate>
                </asp:UpdatePanel>        
            </div>
        </div>

        <div>
			<h3><a href="#">Account Administrator Information</a></h3>
            <div class="OForm AccordionPadding">
                <asp:UpdatePanel ID="upnlAdministratorInformation" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                                                
                    <div class="Row">   
                        <div class="Label">Username<span class="Required">*</span>:</div>
                        <div class="Control"> 
                            <asp:TextBox  ID="txtLoginid" runat="server" MaxLength="50" CssClass="Size Medium" ></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" 
                                ControlToValidate= "txtLoginid" ValidationGroup="CSRegisterForm" Display="Dynamic" 
                                ErrorMessage="Username" Text="Username is required">
                            </asp:RequiredFieldValidator>   
                            <asp:CustomValidator ID="CustomValidator1" ValidationGroup="CSRegisterForm" 
                                ControlToValidate="txtLoginid" runat="server" Display="Dynamic"                    
                                ErrorMessage="Username is already in use." Text="Username is already in use" >
                            </asp:CustomValidator>
                        </div>
                        <div class="Clear"></div>   
                    </div>

                    <div class="Row">   
                        <div class="Label">Password<span class="Required">*</span>:</div>
                        <div class="Control" > 
                            <asp:TextBox  TextMode="Password" ID="txtPassword" runat="server" CssClass="Size Medium"  ></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RFV_password" runat="server" 
                                ControlToValidate= "txtPassword" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                ErrorMessage="Password" Text="Password is required">
                            </asp:RequiredFieldValidator> 
                            <asp:RegularExpressionValidator id="REV_password" 
                                ControlToValidate="txtPassword"
                                ValidationExpression="[0-9a-zA-Z~!@#$%^&amp;*]{5,}"
                                Display="Dynamic"
                                ErrorMessage="Password is at least 5 characters or numerics." Text="Password is at least 5 characters or numerics"
                                runat="server"/>                
                        </div>  
                        <div class="Clear"></div>                     
                    </div>

                    <div class="Row">   
                        <div class="Label">Re-enter Password<span class="Required">*</span>:</div>
                        <div class="Control"> 
                            <asp:TextBox  TextMode="Password"  ID="txtPasswordRe" runat="server" CssClass="Size Medium" ></asp:TextBox>   
                            <asp:RequiredFieldValidator ID="RFV_rePassword" runat="server" 
                                ControlToValidate= "txtPasswordRe" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                ErrorMessage="Re-enter Password" Text="Re-enter Password is required">
                            </asp:RequiredFieldValidator>                 
                            <asp:CompareValidator ID="CV_re_password" ControlToValidate="txtPasswordRe"
                                ControlToCompare="txtPassword" Type="String" Operator="Equal" 
                                ErrorMessage ="Passwords must match!" Text="Passwords must match!" runat="server" Display="Dynamic">
                            </asp:CompareValidator>
                        </div> 
                        <div class="Clear"></div>                       
                    </div>

                    <div class="Row">                
                        <div class="Label">Security Question:</div>
                        <div class="Control"> 
                            <asp:DropDownList ID="ddlSecurityQuestion" runat="server"  
                                DataValueField="SecurityQuestionID" DataTextField="SecurityQuestionText" >
	                        </asp:DropDownList> 
                        </div>
                        <div class="Clear"></div>   
                    </div>

                    <div class="Row">   
                        <div class="Label">Security Answer:</div>
                        <div class="Control"> 
                            <asp:TextBox  ID="txtSecurityA" runat="server" MaxLength="255" CssClass="Size XLarge"></asp:TextBox>
                        </div>
                        <div class="Clear"></div>   
                    </div>

                    <div class="Row">   
                        <div class="Label">First Name<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlGenderA" runat="server">
                                <asp:ListItem value="" Text="" />
                                <asp:ListItem value="Dr." Text="Dr." />
	                            <asp:ListItem value="Mr." Text="Mr." />
	                            <asp:ListItem value="Miss" Text="Miss" />
	                            <asp:ListItem value="Mrs." Text="Mrs." />
	                            <asp:ListItem value="Ms." Text="Ms." />
	                        </asp:DropDownList> 
                            <asp:TextBox CssClass="Size Medium" ID="txtFirstA" runat="server" MaxLength="40" ></asp:TextBox> 
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator13" runat="server" 
                                ControlToValidate= "txtFirstA" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                ErrorMessage="First Name" Text="First Name is required" >
                            </asp:RequiredFieldValidator>                      
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">   
                        <div class="Label">Last Name<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:TextBox  ID="txtLastA" runat="server" MaxLength="40" CssClass="Size Medium"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator14" runat="server" 
                                ControlToValidate= "txtLastA" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                ErrorMessage="Last Name" Text="Last Name is required" >
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">   
                        <div class="Label">Gender:</div>
                        <div class="Control"> 
                            <asp:RadioButton ID="rbtnMale" Text="Male" runat="server"   GroupName="rbtGender" Checked="true" />
                            <asp:RadioButton ID="rbtnFemale" Text="Female" runat="server"   GroupName="rbtGender"  />
                        </div>
                        <div class="Clear"></div>
                    </div>
                
                    <div class="Row">   
                        <div class="Label">Email<span class="Required">*</span>:</div>
                        <div class="Control"> 
                            <asp:TextBox  ID="txtEmail" runat="server" MaxLength="60" CssClass="Size Medium" ></asp:TextBox> 
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" 
                                ControlToValidate= "txtEmail" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                ErrorMessage="Email" Text="Email is required">
                            </asp:RequiredFieldValidator>                                                                              
                        </div>
                        <div class="Clear"></div>
                    </div>
                
                    <div class="Row">   
                        <div class="Label">Telephone<span class="Required">*</span>:</div>
                        <div class="Control"> 
                            <asp:TextBox  ID="txtTelephone" runat="server"  MaxLength="24" CssClass="Size Medium2"></asp:TextBox>  
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" 
                                ControlToValidate= "txtTelephone" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                ErrorMessage="Telephone" Text="Telephone is required">
                            </asp:RequiredFieldValidator>                      
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">                          
                        <div class="Label">Fax:</div>
                        <div class="Control"> 
                            <asp:TextBox  ID="txtFax" runat="server" MaxLength="24" CssClass="Size Medium2"></asp:TextBox>
                        </div>
                        <div class="Clear"></div>
                    </div>
                
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>

        <div>
			<h3><a href="#">Billing Information</a></h3>                       
            <div id="billing_fields" class="OForm AccordionPadding">
                <asp:UpdatePanel ID="upnlBillingInformation" runat="server" UpdateMode="Conditional">

                    <Triggers>
                        <asp:AsyncPostBackTrigger controlid="ckbxCopyCompany" eventname="CheckedChanged" />
                        <asp:AsyncPostBackTrigger ControlID="ddlBrand" EventName="SelectedIndexChanged"></asp:AsyncPostBackTrigger>
                        <asp:AsyncPostBackTrigger ControlID="ddlDealer" EventName="SelectedIndexChanged"></asp:AsyncPostBackTrigger>
                        <asp:AsyncPostBackTrigger ControlID="ddlReferral" EventName="SelectedIndexChanged"></asp:AsyncPostBackTrigger>
                    </Triggers>

                    <ContentTemplate>
                               
                        <div class="Row"> 
                            <div class="Label">Company<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox  ID="txtCompanyNameB" runat="server" MaxLength="100" CssClass="Size Large2"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="reqfldvalCompanyNameB" runat="server" 
                                    ControlToValidate= "txtCompanyNameB" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                    ErrorMessage="Billing Company" Text="Company is required" >
                                </asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>

                       <div class="Row"> 
                            <div class="Label">First Name<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlGenderB" runat="server" >
                                    <asp:ListItem value="" Text="" />
                                    <asp:ListItem value="Dr." Text="Dr." />
	                                <asp:ListItem value="Mr." Text="Mr." />
	                                <asp:ListItem value="Miss" Text="Miss" />
	                                <asp:ListItem value="Mrs." Text="Mrs." />
	                                <asp:ListItem value="Ms." Text="Ms." />
	                            </asp:DropDownList> 
                                <asp:TextBox  ID="txtFirstB"   runat="server" CssClass="Size Medium " MaxLength="50"></asp:TextBox>  
                                 <asp:RequiredFieldValidator ID="reqfldvalFirstNameB" runat="server" 
                                    ControlToValidate= "txtFirstB" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                    ErrorMessage="Billing First Name" Text="First Name is required">
                                </asp:RequiredFieldValidator>                    
                            </div>
                            <div class="Clear"></div>
                        </div>

                       <div class="Row"> 
                            <div class="Label">Last Name<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox  ID="txtLastB"   runat="server" CssClass ="Size Medium" MaxLength="50"></asp:TextBox>    
                                <asp:RequiredFieldValidator ID="reqfldvalLastNameB" runat="server" 
                                    ControlToValidate= "txtLastB" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                    ErrorMessage="Billing Last Name" Text="Last Name is required">
                                </asp:RequiredFieldValidator>               
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

                        <div class="Row"> 
                            <div class="Label">Telephone<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:TextBox  ID="txtTelephoneB" runat="server" CssClass ="Size Medium2" MaxLength="24" ></asp:TextBox>  
                                <asp:RequiredFieldValidator ID="reqfldvalTelephoneB" runat="server" 
                                    ControlToValidate= "txtTelephoneB" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                    ErrorMessage="Billing Telephone" Text="Telephone is required" >
                                </asp:RequiredFieldValidator>                      
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label">Fax:</div>
                            <div class="Control"> 
                                <asp:TextBox  ID="txtFaxB" runat="server" CssClass ="Size Medium2" MaxLength="24"></asp:TextBox>
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row">
                            <div class="Label">E-Mail Address<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox runat="server" ID="txtEmailAddressB" CssClass="Size Large" ValidationGroup="form" MaxLength="60" />
                                <asp:RequiredFieldValidator ID="reqfldvalBillingEmailAddress" runat="server" 
                                ControlToValidate= "txtEmailAddressB" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                ErrorMessage="Billing E-mail Address" Text="E-Mail Address is required" >
                                </asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator ID="regexpvalBillingEmailAddress" runat="server" 
                                ErrorMessage="Billing E-mail Address" ControlToValidate="txtEmailAddressB" Display="Dynamic"
                                ValidationGroup="CSRegisterForm" Text="Invalid e-mail format" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*">
                                </asp:RegularExpressionValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>
                
                    </ContentTemplate>  
                </asp:UpdatePanel>         
            </div>                                               
        </div>

        <div >
			<h3><a href="#">Shipping Information</a></h3>            
            <div id="shipping_fields" class="OForm AccordionPadding">
                <asp:UpdatePanel ID="upnlShippingInformation" runat="server" UpdateMode="Conditional">

                    <Triggers>
                        <asp:AsyncPostBackTrigger controlid="ckbxCopyCompany" eventname="CheckedChanged" />
                        <asp:AsyncPostBackTrigger ControlID="ddlCountryB" EventName="SelectedIndexChanged"></asp:AsyncPostBackTrigger>
                    </Triggers>
                    <ContentTemplate>                                
               
                        <div class="Row"> 
                            <div class="Label"></div>
                            <div class="Control"> <asp:CheckBox ID="copyaddress" runat="server" Text="The same as billing information" OnCheckedChanged="copyaddress_CheckedChanged" AutoPostBack="true"  /></div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label">Company<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox  ID="txtCompanyNameS" runat="server"  MaxLength="100" CssClass="Size Large2" ></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator19" runat="server" 
                                    ControlToValidate= "txtCompanyNameS" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                    ErrorMessage="Shipping Company" Text="Company is required" >
                                </asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>

                       <div class="Row"> 
                            <div class="Label">First Name<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:DropDownList ID="ddlGenderS" runat="server" >
                                    <asp:ListItem value="" Text="" />
                                    <asp:ListItem value="Dr." Text="Dr." />
	                                <asp:ListItem value="Mr." Text="Mr." />
	                                <asp:ListItem value="Miss" Text="Miss" />
	                                <asp:ListItem value="Mrs." Text="Mrs." />
	                                <asp:ListItem value="Ms." Text="Ms." />
	                            </asp:DropDownList> 
                                <asp:TextBox CssClass="Size Medium " MaxLength="50" ID="txtFirstS" runat="server"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator15" runat="server" 
                                    ControlToValidate= "txtFirstS" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                    ErrorMessage="Shipping First Name" Text="First Name is required" >
                                </asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label">Last Name<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox  ID="txtLastS"  runat="server" CssClass="Size Medium " MaxLength ="50" ></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator16" runat="server" 
                                    ControlToValidate= "txtLastS" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                    ErrorMessage="Shipping Last Name" Text="Last Name is required">
                                </asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label">Address<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox  ID="txtAddress1S"  runat="server" CssClass ="Size XLarge" MaxLength="255" ></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator17" runat="server" 
                                    ControlToValidate= "txtAddress1S" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                    ErrorMessage="Shipping Address" Text="Address is required" >
                                </asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label"></div>
                            <div class="Control"> <asp:TextBox  ID="txtAddress2S"   runat="server" CssClass="Size XLarge " MaxLength ="255"></asp:TextBox></div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label"></div>
                            <div class="Control"> <asp:TextBox  ID="txtAddress3S"   runat="server" CssClass="Size XLarge " MaxLength ="255"></asp:TextBox></div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label">Country<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:DropDownList ID="ddlCountryS" runat="server"  
                                    DataValueField="CountryID" DataTextField="CountryName" AutoPostBack="true" OnSelectedIndexChanged="ddlCountryOnSelectedIndexChange" />  
                                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator24" ControlToValidate="ddlCountryS"
                                    ErrorMessage="Shipping Country" Text="Country is required" Display="Dynamic" InitialValue="0" ValidationGroup="CSRegisterForm">
                                </asp:RequiredFieldValidator>                      
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label">City<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox  ID="txtCityS" runat="server" CssClass="Size Large2 " MaxLength ="100"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator18" runat="server" 
                                    ControlToValidate= "txtCityS" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                    ErrorMessage="Shipping City" Text="City is required" >
                                </asp:RequiredFieldValidator>
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label">State/Postal<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlStateS" runat="server"   
                                    DataValueField="StateID" DataTextField="StateAbbName" />
                                <asp:TextBox ID="txtPostalS" runat="server"  CssClass="Size Small" MaxLength ="15" ></asp:TextBox> 

                                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator27" ControlToValidate="ddlStateS"
                                    ErrorMessage="Shipping State" Text="State is required" Display="Dynamic" InitialValue="0" ValidationGroup="CSRegisterForm">
                                </asp:RequiredFieldValidator>

                                <asp:CustomValidator ID="CustomValidator2" ValidationGroup="CSRegisterForm" 
                                    ControlToValidate="ddlStateS" runat="server" Display="Dynamic"                    
                                    ErrorMessage="Shipping State" Text="State is not belonging to the country" >
                                </asp:CustomValidator>

                                <asp:RequiredFieldValidator ID="RequiredFieldValidator26" runat="server" 
                                    ControlToValidate= "txtPostalS" ValidationGroup="CSRegisterForm"  Display="Dynamic" 
                                    ErrorMessage="Shipping Postal Code" Text="Postal Code is required">
                                </asp:RequiredFieldValidator>  
                                                  
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label">Telephone<span class="Required">*</span>:</div>
                            <div class="Control"> 
                                <asp:TextBox  ID="txtTelephoneS" runat="server" CssClass="Size Medium2 " MaxLength ="24" ></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator21" runat="server" 
                                    ControlToValidate= "txtTelephoneS" ValidationGroup="CSRegisterForm" Display="Dynamic"  
                                    ErrorMessage="Shipping Telephone" Text="Telephone is required">
                                </asp:RequiredFieldValidator>                        
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row"> 
                            <div class="Label">Fax:</div>
                            <div class="Control"> 
                                <asp:TextBox  ID="txtFaxS" runat="server" CssClass="Size Medium2 " MaxLength ="24"></asp:TextBox>
                            </div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row">
                            <div class="Label">E-Mail Address:</div>
                            <div class="Control">
                                <asp:TextBox runat="server" ID="txtEmailAddressS" CssClass="Size Large" 
                                    ValidationGroup="form" MaxLength="60" />
                            </div>
                            <div class="Clear"></div>
                        </div>
                                                    
                    </ContentTemplate>
                </asp:UpdatePanel>  
            </div>                                    
        </div>

        <div >
			<h3><a href="#">Default Setup Information</a></h3>
            <div class="OForm AccordionPadding">             
                 <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>                                

                     <div class="Row"> 
                        <div class="Label">Package Type:</div>
                        <div class="Control"> 
                            <asp:DropDownList ID="ddlPackageType" runat="server" 
                            DataValueField="PackageTypeID" DataTextField="PackageDesc"></asp:DropDownList>                                        
                        </div>
                        <div class="Clear"></div>
                    </div>

                     <div class="Row"> 
                        <div class="Label">Shipping Option:</div>
                        <div class="Control"> 
                            <asp:DropDownList ID="ddlShippingOption" runat="server"  
                            DataValueField="ShippingOptionID" DataTextField="ShippingOptionDesc"></asp:DropDownList>
                        </div>
                        <div class="Clear"></div>
                    </div>  

                    <div id="PreferCarrier" runat ="server" class="Row"> 
                        <div class="Label">Prefer Ship Carrier:</div>
                        <div class="Control"> 
                            <asp:DropDownList ID="ddlShippingCarrier" runat="server"  />
                        </div>
                        <div class="Clear"></div>
                    </div>  
                             
                    <div class="Row"> 
                        <div class="Label"></div>
                        <div class="Control"> 
                            <asp:CheckBox ID="chkBoxIncludeLPI" runat="server" Text="Include LPI" />
                        </div>
                        <div class="Clear"></div>
                    </div>
                    
                     <div class="Row"> 
                        <div class="Label">Schedule:</div>
                        <div class="Control"> 
                            <asp:DropDownList ID="ddlSchedule" runat="server" ></asp:DropDownList>                                        
                        </div>
                        <div class="Clear"></div>
                    </div>

                     <div class="Row"> 
                        <div class="Label">Product Group<span class="Required">*</span>:</div>
                        <div class="Control"> 
                            <asp:DropDownList ID="ddlProductGroup" runat="server" AutoPostBack ="true" ></asp:DropDownList>
                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator4" ControlToValidate="ddlProductGroup"
                                ErrorMessage="Product Group" Text="Product Group is required" Display="Dynamic" InitialValue="0" ValidationGroup="CSRegisterForm">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label">Order Options:</div>
                        <div class="Control">
                            <asp:CheckBox ID="chkBoxDeviceInitialize" runat="server" Text="Badges Arrive Initialized" />
                            <asp:CheckBox ID="chkBoxDeviceAssign" runat="server" Text="Badges Arrive Assigned" />
                        </div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row"> 
                        <div class="Label">Label<span class="Required">*</span>:</div>
                        <div class="Control"> 
                            <asp:DropDownList ID="ddlLabel" runat="server" ></asp:DropDownList>
                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator33" ControlToValidate="ddlLabel"
                                ErrorMessage="Label" Text="Label is required" Display="Dynamic" InitialValue="0" ValidationGroup="CSRegisterForm">
                            </asp:RequiredFieldValidator>
                        </div>
                        <div class="Clear"></div>
                    </div> 

                    <div class="Row"> 
                        <div class="Label">Special Instructions:</div>
                        <div class="Control"> 
                            <asp:TextBox  ID="txtSpecialInstruction" TextMode="MultiLine" Height="50" MaxLength="200" runat="server"></asp:TextBox>
                            <asp:RegularExpressionValidator id="RegularExpressionSpecialInstructionValidator" 
                                                    ControlToValidate="txtSpecialInstruction"
                                                    ValidationExpression="^.{0,200}$"
                                                    ValidationGroup="CSRegisterForm"
                                                    Display="Dynamic"
                                                    ErrorMessage="Special Instructions is max 200 characters."
                                                    Text="Special Instructions is max 200 characters"
                                                    runat="server"/>
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
                            <td style="width:22%;" class="Label">
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
                         <tr id ="rowEDI" style="display:none">
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
                        <tr id ="rowDoNotSend" runat="server">
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
            
            <asp:Button ID="btnSave" runat="server" Text="Save" onclick="btnSave_Click" ValidationGroup="CSRegisterForm"  CssClass ="OButton"  />
            <asp:Button ID="btnUpdate" runat="server" Text="Save" onclick="btnUpdate_Click" ValidationGroup="CSRegisterForm"  Visible="false"  CssClass ="OButton" />    
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" onclick="btnCancel_Click"   CssClass ="OButton" />
            <act:confirmbuttonextender ID="ConfirmButtonExtender1" runat="server" TargetControlID="btnCancel"
                ConfirmText="Are you sure want to cancel the registration?"></act:confirmbuttonextender>                 
                
        </div>
        <div class="Clear"> </div>
    </div>

</div>


<div runat="server" id="divConfirmationForm"  visible="false" style="100%" class="ui-widget ui-widget-content" >

    <table style="width:100%;border:0;" cellpadding="0" cellspacing="0" class="OTable">
        <tr >
            <td>
                <asp:Label ID="lbl_CF_Message" runat="server" Text="" Font-Bold="true" />
                <br />
                <br />
                 AccountID: <asp:Label ID="lbl_CF_accountid" runat="server" Text="" Font-Bold="true" />
                 <br /> 
                 Username  :
                 <asp:Label ID="lbl_CF_username" runat="server" Text="" Font-Bold="true"></asp:Label>
            
                <br />
                <hr />
            </td>
        </tr>
        <tr>
            <td>
                <br />
                <table style="width:100%;border:0;" cellpadding="0" cellspacing="0" class="OTable">
                    <tr>
                        <td width="34%" valign="top" >
                            <b><u>Account Information</u></b><br />
                            Username: <asp:Label ID="lbl_CF_loginid" runat="server" Text=""  Font-Bold="true"/><br />
                            Account name: <asp:Label ID="lbl_CF_AccoutName" runat="server" Text=""  Font-Bold="true"/><br />
                            Company: <asp:Label ID="lbl_CF_Company" runat="server" Text=""  Font-Bold="true"/><br />
                            Name: <asp:Label ID="lbl_CF_Name" runat="server" Text="" Font-Bold="true" /><br />
                            Gender: <asp:Label ID="lbl_CF_Gender" runat="server" Text="" Font-Bold="true" /><br />
                            Telephone: <asp:Label ID="lbl_CF_Telephone" runat="server" Text=""  Font-Bold="true"/><br />
                            Fax: <asp:Label ID="lbl_CF_Fax" runat="server" Text="" Font-Bold="true" /><br />
                            Email: <asp:Label ID="lbl_CF_email" runat="server" Text="" Font-Bold="true" /><br />
                            <br />
                            Service Start Date: <asp:Label ID="lbl_CF_ServiceStartDate" runat="server" Text="" Font-Bold="true" /><br />
                            Service End Date: <asp:Label ID="lbl_CF_ServiceEndDate" runat="server" Text="" Font-Bold="true" /><br />
                            Rate Code: <asp:Label ID="lbl_CF_RateCode" runat="server" Text="" Font-Bold="true" /><br />
                            Discount: <asp:Label ID="lbl_CF_Discount" runat="server" Text="" Font-Bold="true" /><br />
                            Customer Type: <asp:Label ID="lbl_CF_CustomerType" runat="server" Text="" Font-Bold="true" />
                            <br />
                            Account Type: <asp:Label ID="lbl_CF_AccountType" runat="server" Text="" Font-Bold="true" /><br />
                            Currency: <asp:Label ID="lbl_CF_Currency" runat="server" Text="" Font-Bold="true" /><br />
                        </td>
                        <td width="33%" valign="top">
                            <b><u>Billing Information</u></b><br />
                            Billing cycle: <asp:Label ID="lbl_CF_BCycle" runat="server" Text=""  Font-Bold="true"/><br />
                            Company: <asp:Label ID="lbl_CF_BCompany" runat="server" Text=""  Font-Bold="true"/><br />
                            Name: <asp:Label ID="lbl_CF_BName" runat="server" Text="" Font-Bold="true" /><br />
                            Telephone: <asp:Label ID="lbl_CF_BTelephone" runat="server" Text="" Font-Bold="true" /><br />
                            Fax: <asp:Label ID="lbl_CF_BFax" runat="server" Text="" Font-Bold="true" /><br />
                            Email: <asp:Label ID="lbl_CF_BEmail" runat="server" Text="" Font-Bold="true" /><br />
                            <br />
                            Address: <br />
                            <asp:Label ID="lbl_CF_BAddress" runat="server" Text="" Font-Bold="true" /><br />
                        </td>
                        <td valign="top">
                            <b><u>Shipping Information</u></b><br />
                            Company: <asp:Label ID="lbl_CF_SCompany" runat="server" Text="" Font-Bold="true" /><br />
                            Name: <asp:Label ID="lbl_CF_SName" runat="server" Text=""  Font-Bold="true"/><br />
                            Telephone: <asp:Label ID="lbl_CF_STelephone" runat="server" Text="" Font-Bold="true" /><br />
                            Fax: <asp:Label ID="lbl_CF_SFax" runat="server" Text="" Font-Bold="true" /><br />
                            Email: <asp:Label ID="lbl_CF_SEmail" runat="server" Text="" Font-Bold="true" /><br />
                            <br />
                            Address: <br />
                            <asp:Label ID="lbl_CF_SAddress" runat="server" Text="" Font-Bold="true" /><br />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td >
                <hr />
                <asp:HiddenField ID="hdnAccountID" runat="server" />
                <asp:Button ID="btn_CF_order2" runat="server" Text="Order Service" CssClass="OButton" OnClick="btn_CF_order2_Click"></asp:Button>
                <asp:Button ID="btn_CF_AccountInfo2" runat="server" Text="View Account" CssClass="OButton" OnClick="btn_CF_AccountInfo2_Click"></asp:Button>
            </td>
        </tr>
    </table>     
</div>
</asp:Content>

