<%@ Page Language="C#" MasterPageFile="~/Masterpages/MirionDSD.master" AutoEventWireup="true" Inherits="TechOps_ReturnInventory" Title="Receipt of Badges" Codebehind="ReturnInventory.aspx.cs" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript">
        $(document).ready(function() {
  
          if($('#ctl00_primaryHolder_txtSerialNos').text() == "") 
          {
              // Disable the continue button
              $('#ctl00_primaryHolder_btnContinue').attr("disabled", "true");
      
               // Disable the Edit Comment button
              $('#ctl00_primaryHolder_btnEdit').attr("disabled", "true");
          }
          else
          {
              // A new item has been scanned.
              var countDevices = countLines('ctl00_primaryHolder_txtSerialNos');
              $('#count').text(countDevices);
          }
          // Set up the serial number validation
          $('#ctl00_primaryHolder_txtSerialNos').keyup(function(event) {
      
              // A new item has been scanned.
              var countDevices = countLines('ctl00_primaryHolder_txtSerialNos');
              $('#count').text(countDevices);
      
              // Enable the continue button if the serials numbers counter is greater than 0.
              if(countDevices <= 0)
              {
                $('#ctl00_primaryHolder_btnContinue').attr("disabled", "true");
                $('#ctl00_primaryHolder_btnEdit').attr("disabled", "true");
              }
              else
              {
                $('#ctl00_primaryHolder_btnContinue').removeAttr('disabled');
                $('#ctl00_primaryHolder_btnEdit').removeAttr('disabled');
              }
      
          }).keypress(function(event) {
              // Get the keycode between FireFox and other browsers.
              var key = (event.keyCode ? event.keyCode : event.which);

              // Allow: backspace, delete, tab, escape, and enter
              if (key == 46 || key == 8 || key == 9 || key == 27 || key == 13 ||
                  // Allow: Ctrl+A
                  (key == 65 && event.ctrlKey === true) ||
                  // Allow: home, end, left, right
                  (key >= 35 && key <= 39)) {
                  // let it happen, don't do anything
                  return;
              }
              else {
                  // Ensure that it is a number and stop the keypress
                  if (event.shiftKey || (key < 48 || key > 57)) {

                      event.preventDefault();
                  }
              }
          });
        });

        /// Count lines of a text box.
        function countLines(id)
        {
          var area = document.getElementById(id)
          if(area.value == "") return 0;
          // trim trailing return char if exists
          var text = area.value.replace(/\s+$/g,"")
          var split = text.split("\n")
          return split.length
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="primaryHolder" Runat="Server">
  
  <asp:Panel ID="plAdd" runat="server">

        <div class="FormError" id="errors" runat="server" visible="false" style="margin:10px" >
	        <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="errorMsg" runat="server" >An error was encountered.</span></p>
        </div>
        <div class="FormMessage" id="messages" runat="server" visible="false" style="margin:10px" > 
	        <p><span class="MessageIcon"></span>
	        <strong>Messages:</strong> <span id="submitMsg" runat ="server" >Ready to search.</span></p>
        </div>             

        <table class="OTable" style="width: auto;">
            <tr>
                <th>
                    Received Badge Serial Numbers:
                </th>
            </tr>
            <tr>
                <td>
                    <div>
                        <asp:TextBox ID="txtSerialNos" TextMode="MultiLine" runat="server" Style="width: 210px;
                            height: 320px; margin: 5px;"></asp:TextBox>
                    </div>
                    <div class="">
                        <div class="Left" style="padding-top: 6px; vertical-align: middle;">
                            <span style="font-weight: bold;">Badges: </span><span id="count">0</span></div>
                        <div class="Right" style="width: auto;">
                            <asp:Button ID="btnContinue" CssClass="OButton" runat="server" Text="Add Badges" onclick="btnContinue_Click" />                           
                            <br /><asp:Button ID="btnEdit" CssClass="OButton" runat="server" Text="Edit Comment" onclick="btnEdit_Click"  />
                        </div>
                        
                        <div class="Clear" />
                        
                    </div>
                </td>
            </tr>
        </table>
  </asp:Panel>

  <asp:Panel ID="plReview" runat="server" Visible="false">
      <div >
          
            <div class="FormError" id="errorReview" runat="server" visible="false">
	            <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorReviewMsg" runat="server" >An error was encountered.</span>
                </p>
            </div>

            <asp:GridView ID="gvReview" runat="server" CssClass="OTable" 
                AutoGenerateColumns="false" DataKeyNames="SerialNo,AccountNo,RequestNo"  OnRowDataBound="gvReview_RowDataBound" >

                <Columns>
                    <asp:BoundField DataField="SerialNo" HeaderText="Serial#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top"/>
                    <asp:BoundField DataField="AccountNo" HeaderText="Acc#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top"/>
                    <asp:BoundField DataField="RequestNo" HeaderText="Req#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top"/>
                    <asp:BoundField DataField="RequestType" HeaderText="Type" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top"/>
                    <asp:BoundField DataField="DepartmentRequest" HeaderText="Dept." ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top"/>
                    <asp:BoundField DataField="DeviveStatus" HeaderText="Status" ItemStyle-Width="120" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top" />
                    <asp:BoundField DataField="Action" HeaderText="Action" ItemStyle-Width="120" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top" />
                    <asp:TemplateField HeaderText="Visual Inspect"  ItemStyle-CssClass="mt-itm centeralign" HeaderStyle-CssClass="mt-hd centeralign" HeaderStyle-Wrap="false"  ItemStyle-VerticalAlign="Top">
                        <ItemTemplate>   
                            <asp:CheckBox ID="ChkBxVisualInspect" Checked="true" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Data Inspect"  ItemStyle-CssClass="mt-itm centeralign" HeaderStyle-CssClass="mt-hd centeralign" HeaderStyle-Wrap="false"  ItemStyle-VerticalAlign="Top">
                        <ItemTemplate>   
                            <asp:CheckBox ID="ChkBxDataInspect" Checked="true" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Approved?"  ItemStyle-CssClass="mt-itm centeralign" HeaderStyle-CssClass="mt-hd centeralign"  ItemStyle-VerticalAlign="Top">
                    <ItemTemplate>   
                        <asp:CheckBox ID="ChkBxApprove" Checked="true" runat="server" />
                    </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Notes" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top">
                    <ItemTemplate >   
                        <asp:TextBox ID="txtNotes" runat="server" Text='Good Condition'  SkinID="none" Width="140" TextMode="MultiLine" Height="65" />  
                    </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Bucket" HeaderText="Bucket" ItemStyle-CssClass="mt-itm CenterAlign mt-itm-bold " HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top"/>
                </Columns>
                <%--<EmptyDataTemplate>
					<div class="NoData">
						No records found!
					</div>
				</EmptyDataTemplate>  --%>
                                                  
                <AlternatingRowStyle CssClass="Alt" />
				<PagerStyle CssClass="Footer" />
            </asp:GridView>

            <table class="OTable" style="width: 500px;">

                <tr>
                    <th  class="mt-hd" >
                        Upload Attachments
                    </th>
                </tr>

                <tr>
                    <td>
                        <div class="OForm">
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
                        </div>
                    </td>
                </tr>                            

            </table> 

            <div class="Left">
                <strong>User Name:</strong> 
                <asp:Label ID="lblusername" runat="server" ></asp:Label>
            </div>
            
            <div class="Right"> 
                  <asp:Button ID="btnFinish" runat="server" CssClass="OButton" 
                      onclick="btnFinish_Click" Text="Acknowledge Receipt of Badges" />
                  <asp:Button ID="btnBack" runat="server" CssClass="OButton" 
                      onclick="btnBack_Click" Text="Back" />
                 
            </div>  
            
            <div class="Clear"></div> 

            <br />
            <br />
            <br />

            <div style="width:500px">
                <table class="OTable" >                                    
                    <tr>
                        <th  class="mt-itm-bold leftalign" >
                            Bucket Color Legend
                        </th>                                                                 
                    </tr>
              
                    <tr>
                        <td class="leftalign" style="color:red">
                            <span id="Bucket1" runat="server" >Bucket 1: Recalls(Recalled-TechOps/DEF)</span>
                        </td>                              
                    </tr>
                    <tr>
                        <td class="leftalign" style="color:orange">
                            <span id="Bucket2" runat="server" >Bucket 2: Selmic or INSTA10 Returns(Returned-Pending Process/WIP)</span>
                        </td>                              
                    </tr>
                    <tr>
                        <td class="leftalign" style="color:green" >
                            <span id="Bucket3" runat="server" >Bucket 3: IMI or Partnertech (s/n < 1000000) Returns(Returned-Defective/DEF)</span>
                        </td>                              
                    </tr>
                    <tr>
                        <td class="leftalign" style="color:navy" >
                            <span id="Bucket4" runat="server" >Bucket 4: INSTA10-B or INSTA PLUS Returns(Returned-TechOps/WIP)</span>
                        </td>                              
                    </tr>
                    <tr>
                        <td class="leftalign" style="color:navy" >
                            <span id="Span1" runat="server" >Bucket 5: Recalls(Recalled Firmware/INS)</span>
                        </td>                              
                    </tr>
                </table> 
            </div>
                    
      </div>
  </asp:Panel>
  
  <asp:Panel ID="plReviewEdit" runat="server" Visible="false">
      <div >          

          <div class="FormError" id="errorReviewEdit" runat="server" visible="false">
	            <p><span class="MessageIcon"></span>
	                <strong>Messages:</strong> <span id="errorReviewEditMsg" runat="server" >An error was encountered.</span>
                </p>
            </div>

            <asp:GridView ID="gvReviewEdit" runat="server" CssClass="OTable" 
                AutoGenerateColumns="false" DataKeyNames="ReturnInventoryID"  >
                <Columns>
                    <%--<asp:BoundField DataField="ReturnInventoryID" HeaderText="ID" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top" HeaderStyle-Width ="60" />--%>
                    <asp:BoundField DataField="SerialNo" HeaderText="Serial#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top" HeaderStyle-Width ="60" />
                    <asp:BoundField DataField="AccountNo" HeaderText="Acc#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top" />
                    <asp:BoundField DataField="RequestNo" HeaderText="Req#" ItemStyle-CssClass="mt-itm rightalign" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top" />
                    <asp:BoundField DataField="RequestType" HeaderText="Type" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top" />
                    <asp:BoundField DataField="Reviewer" HeaderText="Reviewed By" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top" />
                    <asp:BoundField DataField="ReviewDate" HeaderText="Last Reviewed" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top" />
                    <asp:TemplateField HeaderText="Notes" ItemStyle-CssClass="mt-itm" HeaderStyle-CssClass="mt-hd"  ItemStyle-VerticalAlign="Top" >
                        <ItemTemplate >   
                            <asp:TextBox ID="txtNotes" runat="server" Text='<%# Eval("TechNotes") %>'   SkinID="none" Width="300" TextMode="MultiLine" Height="55" />  
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <%--<EmptyDataTemplate>
					<div class="NoData">
						No records found!
					</div>
				</EmptyDataTemplate>  --%>
                                                  
                <AlternatingRowStyle CssClass="Alt" />
				<PagerStyle CssClass="Footer" />
            </asp:GridView>

              
            <div class="Left">
                <strong>User Name:</strong> 
                <asp:Label ID="lblusernameEdit" runat="server" ></asp:Label>
               </div>

            <div class="Right"> 
                  <asp:Button ID="btnFinishEdit" runat="server" CssClass="OButton" 
                      onclick="btnFinishEdit_Click"  Text="Update Badge Comment" />
                  <asp:Button ID="btnBackEdit" runat="server" CssClass="OButton" 
                      onclick="btnBack_Click" Text="Back" />
                 
             </div>

             <div class="Clear"></div>
             
      </div>
  </asp:Panel>

</asp:Content>


