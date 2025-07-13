<%@ Page Title="" Language="C#"  MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="InstaDose_InformationFinder_Details_Return" Codebehind="Return.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript">

        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
        });

        function JQueryControlsLoad() {

            $("#tabsContainer").tabs();

            $('#addNoteDialog').dialog({
                autoOpen: false,
                width: 390,
                resizable: false,
                title: "Add Notes",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Ok": function () {
                        $('#<%= btnAddNotes.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancelNotes.ClientID %>').click();
                    $('.ui-overlay').fadeOut();
                }
            });

           
            $('#returnTypeDialog').dialog({
                autoOpen: false,
                width: 400,
                resizable: false,
                title: "Edit Return Type",
                open: function (type, data) {
                    $(this).parent().appendTo("form");
                    $('#<%= btnLoadReturnType.ClientID %>').click();
                    $('.ui-dialog :input').focus();
                },
                buttons: {
                    "Ok": function () {
                        $('#<%= btnAddReturnType.ClientID %>').click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                    }
                },
                close: function () {
                    $('#<%= btnCancelReturnType.ClientID %>').click();
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

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server" >
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div id="addNoteDialog">
        <asp:UpdatePanel ID="upnlAddNotes" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
            
                <div class="FormError" id="noteDialogErrors" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="noteDialogErrorMsg" runat="server" >An error was encountered.</span></p>
				</div>
                
                <div class="OForm" >              
                    <div class="Row">
                        <div class="Control"><asp:textbox runat="server" ID="txtReturnNotes" TextMode="MultiLine" CssClass="Size Large" Height="75px" />                           
                        </div>
                        <div class="Clear"></div>
                    </div>
                </div>               
                <asp:button text="Save" style="display:none;" id="btnAddNotes" OnClick="btnAddNote_Click" runat="server" />
                <asp:button text="Close" style="display:none;" id="btnCancelNotes" OnClick="btnCancelNote_Click" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <div id="returnTypeDialog">
        <asp:UpdatePanel ID="upnlReturnType" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
            
                <div class="FormError" id="returnTypeDialogErrors" runat="server" visible="false">
					<p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="returnTypeDialogErrorMsg" runat="server" >An error was encountered.</span></p>
				</div>
                
                <div class="OForm" > 
                             
                    <div class="Row">
                        <div class="Label Small">Return #:</div>
                        <div class="LabelValue"><asp:Label runat="server" ID="lblDialogReturnID" CssClass="StaticLabel"/></div>
                        <div class="Clear"></div>
                    </div>

                    <div class="Row">
                        <div class="Label Small">Return Type:<span class="Required">*</span>:</div>
                        <div class="Control">
                            <asp:DropDownList ID="ddlReturnType" runat="server" ></asp:DropDownList>                            
                            <span class="InlineError" id="lblReturnTypeValidate" runat="server" visible="false"></span>
                        </div>
                        <div class="Clear"></div>
                    </div>

                </div>  
                             
                <asp:button text="Save" style="display:none;" id="btnAddReturnType" OnClick="btnAddReturnType_Click" runat="server" />
                <asp:button text="Close" style="display:none;" id="btnCancelReturnType" OnClick="btnCancelReturnType_Click" runat="server" />
                <asp:button text="Load" style="display:none;" id="btnLoadReturnType" OnClick="btnLoadReturnType_Click" runat="server" />

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

  <div >
      <asp:UpdatePanel ID="UpdatePanelNote" runat="server"  UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger controlid="btnAddReturnType" eventname="Click" />
        </Triggers>

        <ContentTemplate>
            <asp:FormView ID="fvReturnHeader" runat="server" DataSourceID="sqlReturn" Width="100%" >
                <ItemTemplate>
                    <table style="width:100%;border:1;" cellpadding="0" cellspacing="0" class="OTable">
                        <tr class="Alt">
                        <td style="width: 12%" class="mt-hd">Return #:</td>
                        <td style="width: 38%" class="mt-itm">
                            <asp:Label ID="lblReturnID" runat="server" Text='<%# Bind("ReturnID") %>' Font-Bold="true" />

                            <asp:LinkButton ID="btnAddAdditionalDevice" runat="server" Visible='<%# DisplayAddAdditionalDeviceButton(DataBinder.Eval(Container.DataItem,"ReturnTypeID","" ), DataBinder.Eval(Container.DataItem,"ReturnID","" )) %>'
                                CommandName="AddAdditionalDevice" CommandArgument='<%= Request.QueryString["ID"] %>' 
                                CssClass="Icon Edit" onclick="btnAddAdditionalDevice_Click" Text = "Add additional device to this Return">
                            </asp:LinkButton>  

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
                    
                            <asp:LinkButton ID="btnEditReturnType" runat="server" Visible='<%# DisplayEditReturnTypeButton(DataBinder.Eval(Container.DataItem,"ReturnTypeID","" )) %>'
                                CommandName="EditReturnType" CommandArgument='<%= Request.QueryString["ID"] %>'
                                CssClass="Icon Edit" onclick="btnEditReturnType_Click" Text = "Edit">
                            </asp:LinkButton>  
                                        
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
                                            <asp:HyperLink ID="HyperLink1" NavigateUrl='<%# Bind("OrderID", "~/CustomerService/ReviewOrder.aspx?ID={0}") %>' 
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
        </ContentTemplate>

      </asp:UpdatePanel>     
  </div>

  
  
  <div  >

    <%--Start TabsContainer Section--%>
    <div id="tabsContainer" >

	    <ul>				
            <li><a href="#Device_tab" id="DeviceTabHeader" runat ="server">Return Devices</a></li>	
            <li><a href="#Note_tab" id="NoteTabHeader" runat ="server">Notes</a></li>				
		    <li><a href="#Upload_tab" id="UploadTabHeader" runat ="server">Additional Attachments</a></li>           
	    </ul>

        <div id="Device_tab">

            <asp:UpdatePanel ID="UpdatePanelDevice" runat="server" UpdateMode="Conditional">
                <ContentTemplate>

                    <asp:GridView CssClass="OTable" ID="gvReturnDetails"
                            AutoGenerateColumns="False" runat="server" DataSourceID="sqlReturnDetails" 
                            onselectedindexchanged="gvReturnDetails_SelectedIndexChanged"
                            DataKeyNames="ReturnDevicesID,SerialNo,ReturnID"
                            AllowSorting="true" AllowPaging="true" PageSize="10" >

                        <Columns>
       
                            <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" 
                                FooterStyle-HorizontalAlign="center"  ItemStyle-HorizontalAlign="center" HeaderText="Actions">
                            <ItemTemplate>
                                    <asp:HyperLink ID="HyperLink2" NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"ReturnDevicesID","../Compose/ReturnDeviceDetails.aspx?ID={0}" ) %>'
                                        runat="server" ToolTip="Click to Edit CS Notes" 
                                        Visible='<%# DisplayLink(DataBinder.Eval(Container.DataItem,"DevicesStatus","" )) %>' Text="EditNotes"></asp:HyperLink>
              
                                    <asp:LinkButton runat="server" ID="linkbutton1" 
                                    CommandName="select" Visible='<%# DisplayLink(DataBinder.Eval(Container.DataItem,"DevicesStatus","" )) %>'
                                    Text="Remove" />
                            </ItemTemplate>
                            </asp:TemplateField>
                              
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="SerialNo"
                                HeaderText="Serial#" ReadOnly="True" ItemStyle-Font-Bold="true" SortExpression="SerialNo">
                            </asp:BoundField>
        
                            <asp:BoundField ItemStyle-CssClass="mt-itm" ItemStyle-Wrap="True" HeaderStyle-CssClass="mt-hd"
                                DataField="Status" HeaderText="Status" ReadOnly="True" SortExpression="Status">
                            </asp:BoundField>
  
                            <asp:BoundField ItemStyle-CssClass="mt-itm" ItemStyle-Wrap="True" HeaderStyle-CssClass="mt-hd"
                                DataField="Notes" HeaderText="CS Notes" ReadOnly="True" SortExpression="Notes">
                            </asp:BoundField>
        
                            <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" 
                                FooterStyle-HorizontalAlign="left"  ItemStyle-HorizontalAlign="left" HeaderText="Receiving Notes & Date">
                                <ItemTemplate>
                                <asp:Label ID="lbl_receiveNotes" runat="server" 
                                    Text='<%#DataBinder.Eval(Container.DataItem,"ReceivedNotes","" )%>' > </asp:Label>
            
                                <br />
                                <asp:Label ID="lbl_receiveDate" runat="server" 
                                    Text=' <%#FuncDateCheck(DataBinder.Eval(Container.DataItem,"ReveivedDate","" ))%>' > </asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
        
                            <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" 
                                FooterStyle-HorizontalAlign="left"  ItemStyle-HorizontalAlign="left" HeaderText="Tech Notes & Date">
                                <ItemTemplate>
                                <asp:Label ID="lbl_techNotes" runat="server" 
                                    Text='<%#DataBinder.Eval(Container.DataItem,"TechNotes","" )%>' > </asp:Label>
            
                                <br />
                                <asp:Label ID="lbl_techReviewDate" runat="server" 
                                    Text=' <%#FuncDateCheck(DataBinder.Eval(Container.DataItem,"TechDate","" ))%>' > </asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
        
                        </Columns>
                        <EmptyDataTemplate>
                            <div class = "NoData">
                                There are no return devices associated with this return.
                            </div>
                        </EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
                        <PagerStyle CssClass="Footer" />

                    </asp:GridView>
                   
                </ContentTemplate>
            </asp:UpdatePanel>
                                                                   
        </div>

	    <div id="Note_tab">

            <asp:UpdatePanel ID="upnlNote" runat="server"  UpdateMode="Conditional">
                <Triggers>
                    <asp:AsyncPostBackTrigger controlid="btnAddNotes" eventname="Click" />
                </Triggers>

                <ContentTemplate>
                    <div class="OToolbar JoinTable" id="CreateNote" runat ="server">
					    <ul>
						    <li>
                                <asp:LinkButton ID="btnNewNote" runat="server"  
                                CommandName="NewNote" CommandArgument='<%= Request.QueryString["ID"] %>' 
                                CssClass="Icon Add" onclick="btnNewNote_Click" >Create Note</asp:LinkButton>
                            </li>
                                
					    </ul>
				    </div>
                    
                    <asp:GridView ID="gvNotes" CssClass="OTable"  runat="server"
                        AutoGenerateColumns="False" DataKeyNames="RMAHeaderID" DataSourceID="sqlNotes"
                        allowpaging="true" AllowSorting="true" pagesize="10">
                        <Columns>
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="CreatedBy"
                                HeaderStyle-Width="100px" HeaderText="Created By" InsertVisible="False" ReadOnly="True" SortExpression="CreatedBy" />
                            <asp:BoundField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" DataField="CreatedDate"
                            HeaderStyle-Width="80"  HeaderText="Created Date" SortExpression="CreatedDate" DataFormatString="{0:d}" />
                            <asp:TemplateField ItemStyle-Width="600px" ItemStyle-CssClass="mt-itm"  ItemStyle-Wrap="true" HeaderStyle-CssClass="mt-hd" HeaderText="Notes">
                            <ItemTemplate>
                                <asp:Label ID="Notes" Text='<%# Bind("Notes") %>' runat="server" />
                            </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="NoData">
                                There are no notes associated with this return.
                            </div>
                        </EmptyDataTemplate>
                        <AlternatingRowStyle CssClass="Alt" />
                        <PagerStyle CssClass="Footer" />
                    </asp:GridView>
                     
                </ContentTemplate>
            </asp:UpdatePanel>   
                    
        </div>

	    <div id="Upload_tab">

    <%--            <asp:UpdatePanel ID="upnlUpload" runat="server" UpdateMode="Conditional">
                <Triggers>
                    <asp:AsyncPostBackTrigger controlid="btnAddUploads" eventname="Click" />
                </Triggers>
                <ContentTemplate> --%>                 
                    <div class="OToolbar JoinTable" id="UploadAttachment" runat ="server">
					    <ul>
						    <li>
                                <%--<asp:LinkButton ID="btnNewUpload" runat="server"  
                                CommandName="NewUpload" CommandArgument='<%= Request.QueryString["ID"] %>' 
                                CssClass="Icon Add" onclick="btnNewUpload_Click" >Upload Attachment</asp:LinkButton>--%>
                        

                                <asp:FileUpload ID="FileUpload1" runat="server" Width="400px" CssClass = "OButtonFormDetail"/>
                                <asp:Button ID="btnUploadAttachment" CssClass="OButtonFormDetail" runat="server" Text="Upload Attachment" 
                                    onclick="btnUploadAttachment_Click" />
                            </li>
                                
					    </ul>
				    </div>
                    
                    <table class="OTable">
                        <tr>
                            <td>
                                <asp:BulletedList ID="BulletedList1" runat="server" />
                            </td>
                        </tr>
                    </table>
                    

    <%--                </ContentTemplate>
            </asp:UpdatePanel>--%>
                                                
        </div>


    </div>
    <%--End TabsContainer Section--%>

    
    <div class="Buttons">
        
        <div class="ButtonHolder">
            <asp:ImageButton ID="ibtnPDF" runat="server" ImageUrl="~/images/DLPDF_RMA.jpg" Height="35"
                onclick="ibtnPDF_click"  ToolTip="Print RMA Letter"  />&nbsp;&nbsp;&nbsp;
                                          
            <asp:Button ID="btnCancel" CssClass="OButton" runat="server" Text="Back to Account Page"
                onclick="btnCancel_Click" />
        </div>
        <div class="Clear"> </div>
    </div>

  </div> 
  
  
    
  

  
  
   <asp:SqlDataSource ID="sqlReturn" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>" 
    SelectCommand="sp_if_RD_GetReturnByReturnNo" SelectCommandType="StoredProcedure">
    <SelectParameters>
      <asp:QueryStringParameter DefaultValue="0" Name="ReturnID" QueryStringField="id" 
        Type="String" />
    </SelectParameters>
  </asp:SqlDataSource>
  <asp:SqlDataSource ID="sqlReturnDetails" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>" 
    SelectCommand="sp_if_RD_GetReturnDetailsByReturnNo" 
    SelectCommandType="StoredProcedure">
    <SelectParameters>
      <asp:QueryStringParameter DefaultValue="0" Name="ReturnID" QueryStringField="id" 
        Type="String" />
    </SelectParameters>
  </asp:SqlDataSource>
  <asp:SqlDataSource ID="sqlNotes" runat="server" ConnectionString="<%$ ConnectionStrings:Instadose.Properties.Settings.InsConnectionString %>"
    SelectCommand="sp_if_RD_GetNotesByReturnNo" SelectCommandType="StoredProcedure">
    <SelectParameters>
      <asp:QueryStringParameter DefaultValue="0" Name="ReturnID" QueryStringField="ID"
        Type="Int32" />
    </SelectParameters>
  </asp:SqlDataSource>
  <script type="text/javascript">
   function ViewRMApdf(returnID) 
    {
       // alert(returnID);
        var screenW = screen.width - 200;
        var screenH = screen.height - 100;
        var winProperty = 'width=' + screenW + ',height=' + screenH+ ',scrollbars=yes,resizable=yes,menubar=no,status=no,location=no';
        window.open('Return_ViewRmaLetter.aspx?id=' +returnID , 'PDFletter', winProperty)
               
    }
   </script>
    
</asp:Content>

