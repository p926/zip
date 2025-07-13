<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="CustomerService_DeactivateAccount" Codebehind="DeactivateAccount.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server" >
    <ContentTemplate>  

<asp:Panel ID="pnlForm" runat="server">

     <div class="FormError" id="formError" runat="server" visible="false">
		<p><span class="MessageIcon"></span>
	    <strong>Messages:</strong>&nbsp;<asp:label ID="lblErrorMessage" text="The account has been successfully deactivated." runat="server" /></p>
    </div>
    <div class="FormMessage" id="formSuccess"  runat="server" visible="false">
		<p><span class="MessageIcon"></span>
	    <strong>Messages:</strong>&nbsp;<asp:label ID="lblSuccessMessage" text="The account has been successfully deactivated." runat="server" /></p>
    </div>

    <div class="OForm">
        <div class="Row">
            <div class="Label">Account #<span class="Required">*</span>:</div>
            <div class="Control">
                <asp:TextBox ID="txtAccountID" runat="server" AutoPostBack="true" OnTextChanged="txtAccountID_OnTextChanged" CssClass="Size XSmall"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtAccountID"
                    ErrorMessage="The Account # is required. " ValidationGroup="ValGrpAcct" Display="Dynamic" />
                <asp:RegularExpressionValidator id="RegularExpressionValidator" ControlToValidate="txtAccountID" 
                    ValidationExpression="^\d*$" Display="Dynamic" ErrorMessage="Must be a number." runat="server" ValidationGroup="ValGrpAcct" />
             </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label">Company Name:</div>
            <div class="Control"><asp:Label ID="lblCompanyName" runat="server" Text=""></asp:Label></div>
            <div class="Clear"></div>
        </div>

        <asp:Panel ID="pnCustomerGroup" runat="server" CssClass="Row" Visible="false">
            <div class="Label">Customer Group:</div>
            <div class="Control">
                <asp:DropDownList runat="server" ID="ddlCustomerGroup"></asp:DropDownList>
            </div>
            <div class="Clear"></div>
        </asp:Panel>

        <asp:HiddenField ID="hdnCurrentContractId" runat="server" ClientIDMode="Static" />
        <asp:Panel ID="pnContractStartDate" runat="server" CssClass="Row" Visible="false">
            <div class="Label">Contract Start<span class="Required">*</span>:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlContractStartDate" runat="server" ClientIDMode="Static"></asp:DropDownList>
            </div>
            <div class="Clear"></div>
        </asp:Panel>

        <div class="Row">
            <div class="Label">Rate<span class="Required">*</span>:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlRateCode" runat="server"  
                    AutoPostBack="true"
                    DataValueField="rateID" DataTextField="RateDesc">
	            </asp:DropDownList> 
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator4" ControlToValidate="ddlRateCode"
                    ErrorMessage="Rate" Text="Please select a Rate." Display="Dynamic"
                    InitialValue="0" ValidationGroup="ValGrpAcct">
                </asp:RequiredFieldValidator>

            </div>
            <div class="Clear"></div>
        </div>

        <asp:Panel ID="pnSalesRep" runat="server" CssClass="Row" Visible="false">
            <div class="Label">Sales Rep:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlSalesRep" runat="server"></asp:DropDownList>
            </div>
            <div class="Clear"></div>
        </asp:Panel>


        <div class="Row">
            <div class="Label">Reason<span class="Required">*</span>:</div>
            <div class="Control">
                <asp:DropDownList ID="ddlReason" runat="server"></asp:DropDownList>
                <asp:RequiredFieldValidator runat="server" Display="Dynamic" ID="RequiredFieldValidator3" ControlToValidate="ddlReason"
                     ErrorMessage="Please provide a reason for canceling." InitialValue="0" ValidationGroup="ValGrpAcct" /></div>
            <div class="Clear"></div>
        </div>
    
        <div class="Row">
            <div class="Label">Notes/Comments<span class="Required">*</span>:</div>
            <div class="Control">
                <asp:TextBox ID="txtNote"  runat="server" Width="400px" Height="107px" 
                    TextMode="MultiLine" ></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtNote"
                ErrorMessage="Please provide notes or comments." Display="Dynamic" ValidationGroup="ValGrpAcct"  />
            </div>
            <div class="Clear"></div>
        </div>

        <div class="Row">
            <div class="Label">Created By:</div>
            <div class="Control">
                <asp:Label ID="lblUserName" runat="server" Text=""></asp:Label>
            </div>
            <div class="Clear"></div>
        </div>

        <hr />

        <asp:PlaceHolder ID="phInvoiceDeliveryMethods" runat="server">
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
                    </td>                            
                </tr>
                <asp:PlaceHolder ID="phInvoiceDeliveryEDI" runat="server">
                <tr id ="rowEDI">
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
                </asp:PlaceHolder>
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
        </asp:PlaceHolder>

        <div style="float: none; text-align: right; width: auto">
            <asp:Button ID="btnSaveInfo" runat="server" Text="Deactivate" CssClass="OButton"
                ValidationGroup="ValGrpAcct" onclick="btnSaveInfo_Click"/>
            <asp:Button ID="btnCancel" runat="server" Text="Return to Account" CssClass="OButton" onclick="btnCancel_Click"/>
        </div>
    </div>
</asp:Panel>

    </ContentTemplate>
    </asp:UpdatePanel>
   
</asp:Content>

