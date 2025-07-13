<%@ Page Title="Help Desk Tool" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="IT_HelpDeskTool" Codebehind="HelpDeskTool.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style type="text/css">
        .ui-helper-reset {
            font-size: 90% !important;
        }
    </style>
    <script type="text/javascript">
        $(document).ready(function () {

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
           
            // Set up the serial number validation
            $('#ctl00_primaryHolder_txtSerialNo_Tab4').keyup(function (event) {
            }).keypress(function (event) {

                // Validate the keys being pressed to ensure only numbers are entered.
                if (("0123456789").indexOf(String.fromCharCode(event.keyCode)) <= -1 && event.keyCode != '13') {
                    // If not, cancel the event.
                    event.preventDefault();
                }
            });            

            // Set up the serial number validation
            $('#ctl00_primaryHolder_txtSerialNo_Tab6').keyup(function (event) {
            }).keypress(function (event) {

                // Validate the keys being pressed to ensure only numbers are entered.
                if (("0123456789").indexOf(String.fromCharCode(event.keyCode)) <= -1 && event.keyCode != '13') {
                    // If not, cancel the event.
                    event.preventDefault();
                }
            });                                    

        });

        function JQueryControlsLoad() {

            $("#TabContainer1").tabs();            
            
        }
        
        /// Count lines of a text box.
        function countLines(id) {
            var area = document.getElementById(id)
            if (area.value == "") return 0;
            // trim trailing return char if exists
            var text = area.value.replace(/\s+$/g, "")
            var split = text.split("\n")
            return split.length
        }       

    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
    <act:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server">
    </act:ToolkitScriptManager>   

    <%--Start TabsContainer Section--%>
    <div id="TabContainer1" >

	    <ul>	
            <li><a href="#TabPanel1" id="A1" runat ="server">Delete Users</a></li>	
            <li><a href="#TabPanel2" id="A2" runat ="server">Set Account Admin User</a></li>	
            <li><a href="#TabPanel3" id="A3" runat ="server">Turn on Account HDN</a></li>	
            <li><a href="#TabPanel4" id="A4" runat ="server">Reverse S/N from RMA</a></li>	
            <li><a href="#TabPanel5" id="A5" runat ="server">Delete Order</a></li>	
            <li><a href="#TabPanel6" id="A6" runat ="server">Link Devices</a></li>	 
            <li><a href="#TabPanel7" id="A7" runat ="server">Online Access</a></li>	    
            <li><a href="#TabPanel8" id="A8" runat ="server">Reminder Days</a></li>	    
	    </ul>

        <!-- Delete Users  -->   
        <div id="TabPanel1">
            <asp:Panel ID="Panel1" runat="server" defaultbutton="btnDelete_Tab1">

            <asp:UpdatePanel ID="Update_Tab1" runat="server" UpdateMode="Conditional" >
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab1" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    

                    <div class="FormError" id="Error_Tab1" runat="server" visible="false" style="margin:10px" >
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="ErrorMsg_Tab1" runat="server" >An error was encountered.</span></p>
                    </div>
                    <div class="FormMessage" id="Success_Tab1" runat="server" visible="false" style="margin:10px" > 
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="SuccessMsg_Tab1" runat ="server" >Ready to search.</span></p>
                    </div>  
                        
                    <div class="OForm" id = "mainForm_Tab1" runat = "server">                                          
                            
                        <div class="Row">
                            <div class="Label Small">Act#<span class="Required">*</span>:</div>
                            <div class="Control"><asp:TextBox runat="server" ID="txtAct_Tab1" AutoPostBack ="true"
                                    CssClass="Size XXSmall " ValidationGroup="form_Tab1" OnTextChanged="txtAct_Tab1_TextChanged" /></div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row">
                            <div class="Label Small">Usernames<span class="Required">*</span>:</div>
                            <div class="Control">                                
                                <asp:ListBox runat ="server" ID ="lboxUsernames_Tab1" SelectionMode ="Multiple" 
                                    CssClass="Size Medium2 " ValidationGroup="form_Tab1" />
                            </div>
                            <div class="Clear"></div>
                        </div>  

                        <div class="Row">
                            <div class="Label Small">&nbsp;</div>
                            <div class="Control">
                                <asp:Button Text="Delete" ID="btnDelete_Tab1" runat="server" ValidationGroup="form_Tab1" cssClass="OButton"
                                    OnClick="btnDelete_Tab1_Click" OnClientClick="return confirm('Are you sure you want to delete these selected users?');" /> 
                            </div>
                            <div class="Clear"></div>
                        </div>                                                            
                                                                                                                                                                                        
                    </div>  
                                                                                         
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                      
        </div>
        <!-- Delete Users  -->  
        
        <!-- Set Account Admin Users  -->   
        <div id="TabPanel2">
            <asp:Panel ID="Panel2" runat="server" defaultbutton="btnOK_Tab2">

            <asp:UpdatePanel ID="Update_Tab2" runat="server" UpdateMode="Conditional">
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab2" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    

                    <div class="FormError" id="Error_Tab2" runat="server" visible="false" style="margin:10px" >
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="ErrorMsg_Tab2" runat="server" >An error was encountered.</span></p>
                    </div>
                    <div class="FormMessage" id="Success_Tab2" runat="server" visible="false" style="margin:10px" > 
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="SuccessMsg_Tab2" runat ="server" >Ready to search.</span></p>
                    </div>  
                        
                    <div class="OForm" id = "mainForm_Tab2" runat = "server">                                          
                            
                        <div class="Row">
                            <div class="Label Medium2">Act#<span class="Required">*</span>:</div>
                            <div class="Control"><asp:TextBox runat="server" ID="txtAct_Tab2" AutoPostBack ="true"
                                    CssClass="Size XXSmall" ValidationGroup="form_Tab2" OnTextChanged="txtAct_Tab2_TextChanged" /></div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row">
                            <div class="Label Medium2">Current Act Admin Username<span class="Required">*</span>:</div>
                            <div class="Control"><asp:TextBox runat="server" ID="txtCurrentActAdmin_Tab2" ReadOnly="true"  
                                    CssClass="Size Medium2" ValidationGroup="form_Tab2" /></div>                            
                            <div class="Clear"></div>
                        </div>  

                        <div class="Row">
                            <div class="Label Medium2">New Act Admin Username<span class="Required">*</span>:</div>
                            <div class="Control"><asp:DropDownList ID="ddlUsernames_Tab2" runat="server"
                                    CssClass="Size Medium2" ValidationGroup="form_Tab2" /></div>                            
                            <div class="Clear"></div>
                        </div>  

                        <div class="Row">
                            <div class="Label Medium2">&nbsp;</div>
                            <div class="Control">
                                <asp:Button Text="OK" ID="btnOK_Tab2" runat="server" ValidationGroup="form_Tab2" cssClass="OButton"
                                     OnClick="btnOK_Tab2_Click" OnClientClick="return confirm('Are you sure you want to set new main admin user?');"   /> 
                            </div>
                            <div class="Clear"></div>
                        </div>                                                            
                                                                                                                                                                                        
                    </div>  
                                                                                         
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                       
        </div>
        <!-- Set Account Admin Users  --> 

        <!-- Set Account HDN  -->   
        <div id="TabPanel3">
            <asp:Panel ID="Panel3" runat="server" defaultbutton="btnOK_Tab3">

            <asp:UpdatePanel ID="Update_Tab3" runat="server" UpdateMode="Conditional">
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab3" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    

                    <div class="FormError" id="Error_Tab3" runat="server" visible="false" style="margin:10px" >
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="ErrorMsg_Tab3" runat="server" >An error was encountered.</span></p>
                    </div>
                    <div class="FormMessage" id="Success_Tab3" runat="server" visible="false" style="margin:10px" > 
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="SuccessMsg_Tab3" runat ="server" >Ready to search.</span></p>
                    </div>  
                        
                    <div class="OForm" id = "mainForm_Tab3" runat = "server">                                          
                            
                        <div class="Row">
                            <div class="Label Small">Act#<span class="Required">*</span>:</div>
                            <div class="Control"><asp:TextBox runat="server" ID="txtAct_Tab3" 
                                    CssClass="Size XXSmall" ValidationGroup="form_Tab3" /></div>
                            <div class="Clear"></div>
                        </div>
                        
                        <div class="Row">
                            <div class="Label Small">&nbsp;</div>
                            <div class="Control">
                                <asp:Button Text="OK" ID="btnOK_Tab3" runat="server"   ValidationGroup="form_Tab3" cssClass="OButton"
                                     OnClick="btnOK_Tab3_Click" OnClientClick="return confirm('Are you sure you want to set HDN for this account?');"  /> 
                            </div>
                            <div class="Clear"></div>
                        </div>                                                            
                                                                                                                                                                                        
                    </div>  
                                                                                         
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                      
        </div>
        <!-- Set Account HDN  --> 

        <!-- Remove S/N from RMA  -->   
        <div id="TabPanel4">
            <asp:Panel ID="Panel4" runat="server" defaultbutton="btnSearch_Tab4">

            <asp:UpdatePanel ID="Update_Tab4" runat="server" UpdateMode="Conditional">
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab4" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    

                    <div class="FormError" id="Error_Tab4" runat="server" visible="false" style="margin:10px" >
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="ErrorMsg_Tab4" runat="server" >An error was encountered.</span></p>
                    </div>

                    <div class="FormMessage" id="Success_Tab4" runat="server" visible="false" style="margin:10px" > 
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="SuccessMsg_Tab4" runat ="server" >Ready to search.</span></p>
                    </div>  

                    <%--Search Tool Bar--%>                
                    <div id="snSearchToolBar" class="OToolbar">
                        <%--S/N SEARCH TOOLBAR--%>
                        <ul>
                            <li> 
                                Enter S/N#:&nbsp;<asp:TextBox ID="txtSerialNo_Tab4" runat="server" Width="100" CssClass ="Size XXSmall" Text=""></asp:TextBox> 
                            </li> 
                            <li>
                                <asp:Button ID="btnSearch_Tab4" runat="server" Text="Search" OnClick="btnSearch_Tab4_Click" CssClass="OButton" />
                            </li>
                        </ul>
                        <%--END--%>
                    </div>
                    <%--END--%>

                    <%--RMA DEVICE INFORMATION--%>
                    <div>                            
                        <asp:FormView ID="fvRMAInformation" runat="server" Width="100%" >
                            <ItemTemplate>
                                <table style="width:100%;border:1;" cellpadding="0" cellspacing="0" class="OTable">
                                    <tr>
                                        <th class="mt-hd" colspan ="4">
                                            RMA Information
                                        </th>
                                    </tr>
                                    <tr class="Alt">
                                        <td style="width: 12%" class="mt-hd">Return #:</td>
                                        <td style="width: 38%" class="mt-itm">
                                            <asp:Label ID="lblReturnID" runat="server" Text='<%# Bind("ReturnID") %>' Font-Bold="true" />                                        
                                        </td>
                                        <td style="width: 12%" class="mt-hd">Account #:</td>
                                        <td  class="mt-itm">
                                            <asp:HyperLink ID="hlAccountNo" NavigateUrl='<%# Bind("AccountID", "Account.aspx?ID={0}#Return_tab") %>' runat="server" Text='<%# Bind("AccountID") %>'></asp:HyperLink>
                                        </td>
                                    </tr>
                                    <tr class="">
                                        <td class="mt-hd">Created By:</td>
                                        <td class="mt-itm"><asp:Label ID="Label1" runat="server" Text='<%# Bind("CreatedBy") %>' /></td>
                                        <td class="mt-hd">Created Date:</td>
                                        <td class="mt-itm"><asp:Label ID="Label2" runat="server" Text='<%# Bind("CreatedDate", "{0:d}") %>' /></td>
                                    </tr>
                                    <tr class="Alt">
                                        <td class="mt-hd">Return Type:</td>
                                        <td class="mt-itm" colspan="3" style="white-space:nowrap;" >
                                            <asp:Label ID="Label3" runat="server" Text='<%# Bind("ReturnType") %>' Font-Bold="true" />                                                                
                                        </td>            
                                    </tr>
                                    <tr class="">
                                        <td class="mt-hd">Reason:</td>
                                        <td class="mt-itm" colspan="3"><asp:Label ID="Label13" runat="server" Text='<%# Bind("Reason") %>' /></td>            
                                    </tr>
                                    <tr class="Alt">
                                        <td class="mt-hd">Status:</td>
                                        <td class="mt-itm" colspan="3"><asp:Label ID="Label14" runat="server" Text='<%# Bind("Status") %>' /></td>
                                    </tr>

                                    <div runat="server" id="div_showRMAinfo" Visible='<%# DisplayRMAinfo(DataBinder.Eval(Container.DataItem,"ReturnTypeID","" )) %>'>
                                    <tr class="">
                                        <td colspan="4">
                                            <table >
                                                <tr>
                                                    <td style="white-space: nowrap;">Replacement Order#:</td>
                                                    <td >
                                                        <asp:HyperLink ID="HyperLink1" NavigateUrl='<%# Bind("OrderID", "order.aspx?ID={0}#Orders") %>' 
                                                        runat="server" Text='<%# Bind("OrderID") %>' Font-Bold="true"></asp:HyperLink>
                                                    </td>

                                                    <td style="white-space: nowrap;">Replacement ShipDate:</td>
                                                    <td class="mt-itm">
                                                        <asp:Label ID="Label4" runat="server" Font-Bold="true" Text='<%# Bind("ShipDate", "{0:d}") %>' />
                                                    </td>

                                                    <td style="white-space: nowrap;">Replacement Tracking#:</td>
                                                    <td>
                                                        <asp:HyperLink ID="HyperLink3" Target="_blank" 
                                                        NavigateUrl='<%# Bind("TrackingNumber", "http://fedex.com/Tracking?ascend_header=1&clienttype=dotcom&cntry_code=us&language=english&tracknumbers={0}") %>'
                                                        runat="server" Font-Bold="true" Text='<%# Bind("TrackingNumber") %>'>
                                                        </asp:HyperLink>
                                                    </td>

                                                    <td style="white-space: nowrap;">Replaced By Serial#: </td>
                                                    <td><asp:Label ID="Label5"  Font-Bold="true" runat="server" Text='<%# Bind("Serialno") %>' /></td>
                                                </tr>

                                                <tr>
                                                    <td colspan="4">
                                                        <asp:Label ID="lblRmaBillingAddress" runat="server" Text='<%# DisplayBillingAddress(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>'></asp:Label>
                                                    </td>
                                                    <td colspan="4">
                                                        <asp:Label ID="lblRmaShippingAddress" runat="server" Text='<%# DisplayShippingAddress(DataBinder.Eval(Container.DataItem,"OrderID","" )) %>'></asp:Label>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    </div>
                                </table>
                            </ItemTemplate>
                        </asp:FormView>                                            
                    </div>
                    <%--END--%>

                    <table class="OTable" id="mainForm_Tab4" runat ="server" >

                        <tr>
                            <th class="mt-hd" >
                                Reverse Information
                            </th>
                        </tr>      
                        <tr>
                            <td>
                                <div class="OForm" >                                                                       
                                    <div class="Row">
                                        <div class="Label Small">&nbsp;</div>
                                        <div class="Control">
                                            <asp:RadioButtonList id="radReverseType_Tab4" runat="server" RepeatColumns="1" RepeatDirection="Vertical"  
                                                OnSelectedIndexChanged="radReverseType_Tab4_SelectedIndexChanged" AutoPostBack ="true" ValidationGroup="form_Tab4">                        
                                                <asp:ListItem Selected ="True"   Value ="1">Remove from RMA request</asp:ListItem>
                                                <asp:ListItem Value ="2">Roll RMA device back to a desired user</asp:ListItem>                            
                                            </asp:RadioButtonList>                               
                                        </div>
                                        <div class="Clear"></div>
                                    </div>                                                                                                                                                               

                                    <div class="Row" id="actIDControl" runat ="server">
                                        <div class="Label Small">Act#<span class="Required">*</span>:</div>
                                        <div class="Control"><asp:TextBox runat="server" ID="txtAct_Tab4" Enabled ="false" ValidationGroup="form_Tab4"
                                                CssClass="Size XXSmall " />
                                        </div>
                                        <div class="Clear"></div>
                                    </div>
                        
                                    <div class="Row" id="usernameControl" runat ="server">
                                        <div class="Label Small">Usernames<span class="Required">*</span>:</div>
                                        <div class="Control"><asp:DropDownList ID="ddlUsers_Tab4" runat="server" ValidationGroup="form_Tab4"
                                                CssClass="Size Medium2"  />    
                                        </div>
                                        <div class="Clear"></div>
                                    </div> 

                                </div>
                            </td>                            
                        </tr>  
                    </table> 

                    <div class="Buttons">
                        <div class="ButtonHolder">
                            <asp:Button Text="Reverse" ID="btnOK_Tab4" runat="server"  ValidationGroup="form_Tab4" cssClass="OButton"
                                    OnClick="btnOK_Tab4_Click"  OnClientClick="return confirm('Are you sure you want to reverse this device from RMA?');" />                            
                        </div>
                        <div class="Clear"></div>
                    </div>                    
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                      
        </div>
        <!-- Remove S/N from RMA  -->         

        <!-- Delete Order  -->   
        <div id="TabPanel5">
            <asp:Panel ID="Panel5" runat="server" defaultbutton="btnSearchOrderNumber">

            <asp:UpdatePanel ID="Update_Tab5" runat="server" UpdateMode="Conditional">
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab5" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    

                    <div class="FormError" id="Error_Tab5" runat="server" visible="false" style="margin:10px" >
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="ErrorMsg_Tab5" runat="server" >An error was encountered.</span></p>
                    </div>
                    <div class="FormMessage" id="Success_Tab5" runat="server" visible="false" style="margin:10px" > 
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="SuccessMsg_Tab5" runat ="server" >Ready to search.</span></p>
                    </div>  

                    <%--Search Tool Bar--%>                
                    <div id="orderNumberSearchToolbar" class="OToolbar">
                        <%--ORDER NUMBER SEARCH TOOLBAR--%>
                        <ul>
                            <li> 
                                Enter Order #:&nbsp;<asp:TextBox ID="txtEnterOrderNumber" runat="server" Width="100" CssClass ="Size XXSmall" Text=""></asp:TextBox> 
                            </li> 
                            <li>
                                <asp:Button ID="btnSearchOrderNumber" runat="server" Text="Search" OnClick="btnSearchOrderNumber_Click" CssClass="OButton" />
                            </li>
                        </ul>
                        <%--END--%>
                    </div>
                    <%--END--%>

                    <%--ORDER INFORMATION--%>
                    <div>            
                
                        <asp:FormView ID="fvOrderInformation" runat="server" Width="100%">
                            <ItemTemplate>
                                <table class="OTable">
                                    <tr>
                                        <td style="width: 15%; font-weight: bold; text-align:right;">Order #:</td>
                                        <td style="width: 35%">
                                            <asp:Label ID="lblOrderNo" runat="server" Text='<%# Bind("OrderNo") %>' />
                                        </td>
                                        <td style="width: 15%; text-align: right; font-weight: bold">Account Name:</td>
                                        <td>
                                            <asp:HyperLink ID="hyprlnkAccountName" runat="server" Text='<%# Bind("AccountName") %>' NavigateUrl='<%# Eval("AccountNo", "~/InformationFinder/Details/Account.aspx?ID={0}#Order_tab") %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="font-weight: bold; text-align: right;">Status:</td>
                                        <td>
                                            <asp:Label ID="lblStatus" runat="server" Text='<%# Bind("Status") %>' />
                                        </td>
                                        <td style="font-weight: bold; text-align: right;">Source:</td>
                                        <td>
                                            <asp:Label ID="lblSource" runat="server" Text='<%# Bind("Source") %>' /></td>
                                    </tr>
                                    <tr>
                                        <td style="font-weight: bold; text-align: right;">Created Date:</td>
                                        <td>
                                            <asp:Label ID="lblCreatedDate" runat="server" Text='<%# string.Format("{0:d}", Eval("CreatedDate")) %>' />
                                        </td>
                                        <td>&nbsp;</td>
                                        <td>&nbsp;</td>
                                    </tr>
                                    <tr>
                                        <td style="font-weight: bold; text-align: right;">Tracking #:</td>
                                        <td>
                                            <asp:HyperLink ID="hyprlnkTrackingNumber" runat="server" Text='<%# Bind("TrackingNumber") %>' />
                                        </td>
                                        <td style="font-weight: bold; text-align: right;">Payment Method:</td>
                                        <td>
                                            <asp:Label ID="lblPaymentMethod" runat="server" Text='<%# Bind("PaymentMethod") %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="font-weight: bold; text-align: right;">Ship Date:</td>
                                        <td>
                                            <asp:Label ID="lblShipDate" runat="server" Text='<%# string.Format("{0:d}", Eval("ShipDate")) %>' />
                                        </td>
                                        <td style="font-weight: bold; text-align: right;">PO #:</td>
                                        <td>
                                            <asp:Label ID="lblPONumber" runat="server" Text='<%# Bind("PONumber") %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="font-weight: bold; text-align: right;">Coupon:</td>
                                        <td>
                                            <asp:Label ID="lblCoupon" runat="server" Text='<%# Bind("Coupon") %>' />
                                        </td>
                                        <td style="font-weight: bold; text-align: right;">Shipping:</td>
                                        <td>
                                            <asp:Label ID="lblOrderShipping" runat="server" Text='<%# Bind("FormattedShipping") %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="font-weight: bold; text-align: right;">Referral Code:</td>
                                        <td>
                                            <asp:Label ID="lblReferralCode" runat="server" Text='<%# Bind("Referral") %>' />
                                        </td>
                                        <td style="font-weight: bold; text-align: right;">Tax:</td>
                                        <td>
                                            <asp:Label ID="lblOrderTax" runat="server" Text='<%# Bind("FormattedTax") %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="font-weight: bold; text-align: right;">Released To Softrax:</td>
                                        <td>
                                            <asp:Label ID="lblReleasedToSoftrax" runat="server" Text='<%# YesNo(Eval("SoftTraxIntegration")) %>' />
                                        </td>
                                        <td style="font-weight: bold; text-align: right;">Credits:</td>
                                        <td>
                                            <asp:Label ID="lblMiscCredit" runat="server" Text='<%# Bind("FormattedCredit") %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="font-weight: bold; text-align: right;">Card Type:</td>
                                        <td>
                                            <asp:Label ID="lblCardType" runat="server" Text="" />
                                        </td>
                                        <td style="font-weight: bold; text-align: right;">Subtotal:</td>
                                        <td>
                                            <asp:Label ID="lblOrderSubtotal" runat="server" Text='<%# Bind("FormattedSubtotal") %>' />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="font-weight: bold; text-align: right;">Special Instruction:</td>
                                        <td>
                                            <asp:Label ID="lblSpecialInstructions" runat="server" Text='<%# Bind("SpecialInstructions") %>' />
                                        </td>
                                        <td style="font-weight: bold; text-align: right;">Total:</td>
                                        <td>
                                            <asp:Label ID="lblOrderTotal" runat="server" Text='<%# Bind("FormattedTotal") %>' />
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                            <EmptyDataTemplate>
                                <div class="NoData">
                                    The Order # you entered has either been deleted or does not exist.
                                </div>
                            </EmptyDataTemplate>
                        </asp:FormView>

                        <%--ORDER DETAILS GRIDVIEW--%>
                        <asp:GridView ID="gvOrderDetails" runat="server" 
                        AutoGenerateColumns="False" CssClass="OTable">
                            <Columns>
                                <asp:BoundField DataField="SKU" HeaderText="SKU" SortExpression="SKU" />
                                <asp:BoundField DataField="ProductName" HeaderText="Product" SortExpression="ProductName" />
                                <asp:BoundField DataField="ProductVariant" HeaderText="Subscription" SortExpression="ProductVariant" />
                                <asp:BoundField DataField="Price" HeaderText="Unit Price" SortExpression="Price" DataFormatString="{0:c}" HeaderStyle-CssClass="ralign">
                                    <HeaderStyle HorizontalAlign="Right" CssClass="RightAlignHeaderText"></HeaderStyle>
                                    <ItemStyle HorizontalAlign="Right"></ItemStyle>
                                </asp:BoundField>
                                <asp:BoundField DataField="Quantity" HeaderText="Qty." SortExpression="Quantity" HeaderStyle-CssClass="CenterAlignHeaderText">
                                    <HeaderStyle CssClass="CenterAlignHeaderText" HorizontalAlign="Center"></HeaderStyle>
                                    <ItemStyle CssClass="CenterAlign" HorizontalAlign="Center"></ItemStyle>
                                </asp:BoundField>
                                <asp:BoundField DataField="LineTotal" HeaderText="Subtotal" SortExpression="LineTotal" DataFormatString="{0:c}" HeaderStyle-CssClass="RightAlignHeaderText">
                                    <HeaderStyle CssClass="RightAlignHeaderText"></HeaderStyle>
                                    <ItemStyle CssClass="rightalign" Width="175px" HorizontalAlign="Right"></ItemStyle>
                                </asp:BoundField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="NoData">
                                    There are no details related to this Order #.
                                </div>
                            </EmptyDataTemplate>
                            <AlternatingRowStyle CssClass="Alt" />
                            <PagerStyle CssClass="Footer" />
                        </asp:GridView>
                        <%--END--%>

                        <%--PACKAGE INFORMATION GRIDVIEW--%>
                        <asp:GridView ID="gvPackageDetails" runat="server" 
                            AutoGenerateColumns="False" CssClass="OTable" 
                            AllowPaging="true" PageSize="10" AllowSorting="true">
                            <Columns>
                                <asp:TemplateField HeaderText="Tracking #" ItemStyle-VerticalAlign="Top" HeaderStyle-Width="200px" ItemStyle-Width="200px">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="hyprlnkTrackingNumber" runat="server" Text='<%# Eval("TrackingNumber") %>' Target="_blank"
                                        NavigateUrl='<%# Eval("TrackingNumber", "http://fedex.com/Tracking?ascend_header=1&clienttype=dotcom&cntry_code=us&language=english&tracknumbers={0}") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ShipDate" HeaderText="Shipped" SortExpression="ShipDate" DataFormatString="{0:d}" ItemStyle-VerticalAlign="Top" HeaderStyle-Width="200px" ItemStyle-Width="200px" />
                                <asp:TemplateField HeaderText="Shipping Address" ItemStyle-VerticalAlign="Top" HeaderStyle-Width="300px" ItemStyle-Width="300px">
                                    <ItemTemplate>
                                        <asp:Label ID="txtPackShipAddress" runat="server"
                                        Text='<%# GeneratePackShippingInfo(DataBinder.Eval(Container.DataItem,"PackageID","" )) %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Shipping Devices" ItemStyle-VerticalAlign="Top">
                                    <ItemTemplate>
                                        <asp:Label ID="lblSerialno" runat="server"
                                        Text='<%# GeneratePackSerialNo(DataBinder.Eval(Container.DataItem,"PackageID","" )) %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="NoData">
                                    There are no packages associated with this Order #.
                                </div>
                            </EmptyDataTemplate>
                            <AlternatingRowStyle CssClass="Alt" />
                            <PagerStyle CssClass="Footer" />
                        </asp:GridView>
                        <%--END--%>
                    
                    </div>
                    <%--END--%>

                    <%-- DELETE BUTTON --%>
                    <div class="Buttons">
                        <div class="ButtonHolder">
                            <asp:Button ID="btnDeleteOrder" CssClass="OButton" runat="server" Text="Delete This Order" 
                                OnClick="btnDeleteOrder_Click"  OnClientClick="return confirm('Are you sure you want to delete this order?');" />                            
                        </div>
                        <div class="Clear"></div>
                    </div>
                    <%--END--%>  
                                                                                         
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                       
        </div>
        <!-- Delete Order  --> 

        <!-- Assign/Transfer Devices to New Account  -->   
        <div id="TabPanel6">
            <asp:Panel ID="Panel6" runat="server" defaultbutton="btnProcess_Tab6">

            <asp:UpdatePanel ID="Update_Tab6" runat="server" UpdateMode="Conditional">
                <ContentTemplate>

                    <asp:UpdateProgress id="UpdateProgress_Tab6" runat="server" DynamicLayout="true" DisplayAfter="0" >
                        <ProgressTemplate>
                            <div style="width: 850px" align="center">
                                <img src="../images/loading11.gif" alt=""/>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>                    

                    <div class="FormError" id="Error_Tab6" runat="server" visible="false" style="margin:10px" >
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="ErrorMsg_Tab6" runat="server" >An error was encountered.</span></p>
                    </div>
                    <div class="FormMessage" id="Success_Tab6" runat="server" visible="false" style="margin:10px" > 
	                    <p><span class="MessageIcon"></span>
	                    <strong>Messages:</strong> <span id="SuccessMsg_Tab6" runat ="server" >Ready to search.</span></p>
                    </div>  
                        
                    <div class="OForm" id = "mainForm_Tab6" runat = "server">                                          
                        
                        <div class="Row">
                            <div class="Label Small">&nbsp;</div>
                            <div class="Control">
                                <asp:RadioButtonList id="radLinkType_Tab6" runat="server" RepeatColumns="1" RepeatDirection="Vertical"  
                                    OnSelectedIndexChanged="radLinkType_Tab6_SelectedIndexChanged" AutoPostBack ="true" ValidationGroup="form_Tab6">                        
                                    <asp:ListItem Selected ="True"   Value ="1">Link devices to new account only</asp:ListItem>
                                    <asp:ListItem Value ="2">Link devices, users, and reads all together to new account</asp:ListItem>                            
                                </asp:RadioButtonList>                                                                  
                            </div>
                            <div class="Clear"></div>
                        </div>  
                        
                        <div class="Row" id="fromAccount" runat ="server">
                            <div class="Label Small">From Act#<span class="Required">*</span>:</div>
                            <div class="Control"><asp:TextBox runat="server" ID="txtFromAccount_Tab6"  
                                    CssClass="Size XXSmall" ValidationGroup="form_Tab6"  /></div>
                            <div class="Clear"></div>
                        </div>                                                                                                

                        <div class="Row">
                            <div class="Label Small">S/N<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:TextBox runat="server" ID="txtSerialNo_Tab6" TextMode="MultiLine" Height="200px"  
                                    CssClass="Size Small " ValidationGroup="form_Tab6" />
                            </div>
                            <div class="Clear"></div>
                        </div>  
                        
                        <div class="Row">
                            <div class="Label Small">To Act#<span class="Required">*</span>:</div>
                            <div class="Control"><asp:TextBox runat="server" ID="txtToAccount_Tab6" AutoPostBack ="true" 
                                    CssClass="Size XXSmall" ValidationGroup="form_Tab6" OnTextChanged="txtToAccount_Tab6_TextChanged"  /></div>
                            <div class="Clear"></div>
                        </div>

                        <div class="Row" id="toLocation" runat ="server" >
                            <div class="Label Small">To Location:<span class="Required">*</span>:</div>
                            <div class="Control">
                                <asp:DropDownList ID="ddlToLocation_Tab6" runat="server" CssClass="Size Medium2"
                                    DataValueField="LocationID" DataTextField="LocationName" />
                            </div>
                            <div class="Clear"></div>
                        </div>
                        
                        <div class="Row">
                            <div class="Label Small">&nbsp;</div>
                            <div class="Control">
                                <asp:Button Text="Process" ID="btnProcess_Tab6" runat="server" cssClass="OButton" ValidationGroup="form_Tab6" 
                                     OnClick="btnProcess_Tab6_Click" OnClientClick="return confirm('Are you sure you want to assign/transfer these devices to new account?');" /> 
                            </div>
                            <div class="Clear"></div>
                        </div>                                                            
                                                                                                                                                                                        
                    </div>  
                                                                                         
                </ContentTemplate>
            </asp:UpdatePanel>
            
            </asp:Panel>                                                      
        </div>
        <!-- Assign/Transfer Devices to New Account  -->      
        
        <!-- Restrict Online Access  -->
        <div id="TabPanel7">
            <asp:Panel ID="Panel7" runat="server" defaultbutton="btnRestrict_Tab7">
                <asp:UpdatePanel ID="Update_Tab7" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>

                        <asp:UpdateProgress id="UpdateProgress_Tab7" runat="server" DynamicLayout="true" DisplayAfter="0" >
                            <ProgressTemplate>
                                <div style="width: 850px" align="center">
                                    <img src="../images/loading11.gif" alt=""/>
                                </div>
                            </ProgressTemplate>
                        </asp:UpdateProgress>

                        <div class="FormError" id="Error_Tab7" runat="server" visible="false" style="margin:10px" >
	                        <p><span class="MessageIcon"></span>
	                        <strong>Messages:</strong> <span id="ErrorMsg_Tab7" runat="server" >An error was encountered.</span></p>
                        </div>
                        <div class="FormMessage" id="Success_Tab7" runat="server" visible="false" style="margin:10px" > 
	                        <p><span class="MessageIcon"></span>
	                        <strong>Messages:</strong> <span id="SuccessMsg_Tab7" runat ="server" >Ready to search.</span></p>
                        </div>

                        <div class="OForm" id = "mainForm_Tab7" runat = "server">
                            <div class="Row">
                                <div class="Label Small">Act#<span class="Required">*</span>:</div>
                                <div class="Control"><asp:TextBox runat="server" ID="txtAct_Tab7" AutoPostBack ="true"
                                        CssClass="Size XXSmall " ValidationGroup="form_Tab7" OnTextChanged="txtAct_Tab7_TextChanged" /></div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label Small">Restrict:</div>
                                <div class="Control">     
                                    <asp:CheckBox runat="server" ID="chkRestrictOnlineAccess_Tab7" />
                                </div>
                                <div class="Clear"></div>
                            </div>

                            <div class="Row">
                                <div class="Label Small">&nbsp;</div>
                                <div class="Control">
                                    <asp:Button Text="Online Access" ID="btnRestrict_Tab7" runat="server" ValidationGroup="form_Tab7" cssClass="OButton"
                                        OnClick="btnRestrict_Tab7_Click" OnClientClick="return confirm('Are you sure you want to update Account Access?');" /> 
                                </div>
                                <div class="Clear"></div>
                            </div> 
                        </div>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
        </div>
        <!-- Restrict Online Access  -->

        <!-- Restrict Reminder Days  -->
        <div id="TabPanel8">
            <asp:Panel ID="Panel8" runat="server" defaultbutton="btnSave_Tab8">
                <asp:UpdatePanel ID="Update_Tab8" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:UpdateProgress id="UpdateProgress_Tab8" runat="server" DynamicLayout="true" DisplayAfter="0" >
                            <ProgressTemplate>
                                <div style="width: 850px" align="center">
                                    <img src="../images/loading11.gif" alt=""/>
                                </div>
                            </ProgressTemplate>
                        </asp:UpdateProgress>

                        <div class="FormError" id="Error_Tab8" runat="server" visible="false" style="margin:10px" >
	                        <p><span class="MessageIcon"></span>
	                        <strong>Messages:</strong> <span id="ErrorMsg_Tab8" runat="server" >An error was encountered.</span></p>
                        </div>
                        <div class="FormMessage" id="Success_Tab8" runat="server" visible="false" style="margin:10px" > 
	                        <p><span class="MessageIcon"></span>
	                        <strong>Messages:</strong> <span id="SuccessMsg_Tab8" runat ="server" >Ready to search.</span></p>
                        </div>

                        <div class="OForm" id = "mainForm_Tab8" runat = "server">
                            <div class="Row">
                                <div class="Label">Account Type<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:RadioButton id="rbInsta" Text="Insta" runat="server" Checked="true" GroupName="accountType"></asp:RadioButton>
                                    <asp:RadioButton id="rbGds" Text="GDS" runat="server" GroupName="accountType"></asp:RadioButton>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">Account#<span class="Required">*</span>:</div>
                                <div class="Control"><asp:TextBox runat="server" ID="txtAct_Tab8" AutoPostBack ="true"
                                        CssClass="Size XXSmall " ValidationGroup="form_Tab8" OnTextChanged="txtAct_Tab8_TextChanged" /></div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">Reminder Days<span class="Required">*</span>:</div>
                                <div class="Control">
                                    <asp:TextBox runat="server" ID="txtReminderDays_Tab8"
                                        CssClass="Size XXSmall " ValidationGroup="form_Tab8" /></div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">&nbsp;</div>
                                <div class="Control">
                                    <asp:Button Text="Save Reminder Days" ID="btnSave_Tab8" runat="server" ValidationGroup="form_Tab8" cssClass="OButton"
                                        OnClick="btnSave_Tab8_Click" OnClientClick="return confirm('Are you sure you want to update Account Access?');" /> 
                                </div>
                                <div class="Clear"></div>
                            </div> 
                        </div>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
        </div>
        <!-- Restrict Reminder Days  -->

        <div class="OForm" id = "Notation" runat = "server">
            <div class="Left">
                <span class="Required">* Indicate required fields.</span>
            </div> 

            <div class="Row"> 
                <div class="Clear"></div>
            </div> 
        </div>
                       
    </div>
    <%--End TabsContainer Section--%>
    

</asp:Content>



