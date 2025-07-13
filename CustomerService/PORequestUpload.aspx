<%@ Page Title="" Language="C#" MasterPageFile="~/masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="Instadose_CustomerService_PORequestUpload" EnableEventValidation="false" Codebehind="PORequestUpload.aspx.cs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style type="text/css">
        .PORequest
        {
            width: 528px;
            height: 331px;
            margin: 70px auto 0 auto;
            padding: 45px 0 0 20px;
            background-image: url('../images/porequest-panel.png');
            background-position: top left;
            background-repeat: no-repeat;
        }
        td.FieldTitle
        {
            width: 130px;
            text-align: right;
            font-size: small;
        }
        td.Field
        {
            width: 320px;
            text-align: left;
            font-size: small;
        }
        .tb
        {
            border: 1px solid #6297BC;
            width: 230px;
        }
        .tb1
        {
            border: 1px solid #6297BC;
            width: 170px;
        }
        .tb2
        {
            border: 1px solid #6297BC;
            width: 100px;
            text-align: right;
        }
        .tb3
        {
            border: 1px solid #6297BC;
            width: 50px;
        }
        td.TotalFieldTitle
        {
            width: 225px;
            text-align: right;
            font-size: small;
        }
        td.TotalField
        {
            width: 225px;
            text-align: left;
            font-size: small;
        }
        .Totaltb2
        {
            border: 1px solid #6297BC;
            width: 145px;
            text-align: right;
        }
                
        
        
        #shareit-box
        {
            position:absolute;
            display:none;
        }
    
        #shareit-header
        {
            width:138px;
        }
        
        #shareit-body
        {
            width:138px; height:100px;
            background:url(images/shareit.png);
        }

        #shareit-blank
        {
            height:20px;
        }

        #shareit-url
        {
            height:50px;
            text-align:center;
        }

        #shareit-url input.field
        {
            width:100px; height:26px;
            background: transparent url(images/field.gif) no-repeat;
            border:none; outline:none;
            padding:7px 5px 0 5px;
            margin:3px auto;font-size:11px;
        }

        #shareit-icon
        {
            height:20px;
        }

        #shareit-icon ul
        {
            list-style:none;
            width:130px;
            margin:0; padding:0 0 0 8px;
        }

        #shareit-icon ul  li
        {
            float:left;
            padding:0 2px;
        }

        #shareit-icon ul  li img
        {
            border:none;
        }			
        
        </style>
    <script type="text/javascript">

        $(document).ready(function () {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(JQueryControlsLoad);
            JQueryControlsLoad();
        });

        function JQueryControlsLoad() {



          function openDialog(id) {
              $('.ui-overlay').fadeIn();
              $('#' + id).dialog("open");
          }

          function closeDialog(id) {
              $('#' + id).dialog("close");
          }	

          $('#POActions').tooltip({
              delay: 0,
              showURL: false,
              bodyHandler: function () {
                  return $("<img/>").attr("src", this.src);
              }
          });
          
          $(document).ready(function(){
            $.pop();
          });
    
        function ViewRMApdf(returnID) {
            var screenW = screen.width - 200;
            var screenH = screen.height - 100;
            var winProperty = 'width=' + screenW + ',height=' + screenH + ',scrollbars=yes,resizable=yes,menubar=no,status=no,location=no';
            window.open('Return_ViewRmaLetter.aspx?id=' + returnID, 'PDFletter', winProperty)

        }

        function EmailOrderAck(AccountID) {
            var screenW = 500;
            var screenH = 350;
            var winProperty = 'width=' + screenW + ',height=' + screenH + ',linkbar=no,toolbar=no,scrollbars=no,resizable=yes,menubar=no,status=no,location=no';
            window.open('../EmailOrderAck.aspx?id=' + AccountID, 'OrderAck', winProperty)

        }

        function ViewDetail(ID, PageName, sWidth, sHeight, Title) {
            var screenW = sWidth;
            var screenH = sHeight;
            var winProperty = 'width=' + screenW + ',height=' + screenH + ',left=0,top=0,linkbar=no,toolbar=no,scrollbars=yes,resizable=yes,menubar=no,status=no,location=no';
            newwindow = window.open(PageName + '?id=' + ID, Title, winProperty);
            if (window.focus) { newwindow.focus() }
            return false;
        }

        function ViewUserDetail(accountID, userID, PageName, sWidth, sHeight) {
            var screenW = sWidth;
            var screenH = sHeight;
            var winProperty = 'width=' + screenW + ',height=' + screenH + ',left=0,top=0,linkbar=no,toolbar=no,scrollbars=yes,resizable=yes,menubar=no,status=no,location=no';
            if (userID == "") {
                newwindow = window.open(PageName + '?accountID=' + accountID, 'UserDetail', winProperty);
            }
            else {
                newwindow = window.open(PageName + '?userID=' + userID + '&accountID=' + accountID, 'UserDetail', winProperty);
            }

            if (window.focus) { newwindow.focus() }
            return false;
        }



    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" runat="Server">
    
    <asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server">
    </asp:ScriptManager>
   
    <asp:UpdatePanel ID="POUploadContainer" Visible="true" runat="server">
        
      <ContentTemplate>
     
      </ContentTemplate>
     </asp:UpdatePanel>
   
          <div class="OForm">
            <table class="OTable" style="width:500px;">
                <tr class="Header">
                    <td>Upload PO File(s)</td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="lblOlUploadResult" runat="server"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>
                            <asp:Label ID="Label1" Visible="true" runat="server"
                                        Text="Order #:">
                            </asp:Label>
                            <asp:Label ID="lblOLUploadOrderID" Visible="true" runat="server">
                            </asp:Label>
                    </td>
                </tr>
                <tr>
                    <td></td>
                </tr>
                <tr> 
                    <td>
                        <asp:Label ID="lblAttach1" runat="server" Font-Bold="true" 
                            Text="Attachment 1" />
                        <asp:FileUpload ID="attachmentUpload1" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="lblAttach2" runat="server" Font-Bold="true" 
                            Text="Attachment 2" />
                        <asp:FileUpload ID="attachmentUpload2" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td class="Clear" />
                </tr>
                <tr>
                    <td>
                        <asp:Button ID="btnPOUpload" runat="server" Text="Upload" 
                            OnClick="btnPOUpload_Click" CssClass="OButton"/>
               
                        <asp:Button ID="btnClose" runat="server" Text="Close" CssClass="OButton" 
                        OnClick="btnClose_Click" />
                    </td>
                </tr>
                <tr>
                    <td class="Clear" />
                </tr>
            </table>
         </div>
        
        <div style="width: 500px"> 
        <table>
            <tr>
                <%--
                            <asp:HyperLink ID="HyperLink2" 
                                NavigateUrl='<%# FuncGenerateHTTPLink(DataBinder.Eval(Container.DataItem,"DocumentID","" ))%>'                           
                                runat="server" ToolTip="Click to View Acknowledgement" 
                                Text="View"></asp:HyperLink>
                                --%>
                <td>
                 <asp:GridView ID="gvPORequest" runat="server" AutoGenerateColumns="False" 
                    CssClass="OTable" 
                    GridLines="None" DataKeyNames="DocumentID" 
                        EmptyDataText="No PO Request for this Order#. ">
                  <RowStyle CssClass="mt-hd" />
                  <Columns>
                      <asp:TemplateField ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd" 
                        FooterStyle-HorizontalAlign="center"  ItemStyle-HorizontalAlign="center" 
                            HeaderText="" ControlStyle-Width="50">
                        <ItemTemplate>
                        
                            <asp:LinkButton ID="lnkBtnView" ToolTip="Click to View PO Request"
                                  OnClick="lnkBtnView_Click" runat="server" CommandArgument='<%# Eval("DocumentID") %>'>View</asp:LinkButton>

                        </ItemTemplate>
                      </asp:TemplateField>
                      
                      <asp:BoundField DataField="AccountID" HeaderText="Account#" 
                          SortExpression="AccountID" ItemStyle-Width="50" ItemStyle-HorizontalAlign="Right"  />
                      <asp:BoundField DataField="Filename" HeaderText="File Name" 
                          SortExpression="POFilename"  ItemStyle-Width="250" />
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
                </td>
            </tr>
        </table>      
        </div>


   <%------------------------------- START MAIN PAGE CONTENT OVER HERE ------------------------------------------------------%>
  
  <asp:Panel runat="server" ID="pnlMain" Visible="true">
    
        <div class="FormError" id="lblError" runat="server" visible="false">
            <p>
                <span class="MessageIcon"></span><strong>Messages:</strong> <span id="lblRenewalRowMessage"
                    runat="server">An error was encountered.</span>
            </p>
                <asp:BulletedList ID="blstErrors" runat="server" BulletStyle="Disc">
                </asp:BulletedList>
        </div>       
        
    </asp:Panel>

  
</asp:Content>
