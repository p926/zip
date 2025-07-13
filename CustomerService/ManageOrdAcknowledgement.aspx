<%@ Page Title="Home" Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_CustomerService_ManageOrdAcknowledgement" Codebehind="ManageOrdAcknowledgement.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">

 <style type="text/css">
        .style1
        {
            width: 387px;
        }
        .style2
        {
            width: 94px;
        }
  </style>
  
  <script type="text/javascript">
    $(document).ready(function () {
//        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
//        JQueryControlsLoad();
    });

    function JQueryControlsLoad() {

//        $('#UploadOrderAckDialog').dialog({
//                autoOpen: false,
//                modal: true,
//                width: 500,
//                resizable: false,
//                title: "Upload  Order Acknowledgement File(s)",
//                open: function (type, data) {
//                    $(this).parent().appendTo("form"); //$("form:first")); 
//                    $('#<%= btnUpload.ClientID %>').click();
//  
//                    $('.ui-dialog :input').focus();
//                },
//                buttons: {
//                    "Ok": function () {
//                        $('#<%= btnUpload.ClientID %>').click();
//                        //alert(this);
//                    },
//                    "Cancel": function () {
//                        $(this).dialog("close");
//                    }
//                },
//                close: function () {
//                    $('#<%= btnCancel.ClientID %>').click();
//                    $('.ui-overlay').fadeOut();
//                },
//            });

//        $('#UploadOrderAckDialog').dialog({
//            autoOpen: false,
//            width: 550,
//            resizable: false,
//            title: "Upload Order Acknowledgement File(s)",
//            open: function (type, data) {
//                $(this).parent().appendTo("form");
//                $('.ui-dialog :input').focus();
//            },
//            buttons: {
//                "Upload": function () {
//                    $('#<= btnUpload.ClientID %>').click();
//                },
//                "Cancel": function () {
//                    $(this).dialog("close");
//                }
//            },
//            close: function () {
//                $('#<= btnCancel.ClientID %>').click();
//                $('.ui-overlay').fadeOut();
//            }
//        });
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
    <act:toolkitscriptmanager id="ToolkitScriptManager1" runat="server"></act:toolkitscriptmanager>

   <%-- <div id="UploadOrderAckDialog2">
        <asp:UpdatePanel ID="plUploadContainer"  ChildrenAsTriggers="true" runat="server" UpdateMode="Always">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnUpload" EventName="Click" />
            </Triggers>
            <ContentTemplate>
  
                <div class="FormError" id="modalDialogErrors2" runat="server" visible="false">
                    <p><span class="MessageIcon"></span>
                    <strong>Messages:</strong> <span id="modalDialogErrorMsg2" runat="server" >An error was encountered.</span></p>
                </div>

                <div class="FormMessage" id="UploadDialogMsg2" runat="server" visible="false" > 
                    <p><span class="MessageIcon"></span>
                    <strong>Messages:</strong> <span id="UploadDialogMsgText" runat="server" >Ready to search.</span></p>
                </div>
                 
                <div class="For"OForm" style="width: 100%">
                    <asp:Label ID="lblOlUploadResult" runat="server"></asp:Label>
                    <asp:HiddenField ID="hfUploadAccountID" runat="server" Value="0" />
                    <div class="Row">
                        <div class="Label Small ">Order #:</div>
                            <div class="LabelValue"  style="width: 200px;">
                                <asp:Label runat="server" ID="lblOLUploadOrderID"  /></div>
                            <div class="Clear">
                        </div>
                    </div>
                     
                        <div class="Row">
                            <div class="Label">
                                Attachment 1:</div>
                            <div class="Control">
                                <asp:FileUpload ID="FileUpload3" runat="server" />
                            </div>
                            <div class="Clear">
                        
                        </div>
                        </div>
                        <div class="Row">
                            <div class="Label">
                                Attachment 2:</div>
                            <div class="Control">
                                <asp:FileUpload ID="FileUpload4" runat="server" />
                            </div>
                            <div class="Clear">
                        </div>
                    </div>
                  
                    <asp:Button ID="btnUpload2" runat="server" Text="Upload" Style="display: none;"
                        OnClick="btnUpload_Click" CommandArgument='<%# Eval("AccountID") %>' />

                    <asp:Button ID="btnCancel2" runat="server" Text="" CssClass="btnClose" Style="display: none;"
                        OnClick="btnCancel_Click" />

                    <--
                        <asp:button text="Load" style="display:none;" 
                        id="btnLoadPOUpload" OnClick="btnLoadPOUpload_Click" runat="server" />
                       -->
              
               
           </ContentTemplate>
       </asp:UpdatePanel>
    </div>
    --%>
 
    

 <%-- <asp:UpdatePanel runat="server">
    <ContentTemplate>--%>


    <div class="FormError" id="formErrors" runat="server" visible="false">
		<p><span class="MessageIcon"></span>
		<strong>Messages:</strong>&nbsp;<span id="errorMsg" runat="server" >An error was encountered.</span></p>
        
        <asp:BulletedList ID="blstErrors" runat="server" BulletStyle="Disc">
            </asp:BulletedList>
    </div>
    <div class="FormMessage" id="formSuccesses"  runat="server" visible="false">
		<p><span class="MessageIcon"></span>
	    <strong>Messages:</strong>&nbsp;<span id="successMsg" runat="server" >Submission of information has been successful.</span></p>
        <asp:BulletedList ID="blstMessage" runat="server" BulletStyle="Disc">
            </asp:BulletedList>
    </div>
    
    <div>

        <div id="Toolbar" class="OToolbar JoinTable">
			<ul id="accountInfo">
				<asp:HiddenField ID="HidAccountID" runat="server" Value='<%# Eval("ID") %>' />
				<li>Account #:</li>
				<li>    
					    <asp:TextBox runat="server" ID="txtAccountID" Text="" Width="76px" 
                                ontextchanged="txtAccountID_TextChanged"></asp:TextBox>
                       
                        <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator1" ControlToValidate="txtAccountID"
                                 ErrorMessage="<br />*Required" Display="Dynamic" ValidationGroup="AckUpload">
                        </asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator id="RegularExpressionValidator1" 
                                    ControlToValidate="txtAccountID"
                                    ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"
                                    Display="Dynamic"
                                    ErrorMessage="Account# must be numeric."
                                    runat="server"/>                
                </li>
				<li>Account Name:</li>
				<li>
					<asp:TextBox ID="lblAccountName" runat="server" Style="width: 150px;" ValidationGroup="SUBMIT">
					</asp:TextBox>
				</li>
                <li>
                    <asp:Button ID="btnView" runat="server" CssClass="OButton"
                                onclick="btnSearch_Click" Text="Search" ValidationGroup="AckUpload" />
                </li>
                <li>           
                    <asp:Button ID="btnUploadForm" runat="server"  CssClass="OButton"
                           onclick="btnUploadForm_Click" Text="Upload" ValidationGroup="AckUpload"
                           CommandArgument='<%# Eval("AccountID") %>' />
			    </li>
                               <li>
                    <asp:Button ID="btnClear" runat="server" CssClass="OButton"
                                onclick="btnClear_Click" Text="Clear" ValidationGroup="AckUpload" />
                </li>
            </ul>
		</div>

        <asp:Panel ID="orderAckfiles" runat="server">
          
            <%--Acknowledgement Files for given Account--%>
                <div id="updpnlFiles">
                    <asp:UpdatePanel ID="updtpnlAckfiles" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>

                        <asp:GridView ID="gvAck" runat="server" AutoGenerateColumns="False" 
                             CssClass="OTable" 
                            GridLines="None" DataKeyNames="DocumentID" 
                            EmptyDataText="No Acknowledgement Received. ">
                          <RowStyle CssClass="mt-hd" />
                          <Columns>
                              <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" 
                                FooterStyle-HorizontalAlign="center"  ItemStyle-HorizontalAlign="center" 
                                    HeaderText="" ControlStyle-Width="150">
                                <ItemTemplate>
                                    <asp:HyperLink ID="HyperLink2" 
                                        NavigateUrl='<%# FuncGenerateHTTPLink(DataBinder.Eval(Container.DataItem,"DocumentID","" ))%>'
                                        runat="server" ToolTip="Click to View Acknowledgement" 
                                        Text="View"></asp:HyperLink>
                                </ItemTemplate>
                              </asp:TemplateField>
                              <asp:BoundField DataField="AccountID" HeaderText="Account#" 
                                  SortExpression="AccountID" ItemStyle-Width="150" ItemStyle-HorizontalAlign="Right"  />
                              <asp:BoundField DataField="Filename" HeaderText="File Name" 
                                  SortExpression="AckFilename"  ItemStyle-Width="450" />
                              <asp:BoundField DataField="CreatedBy" HeaderText="Created By" 
                                  SortExpression="CreatedBy"  ItemStyle-Width="150" />
                              <asp:BoundField DataField="CreatedDate" HeaderText="Date" DataFormatString="{0:d}"
                                  SortExpression="CreatedDate" ItemStyle-Width="150" />
                          </Columns>
                            <HeaderStyle CssClass="mt-hd" />
                            <AlternatingRowStyle CssClass="Alt" />
                            <PagerSettings Mode="NextPreviousFirstLast" />
                            <PagerStyle CssClass="mt-hd" />
                            <RowStyle CssClass="Row" />
                            <SelectedRowStyle CssClass="Row Selected" />
            
                         </asp:GridView>
      
                        </ContentTemplate>
                    </asp:UpdatePanel>    
                </div>
            <%--END--%>
            </asp:Panel>

         </div>
 
        <%-- Upload Order Acknowledgement file form --%>	
             <form>          
            <asp:Panel ID="UploadOrderAckDialog" runat="server">
            <div class="OTable">
                    <div class="FormError" id="modalDialogErrors" runat="server" visible="false">
					    <p><span class="MessageIcon"></span>
					    <strong>Messages:</strong> <span id="modalDialogErrorMsg" runat="server" >An error was encountered.</span></p>
                    </div>
                    <div class="OForm" style="width:500px">
                        <div class="Row">
                            <div class="Label">Upload File(s)</div>
                            <div class="Clear"></div>
                        </div>
                      
                        <div class="Row">
                            <div class="Label">Attachment 1:</div>
                            <div class="Control"><asp:FileUpload ID="FileUpload1" runat="server" /></div>
                            <div class="Clear"></div>
                        </div>
                        <div class="Row">
                            <div class="Label">Attachment 2:</div>
                            <div class="Control"><asp:FileUpload ID="FileUpload2" runat="server" /></div>
                            <div class="Clear"></div>
                        </div>
                      

                        <div class="Clear"></div>
                        <div class="Buttons">
                            <div class="ButtonsHolder">
                                <asp:Button ID="btnUpload" runat="server" class="OButton" Text="Upload" 
                                    CommandName="Update" OnClick="btnUpload_Click"
                                    ValidationGroup="UPDATE_LOCATION">
                                </asp:Button>
                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" 
                                    class="OButton"
                                    OnClick="btnCancel_Click">
                                </asp:Button>
                            </div>
                        </div>
            </div>	
            </div> 
           </asp:Panel>
           </form>
         <%--END --%>
            
          <%--
           <div id="uploadPODialog">
                <asp:UpdatePanel ID="plUploadContainer" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>

                        <div class="OForm">
                            <asp:Label ID="lblOlUploadResult" runat="server"></asp:Label>
                            <asp:HiddenField ID="hfUploadOrderID" runat="server" Value="0" />
 
                            <div class="Row">
                                <div class="Label">	Order #:</div>
                                <div class="LabelValue">
                                    <asp:Label runat="server" ID="lblOLUploadOrderID"  
                                       Text="test"/>
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">	Attachment 1:</div>
                                <div class="Control">
                                    <asp:FileUpload ID="attachmentUpload1" runat="server" />
                                </div>
                                <div class="Clear"></div>
                            </div>
                            <div class="Row">
                                <div class="Label">Attachment 2:</div>
                                <div class="Control">
                                    <asp:FileUpload ID="attachmentUpload2" runat="server" />
                                </div>
                            </div>
                        </div>
                                    
                        <asp:Button ID="btnOLUpload" runat="server" Text="Upload" Style="display: none;" 
                            OnClick="btnOLUpload_Click" />

                        <asp:Button ID="btnClose" runat="server" Text="" CssClass="btnClose" Style="display: none;"
                            OnClick="btnClose_Click" />
   
                        <asp:button text="Load" style="display:none;" id="btnLoadPOUpload" OnClick="btnLoadPOUpload_Click" runat="server" />
   
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            --%>
          
          <%--
    <asp:SqlDataSource ID="SqlDataSource1" runat="server" 
         ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>" 
         SelectCommand="SELECT * FROM Documents
        WHERE AccountID=@accountid 
            AND DocumentCategory='Order Acknowledgement'
            AND Active=1 ORDER BY createdDate">
        <SelectParameters>
            <asp:ControlParameter ControlID="txtAccountID" DefaultValue="0" 
                Name="accountid" PropertyName="Text" />
        </SelectParameters>
     </asp:SqlDataSource>
     --%>
  <%--
     </ContentTemplate>
  </asp:UpdatePanel> --%>

   


</asp:Content>
